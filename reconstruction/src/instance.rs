use self::actions::{Action, Actions};
use crate::{
    tools::{status_print, Status},
    world::World,
};

pub mod actions;
pub mod expression;
mod parameters;

pub struct Instance {
    pub actions: Actions,
    meta_actions: Actions,
}

impl Instance {
    pub fn new(domain: spingus::domain::Domain, meta_domain: spingus::domain::Domain) -> Self {
        status_print(Status::Init, "Generating actions");
        let actions = Actions::new(domain.actions.to_owned());
        status_print(Status::Init, "Generating meta actions");
        let meta_actions = Actions::new(meta_domain.actions.to_owned());

        Self {
            actions,
            meta_actions,
        }
    }

    pub fn get_action(&self, name: &str) -> &Action {
        match World::global().is_meta_action(name) {
            true => &self.meta_actions.actions[World::global().get_meta_index(name) as usize],
            false => &self.actions.actions[World::global().get_action_index(name) as usize],
        }
    }
}
