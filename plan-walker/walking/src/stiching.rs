use parsing::sas::SASPlan;

pub fn stich_single(plan: &SASPlan, meta_plan: &SASPlan) -> SASPlan {
    let meta_pos = plan.meta_pos().unwrap();
    let mut steps = vec![];
    steps.append(&mut plan.steps[0..meta_pos].to_vec());
    steps.append(&mut meta_plan.steps.clone());
    steps.append(&mut plan.steps[meta_pos + 1..plan.steps.len()].to_vec());
    SASPlan { steps }
}

#[cfg(test)]
mod test {
    use parsing::{
        sas::{SASPlan, StepType},
        term::Term,
    };

    use crate::stich_single;
    #[test]
    fn stitch_single() {
        assert_eq!(
            SASPlan {
                steps: vec![
                    (
                        Term {
                            name: "1".to_string(),
                            parameters: vec![]
                        },
                        StepType::Normal
                    ),
                    (
                        Term {
                            name: "2".to_string(),
                            parameters: vec![]
                        },
                        StepType::Normal
                    ),
                    (
                        Term {
                            name: "3".to_string(),
                            parameters: vec![]
                        },
                        StepType::Normal
                    ),
                    (
                        Term {
                            name: "4".to_string(),
                            parameters: vec![]
                        },
                        StepType::Normal
                    ),
                    (
                        Term {
                            name: "5".to_string(),
                            parameters: vec![]
                        },
                        StepType::Normal
                    ),
                ]
            },
            stich_single(
                &SASPlan {
                    steps: vec![
                        (
                            Term {
                                name: "1".to_string(),
                                parameters: vec![]
                            },
                            StepType::Normal
                        ),
                        (
                            Term {
                                name: "$".to_string(),
                                parameters: vec![]
                            },
                            StepType::Meta
                        ),
                        (
                            Term {
                                name: "5".to_string(),
                                parameters: vec![]
                            },
                            StepType::Normal
                        ),
                    ]
                },
                &SASPlan {
                    steps: vec![
                        (
                            Term {
                                name: "2".to_string(),
                                parameters: vec![]
                            },
                            StepType::Normal
                        ),
                        (
                            Term {
                                name: "3".to_string(),
                                parameters: vec![]
                            },
                            StepType::Normal
                        ),
                        (
                            Term {
                                name: "4".to_string(),
                                parameters: vec![]
                            },
                            StepType::Normal
                        ),
                    ]
                }
            )
        )
    }
}
