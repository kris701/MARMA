use std::collections::HashSet;

use crate::{
    fact::Fact,
    instance::{operator::Operator, Instance},
    world::World,
};

#[derive(PartialEq, Eq, Clone, Debug)]
pub struct State {
    statics: HashSet<Fact>,
    mutable: HashSet<Fact>,
}

impl State {
    pub fn new(instance: &Instance, facts: &Vec<Fact>) -> Self {
        let (statics, mutable): (HashSet<Fact>, HashSet<Fact>) =
            facts.iter().partition(|fact| is_static(instance, &fact));
        Self { statics, mutable }
    }

    pub fn from_init(instance: &Instance) -> Self {
        State::new(instance, World::global().init())
    }

    pub fn apply(&mut self, operator: &Operator) {
        for i in operator.eff_neg.iter() {
            self.mutable.remove(i);
        }
        for i in operator.eff_pos.iter() {
            self.mutable.insert(*i);
        }
    }

    pub fn get(&self) -> &HashSet<Fact> {
        &self.mutable
    }

    pub fn is_legal(&self, operator: &Operator) -> bool {
        let has_pos = operator.pre_pos.iter().all(|i| self.mutable.contains(i));
        let has_neg = operator.pre_neg.iter().all(|i| !self.mutable.contains(i));
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
}

fn is_static(instance: &Instance, fact: &Fact) -> bool {
    let predicate = fact.predicate();
    instance
        .actions
        .actions
        .iter()
        .any(|a| a.effect.literals.iter().any(|l| l.predicate == predicate))
}
