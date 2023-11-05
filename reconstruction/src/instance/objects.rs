use crate::world::World;

pub struct Objects {
    object_types: Vec<Option<usize>>,
}

impl Objects {
    pub fn new(o_objects: spingus::problem::object::Objects) -> Self {
        let mut object_types: Vec<Option<usize>> = Vec::new();

        for object in o_objects {
            let type_index = match object.type_name {
                Some(type_name) => {
                    let type_index = World::global().get_type_index(&type_name);
                    Some(type_index)
                }
                None => None,
            };
            object_types.push(type_index);
        }

        Self { object_types }
    }

    pub fn get_type(&self, object_index: usize) -> &Option<usize> {
        &self.object_types[object_index]
    }
}
