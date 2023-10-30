mod cache;
mod reconstruction;
mod state;
mod tools;

use cache::generation::{generate_cache, CacheMethod};
use reconstruction::reconstruction::reconstruct;
use spingus::domain::parse_domain;
use spingus::problem::parse_problem;
use spingus::sas_plan::export_sas;
use state::instance::Instance;
use tools::time::{init_time, run_time};

use std::fs;
use std::path::PathBuf;

use clap::Parser;

use crate::reconstruction::downward_wrapper::Downward;

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
    #[arg(short = 'f')]
    downward: PathBuf,
    /// Path to solution for meta domain + problem.
    /// If not provided, uses fast downward to generate it
    #[arg(short = 's')]
    solution: Option<PathBuf>,
    /// Path to write final solution to
    /// If not given, simply prints to stdout
    #[arg(short = 'o')]
    out: Option<PathBuf>,
    #[arg(long, default_value = "/tmp")]
    temp_dir: PathBuf,
    /// Path to a set of lifted macros used to cache meta action reconstruction
    #[arg(short = 'c')]
    cache: Option<PathBuf>,
    /// Type of caching
    #[arg(long = "cache_method", default_value = "hash")]
    cache_method: CacheMethod,
    /// Stop after translation, mainly used for debugging
    #[arg(long = "translate_only", num_args = 0)]
    translate_only: bool,
}

fn main() {
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
    println!("{} Checking cache...", run_time());
    let cache = generate_cache(&instance, &args.cache, args.cache_method);
    if !args.translate_only {
        println!("{} Beginning reconstruction...", run_time());
        println!("{} Finding fast downward...", run_time());
        let downward = Downward::new(args.downward, args.temp_dir);
        println!("{} Finding meta solution...", run_time());
        let meta_plan = downward.find_or_solve(&args.meta_domain, &args.problem, &args.solution);
        let plan = reconstruct(
            &instance,
            &meta_domain,
            &args.domain,
            &downward,
            &cache,
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

    println!("{} Done", run_time());
}
