use crate::{
    fact::Fact,
    instance::{actions::Action, expression::Expression, Instance},
    world::World,
};
use itertools::Itertools;

#[derive(Clone, Debug, PartialEq, Eq)]
pub struct Operator {
    pub pre_pos: Vec<Fact>,
    pub pre_neg: Vec<Fact>,
    pub eff_pos: Vec<Fact>,
    pub eff_neg: Vec<Fact>,
}

impl Operator {
    pub fn get_effect(&self) -> Vec<(Fact, bool)> {
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

pub fn extract_from_action(parameters: &Vec<u16>, action: &Action) -> Option<Operator> {
    let mut pre_pos: Vec<Fact> = Vec::new();
    let mut pre_neg: Vec<Fact> = Vec::new();
    let mut eff_pos: Vec<Fact> = Vec::new();
    let mut eff_neg: Vec<Fact> = Vec::new();
    if let Some(exp) = &action.precondition {
        if !walk(parameters, &mut pre_pos, &mut pre_neg, exp) {
            return None;
        }
    }
    if !walk(parameters, &mut eff_pos, &mut eff_neg, &action.effect) {
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
    let parameters: Vec<u16> = World::global().get_object_indexes(parameters);
    extract_from_action(&parameters, action).unwrap()
}

pub fn generate_operators_by_candidates<'a>(
    action: &'a Action,
    candidates: Vec<Vec<u16>>,
) -> impl Iterator<Item = (Operator, Vec<u16>)> + 'a {
    candidates
        .into_iter()
        .multi_cartesian_product()
        .filter_map(|p| {
            let operator = extract_from_action(&p, action)?;
            Some((operator, p))
        })
}

fn walk(
    permutation: &Vec<u16>,
    pos: &mut Vec<Fact>,
    neg: &mut Vec<Fact>,
    exp: &Expression,
) -> bool {
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

        let fact = Fact::new(predicate, parameters);
        match literal.value {
            true => pos.push(fact),
            false => neg.push(fact),
        };
    }
    true
}
