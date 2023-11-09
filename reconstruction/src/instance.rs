use spingus::{domain::Domain, problem::Problem};

use crate::world::World;

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
    pub objects: Objects,
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
        let predicates = Predicates::new(domain.predicates.to_owned());
        let actions = Actions::new(&predicates, domain.actions.to_owned());
        let meta_actions = Actions::new(&predicates, meta_domain.actions.to_owned());
        let objects = Objects::new(problem.objects.to_owned());
        let facts = Facts::new(&types, &predicates, &actions, &objects, &problem.inits);

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
