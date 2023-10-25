use std::collections::HashMap;

use shared::time::run_time;
use spingus::{
    domain::{
        action::{string_expression::StringExpression, Actions},
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

#[derive(PartialEq, Eq, Debug)]
enum FactType {
    Fact(usize),
    Static(usize),
}

#[derive(Debug, PartialEq)]
pub struct Facts {
    /// Predicate name to index
    /// Simply corresponds to position the predicate is found at
    predicate_map: HashMap<String, usize>,
    /// Object name to index
    /// Simply corresponds to position the object is found at
    object_map: HashMap<String, usize>,
    /// Facts that appear in no effects
    static_predicates: Vec<bool>,
    /// Facts that can change value
    facts: Vec<Fact>,
    /// Facts that can not change value
    facts_static: Vec<(Fact, bool)>,
}

impl Facts {
    pub fn new(domain: &Domain, problem: &Problem) -> Self {
        println!("{} Generating object map...", run_time());
        let object_map = generate_object_map(&problem.objects);
        println!("{} Generating predicate map...", run_time());
        let predicate_map = generate_predicate_map(&domain.predicates);
        println!("{} Finding static predicates...", run_time());
        let static_predicates: Vec<bool> = find_static_predicates(domain);
        for (i, s) in static_predicates.iter().enumerate() {
            println!("{}: {}", domain.predicates[i].name, s)
        }
        println!("{} Generating facts...", run_time());
        let mut facts: Vec<Fact> = Vec::new();
        let mut facts_static: Vec<(Fact, bool)> = Vec::new();
        for (i, predicate) in domain.predicates.iter().enumerate() {
            let temp_facts = generate_facts(domain, problem, predicate, i);
            println!("{}: {}", predicate.name, temp_facts.len());
            for fact in temp_facts {
                match static_predicates[i] {
                    true => facts_static.push((fact, false)),
                    false => facts.push(fact),
                }
            }
        }
        println!("non-static: {}", facts.len());
        println!("static: {}", facts_static.len());
        println!("total: {}", facts.len() + facts_static.len());

        println!("{} Finding statically true...", run_time());
        for fact in problem
            .inits
            .iter()
            .filter(|f| static_predicates[predicate_map[&f.name]])
        {
            let pred_index: usize = predicate_map[&fact.name];
            let parameters: Vec<usize> = fact.parameters.iter().map(|o| object_map[o]).collect();
            let pos = facts_static
                .iter()
                .position(|(fact, _)| fact.predicate == pred_index && fact.parameters == parameters)
                .unwrap();
            facts_static[pos].1 = true;
        }

        Self {
            predicate_map,
            object_map,
            static_predicates,
            facts,
            facts_static,
        }
    }

    pub fn count(&self) -> usize {
        self.facts.len()
    }

    pub fn index(&self, predicate: usize, parameters: &Vec<usize>) -> usize {
        match self.static_predicates[predicate] {
            true => self
                .facts_static
                .iter()
                .position(|fact| fact.0.predicate == predicate && fact.0.parameters == *parameters)
                .unwrap(),
            false => self
                .facts
                .iter()
                .position(|fact| fact.predicate == predicate && fact.parameters == *parameters)
                .unwrap(),
        }
    }

    pub fn fact_predicate(&self, fact_index: usize, is_static: bool) -> usize {
        match is_static {
            true => self.facts_static[fact_index].0.predicate,
            false => self.facts[fact_index].predicate,
        }
    }

    pub fn fact_parameters(&self, fact_index: usize, is_static: bool) -> &Vec<usize> {
        match is_static {
            true => &self.facts_static[fact_index].0.parameters,
            false => &self.facts[fact_index].parameters,
        }
    }

    pub fn predicate_index(&self, predicate: &String) -> usize {
        self.predicate_map[predicate]
    }

    pub fn object_index(&self, object: &String) -> usize {
        self.object_map[object]
    }

    pub fn is_static(&self, predicate: usize) -> bool {
        self.static_predicates[predicate]
    }

    pub fn is_statically_true(&self, index: usize) -> bool {
        self.facts_static[index].1
    }

    pub fn get_static_true(&self) -> Vec<usize> {
        self.facts_static
            .iter()
            .enumerate()
            .filter(|(_, (_, v))| *v)
            .map(|(i, _)| i)
            .collect()
    }
}

fn generate_object_map(objects: &Objects) -> HashMap<String, usize> {
    objects
        .iter()
        .enumerate()
        .map(|(i, object)| (object.name.to_owned(), i))
        .collect()
}

fn generate_predicate_map(predicates: &Predicates) -> HashMap<String, usize> {
    predicates
        .iter()
        .enumerate()
        .map(|(i, pred)| (pred.name.to_owned(), i))
        .collect()
}

fn contains_predicate(exp: &StringExpression, predicate: &Predicate) -> bool {
    match exp {
        StringExpression::Predicate(p) => *p.name == *predicate.name,
        StringExpression::And(e) => e.iter().any(|e| contains_predicate(e, predicate)),
        StringExpression::Not(e) => contains_predicate(e, predicate),
        _ => false,
    }
}

fn is_static(actions: &Actions, predicate: &Predicate) -> bool {
    actions
        .iter()
        .all(|a| !contains_predicate(&a.effect, predicate))
}

fn find_static_predicates(domain: &Domain) -> Vec<bool> {
    domain
        .predicates
        .iter()
        .map(|p| is_static(&domain.actions, &p))
        .collect()
}

fn generate_facts(
    domain: &Domain,
    problem: &Problem,
    predicate: &Predicate,
    predicate_index: usize,
) -> Vec<Fact> {
    let permutations = permute(&domain.types, problem, &predicate.parameters);
    permutations
        .iter()
        .map(|p| Fact {
            predicate: predicate_index,
            parameters: p.to_vec(),
        })
        .collect()
}
