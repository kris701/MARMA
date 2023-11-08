mod cache_data;
pub mod generation;
mod hash_cache;

use crate::{instance::Instance, state::State};
use spingus::{sas_plan::SASPlan, term::Term};

pub trait Cache {
    /// Retrives replacement from cache from given init to goal
    fn get_replacement(
        &self,
        instance: &Instance,
        meta_term: &Term,
        init: &State,
        goal: &State,
    ) -> Option<SASPlan>;
}
