use spingus::domain::action::{string_expression::StringExpression, Action};

fn clean_string_expression(expression: StringExpression) -> Option<StringExpression> {
    match expression {
        StringExpression::Predicate(e) => {
            if e.name.contains("stai_") || e.name.contains("stag_") {
                None
            } else {
                Some(StringExpression::Predicate(e))
            }
        }
        StringExpression::Equal(e) => {
            let out: Vec<String> = e
                .iter()
                .filter_map(|e| {
                    if e.contains("stai_") || e.contains("stag_") {
                        None
                    } else {
                        Some(e.to_owned())
                    }
                })
                .collect();
            Some(StringExpression::Equal(out))
        }
        StringExpression::And(e) => {
            let out: Vec<StringExpression> = e
                .iter()
                .filter_map(|e| clean_string_expression(e.to_owned()))
                .collect();
            if out.is_empty() {
                None
            } else {
                Some(StringExpression::And(out))
            }
        }
        StringExpression::Or(_) => todo!(),
        StringExpression::Not(e) => {
            let out = clean_string_expression(*e);
            out.map_or(None, |o| Some(StringExpression::Not(Box::new(o))))
        }
    }
}

pub fn clean_action(action: Action) -> Action {
    let name = action.name;
    let parameters = action.parameters;
    let precondition = action
        .precondition
        .map_or(None, |p| clean_string_expression(p));
    let effect = clean_string_expression(action.effect).unwrap();
    Action {
        name,
        parameters,
        precondition,
        effect,
    }
}
