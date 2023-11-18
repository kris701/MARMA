use super::{expression::Expression, parameters::Parameters};

#[derive(Debug)]
pub struct Action {
    pub name: String,
    pub parameters: Parameters,
    pub precondition: Option<Expression>,
    pub effect: Expression,
}

impl Action {
    pub fn new(action: spingus::domain::action::Action) -> Self {
        let parameters = Parameters::new(action.parameters);
        let precondition = match action.precondition {
            Some(e) => Some(Expression::new(&parameters, e)),
            None => None,
        };
        let effect = Expression::new(&parameters, action.effect);
        Self {
            name: action.name,
            parameters,
            precondition,
            effect,
        }
    }
}

#[derive(Debug)]
pub struct Actions {
    pub actions: Vec<Action>,
}

impl Actions {
    pub fn new(o_actions: spingus::domain::action::Actions) -> Self {
        let actions = o_actions.into_iter().map(|a| Action::new(a)).collect();
        Self { actions }
    }
}
