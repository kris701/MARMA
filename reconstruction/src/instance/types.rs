use crate::world::World;

pub struct Types {
    parent: Vec<Option<usize>>,
}

impl Types {
    pub fn new(types: spingus::domain::types::Types) -> Self {
        let mut parent: Vec<Option<usize>> = Vec::new();

        for t in types {
            let type_index = World::global().get_type_index(&t.name);
            for child in t.sub_types {
                let child_index = World::global().get_type_index(&child);
                parent[type_index as usize] = Some(child_index);
            }
        }

        Self { parent }
    }

    pub fn get_parent(&self, type_index: usize) -> &Option<usize> {
        &self.parent[type_index]
    }

    pub fn is_of_type(&self, type_index: usize, wished: usize) -> bool {
        if type_index == wished {
            return true;
        }
        match self.get_parent(type_index) {
            Some(parent) => return self.is_of_type(*parent, wished),
            None => return false,
        }
    }
}
