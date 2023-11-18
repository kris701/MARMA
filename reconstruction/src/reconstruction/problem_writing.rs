use std::{fs, path::PathBuf};

use crate::{state::State, world::World};

fn generate_objects() -> String {
    let mut s = "".to_string();
    for (object_name, type_name) in World::global().objects.iterate_named() {
        s.push_str(&format!(" {} - {}", object_name, type_name));
    }
    s
}

fn generate_problem(init_state: &State, goal_state: &State) -> String {
    let mut s: String = "(define\n\t(problem temp)\n".to_string();
    s.push_str(&format!("\t(:domain {})\n", World::global().domain_name()));
    s.push_str(&format!("\t(:objects{})\n", generate_objects()));
    s.push_str(&format!("\t(:init{}\n\t)\n", init_state.export_all()));
    s.push_str(&format!(
        "\t(:goal (and{}\n\t))\n",
        goal_state.export_mutable()
    ));
    s.push_str(")");
    s
}

pub fn write_problem(init_state: &State, goal_state: &State, path: &PathBuf) {
    let content = generate_problem(init_state, goal_state);
    let _ = fs::write(path, content);
}
