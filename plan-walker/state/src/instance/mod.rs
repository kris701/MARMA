use std::ffi::OsString;

use parsing::{
    domain::{parse_domain, Domain},
    problem::Problem,
};

use self::fact::Facts;

pub mod fact;
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

pub fn instance_from(domain_path: &OsString, problem_path: &OsString) -> Instance {
    let domain = Domain::from(domain_path);
    let problem = Problem::from(problem_path);
    Instance::new(domain, problem)
}
