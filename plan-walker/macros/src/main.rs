use color_eyre::eyre::Result;
use parsing::domain::action::Action;
use parsing::domain::Domain;
use parsing::problem::parse_problem;
use parsing::problem::Problem;
use shared::time::{init_time, run_time};

use itertools::Itertools;
use std::ffi::OsString;
use std::fs;
use std::fs::remove_dir_all;
use std::path::Path;
use std::path::PathBuf;
use std::process::Command;

use clap::Parser;

use crate::script_writing::generate_script;

mod csms_wrapper;
mod script_writing;

#[derive(Parser, Default, Debug)]
#[command(term_width = 0)]
pub struct Args {
    /// Path to original domain
    #[arg(short = 'd')]
    domain: PathBuf,
    /// Path to problems
    #[arg(short = 'p')]
    problems: PathBuf,
    /// Path to CSMs root
    #[arg(short = 'c')]
    csms: PathBuf,
    /// Path to fast downward
    #[arg(short = 'f')]
    fast_downward: PathBuf,
    /// Path to write macros to
    /// If not given, simply prints to stdout
    #[arg(short = 'o')]
    out: Option<PathBuf>,
}

fn main() -> Result<()> {
    color_eyre::install()?;
    init_time();

    let args = Args::parse();
    let macros = csms_wrapper::generate_macros(
        &args.fast_downward,
        &args.csms,
        &args.domain,
        &args.problems,
    );

    let macros: Vec<&Action> = macros.iter().unique().collect();

    println!("Found {} macro actions", macros.len());

    match args.out {
        Some(o) => {
            println!("{} Creating out dir...", run_time());
            let _ = fs::remove_dir_all(&o);
            let _ = fs::create_dir(&o);
            for (i, action) in macros.iter().enumerate() {
                let _ = fs::write(
                    Path::new(&o).join(format!("{}.pddl", i)),
                    format!("{}", action.to_string()),
                );
            }
        }
        None => {
            for (i, action) in macros.iter().enumerate() {
                println!("{}: {}", i, action.name);
            }
        }
    }
    Ok(())
}
