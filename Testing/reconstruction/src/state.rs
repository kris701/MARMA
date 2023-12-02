use crate::{
    fact::Fact,
    world::{action::Action, World},
};
use bitvec::prelude::*;
use std::collections::HashSet;

#[derive(PartialEq, Eq, Clone, Debug)]
pub struct State {
    facts: HashSet<Fact>,
    partial_facts: Vec<Vec<BitVec>>,
}

impl State {
    pub fn new(facts: &Vec<Fact>) -> Self {
        let facts: HashSet<Fact> = facts.iter().cloned().collect();
        let partial_facts = generate_partial_facts(&facts);
        Self {
            facts,
            partial_facts,
        }
    }

    pub fn from_init() -> Self {
        State::new(&World::global().init)
    }

    pub fn apply(&mut self, action: &Action, arguments: &Vec<usize>) {
        for atom in action.effect.iter() {
            let corresponding: Vec<usize> = atom.map_args(arguments);
            let fact = Fact::new(atom.predicate, corresponding);
            match atom.value {
                true => self.facts.insert(fact),
                false => self.facts.remove(&fact),
            };
        }
        self.partial_facts = generate_partial_facts(&self.facts);
    }

    fn get(&self) -> &HashSet<Fact> {
        &self.facts
    }

    pub fn has(&self, fact: &Fact) -> bool {
        self.facts.contains(&fact)
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

    pub fn has_partial(&self, predicate: usize, index: usize, arg: usize) -> bool {
        self.partial_facts[predicate][index][arg]
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
        self.facts
            .iter()
            .for_each(|fact| s.push_str(&format!("\n\t\t({})", fact.to_string())));
        s
    }

    pub fn export_mutable(&self) -> String {
        let mut s: String = "".to_owned();
        self.facts
            .iter()
            .for_each(|fact| s.push_str(&format!("\n\t\t({})", fact.to_string())));
        s
    }
}

fn generate_partial_facts(facts: &HashSet<Fact>) -> Vec<Vec<BitVec>> {
    let mut partial_facts: Vec<Vec<BitVec>> = vec![vec![]];
    for i in 1..World::global().predicates.count() + 1 {
        let mut predicate_partials: Vec<BitVec> = Vec::new();

        let arity = World::global().predicates.arity(i);
        if arity > 1 {
            for _ in 0..arity {
                predicate_partials.push(bitvec![0; World::global().objects.count() + 1]);
            }
        }

        partial_facts.push(predicate_partials);
    }

    for fact in facts.iter().filter(|f| f.parameters().len() > 1) {
        let predicate = fact.predicate();
        let args = fact.parameters();
        for i in 0..args.len() {
            partial_facts[predicate][i].set(args[i], true);
        }
    }
    partial_facts
}
