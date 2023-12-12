use crate::world::{action::Action, World};
use itertools::Itertools;
use spingus::sas_plan::{parse_sas, SASPlan};
use std::fs;

#[derive(Debug, Clone)]
pub struct Step {
    pub action: usize,
    pub args: Vec<usize>,
}

impl Step {
    pub fn is_meta(&self) -> bool {
        World::global().actions.is_meta(self.action)
    }

    pub fn export(&self) -> spingus::term::Term {
        spingus::term::Term {
            name: World::global().actions.name(self.action).to_owned(),
            parameters: World::global().objects.names_cloned(&self.args),
        }
    }
}

fn convert_step(step: &spingus::term::Term) -> Step {
    Step {
        action: World::global().actions.index(&step.name),
        args: World::global().objects.indexes(&step.parameters),
    }
}

#[derive(Debug, Clone)]
pub struct Plan(pub Vec<Step>);

impl Plan {
    pub fn new(steps: Vec<Step>) -> Self {
        Plan(steps)
    }

    pub fn iter(&self) -> impl Iterator<Item = &Step> {
        self.0.iter()
    }

    pub fn into_iter(self) -> impl Iterator<Item = Step> {
        self.0.into_iter()
    }

    pub fn len(&self) -> usize {
        self.0.len()
    }

    pub fn meta_count(&self) -> usize {
        self.0.iter().filter(|s| s.is_meta()).count()
    }

    pub fn meta_count_unique(&self) -> usize {
        self.0
            .iter()
            .filter(|s| s.is_meta())
            .unique_by(|s| s.action)
            .count()
    }

    pub fn export(&self) -> SASPlan {
        self.0.iter().map(|s| s.export()).collect()
    }
}

pub fn convert_plan(sas_plan: &SASPlan) -> Plan {
    Plan(sas_plan.iter().map(|s| convert_step(s)).collect())
}

pub fn convert_replacement_plan(action: &Action, sas_plan: &SASPlan) -> Plan {
    Plan(
        sas_plan
            .iter()
            .map(|s| Step {
                action: World::global().actions.index(&s.name),
                args: s
                    .parameters
                    .iter()
                    .map(|p| action.parameters.index(&p))
                    .collect(),
            })
            .collect(),
    )
}

pub fn read_plan(path: &String) -> Result<Plan, String> {
    let plan = fs::read_to_string(path).map_err(|e| e.to_string())?;
    let plan = parse_sas(&plan)?;
    Ok(convert_plan(&plan))
}
