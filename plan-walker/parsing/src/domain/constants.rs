use nom::{bytes::complete::tag, IResult};

use crate::shared::spaced;

use super::parameter::{parse_parameters, Parameters};

pub fn parse_constants(input: &str) -> IResult<&str, Parameters> {
    let (remainder, _) = spaced(tag(":constants"))(input)?;
    parse_parameters(remainder)
}
