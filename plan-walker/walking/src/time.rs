use std::{
    sync::atomic::{AtomicUsize, Ordering},
    time::{Instant, SystemTime},
};

use std::time::UNIX_EPOCH;

static START_TIME: AtomicUsize = AtomicUsize::new(0);

fn time() -> usize {
    SystemTime::now()
        .duration_since(UNIX_EPOCH)
        .unwrap()
        .as_millis() as usize
}

pub fn init_time() {
    START_TIME.store(time(), Ordering::SeqCst);
}

pub fn run_time() -> String {
    let start_time = START_TIME.load(Ordering::SeqCst);
    let curr_time = time();
    let elapsed = curr_time - start_time;
    match elapsed {
        0..=9999 => format!("[{:4}ms]", elapsed),
        _ => format!("[{:4}s]", elapsed / 1000),
    }
}
