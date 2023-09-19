use parsing::{domain::Domain, problem::Problem, sas::SASPlan};

use crate::instance::{expression::Expression, fact::Facts};

#[derive(Debug, PartialEq)]
pub struct Plan {
    pub steps: Vec<Expression>,
}

fn convert_step(
    domain: &Domain,
    problem: &Problem,
    facts: &Facts,
    i: &parsing::term::Term,
) -> Expression {
    let action = domain
        .actions
        .iter()
        .find(|action| action.name == i.name)
        .unwrap();
    let parameters = i
        .parameters
        .iter()
        .map(|par| {
            problem
                .objects
                .iter()
                .position(|o| o.name.to_lowercase() == *par.to_lowercase())
                .unwrap()
        })
        .collect();
    Expression::new(domain, facts, action, &action.effect, &parameters)
}

pub fn next_init(domain: &Domain, problem: &Problem, facts: &Facts, sas_plan: &SASPlan) -> Plan {
    let steps = sas_plan.steps[0..sas_plan.meta_pos().unwrap()]
        .iter()
        .map(|i| convert_step(domain, problem, facts, &i.0))
        .collect();
    Plan { steps }
}

pub fn next_goal(
    meta_domain: &Domain,
    domain: &Domain,
    problem: &Problem,
    facts: &Facts,
    sas_plan: &SASPlan,
) -> Plan {
    let mut steps: Vec<Expression> = sas_plan.steps[0..sas_plan.meta_pos().unwrap()]
        .iter()
        .map(|i| convert_step(domain, problem, facts, &i.0))
        .collect();
    steps.push(convert_step(
        meta_domain,
        problem,
        facts,
        &sas_plan.steps[sas_plan.meta_pos().unwrap()].0,
    ));
    Plan { steps }
}
