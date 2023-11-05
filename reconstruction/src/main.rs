mod cache;
mod instance;
mod reconstruction;
mod state;
mod tools;

use cache::generation::{generate_cache, CacheMethod};
use reconstruction::reconstruction::reconstruct;
use spingus::domain::parse_domain;
use spingus::problem::parse_problem;
use spingus::sas_plan::export_sas;
use tools::time::{init_time, run_time};

use std::fs;
use std::path::PathBuf;

use clap::Parser;

use crate::instance::Instance;
use crate::reconstruction::downward_wrapper::Downward;
use crate::tools::val::check_val;

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
    #[arg(long, default_value = "hash")]
    cache_method: CacheMethod,
    /// Stop after translation, mainly used for debugging
    #[arg(long, num_args = 0)]
    translate_only: bool,
    /// Path to val
    /// If given checks reconstructed plan with VAL
    #[arg(short, long)]
    val: Option<PathBuf>,
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
    let instance = Instance::new(domain, problem, meta_domain.to_owned());
    println!("{} Checking cache...", run_time());
    let cache = generate_cache(&instance, &args.cache, args.cache_method);
    if !args.translate_only {
        println!("{} Beginning reconstruction...", run_time());
        println!("{} Finding fast downward...", run_time());
        let downward = Downward::new(&args.downward, &args.temp_dir);
        println!("{} Finding meta solution...", run_time());
        let meta_plan = downward.find_or_solve(&args.meta_domain, &args.problem, &args.solution);
        let plan = reconstruct(&instance, &args.domain, &downward, &cache, meta_plan);
        if let Some(val_path) = args.val {
            println!("{} Checking plan with val...", run_time());
            if check_val(
                &args.domain,
                &args.problem,
                &val_path,
                &args.temp_dir,
                &plan,
            ) {
                println!("Plan is valid");
            } else {
                println!("Plan is not valid");
            }
        }
        match args.out {
            Some(path) => {
                let plan_export = export_sas(&plan);
                fs::write(path, plan_export).unwrap();
            }
            None => {
                println!("Final plan had {} steps", plan.len());
            }
        }
    }

    println!("{} Done", run_time());
}
