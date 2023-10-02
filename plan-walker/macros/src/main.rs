use std::ffi::OsString;

use clap::Parser;
use color_eyre::eyre::Result;
use parsing::{domain::Domain, problem::Problem, sas::SASPlan};
use shared::time::{init_time, run_time};
use state::{instance::fact::Facts, plan::Plan, state::State};

use crate::blocks::block_decomposition;

mod blocks;

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
    let state = State::new(&domain, &problem, &facts);
    println!("{} Parsing solution...", run_time());
    let solution = SASPlan::from(&args.solution.unwrap());
    println!("Steps: {}", solution.steps.len());
    println!("{} Converting solution...", run_time());
    let plan = Plan::new(&domain, &problem, &facts, solution);
    println!("{} Decomposing plan...", run_time());
    let blocks = block_decomposition(&state, plan);
    println!("Blocks: {}", blocks.len());
    for i in 0..blocks.len() {
        let block = &blocks[i];
        println!("---{}---", i);
        for step in &block.steps {
            println!("{}", domain.actions[step.action_index].name);
        }
    }

    println!("{} Finished...", run_time());
    Ok(())
}
