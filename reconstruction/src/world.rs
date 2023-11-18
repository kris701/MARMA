#![allow(dead_code)]
#![allow(unused_variables)]

mod objects;
mod types;

use crate::{
    fact::Fact,
    world::{objects::translate_objects, types::translate_types},
};
use once_cell::sync::OnceCell;
use spingus::domain::{action::Actions, predicate::Predicates};
use std::collections::HashMap;

use self::{objects::Objects, types::Types};

pub struct World {
    pub domain_name: String,
    pub types: Types,
    pub objects: Objects,
    predicates: HashMap<String, u16>,
    /// Maps action name to its index
    actions: HashMap<String, u16>,
    /// Maps meta action name to its index
    meta_actions: HashMap<String, u16>,
    /// Initial facts
    init: Vec<Fact>,
}

pub static WORLD: OnceCell<World> = OnceCell::new();

impl World {
    pub fn global() -> &'static World {
        WORLD.get().expect("world is not initialized")
    }

    pub fn generate(
        domain: &spingus::domain::Domain,
        meta_domain: &spingus::domain::Domain,
        problem: &spingus::problem::Problem,
    ) -> World {
        let domain_name = domain.name.to_owned();
        let types = translate_types(domain.types.to_owned());
        let predicates = extract_predicates(&domain.predicates);
        println!("predicate_count={}", predicates.len());
        let actions = extract_actions(&domain.actions);
        println!("action_count={}", actions.len());
        let meta_actions = extract_meta_actions(&actions, &meta_domain.actions);
        println!("meta_action_count={}", meta_actions.len());
        let objects = translate_objects(
            &types,
            domain.constants.to_owned(),
            problem.objects.to_owned(),
        );
        let init = problem
            .inits
            .iter()
            .map(|i| {
                Fact::new(
                    predicates[&i.name],
                    i.parameters.iter().map(|p| objects.index(p)).collect(),
                )
            })
            .collect();
        Self {
            domain_name,
            types,
            predicates,
            actions,
            meta_actions,
            objects,
            init,
        }
    }

    pub fn domain_name(&self) -> &str {
        &self.domain_name
    }

    pub fn get_action_index(&self, name: &str) -> u16 {
        self.actions[name]
    }

    pub fn get_action_name(&self, index: u16) -> &String {
        &self.actions.iter().find(|(_, i)| **i == index).unwrap().0
    }

    pub fn is_meta_action(&self, name: &str) -> bool {
        self.meta_actions.contains_key(name) && !self.actions.contains_key(name)
    }

    pub fn get_meta_index(&self, name: &str) -> u16 {
        self.meta_actions[name]
    }

    pub fn get_meta_name(&self, index: u16) -> &String {
        &self
            .meta_actions
            .iter()
            .find(|(_, i)| **i == index)
            .unwrap()
            .0
    }

    pub fn get_predicate_index(&self, name: &str) -> u16 {
        self.predicates[name]
    }

    pub fn get_predicate_name(&self, index: u16) -> &String {
        &self
            .predicates
            .iter()
            .find(|(_, i)| **i == index)
            .unwrap()
            .0
    }

    pub fn init(&self) -> &Vec<Fact> {
        &self.init
    }
}

fn extract_predicates(predicates: &Predicates) -> HashMap<String, u16> {
    predicates
        .iter()
        .enumerate()
        .map(|(i, p)| (p.name.to_owned(), i as u16 + 1))
        .collect()
}

fn extract_actions(actions: &Actions) -> HashMap<String, u16> {
    actions
        .iter()
        .enumerate()
        .map(|(i, a)| (a.name.to_owned(), i as u16))
        .collect()
}

fn extract_meta_actions(
    actions: &HashMap<String, u16>,
    meta_actions: &Actions,
) -> HashMap<String, u16> {
    let mut index_map: HashMap<String, u16> = HashMap::new();
    for (i, a) in meta_actions.iter().enumerate() {
        if !actions.contains_key(&a.name) {
            index_map.insert(a.name.to_owned(), i as u16);
        }
    }
    index_map
}
