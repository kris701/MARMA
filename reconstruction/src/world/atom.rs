use spingus::domain::action::string_expression::StringExpression;

use super::{parameter::Parameters, predicates::Predicates};

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

pub(super) fn convert_expression(
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
