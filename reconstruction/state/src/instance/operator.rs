use spingus::{
    domain::{action::Action, Domain},
    problem::Problem,
};

use crate::bit_expression::{extract, BitExp};

use super::{facts::Facts, permutation::permute};

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
) -> Result<Operator, &'static str> {
    let (pre_neg, pre_pos) = extract(facts, action, &parameters, &action.precondition)?;
    let (eff_neg, eff_pos) = extract(facts, action, &parameters, &Some(action.effect.clone()))?;
    Ok(Operator {
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

pub fn generate_operators(
    domain: &Domain,
    problem: &Problem,
    facts: &Facts,
    action: &Action,
) -> Vec<(Operator, Vec<usize>)> {
    let permutations = permute(&domain.types, problem, &action.parameters);
    permutations
        .iter()
        .map(|p| (extract_from_action(p, action, facts).unwrap(), p.to_owned()))
        .collect()
}
