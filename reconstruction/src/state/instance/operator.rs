use bitvec::prelude::*;
use bitvec::vec::BitVec;
use itertools::Itertools;

use crate::tools::time::run_time;

use super::{
    actions::Action, expression::Expression, facts::Facts, permute::permute_mutable, Instance,
};

#[derive(Clone, Debug, Hash, PartialEq, Eq)]
pub struct Operator {
    pub pre_pos: BitVec,
    pub pre_neg: BitVec,
    pub eff_pos: BitVec,
    pub eff_neg: BitVec,
}

fn extract_from_action(
    instance: &Instance,
    parameters: &Vec<usize>,
    action: &Action,
) -> Option<Operator> {
    let mut pre_pos = bitvec!(usize, Lsb0; 0; instance.facts.count());
    let mut pre_neg = bitvec!(usize, Lsb0; 0; instance.facts.count());
    let mut eff_pos = bitvec!(usize, Lsb0; 0; instance.facts.count());
    let mut eff_neg = bitvec!(usize, Lsb0; 0; instance.facts.count());
    if let Some(exp) = &action.precondition {
        if !walk(instance, parameters, &mut pre_pos, &mut pre_neg, exp, true) {
            return None;
        }
    }
    if !walk(
        instance,
        parameters,
        &mut eff_pos,
        &mut eff_neg,
        &action.effect,
        true,
    ) {
        return None;
    }
    Some(Operator {
        pre_pos,
        pre_neg,
        eff_pos,
        eff_neg,
    })
}

pub fn generate_operator_string(
    instance: &Instance,
    action: &str,
    parameters: &Vec<String>,
) -> Operator {
    let action: &Action = instance.get_action(action);
    let parameters: Vec<usize> = instance.objects.get_indexes(parameters);
    extract_from_action(instance, &parameters, action).unwrap()
}

pub fn generate_operators<'a>(
    instance: &'a Instance,
    action: &'a Action,
) -> impl Iterator<Item = (Operator, Vec<usize>)> + 'a {
    let permutations = permute_mutable(
        &instance.types,
        &instance.objects,
        &action.parameters.parameter_types,
    );
    permutations.into_iter().filter_map(|p| {
        let operator = extract_from_action(instance, &p, action)?;
        Some((operator, p))
    })
}

fn walk(
    instance: &Instance,
    permutation: &Vec<usize>,
    pos: &mut BitVec,
    neg: &mut BitVec,
    exp: &Expression,
    value: bool,
) -> bool {
    match exp {
        Expression::Predicate { index, parameters } => {
            let exp = if value { pos } else { neg };

            let parameters = parameters.iter().map(|p| permutation[*p]).collect_vec();

            match instance.facts.is_static(*index) {
                true => {
                    return instance.facts.is_statically_true(*index, &parameters);
                }
                false => exp.set(instance.facts.index(*index, &parameters), true),
            }

            true
        }
        Expression::Equal(exps) => exps.iter().all_equal(),
        Expression::And(exps) => exps
            .iter()
            .all(|exp| walk(instance, permutation, pos, neg, exp, value)),
        Expression::Not(exp) => walk(instance, permutation, pos, neg, exp, !value),
        _ => todo!(),
    }
}
