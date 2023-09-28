use std::ffi::OsString;

use nom::{
    branch::permutation, bytes::complete::tag, character::complete::char, combinator::opt,
    multi::many1, sequence::delimited, IResult,
};
use shared::io::file::read_file;

use crate::{
    domain::requirement::parse_requirements,
    shared::{remove_comments, spaced},
};

use self::{
    action::{parse_action, Actions},
    constants::parse_constants,
    name::parse_name,
    parameter::Parameters,
    predicate::{parse_predicates, Predicates},
    requirement::Requirements,
    types::{parse_types, Types},
};

pub mod action;
pub mod constants;
mod name;
pub mod parameter;
pub mod predicate;
pub mod requirement;
pub mod types;

#[derive(Debug, PartialEq, Clone)]
pub struct Domain {
    pub name: String,
    pub requirements: Option<Requirements>,
    pub types: Option<Types>,
    pub constants: Option<Parameters>,
    pub predicates: Predicates,
    pub actions: Actions,
}

fn parse_internal(input: &str) -> IResult<&str, Domain> {
    let (remaining, _) = spaced(tag("define"))(input)?;
    let (remaining, (name, requirements, types, constants, predicates, actions)) =
        permutation((
            spaced(delimited(char('('), parse_name, char(')'))),
            opt(spaced(delimited(char('('), parse_requirements, char(')')))),
            opt(spaced(delimited(char('('), parse_types, char(')')))),
            opt(spaced(delimited(char('('), parse_constants, char(')')))),
            spaced(delimited(char('('), parse_predicates, char(')'))),
            many1(spaced(delimited(char('('), parse_action, char(')')))),
        ))(remaining)?;
    Ok((
        remaining,
        Domain {
            name,
            requirements,
            types,
            constants,
            predicates,
            actions,
        },
    ))
}

pub fn parse_domain(input: &str) -> Result<Domain, String> {
    let clean = remove_comments(input);
    let (_, domain) = match delimited(spaced(char('(')), parse_internal, spaced(char(')')))(&clean)
    {
        Ok(it) => it,
        Err(err) => return Err(err.to_string()),
    };
    Ok(domain)
}

impl From<&OsString> for Domain {
    fn from(path: &OsString) -> Self {
        let content = read_file(&path.into());
        match parse_domain(&content) {
            Ok(domain) => domain,
            Err(err) => panic!(
                "Error while parsing domain \"{}\"\n{}",
                path.to_str().unwrap(),
                err
            ),
        }
    }
}

#[cfg(test)]
mod test {
    use crate::{
        domain::{
            action::{string_expression::StringExpression, Action},
            parameter::Parameter,
            parse_domain,
            predicate::Predicate,
            types::Type,
            Domain,
        },
        term::Term,
    };

    #[test]
    fn parse_dummy_domain() {
        assert_eq!(
            Ok(Domain {
                name: "name".to_string(),
                requirements: Some(vec!["strips".to_string(), "typing".to_string()]),
                types: Some(vec![
                    Type {
                        name: "object".to_string(),
                        sub_types: vec!["type1".to_string(), "type2".to_string()]
                    },
                    Type {
                        name: "type1".to_string(),
                        sub_types: vec!["subtype1".to_string()]
                    },
                ]),
                constants: None,
                predicates: vec![
                    Predicate {
                        name: "predicate1".to_string(),
                        parameters: vec![Parameter::Typed {
                            name: "a".to_string(),
                            type_name: "type1".to_string()
                        },]
                    },
                    Predicate {
                        name: "predicate2".to_string(),
                        parameters: vec![Parameter::Untyped {
                            name: "a".to_string(),
                        },]
                    }
                ],
                actions: vec![Action {
                    name: "action1".to_string(),
                    parameters: vec![Parameter::Typed {
                        name: "a".to_string(),
                        type_name: "type1".to_string()
                    }],
                    precondition: Some(StringExpression::And(vec![
                        StringExpression::Predicate(Term {
                            name: "predicate1".to_string(),
                            parameters: vec!["a".to_string()]
                        }),
                        StringExpression::Not(Box::new(StringExpression::Predicate(Term {
                            name: "predicate2".to_string(),
                            parameters: vec!["a".to_string()]
                        })))
                    ])),
                    effect: StringExpression::And(vec![
                        StringExpression::Predicate(Term {
                            name: "predicate1".to_string(),
                            parameters: vec!["a".to_string()]
                        }),
                        StringExpression::Predicate(Term {
                            name: "predicate2".to_string(),
                            parameters: vec!["a".to_string()]
                        })
                    ])
                }]
            }),
            parse_domain(
                "(define (domain name)
                     (:requirements :strips :typing)
                     (:types
                        type1 type2 - object
                        subtype1 - type1
                     )
                     (:predicates
                         (predicate1 ?a - type1)
                         (predicate2 ?a)
                     )

                    (:action action1
                        :parameters (?a - type1)
                        :precondition (and
                            (predicate1 ?a)
                            (not (predicate2 ?a))
                        )
                        :effect (and
                            (predicate1 ?a)
                            (predicate2 ?a)
                        )
                    )
             )",
            )
        );
    }
}
