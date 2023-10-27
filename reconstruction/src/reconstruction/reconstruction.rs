use rand::{distributions::Alphanumeric, Rng};
use spingus::{domain::Domain, sas_plan::SASPlan, term::Term};
use std::{
    fs,
    path::{Path, PathBuf},
};

use crate::{
    cache::Cache,
    reconstruction::{problem_writing::write_problem, stiching::stich},
    state::{
        instance::{
            operator::{generate_operator_string, Operator},
            Instance,
        },
        state::State,
    },
    tools::{generate_progressbar, random_name, time::run_time},
};

use super::downward_wrapper::Downward;

fn generate_operators(
    instance: &Instance,
    meta_domain: &Domain,
    _downward: &Downward,
    plan: &SASPlan,
) -> (Vec<usize>, Vec<Operator>) {
    let mut meta_actions: Vec<usize> = Vec::new();
    let mut operators: Vec<Operator> = Vec::new();
    for (i, step) in plan.iter().enumerate() {
        if step.name.contains('$') {
            meta_actions.push(i);
            operators.push(generate_operator_string(
                meta_domain,
                &instance.facts,
                &step.name,
                &step.parameters,
            ));
        } else {
            operators.push(generate_operator_string(
                &instance.domain,
                &instance.facts,
                &step.name,
                &step.parameters,
            ));
        }
    }
    (meta_actions, operators)
}

pub fn reconstruct(
    instance: &Instance,
    meta_domain: &Domain,
    domain_path: &PathBuf,
    downward: &Downward,
    cache: &Option<Box<dyn Cache>>,
    plan: SASPlan,
) -> SASPlan {
    let mut replacements: Vec<SASPlan> = Vec::new();
    let mut state = State::new(&instance.domain, &instance.problem, &instance.facts);
    let (meta_actions, operators) = generate_operators(&instance, meta_domain, downward, &plan);

    let progress_bar = generate_progressbar(meta_actions.len());
    for (i, operator) in operators.iter().enumerate() {
        if !meta_actions.contains(&i) {
            state.apply(operator);
            continue;
        }
        progress_bar.inc(1);
        let init = state.clone();
        state.apply(operator);
        if let Some(cache) = cache {
            progress_bar.set_message("Checking cache");
            if let Some(replacement) = cache.get_replacement(instance, &plan[i], &init, &state) {
                replacements.push(replacement);
                continue;
            }
        }
        progress_bar.set_message("Using fast downward");
        let problem_file = Path::new(&random_name()).to_path_buf();
        write_problem(instance, &init, &state, &problem_file);
        let plan = downward.solve(domain_path, &problem_file);
        let _ = fs::remove_file(&problem_file);
        if let Ok(plan) = plan {
            replacements.push(plan);
        } else {
            panic!(
                "Had error trying to replace meta action at index {}. Error: {}",
                i,
                plan.unwrap_err()
            )
        }
    }
    progress_bar.finish_and_clear();
    stich(&plan, meta_actions.into_iter().zip(replacements).collect())
}
