use color_eyre::eyre::Result;
use spingus::domain::action::Action;

use itertools::Itertools;
use std::fs;
use std::path::Path;
use std::path::PathBuf;

use clap::Parser;

use crate::action_cleaning::clean_action;

mod action_cleaning;
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
    /// Dir for temp files
    /// Should only outlive the program if it crashes
    #[arg(short = 't', default_value = ".temp")]
    temp: Option<PathBuf>,
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
    if !args.csms.is_dir() {
        panic!("Could not CSMS at {}", args.domain.to_str().unwrap());
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
        &args.temp.unwrap(),
    );

    let macros: Vec<Action> = macros
        .iter()
        .unique()
        .map(|a| clean_action(a.to_owned()))
        .collect();
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
