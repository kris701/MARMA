use std::{collections::HashMap, time::Duration};

use crate::input_handling::Record;

fn calculate_score(time_used: &Duration, time_limit: &Duration) -> f64 {
    let time_used = time_used.as_secs_f64();
    if time_used < 1.0 {
        return 1.0;
    } else {
        let time_limit = time_limit.as_secs_f64();
        return 1.0 - (time_used.log2() / time_limit.log2());
    }
}

fn generate_scores(records: Vec<Record>, time_limit: &Duration) -> HashMap<String, f64> {
    let mut scores: HashMap<String, f64> = HashMap::new();
    for record in records.into_iter() {
        if !record.solved {
            continue;
        }
        if let Some(total_time) = record.total_time {
            let time_used = Duration::from_secs_f64(total_time);
            let score = calculate_score(&time_used, time_limit);

            match scores.get(&record.name) {
                Some(count) => scores.insert(record.name, count + score),
                None => scores.insert(record.name, score),
            };
        }
    }
    scores
}

pub fn generate_report(records: Vec<Record>, time_limit: &Duration) -> String {
    let scores = generate_scores(records, time_limit);
    let mut s = String::new();
    s.push_str("name,score\n");
    for (name, score) in scores.iter() {
        s.push_str(&format!("{},{:.2}\n", name, score));
    }
    s
}
