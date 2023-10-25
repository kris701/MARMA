use std::ops::{BitAnd, BitAndAssign, BitOrAssign};

use bitvec::prelude::*;
use bitvec::vec::BitVec;

use spingus::domain::Domain;
use spingus::problem::Problem;

use crate::instance::facts::Facts;
use crate::instance::operator::Operator;

#[derive(Hash, PartialEq, Eq, PartialOrd, Ord, Clone, Debug)]
pub struct State {
    /// Each index in this bitvector corresponds to a fact
    /// Some index being true then means that fact is true
    internal: BitVec,
}

impl State {
    // TODO: Clean this
    pub fn new(domain: &Domain, problem: &Problem, facts: &Facts) -> Self {
        let mut internal = bitvec!(usize, Lsb0; 0; facts.count());
        for init in &problem.inits {
            let predicate = domain
                .predicates
                .iter()
                .position(|predicate| predicate.name == init.name)
                .unwrap();
            if !facts.is_static(predicate) {
                let parameters: Vec<usize> = init
                    .parameters
                    .iter()
                    .map(|p| problem.objects.iter().position(|o| o.name == *p).unwrap())
                    .collect();
                let fact = facts.index(predicate, &parameters);
                internal.set(fact, true);
            }
        }
        Self { internal }
    }

    pub fn apply(&mut self, operator: &Operator) {
        self.internal.bitand_assign(!operator.eff_neg.to_owned());
        self.internal.bitor_assign(&operator.eff_pos);
    }

    pub fn apply_clone(&self, operator: &Operator) -> State {
        let mut clone = self.clone();
        clone.apply(operator);
        clone
    }

    pub fn apply_multiple(&self, operators: &Vec<Operator>) -> State {
        let mut clone = self.clone();
        for operator in operators {
            clone.apply(operator);
        }
        clone
    }

    pub fn get(&self) -> BitVec {
        self.internal.clone()
    }

    pub fn is_legal(&self, operator: &Operator) -> bool {
        let has_pos = self.get().bitand(&operator.pre_pos) == operator.pre_pos;
        let has_neg = self.get().bitand(&operator.pre_neg).not_any();
        has_pos && has_neg
    }
}
