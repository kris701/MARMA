mod cache;
mod fact;
mod macro_generation;
mod reconstruction;
mod state;
mod successor_genrator;
mod tools;
mod world;

use crate::reconstruction::downward_wrapper::{init_downward, Downward};
use crate::successor_genrator::get_permutation_count;
use crate::tools::val::check_val;
use crate::tools::{random_file_name, status_print};
use crate::world::{init_world, World};
use cache::generation::{generate_cache, CacheMethod};
use cache::Cache;
use clap::Parser;
use reconstruction::reconstruction::reconstruct;
use spingus::sas_plan::{export_sas, SASPlan};
use std::fs;
use std::path::PathBuf;
use std::process::exit;
use std::time::Instant;
use tools::time::init_time;
use tools::Status;

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
    downward: Option<PathBuf>,
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
    #[arg(long, default_value = "lifted")]
    cache_method: CacheMethod,
    /// If given, adds entries found by planner to cache
    #[arg(short, long)]
    iterative_cache: bool,
    /// Path to val
    /// If given checks reconstructed plan with VAL
    #[arg(short, long)]
    val: Option<PathBuf>,
}

fn meta_solve(
    cache: &mut Option<Box<dyn Cache>>,
    iterative: bool,
    domain_path: &PathBuf,
    problem_path: &PathBuf,
) -> SASPlan {
    let mut search_time: f64 = 0.0;
    let meta_count = World::global().meta_actions.len();
    let mut banned_meta_actions: Vec<usize> = Vec::new();
    while banned_meta_actions.len() < meta_count {
        let meta_domain = World::global().export_meta_domain(&banned_meta_actions);
        let meta_file = PathBuf::from(random_file_name(&Downward::global().temp_dir));
        let _ = fs::write(&meta_file, meta_domain);
        let search_begin = Instant::now();
        let plan = Downward::global().solve(&meta_file, problem_path).unwrap();
        search_time += search_begin.elapsed().as_secs_f64();
        let reconstructed = reconstruct(cache, iterative, domain_path, plan);
        let _ = fs::remove_file(&meta_file);
        match reconstructed {
            Ok(plan) => {
                println!("search_time={:.4}", search_time);
                return plan;
            }
            Err(err) => {
                println!(
                    "Invalid meta action: {}",
                    World::global().meta_actions[err].name
                );
                banned_meta_actions.push(err)
            }
        }
    }
    panic!()
}

fn main() {
    init_time();

    let args = Args::parse();

    init_world(&args.domain, &args.meta_domain, &args.problem);
    init_downward(&args.downward, &args.temp_dir);
    let mut cache = generate_cache(&args.cache, args.cache_method, args.iterative_cache);

    let plan = meta_solve(
        &mut cache,
        args.iterative_cache,
        &args.domain,
        &args.problem,
    );
    println!("operator_count={}", get_permutation_count());
    if let Some(val_path) = args.val {
        status_print(Status::Validation, "Checking VAL");
        if check_val(
            &args.domain,
            &args.problem,
            &val_path,
            &args.temp_dir,
            &plan,
        ) {
            println!("---VALID---");
        } else {
            println!("---NOT VALID---");
        }
    }
    println!("final_plan_length={}", &plan.len());
    if let Some(path) = args.out {
        let plan_export = export_sas(&plan);
        fs::write(path, plan_export).unwrap();
    }
    exit(0)
}
