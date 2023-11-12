use self::{
    actions::{Action, Actions},
    facts::Facts,
    predicates::Predicates,
};
use crate::{
    tools::{status_print, Status},
    world::World,
};

pub mod actions;
mod expression;
pub mod facts;
pub mod operator;
mod parameters;
pub mod permute;
mod predicates;

pub struct Instance {
    predicates: Predicates,
    actions: Actions,
    meta_actions: Actions,
    pub facts: Facts,
}

impl Instance {
    pub fn new(
        domain: spingus::domain::Domain,
        problem: spingus::problem::Problem,
        meta_domain: spingus::domain::Domain,
    ) -> Self {
        status_print(Status::Init, "Generating predicates");
        let predicates = Predicates::new(domain.predicates.to_owned());
        status_print(Status::Init, "Generating actions");
        let actions = Actions::new(&predicates, domain.actions.to_owned());
        status_print(Status::Init, "Generating meta actions");
        let meta_actions = Actions::new(&predicates, meta_domain.actions.to_owned());
        status_print(Status::Init, "Generating facts");
        let facts = Facts::new(&predicates, &actions, &problem.inits);

        Self {
            predicates,
            actions,
            meta_actions,
            facts,
        }
    }

    pub fn get_action(&self, name: &str) -> &Action {
        match World::global().is_meta_action(name) {
            true => &self.meta_actions.actions[World::global().get_meta_index(name) as usize],
            false => &self.actions.actions[World::global().get_action_index(name) as usize],
        }
    }

    pub fn convert_action(&self, action: spingus::domain::action::Action) -> Action {
        Action::new(
            &self.predicates,
            action.name,
            action.parameters,
            action.precondition,
            action.effect,
        )
    }

    pub fn get_fact_string(&self, index: u32) -> String {
        let predicate = self.facts.fact_predicate(index);
        let predicate = World::global().get_predicate_name(predicate);
        let parameters = self.facts.fact_parameters(index);
        let parameters = World::global().get_object_names(parameters);
        let mut s = format!("{}", predicate);
        for param in parameters {
            s.push_str(&format!(" {}", param));
        }
        s
    }
}
