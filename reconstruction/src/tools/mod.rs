use std::path::{Path, PathBuf};

use indicatif::{ProgressBar, ProgressStyle};
use rand::{distributions::Alphanumeric, Rng};

use crate::tools::{memory::memory_usage, time::run_time};

pub mod io;
pub mod memory;
pub mod time;
pub mod val;

pub fn random_name() -> String {
    rand::thread_rng()
        .sample_iter(&Alphanumeric)
        .take(32)
        .map(char::from)
        .collect()
}

pub fn random_file_name(dir: &PathBuf) -> String {
    let name = random_name();
    Path::new(&dir).join(name).to_str().unwrap().to_string()
}

pub fn generate_progressbar(limit: usize) -> ProgressBar {
    let bar_style = ProgressStyle::with_template("{msg} {bar:40} {pos:>4}/{len:4}").unwrap();
    let bar = ProgressBar::new(limit as u64);
    bar.set_style(bar_style);
    bar
}

pub fn statbar() -> String {
    if let Some(mem) = memory_usage() {
        format!("[{}, {}]", run_time(), mem)
    } else {
        format!("[{}, N/A]", run_time())
    }
}

pub enum Status {
    Init,
    Cache,
    Reconstruction,
    Validation,
    Report,
}

pub fn status_print(status: Status, input: &str) {
    let status = match status {
        Status::Init => "INIT",
        Status::Cache => "CACHE",
        Status::Reconstruction => "RECONSTRUCTION",
        Status::Validation => "VALIDATION",
        Status::Report => "REPORT",
    };
    println!("{} [{}] {}", statbar(), status, input);
}
