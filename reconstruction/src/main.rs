mod cache;
mod instance;
mod reconstruction;
mod state;
mod tools;
mod world;

use cache::generation::{generate_cache, CacheMethod};
use reconstruction::reconstruction::reconstruct;
use spingus::domain::parse_domain;
use spingus::problem::parse_problem;
use spingus::sas_plan::{export_sas, SASPlan};
use tools::time::init_time;
use tools::Status;

use std::fs;
use std::path::PathBuf;

use clap::Parser;

use crate::instance::Instance;
use crate::reconstruction::downward_wrapper::Downward;
use crate::tools::status_print;
use crate::tools::val::check_val;
use crate::world::{World, WORLD};

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
    /// Path to val
    /// If given checks reconstructed plan with VAL
    #[arg(short, long)]
    val: Option<PathBuf>,
}

// TODO: Make plan object with this
fn contains_meta(plan: &SASPlan) -> bool {
    plan.iter().any(|s| World::global().is_meta_action(&s.name))
}

fn main() {
    init_time();

    let args = Args::parse();

    status_print(Status::Init, "Reading meta domain");
    let meta_domain = fs::read_to_string(&args.meta_domain).unwrap();
    status_print(Status::Init, "Reading domain");
    let domain = fs::read_to_string(&args.domain).unwrap();
    status_print(Status::Init, "Reading problem");
    let problem = fs::read_to_string(&args.problem).unwrap();
    status_print(Status::Init, "Parsing meta domain");
    let meta_domain = parse_domain(&meta_domain).unwrap();
    status_print(Status::Init, "Parsing domain");
    let domain = parse_domain(&domain).unwrap();
    status_print(Status::Init, "Parsing problem");
    let problem = parse_problem(&problem).unwrap();
    status_print(Status::Init, "Generating world");
    let world = World::generate(&domain, &meta_domain, &problem);
    let _ = WORLD.set(world);
    status_print(Status::Init, "Finding fast downward");
    let downward = Downward::new(&args.downward, &args.temp_dir);
    status_print(Status::Init, "Checking plan for meta actions");
    let meta_plan = downward.find_or_solve(&args.meta_domain, &args.problem, &args.solution);
    if !contains_meta(&meta_plan) {
        status_print(Status::Init, "None found. Exiting.");
        return;
    }
    status_print(Status::Init, "Plan contains meta actions. Continuing");
    status_print(Status::Init, "Generating instance");
    let instance = Instance::new(domain, problem, meta_domain.to_owned());
    let cache = generate_cache(&instance, &args.cache, args.cache_method);
    status_print(Status::Reconstruction, "Finding meta solution downward");
    let plan = reconstruct(&instance, &args.domain, &downward, &cache, meta_plan);
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
