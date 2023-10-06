use std::ffi::OsString;

use clap::Parser;
use color_eyre::eyre::Result;
use parsing::{domain::Domain, problem::Problem, sas::SASPlan};
use shared::time::{init_time, run_time};
use state::{
    instance::fact::Facts,
    plan::Plan,
    state::{generate_state, State},
};

use crate::{blocks::block_decomposition, deordering::deorder};

mod blocks;
mod constraints;
pub mod deordering;

#[derive(Parser, Default, Debug)]
#[command(term_width = 0)]
pub struct Args {
    /// Path to original domain
    #[arg(short = 'd')]
    domain: OsString,
    /// Path to original problem
    #[arg(short = 'p')]
    problem: OsString,
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

fn main() -> Result<()> {
    color_eyre::install()?;
    init_time();

    let args = Args::parse();

    println!("{} Parsing domain...", run_time());
    let domain = Domain::from(&args.domain);
    println!("{} Parsing problem...", run_time());
    let problem = Problem::from(&args.problem);
    println!("{} Generating facts...", run_time());
    let facts = Facts::new(&domain, &problem);
    println!("Total: {}", facts.count());
    println!("{} Generating state...", run_time());
    let state = generate_state(&domain, &problem, &facts);
    println!("{} Parsing solution...", run_time());
    let solution = SASPlan::from(&args.solution.unwrap());
    println!("Steps: {}", solution.steps.len());
    println!("{} Converting solution...", run_time());
    let plan = Plan::new(&domain, &problem, &facts, solution);
    let deordered = deorder(&domain, &problem, &facts, &state, plan);
    println!("Deordered len: {}", deordered.len());
    for i in 0..deordered.len() {
        println!("Layer: {}", i);
        for block in &deordered[i] {
            for step in &block.steps {
                println!("{}", domain.actions[step.action_index].name);
            }
        }
    }

    println!("{} Finished...", run_time());
    Ok(())
}
