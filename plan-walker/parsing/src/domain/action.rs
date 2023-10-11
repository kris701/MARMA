pub mod string_expression;
use self::string_expression::{parse_expression, StringExpression};

use nom::{
    branch::permutation, bytes::complete::tag, character::complete::char, combinator::opt,
    sequence::delimited, IResult,
};

use crate::{
    domain::parameter::parameters_to_string,
    shared::{named, spaced},
};

use super::parameter::{self, Parameters};

#[derive(Debug, PartialEq, Clone)]
pub struct Action {
    pub name: String,
    pub parameters: Parameters,
    pub precondition: Option<StringExpression>,
    pub effect: StringExpression,
}
pub type Actions = Vec<Action>;

impl Action {
    fn precondition_to_string(&self) -> String {
        match &self.precondition {
            Some(precondition) => format!(":precondition {}", precondition.to_string()),
            None => "".to_string(),
        }
    }
    fn effect_to_string(&self) -> String {
        format!(":effect {}", self.effect.to_string()).to_string()
    }

    pub fn to_string(&self) -> String {
        format!(
            "(:action {}
    :parameters ({})
    {}
    {}
)",
            self.name,
            parameters_to_string(&self.parameters),
            self.precondition_to_string(),
            self.effect_to_string()
        )
    }
}

fn parse_name(input: &str) -> IResult<&str, String> {
    let (remainder, name) = spaced(named)(input)?;
    Ok((remainder, name.to_string()))
}

fn parse_parameters(input: &str) -> IResult<&str, Parameters> {
    let (remainder, _) = spaced(tag(":parameters"))(input)?;
    delimited(
        spaced(char('(')),
        parameter::parse_parameters,
        spaced(char(')')),
    )(remainder)
}

fn parse_precondition(input: &str) -> IResult<&str, StringExpression> {
    let (remainder, _) = spaced(tag(":precondition"))(input)?;
    parse_expression(remainder)
}

fn parse_effect(input: &str) -> IResult<&str, StringExpression> {
    let (remainder, _) = spaced(tag(":effect"))(input)?;
    parse_expression(remainder)
}

#[allow(dead_code)]
pub(super) fn parse_action(input: &str) -> IResult<&str, Action> {
    let (remainder, _) = spaced(tag(":action"))(input)?;
    let (remainder, (name, parameters, precondition, effect)) = permutation((
        parse_name,
        parse_parameters,
        opt(parse_precondition),
        parse_effect,
    ))(remainder)?;
    Ok((
        remainder,
        Action {
            name,
            parameters,
            precondition,
            effect,
        },
    ))
}

#[test]
fn test() {}
