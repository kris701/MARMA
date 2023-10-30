use std::collections::HashSet;

use spingus::domain::Domain;
use spingus::problem::Problem;

use super::instance::facts::Facts;
use super::instance::operator::Operator;

#[derive(PartialEq, Eq, Clone, Debug)]
pub struct State {
    internal: HashSet<usize>,
}

impl State {
    // TODO: Clean this
    pub fn new(domain: &Domain, problem: &Problem, facts: &Facts) -> Self {
        let mut internal: HashSet<usize> = HashSet::new();
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
                internal.insert(fact);
            }
        }
        Self { internal }
    }

    pub fn apply(&mut self, operator: &Operator) {
        for i in operator.eff_neg.iter() {
            self.internal.remove(&i);
        }
        for i in operator.eff_pos.iter() {
            self.internal.insert(*i);
        }
    }

    pub fn get(&self) -> &HashSet<usize> {
        &self.internal
    }

    pub fn is_legal(&self, operator: &Operator) -> bool {
        let has_pos = self.internal.is_superset(&operator.pre_pos);
        let has_neg = self.internal.is_disjoint(&operator.pre_neg);
        has_pos && has_neg
    }

    pub fn diff(&self, state: &State) -> Vec<(usize, bool)> {
        let mut diff = vec![];
        for i in self.get().difference(state.get()) {
            diff.push((*i, false))
        }
        for i in state.get().difference(self.get()) {
            diff.push((*i, true))
        }
        diff.sort_by(|a, b| a.0.cmp(&b.0));
        diff
    }
}
