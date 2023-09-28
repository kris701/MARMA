use nom::{
    branch::alt,
    bytes::complete::tag,
    character::complete::multispace0,
    combinator::opt,
    multi::{fold_many1, many1, separated_list1},
    sequence::{delimited, preceded, separated_pair},
    IResult,
};

use crate::shared::{named, spaced};
use nom::character::complete::char;

#[derive(Debug, PartialEq, Clone)]
pub struct Object {
    pub name: String,
    pub type_name: Option<String>,
}
pub type Objects = Vec<Object>;

fn parse_untyped(input: &str) -> IResult<&str, Objects> {
    let (remainder, objects) = many1(spaced(named))(input)?;
    Ok((
        remainder,
        objects
            .iter()
            .map(|e| Object {
                name: e.to_string(),
                type_name: None,
            })
            .collect(),
    ))
}

fn parse_typed(input: &str) -> IResult<&str, Objects> {
    let (remainder, (objects, type_name)) = separated_pair(
        separated_list1(multispace0, preceded(opt(char('?')), named)),
        delimited(multispace0, char('-'), multispace0),
        named,
    )(input)?;
    Ok((
        remainder,
        objects
            .iter()
            .map(|e| Object {
                name: e.to_string(),
                type_name: Some(type_name.to_string()),
            })
            .collect(),
    ))
}

fn parse_internal(input: &str) -> IResult<&str, Objects> {
    alt((parse_typed, parse_untyped))(input)
}

pub(super) fn parse_objects(input: &str) -> IResult<&str, Objects> {
    let (remainder, _) = preceded(multispace0, tag(":objects"))(input)?;
    fold_many1(
        spaced(parse_internal),
        Vec::new,
        |mut acc: Vec<_>, mut item| {
            acc.append(&mut item);
            acc
        },
    )(remainder)
}

#[test]
fn test() {
    assert_eq!(
        Ok((
            "",
            vec![Object {
                name: "object".to_string(),
                type_name: None
            }]
        )),
        parse_objects(":objects object")
    );
    assert_eq!(
        Ok((
            "",
            vec![
                Object {
                    name: "object1".to_string(),
                    type_name: None
                },
                Object {
                    name: "object2".to_string(),
                    type_name: None
                }
            ]
        )),
        parse_objects(":objects object1 object2")
    );
    assert_eq!(
        Ok((
            "",
            vec![Object {
                name: "object".to_string(),
                type_name: Some("type".to_string())
            }]
        )),
        parse_objects(":objects object - type")
    );
    assert_eq!(
        Ok((
            "",
            vec![
                Object {
                    name: "object1".to_string(),
                    type_name: Some("type".to_string())
                },
                Object {
                    name: "object2".to_string(),
                    type_name: Some("type".to_string())
                }
            ]
        )),
        parse_objects(":objects object1 object2 - type")
    );
    assert_eq!(
        Ok((
            "",
            vec![
                Object {
                    name: "object1".to_string(),
                    type_name: Some("type1".to_string())
                },
                Object {
                    name: "object2".to_string(),
                    type_name: Some("type2".to_string())
                }
            ]
        )),
        parse_objects(":objects object1 - type1 object2 - type2")
    );
    assert_eq!(
        Ok((
            "",
            vec![
                Object {
                    name: "object1".to_string(),
                    type_name: Some("type1".to_string())
                },
                Object {
                    name: "object2".to_string(),
                    type_name: Some("type2".to_string())
                },
                Object {
                    name: "object3".to_string(),
                    type_name: None
                }
            ]
        )),
        parse_objects(":objects object1 - type1 object2 - type2 object3")
    );
}
