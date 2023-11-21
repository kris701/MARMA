#![allow(dead_code)]
#![allow(unused_variables)]

pub mod action;
mod objects;
pub mod parameter;
mod predicates;
mod types;

use crate::{
    fact::Fact,
    world::{
        action::translate_actions, objects::translate_objects, predicates::translate_predicates,
        types::translate_types,
    },
};
use once_cell::sync::OnceCell;
use std::collections::HashSet;

use self::{action::Action, objects::Objects, predicates::Predicates, types::Types};

pub struct World {
    pub domain_name: String,
    pub types: Types,
    pub objects: Objects,
    pub predicates: Predicates,
    pub actions: Vec<Action>,
    pub meta_actions: Vec<Action>,
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
        let actions: Vec<Action> = translate_actions(&types, &predicates, domain.actions);
        println!("action_count={}", actions.len());
        let meta_actions: Vec<Action> = translate_actions(&types, &predicates, meta_domain.actions);
        println!("meta_action_count={}", meta_actions.len());
        let objects = translate_objects(&types, domain.constants, problem.objects);
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
            meta_actions,
            objects,
            init,
            static_facts,
        }
    }

    pub fn is_meta_action(&self, name: &str) -> bool {
        if self.actions.iter().any(|a| a.name == name) {
            return false;
        }

        if self.meta_actions.iter().any(|a| a.name == name) {
            return true;
        }
        panic!("Undeclared action: {}", name);
    }

    pub fn action_index(&self, name: &str) -> usize {
        self.actions.iter().position(|a| a.name == name).unwrap()
    }

    pub fn meta_index(&self, name: &str) -> usize {
        self.meta_actions
            .iter()
            .position(|a| a.name == name)
            .unwrap()
    }

    pub fn get_action(&self, name: &str) -> &Action {
        match self.is_meta_action(name) {
            true => &self.meta_actions[self.meta_index(name) as usize],
            false => &self.actions[self.action_index(name) as usize],
        }
    }
}
