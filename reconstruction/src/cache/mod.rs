mod cache_data;
pub mod generation;
mod hash;
mod lifted;

use std::collections::HashMap;

use crate::{
    state::State,
    world::{action::Action, World},
};
use spingus::{sas_plan::SASPlan, term::Term};

pub trait Cache {
    /// Retrives replacement from cache from given init to goal
    fn get_replacement(&self, meta_term: &Term, init: &State, goal: &State) -> Option<SASPlan>;
}

pub(super) fn generate_plan(
    replacement_macro: &Action,
    replacement_plan: &SASPlan,
    parameters: &Vec<usize>,
) -> SASPlan {
    let macro_parameters = &replacement_macro.parameters;
    let actions: Vec<String> = replacement_plan.iter().map(|t| t.name.to_owned()).collect();
    let replacements: Vec<&Action> = actions
        .iter()
        .map(|n| World::global().get_action(n))
        .collect();
    let mut plan: SASPlan = Vec::new();
    for (action, step) in replacements.iter().zip(replacement_plan.iter()) {
        let name = action.name.to_owned();
        let parameters: Vec<usize> = step
            .parameters
            .iter()
            .map(|n| {
                let index = macro_parameters.names.iter().position(|p| p == n).unwrap();
                parameters[index]
            })
            .collect();
        let parameters = World::global().objects.names_cloned(&parameters);
        plan.push(Term { name, parameters })
    }
    plan
}

pub(super) fn find_fixed(arguments: &Vec<usize>, macro_action: &Action) -> HashMap<usize, usize> {
    macro_action
        .parameters
        .names
        .iter()
        .enumerate()
        .filter_map(|(i, name)| match name.to_uppercase().contains('O') {
            true => None,
            false => {
                let parameter_index = name.parse::<usize>().unwrap();
                Some((i, arguments[parameter_index]))
            }
        })
        .collect()
}
