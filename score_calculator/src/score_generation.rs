use std::collections::HashMap;

use crate::input_handling::Record;

fn calculate_score(time_used: &f64, time_limit: &f64) -> f64 {
    if *time_used < 1.0 {
        return 1.0;
    } else {
        return 1.0 - (time_used.log2() / time_limit.log2());
    }
}

fn generate_scores(records: Vec<Record>, time_limit: &f64) -> HashMap<String, f64> {
    let mut scores: HashMap<String, f64> = HashMap::new();
    for record in records.into_iter() {
        if !record.solved {
            continue;
        }
        if let Some(time_used) = record.total_time {
            let score = calculate_score(&time_used, time_limit);

            match scores.get(&record.name) {
                Some(count) => scores.insert(record.name, count + score),
                None => scores.insert(record.name, score),
            };
        }
    }
    scores
}

pub fn generate_report(records: Vec<Record>, time_limit: &f64) -> String {
    let scores = generate_scores(records, time_limit);
    let mut s = String::new();
    s.push_str("name,score\n");
    for (name, score) in scores.iter() {
        s.push_str(&format!("{},{:.2}\n", name, score));
    }
    s
}
