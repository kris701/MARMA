use spingus::domain::action::string_expression::StringExpression;

use crate::world::World;

use super::{parameters::Parameters, predicates::Predicates};

#[derive(Debug)]
pub enum Expression {
    Predicate {
        index: usize,
        parameters: Vec<usize>,
    },
    Equal(Vec<usize>),
    And(Vec<Expression>),
    Or(Vec<Expression>),
    Not(Box<Expression>),
    Imply(Box<Expression>, Box<Expression>),
}

impl Expression {
    pub fn new(
        predicates: &Predicates,
        parameters: &Parameters,
        expression: StringExpression,
    ) -> Self {
        match expression {
            StringExpression::Predicate(t) => Expression::Predicate {
                index: World::global().get_predicate_index(&t.name),
                parameters: parameters.get_indexes(&t.parameters),
            },
            StringExpression::Equal(p) => Expression::Equal(parameters.get_indexes(&p)),
            StringExpression::And(e) => Expression::And(
                e.into_iter()
                    .map(|e| Expression::new(predicates, parameters, e))
                    .collect(),
            ),
            StringExpression::Or(e) => Expression::Or(
                e.into_iter()
                    .map(|e| Expression::new(predicates, parameters, e))
                    .collect(),
            ),
            StringExpression::Not(e) => {
                Expression::Not(Box::new(Expression::new(predicates, parameters, *e)))
            }
            StringExpression::Imply(e1, e2) => Expression::Imply(
                Box::new(Expression::new(predicates, parameters, *e1)),
                Box::new(Expression::new(predicates, parameters, *e2)),
            ),
        }
    }
}
