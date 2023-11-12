use crate::world::World;
use itertools::Itertools;

pub fn permute_unary(candidates: Vec<Vec<u32>>) -> impl Iterator<Item = Vec<u32>> {
    candidates.into_iter().multi_cartesian_product()
}

pub fn get_candidates_typed(parameter_types: &Vec<u32>) -> Vec<Vec<u32>> {
    parameter_types
        .iter()
        .map(move |t| {
            World::global()
                .iterate_objects()
                .filter_map(
                    |(object_id, type_id)| match World::global().is_of_type(type_id, *t) {
                        true => Some(object_id),
                        false => None,
                    },
                )
                .collect()
        })
        .collect()
}

fn permute_typed(parameter_types: &Vec<u32>) -> Vec<Vec<u32>> {
    let candidates = get_candidates_typed(parameter_types);
    permute_unary(candidates).collect()
}

pub fn permute_mutable(parameter_types: &Vec<u32>) -> Vec<Vec<u32>> {
    if parameter_types.is_empty() {
        return vec![vec![]];
    }
    permute_typed(parameter_types)
}
