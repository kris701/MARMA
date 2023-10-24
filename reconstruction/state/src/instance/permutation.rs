use itertools::Itertools;
use spingus::{
    domain::{
        parameter::{Parameter, Parameters},
        types::Types,
    },
    problem::{object::Object, Problem},
};

fn is_valid_type(types: &Types, expected_type: &String, actual_type: &Option<String>) -> bool {
    match actual_type {
        Some(t) => match t == expected_type {
            true => true,
            false => match types.iter().find(|a| a.name == *t) {
                Some(t) => t
                    .sub_types
                    .iter()
                    .any(|t| is_valid_type(types, expected_type, &Some(t.to_owned()))),
                None => false,
            },
        },
        None => false,
    }
}

fn is_valid_object(types: &Types, parameter: &Parameter, o: &Object) -> bool {
    match parameter {
        Parameter::Untyped { name: _ } => true,
        Parameter::Typed { name: _, type_name } => is_valid_type(types, type_name, &o.type_name),
        _ => todo!(),
    }
}

fn permute_all(types: &Types, problem: &Problem, parameters: &Parameters) -> Vec<Vec<usize>> {
    parameters
        .iter()
        .map(|parameter| {
            problem.objects.iter().enumerate().filter_map(|(i, o)| {
                if is_valid_object(types, parameter, o) {
                    Some(i)
                } else {
                    None
                }
            })
        })
        .multi_cartesian_product()
        .collect()
}

/// Generates all pseudo-legal parameter permutations with regards to types
pub fn permute(
    types: &Option<Types>,
    problem: &Problem,
    parameters: &Parameters,
) -> Vec<Vec<usize>> {
    if parameters.is_empty() {
        return vec![vec![]];
    }
    if let Some(types) = types {
        return permute_all(&types, problem, parameters);
    } else {
        return permute_all(&vec![], problem, parameters);
    }
}
