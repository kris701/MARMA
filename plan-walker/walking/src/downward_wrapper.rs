use std::{env, ffi::OsString, path::Path, process::Command};

use parsing::sas::{parse_sas, SASPlan};
use shared::{io::file::read_file, time::run_time};

pub struct Downward {
    pub path: OsString,
}

impl Downward {
    pub fn new(path: &Option<OsString>) -> Self {
        // First if given path, use that
        match path {
            Some(path) => {
                if !Path::new(path).exists() {
                    panic!(
                        "Could not find any file at given downward path: {}",
                        path.to_str().unwrap()
                    );
                }
                return Downward {
                    path: path.to_owned(),
                };
            }
            None => {}
        };
        let checks = [
            "downward",
            "DOWNWARD",
            "Downward",
            "fast-donward",
            "FAST-DOWNWARD",
            "Fast-Downward",
            "fastdownward",
            "FASTDOWNWARD",
            "FastDownward",
        ];
        // Check enviornment
        for check in checks {
            match env::var(check) {
                Ok(c) => {
                    if !Path::new(&c).exists() {
                        panic!(
                        "Env/Path indicates downward at following location, however no such file exists: {}",
                        c
                    );
                    }

                    return Downward { path: c.into() };
                }
                Err(_) => {}
            }
        }
        panic!("Fast downward not given as argument, nor found in env or path");
    }

    pub fn solve(&self, domain_path: &OsString, problem_path: &OsString) -> SASPlan {
        if !Path::new(&domain_path).exists() {
            panic!(
                "No file at given domain path: {}",
                domain_path.to_str().unwrap()
            );
        }
        if !Path::new(&problem_path).exists() {
            panic!(
                "No file at given problem path: {}",
                problem_path.to_str().unwrap()
            );
        }
        let mut cmd = Command::new(Path::new(&self.path).to_str().unwrap());
        cmd.args(&[
            "--alias",
            "lama-first",
            domain_path.to_str().unwrap(),
            problem_path.to_str().unwrap(),
        ]);

        match cmd.output() {
            Ok(out) => {
                if !out.status.success() {
                    panic!(
                        "Downward failed with args:\n{:?}stdout:\n{}err:\n{}",
                        cmd.get_args(),
                        String::from_utf8(out.stdout).unwrap(),
                        String::from_utf8(out.stderr).unwrap()
                    )
                }
                let content = read_file(&"sas_plan".into());
                match parse_sas(&content) {
                    Ok(plan) => return plan,
                    Err(err) => panic!("Could not parse sas plan generated by downward.\nThis is likely caused by the init and goal state of the problem being the same, which would lead to a plan length of 0.\nErr:\n{}", err),
                };
            }
            Err(err) => panic!("Could not run downward: {}", err),
        }
    }

    pub fn solve_or_find(
        &self,
        meta_domain_path: &OsString,
        problem_path: &OsString,
        solution_path: &Option<OsString>,
    ) -> SASPlan {
        if let Some(path) = solution_path {
            println!("{} Solution given", run_time());
            let content = read_file(path);
            println!("{} Parsing solution...", run_time());
            match parse_sas(&content) {
                Ok(plan) => return plan,
                Err(err) => panic!("Could not parse given solution with error:\n{}.", err),
            }
        }
        println!("{} Solution not given", run_time());
        println!("{} Using downward to find...", run_time());
        self.solve(meta_domain_path, problem_path)
    }
}
