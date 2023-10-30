use std::collections::HashMap;

use super::{expression::Expression, parameters::Parameters, predicates::Predicates, types::Types};

#[derive(Debug)]
pub struct Action {
    pub name: String,
    pub parameters: Parameters,
    pub precondition: Option<Expression>,
    pub effect: Expression,
}

impl Action {
    pub fn new(
        types: &Option<Types>,
        predicates: &Predicates,
        name: String,
        parameters: spingus::domain::parameter::Parameters,
        precondition: Option<spingus::domain::action::string_expression::StringExpression>,
        effect: spingus::domain::action::string_expression::StringExpression,
    ) -> Self {
        let parameters = Parameters::new(types, parameters);
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
    index_map: HashMap<String, usize>,
    pub actions: Vec<Action>,
}

impl Actions {
    pub fn new(
        types: &Option<Types>,
        predicates: &Predicates,
        o_actions: spingus::domain::action::Actions,
    ) -> Self {
        let mut index_map: HashMap<String, usize> = HashMap::new();
        let mut actions: Vec<Action> = Vec::new();

        for action in o_actions {
            index_map.insert(action.name.to_owned(), index_map.len());
            actions.push(Action::new(
                types,
                predicates,
                action.name,
                action.parameters,
                action.precondition,
                action.effect,
            ));
        }

        Self { index_map, actions }
    }

    pub fn index(&self, action_name: &str) -> Option<usize> {
        self.index_map.get(action_name).copied()
    }

    pub fn get_by_name(&self, action_name: &str) -> Option<&Action> {
        let index = self.index_map.get(action_name)?;
        Some(&self.actions[*index])
    }
}
