use spingus::{domain::Domain, problem::Problem};

use self::facts::Facts;

pub mod facts;
pub mod operator;
pub mod permutation;

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
