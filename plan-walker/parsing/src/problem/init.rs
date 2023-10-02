use nom::character::complete::char;
use nom::multi::many1;
use nom::sequence::delimited;
use nom::{bytes::complete::tag_no_case, IResult};

use crate::{
    shared::spaced,
    term::{parse_term, Term},
};

pub type Init = Term;
pub type Inits = Vec<Init>;

fn parse_init(input: &str) -> IResult<&str, Init> {
    parse_term(input)
}

pub(super) fn parse_inits(input: &str) -> IResult<&str, Inits> {
    let (remainder, _) = spaced(tag_no_case(":init"))(input)?;
    many1(delimited(spaced(char('(')), parse_init, spaced(char(')'))))(remainder)
}

#[test]
fn parse_goal_test() {
    assert_eq!(
        Ok((
            "",
            vec![Term {
                name: "name".to_string(),
                parameters: vec![]
            }]
        )),
        parse_inits(":init (Name)")
    );
    assert_eq!(
        Ok((
            "",
            vec![Term {
                name: "name".to_string(),
                parameters: vec!["param1".to_string()]
            }]
        )),
        parse_inits(":init (Name param1)")
    );
    assert_eq!(
        Ok((
            "",
            vec![
                Term {
                    name: "name1".to_string(),
                    parameters: vec!["param1".to_string()]
                },
                Term {
                    name: "name2".to_string(),
                    parameters: vec!["param2".to_string()]
                }
            ]
        )),
        parse_inits(
            ":init (Name1 param1)
                           (Name2 param2)"
        )
    );
    assert_eq!(
        Ok((
            "",
            vec![
                Term {
                    name: "name1".to_string(),
                    parameters: vec!["param1".to_string()]
                },
                Term {
                    name: "name2".to_string(),
                    parameters: vec!["param2".to_string(), "param3".to_string()]
                }
            ]
        )),
        parse_inits(
            ":init (Name1 param1)
                           (Name2 param2 param3)"
        )
    );
}
