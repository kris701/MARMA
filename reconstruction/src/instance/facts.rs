use spingus::problem::init::Inits;
use std::collections::{HashMap, HashSet};

use crate::{
    tools::{status_print, Status},
    world::World,
};

use super::{
    actions::Actions, expression::Expression, objects::Objects, parameters::Parameters,
    permute::permute_mutable, predicates::Predicates, types::Types,
};

#[derive(Debug, PartialEq)]
struct PredicateFacts {
    index_map: HashMap<Vec<u32>, u32>,
}
impl PredicateFacts {
    fn new(
        types: &Option<Types>,
        objects: &Objects,
        predicate: u32,
        statics: &Vec<(u32, Vec<u32>)>,
        is_static: bool,
        parameters: &Parameters,
        offset: u32,
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
        let mut index_map: HashMap<Vec<u32>, u32> = HashMap::new();
        for permutation in permutations.into_iter() {
            index_map.insert(permutation, offset + index_map.len() as u32);
        }
        Self { index_map }
    }

    fn count(&self) -> usize {
        self.index_map.len()
    }

    fn index(&self, permutation: &Vec<u32>) -> u32 {
        self.index_map[permutation]
    }

    fn get_permutation(&self, index: u32) -> &Vec<u32> {
        self.index_map
            .iter()
            .find(|(_, i)| **i == index)
            .map(|(permutation, _)| permutation)
            .unwrap()
    }

    fn contains(&self, permutation: &Vec<u32>) -> bool {
        self.index_map.contains_key(permutation)
    }
}

#[derive(Debug, PartialEq)]
pub struct Facts {
    facts: Vec<PredicateFacts>,
    static_predicates: HashSet<u32>,
    init: HashSet<u32>,
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
        let mut static_predicates: HashSet<u32> = HashSet::new();

        let mut statics: Vec<(u32, Vec<u32>)> = Vec::new();
        for init in inits.iter() {
            let predicate = World::global().get_predicate_index(&init.name);
            let parameters = World::global().get_object_indexes(&init.parameters);
            statics.push((predicate, parameters));
        }

        let mut offset: u32 = 0;
        for (i, predicate) in predicates.predicate_parameters().iter().enumerate() {
            let predicate_name = World::global().get_predicate_name(i as u32);
            status_print(Status::Init, &format!("Grounding: {}", predicate_name));
            // TODO: Make sure this works!
            let is_static =
                check_static_all(actions, i as u32) || check_degrading_all(actions, i as u32);
            println!("is static: {}", is_static);
            if is_static {
                static_predicates.insert(i as u32);
            }
            let predicate_facts = PredicateFacts::new(
                types, objects, i as u32, &statics, is_static, predicate, offset,
            );
            println!("facts: {}", predicate_facts.count());
            offset += predicate_facts.count() as u32;
            facts.push(predicate_facts);
        }

        let init: HashSet<u32> = statics
            .iter()
            .map(|(p, par)| facts[*p as usize].index(par))
            .collect();
        Self {
            facts,
            static_predicates,
            init,
        }
    }

    pub fn index(&self, predicate: u32, parameters: &Vec<u32>) -> u32 {
        self.facts[predicate as usize].index(parameters)
    }

    pub fn fact_predicate(&self, fact_index: u32) -> u32 {
        let mut acc = 0;
        for i in 0..self.facts.len() as u32 {
            acc += self.facts[i as usize].count() as u32;
            if fact_index < acc {
                return i;
            }
        }
        self.facts.len() as u32 - 1
    }

    pub fn fact_parameters(&self, fact_index: u32) -> &Vec<u32> {
        let predicate = self.fact_predicate(fact_index);
        let facts = &self.facts[predicate as usize];
        facts.get_permutation(fact_index)
    }

    pub fn is_static(&self, predicate: u32) -> bool {
        self.static_predicates.contains(&predicate)
    }

    pub fn is_statically_true(&self, predicate: u32, parameters: &Vec<u32>) -> bool {
        self.facts[predicate as usize].contains(parameters)
    }

    pub fn get_static_true(&self) -> Vec<u32> {
        self.init
            .iter()
            .filter_map(|i| match !self.is_static(self.fact_predicate(*i)) {
                true => None,
                false => Some(*i),
            })
            .collect()
    }

    pub fn get_init(&self) -> HashSet<u32> {
        self.init
            .iter()
            .filter_map(|i| match self.is_static(self.fact_predicate(*i)) {
                true => None,
                false => Some(*i),
            })
            .collect()
    }
}

fn check_static(predicate: u32, exp: &Expression) -> bool {
    match exp {
        Expression::Predicate { index, .. } => *index != predicate,
        Expression::And(exp) => exp.iter().all(|exp| check_static(predicate, exp)),
        Expression::Not(exp) => check_static(predicate, exp),
        _ => todo!(),
    }
}

fn check_static_all(actions: &Actions, predicate: u32) -> bool {
    actions
        .actions
        .iter()
        .all(|a| check_static(predicate, &a.effect))
}

fn check_degrading(predicate: u32, exp: &Expression, val: bool) -> bool {
    match exp {
        Expression::Predicate { index, .. } => *index != predicate || !val,
        Expression::And(exp) => exp.iter().all(|exp| check_degrading(predicate, exp, val)),
        Expression::Not(exp) => check_degrading(predicate, exp, !val),
        _ => todo!(),
    }
}

fn check_degrading_all(actions: &Actions, predicate: u32) -> bool {
    actions
        .actions
        .iter()
        .all(|a| check_degrading(predicate, &a.effect, true))
}
