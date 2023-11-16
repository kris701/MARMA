mod cache;
mod fact;
mod instance;
mod reconstruction;
mod state;
mod tools;
mod world;

use cache::generation::{generate_cache, CacheMethod};
use itertools::Itertools;
use reconstruction::reconstruction::reconstruct;
use spingus::domain::parse_domain;
use spingus::problem::parse_problem;
use spingus::sas_plan::{export_sas, parse_sas, SASPlan};
use tools::time::init_time;
use tools::Status;

use clap::Parser;
use std::fs;
use std::path::PathBuf;
use std::process::exit;
use std::time::Instant;

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
    downward: Option<PathBuf>,
    /// Path to solution for meta domain + problem.
    /// If not provided, uses fast downward to generate it
    #[arg(short = 's')]
    solution: PathBuf,
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

fn meta_action_count(plan: &SASPlan) -> usize {
    plan.iter()
        .filter(|s| World::global().is_meta_action(&s.name))
        .count()
}

fn meta_action_count_unique(plan: &SASPlan) -> usize {
    plan.iter()
        .filter(|s| World::global().is_meta_action(&s.name))
        .map(|s| &s.name)
        .unique()
        .count()
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
    status_print(Status::Init, "Reading meta solution");
    let meta_plan = parse_sas(&fs::read_to_string(&args.solution).unwrap()).unwrap();
    println!("meta_plan_length={}", meta_plan.len());
    println!("meta_actions_in_plan={}", meta_action_count(&meta_plan));
    println!(
        "meta_actions_in_plan_unique={}",
        meta_action_count_unique(&meta_plan)
    );
    let plan = match contains_meta(&meta_plan) {
        true => {
            status_print(Status::Init, "Generating instance");
            let instance = Instance::new(domain, meta_domain.to_owned());
            let cache_begin = Instant::now();
            let cache = generate_cache(&instance, &meta_plan, &args.cache, args.cache_method);
            println!(
                "cache_init_time={:.2?}",
                cache_begin.elapsed().as_secs_f64()
            );
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
            plan
        }
        false => meta_plan,
    };
    status_print(Status::Report, "Finished reconstruction");
    println!("final_plan_length={}", &plan.len());
    if let Some(path) = args.out {
        let plan_export = export_sas(&plan);
        fs::write(path, plan_export).unwrap();
    }
    exit(0)
}
