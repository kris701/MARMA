#![allow(dead_code)]
#![allow(unused_variables)]

use once_cell::sync::OnceCell;
use spingus::{
    domain::{action::Actions, predicate::Predicates, types::Types},
    problem::object::Objects,
};
use std::collections::HashMap;

pub struct World {
    /// Name of original domain
    domain_name: String,
    /// Maps type name to its index
    types: HashMap<String, u32>,
    /// The parent of each index
    type_children: HashMap<u32, Vec<u32>>,
    /// Maps predicate name to its index
    predicates: HashMap<String, u32>,
    /// Maps action name to its index
    actions: HashMap<String, u32>,
    /// Maps meta action name to its index
    meta_actions: HashMap<String, u32>,
    /// Maps object name to its index
    objects: HashMap<String, u32>,
    /// Maps object index to its type index
    object_types: HashMap<u32, u32>,
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
        let (objects, object_types) = extract_objects(&types, &problem.objects);
        Self {
            domain_name,
            types,
            type_children,
            predicates,
            actions,
            meta_actions,
            objects,
            object_types,
        }
    }

    pub fn domain_name(&self) -> &str {
        &self.domain_name
    }

    pub fn get_default_type_index(&self) -> u32 {
        0
    }

    pub fn get_type_index(&self, name: &str) -> u32 {
        assert!(
            self.types.contains_key(name),
            "Found undeclared type: {}. Actual types: {:?}",
            name,
            self.types
        );
        self.types[name]
    }

    pub fn get_type_name(&self, index: u32) -> &String {
        &self.types.iter().find(|(_, i)| **i == index).unwrap().0
    }

    pub fn get_type_children(&self, index: u32) -> Option<&Vec<u32>> {
        self.type_children.get(&index)
    }

    pub fn is_of_type(&self, type_index: u32, wished_index: u32) -> bool {
        if type_index == wished_index {
            return true;
        }
        let children = self.get_type_children(wished_index);
        match children {
            Some(children) => children.iter().any(|i| self.is_of_type(*i, wished_index)),
            None => false,
        }
    }

    pub fn get_action_index(&self, name: &str) -> u32 {
        assert!(
            self.actions.contains_key(name),
            "Found undeclared action: {}",
            name
        );
        self.actions[name]
    }

    pub fn get_action_name(&self, index: u32) -> &String {
        &self.actions.iter().find(|(_, i)| **i == index).unwrap().0
    }

    pub fn is_meta_action(&self, name: &str) -> bool {
        self.meta_actions.contains_key(name) && !self.actions.contains_key(name)
    }

    pub fn get_meta_index(&self, name: &str) -> u32 {
        assert!(
            self.meta_actions.contains_key(name),
            "Found undeclared action: {}",
            name
        );
        self.meta_actions[name]
    }

    pub fn get_meta_name(&self, index: u32) -> &String {
        &self
            .meta_actions
            .iter()
            .find(|(_, i)| **i == index)
            .unwrap()
            .0
    }

    pub fn get_predicate_index(&self, name: &str) -> u32 {
        assert!(
            self.predicates.contains_key(name),
            "Found undeclared predicate: {}",
            name
        );
        self.predicates[name]
    }

    pub fn get_predicate_name(&self, index: u32) -> &String {
        &self
            .predicates
            .iter()
            .find(|(_, i)| **i == index)
            .unwrap()
            .0
    }

    pub fn get_object_count(&self) -> u32 {
        self.objects.len() as u32
    }

    pub fn get_object_index(&self, name: &str) -> u32 {
        assert!(
            self.objects.contains_key(name),
            "Found undeclared object: {}",
            name
        );
        self.objects[name]
    }

    pub fn get_object_indexes(&self, indexes: &Vec<String>) -> Vec<u32> {
        indexes.iter().map(|i| self.get_object_index(i)).collect()
    }

    pub fn get_object_name(&self, index: u32) -> &String {
        &self.objects.iter().find(|(_, i)| **i == index).unwrap().0
    }

    pub fn get_object_names(&self, indexes: &Vec<u32>) -> Vec<&String> {
        indexes.iter().map(|i| self.get_object_name(*i)).collect()
    }

    pub fn get_object_names_cloned(&self, indexes: &Vec<u32>) -> Vec<String> {
        indexes
            .iter()
            .map(|i| self.get_object_name(*i).to_owned())
            .collect()
    }

    pub fn get_object_type(&self, object: u32) -> u32 {
        self.object_types[&object]
    }

    pub fn iterate_objects<'a>(&'a self) -> impl Iterator<Item = (u32, u32)> + 'a {
        (0..self.get_object_count())
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

    pub fn get_objects_with_type(&self, type_id: u32) -> Vec<u32> {
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
}

fn extract_types(types: &Option<Types>) -> (HashMap<String, u32>, HashMap<u32, Vec<u32>>) {
    let mut index_map: HashMap<String, u32> = HashMap::new();
    let mut type_children: HashMap<u32, Vec<u32>> = HashMap::new();
    index_map.insert("object".to_string(), 0);

    if let Some(types) = types {
        for t in types.iter() {
            if !index_map.contains_key(&t.name) {
                index_map.insert(t.name.to_owned(), index_map.len() as u32);
            }
            let type_index = index_map.get(&t.name).unwrap().to_owned();
            for t in t.sub_types.iter() {
                if !index_map.contains_key(t) {
                    index_map.insert(t.to_owned(), index_map.len() as u32);
                }
                let child_index = index_map.get(t).unwrap();
                let children_entry = &mut type_children.entry(type_index).or_default();
                children_entry.push(*child_index);
            }
        }
    }

    (index_map, type_children)
}

fn extract_predicates(predicates: &Predicates) -> HashMap<String, u32> {
    predicates
        .iter()
        .enumerate()
        .map(|(i, p)| (p.name.to_owned(), i as u32))
        .collect()
}

fn extract_actions(actions: &Actions) -> HashMap<String, u32> {
    actions
        .iter()
        .enumerate()
        .map(|(i, a)| (a.name.to_owned(), i as u32))
        .collect()
}

fn extract_meta_actions(
    actions: &HashMap<String, u32>,
    meta_actions: &Actions,
) -> HashMap<String, u32> {
    let mut index_map: HashMap<String, u32> = HashMap::new();
    for (i, a) in meta_actions.iter().enumerate() {
        if !actions.contains_key(&a.name) {
            index_map.insert(a.name.to_owned(), i as u32);
        }
    }
    index_map
}

fn extract_objects(
    type_map: &HashMap<String, u32>,
    objects: &Objects,
) -> (HashMap<String, u32>, HashMap<u32, u32>) {
    let temp: Vec<((String, u32), (u32, u32))> = objects
        .iter()
        .enumerate()
        .map(|(i, o)| {
            let object_name = o.name.to_owned();
            let object_index = i as u32;
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
