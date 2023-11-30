use crate::{
    fact::Fact,
    world::{action::Action, World},
};
use std::collections::HashSet;

#[derive(PartialEq, Eq, Clone, Debug)]
pub struct State {
    internal: HashSet<Fact>,
}

impl State {
    pub fn new(facts: &Vec<Fact>) -> Self {
        let internal: HashSet<Fact> = facts
            .iter()
            .filter(|f| !World::global().predicates.is_static(f.predicate()))
            .cloned()
            .collect();
        Self { internal }
    }

    pub fn from_init() -> Self {
        State::new(&World::global().init)
    }

    pub fn apply(&mut self, action: &Action, arguments: &Vec<usize>) {
        for atom in action.effect.iter() {
            let corresponding: Vec<usize> = atom.map_args(arguments);
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

    pub fn has(&self, fact: &Fact) -> bool {
        match World::global().predicates.is_static(fact.predicate()) {
            true => World::global().static_facts.contains(&fact),
            false => self.internal.contains(&fact),
        }
    }

    pub fn has_nullary(&self, predicate: usize) -> bool {
        let fact = Fact::new_nullary(predicate);
        self.has(&fact)
    }

    pub fn has_unary(&self, predicate: usize, arg: usize) -> bool {
        let fact = Fact::new_unary(predicate, arg);
        self.has(&fact)
    }

    pub fn has_nary(&self, predicate: usize, arguments: &Vec<usize>) -> bool {
        let fact = Fact::new(predicate, arguments.clone());
        self.has(&fact)
    }

    pub fn diff(&self, state: &State) -> Vec<(Fact, bool)> {
        let mut diff = vec![];
        for i in self.get().difference(state.get()) {
            diff.push((*i, false))
        }
        for i in state.get().difference(self.get()) {
            diff.push((*i, true))
        }
        diff.sort();
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
