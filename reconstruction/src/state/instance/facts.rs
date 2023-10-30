use std::collections::{HashMap, HashSet};

use crate::tools::time::run_time;

use super::{
    actions::Actions, expression::Expression, objects::Objects, parameters::Parameters,
    permute::permute_all, predicates::Predicates, types::Types,
};

#[derive(Debug, PartialEq)]
struct PredicateFacts {
    index_map: HashMap<Vec<usize>, usize>,
}
impl PredicateFacts {
    fn new(
        types: &Option<Types>,
        objects: &Objects,
        parameters: &Parameters,
        offset: usize,
    ) -> Self {
        let permutations = permute_all(types, objects, parameters);
        let mut index_map: HashMap<Vec<usize>, usize> = HashMap::new();
        for permutation in permutations.into_iter() {
            index_map.insert(permutation, offset + index_map.len());
        }
        Self { index_map }
    }

    fn count(&self) -> usize {
        self.index_map.len()
    }

    fn index(&self, permutation: &Vec<usize>) -> usize {
        self.index_map[permutation]
    }

    fn get_permutation(&self, index: usize) -> &Vec<usize> {
        self.index_map
            .iter()
            .find(|(_, i)| **i == index)
            .map(|(permutation, _)| permutation)
            .unwrap()
    }
}

#[derive(Debug, PartialEq)]
pub struct Facts {
    facts: Vec<PredicateFacts>,
    statics: HashSet<usize>,
}

impl Facts {
    pub fn new(
        types: &Option<Types>,
        predicates: &Predicates,
        actions: &Actions,
        objects: &Objects,
    ) -> Self {
        let mut facts: Vec<PredicateFacts> = Vec::new();
        let mut statics: HashSet<usize> = HashSet::new();

        let mut offset: usize = 0;
        for (i, predicate) in predicates.predicate_parameters().iter().enumerate() {
            println!(
                "{} Grounding predicate '{}'...",
                run_time(),
                predicates.get_name(i)
            );
            let is_static = check_static_all(actions, i);
            println!("is static: {}", is_static);
            if is_static {
                statics.insert(i);
            }
            let predicate_facts = PredicateFacts::new(types, objects, predicate, offset);
            offset += predicate_facts.count();
            facts.push(predicate_facts);
        }

        Self { facts, statics }
    }

    pub fn count(&self) -> usize {
        self.facts.iter().map(|f| f.count()).sum()
    }

    pub fn index(&self, predicate: usize, parameters: &Vec<usize>) -> usize {
        self.facts[predicate].index(parameters)
    }

    pub fn fact_predicate(&self, fact_index: usize, is_static: bool) -> usize {
        let mut acc = 0;
        for i in 0..self.facts.len() {
            acc += self.facts[i].count();
            if fact_index < acc {
                return i;
            }
        }
        self.facts.len() - 1
    }

    pub fn fact_parameters(&self, fact_index: usize, is_static: bool) -> &Vec<usize> {
        let predicate = self.fact_predicate(fact_index, is_static);
        let facts = &self.facts[predicate];
        facts.get_permutation(fact_index)
    }

    pub fn predicate_index(&self, predicate: &String) -> usize {
        todo!()
    }

    pub fn is_static(&self, predicate: usize) -> bool {
        false
    }

    pub fn is_statically_true(&self, index: usize) -> bool {
        todo!()
    }

    pub fn get_static_true(&self) -> Vec<usize> {
        vec![]
    }
}

fn check_static(predicate: usize, exp: &Expression) -> bool {
    match exp {
        Expression::Predicate { index, .. } => *index != predicate,
        Expression::And(exp) => exp.iter().all(|exp| check_static(predicate, exp)),
        Expression::Not(exp) => check_static(predicate, exp),
        _ => todo!(),
    }
}

fn check_static_all(actions: &Actions, predicate: usize) -> bool {
    actions
        .actions
        .iter()
        .all(|a| check_static(predicate, &a.effect))
}
