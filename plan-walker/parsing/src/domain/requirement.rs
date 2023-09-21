use nom::{
    bytes::complete::tag, character::complete::char, multi::separated_list0, sequence::preceded,
    IResult,
};

use crate::shared::{named, spaced};

pub type Requirements = Vec<String>;

#[allow(dead_code)]
pub(super) fn parse_requirements(input: &str) -> IResult<&str, Requirements> {
    let (remainder, _) = spaced(tag(":requirements"))(input)?;
    let (remainder, requirements) =
        separated_list0(char(' '), preceded(char(':'), named))(remainder)?;
    Ok((
        remainder,
        requirements.iter().map(|r| r.to_string()).collect(),
    ))
}

#[test]
fn test() {
    assert_eq!(Ok(("", vec![])), parse_requirements(":requirements"));
    assert_eq!(
        Ok(("", vec!["a".to_string()])),
        parse_requirements(":requirements :a")
    );
    assert_eq!(
        Ok(("", vec!["a".to_string(), "b".to_string()])),
        parse_requirements(":requirements :a :b")
    );
}
