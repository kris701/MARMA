use core::fmt;

use spingus::domain::action::string_expression::StringExpression;

use super::{
    parameter::{translate_parameters, Parameters},
    predicates::Predicates,
    types::Types,
    World,
};

#[derive(Debug, PartialEq, Eq)]
pub struct Atom {
    /// NOTE: predicate of 0 indicates equality
    pub predicate: usize,
    pub parameters: Vec<usize>,
    pub value: bool,
}

impl Atom {
    pub fn is_nullary(&self) -> bool {
        self.parameters.is_empty()
    }

    pub fn is_unary(&self) -> bool {
        self.parameters.len() == 1
    }

    pub fn is_nary(&self) -> bool {
        self.parameters.len() >= 2
    }

    pub fn map_args(&self, args: &Vec<usize>) -> Vec<usize> {
        self.parameters.iter().map(|p| args[*p]).collect()
    }
}

#[derive(Debug, PartialEq, Eq)]
pub struct Action {
    pub name: String,
    pub parameters: Parameters,
    pub precondition: Vec<Atom>,
    pub effect: Vec<Atom>,
}

impl Action {
    pub fn new(action: spingus::domain::action::Action) -> Self {
        let name = action.name;
        let parameters = translate_parameters(&World::global().types, action.parameters);
        let precondition = match action.precondition {
            Some(e) => translate_expression(&World::global().predicates, &parameters, e),
            None => vec![],
        };
        let effect = translate_expression(&World::global().predicates, &parameters, action.effect);
        Action {
            name,
            parameters,
            precondition,
            effect,
        }
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

fn translate_expression(
    predicates: &Predicates,
    parameters: &Parameters,
    expression: StringExpression,
) -> Vec<Atom> {
    let mut atoms: Vec<Atom> = Vec::new();

    let mut queue: Vec<(StringExpression, bool)> = vec![(expression, true)];

    while !queue.is_empty() {
        let e = queue.pop().unwrap();
        match e {
            (StringExpression::Predicate(p), v) => atoms.push(Atom {
                predicate: predicates.index(&p.name),
                parameters: parameters.indexes(&p.parameters),
                value: v,
            }),
            (StringExpression::Equal(e), v) => atoms.push(Atom {
                predicate: 0,
                parameters: parameters.indexes(&e),
                value: v,
            }),
            (StringExpression::And(e), v) => queue.extend(e.into_iter().map(|e| (e, v))),
            (StringExpression::Not(e), v) => queue.push((*e, !v)),
            _ => todo!(),
        };
    }

    atoms
}

pub(super) fn translate_actions(
    types: &Types,
    predicates: &Predicates,
    actions: spingus::domain::action::Actions,
) -> Vec<Action> {
    actions
        .into_iter()
        .map(|a| {
            let name = a.name;
            let parameters = translate_parameters(types, a.parameters);
            let precondition = match a.precondition {
                Some(e) => translate_expression(predicates, &parameters, e),
                None => vec![],
            };
            let effect = translate_expression(predicates, &parameters, a.effect);
            Action {
                name,
                parameters,
                precondition,
                effect,
            }
        })
        .collect()
}
