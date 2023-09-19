use parsing::{
    domain::{action::string_expression::StringExpression, Domain},
    term::Term,
};

use crate::instance::fact::{Fact, Facts};

#[derive(Debug, PartialEq)]
pub enum Expression {
    Term(usize),
    Not(Box<Expression>),
    And(Vec<Expression>),
    Or(Vec<Expression>),
    Equal(Vec<Expression>),
}

fn convert_term(
    domain: &Domain,
    facts: &Facts,
    action: &parsing::domain::action::Action,
    parameters: &Vec<usize>,
    e: &Term,
) -> Expression {
    let fact = Fact {
        predicate: domain
            .predicates
            .iter()
            .position(|p| p.name == e.name)
            .unwrap(),
        parameters: e
            .parameters
            .iter()
            .map(|p| {
                parameters
                    .get(
                        action
                            .parameters
                            .iter()
                            .position(|p2| match p2 {
                                parsing::domain::parameter::Parameter::Untyped { name } => {
                                    name == p
                                }
                                parsing::domain::parameter::Parameter::Typed {
                                    name,
                                    type_name: _,
                                } => name == p,
                            })
                            .unwrap(),
                    )
                    .unwrap()
                    .to_owned()
            })
            .collect(),
    };
    Expression::Term(*facts.fact_map.get(&fact).unwrap())
}

fn convert_children(
    domain: &Domain,
    facts: &Facts,
    action: &parsing::domain::action::Action,
    parameters: &Vec<usize>,
    exp: &Vec<StringExpression>,
) -> Vec<Expression> {
    exp.iter()
        .map(|e_s| convert(domain, facts, action, parameters, e_s))
        .collect()
}

fn convert(
    domain: &Domain,
    facts: &Facts,
    action: &parsing::domain::action::Action,
    parameters: &Vec<usize>,
    expression: &StringExpression,
) -> Expression {
    match expression {
        StringExpression::Predicate(e) => convert_term(domain, facts, action, parameters, e),
        StringExpression::Equal(e) => {
            Expression::Equal(convert_children(domain, facts, action, parameters, e))
        }
        StringExpression::And(e) => {
            Expression::And(convert_children(domain, facts, action, parameters, e))
        }
        StringExpression::Or(e) => {
            Expression::Or(convert_children(domain, facts, action, parameters, e))
        }
        StringExpression::Not(e) => {
            Expression::Not(Box::new(convert(domain, facts, action, parameters, e)))
        }
    }
}

impl Expression {
    pub fn new(
        domain: &Domain,
        facts: &Facts,
        action: &parsing::domain::action::Action,
        expression: &StringExpression,
        parameters: &Vec<usize>,
    ) -> Self {
        convert(domain, facts, action, parameters, expression)
    }
}
