pub mod expression;
pub mod fact;
mod permutation;

use std::time::Instant;

use crate::time::run_time;

use self::fact::Facts;
use parsing::{domain::Domain, problem::Problem};
