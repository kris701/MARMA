use spingus::{domain::Domain, problem::Problem};

use self::{
    actions::{Action, Actions},
    facts::Facts,
    objects::Objects,
    predicates::Predicates,
    types::Types,
};

pub mod actions;
mod expression;
pub mod facts;
mod objects;
pub mod operator;
mod parameters;
mod permute;
mod predicates;
mod types;

pub struct Instance {
    types: Option<Types>,
    predicates: Predicates,
    actions: Actions,
    meta_actions: Actions,
    objects: Objects,
    pub domain: Domain,
    pub problem: Problem,
    pub meta_domain: Domain,
    pub facts: Facts,
}

impl Instance {
    pub fn new(
        domain: spingus::domain::Domain,
        problem: spingus::problem::Problem,
        meta_domain: spingus::domain::Domain,
    ) -> Self {
        let types = match domain.types.to_owned() {
            Some(types) => Some(Types::new(types)),
            None => None,
        };
        let predicates = Predicates::new(&types, domain.predicates.to_owned());
        let actions = Actions::new(&types, &predicates, domain.actions.to_owned());
        let meta_actions = Actions::new(&types, &predicates, meta_domain.actions.to_owned());
        let objects = Objects::new(&types, problem.objects.to_owned());
        let facts = Facts::new(&types, &predicates, &actions, &objects);

        Self {
            types,
            predicates,
            actions,
            meta_actions,
            objects,
            domain,
            problem,
            meta_domain,
            facts,
        }
    }

    pub fn get_action(&self, name: &str) -> &Action {
        if let Some(action) = self.meta_actions.get_by_name(name) {
            action
        } else if let Some(action) = self.actions.get_by_name(name) {
            action
        } else {
            panic!()
        }
    }

    pub fn convert_action(&self, action: spingus::domain::action::Action) -> Action {
        Action::new(
            &self.types,
            &self.predicates,
            action.parameters,
            action.precondition,
            action.effect,
        )
    }
}
