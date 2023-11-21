use std::{
    collections::{HashMap, HashSet},
    path::PathBuf,
};

use clap::ValueEnum;
use spingus::sas_plan::SASPlan;

use super::{cache_data::read_cache, hash::HashCache, lifted::LiftedCache, Cache};
use crate::{
    tools::{status_print, Status},
    world::World,
};

#[derive(Debug, Copy, Clone, PartialEq, Default, ValueEnum)]
pub enum CacheMethod {
    #[default]
    Lifted,
    Hash,
    None,
}

fn find_used_meta_actions(meta_plan: &SASPlan) -> HashMap<usize, HashSet<Vec<usize>>> {
    let mut used: HashMap<usize, HashSet<Vec<usize>>> = HashMap::new();
    for step in meta_plan
        .iter()
        .filter(|t| World::global().is_meta_action(&t.name))
    {
        let meta_index = World::global().meta_index(&step.name);
        let parameters = World::global().objects.indexes(&step.parameters);
        used.entry(meta_index).or_default().insert(parameters);
    }
    used
}

pub fn generate_cache(
    meta_plan: &SASPlan,
    cache_path: &Option<PathBuf>,
    cache_type: CacheMethod,
    iterative_cache: bool,
) -> Option<Box<dyn Cache>> {
    if cache_type == CacheMethod::None || (cache_path.is_none() && !iterative_cache) {
        return None;
    }
    let data = cache_path
        .to_owned()
        .map_or(HashMap::new(), |p| read_cache(&p));
    status_print(Status::Cache, "Reading cache");
    let used_meta_actions = find_used_meta_actions(meta_plan);
    match cache_type {
        CacheMethod::Lifted => Some(Box::new(LiftedCache::new(data, used_meta_actions))),
        CacheMethod::Hash => Some(Box::new(HashCache::new(data, used_meta_actions))),
        _ => panic!(),
    }
}
