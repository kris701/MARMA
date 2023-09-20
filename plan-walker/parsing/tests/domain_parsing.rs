use std::fs;

use parsing::domain;

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
#[case("driverlog")]
#[case("barman")]
#[case("grid")]
#[case("child-snack")]
#[case("hiking")]
fn parse_domain(#[case] domain_name: &str) {
    if let Ok(str) = fs::read_to_string(format!("tests/data/{}/domain.pddl", domain_name)) {
        let parse_result = domain::parse_domain(&str);
        if let Ok(dom) = parse_result {
            assert!(!dom.name.is_empty());
        } else if let Err(err) = parse_result {
            panic!(
                "Could not parse domain: \"{}\".\nWith error: \"{}\"",
                domain_name, err
            );
        }
    } else {
        panic!("Could not open");
    }
}
