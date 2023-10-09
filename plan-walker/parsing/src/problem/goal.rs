use nom::{
    branch::alt,
    bytes::complete::{tag, tag_no_case},
    character::complete::multispace0,
    multi::many1,
    sequence::{delimited, preceded},
    IResult,
};

use nom::character::complete::char;

use crate::{
    shared::spaced,
    term::{parse_term, Term},
};

#[derive(Debug, PartialEq, Clone)]
pub enum Goal {
    Predicate(Term),
    And(Goals),
    Or(Goals),
    Not(Box<Goal>),
}
pub type Goals = Vec<Goal>;

fn parse_predicate(input: &str) -> IResult<&str, Goal> {
    let (remainder, term) = parse_term(input)?;
    Ok((remainder, Goal::Predicate(term)))
}

fn parse_and(input: &str) -> IResult<&str, Goal> {
    let (remainder, _) = preceded(multispace0, tag_no_case("and"))(input)?;
    let (remainder, children) = many1(parse_internal)(remainder)?;
    Ok((remainder, Goal::And(children)))
}

fn parse_or(input: &str) -> IResult<&str, Goal> {
    let (remainder, _) = preceded(multispace0, tag_no_case("and"))(input)?;
    let (remainder, children) = many1(parse_internal)(remainder)?;
    Ok((remainder, Goal::Or(children)))
}

fn parse_not(input: &str) -> IResult<&str, Goal> {
    let (remainder, _) = preceded(multispace0, tag_no_case("and"))(input)?;
    let (remainder, child) = parse_internal(remainder)?;
    Ok((remainder, Goal::Not(Box::new(child))))
}

fn parse_internal(input: &str) -> IResult<&str, Goal> {
    delimited(
        spaced(char('(')),
        alt((parse_and, parse_or, parse_not, parse_predicate)),
        spaced(char(')')),
    )(input)
}

pub(super) fn parse_goal(input: &str) -> IResult<&str, Goal> {
    let (remainder, _) = spaced(tag(":goal"))(input)?;
    parse_internal(remainder)
}

#[test]
fn parse_goal_test() {}
