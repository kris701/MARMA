use bitvec::prelude::*;
use bitvec::vec::BitVec;
use itertools::Itertools;
use parsing::{
    domain::{
        action::{string_expression::StringExpression, Action},
        parameter::Parameter::{Typed, Untyped},
    },
    term::Term,
};

use crate::instance::fact::Facts;

pub type BitExp = BitVec;
pub fn generate(facts: &Facts) -> BitExp {
    bitvec!(usize, Lsb0; 0; facts.count())
}

fn match_parameter(action: &Action, parameters: &Vec<usize>, parameter: &str) -> usize {
    parameters
        .get(
            action
                .parameters
                .iter()
                .position(|p2| match p2 {
                    Untyped { name } => name == parameter,
                    Typed { name, type_name: _ } => name == parameter,
                })
                .unwrap(),
        )
        .unwrap()
        .to_owned()
}

fn evaluate_term(
    facts: &Facts,
    action: &Action,
    parameters: &Vec<usize>,
    term: &Term,
    pos: &mut BitExp,
    neg: &mut BitExp,
    val: bool,
) -> bool {
    let exp = if val { pos } else { neg };
    let pred_index = facts.predicate_map[&term.name];
    let parameter_indexes: Vec<usize> = term
        .parameters
        .iter()
        .map(|p| match_parameter(action, parameters, p))
        .collect();
    let fact = facts.index(pred_index, &parameter_indexes);
    exp.set(fact, val);

    true
}

fn evaluate_equality(
    action: &Action,
    exp: &Vec<String>,
    parameters: &Vec<usize>,
    val: bool,
) -> bool {
    let is_equal = exp
        .iter()
        .map(|p| match_parameter(action, parameters, p))
        .all_equal();
    is_equal == val
}

fn extract_internal(
    facts: &Facts,
    action: &Action,
    parameters: &Vec<usize>,
    exp: &StringExpression,
    pos: &mut BitExp,
    neg: &mut BitExp,
    val: bool,
) -> bool {
    match exp {
        StringExpression::Predicate(e) => {
            evaluate_term(facts, action, parameters, e, pos, neg, val)
        }
        StringExpression::And(e) => e
            .iter()
            .all(|e| extract_internal(facts, action, parameters, e, pos, neg, val)),
        StringExpression::Not(e) => extract_internal(facts, action, parameters, e, pos, neg, !val),
        StringExpression::Equal(e) => evaluate_equality(action, e, parameters, val),
        StringExpression::Or(_) => todo!("Or expressions are not implemented yet for actions"),
    }
}

pub fn extract(
    facts: &Facts,
    action: &Action,
    parameters: &Vec<usize>,
    exp: &Option<StringExpression>,
) -> Result<(BitExp, BitExp), &'static str> {
    let mut neg = generate(&facts);
    let mut pos = generate(&facts);
    let valid = match exp {
        Some(exp) => extract_internal(facts, action, parameters, exp, &mut pos, &mut neg, true),
        None => true,
    };
    if valid {
        Ok((neg, pos))
    } else {
        Err("Invalid parameters")
    }
}
