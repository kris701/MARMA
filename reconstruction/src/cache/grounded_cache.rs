use std::{collections::HashMap, ops::BitAnd};

use spingus::{sas_plan::SASPlan, term::Term};

use crate::{
    state::{
        instance::{
            actions::Action,
            operator::{generate_operators, Operator},
            Instance,
        },
        state::State,
    },
    tools::time::run_time,
};

use super::Cache;

struct Entry {
    operator: Operator,
    _parameters: Vec<usize>,
}

struct MacroEntry {
    _action: Action,
    plan: SASPlan,
    entries: Vec<Entry>,
}

struct MetaEntry {
    macro_replacements: Vec<MacroEntry>,
}

pub struct GroundedCache {
    meta_map: HashMap<String, MetaEntry>,
}

impl GroundedCache {
    pub fn new(
        instance: &Instance,
        cache_data: HashMap<String, Vec<(spingus::domain::action::Action, SASPlan)>>,
    ) -> Self {
        println!("{} Init grounded-cache...", run_time());
        let mut meta_map: HashMap<String, MetaEntry> = HashMap::new();
        for (meta_name, macros) in cache_data {
            let mut macro_replacements: Vec<MacroEntry> = Vec::new();
            for (action, plan) in macros {
                println!("{} Generating operators for {}...", run_time(), action.name);
                let action = instance.convert_action(action);
                let entries = generate_operators(instance, &action)
                    .map(|(operator, parameters)| Entry {
                        operator,
                        _parameters: parameters,
                    })
                    .collect();
                macro_replacements.push(MacroEntry {
                    _action: action,
                    plan,
                    entries,
                });
            }
            meta_map.insert(meta_name, MetaEntry { macro_replacements });
        }
        GroundedCache { meta_map }
    }
}

impl Cache for GroundedCache {
    // TODO: Actually replace parameters
    fn get_replacement(
        &self,
        _instance: &Instance,
        meta_term: &Term,
        init: &State,
        goal: &State,
    ) -> Option<SASPlan> {
        let desired_pos = goal.get().bitand(!init.get());
        let desired_neg = init.get().bitand(!goal.get());
        for macro_entry in self.meta_map[&meta_term.name].macro_replacements.iter() {
            for entry in macro_entry.entries.iter() {
                let operator = &entry.operator;
                if operator.eff_pos != desired_pos || operator.eff_neg != desired_neg {
                    continue;
                }
                if init.is_legal(operator) {
                    return Some(macro_entry.plan.to_owned());
                }
            }
        }
        None
    }
}
