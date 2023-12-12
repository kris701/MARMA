pub mod action;
pub mod actions;
pub mod atom;
pub mod domain_writing;
mod objects;
pub mod parameter;
mod predicates;
pub mod problem_writing;
mod types;

use crate::{
    fact::Fact,
    tools::{status_print, Status},
    world::{objects::translate_objects, predicates::translate_predicates, types::translate_types},
};
use once_cell::sync::OnceCell;
use spingus::{domain::parse_domain, problem::parse_problem};
use std::{collections::HashSet, fs, path::PathBuf};

use self::{
    actions::{translate_actions, Actions},
    objects::Objects,
    predicates::Predicates,
    types::Types,
};

pub struct World {
    pub domain_name: String,
    pub types: Types,
    pub objects: Objects,
    pub predicates: Predicates,
    pub actions: Actions,
    pub init: Vec<Fact>,
    pub static_facts: HashSet<Fact>,
}

pub static WORLD: OnceCell<World> = OnceCell::new();

impl World {
    pub fn global() -> &'static World {
        WORLD.get().expect("world is not initialized")
    }

    pub fn generate(
        domain: spingus::domain::Domain,
        meta_domain: spingus::domain::Domain,
        problem: spingus::problem::Problem,
    ) -> World {
        let domain_name = domain.name;
        let types = translate_types(domain.types);
        let predicates = translate_predicates(&types, &domain.actions, domain.predicates);
        let objects = translate_objects(&types, domain.constants, problem.objects);
        let actions = translate_actions(
            &types,
            &predicates,
            &objects,
            domain.actions,
            meta_domain.actions,
        );
        let init: Vec<Fact> = problem
            .inits
            .iter()
            .map(|i| {
                Fact::new(
                    predicates.index(&i.name),
                    i.parameters.iter().map(|p| objects.index(p)).collect(),
                )
            })
            .collect();
        let static_facts = init
            .iter()
            .filter(|f| predicates.is_static(f.predicate() as usize))
            .cloned()
            .collect();
        Self {
            domain_name,
            types,
            predicates,
            actions,
            objects,
            init,
            static_facts,
        }
    }
}

pub fn init_world(domain: &PathBuf, meta_domain: &PathBuf, problem: &PathBuf) {
    status_print(Status::Init, "Reading meta domain");
    let meta_domain = fs::read_to_string(meta_domain).unwrap();
    status_print(Status::Init, "Reading domain");
    let domain = fs::read_to_string(domain).unwrap();
    status_print(Status::Init, "Reading problem");
    let problem = fs::read_to_string(problem).unwrap();
    status_print(Status::Init, "Parsing meta domain");
    let meta_domain = parse_domain(&meta_domain).unwrap();
    status_print(Status::Init, "Parsing domain");
    let domain = parse_domain(&domain).unwrap();
    status_print(Status::Init, "Parsing problem");
    let problem = parse_problem(&problem).unwrap();
    status_print(Status::Init, "Generating world");
    let _ = WORLD.set(World::generate(domain, meta_domain, problem));
}
