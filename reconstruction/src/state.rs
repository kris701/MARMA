use std::collections::HashSet;

use crate::{fact::Fact, operator::Operator, world::World};

#[derive(PartialEq, Eq, Clone, Debug)]
pub struct State {
    internal: HashSet<Fact>,
}

impl State {
    pub fn new(facts: &Vec<Fact>) -> Self {
        let internal: HashSet<Fact> = facts
            .into_iter()
            .filter(|fact| !World::global().predicates.is_static(fact.predicate()))
            .cloned()
            .collect();
        Self { internal }
    }

    pub fn from_init() -> Self {
        State::new(&World::global().init)
    }

    pub fn apply(&mut self, operator: &Operator) {
        for i in operator.eff_neg.iter() {
            self.internal.remove(i);
        }
        for i in operator.eff_pos.iter() {
            self.internal.insert(*i);
        }
    }

    fn get(&self) -> &HashSet<Fact> {
        &self.internal
    }

    pub fn is_legal(&self, operator: &Operator) -> bool {
        let has_pos = operator.pre_pos.iter().all(|i| self.internal.contains(i));
        let has_neg = operator.pre_neg.iter().all(|i| !self.internal.contains(i));
        has_pos && has_neg
    }

    pub fn diff(&self, state: &State) -> Vec<(Fact, bool)> {
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

    pub fn export_all(&self) -> String {
        let mut s: String = "".to_owned();
        World::global()
            .static_facts
            .iter()
            .chain(self.internal.iter())
            .for_each(|fact| s.push_str(&format!("\n\t\t({})", fact.to_string())));
        s
    }

    pub fn export_mutable(&self) -> String {
        let mut s: String = "".to_owned();
        self.internal
            .iter()
            .for_each(|fact| s.push_str(&format!("\n\t\t({})", fact.to_string())));
        s
    }
}
