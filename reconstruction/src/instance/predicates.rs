use super::{parameters::Parameters, types::Types};
use std::collections::HashMap;

#[derive(Debug)]
pub struct Predicates {
    index_map: HashMap<String, usize>,
    predicate_parameters: Vec<Parameters>,
}

impl Predicates {
    pub fn new(
        types: &Option<Types>,
        o_predicates: spingus::domain::predicate::Predicates,
    ) -> Self {
        let mut index_map: HashMap<String, usize> = HashMap::new();
        let mut predicate_parameters: Vec<Parameters> = Vec::new();

        for predicate in o_predicates {
            let index = index_map.len();

            index_map.insert(predicate.name, index);
            predicate_parameters.push(Parameters::new(types, predicate.parameters));
        }

        Self {
            index_map,
            predicate_parameters,
        }
    }

    pub fn get_index(&self, predicate_name: &str) -> usize {
        self.index_map[predicate_name]
    }

    pub fn get_name(&self, predicate_index: usize) -> &String {
        &self
            .index_map
            .iter()
            .find(|(_, index)| **index == predicate_index)
            .unwrap()
            .0
    }

    pub fn predicate_parameters(&self) -> &Vec<Parameters> {
        &self.predicate_parameters
    }
}
