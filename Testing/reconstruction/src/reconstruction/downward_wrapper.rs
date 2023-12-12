use pathsearch::find_executable_in_path;
use std::{
    fs,
    path::{Path, PathBuf},
    process::Command,
};

use crate::{
    plan::{read_plan, Plan},
    tools::{random_file_name, status_print, Status},
};
use once_cell::sync::OnceCell;

#[derive(Debug)]
pub enum DownwardError {
    Unsolvable,
    Launch(String),
    RunTime(String),
    PlanParse(String),
}

impl DownwardError {
    pub fn to_string(&self) -> String {
        match self {
            DownwardError::Unsolvable => format!("Downward could not solve problem"),
            DownwardError::Launch(err) => format!("Could not launch downward with err: {}", err),
            DownwardError::RunTime(err) => format!("Downward crashed with err: {}", err),
            DownwardError::PlanParse(err) => format!("Failed to parse plan with err: {}", err),
        }
    }
}

pub struct Downward {
    pub path: PathBuf,
    pub temp_dir: PathBuf,
}

pub static DOWNWARD: OnceCell<Downward> = OnceCell::new();

impl Downward {
    pub fn global() -> &'static Downward {
        DOWNWARD.get().expect("downward is not initialized")
    }

    pub fn new(path: &Option<PathBuf>, temp_dir: &PathBuf) -> Self {
        let path = match path {
            Some(path) => path.into(),
            None => find_executable_in_path("fast-downward.py").unwrap(),
        };
        if !Path::new(&path).exists() {
            panic!("Could not find fast downward at given location");
        }
        Self {
            path: path.to_owned(),
            temp_dir: temp_dir.to_owned(),
        }
    }

    fn run(&self, cmd: &mut Command, plan_path: &String) -> Result<Plan, DownwardError> {
        let out = cmd
            .output()
            .map_err(|e| DownwardError::Launch(e.to_string()))?;

        if !out.status.success() {
            if out.status.code().unwrap() == 12 {
                return Err(DownwardError::Unsolvable);
            } else {
                return Err(DownwardError::RunTime(format!(
                    "{:?}stdout:\n{}err:\n{}",
                    cmd.get_args(),
                    String::from_utf8(out.stdout).unwrap(),
                    String::from_utf8(out.stderr).unwrap(),
                )));
            }
        }
        read_plan(plan_path).map_err(|e| DownwardError::PlanParse(e))
    }

    pub fn solve(
        &self,
        domain_path: &PathBuf,
        problem_path: &PathBuf,
    ) -> Result<Plan, DownwardError> {
        let command_path = match Path::new(&self.path).to_str() {
            Some(p) => p,
            None => panic!("Had error trying to generate command path"),
        };
        let mut cmd: Command = Command::new(command_path);
        let plan_path = random_file_name(&self.temp_dir);
        let sas_path = random_file_name(&self.temp_dir);

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
        println!("Running command: {:?}", cmd);
        let result = self.run(&mut cmd, &plan_path);
        _ = fs::remove_file(sas_path);
        _ = fs::remove_file(plan_path);
        result
    }
}

pub fn init_downward(downward: &Option<PathBuf>, temp_dir: &PathBuf) {
    status_print(Status::Init, "Finding downward");
    let _ = DOWNWARD.set(Downward::new(downward, temp_dir));
}
