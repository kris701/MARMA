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

#[derive(Debug, PartialEq, Clone)]
pub enum StringExpression {
    Predicate(Term),
    Equal(StringExpressions),
    And(StringExpressions),
    Or(StringExpressions),
    Not(Box<StringExpression>),
}
pub type StringExpressions = Vec<StringExpression>;

fn parse_predicate(input: &str) -> IResult<&str, StringExpression> {
    let (remainder, term) = parse_term(input)?;
    Ok((remainder, StringExpression::Predicate(term)))
}

fn parse_equal(input: &str) -> IResult<&str, StringExpression> {
    let (remainder, _) = preceded(multispace0, tag_no_case("="))(input)?;
    let (remainder, children) = many1(parse_expression)(remainder)?;
    Ok((remainder, StringExpression::Equal(children)))
}

fn parse_and(input: &str) -> IResult<&str, StringExpression> {
    let (remainder, _) = preceded(multispace0, tag_no_case("and"))(input)?;
    let (remainder, children) = many1(parse_expression)(remainder)?;
    Ok((remainder, StringExpression::And(children)))
}

fn parse_or(input: &str) -> IResult<&str, StringExpression> {
    let (remainder, _) = preceded(multispace0, tag_no_case("or"))(input)?;
    let (remainder, children) = many1(parse_expression)(remainder)?;
    Ok((remainder, StringExpression::Or(children)))
}

fn parse_not(input: &str) -> IResult<&str, StringExpression> {
    let (remainder, _) = preceded(multispace0, tag_no_case("not"))(input)?;
    let (remainder, child) = parse_expression(remainder)?;
    Ok((remainder, StringExpression::Not(Box::new(child))))
}

pub(super) fn parse_expression(input: &str) -> IResult<&str, StringExpression> {
    delimited(
        spaced(char('(')),
        alt((parse_and, parse_or, parse_not, parse_equal, parse_predicate)),
        spaced(char(')')),
    )(input)
}

#[test]
fn test() {
    assert_eq!(
        Ok((
            "",
            StringExpression::Predicate(Term {
                name: "predicate".to_string(),
                parameters: vec![]
            })
        )),
        parse_expression("(predicate)")
    );
    assert_eq!(
        Ok((
            "",
            StringExpression::Predicate(Term {
                name: "predicate".to_string(),
                parameters: vec!["a".to_string()]
            })
        )),
        parse_expression("(predicate ?a)")
    );
    assert_eq!(
        Ok((
            "",
            StringExpression::Predicate(Term {
                name: "predicate".to_string(),
                parameters: vec!["a".to_string(), "b".to_string()]
            })
        )),
        parse_expression("(predicate ?a ?b)")
    );
    assert_eq!(
        Ok((
            "",
            StringExpression::Not(Box::new(StringExpression::Predicate(Term {
                name: "predicate".to_string(),
                parameters: vec!["a".to_string()]
            })))
        )),
        parse_expression("(not (predicate ?a))")
    );
    assert_eq!(
        Ok((
            "",
            StringExpression::And(vec![StringExpression::Predicate(Term {
                name: "predicate".to_string(),
                parameters: vec!["a".to_string()]
            })])
        )),
        parse_expression("(and (predicate ?a))")
    );
    assert_eq!(
        Ok((
            "",
            StringExpression::And(vec![
                StringExpression::Predicate(Term {
                    name: "predicate".to_string(),
                    parameters: vec!["a".to_string()]
                }),
                StringExpression::Predicate(Term {
                    name: "predicate".to_string(),
                    parameters: vec!["b".to_string()]
                })
            ])
        )),
        parse_expression("(and (predicate ?a) (predicate ?b))")
    );
    assert_eq!(
        Ok((
            "",
            StringExpression::Or(vec![StringExpression::Predicate(Term {
                name: "predicate".to_string(),
                parameters: vec!["a".to_string()]
            })])
        )),
        parse_expression("(or (predicate ?a))")
    );
    assert_eq!(
        Ok((
            "",
            StringExpression::Or(vec![
                StringExpression::Predicate(Term {
                    name: "predicate".to_string(),
                    parameters: vec!["a".to_string()]
                }),
                StringExpression::Predicate(Term {
                    name: "predicate".to_string(),
                    parameters: vec!["b".to_string()]
                }),
            ])
        )),
        parse_expression("(or (predicate ?a) (predicate ?b))")
    );
    assert_eq!(
        Ok((
            "",
            StringExpression::Equal(vec![
                StringExpression::Predicate(Term {
                    name: "predicate".to_string(),
                    parameters: vec!["a".to_string()]
                }),
                StringExpression::Predicate(Term {
                    name: "predicate".to_string(),
                    parameters: vec!["b".to_string()]
                }),
            ])
        )),
        parse_expression("(= (predicate ?a) (predicate ?b))")
    );
}
