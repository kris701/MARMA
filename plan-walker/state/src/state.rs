use parsing::{domain::Domain, problem::Problem};

use crate::{
    expression::Expression,
    instance::fact::{Fact, Facts},
    plan::Plan,
};

#[derive(Clone, Hash, Debug, PartialEq)]
pub struct State {
    pub values: Vec<bool>,
}

fn convert_init(
    domain: &Domain,
    problem: &Problem,
    i: &parsing::term::Term,
    facts: &Facts,
) -> usize {
    let predicate = domain
        .predicates
        .iter()
        .position(|pre| pre.name == i.name)
        .unwrap();
    let parameters = i
        .parameters
        .iter()
        .map(|par| problem.objects.iter().position(|o| o.name == *par).unwrap())
        .collect();

    facts
        .fact_map
        .get(&Fact {
            predicate,
            parameters,
        })
        .unwrap()
        .to_owned()
}

impl State {
    pub fn new(domain: &Domain, problem: &Problem, facts: &Facts) -> Self {
        let inits: Vec<usize> = problem
            .inits
            .iter()
            .map(|i| convert_init(domain, problem, i, facts))
            .collect();
        let values = (0..facts.facts.len())
            .map(|num| inits.contains(&num))
            .collect();
        State { values }
    }

    fn apply_term(&mut self, val: bool, i: &usize) {
        self.values[*i] = val;
    }

    fn apply_internal(&mut self, expression: &Expression, val: bool) {
        match expression {
            Expression::Term(i) => self.apply_term(val, i),
            Expression::Not(e) => self.apply_internal(e, !val),
            Expression::And(e) => {
                e.iter()
                    .filter(|e| match e {
                        Expression::Not(_) => true,
                        _ => false,
                    })
                    .for_each(|e| self.apply_internal(e, val));
                e.iter()
                    .filter(|e| match e {
                        Expression::Not(_) => false,
                        _ => true,
                    })
                    .for_each(|e| self.apply_internal(e, val));
            }
            Expression::Equal(_) => panic!("Cannot apply equal expression"),
            Expression::Or(_) => panic!("Cannot apply or expression"),
        }
    }

    pub fn apply(&mut self, expression: &Expression) {
        self.apply_internal(expression, true)
    }
    pub fn apply_plan(&self, plan: &Plan) -> State {
        let mut new_state = self.clone();
        plan.steps.iter().for_each(|step| new_state.apply(step));
        return new_state;
    }
}
