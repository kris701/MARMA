use itertools::Itertools;

use crate::state::instance::{objects::Objects, types::Types};

fn permute_untyped(_objects: &Objects, _parameter_count: usize) -> Vec<Vec<usize>> {
    todo!()
}

fn permute_typed(
    types: &Types,
    objects: &Objects,
    parameter_types: &Vec<Option<usize>>,
) -> Vec<Vec<usize>> {
    parameter_types
        .iter()
        .map(|parameter_type| {
            if let Some(parameter_type) = parameter_type {
                (0..objects.count()).into_iter().filter_map(|i| {
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
    parameter_types: &Vec<Option<usize>>,
) -> Vec<Vec<usize>> {
    match types {
        Some(types) => permute_typed(types, objects, parameter_types),
        None => permute_untyped(objects, parameter_types.len()),
    }
}