use super::{objects::Objects, types::Types};
use crate::world::World;
use itertools::Itertools;

pub fn permute_unary(candidates: Vec<Vec<u32>>) -> impl Iterator<Item = Vec<u32>> {
    candidates.into_iter().multi_cartesian_product()
}

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
    let object_range: Vec<u32> = (0..World::global().get_object_count()).collect();
    let candidates = parameter_types
        .iter()
        .map(move |t| match t {
            Some(t) => object_range
                .iter()
                .filter_map(|o| match objects.get_type(*o) {
                    Some(o_t) => match types.is_of_type(*o_t, *t) {
                        true => Some(*o),
                        false => None,
                    },
                    None => None,
                })
                .collect(),
            None => object_range.iter().map(|o| *o).collect(),
        })
        .collect();
    permute_unary(candidates).collect()
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
