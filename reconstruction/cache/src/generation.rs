use std::path::PathBuf;

use clap::ValueEnum;
use shared::time::run_time;
use state::instance::Instance;

use crate::{bit_cache::BitCache, Cache};

#[derive(Debug, Copy, Clone, PartialEq, Default, ValueEnum)]
pub enum CacheMethod {
    #[default]
    BitExp,
}

pub fn generate_cache(
    instance: &Instance,
    cache_path: &Option<PathBuf>,
    cache_type: CacheMethod,
) -> Option<impl Cache> {
    if let Some(path) = cache_path {
        println!("{} Generating cache...", run_time());
        match cache_type {
            CacheMethod::BitExp => Some(BitCache::init(instance, path)),
        }
    } else {
        println!("No cache given");
        return None;
    }
}
