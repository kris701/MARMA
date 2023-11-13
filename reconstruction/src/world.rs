#![allow(dead_code)]
#![allow(unused_variables)]

use crate::fact::Fact;
use once_cell::sync::OnceCell;
use spingus::{
    domain::{
        action::Actions,
        parameter::{Parameter, Parameters},
        predicate::Predicates,
        types::Types,
    },
    problem::object::{Object, Objects},
};
use std::collections::HashMap;

pub struct World {
    /// Name of original domain
    domain_name: String,
    /// Maps type name to its index
    types: HashMap<String, u16>,
    /// The parent of each index
    type_children: HashMap<u16, Vec<u16>>,
    /// Maps predicate name to its index
    predicates: HashMap<String, u16>,
    /// Maps action name to its index
    actions: HashMap<String, u16>,
    /// Maps meta action name to its index
    meta_actions: HashMap<String, u16>,
    /// Maps object name to its index
    objects: HashMap<String, u16>,
    /// Maps object index to its type index
    object_types: HashMap<u16, u16>,
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
        let (types, type_children) = extract_types(&domain.types);
        let predicates = extract_predicates(&domain.predicates);
        let actions = extract_actions(&domain.actions);
        let meta_actions = extract_meta_actions(&actions, &meta_domain.actions);
        let (objects, object_types) = extract_objects(&types, &problem.objects, &domain.constants);
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
            type_children,
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

    pub fn get_default_type_index(&self) -> u16 {
        0
    }

    pub fn get_type_index(&self, name: &str) -> u16 {
        assert!(
            self.types.contains_key(name),
            "Found undeclared type: {}. Actual types: {:?}",
            name,
            self.types
        );
        self.types[name]
    }

    pub fn get_type_name(&self, index: u16) -> &String {
        &self.types.iter().find(|(_, i)| **i == index).unwrap().0
    }

    pub fn get_type_children(&self, index: u16) -> Option<&Vec<u16>> {
        self.type_children.get(&index)
    }

    pub fn is_of_type(&self, type_index: u16, wished_index: u16) -> bool {
        if type_index == wished_index {
            return true;
        }
        let children = self.get_type_children(wished_index);
        match children {
            Some(children) => children.iter().any(|i| self.is_of_type(*i, wished_index)),
            None => false,
        }
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

    pub fn get_object_type(&self, object: u16) -> u16 {
        self.object_types[&object]
    }

    pub fn iterate_objects<'a>(&'a self) -> impl Iterator<Item = (u16, u16)> + 'a {
        (1..self.get_object_count() + 1)
            .into_iter()
            .map(|i| (i, self.get_object_type(i)))
    }

    pub fn iterate_objects_named<'a>(&'a self) -> impl Iterator<Item = (&String, &String)> + 'a {
        self.objects.iter().map(|(name, index)| {
            let object_type = self.get_object_type(*index);
            let type_name = self.get_type_name(object_type);
            (name, type_name)
        })
    }

    pub fn get_objects_with_type(&self, type_id: u16) -> Vec<u16> {
        World::global()
            .iterate_objects()
            .filter_map(
                |(object_id, t)| match World::global().is_of_type(t, type_id) {
                    true => Some(object_id),
                    false => None,
                },
            )
            .collect()
    }

    pub fn init(&self) -> &Vec<Fact> {
        &self.init
    }
}

fn extract_types(types: &Option<Types>) -> (HashMap<String, u16>, HashMap<u16, Vec<u16>>) {
    let mut index_map: HashMap<String, u16> = HashMap::new();
    let mut type_children: HashMap<u16, Vec<u16>> = HashMap::new();
    index_map.insert("object".to_string(), 0);

    if let Some(types) = types {
        for t in types.iter() {
            if !index_map.contains_key(&t.name) {
                index_map.insert(t.name.to_owned(), index_map.len() as u16);
            }
            let type_index = index_map.get(&t.name).unwrap().to_owned();
            for t in t.sub_types.iter() {
                if !index_map.contains_key(t) {
                    index_map.insert(t.to_owned(), index_map.len() as u16);
                }
                let child_index = index_map.get(t).unwrap();
                let children_entry = &mut type_children.entry(type_index).or_default();
                children_entry.push(*child_index);
            }
        }
    }

    (index_map, type_children)
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
    type_map: &HashMap<String, u16>,
    objects: &Objects,
    constants: &Option<Parameters>,
) -> (HashMap<String, u16>, HashMap<u16, u16>) {
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
    let temp: Vec<((String, u16), (u16, u16))> = objects
        .iter()
        .enumerate()
        .map(|(i, o)| {
            let object_name = o.name.to_owned();
            let object_index = i as u16 + 1;
            let object_type = match &o.type_name {
                Some(t) => t,
                None => "object",
            };
            let type_index = type_map[object_type];
            ((object_name, object_index), (object_index, type_index))
        })
        .collect();
    temp.into_iter().unzip()
}
