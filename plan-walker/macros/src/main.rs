use color_eyre::eyre::Result;
use parsing::domain::action::Action;
use parsing::domain::Domain;
use parsing::problem::parse_problem;
use parsing::problem::Problem;
use shared::time::{init_time, run_time};

use std::ffi::OsString;
use std::fs;
use std::fs::remove_dir_all;
use std::path::Path;
use std::process::Command;

use clap::Parser;

use crate::script_writing::generate_script;

mod script_writing;

#[derive(Parser, Default, Debug)]
#[command(term_width = 0)]
pub struct Args {
    /// Path to original domain
    #[arg(short = 'd')]
    domain: OsString,
    /// Path to problems
    #[arg(short = 'p')]
    problems: OsString,
    /// Path to CSMs root
    #[arg(short = 'c')]
    csms: OsString,
    /// Path to fast downward
    #[arg(short = 'f')]
    fast_downward: OsString,
    /// Path to write macros to
    /// If not given, simply prints to stdout
    #[arg(short = 'o')]
    out: Option<OsString>,
}

fn main() -> Result<()> {
    color_eyre::install()?;
    init_time();

    let args = Args::parse();

    let temp_path = "temp_data";
    let script_path = Path::new(&args.csms).join("scripts").join("fd.sh");

    println!("{} Removing old temp dir...", run_time());
    let _ = remove_dir_all(temp_path);

    println!("{} Creating temp dir...", run_time());
    let _ = fs::create_dir(temp_path);

    println!("{} Copying domain to temp dir...", run_time());
    let _ = fs::copy(&args.domain, Path::new(temp_path).join("domain.pddl"));

    println!("{} Copying learning problems to temp dir...", run_time());
    let traning_path = Path::new(temp_path).join("learn");
    let _ = fs::create_dir(&traning_path);
    if let Ok(files) = fs::read_dir(&args.problems) {
        for file in files {
            let _ = fs::copy(
                file.as_ref().unwrap().path(),
                traning_path.join(file.as_ref().unwrap().file_name()),
            );
        }
    }

    let testing_path = Path::new(temp_path).join("testing");
    let _ = fs::create_dir(&testing_path);
    if let Ok(files) = fs::read_dir(&args.problems) {
        for file in files {
            let _ = fs::copy(
                file.as_ref().unwrap().path(),
                testing_path.join(file.as_ref().unwrap().file_name()),
            );
        }
    }

    println!("{} Generating script...", run_time());
    generate_script(&args.fast_downward, &OsString::from(&script_path));

    println!("{} Running CSM...", run_time());
    match Command::new(Path::new(&args.csms).join("scripts").join("learn-csm.sh"))
        .current_dir(Path::new(&args.csms).join("scripts"))
        .args(&[
            fs::canonicalize(Path::new(temp_path).to_path_buf())
                .unwrap()
                .to_str()
                .unwrap(),
            "fd.sh",
            "csm",
        ])
        .output()
    {
        Ok(out) => {
            if out.status.success() {
                //println!(
                //    "Could not run CSMs with error\nstdout: {}\nstderr: {}",
                //    String::from_utf8(out.stdout).unwrap(),
                //    String::from_utf8(out.stderr).unwrap()
                //);
                println!("{} CSMs success. I think, although with the amount of errors it emits, I can't be certain.", run_time());
            } else {
                panic!(
                    "Could not run CSMs with error\nstdout: {}\nstderr: {}",
                    String::from_utf8(out.stdout).unwrap(),
                    String::from_utf8(out.stderr).unwrap()
                )
            }
        }
        Err(err) => panic!("Could not run CSM with error: {}", err),
    }

    let _ = fs::remove_file(&script_path);

    println!("{} Reading enhanced domain...", run_time());
    let enhanced_domain = Domain::from(&Into::<OsString>::into(
        Path::new(temp_path)
            .join("domain_csm.pddl")
            .to_str()
            .unwrap(),
    ));

    println!("{} Removing temp dir...", run_time());
    let _ = remove_dir_all(temp_path);

    println!("{} Finding macro actions in domain...", run_time());
    let macro_actions: Vec<&Action> = enhanced_domain
        .actions
        .iter()
        .filter_map(|a| {
            if a.name.contains("_mcr_") {
                Some(a)
            } else {
                None
            }
        })
        .collect();

    println!("Found {} macro actions", macro_actions.len());

    match args.out {
        Some(o) => {
            println!("{} Creating out dir...", run_time());
            let _ = fs::create_dir(&o);
            for (i, action) in macro_actions.iter().enumerate() {
                let _ = fs::write(
                    Path::new(&o).join(format!("{}.pddl", i)),
                    format!("{}", action.to_string()),
                );
            }
        }
        None => {
            for (i, action) in macro_actions.iter().enumerate() {
                println!("{}: {}", i, action.name);
            }
        }
    }
    Ok(())
}
