use std::{collections::HashMap, ops::BitAnd, path::PathBuf};

use spingus::{
    domain::{action::Action, Domain},
    problem::Problem,
    sas_plan::SASPlan,
};
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
    entry_macro: HashMap<usize, usize>,
}
impl Cache for BitCache {
    fn init(instance: &Instance, path: &PathBuf) -> Self {
        let data = read_cache_input(path).unwrap();
        let mut lifted_macros: Vec<(Action, SASPlan)> = Vec::new();
        let mut entries: Vec<(Operator, Vec<usize>)> = Vec::new();
        let mut effect_map: HashMap<BitExp, Vec<usize>> = HashMap::new();
        let mut entry_macro: HashMap<usize, usize> = HashMap::new();
        for (action, plan) in data
            .iter()
            .flat_map(|(_, a)| *a)
            .collect::<Vec<(Action, SASPlan)>>()
        {
            let action_index = lifted_macros.len();
            let operators = generate_operators(
                &instance.domain,
                &instance.problem,
                &instance.facts,
                &action,
            );
            for (operator, permutation) in operators {
                let entry_index = entries.len();
                entry_macro.insert(entry_index, action_index);
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
        todo!()
    }
}
impl BitCache {
    pub fn get(&self, state: &State, goal: &State) -> Option<usize> {
        let mut desired = state.get().bitand(!goal.get());
        desired.append(&mut goal.get().bitand(!state.get()));
        let candidates: Option<&Vec<usize>> = self.effect_map.get(&(desired as BitExp));
        match candidates {
            Some(candidates) => candidates
                .iter()
                .find(|i| state.is_legal(&self.entries[**i].0))
                .copied(),
            None => return None,
        }
    }

    pub fn get_replacement(&self, domain: &Domain, problem: &Problem, index: usize) -> SASPlan {
        let macro_index = self.entry_macro[&index];
        let lifted_macro = self.lifted_macros.get(macro_index).unwrap();
        let actions: Vec<&str> = lifted_macro.name.split("#").collect();
        let actions: Vec<&Action> = actions
            .iter()
            .map(|n| domain.actions.iter().find(|a| a.name == **n).unwrap())
            .collect();
        let permutation: Vec<usize> = self.entries[index].1.to_owned();
        println!("{:?}", permutation);
        vec![]
    }
}
