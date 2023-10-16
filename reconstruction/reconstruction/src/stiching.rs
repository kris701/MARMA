use spingus::{sas_plan::SASPlan, term::Term};

pub fn stich(plan: SASPlan, replacements: Vec<(&usize, &SASPlan)>) -> SASPlan {
    if replacements.is_empty() {
        return plan;
    }
    let mut steps: Vec<Term> = vec![];

    let mut next_replacement: Option<usize> = Some(0);
    for i in 0..plan.len() {
        if next_replacement.is_some() && i == *replacements[next_replacement.unwrap()].0 {
            for step in replacements[next_replacement.unwrap()].1 {
                steps.push(step.clone());
            }
            if next_replacement.unwrap() == replacements.len() {
                next_replacement = None;
            } else {
                next_replacement = Some(next_replacement.unwrap() + 1);
            }
        } else {
            steps.push(plan[i].clone());
        }
    }

    steps
}
