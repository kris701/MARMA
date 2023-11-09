use std::collections::HashMap;

use spingus::{sas_plan::SASPlan, term::Term};

use crate::{
    instance::{
        actions::Action,
        operator::extract_from_action,
        permute::{get_candidates, permute_unary},
        Instance,
    },
    state::State,
    tools::{status_print, Status},
};

use super::{cache_data::CacheData, generate_plan, Cache};

#[derive(Debug)]
struct Replacement {
    action: Action,
    plan: SASPlan,
    candidates: Vec<Vec<u32>>,
}

#[derive(Debug)]
pub struct LiftedCache {
    replacements: HashMap<String, Vec<Replacement>>,
}

impl LiftedCache {
    pub fn new(instance: &Instance, cache_data: CacheData) -> Self {
        status_print(Status::Cache, "Init Lifted Cache");
        let mut replacements: HashMap<String, Vec<Replacement>> = HashMap::new();

        for (meta_action, m_replacements) in cache_data.into_iter() {
            let mut meta_replacements: Vec<Replacement> = Vec::new();
            for (action, plan) in m_replacements.into_iter() {
                let action = instance.convert_action(action);
                let candidates = get_candidates(
                    &instance.types,
                    &instance.objects,
                    &action.parameters.parameter_types,
                );

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
            let candidates = replacement.candidates.clone();
            let permutations = permute_unary(candidates);
            for permutation in permutations.into_iter() {
                let operator = extract_from_action(&instance, &permutation, action).unwrap();
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
