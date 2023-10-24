use cache::{generate_cache, read_cache_input, Cache};
use color_eyre::eyre::Result;
use reconstruction::reconstruct;
use shared::time::{init_time, run_time};
use spingus::domain::parse_domain;
use spingus::problem::parse_problem;
use spingus::sas_plan::export_sas;
use state::instance::Instance;

use std::fs;
use std::path::PathBuf;

use crate::downward_wrapper::Downward;
use clap::{Parser, ValueEnum};

mod downward_wrapper;
mod problem_writing;
mod reconstruction;
mod stiching;

#[derive(Debug, Copy, Clone, PartialEq, Default, ValueEnum)]
enum ReconstructionMethod {
    #[default]
    None,
    FastDownward,
}

#[derive(Debug, Copy, Clone, PartialEq, Default, ValueEnum)]
enum CacheMethod {
    #[default]
    BitExp,
}

#[derive(Parser, Default, Debug)]
#[command(term_width = 0)]
pub struct Args {
    /// Path to original domain
    #[arg(short = 'd')]
    domain: PathBuf,
    /// Path to original problem
    #[arg(short = 'p')]
    problem: PathBuf,
    /// Path to meta domain
    #[arg(short = 'm')]
    meta_domain: PathBuf,
    /// Path to fast-downward.
    /// Required only if not found in env or path.
    /// Searches for {downward, fast-downward, fastdownward} irregardles (somewhat) of casing
    #[arg(short = 'f')]
    downward: Option<PathBuf>,
    /// Path to solution for meta domain + problem.
    /// If not provided, uses fast downward to generate it
    #[arg(short = 's')]
    solution: Option<PathBuf>,
    /// Path to write final solution to
    /// If not given, simply prints to stdout
    #[arg(short = 'o')]
    out: Option<PathBuf>,
    /// Path to a set of lifted macros used to cache meta action reconstruction
    #[arg(short = 'c')]
    cache: Option<PathBuf>,
    /// Type of caching
    #[arg(long = "cache_method", default_value = "bit-exp")]
    cache_method: CacheMethod,
    /// Method of reconstruction
    #[arg(long = "reconstruction_method", default_value = "none")]
    reconstruction_method: ReconstructionMethod,
}

fn main() -> Result<()> {
    color_eyre::install()?;
    init_time();

    let args = Args::parse();

    println!("{} Reading meta domain....", run_time());
    let meta_domain = fs::read_to_string(&args.meta_domain).unwrap();
    println!("{} Reading domain....", run_time());
    let domain = fs::read_to_string(&args.domain).unwrap();
    println!("{} Reading problem....", run_time());
    let problem = fs::read_to_string(&args.problem).unwrap();
    println!("{} Parsing meta domain....", run_time());
    let meta_domain = parse_domain(&meta_domain).unwrap();
    println!("{} Parsing domain....", run_time());
    let domain = parse_domain(&domain).unwrap();
    println!("{} Parsing problem....", run_time());
    let problem = parse_problem(&problem).unwrap();
    println!("{} Converting instance....", run_time());
    let instance = Instance::new(domain, problem);

    println!("{} Checking for cache...", run_time());
    let cache: Option<Cache> = match args.cache {
        Some(p) => {
            println!("{} Reading cache files...", run_time());
            let files = match read_cache_input(&p) {
                Ok(f) => f,
                Err(e) => panic!("Could not read cache files with err: {}", e),
            };
            println!("Found {} lifted macros", files.len());
            println!("{} Generating cache...", run_time());
            Some(generate_cache(
                &instance.domain,
                &instance.problem,
                &instance.facts,
                files,
            ))
        }
        None => {
            println!("{} No cache given", run_time());
            None
        }
    };
    if args.reconstruction_method != ReconstructionMethod::None {
        println!("{} Finding fast downward...", run_time());
        let downward = Downward::new(&args.downward);
        println!("{} Finding meta solution...", run_time());
        let meta_plan = downward.solve_or_find(&args.meta_domain, &args.problem, &args.solution);
        let plan = reconstruct(
            instance,
            &meta_domain,
            &args.domain,
            &downward,
            cache.as_ref(),
            meta_plan,
        );
        let plan_export = export_sas(&plan);
        match args.out {
            Some(path) => fs::write(path, plan_export).unwrap(),
            None => {
                println!("Final plan\n{}", plan_export);
            }
        }
    }

    Ok(())
}
