use itertools::Itertools;

use crate::world::World;

use super::{objects::Objects, types::Types};

fn permute_untyped(parameter_count: u32) -> Vec<Vec<u32>> {
    (0..parameter_count)
        .into_iter()
        .map(|_| (0..World::global().get_object_count()).into_iter())
        .multi_cartesian_product()
        .collect()
}

fn permute_typed(
    types: &Types,
    objects: &Objects,
    parameter_types: &Vec<Option<u32>>,
) -> Vec<Vec<u32>> {
    parameter_types
        .iter()
        .map(|parameter_type| {
            if let Some(parameter_type) = parameter_type {
                (0..World::global().get_object_count())
                    .into_iter()
                    .filter_map(|i| {
                        let object_type = objects.get_type(i);
                        if let Some(object_type) = object_type {
                            match types.is_of_type(*object_type, *parameter_type) {
                                true => Some(i),
                                false => None,
                            }
                        } else {
                            None
                        }
                    })
            } else {
                panic!()
            }
        })
        .multi_cartesian_product()
        .collect()
}

pub fn permute_mutable(
    types: &Option<Types>,
    objects: &Objects,
    parameter_types: &Vec<Option<u32>>,
) -> Vec<Vec<u32>> {
    if parameter_types.is_empty() {
        return vec![vec![]];
    }
    match types {
        Some(types) => permute_typed(types, objects, parameter_types),
        None => permute_untyped(parameter_types.len() as u32),
    }
}
