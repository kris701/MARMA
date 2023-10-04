use state::{instance::operator::Operator, state::State};

pub enum Constraint {
    PC,
    CT,
}

pub fn find_constraint(prior_state: &State, i: &Operator, j: &Operator) -> Vec<Constraint> {
    let mut constraints = vec![];
    if producer_consumer(prior_state, i, j) {
        constraints.push(Constraint::PC);
    }
    if consumer_threat(prior_state, i, j) {
        constraints.push(Constraint::CT);
    }
    constraints
}

fn producer_consumer(prior_state: &State, i: &Operator, j: &Operator) -> bool {
    let pos = (i.add.to_owned() & !(prior_state.to_owned())) & j.has.to_owned();
    pos.any()
}

fn consumer_threat(prior_state: &State, i: &Operator, j: &Operator) -> bool {
    let overlap = i.has.to_owned() & j.del.to_owned();
    overlap.any()
}
