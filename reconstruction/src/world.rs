use once_cell::sync::OnceCell;
use spingus::{
    domain::{action::Actions, predicate::Predicates, types::Types},
    problem::object::Objects,
};
use std::collections::HashMap;

pub struct World {
    pub types: HashMap<String, u32>,
    pub predicates: HashMap<String, u32>,
    pub actions: HashMap<String, u32>,
    pub objects: HashMap<String, u32>,
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
        let types = extract_tyes(&domain.types);
        let predicates = extract_predicates(&domain.predicates);
        let actions = extract_actions(&domain.actions);
        let objects = extract_objects(&problem.objects);
        Self {
            types,
            predicates,
            actions,
            objects,
        }
    }
}

fn extract_tyes(types: &Option<Types>) -> HashMap<String, u32> {
    let mut index_map: HashMap<String, u32> = HashMap::new();

    if let Some(types) = types {
        for t in types.iter() {
            if !index_map.contains_key(&t.name) {
                index_map.insert(t.name.to_owned(), index_map.len() as u32);
            }
            for t in t.sub_types.iter() {
                if !index_map.contains_key(t) {
                    index_map.insert(t.to_owned(), index_map.len() as u32);
                }
            }
        }
    }

    index_map
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

fn extract_objects(objects: &Objects) -> HashMap<String, u32> {
    objects
        .iter()
        .enumerate()
        .map(|(i, o)| (o.name.to_owned(), i as u32))
        .collect()
}
