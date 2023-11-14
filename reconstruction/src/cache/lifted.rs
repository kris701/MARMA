use std::collections::HashMap;

use itertools::Itertools;
use spingus::{sas_plan::SASPlan, term::Term};

use crate::{
    instance::{actions::Action, operator::generate_operators_by_candidates, Instance},
    state::State,
    tools::{status_print, Status},
    world::World,
};

use super::{cache_data::CacheData, generate_plan, Cache};

#[derive(Debug)]
struct Replacement {
    action: Action,
    plan: SASPlan,
    candidates: Vec<Vec<u16>>,
}

fn generate_replacements(
    instance: &Instance,
    cache_data: &CacheData,
    meta_index: &u16,
    parameters: &Vec<u16>,
) -> Option<Vec<Replacement>> {
    let relevant_replacements = cache_data
        .iter()
        .find(|(meta_action, replacements)| {
            *meta_index == World::global().get_meta_index(meta_action)
        })?
        .1;
    let replacements = relevant_replacements
        .iter()
        .map(|(action, sas_plan)| {
            let action = instance.convert_action(action.clone());
            let plan = sas_plan.to_owned();
            let candidates = action
                .parameters
                .parameter_names
                .iter()
                .zip(action.parameters.parameter_types.iter())
                .map(|(name, type_id)| match name.to_uppercase().contains('O') {
                    true => World::global().get_objects_with_type(*type_id),
                    false => {
                        let parameter_index = name.parse::<usize>().unwrap();
                        vec![parameters[parameter_index]]
                    }
                })
                .collect();
            Replacement {
                action,
                plan,
                candidates,
            }
        })
        .collect();
    Some(replacements)
}

#[derive(Debug)]
pub struct LiftedCache {
    replacements: HashMap<(u16, Vec<u16>), Vec<Replacement>>,
}

impl LiftedCache {
    pub fn new(
        instance: &Instance,
        cache_data: CacheData,
        used_meta_actions: Vec<(u16, Vec<u16>)>,
    ) -> Self {
        status_print(Status::Cache, "Init Lifted Cache");
        let mut replacements: HashMap<(u16, Vec<u16>), Vec<Replacement>> = HashMap::new();

        for meta_action in used_meta_actions.into_iter() {
            let meta_index = meta_action.0.to_owned();
            let parameters = meta_action.1.to_owned();

            let action_replacements =
                generate_replacements(instance, &cache_data, &meta_index, &parameters);

            if let Some(action_replacements) = action_replacements {
                replacements.insert(meta_action, action_replacements);
            }
        }

        Self { replacements }
    }
}
impl Cache for LiftedCache {
    fn get_replacement(
        &self,
        instance: &Instance,
        meta_term: &Term,
        init: &State,
        goal: &State,
    ) -> Option<SASPlan> {
        let desired = init.diff(goal);
        let meta_index = World::global().get_meta_index(&meta_term.name);
        let meta_parameters = World::global().get_object_indexes(&meta_term.parameters);
        let replacement_candidates = &self.replacements.get(&(meta_index, meta_parameters))?;
        for replacement in replacement_candidates.iter() {
            let action = &replacement.action;
            for (operator, permutation) in
                generate_operators_by_candidates(action, replacement.candidates.to_owned())
            {
                if desired.iter().any(|(i, v)| match v {
                        true => !operator.eff_pos.contains(&i),
                        false => !operator.eff_neg.contains(&i),
                    } || !init.is_legal(&operator)) {
                        continue;
                    }
                return Some(generate_plan(
                    instance,
                    &replacement.action,
                    &replacement.plan,
                    &permutation,
                ));
            }
        }
        None
    }
}
