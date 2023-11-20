use std::collections::HashMap;

use itertools::Itertools;

use crate::{
    fact::Fact,
    world::{
        action::{Action, Atom},
        parameter::Parameters,
        World,
    },
};

// NOTE: Assumes that is a legal macro
pub fn generate_macro(operators: Vec<(&Action, Vec<usize>)>) -> Action {
    let name: String = operators
        .iter()
        .map(|(a, ..)| format!("_{}", a.name))
        .collect();
    let mut cul_args: Vec<usize> = Vec::new();
    let mut cul_pre: HashMap<Fact, bool> = HashMap::new();
    let mut cul_eff: HashMap<Fact, bool> = HashMap::new();

    for (action, args) in operators.iter() {
        let new_args = args.iter().filter(|a| !cul_args.contains(a)).collect_vec();
        cul_args.extend(new_args);
        combine_pre(&mut cul_pre, &cul_eff, &action.precondition, args);
        combine_eff(&mut cul_eff, &action.precondition, args);
    }

    let parameter_names = cul_args
        .iter()
        .enumerate()
        .map(|(i, _)| i.to_string())
        .collect();
    let parameter_types = cul_args
        .iter()
        .map(|a| World::global().objects.object_type(*a))
        .collect();
    let parameters = Parameters {
        names: parameter_names,
        types: parameter_types,
    };

    let precondition = cul_pre
        .into_iter()
        .map(|(fact, value)| Atom {
            predicate: fact.predicate(),
            parameters: fact
                .parameters()
                .iter()
                .map(|f_a| cul_args.iter().position(|a| f_a == a).unwrap())
                .collect(),
            value,
        })
        .collect();
    let effect = cul_eff
        .into_iter()
        .map(|(fact, value)| Atom {
            predicate: fact.predicate(),
            parameters: fact
                .parameters()
                .iter()
                .map(|f_a| cul_args.iter().position(|a| f_a == a).unwrap())
                .collect(),
            value,
        })
        .collect();
    Action {
        name,
        parameters,
        precondition,
        effect,
    }
}

fn combine_pre(
    cul_pre: &mut HashMap<Fact, bool>,
    cul_eff: &HashMap<Fact, bool>,
    pre: &Vec<Atom>,
    args: &Vec<usize>,
) {
    for atom in pre.iter() {
        let corresponding: Vec<usize> = atom.parameters.iter().map(|p| args[*p]).collect();
        let fact = Fact::new(atom.predicate, corresponding);
        if !cul_pre.contains_key(&fact) && !cul_eff.contains_key(&fact) {
            cul_pre.insert(fact, atom.value);
        }
    }
}

fn combine_eff(cul_eff: &mut HashMap<Fact, bool>, eff: &Vec<Atom>, args: &Vec<usize>) {
    for atom in eff.iter() {
        let corresponding: Vec<usize> = atom.parameters.iter().map(|p| args[*p]).collect();
        let fact = Fact::new(atom.predicate, corresponding);
        cul_eff.insert(fact, atom.value);
    }
}
