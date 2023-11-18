use std::collections::HashMap;

use itertools::Itertools;

use crate::{
    state::State,
    world::{action::Action, World},
};

/// Generates all legal permutations, with some parameters fixed, of an action in a given state
pub fn get_applicable_with_fixed<'a>(
    action: &'a Action,
    state: &'a State,
    fixed: &'a HashMap<usize, usize>,
) -> impl Iterator<Item = Vec<usize>> + 'a {
    let candidates: Vec<Vec<usize>> = action
        .parameters
        .types
        .iter()
        .enumerate()
        .map(|(i, t)| match fixed.get(&i) {
            Some(o) => vec![*o].into_iter().collect(),
            None => World::global()
                .objects
                .iterate_with_type(t)
                .map(|v| v as usize)
                .collect(),
        })
        .collect();

    candidates
        .into_iter()
        .multi_cartesian_product()
        .filter(|p| is_valid(action, state, p))
}

fn is_valid<'a>(action: &'a Action, state: &'a State, permutation: &Vec<usize>) -> bool {
    for atom in action.precondition.iter() {
        let corresponding: Vec<u16> = atom
            .parameters
            .iter()
            .map(|p| permutation[*p as usize] as u16)
            .collect();
        if state.has(atom.predicate, &corresponding) != atom.value {
            return false;
        }
    }
    true
}



