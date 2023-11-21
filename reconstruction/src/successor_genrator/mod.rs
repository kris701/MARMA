pub mod r#static;

use crate::{
    state::State,
    world::{action::Action, World},
};
use itertools::Itertools;
use std::{
    collections::HashMap,
    sync::atomic::{AtomicUsize, Ordering},
};

static PERMUTATION_COUNT: AtomicUsize = AtomicUsize::new(0);

fn increment_counter() {
    PERMUTATION_COUNT.fetch_add(1, Ordering::SeqCst);
}

pub fn get_permutation_count() -> usize {
    PERMUTATION_COUNT.load(Ordering::SeqCst)
}

/// Generates all legal permutations, with some parameters fixed, of an action in a given state
pub fn get_applicable_with_fixed<'a>(
    action: &'a Action,
    state: &'a State,
    fixed: &'a HashMap<usize, usize>,
) -> impl Iterator<Item = Vec<usize>> + 'a {
    let mut candidates: Vec<Vec<usize>> = action
        .parameters
        .types
        .iter()
        .enumerate()
        .map(|(i, t)| match fixed.get(&i) {
            Some(o) => vec![*o].into_iter().collect(),
            None => World::global()
                .objects
                .iterate_with_type(t)
                .map(|v| v)
                .collect(),
        })
        .collect();

    for atom in action.precondition.iter().filter(|a| a.is_unary()) {
        let parameter = atom.parameters[0];
        candidates[parameter].retain(|o| state.has(atom.predicate, &vec![*o]) == atom.value);
    }

    candidates
        .into_iter()
        .multi_cartesian_product()
        .filter(|p| {
            increment_counter();
            is_valid(action, state, p)
        })
}

fn is_valid<'a>(action: &'a Action, state: &'a State, permutation: &Vec<usize>) -> bool {
    for atom in action.precondition.iter().filter(|a| !a.is_unary()) {
        let corresponding: Vec<usize> = atom.map_args(permutation);
        if atom.predicate == 0 && corresponding.iter().all_equal() != atom.value {
            return false;
        } else if state.has(atom.predicate, &corresponding) != atom.value {
            return false;
        }
    }
    true
}
