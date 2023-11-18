use super::types::Types;

#[derive(Debug)]
pub struct Parameter {
    pub name: String,
    pub type_index: usize,
}

pub type Parameters = Vec<Parameter>;

pub(super) fn translate_parameters(
    types: &Types,
    parameters: spingus::domain::parameter::Parameters,
) -> Parameters {
    parameters
        .into_iter()
        .map(|p| match p {
            spingus::domain::parameter::Parameter::Untyped { name } => Parameter {
                name,
                type_index: types.default(),
            },
            spingus::domain::parameter::Parameter::Typed { name, type_name } => Parameter {
                name,
                type_index: types.index(&type_name),
            },
            _ => todo!(),
        })
        .collect()
}
