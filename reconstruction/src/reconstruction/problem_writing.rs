use spingus::problem::Problem;

use crate::{state::state::State, Instance};
use std::{fs, path::PathBuf};

fn generate_objects(problem: &Problem) -> String {
    let mut s = "".to_string();
    problem.objects.iter().for_each(|o| {
        s.push_str(&format!(" {}", o.name));
        if o.type_name.is_some() {
            s.push_str(&format!(" - {}", o.type_name.as_ref().unwrap()));
        }
    });
    s
}

fn generate_fact(instance: &Instance, i: usize) -> String {
    let mut s = "".to_string();
    let predicate_index = instance.facts.fact_predicate(i);
    let parameters = instance.facts.fact_parameters(i);
    s.push_str(&instance.domain.predicates[predicate_index].name);
    parameters
        .iter()
        .for_each(|i| s.push_str(&format!(" {}", instance.problem.objects[*i].name)));
    s
}

fn generate_state(instance: &Instance, state: &State) -> String {
    let mut s = "".to_string();
    state
        .get()
        .iter()
        .enumerate()
        .for_each(|(i, _)| s.push_str(&format!("\t\t({})\n", generate_fact(instance, i))));
    s
}

fn generate_static(instance: &Instance) -> String {
    let mut s = "".to_string();
    instance
        .facts
        .get_static_true()
        .iter()
        .for_each(|i| s.push_str(&format!("\t\t({})\n", generate_fact(instance, *i))));
    s
}

fn generate_problem(instance: &Instance, init_state: &State, goal_state: &State) -> String {
    let mut s: String = "(define\n\t(problem temp)\n".to_string();
    s.push_str(&format!("\t(:domain {})\n", instance.domain.name));
    s.push_str(&format!(
        "\t(:objects{})\n",
        generate_objects(&instance.problem)
    ));
    s.push_str(&format!(
        "\t(:init\n{}\n{}\t)\n",
        generate_static(instance),
        generate_state(instance, init_state)
    ));
    s.push_str(&format!(
        "\t(:goal (and \n{}\t))\n",
        generate_state(instance, goal_state)
    ));
    s.push_str(")");
    s
}

pub fn write_problem(instance: &Instance, init_state: &State, goal_state: &State, path: &PathBuf) {
    let content = generate_problem(instance, init_state, goal_state);
    let _ = fs::write(path, content);
}
