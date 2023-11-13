use super::parameters::Parameters;
#[derive(Debug)]
pub struct Predicates {
    predicate_parameters: Vec<Parameters>,
}

impl Predicates {
    pub fn new(o_predicates: spingus::domain::predicate::Predicates) -> Self {
        let mut predicate_parameters: Vec<Parameters> = Vec::new();

        for predicate in o_predicates {
            predicate_parameters.push(Parameters::new(predicate.parameters));
        }

        Self {
            predicate_parameters,
        }
    }
}
