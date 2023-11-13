use std::{fs, path::PathBuf};

use crate::{state::State, world::World};

fn generate_objects() -> String {
    let mut s = "".to_string();
    for (object_name, type_name) in World::global().iterate_objects_named() {
        s.push_str(&format!(" {} - {}", object_name, type_name));
    }
    s
}

pub fn generate_state(state: &State) -> String {
    let mut s = "".to_string();
    state
        .get()
        .iter()
        .for_each(|i| s.push_str(&format!("\t\t({})\n", i.to_string())));
    s
}

fn generate_problem(init_state: &State, goal_state: &State) -> String {
    let mut s: String = "(define\n\t(problem temp)\n".to_string();
    s.push_str(&format!("\t(:domain {})\n", World::global().domain_name()));
    s.push_str(&format!("\t(:objects{})\n", generate_objects()));
    s.push_str(&format!("\t(:init\n{}\t)\n", generate_state(init_state)));
    //TODO: Add statics init
    s.push_str(&format!(
        "\t(:goal (and \n{}\t))\n",
        generate_state(goal_state)
    ));
    s.push_str(")");
    s
}

pub fn write_problem(init_state: &State, goal_state: &State, path: &PathBuf) {
    let content = generate_problem(init_state, goal_state);
    let _ = fs::write(path, content);
}
