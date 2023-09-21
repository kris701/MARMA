use std::{env, ffi::OsString, fs, process::Command};

use parsing::sas::{parse_sas, SASPlan};

use crate::{read_file, time::run_time};

pub struct Downward {
    path: OsString,
}

impl Downward {
    pub fn new(path: &Option<OsString>) -> Self {
        // First if given path, use that
        match path {
            Some(path) => {
                return Downward {
                    path: path.to_owned(),
                }
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
                Ok(c) => return Downward { path: c.into() },
                Err(_) => {}
            }
        }
        panic!("Fast downward not given as argument, nor found in env or path");
    }

    pub fn solve(&self, domain_path: &OsString, problem_path: &OsString) -> SASPlan {
        let mut cmd = Command::new("python");
        cmd.args(&[
            self.path.to_str().unwrap(),
            "--alias",
            "lama-first",
            domain_path.to_str().unwrap(),
            problem_path.to_str().unwrap(),
        ]);

        match cmd.output() {
            Ok(out) => {
                if !out.status.success() {
                    panic!(
                        "Downward failed with args: \n{:?}stdout: \n{}err: \n{}",
                        cmd.get_args(),
                        String::from_utf8(out.stdout).unwrap(),
                        String::from_utf8(out.stderr).unwrap()
                    )
                }
                let content = match fs::read_to_string(format!("sas_plan")) {
                    Ok(content) => content,
                    Err(err) => panic!("Could not read sas plan: {}", err),
                };
                match parse_sas(&content) {
                    Ok(plan) => return plan,
                    Err(err) => panic!("Could not parse sas plan(This is likely caused by init and goal being the same in problem): {}", err),
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
            return parse_sas(&content).unwrap();
        }
        println!("{} Solution not given", run_time());
        println!("{} Using downward to find...", run_time());
        self.solve(meta_domain_path, problem_path)
    }
}
