use std::{env, fs, time::Instant};

use parsing::domain::parse_domain;
use parsing::problem::parse_problem;
use parsing::sas::parse_sas;

use crate::downward_wrapper::Downward;
use crate::instance::fact::Facts;
use crate::plan::{next_goal, next_init, Plan};
use crate::problem_writing::write_problem;
use crate::state::State;
use crate::stiching::stich_single;
use crate::time::{init_time, run_time};

use std::collections::hash_map::DefaultHasher;
use std::hash::{Hash, Hasher};

mod downward_wrapper;
mod instance;
mod plan;
mod problem_writing;
mod state;
mod stiching;
mod time;

fn main() {
    init_time();
    let args: Vec<String> = env::args().collect();
    if args.len() == 1 || args[1] == "-h" || args[1] == "--help" {
        println!("First argument is meta-domain file path");
        println!("Second argument is domain file path");
        println!("Third argument is problem file path");
        println!("Fourth argument is fast-downward path");
        println!("Fifth argument is sas plan path");
        return;
    }
    let metadomain_path = &args[1];
    let domain_path = &args[2];
    let problem_path = &args[3];
    println!("{} Reading domain...", run_time());
    let metadomain_string = fs::read_to_string(metadomain_path).unwrap();
    let domain_string = fs::read_to_string(domain_path).unwrap();
    println!("{} Reading problem...", run_time());
    let problem_string = fs::read_to_string(problem_path).unwrap();
    println!("{} Parsing domain...", run_time());
    let metadomain = parse_domain(&metadomain_string).unwrap();
    let domain = parse_domain(&domain_string).unwrap();
    println!("{} Parsing problem...", run_time());
    let problem = parse_problem(&problem_string).unwrap();
    println!("{} Converting predicates...", run_time());
    let facts = Facts::new(&domain, &problem);
    let downward_path = &args[4];
    let downward = Downward::new(downward_path.to_string());
    let sas_path = &args[5];
    println!("{} Generating init...", run_time());
    let init = State::new(&domain, &problem, &facts);
    println!("{} Reading plan...", run_time());
    let sas_string = fs::read_to_string(sas_path).unwrap();
    println!("{} Parsing plan...", run_time());
    let mut sas_plan = parse_sas(&sas_string).unwrap();
    while sas_plan.has_meta() {
        let init_plan = next_init(&domain, &problem, &facts, &sas_plan);
        let goal_plan = next_goal(&metadomain, &domain, &problem, &facts, &sas_plan);
        assert_ne!(init_plan, goal_plan);
        let init_state = init.apply_plan(&facts, &init_plan);
        let goal_state = init.apply_plan(&facts, &goal_plan);
        assert_ne!(init_state, goal_state);
        let temp_problem_path = ".temp_problem.pddl";
        println!("{} Writing temp problem...", run_time());
        write_problem(
            &domain,
            &problem,
            &facts,
            &init_state,
            &goal_state,
            &temp_problem_path,
        );
        println!("{} Solving temp problem...", run_time());
        downward.solve(&domain_path, &temp_problem_path);
        println!("{} Reading temp plan...", run_time());
        let sas_string = fs::read_to_string("sas_plan").unwrap();
        println!("{} Parsing temp plan...", run_time());
        let temp_plan = parse_sas(&sas_string).unwrap();
        println!("{} Stitching plan...", run_time());
        sas_plan = stich_single(&sas_plan, &temp_plan);
    }
    println!("{} Final Plan", run_time());
    sas_plan.print();
}
