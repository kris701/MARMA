use std::ops::{BitAnd, BitAndAssign, BitOr, BitOrAssign};

use bitvec::vec::BitVec;
use itertools::Itertools;
use parsing::{domain::Domain, problem::Problem};

use crate::instance::{
    fact::{Fact, Facts},
    operator::Operator,
};

pub type State = BitVec;

fn convert_init(
    domain: &Domain,
    problem: &Problem,
    i: &parsing::term::Term,
    facts: &Facts,
) -> usize {
    let predicate = domain
        .predicates
        .iter()
        .position(|pre| pre.name == i.name)
        .unwrap();
    let parameters: Vec<usize> = i
        .parameters
        .iter()
        .map(|par| problem.objects.iter().position(|o| o.name == *par).unwrap())
        .collect();

    facts
        .fact_map
        .get(&Fact {
            predicate,
            parameters,
        })
        .unwrap()
        .to_owned()
}

pub fn generate_state(domain: &Domain, problem: &Problem, facts: &Facts) -> State {
    let inits: Vec<usize> = problem
        .inits
        .iter()
        .map(|i| convert_init(domain, problem, i, facts))
        .collect();
    let values = (0..facts.facts.len())
        .map(|num| inits.contains(&num))
        .collect();
    values
}

pub fn apply_to_state(state: &mut State, operator: &Operator) {
    state.bitand_assign(operator.del.to_owned());
    state.bitor_assign(operator.add.to_owned());
}
