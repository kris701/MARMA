use std::collections::HashMap;

use parsing::{
    domain::{predicate::Predicate, Domain},
    problem::{object::Objects, Problem},
};

use itertools::Itertools;

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
    pub values: Vec<bool>,
}

//fn generate_facts_predicate(index: usize, predicate: &Predicate, objects: Vec<usize>) -> Vec<Fact> {
//    if predicate.parameters.is_empty() {
//        return vec![Fact {
//            predicate: index,
//            parameters: vec![],
//        }];
//    }
//    predicate
//        .parameters
//        .iter()
//        .map(|_| objects.iter())
//        .multi_cartesian_product()
//        .map(|p| Fact {
//            predicate: index,
//            parameters: p.iter().map(|i| **i).collect(),
//        })
//        .collect()
//}
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

impl Facts {
    pub fn new(domain: &Domain, problem: &Problem) -> Self {
        let facts = generate_facts_all(domain, problem);
        let fact_map = generate_fact_map(&facts);
        let values = vec![false; facts.len()];
        Facts {
            facts,
            fact_map,
            values,
        }
    }
}
