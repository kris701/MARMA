use crate::world::World;

#[derive(Debug)]
pub struct Parameters {
    pub parameter_names: Vec<String>,
    pub parameter_types: Vec<u16>,
}

impl Parameters {
    pub fn new(parameters: spingus::domain::parameter::Parameters) -> Self {
        let mut parameter_names: Vec<String> = Vec::new();
        let mut parameter_types: Vec<u16> = Vec::new();

        for parameter in parameters {
            match parameter {
                spingus::domain::parameter::Parameter::Untyped { name } => {
                    parameter_types.push(World::global().get_default_type_index());
                    parameter_names.push(name);
                }
                spingus::domain::parameter::Parameter::Typed { name, type_name } => {
                    parameter_types.push(World::global().get_type_index(&type_name));
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

    pub fn get_index(&self, parameter_name: &str) -> u16 {
        match self
            .parameter_names
            .iter()
            .position(|p| p == parameter_name)
        {
            Some(i) => i as u16,
            None => {
                panic!(
                    "Could not find parameter with name '{}'. Actual parameters: {:?}",
                    parameter_name, self.parameter_names
                )
            }
        }
    }

    pub fn get_indexes(&self, parameter_names: &Vec<String>) -> Vec<u16> {
        parameter_names.iter().map(|p| self.get_index(p)).collect()
    }
}
