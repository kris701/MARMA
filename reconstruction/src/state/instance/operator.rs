use std::collections::HashSet;

use itertools::Itertools;

use super::{actions::Action, expression::Expression, permute::permute_mutable, Instance};

#[derive(Clone, Debug, PartialEq, Eq)]
pub struct Operator {
    pub pre_pos: HashSet<usize>,
    pub pre_neg: HashSet<usize>,
    pub eff_pos: HashSet<usize>,
    pub eff_neg: HashSet<usize>,
}

impl Operator {
    pub fn get_effect(&self) -> Vec<(usize, bool)> {
        let mut effect = vec![];

        for i in self.eff_pos.iter() {
            effect.push((*i, true));
        }
        for i in self.eff_neg.iter() {
            effect.push((*i, false));
        }

        effect.sort_by(|a, b| a.0.cmp(&b.0));
        effect
    }
}

fn extract_from_action(
    instance: &Instance,
    parameters: &Vec<usize>,
    action: &Action,
) -> Option<Operator> {
    let mut pre_pos: HashSet<usize> = HashSet::new();
    let mut pre_neg: HashSet<usize> = HashSet::new();
    let mut eff_pos: HashSet<usize> = HashSet::new();
    let mut eff_neg: HashSet<usize> = HashSet::new();
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
    pos: &mut HashSet<usize>,
    neg: &mut HashSet<usize>,
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
                false => {
                    exp.insert(instance.facts.index(*index, &parameters));
                    return true;
                }
            }
        }
        Expression::Equal(exps) => exps.iter().all_equal(),
        Expression::And(exps) => exps
            .iter()
            .all(|exp| walk(instance, permutation, pos, neg, exp, value)),
        Expression::Not(exp) => walk(instance, permutation, pos, neg, exp, !value),
        _ => todo!(),
    }
}
