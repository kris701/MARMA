use std::{collections::HashMap, path::PathBuf};

use spingus::{
    domain::{action::Action, Domain},
    problem::Problem,
    sas_plan::SASPlan,
};
use state::{
    bit_expression::BitExp,
    instance::{
        facts::Facts,
        operator::{generate_operators, Operator},
    },
};

#[derive(Debug)]
pub struct Cache {
    entries: Vec<Operator>,
    plans: Vec<SASPlan>,
    entry_plan: HashMap<usize, usize>,
    effect_map: HashMap<BitExp, Vec<usize>>,
}

pub fn generate_cache(
    domain: &Domain,
    problem: &Problem,
    facts: &Facts,
    macros: Vec<(Action, SASPlan)>,
) -> Cache {
    let mut entries: Vec<Operator> = Vec::new();
    let mut plans: Vec<SASPlan> = Vec::new();
    let mut entry_plan: HashMap<usize, usize> = HashMap::new();
    let mut effect_map: HashMap<BitExp, Vec<usize>> = HashMap::new();
    for (action, plan) in macros {
        let plan_index = plans.len();
        plans.push(plan);
        let operators = generate_operators(domain, problem, facts, &action);
        for operator in operators {
            let entry_index = entries.len();
            let mut c_effect = operator.eff_neg.to_owned();
            c_effect.append(&mut operator.eff_pos.to_owned());
            effect_map.get_mut(&c_effect).map(|v| v.push(entry_index));
            entries.push(operator);
            entry_plan.insert(entry_index, plan_index);
        }
    }
    let c = Cache {
        entries,
        plans,
        entry_plan,
        effect_map,
    };
    c
}

pub fn generate_cache_from_files(
    domain: &Domain,
    problem: &Problem,
    facts: &Facts,
    path: &PathBuf,
) -> Cache {
    todo!();
}
