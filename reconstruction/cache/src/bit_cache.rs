use std::{collections::HashMap, ops::BitAnd, path::PathBuf};

use shared::time::run_time;
use spingus::{domain::action::Action, sas_plan::SASPlan, term::Term};
use state::{
    bit_expression::BitExp,
    instance::{
        operator::{generate_operators, Operator},
        Instance,
    },
    state::State,
};

use crate::{read_cache_input, Cache};

#[derive(Debug)]
pub struct BitCache {
    lifted_macros: Vec<(Action, SASPlan)>,
    entries: Vec<(Operator, Vec<usize>)>,
    effect_map: HashMap<BitExp, Vec<usize>>,
    entry_macro: Vec<usize>,
}
impl Cache for BitCache {
    fn init(instance: &Instance, path: &PathBuf) -> Self {
        println!("{} Reading cache data...", run_time());
        let data = read_cache_input(path).unwrap();
        println!("{} Init bitcache...", run_time());
        let mut lifted_macros: Vec<(Action, SASPlan)> = Vec::new();
        let mut entries: Vec<(Operator, Vec<usize>)> = Vec::new();
        let mut effect_map: HashMap<BitExp, Vec<usize>> = HashMap::new();
        let mut entry_macro: Vec<usize> = Vec::new();
        for (action, plan) in data
            .iter()
            .flat_map(|(_, a)| a.to_owned())
            .collect::<Vec<(Action, SASPlan)>>()
        {
            println!("{} Generating cache for {}", run_time(), action.name);
            let action_index = lifted_macros.len();
            let operators = generate_operators(&instance, &action);
            for (operator, permutation) in operators {
                let entry_index = entries.len();
                entry_macro.push(action_index);
                let mut c_effect = operator.eff_neg.to_owned();
                c_effect.append(&mut operator.eff_pos.to_owned());
                match effect_map.get_mut(&c_effect) {
                    Some(e) => {
                        e.push(entry_index);
                    }
                    None => {
                        effect_map.insert(c_effect.to_owned(), vec![entry_index]);
                    }
                };
                entries.push((operator, permutation));
            }
            lifted_macros.push((action, plan));
        }
        let c = BitCache {
            lifted_macros,
            entries,
            effect_map,
            entry_macro,
        };
        println!("Generated cache with {} entries", c.entries.len());
        println!("Number of effects {}", c.effect_map.len());
        c
    }

    fn get_replacement(&self, instance: &Instance, init: &State, goal: &State) -> Option<SASPlan> {
        let mut desired = init.get().bitand(!goal.get());
        desired.append(&mut goal.get().bitand(!init.get()));
        let candidates: &Vec<usize> = self.effect_map.get(&(desired as BitExp))?;
        let index: &usize = candidates
            .iter()
            .find(|i| init.is_legal(&self.entries[**i].0))?;
        let (_, parameters) = &self.entries[*index];
        let macro_index = self.entry_macro[*index];
        let (lifted_macro, plan) = self.lifted_macros.get(macro_index)?;
        let actions: Vec<&str> = lifted_macro.name.split('#').collect();
        let replacements: Vec<&Action> = actions
            .iter()
            .map(|n| {
                instance
                    .domain
                    .actions
                    .iter()
                    .find(|a| a.name == **n)
                    .unwrap()
            })
            .collect();
        let mut replacement: Vec<Term> = Vec::new();
        for (action, step) in replacements.iter().zip(plan.iter()) {
            let name = action.name.to_owned();
            let objects: Vec<usize> = step
                .parameters
                .iter()
                .map(|i| parameters[i.parse::<usize>().unwrap()])
                .collect();
            let parameters: Vec<String> = objects
                .iter()
                .map(|i| instance.problem.objects[*i].name.to_owned())
                .collect();
            replacement.push(Term { name, parameters })
        }
        Some(replacement)
    }
}
