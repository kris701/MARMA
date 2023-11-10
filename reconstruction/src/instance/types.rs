use std::collections::HashMap;

use crate::world::World;

pub struct Types {
    parent: HashMap<u32, u32>,
}

impl Types {
    pub fn new(types: spingus::domain::types::Types) -> Self {
        let mut parent: HashMap<u32, u32> = HashMap::new();

        for t in types {
            let type_index = World::global().get_type_index(&t.name);
            for child in t.sub_types {
                let child_index = World::global().get_type_index(&child);
                parent.insert(type_index, child_index);
            }
        }

        Self { parent }
    }

    pub fn get_parent(&self, type_index: u32) -> Option<u32> {
        self.parent.get(&type_index).cloned()
    }

    pub fn is_of_type(&self, type_index: u32, wished: u32) -> bool {
        if type_index == wished {
            return true;
        }
        match self.get_parent(type_index) {
            Some(parent) => return self.is_of_type(parent, wished),
            None => return false,
        }
    }
}
