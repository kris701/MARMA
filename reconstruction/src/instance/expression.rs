use spingus::domain::action::string_expression::StringExpression;

use crate::world::World;

use super::{parameters::Parameters, predicates::Predicates};

#[derive(Debug)]
pub struct Literal {
    pub predicate: u32,
    pub parameters: Vec<u32>,
    pub value: bool,
}

#[derive(Debug)]
pub struct Equal {
    pub parameters: Vec<u32>,
    pub value: bool,
}

#[derive(Debug)]
pub struct Expression {
    pub literals: Vec<Literal>,
    pub equals: Vec<Equal>,
}

impl Expression {
    pub fn new(
        predicates: &Predicates,
        parameters: &Parameters,
        expression: StringExpression,
    ) -> Self {
        let mut literals: Vec<Literal> = Vec::new();
        let mut equals: Vec<Equal> = Vec::new();
        extract(
            predicates,
            parameters,
            &expression,
            &mut literals,
            &mut equals,
            true,
        );
        literals.sort_by(|a, b| a.parameters.len().cmp(&b.parameters.len()));
        Self { literals, equals }
    }
}

fn extract(
    predicates: &Predicates,
    parameters: &Parameters,
    expression: &StringExpression,
    literals: &mut Vec<Literal>,
    equals: &mut Vec<Equal>,
    value: bool,
) {
    match expression {
        StringExpression::Predicate(p) => literals.push(Literal {
            predicate: World::global().get_predicate_index(&p.name),
            parameters: parameters.get_indexes(&p.parameters),
            value,
        }),
        StringExpression::And(e) => e
            .into_iter()
            .for_each(|e| extract(predicates, parameters, e, literals, equals, value)),
        StringExpression::Equal(e) => equals.push(Equal {
            parameters: parameters.get_indexes(&e),
            value,
        }),
        StringExpression::Not(e) => extract(predicates, parameters, e, literals, equals, !value),
        StringExpression::Or(_) => todo!("Or expressions are not implemented"),
        StringExpression::Imply(_, _) => todo!("Imply in expressions are not implemented"),
    }
}
