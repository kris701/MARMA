use std::ffi::OsString;

use parsing::{domain::Domain, sas::SASPlan};
use shared::time::run_time;
use state::{
    instance::Instance,
    plan::{next_goal, next_init},
    problem_writing::write_problem,
    state::{generate_state, State},
};

use crate::{downward_wrapper::Downward, stiching::stich_single};

pub fn reconstruct(
    instance: Instance,
    domain_path: &OsString,
    meta_domain: Domain,
    downward: &Downward,
    plan: SASPlan,
) -> SASPlan {
    let init = generate_state(&instance.domain, &instance.problem, &instance.facts);
    let meta_count = plan.meta_count();
    let mut plan = plan;
    while plan.has_meta() {
        todo!();
        //println!(
        //    "{} Replacing meta action {}/{}",
        //    run_time(),
        //    plan.meta_count(),
        //    meta_count
        //);
        //let init_plan = next_init(&instance, &plan);
        //let goal_plan = next_goal(&instance, &meta_domain, &plan);
        //if init_plan == goal_plan {
        //    panic!("LOGIC ERROR: Somehow plan for init and goal are the same. This is a bug.")
        //}
        //let init_state = init.apply_plan(&init_plan);
        //let goal_state = init.apply_plan(&goal_plan);
        //if init_state == goal_state {
        //    panic!("LOGIC ERROR: Somehow state for init and goal are the same. This is a bug.")
        //}
        //let problem_path = OsString::from(".temp_problem.pddl");
        //write_problem(&instance, &init_state, &goal_state, &problem_path);
        //let replacement_plan = downward.solve(domain_path, &problem_path);
        //plan = stich_single(&plan, &replacement_plan);
    }
    plan
}
