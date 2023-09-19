pub mod action;
pub mod fact;
mod permutation;

use std::time::Instant;

use crate::time::run_time;

use self::{action::Actions, fact::Facts};
use parsing::{domain::Domain, problem::Problem};

pub struct Instance {
    pub facts: Facts,
    pub actions: Actions,
}

impl Instance {
    pub fn new(domain: &Domain, problem: &Problem) -> Self {
        let fact_generation_begin = Instant::now();
        println!("{} Generating pseudo-facts...", run_time());
        let facts = Facts::new(domain, problem);
        println!("{} Generated Facts {}", run_time(), facts.facts.len(),);
        let action_generation_begin = Instant::now();
        println!("{} Generating pseudo-actions...", run_time());
        let actions = Actions::new(domain, problem, &facts);
        println!("{} Generated Actions {}", run_time(), actions.actions.len(),);
        Instance { facts, actions }
    }
}
