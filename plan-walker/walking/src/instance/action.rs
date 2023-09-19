pub mod expression;

use parsing::{
    domain::{
        action::{self},
        Domain,
    },
    problem::Problem,
};

use self::expression::Expression;

use super::{fact::Facts, permutation::permute};

pub struct Action {
    pub precondition: Option<Expression>,
    pub effect: Expression,
}
impl Action {
    fn new(
        domain: &Domain,
        facts: &Facts,
        action: &action::Action,
        permutation: &Vec<usize>,
    ) -> Self {
        let precondition = match &action.precondition {
            Some(p) => Some(Expression::new(domain, facts, action, &p, permutation)),
            None => None,
        };
        let effect = Expression::new(domain, facts, action, &action.effect, permutation);
        Action {
            precondition,
            effect,
        }
    }
}

pub struct Actions {
    pub actions: Vec<Action>,
}

fn generate_actions(
    domain: &Domain,
    problem: &Problem,
    facts: &Facts,
    action: &action::Action,
) -> Vec<Action> {
    let permutations = permute(&domain.types, problem, &action.parameters);
    println!("{}: {}", action.name, permutations.len());
    permutations
        .iter()
        .map(|permutation| Action::new(domain, facts, action, permutation))
        .collect()
}

impl Actions {
    pub fn new(domain: &Domain, problem: &Problem, facts: &Facts) -> Self {
        let actions = domain
            .actions
            .iter()
            .flat_map(|action| generate_actions(domain, problem, facts, action))
            .collect();
        Actions { actions }
    }
}
