use spingus::problem::init::Inits;
use std::collections::HashSet;

use crate::world::World;

use super::actions::Actions;

#[derive(Debug, PartialEq)]
pub struct Facts {
    init: Vec<u64>,
}

impl Facts {
    pub fn new(_actions: &Actions, inits: &Inits) -> Self {
        let init: Vec<u64> = inits
            .iter()
            .map(|init| {
                let predicate = World::global().get_predicate_index(&init.name);
                let parameters = World::global().get_object_indexes(&init.parameters);
                Facts::index(predicate, &parameters)
            })
            .collect();
        Self { init }
    }

    pub fn index(predicate: u16, parameters: &Vec<u16>) -> u64 {
        debug_assert!(parameters.len() <= 3);
        predicate as u64
            + parameters
                .iter()
                .enumerate()
                .map(|(i, p)| (*p as u64) << 16 * (i + 1))
                .sum::<u64>()
    }

    pub fn fact_predicate(fact_index: u64) -> u16 {
        fact_index as u16
    }

    pub fn fact_parameters(mut fact_index: u64) -> Vec<u16> {
        let mut parameters: Vec<u16> = Vec::new();
        fact_index = fact_index >> 16;
        while fact_index != 0 {
            parameters.push(fact_index as u16);
            fact_index = fact_index >> 16;
        }
        parameters
    }

    pub fn is_static(&self, _predicate: u16) -> bool {
        false // todo
    }

    pub fn is_statically_true(&self, _predicate: u16, _parameters: &Vec<u16>) -> bool {
        false // todo
    }

    pub fn get_static_true(&self) -> Vec<u64> {
        vec![] // todo
    }

    pub fn get_init(&self) -> HashSet<u64> {
        self.init.iter().map(|x| *x).collect()
    }
}
