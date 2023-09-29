use parsing::domain::action::{Action, Actions};
use parsing::domain::types::Types;
use parsing::domain::Domain;
use parsing::problem::Problem;

use crate::bit_expression::{extract, generate, BitExp};

use super::fact::Facts;
use super::permutation::permute;

pub struct Operator {
    pub has: BitExp,
    pub not: BitExp,
    pub add: BitExp,
    pub del: BitExp,
}

pub struct Operators {
    operators: Vec<Operator>,
}

impl Operators {
    pub fn new(domain: &Domain, problem: &Problem, facts: &Facts) -> Self {
        let mut operators = Vec::<Operator>::new();
        for action in domain.actions.iter() {
            for permutation in permute(&domain.types, problem, &action.parameters) {
                let operator = extract_from_action(permutation, &action, &domain.types, facts);
                if let Ok(operator) = operator {
                    operators.push(operator);
                }
            }
        }
        Operators { operators }
    }

    pub fn count(&self) -> usize {
        self.operators.len()
    }
}

pub fn extract_from_action(
    parameters: Vec<usize>,
    action: &Action,
    types: &Option<Types>,
    facts: &Facts,
) -> Result<Operator, &'static str> {
    let (pre_neg, pre_pos) = extract(facts, action, &parameters, &action.precondition)?;
    let (eff_neg, eff_pos) = extract(facts, action, &parameters, &Some(action.effect.clone()))?;
    Ok(Operator {
        has: pre_pos,
        not: pre_neg,
        add: eff_pos,
        del: eff_neg,
    })
}
