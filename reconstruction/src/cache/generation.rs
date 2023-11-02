use std::path::PathBuf;

use clap::ValueEnum;

use crate::{
    cache::{hash_cache::HashCache, lifted_cache::LiftedCache, read_cache_input},
    state::instance::Instance,
    tools::time::run_time,
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
        println!("{} Reading cache data...", run_time());
        let data = read_cache_input(path).unwrap();
        println!("{} Generating cache...", run_time());
        match cache_type {
            CacheMethod::Grounded => Some(Box::new(GroundedCache::new(instance, data))),
            CacheMethod::Lifted => Some(Box::new(LiftedCache::new(instance, data))),
            CacheMethod::Hash => Some(Box::new(HashCache::new(instance, data))),
        }
    } else {
        println!("No cache given");
        return None;
    }
}
