use itertools::Itertools;

use crate::state::instance::{
    expression::Expression, objects::Objects, parameters::Parameters, types::Types,
};

fn permute_static(init: &Expression) -> Vec<Vec<usize>> {
    match init {
        Expression::And(e) => e
            .iter()
            .map(|t| match t {
                Expression::Predicate { parameters, .. } => parameters.clone(),
                _ => panic!("Unexpected expression type."),
            })
            .collect(),
        _ => panic!("Unexpected expression type. This is a logic error, should not happen."),
    }
}

fn permute_untyped(objects: &Objects, parameter_count: usize) -> Vec<Vec<usize>> {
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

fn permute_mutable(
    types: &Option<Types>,
    objects: &Objects,
    parameter_types: &Vec<Option<usize>>,
) -> Vec<Vec<usize>> {
    match types {
        Some(types) => permute_typed(types, objects, parameter_types),
        None => permute_untyped(objects, parameter_types.len()),
    }
}

pub fn permute_all(
    types: &Option<Types>,
    objects: &Objects,
    parameters: &Parameters,
) -> Vec<Vec<usize>> {
    return permute_mutable(types, objects, &parameters.parameter_types);
}
