use parsing::sas::SASPlan;

pub fn stich_single(plan: &SASPlan, meta_plan: &SASPlan) -> SASPlan {
    let meta_pos = plan.meta_pos().unwrap();
    let mut steps = vec![];
    steps.append(&mut plan.steps[0..meta_pos].to_vec());
    steps.append(&mut meta_plan.steps.clone());
    steps.append(&mut plan.steps[meta_pos + 1..plan.steps.len()].to_vec());
    SASPlan { steps }
}
