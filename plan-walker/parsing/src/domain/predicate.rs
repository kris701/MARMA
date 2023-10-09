use nom::{
    bytes::complete::tag,
    character::complete::{char, multispace0},
    multi::many1,
    sequence::{delimited, preceded},
    IResult,
};

use crate::shared::{named, spaced};

use super::parameter::{parse_parameters, Parameters};

#[derive(Debug, PartialEq, Clone)]
pub struct Predicate {
    pub name: String,
    pub parameters: Parameters,
}
pub type Predicates = Vec<Predicate>;

pub fn parse_predicate(input: &str) -> IResult<&str, Predicate> {
    let (remainder, name) = preceded(multispace0, named)(input)?;
    let (remainder, parameters) = parse_parameters(remainder)?;
    Ok((
        remainder,
        Predicate {
            name: name.to_string(),
            parameters,
        },
    ))
}

#[allow(dead_code)]
pub(super) fn parse_predicates(input: &str) -> IResult<&str, Predicates> {
    let (remainder, _) = preceded(multispace0, tag(":predicates"))(input)?;
    let (remainder, predicates) = many1(delimited(
        spaced(char('(')),
        parse_predicate,
        spaced(char(')')),
    ))(remainder)?;
    Ok((remainder, predicates))
}

#[cfg(test)]
mod test {
    use crate::domain::{
        parameter::Parameter,
        predicate::{parse_predicates, Predicate},
    };

    #[test]
    fn test() {
        assert_eq!(
            Ok((
                "",
                vec![Predicate {
                    name: "predicate".to_string(),
                    parameters: vec![Parameter::Untyped {
                        name: "p".to_string()
                    }]
                }]
            )),
            parse_predicates(":predicates (predicate ?p)")
        );
        assert_eq!(
            Ok((
                "",
                vec![Predicate {
                    name: "predicate".to_string(),
                    parameters: vec![Parameter::Typed {
                        name: "p".to_string(),
                        type_name: "type".to_string()
                    }]
                }]
            )),
            parse_predicates(":predicates (predicate ?p - type)")
        );
        assert_eq!(
            Ok((
                "",
                vec![Predicate {
                    name: "predicate".to_string(),
                    parameters: vec![
                        Parameter::Typed {
                            name: "p1".to_string(),
                            type_name: "type".to_string()
                        },
                        Parameter::Typed {
                            name: "p2".to_string(),
                            type_name: "type".to_string()
                        },
                    ]
                }]
            )),
            parse_predicates(":predicates (predicate ?p1 ?p2 - type)")
        );
        assert_eq!(
            Ok((
                "",
                vec![Predicate {
                    name: "predicate".to_string(),
                    parameters: vec![
                        Parameter::Typed {
                            name: "p1".to_string(),
                            type_name: "type1".to_string()
                        },
                        Parameter::Typed {
                            name: "p2".to_string(),
                            type_name: "type2".to_string()
                        },
                    ]
                }]
            )),
            parse_predicates(":predicates (predicate ?p1 - type1 ?p2 - type2)")
        );
        assert_eq!(
            Ok((
                "",
                vec![Predicate {
                    name: "predicate".to_string(),
                    parameters: vec![
                        Parameter::Typed {
                            name: "p1".to_string(),
                            type_name: "type".to_string()
                        },
                        Parameter::Untyped {
                            name: "p2".to_string(),
                        },
                    ]
                }]
            )),
            parse_predicates(":predicates (predicate ?p1 - type ?p2)")
        );
    }
}
