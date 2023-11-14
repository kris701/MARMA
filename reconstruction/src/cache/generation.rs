use std::path::PathBuf;

use clap::ValueEnum;
use spingus::sas_plan::SASPlan;

use super::{cache_data::read_cache, lifted::LiftedCache, Cache};
use crate::{
    cache::hash_cache::HashCache,
    instance::Instance,
    tools::{status_print, Status},
    world::World,
};

#[derive(Debug, Copy, Clone, PartialEq, Default, ValueEnum)]
pub enum CacheMethod {
    #[default]
    Hash,
    Lifted,
    None,
}

fn find_used_meta_actions(meta_plan: &SASPlan) -> Vec<(u16, Vec<u16>)> {
    meta_plan
        .iter()
        .filter_map(|s| match World::global().is_meta_action(&s.name) {
            true => {
                let meta_index = World::global().get_meta_index(&s.name);
                let parameters = World::global().get_object_indexes(&s.parameters);
                Some((meta_index, parameters))
            }
            false => None,
        })
        .collect()
}

pub fn generate_cache(
    instance: &Instance,
    meta_plan: &SASPlan,
    cache_path: &Option<PathBuf>,
    cache_type: CacheMethod,
) -> Option<Box<dyn Cache>> {
    if let Some(path) = cache_path {
        status_print(Status::Cache, "Reading cache");
        let data = read_cache(path);
        let used_meta_actions = find_used_meta_actions(meta_plan);
        match cache_type {
            CacheMethod::Hash => Some(Box::new(HashCache::new(instance, data, used_meta_actions))),
            CacheMethod::Lifted => Some(Box::new(LiftedCache::new(
                instance,
                data,
                used_meta_actions,
            ))),
            CacheMethod::None => None,
        }
    } else {
        return None;
    }
}
