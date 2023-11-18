#![allow(dead_code)]
#![allow(unused_variables)]

mod types;

use crate::{fact::Fact, world::types::translate_types};
use once_cell::sync::OnceCell;
use spingus::{
    domain::{
        action::Actions,
        parameter::{Parameter, Parameters},
        predicate::Predicates,
    },
    problem::object::{Object, Objects},
};
use std::collections::HashMap;

use self::types::Types;

pub struct World {
    pub domain_name: String,
    pub types: Types,
    /// Maps predicate name to its index
    predicates: HashMap<String, u16>,
    /// Maps action name to its index
    actions: HashMap<String, u16>,
    /// Maps meta action name to its index
    meta_actions: HashMap<String, u16>,
    /// Maps object name to its index
    objects: HashMap<String, u16>,
    /// Maps object index to its type index
    object_types: HashMap<u16, usize>,
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
        let (objects, object_types) = extract_objects(&types, &problem.objects, &domain.constants);
        println!("object_count={}", objects.len());
        let init = problem
            .inits
            .iter()
            .map(|i| {
                Fact::new(
                    predicates[&i.name],
                    i.parameters.iter().map(|p| objects[p]).collect(),
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
            object_types,
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

    pub fn get_object_count(&self) -> u16 {
        self.objects.len() as u16
    }

    pub fn get_object_index(&self, name: &str) -> u16 {
        self.objects[name]
    }

    pub fn get_object_indexes(&self, indexes: &Vec<String>) -> Vec<u16> {
        indexes.iter().map(|i| self.get_object_index(i)).collect()
    }

    pub fn get_object_name(&self, index: u16) -> &String {
        &self.objects.iter().find(|(_, i)| **i == index).unwrap().0
    }

    pub fn get_object_names(&self, indexes: &Vec<u16>) -> Vec<&String> {
        indexes.iter().map(|i| self.get_object_name(*i)).collect()
    }

    pub fn get_object_names_cloned(&self, indexes: &Vec<u16>) -> Vec<String> {
        indexes
            .iter()
            .map(|i| self.get_object_name(*i).to_owned())
            .collect()
    }

    pub fn get_object_type(&self, object: u16) -> usize {
        self.object_types[&object]
    }

    pub fn iterate_objects<'a>(&'a self) -> impl Iterator<Item = (u16, usize)> + 'a {
        (1..self.get_object_count() + 1)
            .into_iter()
            .map(|i| (i, self.get_object_type(i)))
    }

    pub fn iterate_objects_named<'a>(&'a self) -> impl Iterator<Item = (&String, &String)> + 'a {
        self.objects.iter().map(|(name, index)| {
            let object_type = self.get_object_type(*index);
            let type_name = self.types.name(object_type);
            (name, type_name)
        })
    }

    pub fn get_objects_with_type(&self, type_id: usize) -> Vec<u16> {
        World::global()
            .iterate_objects()
            .filter_map(|(object_id, t)| match self.types.is_of_type(t, type_id) {
                true => Some(object_id),
                false => None,
            })
            .collect()
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

fn extract_objects(
    types: &Types,
    objects: &Objects,
    constants: &Option<Parameters>,
) -> (HashMap<String, u16>, HashMap<u16, usize>) {
    let mut objects = objects.clone();
    match constants {
        Some(parameters) => objects.append(
            &mut parameters
                .iter()
                .map(|p| match p {
                    Parameter::Untyped { name } => Object {
                        name: name.to_string(),
                        type_name: None,
                    },
                    Parameter::Typed { name, type_name } => Object {
                        name: name.to_string(),
                        type_name: Some(type_name.to_string()),
                    },
                    _ => todo!(),
                })
                .collect(),
        ),
        None => {}
    };
    let temp: Vec<((String, u16), (u16, usize))> = objects
        .iter()
        .enumerate()
        .map(|(i, o)| {
            let object_name = o.name.to_owned();
            let object_index = i as u16 + 1;
            let object_type = match &o.type_name {
                Some(t) => t,
                None => "object",
            };
            let type_index = types.index(object_type);
            ((object_name, object_index), (object_index, type_index))
        })
        .collect();
    temp.into_iter().unzip()
}
