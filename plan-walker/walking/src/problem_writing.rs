use parsing::domain::Domain;
use parsing::problem::Problem;

use crate::instance::fact::Facts;
use crate::state::State;
use crate::write_file;
use std::ffi::OsString;

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

fn generate_fact(domain: &Domain, problem: &Problem, facts: &Facts, i: usize) -> String {
    let mut s = "".to_string();
    let fact = &facts.facts[i];
    s.push_str(&domain.predicates[fact.predicate].name);
    fact.parameters
        .iter()
        .for_each(|i| s.push_str(&format!(" {}", problem.objects[*i].name)));
    s
}

fn generate_state(domain: &Domain, problem: &Problem, facts: &Facts, state: &State) -> String {
    let mut s = "".to_string();
    state
        .values
        .iter()
        .enumerate()
        .filter(|(_, v)| **v)
        .for_each(|(i, _)| {
            s.push_str(&format!(
                "\t\t({})\n",
                generate_fact(domain, problem, facts, i)
            ))
        });
    s
}

fn generate_problem(
    domain: &Domain,
    problem: &Problem,
    facts: &Facts,
    init_state: &State,
    goal_state: &State,
) -> String {
    let mut s: String = "(define\n\t(problem temp)\n".to_string();
    s.push_str(&format!("\t(:domain {})\n", domain.name));
    s.push_str(&format!("\t(:objects{})\n", generate_objects(problem)));
    s.push_str(&format!(
        "\t(:init\n{}\t)\n",
        generate_state(domain, problem, facts, init_state)
    ));
    s.push_str(&format!(
        "\t(:goal (and \n{}\t))\n",
        generate_state(domain, problem, facts, goal_state)
    ));
    s.push_str(")");
    s
}

pub fn write_problem(
    domain: &Domain,
    problem: &Problem,
    facts: &Facts,
    init_state: &State,
    goal_state: &State,
    path: &OsString,
) {
    let content = generate_problem(domain, problem, facts, init_state, goal_state);
    write_file(path, content);
}
