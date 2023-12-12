use crate::plan::{Plan, Step};

pub fn stich(plan: &Plan, replacements: Vec<(usize, Plan)>) -> Plan {
    if replacements.is_empty() {
        return plan.clone();
    }

    let mut stiched_plan: Vec<Step> = plan.0.clone();

    replacements.into_iter().rev().for_each(|(i, plan)| {
        stiched_plan.remove(i);
        stiched_plan.splice(i..i, plan.into_iter());
    });

    Plan::new(stiched_plan)
}
