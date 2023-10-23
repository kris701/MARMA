use std::{
    collections::HashMap,
    fs,
    io::{self},
    ops::BitAnd,
    path::PathBuf,
};

use spingus::{
    domain::{
        action::{parse_action, Action},
        Domain,
    },
    problem::Problem,
    sas_plan::SASPlan,
};
use state::{
    bit_expression::BitExp,
    instance::{
        facts::Facts,
        operator::{generate_operators, Operator},
    },
    state::State,
};

#[derive(Debug)]
pub struct Cache {
    lifted_macros: Vec<Action>,
    entries: Vec<Operator>,
    effect_map: HashMap<BitExp, Vec<usize>>,
    entry_macro: HashMap<usize, usize>,
}

impl Cache {
    pub fn get(&self, state: &State, goal: &State) -> Option<usize> {
        let mut desired = state.get().bitand(!goal.get());
        desired.append(&mut goal.get().bitand(!state.get()));
        let candidates: Option<&Vec<usize>> = self.effect_map.get(&(desired as BitExp));
        match candidates {
            Some(candidates) => candidates
                .iter()
                .find(|i| state.is_legal(&self.entries[**i]))
                .copied(),
            None => return None,
        }
    }

    pub fn get_replacement(&self, index: usize) -> SASPlan {
        todo!()
    }
}

pub fn generate_cache(
    domain: &Domain,
    problem: &Problem,
    facts: &Facts,
    macros: Vec<Action>,
) -> Cache {
    let mut lifted_macros: Vec<Action> = Vec::new();
    let mut entries: Vec<Operator> = Vec::new();
    let mut effect_map: HashMap<BitExp, Vec<usize>> = HashMap::new();
    let mut entry_macro: HashMap<usize, usize> = HashMap::new();
    for action in &macros {
        let action_index = lifted_macros.len();
        lifted_macros.push(action.to_owned());
        let operators = generate_operators(domain, problem, facts, &action);
        for operator in operators {
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
            entries.push(operator);
        }
    }
    let c = Cache {
        lifted_macros,
        entries,
        effect_map,
        entry_macro,
    };
    println!("Generated cache with {} entries", c.entries.len());
    println!("Number of effects {}", c.effect_map.len());
    c
}

pub fn read_cache_input(path: &PathBuf) -> io::Result<Vec<Action>> {
    let files: Vec<PathBuf> = fs::read_dir(path)?.map(|p| p.unwrap().path()).collect();
    let instances: Vec<Action> = files
        .iter()
        .map(|path| {
            let action_content: String = fs::read_to_string(path).unwrap();
            let action = parse_action(&action_content).unwrap().1;
            action
        })
        .collect();
    Ok(instances)
}
