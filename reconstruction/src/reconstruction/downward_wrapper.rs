use spingus::sas_plan::{parse_sas, SASPlan};
use std::{
    fs,
    path::{Path, PathBuf},
    process::Command,
};

use crate::tools::{random_file_name, random_name, time::run_time};

pub struct Downward {
    pub path: PathBuf,
}

impl Downward {
    pub fn new(path: PathBuf) -> Self {
        if !Path::new(&path).exists() {
            panic!("Could not find fast downward at given location");
        }
        Self { path }
    }

    fn run(&self, cmd: &mut Command, plan_path: &String) -> Result<SASPlan, String> {
        let out = cmd.output().map_err(|e| e.to_string())?;
        if !out.status.success() {
            return Err(format!(
                "Downward failed with args:\n{:?}stdout:\n{}err:\n{}",
                cmd.get_args(),
                String::from_utf8(out.stdout).unwrap(),
                String::from_utf8(out.stderr).unwrap(),
            ));
        }
        let plan = fs::read_to_string(plan_path).map_err(|e| e.to_string())?;
        parse_sas(&plan).map_err(|e| {
            format!(
                "Could not parse the plan downward outputted. Plan: {}\nError: {}",
                &plan,
                e.to_string()
            )
        })
    }

    pub fn solve(&self, domain_path: &PathBuf, problem_path: &PathBuf) -> Result<SASPlan, String> {
        let mut cmd: Command = Command::new(Path::new(&self.path).to_str().unwrap());
        let plan_path = random_name();
        let sas_path = random_name();

        cmd.args(&[
            "--alias",
            "lama-first",
            "--sas-file",
            &sas_path,
            "--plan-file",
            &plan_path,
            domain_path.to_str().unwrap(),
            problem_path.to_str().unwrap(),
        ]);

        let result = self.run(&mut cmd, &plan_path);
        _ = fs::remove_file(sas_path);
        _ = fs::remove_file(plan_path);
        match result {
            Ok(plan) => Ok(plan),
            Err(err) => Err(format!(
                "Had error during fast downward replacement. Error: {}",
                err
            )),
        }
    }

    pub fn find_or_solve(
        &self,
        domain_path: &PathBuf,
        problem_path: &PathBuf,
        plan: &Option<PathBuf>,
    ) -> SASPlan {
        if let Some(path) = plan {
            println!("Solution given");
            let content = fs::read_to_string(path).unwrap();
            println!("{} Parsing solution...", run_time());
            match parse_sas(&content) {
                Ok(plan) => return plan,
                Err(err) => panic!("Could not parse given solution with error:\n{}.", err),
            }
        }
        println!("{} Solution not given", run_time());
        println!("{} Using downward to find...", run_time());
        match self.solve(domain_path, problem_path) {
            Ok(plan) => plan,
            Err(err) => panic!(
                "Could not solve provided meta domain / problem. Had error: {}",
                err.to_string()
            ),
        }
    }
}
