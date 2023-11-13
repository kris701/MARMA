use spingus::sas_plan::SASPlan;
use std::{fs, path::PathBuf};

use crate::{
    cache::Cache,
    instance::{
        operator::{generate_operator_string, Operator},
        Instance,
    },
    reconstruction::{problem_writing::write_problem, stiching::stich},
    state::State,
    tools::{generate_progressbar, random_file_name, statbar, status_print, Status},
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
    let mut found_in_cache: Vec<usize> = Vec::new();
    let mut state = State::from_init(instance);
    let (meta_actions, operators) = generate_operators(&instance, downward, &plan);

    let progress_bar = generate_progressbar(meta_actions.len());
    status_print(Status::Reconstruction, "Generating replacements");
    for (i, operator) in operators.iter().enumerate() {
        progress_bar.set_message(statbar());
        if !meta_actions.contains(&i) {
            state.apply(operator);
            continue;
        }
        progress_bar.inc(1);
        let init = state.clone();
        state.apply(operator);
        if let Some(cache) = cache {
            if let Some(replacement) = cache.get_replacement(instance, &plan[i], &init, &state) {
                replacements.push(replacement);
                found_in_cache.push(i);
                continue;
            }
        }
        let problem_file = PathBuf::from(random_file_name(&downward.temp_dir));
        debug_assert_ne!(init, state);
        write_problem(&init, &state, &problem_file);
        let plan = downward.solve(domain_path, &problem_file);
        if let Ok(plan) = plan {
            debug_assert!(!plan.is_empty());
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
    status_print(
        Status::Reconstruction,
        &format!(
            "Found {} of {} in cache ({})",
            found_in_cache.len(),
            meta_actions.len(),
            found_in_cache.len() as f64 / meta_actions.len() as f64
        ),
    );
    println!("found_in_cache {}", found_in_cache.len());
    stich(&plan, meta_actions.into_iter().zip(replacements).collect())
}
