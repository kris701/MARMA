use super::{cache_data::CacheData, find_fixed, generate_plan, Cache};
use crate::{
    fact::Fact,
    instantiation::instantiate,
    macro_generation::generate_macro,
    plan::{convert_replacement_plan, Plan},
    state::State,
    tools::{status_print, Status},
    world::{action::Action, World},
};
use std::collections::HashMap;

#[derive(Debug)]
struct Replacement {
    action: Action,
    plan: Plan,
}

fn generate_replacements(cache_data: &CacheData, meta_index: &usize) -> Option<Vec<Replacement>> {
    let relevant_replacements = cache_data.get(meta_index)?;
    let replacements = relevant_replacements
        .iter()
        .map(|(action, sas_plan)| {
            let action = Action::new(action.clone());
            let plan = convert_replacement_plan(&action, sas_plan);
            Replacement { action, plan }
        })
        .collect();
    Some(replacements)
}

#[derive(Debug)]
pub struct ExactCache {
    replacements: HashMap<usize, Vec<Replacement>>,
}

impl ExactCache {
    pub fn new(cache_data: CacheData) -> Self {
        status_print(Status::Cache, "Init Exact Cache");
        let mut replacements: HashMap<usize, Vec<Replacement>> = HashMap::new();

        for a in World::global().actions.iterate_meta() {
            let index = World::global().actions.index(&a.name);
            let action_replacements = generate_replacements(&cache_data, &index);

            if let Some(action_replacements) = action_replacements {
                replacements.insert(index, action_replacements);
            }
        }

        Self { replacements }
    }
}
impl Cache for ExactCache {
    fn get_replacement(
        &self,
        step: &crate::plan::Step,
        init: &State,
        goal: &State,
    ) -> Option<Plan> {
        let desired = init.diff(goal);
        let replacement_candidates = &self.replacements.get(&step.action)?;
        for replacement in replacement_candidates.iter() {
            let action = &replacement.action;
            let fixed = find_fixed(&step.args, action);
            for permutation in instantiate(&action, init, &fixed)? {
                let mut eff: Vec<(Fact, bool)> = Vec::new();
                for atom in action.effect.iter() {
                    let corresponding: Vec<usize> = atom.map_args(&permutation);
                    if atom.value != init.has_nary(atom.predicate, &corresponding) {
                        let fact = Fact::new(atom.predicate, corresponding);
                        eff.push((fact, atom.value))
                    }
                }
                eff.sort();
                if eff != desired {
                    continue;
                }
                let plan =
                    generate_plan(&init, &replacement.action, &replacement.plan, &permutation);
                if plan.is_some() {
                    return plan;
                }
            }
        }
        None
    }

    fn add_entry(&mut self, step: &crate::plan::Step, replacement_plan: &Plan) {
        let meta_action = World::global().actions.get(step.action);
        let (action, plan) = generate_macro(meta_action, replacement_plan);
        let replacement = Replacement { action, plan };
        self.replacements
            .entry(step.action)
            .or_default()
            .push(replacement);
    }
}
