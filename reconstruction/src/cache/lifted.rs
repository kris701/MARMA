use std::collections::HashMap;

use spingus::{sas_plan::SASPlan, term::Term};

use crate::{
    instance::{actions::Action, operator::generate_operators, Instance},
    state::State,
    tools::{status_print, Status},
};

use super::{cache_data::CacheData, generate_plan, Cache};

#[derive(Debug)]
struct Replacement {
    action: Action,
    plan: SASPlan,
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

                meta_replacements.push(Replacement { action, plan })
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
            for (operator, permutation) in generate_operators(instance, action) {
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
