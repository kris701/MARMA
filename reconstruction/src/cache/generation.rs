use std::path::PathBuf;

use clap::ValueEnum;

use super::{cache_data::read_cache, lifted::LiftedCache, Cache};
use crate::{
    cache::hash_cache::HashCache,
    instance::Instance,
    tools::{status_print, Status},
};

#[derive(Debug, Copy, Clone, PartialEq, Default, ValueEnum)]
pub enum CacheMethod {
    #[default]
    Hash,
    Lifted,
    None
}

pub fn generate_cache(
    instance: &Instance,
    cache_path: &Option<PathBuf>,
    cache_type: CacheMethod,
) -> Option<Box<dyn Cache>> {
    if let Some(path) = cache_path {
        status_print(Status::Cache, "Reading cache");
        let data = read_cache(path);
        match cache_type {
            CacheMethod::Hash => Some(Box::new(HashCache::new(instance, data))),
            CacheMethod::Lifted => Some(Box::new(LiftedCache::new(instance, data))),
            CacheMethod::None => None,
        }
    } else {
        return None;
    }
}
