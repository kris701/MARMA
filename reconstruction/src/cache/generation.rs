use std::path::PathBuf;

use clap::ValueEnum;

use crate::{
    cache::{hash_cache::HashCache, lifted_cache::LiftedCache, read_cache_input},
    instance::Instance,
    tools::{status_print, time::run_time, Status},
};

use super::{grounded_cache::GroundedCache, Cache};

#[derive(Debug, Copy, Clone, PartialEq, Default, ValueEnum)]
pub enum CacheMethod {
    #[default]
    Grounded,
    Lifted,
    Hash,
}

pub fn generate_cache(
    instance: &Instance,
    cache_path: &Option<PathBuf>,
    cache_type: CacheMethod,
) -> Option<Box<dyn Cache>> {
    if let Some(path) = cache_path {
        status_print(Status::Cache, "Reading cache");
        let data = read_cache_input(path).unwrap();
        status_print(Status::Cache, "Generating cache");
        match cache_type {
            CacheMethod::Grounded => Some(Box::new(GroundedCache::new(instance, data))),
            CacheMethod::Lifted => Some(Box::new(LiftedCache::new(instance, data))),
            CacheMethod::Hash => Some(Box::new(HashCache::new(instance, data))),
        }
    } else {
        return None;
    }
}
