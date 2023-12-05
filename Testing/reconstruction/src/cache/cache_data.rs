use std::{collections::HashMap, fs, io, path::PathBuf};

use spingus::{
    domain::action::{parse_action, Action},
    sas_plan::{parse_sas, SASPlan},
};

use crate::{
    tools::io::file::{dir_dirs, dir_files_by_extension, dir_name, match_files, read_pairs},
    world::World,
};

pub type CacheData = HashMap<usize, Vec<(Action, Vec<SASPlan>)>>;

pub fn read_cache(path: &PathBuf) -> CacheData {
    let meta_dirs = dir_dirs(path)
        .map_err(|e| {
            panic!(
                "Failed finding sub directories of meta cache {:?}. Had error {:?}",
                path, e
            )
        })
        .unwrap();
    let mut cache_data: CacheData = CacheData::new();
    for dir in meta_dirs {
        let name = dir_name(&dir);
        let replacements = read_meta_dir(&dir)
            .map_err(|e| panic!("Failed to read cache dir {:?}. Had error {:?}", dir, e))
            .unwrap();
        cache_data.insert(World::global().meta_index(&name), replacements);
    }
    cache_data
}

fn read_meta_dir(path: &PathBuf) -> io::Result<Vec<(Action, Vec<SASPlan>)>> {
    let macros: Vec<PathBuf> = dir_files_by_extension(path, "pddl")?;
    let plans: Vec<PathBuf> = dir_files_by_extension(path, "plan")?;

    let matched: Vec<(PathBuf, Vec<PathBuf>)> = match_files(macros, plans);
    let content: Vec<(String, Vec<String>)> = matched
        .into_iter()
        .map(|(action, plans)| {
            (
                fs::read_to_string(action).unwrap(),
                plans
                    .into_iter()
                    .map(|p| fs::read_to_string(p).unwrap())
                    .collect(),
            )
        })
        .collect();
    Ok(content
        .into_iter()
        .map(|(action, plans)| {
            (
                parse_action(&action[1..action.len() - 1]).unwrap().1,
                plans.into_iter().map(|p| parse_sas(&p).unwrap()).collect(),
            )
        })
        .collect())
}
