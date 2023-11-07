use memory_stats::memory_stats;

pub fn memory_usage() -> Option<String> {
    if let Some(usage) = memory_stats() {
        let usage = usage.physical_mem;
        if usage < 1000 {
            return Some(format!("{: >2}B", usage));
        } else if usage < 1000 * 1000 {
            return Some(format!("{: >3}KB", usage / (1000)));
        } else if usage < 1000 * 1000 * 1000 {
            return Some(format!("{: >3}MB", usage / (1000 * 1000)));
        } else {
            return Some(format!("{: >3}GB", usage / (1000 * 1000 * 1000)));
        }
    } else {
        None
    }
}
