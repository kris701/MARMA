use std::collections::HashMap;

use itertools::Itertools;
use spingus::{sas_plan::SASPlan, term::Term};

use crate::{
    instance::{
        actions::Action,
        operator::{generate_operators, generate_operators_by_candidates},
        Instance,
    },
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

#[derive(Debug)]
pub struct LiftedCache {
    replacements: HashMap<String, Vec<Replacement>>,
}

impl LiftedCache {
    pub fn new(
        instance: &Instance,
        cache_data: CacheData,
        used_meta_actions: Vec<(u16, Vec<u16>)>,
    ) -> Self {
        status_print(Status::Cache, "Init Lifted Cache");
        let mut replacements: HashMap<String, Vec<Replacement>> = HashMap::new();

        for (meta_action, m_replacements) in cache_data.into_iter() {
            let meta_index = World::global().get_meta_index(&meta_action);
            let mut meta_replacements: Vec<Replacement> = Vec::new();
            for (action, plan) in m_replacements.into_iter() {
                let action = instance.convert_action(action);
                let candidates = action
                    .parameters
                    .parameter_names
                    .iter()
                    .zip(action.parameters.parameter_types.iter())
                    .map(|(name, type_id)| match name.to_uppercase().contains("O") {
                        true => World::global().get_objects_with_type(*type_id),
                        false => {
                            let parameter_index = name.parse::<usize>().unwrap();
                            used_meta_actions
                                .iter()
                                .filter_map(|(meta_action, parameters)| {
                                    match *meta_action == meta_index {
                                        true => Some(parameters[parameter_index]),
                                        false => None,
                                    }
                                })
                                .unique()
                                .collect()
                        }
                    })
                    .collect();

                meta_replacements.push(Replacement {
                    action,
                    plan,
                    candidates,
                })
            }
            replacements.insert(meta_action, meta_replacements);
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
        let replacement_candidates = &self.replacements.get(&meta_term.name)?;
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
