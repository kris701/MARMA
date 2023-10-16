use spingus::{
    domain::{action::Action, Domain},
    problem::Problem,
};

use self::{facts::Facts, operator::Operator};

pub mod facts;
pub mod operator;
mod permutation;

pub struct Instance {
    pub domain: Domain,
    pub problem: Problem,
    pub facts: Facts,
}

impl Instance {
    pub fn new(domain: Domain, problem: Problem) -> Self {
        let facts = Facts::new(&domain, &problem);

        Self {
            domain,
            problem,
            facts,
        }
    }
}
