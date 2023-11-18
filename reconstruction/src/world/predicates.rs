use std::collections::{HashMap, HashSet};

use spingus::domain::action::string_expression::StringExpression;

use super::{
    parameter::{translate_parameters, Parameters},
    types::Types,
};

pub struct Predicates {
    index_map: HashMap<String, u16>,
    static_predicates: HashSet<u16>,
    parameters: HashMap<u16, Parameters>,
}

impl Predicates {
    pub fn count(&self) -> usize {
        self.index_map.len()
    }

    pub fn index(&self, name: &str) -> u16 {
        self.index_map[name]
    }

    pub fn name(&self, index: u16) -> &String {
        self.index_map.iter().find(|(_, v)| **v == index).unwrap().0
    }

    pub fn arity(&self, index: u16) -> usize {
        self.parameters[&index].len()
    }

    pub fn is_static(&self, index: u16) -> bool {
        self.static_predicates.contains(&index)
    }
}

fn find_static_predicates(
    actions: &spingus::domain::action::Actions,
    index_map: &HashMap<String, u16>,
) -> HashSet<u16> {
    let mut mutable_predicates: HashSet<u16> = HashSet::new();
    let mut queue: Vec<&StringExpression> = actions.iter().map(|a| &a.effect).collect();

    while !queue.is_empty() {
        let e: &StringExpression = queue.pop().unwrap();
        match e {
            StringExpression::Predicate(p) => {
                mutable_predicates.insert(index_map[&p.name]);
            }
            StringExpression::And(e) => queue.extend(e),
            StringExpression::Not(e) => queue.push(e),
            _ => {}
        };
    }

    index_map
        .iter()
        .filter(|(_, v)| !mutable_predicates.contains(v))
        .map(|(_, v)| *v)
        .collect()
}

pub(super) fn translate_predicates(
    types: &Types,
    actions: &spingus::domain::action::Actions,
    predicates: spingus::domain::predicate::Predicates,
) -> Predicates {
    let index_map: HashMap<String, u16> = predicates
        .iter()
        .enumerate()
        .map(|(i, p)| (p.name.to_owned(), i as u16 + 1))
        .collect();
    let parameters: HashMap<u16, Parameters> = predicates
        .into_iter()
        .map(|p| {
            let index = index_map[&p.name];
            (index, translate_parameters(types, p.parameters))
        })
        .collect();
    let static_predicates = find_static_predicates(actions, &index_map);
    println!("predicate_count={}", index_map.len());
    println!("static_predicates={}", static_predicates.len());

    Predicates {
        index_map,
        static_predicates,
        parameters,
    }
}
