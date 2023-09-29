use nom::{bytes::complete::is_not, IResult};
use nom::{
    bytes::complete::tag, character::complete::multispace0, combinator::not, sequence::delimited,
};

pub fn remove_comments(input: &str) -> String {
    let mut out: String = String::new();
    let mut in_comment = false;
    input.chars().for_each(|c| {
        if c == ';' {
            in_comment = true;
        } else if c == '\n' {
            in_comment = false;
        }
        if !in_comment {
            out.push(c);
        }
    });
    out
}

pub fn spaced<F, I, O, E>(f: F) -> impl FnMut(I) -> IResult<I, O, E>
where
    F: FnMut(I) -> IResult<I, O, E>,
    I: nom::InputTakeAtPosition,
    <I as nom::InputTakeAtPosition>::Item: nom::AsChar + Clone,
    E: nom::error::ParseError<I>,
{
    delimited(multispace0, f, multispace0)
}

pub fn named(input: &str) -> IResult<&str, String> {
    let (remainder, name) = is_not(" \t\r\n()?:")(input)?;
    not(tag("-"))(name)?;
    not(tag("="))(name)?;
    return Ok((remainder, name.to_owned()));
}

#[test]
fn remove_comments_test() {
    assert_eq!("", remove_comments(""));
    assert_eq!("abc", remove_comments("abc"));
    assert_eq!("", remove_comments(";"));
    assert_eq!("", remove_comments(";abc"));
    assert_eq!("\n", remove_comments(";abc\n"));
    assert_eq!("\n123", remove_comments(";abc\n123"));
    assert_eq!("0\n123", remove_comments("0;abc\n123"));
}

#[test]
fn named_test() {
    //assert_eq!(Ok(("", "abc")), named("abc"));
    //assert_eq!(Ok((" d", "abc")), named("abc d"));
    //assert_eq!(Ok(("\td", "abc")), named("abc\td"));
    //assert_eq!(Ok(("\r\nd", "abc")), named("abc\r\nd"));
    //assert_eq!(Ok(("(d", "abc")), named("abc(d"));
    //assert_eq!(Ok((")d", "abc")), named("abc)d"));
    //assert_eq!(Ok((" c", "a-b")), named("a-b c"));
}
