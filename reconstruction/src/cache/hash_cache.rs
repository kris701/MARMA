use std::collections::HashMap;

use itertools::Itertools;
use spingus::{sas_plan::SASPlan, term::Term};

use crate::{
    instance::{
        actions::Action,
        operator::{generate_operators, Operator},
        Instance,
    },
    state::State,
    tools::{status_print, Status},
};

use super::{cache_data::CacheData, generate_plan, Cache};

#[derive(Debug)]
pub struct HashCache {
    lifted_macros: Vec<(Action, SASPlan)>,
    entries: Vec<(Operator, Vec<u16>)>,
    effect_map: HashMap<Vec<(u64, bool)>, Vec<usize>>,
    entry_macro: Vec<u16>,
}

impl HashCache {
    pub fn new(instance: &Instance, cache_data: CacheData) -> Self {
        status_print(Status::Cache, "Init Hash Cache");
        let mut lifted_macros: Vec<(Action, SASPlan)> = Vec::new();
        let mut entries: Vec<(Operator, Vec<u16>)> = Vec::new();
        let mut effect_map: HashMap<Vec<(u64, bool)>, Vec<usize>> = HashMap::new();
        let mut entry_macro: Vec<u16> = Vec::new();
        for (action, plan) in cache_data
            .into_iter()
            .flat_map(|(_, d)| {
                d.into_iter()
                    .map(|(a, p)| (instance.convert_action(a), p))
                    .collect_vec()
            })
            .collect::<Vec<(Action, SASPlan)>>()
        {
            status_print(Status::Cache, &format!("Grounding {}", action.name));
            let action_index = lifted_macros.len() as u16;
            let operators = generate_operators(&instance, &action);
            let mut count = 0;
            for (operator, permutation) in operators {
                let entry_index = entries.len();
                entry_macro.push(action_index);
                let c_effect = operator.get_effect();
                match effect_map.get_mut(&c_effect) {
                    Some(e) => {
                        e.push(entry_index);
                    }
                    None => {
                        effect_map.insert(c_effect.to_owned(), vec![entry_index]);
                    }
                };
                entries.push((operator, permutation));
                count += 1;
            }
            println!("Entries: {}", count);
            lifted_macros.push((action, plan));
        }
        let c = HashCache {
            lifted_macros,
            entries,
            effect_map,
            entry_macro,
        };
        println!("Generated cache with {} entries", c.entries.len());
        println!("Number of effects {}", c.effect_map.len());
        c
    }
}
impl Cache for HashCache {
    fn get_replacement(
        &self,
        instance: &Instance,
        _meta_term: &Term,
        init: &State,
        goal: &State,
    ) -> Option<SASPlan> {
        let desired = init.diff(goal);
        let candidates: &Vec<usize> = self.effect_map.get(&desired)?;
        let index: &usize = candidates
            .iter()
            .find(|i| init.is_legal(&self.entries[**i].0))?;
        let (_, parameters) = &self.entries[*index as usize];
        let macro_index = self.entry_macro[*index as usize];
        let (lifted_macro, plan) = self.lifted_macros.get(macro_index as usize)?;
        Some(generate_plan(instance, lifted_macro, plan, parameters))
    }
}
