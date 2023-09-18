use nom::{
    branch::alt,
    bytes::complete::tag_no_case,
    character::complete::{char, multispace0},
    multi::many1,
    sequence::{delimited, preceded},
    IResult,
};

use crate::{
    shared::spaced,
    term::{parse_term, Term},
};

#[derive(Debug, PartialEq)]
pub enum Effect {
    Predicate(Term),
    And(Effects),
    Not(Box<Effect>),
}
pub type Effects = Vec<Effect>;

fn parse_predicate(input: &str) -> IResult<&str, Effect> {
    let (remainder, term) = parse_term(input)?;
    Ok((remainder, Effect::Predicate(term)))
}

fn parse_and(input: &str) -> IResult<&str, Effect> {
    let (remainder, _) = preceded(multispace0, tag_no_case("and"))(input)?;
    let (remainder, children) = many1(parse_effect)(remainder)?;
    Ok((remainder, Effect::And(children)))
}

fn parse_not(input: &str) -> IResult<&str, Effect> {
    let (remainder, _) = preceded(multispace0, tag_no_case("not"))(input)?;
    let (remainder, child) = parse_effect(remainder)?;
    Ok((remainder, Effect::Not(Box::new(child))))
}

pub(super) fn parse_effect(input: &str) -> IResult<&str, Effect> {
    delimited(
        spaced(char('(')),
        alt((parse_and, parse_not, parse_predicate)),
        spaced(char(')')),
    )(input)
}

#[test]
fn test() {
    assert_eq!(
        Ok((
            "",
            Effect::Predicate(Term {
                name: "predicate".to_string(),
                parameters: vec!["a".to_string()]
            })
        )),
        parse_effect("(predicate ?a)")
    );
    assert_eq!(
        Ok((
            "",
            Effect::Predicate(Term {
                name: "predicate".to_string(),
                parameters: vec!["a".to_string(), "b".to_string()]
            })
        )),
        parse_effect("(predicate ?a ?b)")
    );
    assert_eq!(
        Ok((
            "",
            Effect::And(vec![Effect::Predicate(Term {
                name: "predicate".to_string(),
                parameters: vec!["a".to_string()]
            })])
        )),
        parse_effect("(and (predicate ?a))")
    );
    assert_eq!(
        Ok((
            "",
            Effect::And(vec![
                Effect::Predicate(Term {
                    name: "predicate1".to_string(),
                    parameters: vec!["a".to_string()]
                }),
                Effect::Predicate(Term {
                    name: "predicate2".to_string(),
                    parameters: vec!["b".to_string()]
                }),
            ])
        )),
        parse_effect("(and (predicate1 ?a) (predicate2 ?b))")
    );
    assert_eq!(
        Ok((
            "",
            Effect::Not(Box::new(Effect::Predicate(Term {
                name: "predicate".to_string(),
                parameters: vec!["a".to_string()]
            })))
        )),
        parse_effect("(not (predicate ?a))")
    );
}
