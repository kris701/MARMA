use std::{env, fs, time::Instant};

use parsing::domain::parse_domain;
use parsing::problem::parse_problem;

use crate::instance::Instance;

mod instance;

fn main() {
    let args: Vec<String> = env::args().collect();
    let domain_path = &args[1];
    let problem_path = &args[2];
    let domain_string = fs::read_to_string(domain_path).unwrap();
    let problem_string = fs::read_to_string(problem_path).unwrap();
    let parse_begin = Instant::now();
    let domain = parse_domain(&domain_string).unwrap();
    let problem = parse_problem(&problem_string).unwrap();
    println!("Parsing [{}ms]", parse_begin.elapsed().as_millis());
    let conversion_begin = Instant::now();
    let instance = Instance::new(&domain, &problem);
    println!("Conversion [{}ms]", conversion_begin.elapsed().as_millis());
}
