use std::fs;

use parsing::problem;

use rstest::*;

#[rstest]
#[case("gripper")]
#[case("logistics")]
#[case("movie")]
#[case("mystery")]
#[case("blocks-typed")]
#[case("blocks-untyped")]
#[case("elevator-typed")]
#[case("elevator-untyped")]
#[case("freecell-typed")]
#[case("freecell-untyped")]
#[case("logistics-typed")]
#[case("logistics-untyped")]
#[case("satellite")]
#[case("driverlog-automatic")]
#[case("driverlog-hand-coded")]
#[case("barman-agile")]
#[case("barman-satisficing")]
#[case("grid")]
#[case("child-snack-agile")]
#[case("child-snack-satisficing")]
#[case("hiking-sequential-agile")]
fn parse_problem(#[case] domain_name: &str) {
    let problem_path = format!("tests/data/{}/instances/", domain_name);
    let files = fs::read_dir(problem_path).unwrap();
    for file in files {
        if let Ok(content) = fs::read_to_string(file.unwrap().path()) {
            let parse_result = problem::parse_problem(&content);
            if let Ok(problem) = parse_result {
                assert!(!problem.name.is_empty());
            } else if let Err(err) = parse_result {
                panic!(
                    "Could not parse problem: \"{}\".\nWith error: \"{}\"",
                    domain_name, err
                );
            }
        }
    }
}
