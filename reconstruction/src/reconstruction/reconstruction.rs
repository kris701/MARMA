use super::downward_wrapper::Downward;
use crate::{
    cache::Cache,
    reconstruction::{problem_writing::write_problem, stiching::stich},
    state::State,
    tools::{random_file_name, status_print, Status},
    world::World,
};
use spingus::sas_plan::SASPlan;
use std::{fs, path::PathBuf, time::Instant};

pub fn reconstruct(
    domain_path: &PathBuf,
    downward: &Downward,
    cache: &Option<Box<dyn Cache>>,
    plan: SASPlan,
) -> SASPlan {
    let mut cache_time: f64 = 0.0;
    let mut fd_time: f64 = 0.0;
    let mut replacements: Vec<(usize, SASPlan)> = Vec::new();
    let mut found_in_cache: usize = 0;
    let mut state = State::from_init();

    status_print(Status::Reconstruction, "Generating replacements");
    for (i, step) in plan.iter().enumerate() {
        let action = World::global().get_action(&step.name);
        let arguments = World::global().objects.indexes(&step.parameters);
        if !World::global().is_meta_action(&step.name) {
            state.apply(action, &arguments);
            continue;
        }
        let init = state.clone();
        state.apply(action, &arguments);
        if let Some(cache) = cache {
            let cache_lookup_begin = Instant::now();
            let replacement = cache.get_replacement(&plan[i], &init, &state);
            cache_time += cache_lookup_begin.elapsed().as_secs_f64();
            if let Some(replacement) = replacement {
                replacements.push((i, replacement));
                found_in_cache += 1;
                continue;
            }
        }
        let problem_file = PathBuf::from(random_file_name(&downward.temp_dir));
        assert_ne!(init, state);
        write_problem(&init, &state, &problem_file);
        let fd_plan_time = Instant::now();
        let plan = downward.solve(domain_path, &problem_file);
        fd_time += fd_plan_time.elapsed().as_secs_f64();
        if let Ok(plan) = plan {
            debug_assert!(!plan.is_empty());
            let _ = fs::remove_file(&problem_file);
            replacements.push((i, plan));
        } else {
            panic!(
                "Had error trying to replace meta action at index {}. Error: {}",
                i,
                plan.unwrap_err()
            )
        }
    }
    println!("found_in_cache={}", found_in_cache);
    println!("cache_lookup_time={:.4?}", cache_time);
    println!("planner_time={:.4?}", fd_time);
    stich(&plan, replacements)
}
