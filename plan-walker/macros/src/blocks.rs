use state::{
    instance::operator::Operator,
    plan::{Plan, Step},
    state::State,
};

enum Relations {
    PC {
        predicate: usize,
        parameters: Vec<usize>,
    },
}

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
    let mut prior_sate = init_state.to_owned();
    for j in 1..plan.steps.len() {
        let step_j = plan.steps[j].to_owned();
        if producer_consumer(
            &prior_sate,
            &block.last().unwrap().operator,
            &step_j.operator,
        ) {
            block.push(step_j);
        } else {
            blocks.push(Block::new(block.clone()));
            prior_sate.apply_operators(&block.iter().map(|s| s.operator.clone()).collect());
            block = vec![step_j];
        }
    }
    blocks.push(Block::new(block));
    blocks
}

fn producer_consumer(prior_state: &State, i: &Operator, j: &Operator) -> bool {
    let pos = (i.add.to_owned() & !(prior_state.values.to_owned())) & j.has.to_owned();
    pos.any()
}
