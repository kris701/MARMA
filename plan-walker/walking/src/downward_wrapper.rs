use std::{
    env, fs,
    process::{Command, ExitStatus},
};

use parsing::sas::{parse_sas, SASPlan};

pub struct Downward {
    path: String,
}

impl Downward {
    pub fn new(path: String) -> Self {
        Self { path }
    }

    pub fn solve(&self, domain_path: &str, problem_path: &str) -> SASPlan {
        let mut cmd = Command::new(format!("{}/fast-downward.py", self.path));
        cmd.args(&["--alias", "lama-first", domain_path, problem_path]);

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
            Err(err) => panic!("Could not solve problem: {}", err),
        }
    }
}
