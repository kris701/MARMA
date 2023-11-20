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
