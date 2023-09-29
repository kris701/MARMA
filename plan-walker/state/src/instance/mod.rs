use std::ffi::OsString;

use parsing::{
    domain::{parse_domain, Domain},
    problem::Problem,
};
use shared::time::run_time;

use self::{fact::Facts, operator::Operators};

pub mod fact;
pub mod operator;
mod permutation;

pub struct Instance {
    pub domain: Domain,
    pub problem: Problem,
    pub facts: Facts,
    pub operators: Operators,
}

impl Instance {
    pub fn new(domain: Domain, problem: Problem) -> Self {
        let facts = Facts::new(&domain, &problem);
        let operators = Operators::new(&domain, &problem, &facts);

        Self {
            domain,
            problem,
            facts,
            operators,
        }
    }
}
