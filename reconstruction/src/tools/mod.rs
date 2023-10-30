use std::path::{Path, PathBuf};

use indicatif::{ProgressBar, ProgressStyle};
use rand::{distributions::Alphanumeric, Rng};

pub mod io;
pub mod time;

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
    let bar_style =
        ProgressStyle::with_template("[{elapsed_precise}] {bar:40} {pos:>4}/{len:4} {msg}")
            .unwrap();
    let bar = ProgressBar::new(limit as u64);
    bar.set_style(bar_style);
    bar
}
