use std::collections::{HashMap, HashSet};

use itertools::Itertools;
use spingus::problem::init::Inits;

use crate::tools::time::run_time;

use super::{
    actions::Actions, expression::Expression, objects::Objects, parameters::Parameters,
    permute::permute_mutable, predicates::Predicates, types::Types,
};

#[derive(Debug, PartialEq)]
struct PredicateFacts {
    index_map: HashMap<Vec<usize>, usize>,
}
impl PredicateFacts {
    fn new(
        types: &Option<Types>,
        objects: &Objects,
        predicate: usize,
        statics: &Vec<(usize, Vec<usize>)>,
        is_static: bool,
        parameters: &Parameters,
        offset: usize,
    ) -> Self {
        let permutations = match is_static {
            true => statics
                .iter()
                .filter_map(|(i, permutation)| match *i == predicate {
                    true => Some(permutation.to_owned()),
                    false => None,
                })
                .collect(),
            false => permute_mutable(types, objects, &parameters.parameter_types),
        };
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

    fn contains(&self, permutation: &Vec<usize>) -> bool {
        self.index_map.contains_key(permutation)
    }
}

#[derive(Debug, PartialEq)]
pub struct Facts {
    facts: Vec<PredicateFacts>,
    static_predicates: HashSet<usize>,
    init: HashSet<usize>,
}

impl Facts {
    pub fn new(
        types: &Option<Types>,
        predicates: &Predicates,
        actions: &Actions,
        objects: &Objects,
        inits: &Inits,
    ) -> Self {
        let mut facts: Vec<PredicateFacts> = Vec::new();
        let mut static_predicates: HashSet<usize> = HashSet::new();

        let mut statics: Vec<(usize, Vec<usize>)> = Vec::new();
        for init in inits.iter() {
            let predicate = predicates.get_index(&init.name);
            let parameters = objects.get_indexes(&init.parameters);
            statics.push((predicate, parameters));
        }

        let mut offset: usize = 0;
        for (i, predicate) in predicates.predicate_parameters().iter().enumerate() {
            println!(
                "{} Grounding predicate '{}'...",
                run_time(),
                predicates.get_name(i)
            );
            // TODO: Make sure this works!
            let is_static = check_static_all(actions, i);
            println!("is static: {}", is_static);
            if is_static {
                static_predicates.insert(i);
            }
            let predicate_facts =
                PredicateFacts::new(types, objects, i, &statics, is_static, predicate, offset);
            println!("facts: {}", predicate_facts.count());
            offset += predicate_facts.count();
            facts.push(predicate_facts);
        }

        let init: HashSet<usize> = statics
            .iter()
            .map(|(p, par)| facts[*p].index(par))
            .collect();
        Self {
            facts,
            static_predicates,
            init,
        }
    }

    pub fn index(&self, predicate: usize, parameters: &Vec<usize>) -> usize {
        self.facts[predicate].index(parameters)
    }

    pub fn fact_predicate(&self, fact_index: usize) -> usize {
        let mut acc = 0;
        for i in 0..self.facts.len() {
            acc += self.facts[i].count();
            if fact_index < acc {
                return i;
            }
        }
        self.facts.len() - 1
    }

    pub fn fact_parameters(&self, fact_index: usize) -> &Vec<usize> {
        let predicate = self.fact_predicate(fact_index);
        let facts = &self.facts[predicate];
        facts.get_permutation(fact_index)
    }

    pub fn is_static(&self, predicate: usize) -> bool {
        self.static_predicates.contains(&predicate)
    }

    pub fn is_statically_true(&self, predicate: usize, parameters: &Vec<usize>) -> bool {
        self.facts[predicate].contains(parameters)
    }

    pub fn get_static_true(&self) -> Vec<usize> {
        self.init
            .iter()
            .filter_map(|i| match !self.is_static(self.fact_predicate(*i)) {
                true => None,
                false => Some(*i),
            })
            .collect()
    }

    pub fn get_init(&self) -> HashSet<usize> {
        self.init
            .iter()
            .filter_map(|i| match self.is_static(self.fact_predicate(*i)) {
                true => None,
                false => Some(*i),
            })
            .collect()
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

fn check_degrading(predicate: usize, exp: &Expression, val: bool) -> bool {
    match exp {
        Expression::Predicate { index, .. } => *index != predicate || !val,
        Expression::And(exp) => exp.iter().all(|exp| check_degrading(predicate, exp, val)),
        Expression::Not(exp) => check_degrading(predicate, exp, !val),
        _ => todo!(),
    }
}

fn check_degrading_all(actions: &Actions, predicate: usize) -> bool {
    actions
        .actions
        .iter()
        .all(|a| check_degrading(predicate, &a.effect, true))
}
