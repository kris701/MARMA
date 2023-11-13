mod cache_data;
pub mod generation;
mod hash_cache;
mod lifted;

use crate::{
    instance::{actions::Action, Instance},
    state::State,
    world::World,
};
use spingus::{sas_plan::SASPlan, term::Term};

pub trait Cache {
    /// Retrives replacement from cache from given init to goal
    fn get_replacement(
        &self,
        instance: &Instance,
        meta_term: &Term,
        init: &State,
        goal: &State,
    ) -> Option<SASPlan>;
}

pub(super) fn generate_plan(
    instance: &Instance,
    replacement_macro: &Action,
    replacement_plan: &SASPlan,
    parameters: &Vec<u16>,
) -> SASPlan {
    let macro_parameters = &replacement_macro.parameters;
    let actions: Vec<String> = replacement_plan.iter().map(|t| t.name.to_owned()).collect();
    let replacements: Vec<&Action> = actions.iter().map(|n| instance.get_action(n)).collect();
    let mut plan: SASPlan = Vec::new();
    for (action, step) in replacements.iter().zip(replacement_plan.iter()) {
        let name = action.name.to_owned();
        let parameters: Vec<u16> = step
            .parameters
            .iter()
            .map(|n| {
                let index = macro_parameters
                    .parameter_names
                    .iter()
                    .position(|p| p == n)
                    .unwrap();
                parameters[index]
            })
            .collect();
        let parameters = World::global().get_object_names_cloned(&parameters);
        plan.push(Term { name, parameters })
    }
    plan
}
