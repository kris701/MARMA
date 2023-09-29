use std::collections::HashMap;

use parsing::{
    domain::{
        predicate::{Predicate, Predicates},
        Domain,
    },
    problem::{object::Objects, Problem},
};

use super::permutation::permute;

#[derive(Debug, PartialEq, Eq, Hash, Clone)]
pub struct Fact {
    pub predicate: usize,
    pub parameters: Vec<usize>,
}
#[derive(Debug, PartialEq)]
pub struct Facts {
    pub facts: Vec<Fact>,
    pub fact_map: HashMap<Fact, usize>,
    pub predicate_map: HashMap<String, usize>,
    pub object_map: HashMap<String, usize>,
}

fn generate_facts_predicate(
    domain: &Domain,
    problem: &Problem,
    predicate: &Predicate,
    predicate_index: usize,
) -> Vec<Fact> {
    let permutations = permute(&domain.types, problem, &predicate.parameters);
    permutations
        .iter()
        .map(|permutation| Fact {
            predicate: predicate_index,
            parameters: permutation.to_owned(),
        })
        .collect()
}

fn generate_facts_all(domain: &Domain, problem: &Problem) -> Vec<Fact> {
    domain
        .predicates
        .iter()
        .enumerate()
        .flat_map(|(i, predicate)| generate_facts_predicate(domain, problem, predicate, i))
        .collect()
}

fn generate_fact_map(facts: &Vec<Fact>) -> HashMap<Fact, usize> {
    facts
        .iter()
        .enumerate()
        .map(|(i, fact)| (fact.to_owned(), i))
        .collect()
}

fn generate_predicate_map(predicates: &Predicates) -> HashMap<String, usize> {
    predicates
        .iter()
        .enumerate()
        .map(|(i, pred)| (pred.name.to_owned(), i))
        .collect()
}

fn generate_object_map(objects: &Objects) -> HashMap<String, usize> {
    objects
        .iter()
        .enumerate()
        .map(|(i, object)| (object.name.to_owned(), i))
        .collect()
}

impl Facts {
    pub fn new(domain: &Domain, problem: &Problem) -> Self {
        let predicate_map = generate_predicate_map(&domain.predicates);
        let object_map = generate_object_map(&problem.objects);
        let facts = generate_facts_all(domain, problem);
        let fact_map = generate_fact_map(&facts);
        Facts {
            facts,
            fact_map,
            predicate_map,
            object_map,
        }
    }

    pub fn count(&self) -> usize {
        self.facts.len()
    }

    pub fn index(&self, predicate: usize, parameters: &Vec<usize>) -> usize {
        let fact = Fact {
            predicate,
            parameters: parameters.to_owned(),
        };
        self.fact_map[&fact]
    }
}
