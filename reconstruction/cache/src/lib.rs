use std::{
    collections::HashMap,
    fs, io,
    path::{Path, PathBuf},
};

use spingus::{
    domain::action::{parse_action, Action},
    sas_plan::{parse_sas, SASPlan},
};
use state::{instance::Instance, state::State};

mod bit_cache;

pub trait Cache {
    /// Initializes cache from files at given path
    fn init(instance: &Instance, path: &PathBuf) -> Self;
    /// Retrives replacement from cache from given init to goal
    fn get_replacement(&self, instance: &Instance, init: &State, goal: &State) -> Option<SASPlan>;
}

fn read_cache_dir(path: &PathBuf) -> io::Result<Vec<PathBuf>> {
    let iter = fs::read_dir(path)?;
    let paths: Vec<PathBuf> = iter.map(|e| e.unwrap().path()).collect();
    Ok(paths)
}

fn read_meta_dir(path: &PathBuf) -> io::Result<Vec<(PathBuf, PathBuf)>> {
    let iter = fs::read_dir(path)?;
    let mut macros: Vec<PathBuf> = Vec::new();
    let mut plans: Vec<PathBuf> = Vec::new();
    for entry in iter {
        let path = entry.unwrap().path();
        let path = Path::new(&path);
        match path.extension() {
            None => {}
            Some(ostr) => match ostr.to_str() {
                Some("pddl") => macros.push(path.to_path_buf()),
                Some("plan") => plans.push(path.to_path_buf()),
                _ => panic!("Unexpected file ending on file {:?}", path.to_path_buf()),
            },
        }
    }

    let combined: Vec<(PathBuf, PathBuf)> = plans
        .iter()
        .map(|p| {
            (
                macros
                    .iter()
                    .find(|m| {
                        p.file_name()
                            .unwrap()
                            .to_str()
                            .unwrap()
                            .contains(m.file_name().unwrap().to_str().unwrap())
                    })
                    .unwrap()
                    .to_owned(),
                p.to_owned(),
            )
        })
        .collect();
    Ok(combined)
}

fn parse_replacements(
    replacements: Vec<(PathBuf, PathBuf)>,
) -> Result<Vec<(Action, SASPlan)>, String> {
    let replaced = replacements
        .iter()
        .map(|(macro_path, plan_path)| {
            return (
                parse_action(&fs::read_to_string(&macro_path).unwrap())
                    .unwrap()
                    .1,
                parse_sas(&fs::read_to_string(&plan_path).unwrap()).unwrap(),
            );
        })
        .collect();
    Ok(replaced)
}

pub(crate) fn read_cache_input(
    path: &PathBuf,
) -> Result<HashMap<String, Vec<(Action, SASPlan)>>, String> {
    let meta_paths = match read_cache_dir(path) {
        Ok(p) => p,
        Err(e) => return Err(format!("Reading cache dir failed with error: {}", e).to_string()),
    };
    let mut replacements: HashMap<String, Vec<(Action, SASPlan)>> = HashMap::new();
    for path in meta_paths {
        let substitutes = match read_meta_dir(&path) {
            Ok(v) => v,
            Err(e) => {
                return Err(format!(
                    "Reading meta dir {:?} failed with error: {}",
                    path, e
                ))
            }
        };

        replacements.insert(
            path.file_name().unwrap().to_str().unwrap().to_string(),
            parse_replacements(substitutes)?,
        );
    }
    Ok(replacements)
}
