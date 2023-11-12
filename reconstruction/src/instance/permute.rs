use super::types::Types;
use crate::world::World;
use itertools::Itertools;

pub fn permute_unary(candidates: Vec<Vec<u32>>) -> impl Iterator<Item = Vec<u32>> {
    candidates.into_iter().multi_cartesian_product()
}

pub fn get_candidates_typed(types: &Types, parameter_types: &Vec<Option<u32>>) -> Vec<Vec<u32>> {
    let object_range: Vec<u32> = (0..World::global().get_object_count()).collect();
    parameter_types
        .iter()
        .map(move |t| match t {
            Some(t) => object_range
                .iter()
                .filter_map(|o| {
                    let object_type = World::global().get_object_type(*o);
                    match types.is_of_type(object_type, *t) {
                        true => Some(*o),
                        false => None,
                    }
                })
                .collect(),
            None => object_range.iter().map(|o| *o).collect(),
        })
        .collect()
}

fn permute_untyped(parameter_count: u32) -> Vec<Vec<u32>> {
    (0..parameter_count)
        .into_iter()
        .map(|_| (0..World::global().get_object_count()).into_iter())
        .multi_cartesian_product()
        .collect()
}

fn permute_typed(types: &Types, parameter_types: &Vec<Option<u32>>) -> Vec<Vec<u32>> {
    let candidates = get_candidates_typed(types, parameter_types);
    permute_unary(candidates).collect()
}

pub fn permute_mutable(types: &Option<Types>, parameter_types: &Vec<Option<u32>>) -> Vec<Vec<u32>> {
    if parameter_types.is_empty() {
        return vec![vec![]];
    }
    match types {
        Some(types) => permute_typed(types, parameter_types),
        None => permute_untyped(parameter_types.len() as u32),
    }
}
