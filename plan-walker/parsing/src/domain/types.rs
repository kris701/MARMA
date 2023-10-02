use nom::{
    branch::alt,
    bytes::complete::tag,
    character::complete::{char, multispace0, multispace1},
    multi::{many1, separated_list1},
    sequence::{delimited, preceded, separated_pair},
    IResult,
};

use crate::shared::{named, spaced};

#[derive(Debug, PartialEq, Clone)]
pub struct Type {
    pub name: String,
    pub sub_types: Vec<String>,
}
pub type Types = Vec<Type>;

fn parse_without_subtypes(input: &str) -> IResult<&str, Type> {
    let (remainder, name) = spaced(named)(input)?;
    Ok((
        remainder,
        Type {
            name: name.to_owned(),
            sub_types: vec![],
        },
    ))
}

fn parse_with_subtypes(input: &str) -> IResult<&str, Type> {
    let (remainder, (sub_types, name)) = separated_pair(
        separated_list1(multispace0, named),
        delimited(multispace1, char('-'), multispace1),
        named,
    )(input)?;

    Ok((
        remainder,
        Type {
            name: name.to_owned(),
            sub_types: sub_types
                .iter()
                .map(|sub_type| sub_type.to_string())
                .collect(),
        },
    ))
}

fn parse_type(input: &str) -> IResult<&str, Type> {
    alt((parse_with_subtypes, parse_without_subtypes))(input)
}

#[allow(dead_code)]
pub(super) fn parse_types(input: &str) -> IResult<&str, Vec<Type>> {
    let (remainder, _) = preceded(multispace0, tag(":types"))(input)?;
    let (remainder, mut types) = many1(spaced(parse_type))(remainder)?;
    // Check if same type occurs multiple times
    // If so combine
    // ---
    // Happens in cases such as this
    // (:types
    //      a - Object
    //      b - Object
    //      ...
    // )
    for i in 0..types.len() {
        for t in i + 1..types.len() {
            if types[i].name == types[t].name {
                let mut o = types.remove(t);
                types[i].sub_types.append(&mut o.sub_types);
            }
        }
    }

    Ok((remainder, types))
}

#[test]
fn test() {
    assert_eq!(
        Ok((
            "",
            vec![Type {
                name: "object".to_string(),
                sub_types: vec![]
            }]
        )),
        parse_types(":types Object")
    );
    assert_eq!(
        Ok((
            "",
            vec![Type {
                name: "object".to_string(),
                sub_types: vec!["type1".to_string()]
            }]
        )),
        parse_types(":types type1 - Object")
    );
    assert_eq!(
        Ok((
            "",
            vec![Type {
                name: "object".to_string(),
                sub_types: vec!["type1".to_string(), "type2".to_string()]
            }]
        )),
        parse_types(":types type1 type2 - Object")
    );
    assert_eq!(
        Ok((
            "",
            vec![
                Type {
                    name: "objects".to_string(),
                    sub_types: vec!["type1".to_string(), "type2".to_string()]
                },
                Type {
                    name: "type1".to_string(),
                    sub_types: vec!["subtype1a".to_string(), "subtype2a".to_string()]
                },
                Type {
                    name: "type2".to_string(),
                    sub_types: vec!["subtype1b".to_string(), "subtype2b".to_string()]
                }
            ]
        )),
        parse_types(
            ":types type1 type2 - Objects subtype1a subtype2a - type1 subtype1b subtype2b - type2"
        )
    );
    assert_eq!(
        Ok((
            "",
            vec![Type {
                name: "object".to_string(),
                sub_types: vec!["type1".to_string(), "type2".to_string()]
            }]
        )),
        parse_types(":types type1 - Object type2 - Object")
    );
    assert_eq!(
        Ok((
            "",
            vec![
                Type {
                    name: "object".to_string(),
                    sub_types: vec!["type-1".to_string()]
                },
                Type {
                    name: "type-1".to_string(),
                    sub_types: vec!["subtype".to_string()]
                }
            ]
        )),
        parse_types(":types type-1 - Object subtype - type-1")
    );
}
