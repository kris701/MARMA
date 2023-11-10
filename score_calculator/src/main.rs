use std::{collections::HashMap, error::Error, io, process};

use serde::Deserialize;

#[derive(Debug, Deserialize)]
struct Record {
    solved: bool,
    name: String,
    #[allow(dead_code)]
    domain: String,
    #[allow(dead_code)]
    problem: String,
    #[allow(dead_code)]
    #[serde(deserialize_with = "csv::invalid_option")]
    memory_used: Option<u64>,
    #[allow(dead_code)]
    #[serde(deserialize_with = "csv::invalid_option")]
    total_time: Option<f64>,
    #[allow(dead_code)]
    #[serde(deserialize_with = "csv::invalid_option")]
    search_time: Option<f64>,
}

fn read() -> Result<Vec<Record>, Box<dyn Error>> {
    let mut records = vec![];
    let mut rdr = csv::Reader::from_reader(io::stdin());
    for result in rdr.deserialize() {
        // Notice that we need to provide a type hint for automatic
        // deserialization.
        let record: Record = result?;
        records.push(record);
    }
    Ok(records)
}

fn generate_scores(records: Vec<Record>) {
    let mut scores: HashMap<String, f64> = HashMap::new();
    println!("name,score");
    for record in records.into_iter() {
        if !record.solved {
            continue;
        }
        let time = record.total_time.unwrap();
        let val = match time < 1.0 {
            true => 1.0,
            false => 1.0 - (time.log(2.0) / 1800f64.log(2.0)),
        };
        match scores.get(&record.name) {
            Some(count) => scores.insert(record.name, count + val),
            None => scores.insert(record.name, val),
        };
    }
    for (name, score) in scores.iter() {
        println!("{}, {}", name, score);
    }
}

fn main() {
    match read() {
        Ok(records) => {
            generate_scores(records);
            process::exit(0);
        }
        Err(err) => {
            println!("could not parse input: {}", err);
            process::exit(1);
        }
    }
}
