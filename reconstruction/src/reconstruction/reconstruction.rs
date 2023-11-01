use spingus::sas_plan::SASPlan;
use std::{fs, path::PathBuf};

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
    tools::{generate_progressbar, random_file_name},
};

use super::downward_wrapper::Downward;

fn generate_operators(
    instance: &Instance,
    _downward: &Downward,
    plan: &SASPlan,
) -> (Vec<usize>, Vec<Operator>) {
    let mut meta_actions: Vec<usize> = Vec::new();
    let mut operators: Vec<Operator> = Vec::new();
    for (i, step) in plan.iter().enumerate() {
        if step.name.contains('$') {
            meta_actions.push(i);
        }
        operators.push(generate_operator_string(
            instance,
            &step.name,
            &step.parameters,
        ));
    }
    (meta_actions, operators)
}

pub fn reconstruct(
    instance: &Instance,
    domain_path: &PathBuf,
    downward: &Downward,
    cache: &Option<Box<dyn Cache>>,
    plan: SASPlan,
) -> SASPlan {
    let mut replacements: Vec<SASPlan> = Vec::new();
    let mut state = State::new(&instance);
    let (meta_actions, operators) = generate_operators(&instance, downward, &plan);

    let mut found_in_cache: usize = 0;
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
                found_in_cache += 1;
                continue;
            }
        }
        progress_bar.set_message("Using fast downward");
        let problem_file = PathBuf::from(random_file_name(&downward.temp_dir));
        assert_ne!(init, state);
        write_problem(instance, &init, &state, &problem_file);
        let plan = downward.solve(domain_path, &problem_file);
        if let Ok(plan) = plan {
            assert!(!plan.is_empty());
            let _ = fs::remove_file(&problem_file);
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
    println!(
        "found {} out of {} in cache ({})",
        found_in_cache,
        meta_actions.len(),
        found_in_cache as f64 / meta_actions.len() as f64
    );
    stich(&plan, meta_actions.into_iter().zip(replacements).collect())
}
