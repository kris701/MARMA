use std::collections::HashSet;

use itertools::Itertools;

use crate::world::World;

use super::{actions::Action, expression::Expression, permute::permute_mutable, Instance};

#[derive(Clone, Debug, PartialEq, Eq)]
pub struct Operator {
    pub pre_pos: HashSet<u32>,
    pub pre_neg: HashSet<u32>,
    pub eff_pos: HashSet<u32>,
    pub eff_neg: HashSet<u32>,
}

impl Operator {
    pub fn get_effect(&self) -> Vec<(u32, bool)> {
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

pub fn extract_from_action(
    instance: &Instance,
    parameters: &Vec<u32>,
    action: &Action,
) -> Option<Operator> {
    let mut pre_pos: HashSet<u32> = HashSet::new();
    let mut pre_neg: HashSet<u32> = HashSet::new();
    let mut eff_pos: HashSet<u32> = HashSet::new();
    let mut eff_neg: HashSet<u32> = HashSet::new();
    if let Some(exp) = &action.precondition {
        if !walk(instance, parameters, &mut pre_pos, &mut pre_neg, exp) {
            return None;
        }
    }
    if !walk(
        instance,
        parameters,
        &mut eff_pos,
        &mut eff_neg,
        &action.effect,
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
    let parameters: Vec<u32> = World::global().get_object_indexes(parameters);
    extract_from_action(instance, &parameters, action).unwrap()
}

pub fn generate_operators<'a>(
    instance: &'a Instance,
    action: &'a Action,
) -> impl Iterator<Item = (Operator, Vec<u32>)> + 'a {
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
    permutation: &Vec<u32>,
    pos: &mut HashSet<u32>,
    neg: &mut HashSet<u32>,
    exp: &Expression,
) -> bool {
    let facts = &instance.facts;
    for equal in exp.equals.iter() {
        let parameters = equal
            .parameters
            .iter()
            .map(|p| permutation[*p as usize])
            .collect_vec();
        if !parameters.iter().all_equal() {
            return false;
        }
    }
    for literal in exp.literals.iter() {
        let predicate = literal.predicate;
        let parameters = literal
            .parameters
            .iter()
            .map(|p| permutation[*p as usize])
            .collect_vec();

        if facts.is_static(predicate) && !facts.is_statically_true(predicate, &parameters) {
            return false;
        } else {
            let fact = facts.index(predicate, &parameters);
            match literal.value {
                true => pos.insert(fact),
                false => neg.insert(fact),
            };
        }
    }
    true
}
