use parsing::{domain::Domain, problem::Problem, sas::SASPlan};

use crate::{
    expression::Expression,
    instance::{fact::Facts, Instance},
};

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

pub fn next_init(instance: &Instance, sas_plan: &SASPlan) -> Plan {
    let steps = sas_plan.steps[0..sas_plan.meta_pos().unwrap()]
        .iter()
        .map(|i| convert_step(&instance.domain, &instance.problem, &instance.facts, &i.0))
        .collect();
    Plan { steps }
}

pub fn next_goal(instance: &Instance, meta_domain: &Domain, sas_plan: &SASPlan) -> Plan {
    let mut steps: Vec<Expression> = sas_plan.steps[0..sas_plan.meta_pos().unwrap()]
        .iter()
        .map(|i| convert_step(&instance.domain, &instance.problem, &instance.facts, &i.0))
        .collect();
    steps.push(convert_step(
        meta_domain,
        &instance.problem,
        &instance.facts,
        &sas_plan.steps[sas_plan.meta_pos().unwrap()].0,
    ));
    Plan { steps }
}
