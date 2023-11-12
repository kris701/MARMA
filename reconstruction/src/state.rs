use std::collections::HashSet;

use crate::instance::{operator::Operator, Instance};

#[derive(PartialEq, Eq, Clone, Debug)]
pub struct State {
    internal: HashSet<u32>,
}

impl State {
    pub fn new(instance: &Instance) -> Self {
        let internal: HashSet<u32> = instance.facts.get_init();
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

    pub fn get(&self) -> &HashSet<u32> {
        &self.internal
    }

    pub fn is_legal(&self, operator: &Operator) -> bool {
        let has_pos = operator.pre_pos.iter().all(|i| self.internal.contains(i));
        let has_neg = operator.pre_neg.iter().all(|i| !self.internal.contains(i));
        has_pos && has_neg
    }

    pub fn diff(&self, state: &State) -> Vec<(u32, bool)> {
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
