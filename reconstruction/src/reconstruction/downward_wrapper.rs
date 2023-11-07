use spingus::sas_plan::{parse_sas, SASPlan};
use std::{
    fs,
    path::{Path, PathBuf},
    process::Command,
};

use crate::tools::random_file_name;

pub struct Downward {
    pub path: PathBuf,
    pub temp_dir: PathBuf,
}

impl Downward {
    pub fn new(path: &PathBuf, temp_dir: &PathBuf) -> Self {
        if !Path::new(&path).exists() {
            panic!("Could not find fast downward at given location");
        }
        Self {
            path: path.to_owned(),
            temp_dir: temp_dir.to_owned(),
        }
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
        let plan_path = random_file_name(&self.temp_dir);
        let sas_path = random_file_name(&self.temp_dir);

        cmd.args(&[
            "--sas-file",
            &sas_path,
            "--plan-file",
            &plan_path,
            domain_path.to_str().unwrap(),
            problem_path.to_str().unwrap(),
            "--evaluator",
            "hff=ff()",
            "--search",
            "lazy_greedy([hff], preferred=[hff])",
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
            let content = fs::read_to_string(path).unwrap();
            match parse_sas(&content) {
                Ok(plan) => return plan,
                Err(err) => panic!("Could not parse given solution with error:\n{}.", err),
            }
        }
        match self.solve(domain_path, problem_path) {
            Ok(plan) => plan,
            Err(err) => panic!(
                "Could not solve provided meta domain / problem. Had error: {}",
                err.to_string()
            ),
        }
    }
}
