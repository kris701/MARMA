use std::ffi::OsString;

use parsing::{domain::Domain, sas::SASPlan};
use shared::time::run_time;
use state::{
    instance::Instance,
    plan::{next_goal, next_init},
    problem_writing::write_problem,
    state::{apply_plan_to_state, generate_state},
};

use crate::{downward_wrapper::Downward, stiching::stich_single};

pub fn reconstruct(
    instance: Instance,
    domain_path: &OsString,
    meta_domain: Domain,
    downward: &Downward,
    plan: SASPlan,
) -> SASPlan {
    let problem_path = OsString::from(".temp_problem.pddl");
    let init = generate_state(&instance.domain, &instance.problem, &instance.facts);
    let meta_count = plan.meta_count();
    let mut plan = plan;
    let mut i = 1;
    while plan.has_meta() {
        println!("{} Replacing meta action {}/{}", run_time(), i, meta_count);
        let init_plan = next_init(&instance, &plan);
        let goal_plan = next_goal(&instance, &meta_domain, &plan);
        if init_plan == goal_plan {
            panic!("LOGIC ERROR: Somehow plan for init and goal are the same. This is a bug.")
        }
        let init_state = apply_plan_to_state(&init, &init_plan);
        let goal_state = apply_plan_to_state(&init, &goal_plan);
        if init_state == goal_state {
            panic!("LOGIC ERROR: Somehow state for init and goal are the same. This is a bug.")
        }
        write_problem(&instance, &init_state, &goal_state, &problem_path);
        let replacement_plan = downward.solve(domain_path, &problem_path);
        plan = stich_single(&plan, &replacement_plan);
        i += 1;
    }
    plan
}
