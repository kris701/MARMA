use std::{
    fs,
    path::{Path, PathBuf},
};

use shared::time::run_time;
use spingus::{domain::Domain, sas_plan::SASPlan};
use state::{
    instance::{
        operator::{generate_operator_string, Operator},
        Instance,
    },
    state::State,
};

use crate::{downward_wrapper::Downward, problem_writing::write_problem, stiching::stich};

fn generate_operators(
    instance: &Instance,
    meta_domain: &Domain,
    _domain_path: &PathBuf,
    _downward: &Downward,
    plan: &SASPlan,
) -> Vec<Operator> {
    plan.iter()
        .map(|step| match step.name.contains("$") {
            true => generate_operator_string(
                &meta_domain,
                &instance.facts,
                &step.name,
                &step.parameters,
            ),
            false => generate_operator_string(
                &instance.domain,
                &instance.facts,
                &step.name,
                &step.parameters,
            ),
        })
        .collect()
}

fn generate_replacement(
    instance: &Instance,
    domain_path: &PathBuf,
    downward: &Downward,
    operators: &Vec<Operator>,
    i: usize,
) -> SASPlan {
    let init = State::new(&instance.domain, &instance.problem, &instance.facts);
    println!("{} Replacing action {}...", run_time(), i);
    let init = init.apply_multiple(&operators[0..i].to_owned());
    let goal = init.apply_clone(&operators[i]);
    assert_ne!(init, goal);
    let problem_path = Path::new(".temp_problem.pddl").to_path_buf();
    write_problem(instance, &init, &goal, &problem_path);
    let solution = downward.solve(domain_path, &problem_path);
    let _ = fs::remove_file(&problem_path);
    let _ = fs::remove_file("sas_plan");
    solution
}

pub fn reconstruct(
    instance: Instance,
    meta_domain: &Domain,
    domain_path: &PathBuf,
    downward: &Downward,
    plan: SASPlan,
) -> SASPlan {
    let meta_steps: Vec<usize> = plan
        .iter()
        .enumerate()
        .filter_map(|(i, step)| match step.name.contains("$") {
            true => Some(i),
            false => None,
        })
        .collect();
    let operators = generate_operators(&instance, meta_domain, domain_path, downward, &plan);
    let replacements: Vec<SASPlan> = meta_steps
        .iter()
        .map(|i| generate_replacement(&instance, domain_path, downward, &operators, *i))
        .collect();
    stich(plan, meta_steps.iter().zip(replacements.iter()).collect())
}
