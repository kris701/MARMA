use color_eyre::eyre::Result;
use parsing::domain::Domain;
use parsing::problem::Problem;
use reconstruction::reconstruct;
use shared::io::file::write_file;
use shared::time::{init_time, run_time};
use state::instance::Instance;

use std::ffi::OsString;

use crate::downward_wrapper::Downward;
use clap::Parser;

mod downward_wrapper;
mod reconstruction;
mod stiching;

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

fn main() -> Result<()> {
    color_eyre::install()?;
    init_time();

    let args = Args::parse();

    println!("{} Pasing instance....", run_time());
    let domain = Domain::from(&args.domain);
    let problem = Problem::from(&args.problem);
    println!("{} Converting instance....", run_time());
    let instance = Instance::new(domain, problem);

    println!("{} Finding fast downward...", run_time());
    let downward = Downward::new(&args.downward);
    println!("{} Finding meta solution...", run_time());
    let meta_plan = downward.solve_or_find(&args.meta_domain, &args.problem, &args.solution);

    let meta_domain = Domain::from(&args.meta_domain);

    let plan = reconstruct(instance, &args.domain, meta_domain, &downward, meta_plan).to_string();
    match args.out {
        Some(path) => {
            write_file(&path, plan);
        }
        None => {
            println!("{} Final plan\n{}", run_time(), plan);
        }
    }

    Ok(())
}
