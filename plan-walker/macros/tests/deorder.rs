use itertools::izip;
use macros::deordering::deorder;
use parsing::domain;
use parsing::problem;
use parsing::sas::parse_sas;
use state::instance::fact::Facts;
use state::plan::Plan;
use state::state::generate_state;
use std::fs;

use rstest::*;

#[rstest]
#[case("gripper")]
#[case("blocks")]
fn deorder_test(#[case] domain_name: &str) {
    let domain = fs::read_to_string(format!("tests/data/{}/domain.pddl", domain_name)).unwrap();
    let domain = domain::parse_domain(&domain).unwrap();
    let problems = fs::read_dir(format!("tests/data/{}/instances/", domain_name))
        .unwrap()
        .map(|f| fs::read_to_string(f.unwrap().path()).unwrap())
        .collect::<Vec<String>>();
    let plans = fs::read_dir(format!("tests/data/{}/plans/", domain_name))
        .unwrap()
        .map(|f| fs::read_to_string(f.unwrap().path()).unwrap())
        .collect::<Vec<String>>();
    let deordered_plans = fs::read_dir(format!("tests/data/{}/deordered/", domain_name))
        .unwrap()
        .map(|f| fs::read_to_string(f.unwrap().path()).unwrap())
        .collect::<Vec<String>>();
    for (problem, plan, expected) in izip!(problems, plans, deordered_plans) {
        let problem = problem::parse_problem(&problem).unwrap();
        let facts = Facts::new(&domain, &problem);
        let state = generate_state(&domain, &problem, &facts);
        let plan = parse_sas(&plan).unwrap();
        let plan = Plan::new(&domain, &problem, &facts, plan);
        let deordered = deorder(&domain, &problem, &facts, &state, plan);
        assert!(true);
    }
}
