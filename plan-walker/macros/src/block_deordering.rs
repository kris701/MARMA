use std::collections::HashSet;

use parsing::{domain::Domain, problem::Problem};
use state::{
    instance::fact::Facts,
    state::{apply_to_state, State},
};

use crate::{blocks::Block, constraints::find_constraint};

pub type DeorderedPlan = Vec<HashSet<Block>>;

fn resolve(state: &State, a: &Block, b: &Block) -> bool {
    find_constraint(state, &a.combined_operator, &b.combined_operator).is_empty()
}

pub fn deorder(
    domain: &Domain,
    problem: &Problem,
    facts: &Facts,
    init: &State,
    blocks: Vec<Block>,
) -> DeorderedPlan {
    if blocks.is_empty() {
        return vec![];
    }

    plan
}
