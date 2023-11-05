use std::collections::HashMap;

use crate::world::World;

pub struct Types {
    parent: HashMap<usize, usize>,
}

impl Types {
    pub fn new(types: spingus::domain::types::Types) -> Self {
        let mut parent: HashMap<usize, usize> = HashMap::new();

        for t in types {
            let type_index = World::global().get_type_index(&t.name);
            for child in t.sub_types {
                let child_index = World::global().get_type_index(&child);
                parent.insert(type_index, child_index);
            }
        }

        Self { parent }
    }

    pub fn get_parent(&self, type_index: usize) -> Option<usize> {
        self.parent.get(&type_index).cloned()
    }

    pub fn is_of_type(&self, type_index: usize, wished: usize) -> bool {
        if type_index == wished {
            return true;
        }
        match self.get_parent(type_index) {
            Some(parent) => return self.is_of_type(parent, wished),
            None => return false,
        }
    }
}
