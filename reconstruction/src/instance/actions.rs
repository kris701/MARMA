use super::{expression::Expression, parameters::Parameters, predicates::Predicates};

#[derive(Debug)]
pub struct Action {
    pub name: String,
    pub parameters: Parameters,
    pub precondition: Option<Expression>,
    pub effect: Expression,
}

impl Action {
    pub fn new(
        predicates: &Predicates,
        name: String,
        parameters: spingus::domain::parameter::Parameters,
        precondition: Option<spingus::domain::action::string_expression::StringExpression>,
        effect: spingus::domain::action::string_expression::StringExpression,
    ) -> Self {
        let parameters = Parameters::new(parameters);
        let precondition = match precondition {
            Some(e) => Some(Expression::new(predicates, &parameters, e)),
            None => None,
        };
        let effect = Expression::new(predicates, &parameters, effect);
        Self {
            name,
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
    pub fn new(predicates: &Predicates, o_actions: spingus::domain::action::Actions) -> Self {
        let mut actions: Vec<Action> = Vec::new();

        for action in o_actions {
            actions.push(Action::new(
                predicates,
                action.name,
                action.parameters,
                action.precondition,
                action.effect,
            ));
        }

        Self { actions }
    }
}
