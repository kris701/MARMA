use crate::{
    cache::Cache,
    plan::{Plan, Step},
    reconstruction::{
        downward_wrapper::{Downward, DownwardError},
        stiching::stich,
    },
    state::State,
    tools::{random_file_name, status_print, Status},
    world::World,
};
use spingus::{sas_plan::SASPlan, term::Term};
use std::{fs, path::PathBuf, time::Instant};

#[derive(Default)]
struct Metrics {
    cache_time: f64,
    downward_time: f64,
    found_in_cache: usize,
    added_to_cache: usize,
}

impl Metrics {
    const fn new() -> Self {
        Self {
            cache_time: 0.0,
            downward_time: 0.0,
            found_in_cache: 0,
            added_to_cache: 0,
        }
    }

    fn print(&self) {
        println!("found_in_cache={}", self.found_in_cache);
        println!("added_to_cache={}", self.added_to_cache);
        println!("cache_lookup_time={:.4?}", self.cache_time);
        println!("planner_time={:.4?}", self.downward_time);
    }
}

static mut METRICS: Metrics = Metrics::new();

fn check_cache(
    cache: Option<&Box<dyn Cache>>,
    step: &Step,
    init: &State,
    goal: &State,
) -> Option<Plan> {
    let cache = cache?;
    let cache_start = Instant::now();
    let replacement = cache.get_replacement(step, init, goal);
    unsafe {
        METRICS.cache_time += cache_start.elapsed().as_secs_f64();
        if replacement.is_some() {
            METRICS.found_in_cache += 1;
        }
    }
    replacement
}

fn generate_replacement(
    domain_path: &PathBuf,
    init: &State,
    goal: &State,
) -> Result<Plan, DownwardError> {
    let downward = Downward::global();
    let problem_path = PathBuf::from(format!("{}", random_file_name(&downward.temp_dir)));
    let problem = World::global().export_problem(&init, &goal);
    fs::write(&problem_path, problem).unwrap();
    let downward_start = Instant::now();
    let result = downward.solve(&domain_path, &problem_path);
    unsafe {
        METRICS.downward_time += downward_start.elapsed().as_secs_f64();
    }
    let _ = fs::remove_file(&problem_path);
    result
}

fn find_replacement(
    cache: &mut Option<Box<dyn Cache>>,
    iterative_cache: bool,
    domain_path: &PathBuf,
    step: &Step,
    init: &State,
    goal: &State,
) -> Result<Plan, DownwardError> {
    if let Some(plan) = check_cache(cache.as_ref(), step, init, goal) {
        return Ok(plan);
    }
    let replacement = generate_replacement(domain_path, init, goal)?;
    if let Some(cache) = cache {
        if iterative_cache {
            unsafe {
                METRICS.added_to_cache += 1;
            }
            cache.add_entry(step, &replacement);
        }
    }
    Ok(replacement)
}

pub fn reconstruct(
    cache: &mut Option<Box<dyn Cache>>,
    iterative_cache: bool,
    domain_path: &PathBuf,
    plan: &Plan,
) -> Result<Plan, usize> {
    let mut replacements: Vec<(usize, Plan)> = Vec::new();
    let mut state = State::from_init();
    status_print(Status::Reconstruction, "Generating replacements");
    for (i, step) in plan.iter().enumerate() {
        let action = World::global().actions.get(step.action);
        let init = state.clone();
        state.apply(action, &step.args);
        if !World::global().actions.is_meta(step.action) {
            continue;
        }
        let replacement =
            find_replacement(cache, iterative_cache, domain_path, &step, &init, &state);
        match replacement {
            Ok(plan) => replacements.push((i, plan)),
            Err(err) => match err {
                DownwardError::Unsolvable => return Err(step.action),
                _ => panic!("{}", err.to_string()),
            },
        };
    }
    unsafe {
        METRICS.print();
    }
    Ok(stich(&plan, replacements))
}
