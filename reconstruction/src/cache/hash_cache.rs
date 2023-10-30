use std::{collections::HashMap, ops::BitAnd};

use bitvec::vec::BitVec;
use itertools::Itertools;
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

#[derive(Debug)]
pub struct HashCache {
    lifted_macros: Vec<(Action, SASPlan)>,
    entries: Vec<(Operator, Vec<usize>)>,
    effect_map: HashMap<BitVec, Vec<usize>>,
    entry_macro: Vec<usize>,
}

impl HashCache {
    pub fn new(
        instance: &Instance,
        cache_data: HashMap<String, Vec<(spingus::domain::action::Action, SASPlan)>>,
    ) -> Self {
        todo!()
        //println!("{} Init hash-cache...", run_time());
        //let mut lifted_macros: Vec<(Action, SASPlan)> = Vec::new();
        //let mut entries: Vec<(Operator, Vec<usize>)> = Vec::new();
        //let mut effect_map: HashMap<BitVec, Vec<usize>> = HashMap::new();
        //let mut entry_macro: Vec<usize> = Vec::new();
        //for (action, plan) in cache_data
        //    .into_iter()
        //    .flat_map(|(_, d)| {
        //        d.into_iter()
        //            .map(|(a, p)| (instance.convert_action(a), p))
        //            .collect_vec()
        //    })
        //    .collect::<Vec<(Action, SASPlan)>>()
        //{
        //    println!("{} Generating cache for {}", run_time(), action.name);
        //    let action_index = lifted_macros.len();
        //    let operators = generate_operators(&instance, &action);
        //    let mut count = 0;
        //    for (operator, permutation) in operators {
        //        let entry_index = entries.len();
        //        entry_macro.push(action_index);
        //        let mut c_effect = operator.eff_neg.to_owned();
        //        c_effect.append(&mut operator.eff_pos.to_owned());
        //        match effect_map.get_mut(&c_effect) {
        //            Some(e) => {
        //                e.push(entry_index);
        //            }
        //            None => {
        //                effect_map.insert(c_effect.to_owned(), vec![entry_index]);
        //            }
        //        };
        //        entries.push((operator, permutation));
        //        count += 1;
        //    }
        //    println!("entries: {}", count);
        //    lifted_macros.push((action, plan));
        //}
        //let c = HashCache {
        //    lifted_macros,
        //    entries,
        //    effect_map,
        //    entry_macro,
        //};
        //println!("Generated cache with {} entries", c.entries.len());
        //println!("Number of effects {}", c.effect_map.len());
        //c
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
        todo!();
        //let mut desired = init.get().bitand(!goal.get());
        //desired.append(&mut goal.get().bitand(!init.get()));
        //let candidates: &Vec<usize> = self.effect_map.get(&desired)?;
        //let index: &usize = candidates
        //    .iter()
        //    .find(|i| init.is_legal(&self.entries[**i].0))?;
        //let (_, parameters) = &self.entries[*index];
        //let macro_index = self.entry_macro[*index];
        //let (lifted_macro, plan) = self.lifted_macros.get(macro_index)?;
        //let actions: Vec<&str> = lifted_macro.name.split('#').collect();
        //let replacements: Vec<&Action> = actions.iter().map(|n| instance.get_action(n)).collect();
        //let mut replacement: Vec<Term> = Vec::new();
        //for (action, step) in replacements.iter().zip(plan.iter()) {
        //    let name = action.name.to_owned();
        //    let objects: Vec<usize> = step
        //        .parameters
        //        .iter()
        //        .map(|i| parameters[i.parse::<usize>().unwrap()])
        //        .collect();
        //    let parameters: Vec<String> = objects
        //        .iter()
        //        .map(|i| instance.problem.objects[*i].name.to_owned())
        //        .collect();
        //    replacement.push(Term { name, parameters })
        //}
        //Some(replacement)
    }
}
