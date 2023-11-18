use std::collections::HashMap;

use spingus::domain::parameter::Parameter;

use super::{types::Types, World};

pub struct Objects {
    objects: HashMap<String, u16>,
    object_types: HashMap<u16, usize>,
}

impl Objects {
    pub fn count(&self) -> u16 {
        self.objects.len() as u16
    }

    pub fn index(&self, name: &str) -> u16 {
        self.objects[name] as u16
    }

    pub fn indexes(&self, names: &Vec<String>) -> Vec<u16> {
        names.iter().map(|i| self.index(i)).collect()
    }

    pub fn name(&self, index: u16) -> &String {
        &self.objects.iter().find(|(_, i)| **i == index).unwrap().0
    }

    pub fn names(&self, indexes: &Vec<u16>) -> Vec<&String> {
        indexes.iter().map(|i| self.name(*i)).collect()
    }

    pub fn names_cloned(&self, indexes: &Vec<usize>) -> Vec<String> {
        indexes
            .iter()
            .map(|i| self.name(*i as u16).to_owned())
            .collect()
    }

    pub fn object_type(&self, index: u16) -> usize {
        self.object_types[&index]
    }

    pub fn iterate_typed<'a>(&'a self) -> impl Iterator<Item = (u16, usize)> + 'a {
        self.objects.iter().map(|(_, v)| (*v, self.object_type(*v)))
    }

    pub fn iterate_named<'a>(&'a self) -> impl Iterator<Item = (&String, &String)> + 'a {
        self.objects.iter().map(|(name, index)| {
            let object_type = self.object_type(*index);
            let type_name = World::global().types.name(object_type);
            (name, type_name)
        })
    }

    pub fn iterate_with_type<'a>(&'a self, type_id: &'a usize) -> impl Iterator<Item = u16> + 'a {
        self.iterate_typed().filter_map(|(object_id, t)| {
            match World::global().types.is_of_type(t, *type_id) {
                true => Some(object_id),
                false => None,
            }
        })
    }
}

pub(super) fn translate_objects(
    types: &Types,
    constants: Option<spingus::domain::parameter::Parameters>,
    objects: spingus::problem::object::Objects,
) -> Objects {
    let mut objects = objects.clone();
    match constants {
        Some(parameters) => objects.append(
            &mut parameters
                .iter()
                .map(|p| match p {
                    Parameter::Untyped { name } => spingus::problem::object::Object {
                        name: name.to_string(),
                        type_name: None,
                    },
                    Parameter::Typed { name, type_name } => spingus::problem::object::Object {
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
    let (objects, object_types): (HashMap<String, u16>, HashMap<u16, usize>) =
        temp.into_iter().unzip();
    println!("object_count={}", objects.len());
    Objects {
        objects,
        object_types,
    }
}
