use crate::{
    state::State,
    world::{
        action::Action,
        atom::{Argument, Atom},
        World,
    },
};
use itertools::Itertools;
use std::{
    collections::HashMap,
    sync::atomic::{AtomicUsize, Ordering},
};

static PSEUDO_OPERATORS: AtomicUsize = AtomicUsize::new(0);
static LEGAL_OPERATORS: AtomicUsize = AtomicUsize::new(0);

fn increment_pseudo() {
    PSEUDO_OPERATORS.fetch_add(1, Ordering::SeqCst);
}

pub fn pseudo_count() -> usize {
    PSEUDO_OPERATORS.load(Ordering::SeqCst)
}

fn increment_legal() {
    LEGAL_OPERATORS.fetch_add(1, Ordering::SeqCst);
}

pub fn legal_count() -> usize {
    LEGAL_OPERATORS.load(Ordering::SeqCst)
}

/// Generates all legal permutations, with some parameters fixed, of an action in a given state
pub fn get_applicable_with_fixed<'a>(
    action: &'a Action,
    state: &'a State,
    fixed: &'a HashMap<usize, usize>,
) -> Option<impl Iterator<Item = Vec<usize>> + 'a> {
    let (nullary_atoms, other_atoms): (Vec<&Atom>, Vec<&Atom>) =
        action.precondition.iter().partition(|a| a.is_nullary());

    if nullary_atoms
        .iter()
        .any(|a| state.has_nullary(a.predicate) != a.value)
    {
        return None;
    }

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

    let (unary_atoms, nary_atoms): (Vec<&Atom>, Vec<&Atom>) =
        other_atoms.into_iter().partition(|a| a.is_unary());

    for atom in unary_atoms {
        match atom.parameters[0] {
            Argument::Parameter(p) => {
                candidates[p].retain(|o| state.has_unary(atom.predicate, *o) == atom.value);
            }
            Argument::Constant(c) => {
                if state.has_unary(atom.predicate, c) != atom.value {
                    return None;
                }
            }
        };
    }

    for atom in nary_atoms.iter().filter(|a| a.value) {
        debug_assert!(World::global().predicates.arity(atom.predicate) > 1);
        for (i, parameter) in atom.parameters.iter().enumerate() {
            match parameter {
                Argument::Parameter(p) => {
                    candidates[*p].retain(|o| state.has_partial(atom.predicate, i, *o))
                }
                Argument::Constant(c) => {
                    if state.has_partial(atom.predicate, i, *c) {
                        return None;
                    }
                }
            }
        }
    }
    Some(
        candidates
            .into_iter()
            .multi_cartesian_product()
            .filter(move |p| {
                increment_pseudo();
                let val = nary_atoms.iter().all(|atom| {
                    let corresponding: Vec<usize> = atom.map_args(p);
                    (atom.predicate == 0 && corresponding.iter().all_equal() == atom.value)
                        || (atom.predicate != 0
                            && state.has_nary(atom.predicate, &corresponding) == atom.value)
                });
                if val {
                    increment_legal();
                }
                val
            }),
    )
}
