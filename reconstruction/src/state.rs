use std::collections::HashSet;

use crate::{
    fact::Fact,
    world::{action::Action, World},
};

#[derive(PartialEq, Eq, Clone, Debug)]
pub struct State {
    internal: HashSet<Fact>,
}

impl State {
    pub fn new(facts: &Vec<Fact>) -> Self {
        let internal: HashSet<Fact> = facts.iter().cloned().collect();
        Self { internal }
    }

    pub fn from_init() -> Self {
        State::new(&World::global().init)
    }

    pub fn apply(&mut self, action: &Action, arguments: &Vec<usize>) {
        for atom in action.effect.iter() {
            let corresponding: Vec<usize> = atom.parameters.iter().map(|p| arguments[*p]).collect();
            let fact = Fact::new(atom.predicate, corresponding);
            match atom.value {
                true => self.internal.insert(fact),
                false => self.internal.remove(&fact),
            };
        }
    }

    fn get(&self) -> &HashSet<Fact> {
        &self.internal
    }

    pub fn has(&self, predicate: usize, arguments: &Vec<usize>) -> bool {
        self.internal
            .contains(&Fact::new(predicate, arguments.clone()))
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
