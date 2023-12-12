use std::collections::HashMap;

use itertools::Itertools;

use crate::{
    fact::Fact,
    plan::{Plan, Step},
    tools::random_name,
    world::{
        action::Action,
        atom::{Argument, Atom},
        parameter::Parameters,
        World,
    },
};

pub(super) fn generate_macro(meta_action: &Action, plan: &Plan) -> (Action, Plan) {
    let mut cul_args: Vec<usize> = Vec::new();
    let mut cul_pre: HashMap<Fact, bool> = HashMap::new();
    let mut cul_eff: HashMap<Fact, bool> = HashMap::new();

    for step in plan.iter() {
        let action = World::global().actions.get(step.action);
        let new_args = step
            .args
            .iter()
            .filter(|a| !cul_args.contains(a))
            .collect_vec();
        cul_args.extend(new_args);
        combine_pre(&mut cul_pre, &cul_eff, &action.precondition, &step.args);
        combine_eff(&mut cul_eff, &action.effect, &step.args);
    }

    let effect: Vec<Atom> = cul_eff
        .into_iter()
        .filter(|(fact, value)| !(cul_pre.contains_key(fact) && cul_pre[fact] == *value))
        .sorted()
        .map(|(fact, value)| Atom {
            predicate: fact.predicate(),
            parameters: fact
                .parameters()
                .iter()
                .map(|f_a| match World::global().objects.is_constant(*f_a) {
                    true => Argument::Constant(*f_a),
                    false => Argument::Parameter(cul_args.iter().position(|a| a == f_a).unwrap()),
                })
                .collect(),
            value,
        })
        .collect();
    let precondition: Vec<Atom> = cul_pre
        .into_iter()
        .sorted()
        .map(|(fact, value)| Atom {
            predicate: fact.predicate(),
            parameters: fact
                .parameters()
                .iter()
                .map(|f_a| match World::global().objects.is_constant(*f_a) {
                    true => Argument::Constant(*f_a),
                    false => Argument::Parameter(cul_args.iter().position(|a| a == f_a).unwrap()),
                })
                .collect(),
            value,
        })
        .collect();
    // macro parameter maps to meta parameter
    let mut fixed_parameters: HashMap<usize, usize> = HashMap::new();
    for atom in meta_action.effect.iter() {
        for corresponding_atom in effect.iter().filter(|a| a.predicate == atom.predicate) {
            for (i, parameter) in corresponding_atom.parameters.iter().enumerate() {
                match parameter {
                    Argument::Parameter(p) => {
                        let _ = fixed_parameters.insert(
                            *p,
                            match atom.parameters[i] {
                                Argument::Parameter(p) => p,
                                Argument::Constant(o) => o,
                            },
                        );
                    }
                    Argument::Constant(_) => {}
                };
            }
        }
    }

    let parameter_names: Vec<String> = cul_args
        .iter()
        .enumerate()
        .map(|(i, _)| match fixed_parameters.contains_key(&i) {
            true => format!("{}", fixed_parameters[&i]),
            false => format!("O{}", i),
        })
        .collect();

    let replacement_plan = Plan::new(
        plan.iter()
            .map(|s| Step {
                action: s.action,
                args: s
                    .args
                    .iter()
                    .map(|a| cul_args.iter().position(|c_arg| a == c_arg).unwrap())
                    .collect(),
            })
            .collect(),
    );
    let parameter_types = cul_args
        .iter()
        .map(|a| World::global().objects.object_type(*a))
        .collect();
    let parameters = Parameters {
        names: parameter_names,
        types: parameter_types,
    };
    (
        Action {
            name: random_name(),
            parameters,
            precondition,
            effect,
        },
        replacement_plan,
    )
}

fn combine_pre(
    cul_pre: &mut HashMap<Fact, bool>,
    cul_eff: &HashMap<Fact, bool>,
    pre: &Vec<Atom>,
    args: &Vec<usize>,
) {
    for atom in pre.iter() {
        let corresponding: Vec<usize> = atom.map_args(args);
        let fact = Fact::new(atom.predicate, corresponding);
        if !cul_pre.contains_key(&fact) && !cul_eff.contains_key(&fact) {
            cul_pre.insert(fact, atom.value);
        }
    }
}

fn combine_eff(cul_eff: &mut HashMap<Fact, bool>, eff: &Vec<Atom>, args: &Vec<usize>) {
    for atom in eff.iter() {
        let corresponding: Vec<usize> = atom.map_args(args);
        let fact = Fact::new(atom.predicate, corresponding);
        cul_eff.insert(fact, atom.value);
    }
}
