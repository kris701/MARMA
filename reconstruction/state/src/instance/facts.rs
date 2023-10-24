use std::collections::HashMap;

use shared::time::run_time;
use spingus::{
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
    facts: Vec<Fact>,
    fact_map: HashMap<Fact, usize>,
    predicate_map: HashMap<String, usize>,
    object_map: HashMap<String, usize>,
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
    println!("{} Generating facts...", run_time());
    let facts: Vec<Fact> = domain
        .predicates
        .iter()
        .enumerate()
        .flat_map(|(i, predicate)| {
            let facts = generate_facts_predicate(domain, problem, predicate, i);
            println!("{}: {}", predicate.name, facts.len());
            facts
        })
        .collect();
    println!("total: {}", facts.len());
    facts
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

    pub fn fact_predicate(&self, fact_index: usize) -> usize {
        self.facts[fact_index].predicate
    }

    pub fn fact_parameters(&self, fact_index: usize) -> &Vec<usize> {
        &self.facts[fact_index].parameters
    }

    pub fn predicate_index(&self, predicate: &String) -> usize {
        self.predicate_map[predicate]
    }

    pub fn object_index(&self, object: &String) -> usize {
        self.object_map[object]
    }
}
