use nom::{
    branch::alt,
    character::complete::{char, multispace0},
    combinator::opt,
    multi::{fold_many0, many1, separated_list1},
    sequence::{delimited, preceded, separated_pair},
    IResult,
};

use crate::shared::{named, spaced};

#[derive(Debug, PartialEq, Clone)]
pub enum Parameter {
    Untyped { name: String },
    Typed { name: String, type_name: String },
}

pub type Parameters = Vec<Parameter>;

fn parse_typed(input: &str) -> IResult<&str, Parameters> {
    let (remainder, parameters) = separated_pair(
        separated_list1(multispace0, preceded(opt(char('?')), named)),
        delimited(multispace0, char('-'), multispace0),
        named,
    )(input)?;
    Ok((
        remainder,
        parameters
            .0
            .iter()
            .map(|parameter| Parameter::Typed {
                name: parameter.to_string(),
                type_name: parameters.1.to_string(),
            })
            .collect(),
    ))
}

fn parse_untyped(input: &str) -> IResult<&str, Parameters> {
    let (remainder, parameters) = many1(preceded(multispace0, preceded(char('?'), named)))(input)?;
    Ok((
        remainder,
        parameters
            .iter()
            .map(|parameter| Parameter::Untyped {
                name: parameter.to_string(),
            })
            .collect(),
    ))
}

pub(super) fn parse_parameters(input: &str) -> IResult<&str, Parameters> {
    fold_many0(
        spaced(alt((parse_typed, parse_untyped))),
        Vec::new,
        |mut acc: Vec<_>, mut item| {
            acc.append(&mut item);
            acc
        },
    )(input)
}

#[test]
fn test() {
    assert_eq!(
        Ok((
            "",
            vec![Parameter::Untyped {
                name: "p".to_string()
            }]
        )),
        parse_parameters("?p")
    );
    assert_eq!(
        Ok((
            "",
            vec![
                Parameter::Untyped {
                    name: "p1".to_string()
                },
                Parameter::Untyped {
                    name: "p2".to_string()
                }
            ]
        )),
        parse_parameters("?p1 ?p2")
    );
    assert_eq!(
        Ok((
            "",
            vec![Parameter::Typed {
                name: "p".to_string(),
                type_name: "type".to_string()
            }]
        )),
        parse_parameters("?p - type")
    );
    assert_eq!(
        Ok((
            "",
            vec![
                Parameter::Typed {
                    name: "p1".to_string(),
                    type_name: "type".to_string()
                },
                Parameter::Typed {
                    name: "p2".to_string(),
                    type_name: "type".to_string()
                }
            ]
        )),
        parse_parameters("?p1 ?p2 - type")
    );
    assert_eq!(
        Ok((
            "",
            vec![
                Parameter::Typed {
                    name: "p1".to_string(),
                    type_name: "type".to_string()
                },
                Parameter::Typed {
                    name: "p2".to_string(),
                    type_name: "type".to_string()
                }
            ]
        )),
        parse_parameters("?p1 - type ?p2 - type")
    );
    assert_eq!(
        Ok((
            "",
            vec![
                Parameter::Typed {
                    name: "p1".to_string(),
                    type_name: "type1".to_string()
                },
                Parameter::Typed {
                    name: "p2".to_string(),
                    type_name: "type2".to_string()
                }
            ]
        )),
        parse_parameters("?p1 - type1 ?p2 - type2")
    );
    assert_eq!(
        Ok((
            "",
            vec![
                Parameter::Typed {
                    name: "p1".to_string(),
                    type_name: "type".to_string()
                },
                Parameter::Untyped {
                    name: "p2".to_string(),
                }
            ]
        )),
        parse_parameters("?p1 - type ?p2")
    );
}
