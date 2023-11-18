use super::types::Types;

#[derive(Debug)]
pub struct Parameters {
    pub names: Vec<String>,
    pub types: Vec<usize>,
}

impl Parameters {
    pub fn arity(&self) -> usize {
        self.names.len()
    }

    pub fn index(&self, name: &str) -> usize {
        self.names.iter().position(|n| n == name).unwrap()
    }

    pub fn indexes(&self, names: &Vec<String>) -> Vec<usize> {
        names.iter().map(|n| self.index(n)).collect()
    }
}

pub(super) fn translate_parameters(
    types: &Types,
    parameters: spingus::domain::parameter::Parameters,
) -> Parameters {
    let mut names: Vec<String> = Vec::new();
    let mut type_indexes: Vec<usize> = Vec::new();
    parameters.into_iter().for_each(|p| match p {
        spingus::domain::parameter::Parameter::Untyped { name } => {
            names.push(name);
            type_indexes.push(types.default());
        }
        spingus::domain::parameter::Parameter::Typed { name, type_name } => {
            names.push(name);
            type_indexes.push(types.index(&type_name));
        }
        _ => todo!(),
    });
    Parameters {
        names,
        types: type_indexes,
    }
}
