use nom::branch::permutation;
use nom::bytes::complete::{tag, tag_no_case};
use nom::combinator::opt;
use nom::multi::many1;
use nom::sequence::delimited;
use nom::{character::complete::char, IResult};

use crate::shared::{remove_comments, spaced};

use self::domain::parse_domain;
use self::goal::{parse_goal, Goals};
use self::init::parse_inits;
use self::name::parse_name;
use self::object::parse_objects;
use self::{goal::Goal, init::Inits, object::Objects};

mod domain;
pub mod goal;
pub mod init;
mod name;
pub mod object;

#[derive(Debug, PartialEq)]
pub struct Problem {
    pub name: String,
    pub domain: String,
    pub objects: Objects,
    pub inits: Inits,
    pub goal: Goal,
}

fn parse_internal(input: &str) -> IResult<&str, Problem> {
    let (remaining, _) = spaced(tag_no_case("define"))(input)?;
    let (remaining, (name, domain, objects, inits, goal)) = permutation((
        spaced(delimited(char('('), parse_name, char(')'))),
        spaced(delimited(char('('), parse_domain, char(')'))),
        spaced(delimited(char('('), parse_objects, char(')'))),
        spaced(delimited(char('('), parse_inits, char(')'))),
        spaced(delimited(char('('), parse_goal, char(')'))),
    ))(remaining)?;
    Ok((
        remaining,
        Problem {
            name,
            objects,
            domain,
            inits,
            goal,
        },
    ))
}

pub fn parse_problem(input: &str) -> Result<Problem, String> {
    let clean = remove_comments(input);
    let (_, domain) = match delimited(spaced(char('(')), parse_internal, spaced(char(')')))(&clean)
    {
        Ok(it) => it,
        Err(err) => return Err(err.to_string()),
    };
    Ok(domain)
}
