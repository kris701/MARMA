use super::{
    atom::{convert_expression, Atom},
    objects::Objects,
    parameter::{translate_parameters, Parameters},
    predicates::Predicates,
    types::Types,
    World,
};
use core::fmt;

#[derive(Debug, PartialEq, Eq)]
pub struct Action {
    pub name: String,
    pub parameters: Parameters,
    pub precondition: Vec<Atom>,
    pub effect: Vec<Atom>,
}

impl Action {
    pub fn new(action: spingus::domain::action::Action) -> Self {
        translate_action(
            &World::global().types,
            &World::global().predicates,
            &World::global().objects,
            action,
        )
    }
}

impl fmt::Display for Action {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        writeln!(f, "{}", self.name)?;
        writeln!(f, "parameters: {:?}", self.parameters.names)?;
        writeln!(f, "precondition:")?;
        for atom in self.precondition.iter() {
            writeln!(f, "\t{:?}", atom)?;
        }
        writeln!(f, "effect:")?;
        for atom in self.effect.iter() {
            writeln!(f, "\t{:?}", atom)?;
        }
        Ok(())
    }
}

pub(super) fn translate_action(
    types: &Types,
    predicates: &Predicates,
    objects: &Objects,
    action: spingus::domain::action::Action,
) -> Action {
    let name = action.name;
    let parameters = translate_parameters(types, action.parameters);
    let precondition = match action.precondition {
        Some(e) => convert_expression(predicates, &parameters, e),
        None => vec![],
    };
    let effect = convert_expression(predicates, &parameters, action.effect);
    Action {
        name,
        parameters,
        precondition,
        effect,
    }
}
