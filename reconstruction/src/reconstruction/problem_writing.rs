use spingus::problem::Problem;

use std::{fs, path::PathBuf};

use crate::{instance::Instance, state::State};

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

pub fn generate_state(instance: &Instance, state: &State) -> String {
    let mut s = "".to_string();
    state
        .get()
        .iter()
        .for_each(|i| s.push_str(&format!("\t\t({})\n", instance.get_fact_string(*i))));
    s
}

pub fn generate_static(instance: &Instance) -> String {
    let mut s = "".to_string();
    instance
        .facts
        .get_static_true()
        .iter()
        .for_each(|i| s.push_str(&format!("\t\t({})\n", instance.get_fact_string(*i))));
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
