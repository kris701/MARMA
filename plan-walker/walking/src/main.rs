use color_eyre::eyre::Result;

use std::ffi::OsString;
use std::fs::{self};

use parsing::domain::{parse_domain, Domain};
use parsing::problem::{parse_problem, Problem};
use parsing::sas::parse_sas;

use crate::downward_wrapper::Downward;
use crate::file_io::{read_file, write_file};
use crate::instance::fact::Facts;
use crate::plan::{next_goal, next_init};
use crate::problem_writing::write_problem;
use crate::state::State;
use crate::stiching::stich_single;
use crate::time::{init_time, run_time};
use clap::Parser;

mod downward_wrapper;
mod file_io;
mod instance;
mod plan;
mod problem_writing;
mod state;
mod stiching;
mod time;

#[derive(Parser, Default, Debug)]
#[command(term_width = 0)]
pub struct Args {
    /// Path to original domain
    #[arg(short = 'd')]
    domain: OsString,
    /// Path to original problem
    #[arg(short = 'p')]
    problem: OsString,
    /// Path to meta domain
    #[arg(short = 'm')]
    meta_domain: OsString,
    /// Path to fast-downward.
    /// Required only if not found in env or path.
    /// Searches for {downward, fast-downward, fastdownward} irregardles (somewhat) of casing
    #[arg(short = 'f')]
    downward: Option<OsString>,
    /// Path to solution for meta domain + problem.
    /// If not provided, uses fast downward to generate it
    #[arg(short = 's')]
    solution: Option<OsString>,
    /// Path to write final solution to
    /// If not given, simply prints to stdout
    #[arg(short = 'o')]
    out: Option<OsString>,
}

fn parse_instance(args: &Args) -> (Problem, Domain, Domain) {
    let problem_s = read_file(&args.problem);
    let domain_s = read_file(&args.domain);
    let meta_domain_s = read_file(&args.meta_domain);

    println!("{} Parsing problem...", run_time());
    let problem = match parse_problem(&problem_s) {
        Ok(p) => p,
        Err(err) => panic!("Could not parse given problem file with err:\n{}", err),
    };
    println!("{} Parsing domain...", run_time());
    let domain = match parse_domain(&domain_s) {
        Ok(p) => p,
        Err(err) => panic!("Could not parse given domain file with err:\n{}", err),
    };
    println!("{} Parsing meta domain...", run_time());
    let meta_domain = match parse_domain(&meta_domain_s) {
        Ok(p) => p,
        Err(err) => panic!("Could not parse given meta domain file with err:\n{}", err),
    };

    (problem, domain, meta_domain)
}

fn main() -> Result<()> {
    color_eyre::install()?;
    init_time();
    let args = Args::parse();

    let (problem, domain, meta_domain) = parse_instance(&args);
    println!("{} Converting predicates...", run_time());
    let facts = Facts::new(&domain, &problem);
    println!("{} Generating init...", run_time());
    let init = State::new(&domain, &problem, &facts);

    println!("{} Finding downward...", run_time());
    let downward = Downward::new(&args.downward);
    println!("{} Finding solution...", run_time());
    let mut sas_plan = downward.solve_or_find(&args.meta_domain, &args.problem, &args.solution);

    while sas_plan.has_meta() {
        let init_plan = next_init(&domain, &problem, &facts, &sas_plan);
        let goal_plan = next_goal(&meta_domain, &domain, &problem, &facts, &sas_plan);
        if init_plan == goal_plan {
            panic!("LOGIC ERROR: Somehow plan for init and goal are the same. This is a bug.")
        }
        let init_state = init.apply_plan(&init_plan);
        let goal_state = init.apply_plan(&goal_plan);
        if init_state == goal_state {
            panic!("LOGIC ERROR: Somehow state for init and goal are the same. This is a bug.")
        }
        let temp_problem_path = OsString::from(".temp_problem.pddl");
        write_problem(
            &domain,
            &problem,
            &facts,
            &init_state,
            &goal_state,
            &temp_problem_path,
        );
        println!("{} Solving temp problem...", run_time());
        let temp_plan = downward.solve(&args.domain, &temp_problem_path);
        println!("{} Stitching plan...", run_time());
        sas_plan = stich_single(&sas_plan, &temp_plan);
    }
    let sas_plan = sas_plan.to_string();
    if let Some(path) = args.out {
        write_file(&path, sas_plan);
    } else {
        println!("{} Final plan\n{}", run_time(), sas_plan);
    }
    Ok(())
}
