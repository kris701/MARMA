use spingus::{
    domain::{action::Action, Domain},
    problem::Problem,
};

use crate::bit_expression::{extract, BitExp};

use super::{facts::Facts, permutation::permute, Instance};

#[derive(Clone, Debug, Hash, PartialEq, Eq)]
pub struct Operator {
    pub pre_pos: BitExp,
    pub pre_neg: BitExp,
    pub eff_pos: BitExp,
    pub eff_neg: BitExp,
}

pub fn extract_from_action(
    parameters: &Vec<usize>,
    action: &Action,
    facts: &Facts,
) -> Option<Operator> {
    let (pre_neg, pre_pos) = extract(facts, action, &parameters, &action.precondition)?;
    let (eff_neg, eff_pos) = extract(facts, action, &parameters, &Some(action.effect.clone()))?;
    Some(Operator {
        pre_pos,
        pre_neg,
        eff_pos,
        eff_neg,
    })
}

pub fn generate_operator_string(
    domain: &Domain,
    facts: &Facts,
    action: &str,
    parameters: &Vec<String>,
) -> Operator {
    let action: &Action = domain.actions.iter().find(|a| a.name == action).unwrap();
    let parameters: Vec<usize> = parameters.iter().map(|p| facts.object_index(p)).collect();
    extract_from_action(&parameters, action, facts).unwrap()
}

pub fn generate_operators(instance: &Instance, action: &Action) -> Vec<(Operator, Vec<usize>)> {
    let permutations = permute(
        &instance.domain.types,
        &instance.problem,
        &action.parameters,
    );
    permutations
        .iter()
        .filter_map(|p| {
            let operator = extract_from_action(p, action, &instance.facts)?;
            Some((operator, p.to_owned()))
        })
        .collect()
}

pub fn generate_operators_iterative<'a>(
    instance: &'a Instance,
    action: &'a Action,
    permutations: &'a Vec<Vec<usize>>,
) -> impl Iterator<Item = Operator> + 'a {
    permutations.into_iter().filter_map(|p| {
        let operator = extract_from_action(&p, action, &instance.facts)?;
        Some(operator)
    })
}
