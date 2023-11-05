use std::collections::HashMap;

use crate::world::World;

pub struct Objects {
    index_map: HashMap<String, usize>,
    object_types: Vec<Option<usize>>,
}

impl Objects {
    pub fn new(o_objects: spingus::problem::object::Objects) -> Self {
        let mut index_map: HashMap<String, usize> = HashMap::new();
        let mut object_types: Vec<Option<usize>> = Vec::new();

        for object in o_objects {
            index_map.insert(object.name, index_map.len());
            let type_index = match object.type_name {
                Some(type_name) => {
                    let type_index = World::global().get_type_index(&type_name);
                    Some(type_index)
                }
                None => None,
            };
            object_types.push(type_index);
        }

        Self {
            index_map,
            object_types,
        }
    }

    pub fn count(&self) -> usize {
        self.index_map.len()
    }

    pub fn get_index(&self, object_name: &str) -> usize {
        self.index_map[object_name]
    }

    pub fn get_indexes(&self, object_names: &Vec<String>) -> Vec<usize> {
        object_names.iter().map(|n| self.get_index(n)).collect()
    }

    #[allow(dead_code)]
    pub fn get_name(&self, index: usize) -> &String {
        &self.index_map.iter().find(|(_, i)| **i == index).unwrap().0
    }

    #[allow(dead_code)]
    pub fn get_names(&self, indexes: &Vec<usize>) -> Vec<&String> {
        indexes.iter().map(|i| self.get_name(*i)).collect()
    }

    #[allow(dead_code)]
    pub fn get_names_cloned(&self, indexes: &Vec<usize>) -> Vec<String> {
        indexes
            .iter()
            .map(|i| self.get_name(*i).to_owned())
            .collect()
    }

    pub fn get_type(&self, object_index: usize) -> &Option<usize> {
        &self.object_types[object_index]
    }
}
