use super::types::Types;

#[derive(Debug)]
pub struct Parameters {
    pub parameter_names: Vec<String>,
    pub parameter_types: Vec<Option<usize>>,
}

impl Parameters {
    pub fn new(types: &Option<Types>, parameters: spingus::domain::parameter::Parameters) -> Self {
        let mut parameter_names: Vec<String> = Vec::new();
        let mut parameter_types: Vec<Option<usize>> = Vec::new();

        for parameter in parameters {
            match parameter {
                spingus::domain::parameter::Parameter::Untyped { name } => {
                    parameter_types.push(None);
                    parameter_names.push(name);
                }
                spingus::domain::parameter::Parameter::Typed { name, type_name } => {
                    parameter_types.push(Some(
                        match types {
                            Some(val) => val,
                            None => panic!(
                                "Predicate {} has typed parameters in a non-typed domain",
                                name
                            ),
                        }
                        .get_index(&type_name)
                        .to_owned(),
                    ));
                    parameter_names.push(name);
                }
                spingus::domain::parameter::Parameter::Either { .. } => {
                    todo!("Either predicate not implemented")
                }
            }
        }
        Self {
            parameter_names,
            parameter_types,
        }
    }

    pub fn get_index(&self, parameter_name: &str) -> usize {
        match self
            .parameter_names
            .iter()
            .position(|p| p == parameter_name)
        {
            Some(i) => i,
            None => {
                panic!(
                    "Could not find parameter with name '{}'. Actual parameters: {:?}",
                    parameter_name, self.parameter_names
                )
            }
        }
    }

    pub fn get_indexes(&self, parameter_names: &Vec<String>) -> Vec<usize> {
        parameter_names.iter().map(|p| self.get_index(p)).collect()
    }
}
