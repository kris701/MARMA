use std::path::PathBuf;

use clap::ValueEnum;
use shared::time::run_time;
use state::instance::Instance;

use crate::{hash_cache::HashCache, Cache};

#[derive(Debug, Copy, Clone, PartialEq, Default, ValueEnum)]
pub enum CacheMethod {
    #[default]
    Hash,
}

pub fn generate_cache(
    instance: &Instance,
    cache_path: &Option<PathBuf>,
    cache_type: CacheMethod,
) -> Option<impl Cache> {
    if let Some(path) = cache_path {
        println!("{} Generating cache...", run_time());
        match cache_type {
            CacheMethod::Hash => Some(HashCache::init(instance, path)),
        }
    } else {
        println!("No cache given");
        return None;
    }
}
