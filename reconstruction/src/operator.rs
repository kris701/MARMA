use crate::{
    fact::Fact,
    world::{
        action::{Action, Atom},
        World,
    },
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
    let (pre_neg, pre_pos) = walk(parameters, &action.precondition)?;
    let (eff_neg, eff_pos) = walk(parameters, &action.effect)?;
    Some(Operator {
        pre_pos,
        pre_neg,
        eff_pos,
        eff_neg,
    })
}

pub fn generate_operator_string(action: &str, parameters: &Vec<String>) -> Operator {
    let action: &Action = World::global().get_action(action);
    let parameters: Vec<u16> = World::global().objects.indexes(parameters);
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

fn walk(permutation: &Vec<u16>, atoms: &Vec<Atom>) -> Option<(Vec<Fact>, Vec<Fact>)> {
    let mut neg: Vec<Fact> = Vec::new();
    let mut pos: Vec<Fact> = Vec::new();

    for atom in atoms.iter() {
        let arguments: Vec<u16> = atom
            .parameters
            .iter()
            .map(|p| permutation[*p as usize])
            .collect();
        if atom.predicate == 0 {
            // Check equality atoms
            if !arguments.iter().all_equal() {
                return None;
            }
            continue;
        }
        let fact = Fact::new(atom.predicate, arguments);
        match atom.value {
            true => pos.push(fact),
            false => neg.push(fact),
        };
    }

    Some((neg, pos))
}
