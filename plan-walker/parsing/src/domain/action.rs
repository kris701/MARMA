pub mod effect;
pub mod precondition;

use nom::{
    branch::permutation, bytes::complete::tag, character::complete::char, combinator::opt,
    sequence::delimited, IResult,
};

use crate::shared::{named, spaced};

use super::parameter::{self, Parameters};

#[allow(dead_code)]
#[derive(Debug, PartialEq)]
pub struct Action {
    pub name: String,
    pub parameters: Parameters,
    pub precondition: Option<precondition::Precondition>,
    pub effect: effect::Effect,
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

fn parse_precondition(input: &str) -> IResult<&str, precondition::Precondition> {
    let (remainder, _) = spaced(tag(":precondition"))(input)?;
    precondition::parse_precondition(remainder)
}

fn parse_effect(input: &str) -> IResult<&str, effect::Effect> {
    let (remainder, _) = spaced(tag(":effect"))(input)?;
    effect::parse_effect(remainder)
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
