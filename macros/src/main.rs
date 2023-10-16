use color_eyre::eyre::Result;
use spingus::domain::action::Action;
use spingus::domain::Domain;
use spingus::problem::parse_problem;
use spingus::problem::Problem;

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

    let args = Args::parse();

    if !args.fast_downward.is_file() {
        panic!(
            "Could not fast downward at {}",
            args.domain.to_str().unwrap()
        );
    }
    if !args.domain.is_file() {
        panic!("Could not find domain at {}", args.domain.to_str().unwrap());
    }
    if !args.problems.is_dir() {
        panic!(
            "Could not find problem folder at {}",
            args.domain.to_str().unwrap()
        );
    }
    if args.problems.read_dir().unwrap().next().is_none() {
        panic!("Found no problems in dir {}", args.domain.to_str().unwrap());
    }

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
            println!("Creating out dir...");
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
