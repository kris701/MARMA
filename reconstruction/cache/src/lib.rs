use std::{
    collections::HashMap,
    fs, io,
    path::{Path, PathBuf},
};

use shared::{io::file::file_name, time::run_time};
use spingus::{
    domain::action::{parse_action, Action},
    sas_plan::{parse_sas, SASPlan},
    term::Term,
};
use state::{instance::Instance, state::State};

pub mod generation;
mod hash_cache;
mod lifted_cache;

pub trait Cache {
    /// Retrives replacement from cache from given init to goal
    fn get_replacement(
        &self,
        instance: &Instance,
        meta_term: &Term,
        init: &State,
        goal: &State,
    ) -> Option<SASPlan>;
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

    let mut combined: Vec<(PathBuf, PathBuf)> = Vec::new();
    for plan_path in &plans {
        let plan_name = file_name(plan_path);
        let mut found = false;
        for macro_path in &macros {
            let macro_name = file_name(macro_path);
            if plan_name.contains(&macro_name) {
                combined.push((macro_path.to_owned(), plan_path.to_owned()));
                found = true;
                break;
            }
        }
        if !found {
            panic!("Could not find macro for plan {:?}", plan_path);
        }
    }

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
        println!(
            "{} Reading replacements for {:?}...",
            run_time(),
            path.file_name().unwrap()
        );
        let substitutes = match read_meta_dir(&path) {
            Ok(v) => v,
            Err(e) => {
                return Err(format!(
                    "Reading meta dir {:?} failed with error: {}",
                    path, e
                ))
            }
        };
        println!("Found {} replacements", substitutes.len());

        replacements.insert(
            path.file_name().unwrap().to_str().unwrap().to_string(),
            parse_replacements(substitutes)?,
        );
    }
    Ok(replacements)
}
