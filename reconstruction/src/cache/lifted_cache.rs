use std::{collections::HashMap, ops::BitAnd};

use spingus::{sas_plan::SASPlan, term::Term};

use crate::{
    state::{
        instance::{actions::Action, operator::generate_operators, Instance},
        state::State,
    },
    tools::time::run_time,
};

use super::Cache;

struct MetaReplacements {
    macros: Vec<Action>,
    plans: Vec<SASPlan>,
}

pub struct LiftedCache {
    replacements: HashMap<String, MetaReplacements>,
}

impl LiftedCache {
    pub fn new(
        _instance: &Instance,
        cache_data: HashMap<String, Vec<(spingus::domain::action::Action, SASPlan)>>,
    ) -> Self {
        println!("{} Init lifted-cache...", run_time());
        let mut replacements: HashMap<String, MetaReplacements> = HashMap::new();
        for (meta_action, macros) in cache_data {
            let mut actions: Vec<Action> = vec![];
            let mut plans: Vec<SASPlan> = vec![];
            for (macro_action, plan) in macros {
                actions.push(_instance.convert_action(macro_action));
                plans.push(plan);
            }
            replacements.insert(
                meta_action,
                MetaReplacements {
                    macros: actions,
                    plans,
                },
            );
        }
        LiftedCache { replacements }
    }
}

impl Cache for LiftedCache {
    // TODO: Actually replace parameters
    fn get_replacement(
        &self,
        instance: &Instance,
        meta_term: &Term,
        init: &State,
        goal: &State,
    ) -> Option<SASPlan> {
        let desired_pos = goal.get().bitand(!init.get());
        let desired_neg = init.get().bitand(!goal.get());
        let replacements = &self.replacements.get(&meta_term.name)?;
        for (i, action) in replacements.macros.iter().enumerate() {
            for operator in generate_operators(instance, &action).map(|(o, ..)| o) {
                if operator.eff_pos != desired_pos || operator.eff_neg != desired_neg {
                    continue;
                }
                if init.is_legal(&operator) {
                    return Some(replacements.plans[i].to_owned());
                }
            }
        }
        None
    }
}
