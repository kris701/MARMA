use std::path::PathBuf;

use clap::ValueEnum;
use shared::time::run_time;
use state::instance::Instance;

use crate::{hash_cache::HashCache, lifted_cache::LiftedCache, read_cache_input, Cache};

#[derive(Debug, Copy, Clone, PartialEq, Default, ValueEnum)]
pub enum CacheMethod {
    #[default]
    Hash,
    Lifted,
}

pub fn generate_cache(
    instance: &Instance,
    cache_path: &Option<PathBuf>,
    cache_type: CacheMethod,
) -> Option<Box<dyn Cache>> {
    if let Some(path) = cache_path {
        println!("{} Reading cache data...", run_time());
        let data = read_cache_input(path).unwrap();
        println!("{} Generating cache...", run_time());
        match cache_type {
            CacheMethod::Hash => Some(Box::new(HashCache::new(instance, data))),
            CacheMethod::Lifted => Some(Box::new(LiftedCache::new(instance, data))),
        }
    } else {
        println!("No cache given");
        return None;
    }
}
