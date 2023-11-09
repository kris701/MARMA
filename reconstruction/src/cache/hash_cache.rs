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
    world::World,
};

use super::{cache_data::CacheData, Cache};

#[derive(Debug)]
pub struct HashCache {
    lifted_macros: Vec<(Action, SASPlan)>,
    entries: Vec<(Operator, Vec<u32>)>,
    effect_map: HashMap<Vec<(u32, bool)>, Vec<usize>>,
    entry_macro: Vec<u32>,
}

impl HashCache {
    pub fn new(instance: &Instance, cache_data: CacheData) -> Self {
        status_print(Status::Cache, "Init Hash Cache");
        let mut lifted_macros: Vec<(Action, SASPlan)> = Vec::new();
        let mut entries: Vec<(Operator, Vec<u32>)> = Vec::new();
        let mut effect_map: HashMap<Vec<(u32, bool)>, Vec<usize>> = HashMap::new();
        let mut entry_macro: Vec<u32> = Vec::new();
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
            let action_index = lifted_macros.len() as u32;
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
        let lifted_parameters = &lifted_macro.parameters.parameter_names;
        let actions: Vec<String> = plan.iter().map(|t| t.name.to_owned()).collect();
        let replacements: Vec<&Action> = actions.iter().map(|n| instance.get_action(n)).collect();
        let mut replacement: Vec<Term> = Vec::new();
        for (action, step) in replacements.iter().zip(plan.iter()) {
            let name = action.name.to_owned();
            let parameters: Vec<u32> = step
                .parameters
                .iter()
                .map(|n| {
                    let index = lifted_parameters.iter().position(|p| p == n).unwrap();
                    parameters[index]
                })
                .collect();
            let parameters = World::global().get_object_names_cloned(&parameters);
            replacement.push(Term { name, parameters })
        }
        Some(replacement)
    }
}
