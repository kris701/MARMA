mod cache_data;
mod exact;
pub mod generation;

use std::{
    collections::HashMap,
    sync::atomic::{AtomicUsize, Ordering},
};

use crate::{
    plan::{Plan, Step},
    state::State,
    world::{action::Action, World},
};

pub static INVALID_REPLACEMENTS: AtomicUsize = AtomicUsize::new(0);

pub trait Cache {
    /// Retrives replacement from cache from given init to goal
    fn get_replacement(&self, step: &Step, init: &State, goal: &State) -> Option<Plan>;
    fn add_entry(&mut self, step: &Step, replacement_plan: &Plan);
}

pub(super) fn generate_plan(
    init: &State,
    replacement_macro: &Action,
    replacement_plan: &Plan,
    parameters: &Vec<usize>,
) -> Option<Plan> {
    let macro_parameters = &replacement_macro.parameters;
    let mut state = init.clone();
    let mut steps: Vec<Step> = Vec::new();
    for step in replacement_plan.iter() {
        let action = World::global().actions.get(step.action);
        let parameters: Vec<usize> = step.args.iter().map(|n| parameters[*n]).collect();
        let parameters_named = World::global().objects.names_cloned(&parameters);
        for pre in action.precondition.iter() {
            if state.has_nary(pre.predicate, &pre.map_args(&parameters)) != pre.value {
                INVALID_REPLACEMENTS.fetch_add(1, Ordering::SeqCst);
                return None;
            }
        }
        state.apply(action, &parameters);
        steps.push(Step {
            action: step.action,
            args: parameters,
        })
    }
    Some(Plan::new(steps))
}
pub(super) fn find_fixed(arguments: &Vec<usize>, macro_action: &Action) -> HashMap<usize, usize> {
    macro_action
        .parameters
        .iterate()
        .enumerate()
        .filter_map(|(i, (name, _))| match name.to_uppercase().contains('O') {
            true => None,
            false => {
                let parameter_index = name.replace('?', "").parse::<usize>().unwrap();
                Some((i, arguments[parameter_index]))
            }
        })
        .collect()
}
