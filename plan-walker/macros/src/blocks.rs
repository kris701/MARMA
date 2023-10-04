use state::{
    instance::operator::Operator,
    plan::{Plan, Step},
    state::{apply_to_state, State},
};

use crate::constraints::find_constraint;

#[derive(Clone, Hash, PartialEq, Eq)]
pub struct Block {
    pub steps: Vec<Step>,
    pub combined_operator: Operator,
}

impl Block {
    pub fn new(steps: Vec<Step>) -> Self {
        let mut combined_operator = steps[0].operator.to_owned();

        for s in steps[1..steps.len()].iter() {
            let o = &s.operator;
            let has = combined_operator.has.to_owned() | (o.has.to_owned() & !o.add.to_owned());
            let not = combined_operator.not.to_owned() | (o.not.to_owned() & !o.del.to_owned());
            let add = (combined_operator.add.to_owned() & !o.del.to_owned()) | o.add.to_owned();
            let del = (combined_operator.del.to_owned() & !o.add.to_owned()) | o.del.to_owned();
            combined_operator = Operator { has, not, add, del }
        }

        Self {
            steps,
            combined_operator,
        }
    }
}

pub type Blocks = Vec<Block>;

pub fn block_decomposition(init_state: &State, plan: Plan) -> Blocks {
    if plan.steps.is_empty() {
        return vec![];
    }

    let mut blocks: Blocks = vec![];
    let mut block: Vec<Step> = vec![plan.steps[0].to_owned()];
    let mut prior_state = init_state.to_owned();
    for j in 1..plan.steps.len() {
        let step_i = block.last().unwrap().to_owned();
        let step_j = plan.steps[j].to_owned();
        if !find_constraint(&prior_state, &step_i.operator, &step_j.operator).is_empty() {
            block.push(step_j);
        } else {
            blocks.push(Block::new(block.clone()));
            for operator in block.iter().map(|s| &s.operator) {
                apply_to_state(&mut prior_state, &operator);
            }
            block = vec![step_j];
        }
    }
    blocks.push(Block::new(block));
    blocks
}
