pub mod string_expression;
use self::string_expression::{parse_expression, StringExpression};

use nom::{
    branch::permutation, bytes::complete::tag, character::complete::char, combinator::opt,
    sequence::delimited, IResult,
};

use crate::shared::{named, spaced};

use super::parameter::{self, Parameters};

#[derive(Debug, PartialEq, Clone)]
pub struct Action {
    pub name: String,
    pub parameters: Parameters,
    pub precondition: Option<StringExpression>,
    pub effect: StringExpression,
}
pub type Actions = Vec<Action>;

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
