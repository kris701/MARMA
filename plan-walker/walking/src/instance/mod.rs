pub mod action;
pub mod fact;
mod permutation;

use std::time::Instant;

use self::{action::Actions, fact::Facts};
use parsing::{domain::Domain, problem::Problem};

pub struct Instance {
    pub facts: Facts,
    pub actions: Actions,
}

impl Instance {
    pub fn new(domain: &Domain, problem: &Problem) -> Self {
        let fact_generation_begin = Instant::now();
        println!("Generating pseudo-facts...");
        let facts = Facts::new(domain, problem);
        println!(
            "Generated Facts {} [{}ms]",
            facts.facts.len(),
            fact_generation_begin.elapsed().as_millis()
        );
        let action_generation_begin = Instant::now();
        println!("Generating pseudo-actions...");
        let actions = Actions::new(domain, problem, &facts);
        println!(
            "Generated Actions {} [{}ms]",
            actions.actions.len(),
            action_generation_begin.elapsed().as_millis()
        );
        Instance { facts, actions }
    }
}
