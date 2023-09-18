use nom::{bytes::complete::tag, character::complete::multispace0, sequence::preceded, IResult};

use crate::shared::named;

pub(super) fn parse_name(input: &str) -> IResult<&str, String> {
    let (remainder, _) = tag("problem")(input)?;
    let (remainder, name) = preceded(multispace0, named)(remainder)?;
    Ok((remainder, name.to_string()))
}

#[test]
fn test() {
    assert_eq!(Ok(("", "test".to_string())), parse_name("problem test"));
    assert_eq!(Ok(("", "test-1".to_string())), parse_name("problem test-1"));
}
