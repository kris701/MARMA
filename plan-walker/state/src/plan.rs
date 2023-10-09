use parsing::{domain::Domain, problem::Problem, sas::SASPlan};

use crate::{
    bit_expression::BitExp,
    expression::Expression,
    instance::{
        fact::Facts,
        operator::{extract_from_action, Operator},
        Instance,
    },
};

#[derive(Clone, Debug, Hash, PartialEq, Eq)]
pub struct Step {
    pub operator: Operator,
    pub action_index: usize,
    pub parameters: Vec<usize>,
}

impl Step {
    pub fn new(
        domain: &Domain,
        facts: &Facts,
        action_index: usize,
        parameters: Vec<usize>,
    ) -> Self {
        let operator =
            extract_from_action(&parameters, &domain.actions[action_index], facts).unwrap();
        Self {
            operator,
            action_index,
            parameters,
        }
    }
}

#[derive(Debug, PartialEq)]
pub struct Plan {
    pub steps: Vec<Step>,
}

impl Plan {
    pub fn new(domain: &Domain, problem: &Problem, facts: &Facts, sas: SASPlan) -> Self {
        let steps = sas
            .steps
            .iter()
            .map(|s| convert_step(domain, problem, facts, &s.0))
            .collect();
        Self { steps }
    }
}

fn convert_step(
    domain: &Domain,
    problem: &Problem,
    facts: &Facts,
    i: &parsing::term::Term,
) -> Step {
    let action = domain
        .actions
        .iter()
        .position(|action| action.name == i.name)
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
    Step::new(domain, facts, action, parameters)
}

pub fn next_init(instance: &Instance, sas_plan: &SASPlan) -> Plan {
    let steps = sas_plan.steps[0..sas_plan.meta_pos().unwrap()]
        .iter()
        .map(|i| convert_step(&instance.domain, &instance.problem, &instance.facts, &i.0))
        .collect();
    Plan { steps }
}

pub fn next_goal(instance: &Instance, meta_domain: &Domain, sas_plan: &SASPlan) -> Plan {
    let mut steps: Vec<Step> = sas_plan.steps[0..sas_plan.meta_pos().unwrap()]
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
