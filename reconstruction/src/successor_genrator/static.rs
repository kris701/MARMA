use std::collections::HashMap;

use itertools::Itertools;

use crate::{
    fact::Fact,
    world::{action::Action, World},
};

use super::increment_counter;

pub fn generate_statically_with_fixed<'a>(
    action: &'a Action,
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
                .map(|v| v as usize)
                .collect(),
        })
        .collect();

    for unary in action.unary.iter() {
        let atom = &action.precondition[*unary];
        let predicate = atom.predicate;
        if !World::global().predicates.is_static(predicate) {
            continue;
        }
        let parameter = atom.parameters[0];
        candidates[parameter].retain(|o| {
            World::global()
                .static_facts
                .contains(&Fact::new(predicate, vec![*o]))
        });
    }
    candidates
        .into_iter()
        .multi_cartesian_product()
        .filter(|p| {
            increment_counter();
            is_valid(action, p)
        })
}

fn is_valid<'a>(action: &'a Action, permutation: &Vec<usize>) -> bool {
    for (_, atom) in action
        .precondition
        .iter()
        .enumerate()
        .filter(|(i, ..)| !action.unary.contains(i))
        .filter(|(_, a)| World::global().predicates.is_static(a.predicate))
    {
        let corresponding: Vec<usize> = atom.parameters.iter().map(|p| permutation[*p]).collect();
        if atom.predicate == 0 && corresponding.iter().all_equal() != atom.value {
            return false;
        } else if World::global()
            .static_facts
            .contains(&Fact::new(atom.predicate, corresponding))
            != atom.value
        {
            return false;
        }
    }
    true
}
