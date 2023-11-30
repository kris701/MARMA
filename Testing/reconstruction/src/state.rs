use crate::{
    fact::Fact,
    world::{action::Action, World},
};
use std::collections::HashSet;

#[derive(PartialEq, Eq, Clone, Debug)]
pub struct State {
    facts: HashSet<Fact>,
    partial_facts: HashSet<Fact>,
}

impl State {
    pub fn new(facts: &Vec<Fact>) -> Self {
        let facts: HashSet<Fact> = facts
            .iter()
            .filter(|f| !World::global().predicates.is_static(f.predicate()))
            .cloned()
            .collect();
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
        match World::global().predicates.is_static(fact.predicate()) {
            true => World::global().static_facts.contains(&fact),
            false => self.facts.contains(&fact),
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

    pub fn has_partial(&self, predicate: usize, index: usize, arg: usize) -> bool {
        let fact = generate_partial_fact(predicate, index, arg);
        self.partial_facts.contains(&fact)
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
            .chain(self.facts.iter())
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

fn generate_partial_facts(facts: &HashSet<Fact>) -> HashSet<Fact> {
    let mut partial_facts: HashSet<Fact> = HashSet::new();
    for fact in facts
        .iter()
        .chain(World::global().init.iter())
        .filter(|f| f.parameters().len() > 1)
    {
        let predicate = fact.predicate();
        let args = fact.parameters();
        for i in 0..args.len() {
            partial_facts.insert(generate_partial_fact(predicate, i, args[i]));
        }
    }
    partial_facts
}

fn generate_partial_fact(predicate: usize, index: usize, arg: usize) -> Fact {
    debug_assert_ne!(arg, 0);
    let arity = World::global().predicates.arity(predicate);
    debug_assert!(arity > 1);
    debug_assert!(index < arity);
    let fact = Fact::new(
        predicate,
        (0..arity)
            .into_iter()
            .map(|i| match i == index {
                true => arg,
                false => 0,
            })
            .collect(),
    );
    debug_assert!(fact.parameters().iter().any(|p| *p != 0));
    fact
}
