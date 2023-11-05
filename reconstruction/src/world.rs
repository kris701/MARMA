use once_cell::sync::OnceCell;
use spingus::{
    domain::{action::Actions, predicate::Predicates, types::Types},
    problem::object::Objects,
};
use std::collections::HashMap;

pub struct World {
    #[allow(dead_code)]
    types: HashMap<String, usize>,
    #[allow(dead_code)]
    predicates: HashMap<String, usize>,
    #[allow(dead_code)]
    actions: HashMap<String, usize>,
    #[allow(dead_code)]
    meta_actions: HashMap<String, usize>,
    #[allow(dead_code)]
    objects: HashMap<String, usize>,
}

pub static WORLD: OnceCell<World> = OnceCell::new();

impl World {
    #[allow(dead_code)]
    pub fn global() -> &'static World {
        WORLD.get().expect("world is not initialized")
    }

    pub fn generate(
        domain: &spingus::domain::Domain,
        meta_domain: &spingus::domain::Domain,
        problem: &spingus::problem::Problem,
    ) -> World {
        let types = extract_tyes(&domain.types);
        let predicates = extract_predicates(&domain.predicates);
        let actions = extract_actions(&domain.actions);
        let meta_actions = extract_meta_actions(&actions, &meta_domain.actions);
        let objects = extract_objects(&problem.objects);
        Self {
            types,
            predicates,
            actions,
            meta_actions,
            objects,
        }
    }

    #[allow(dead_code)]
    pub fn get_type_index(&self, name: &str) -> usize {
        assert!(
            self.types.contains_key(name),
            "Found undeclared type: {}. Actual types: {:?}",
            name,
            self.types
        );
        self.types[name]
    }

    #[allow(dead_code)]
    pub fn get_type_name(&self, index: usize) -> &String {
        &self.types.iter().find(|(_, i)| **i == index).unwrap().0
    }

    #[allow(dead_code)]
    pub fn get_action_index(&self, name: &str) -> usize {
        assert!(
            self.actions.contains_key(name),
            "Found undeclared action: {}",
            name
        );
        self.actions[name]
    }

    #[allow(dead_code)]
    pub fn get_action_name(&self, index: usize) -> &String {
        &self.actions.iter().find(|(_, i)| **i == index).unwrap().0
    }

    #[allow(dead_code)]
    pub fn is_meta_action(&self, name: &str) -> bool {
        self.meta_actions.contains_key(name) && !self.actions.contains_key(name)
    }

    #[allow(dead_code)]
    pub fn get_meta_index(&self, name: &str) -> usize {
        assert!(
            self.meta_actions.contains_key(name),
            "Found undeclared action: {}",
            name
        );
        self.meta_actions[name]
    }

    #[allow(dead_code)]
    pub fn get_meta_name(&self, index: usize) -> &String {
        &self
            .meta_actions
            .iter()
            .find(|(_, i)| **i == index)
            .unwrap()
            .0
    }

    #[allow(dead_code)]
    pub fn get_predicate_index(&self, name: &str) -> usize {
        assert!(
            self.predicates.contains_key(name),
            "Found undeclared predicate: {}",
            name
        );
        self.predicates[name]
    }

    #[allow(dead_code)]
    pub fn get_predicate_name(&self, index: usize) -> &String {
        &self
            .predicates
            .iter()
            .find(|(_, i)| **i == index)
            .unwrap()
            .0
    }
}

fn extract_tyes(types: &Option<Types>) -> HashMap<String, usize> {
    let mut index_map: HashMap<String, usize> = HashMap::new();

    if let Some(types) = types {
        for t in types.iter() {
            if !index_map.contains_key(&t.name) {
                index_map.insert(t.name.to_owned(), index_map.len());
            }
            for t in t.sub_types.iter() {
                if !index_map.contains_key(t) {
                    index_map.insert(t.to_owned(), index_map.len());
                }
            }
        }
    }

    index_map
}

fn extract_predicates(predicates: &Predicates) -> HashMap<String, usize> {
    predicates
        .iter()
        .enumerate()
        .map(|(i, p)| (p.name.to_owned(), i))
        .collect()
}

fn extract_actions(actions: &Actions) -> HashMap<String, usize> {
    actions
        .iter()
        .enumerate()
        .map(|(i, a)| (a.name.to_owned(), i))
        .collect()
}

fn extract_meta_actions(
    actions: &HashMap<String, usize>,
    meta_actions: &Actions,
) -> HashMap<String, usize> {
    let mut index_map: HashMap<String, usize> = HashMap::new();
    for (i, a) in meta_actions.iter().enumerate() {
        if !actions.contains_key(&a.name) {
            index_map.insert(a.name.to_owned(), i);
        }
    }
    index_map
}

fn extract_objects(objects: &Objects) -> HashMap<String, usize> {
    objects
        .iter()
        .enumerate()
        .map(|(i, o)| (o.name.to_owned(), i))
        .collect()
}
