use super::{cache_data::CacheData, find_fixed, generate_plan, Cache};
use crate::{
    fact::Fact,
    instantiation::instantiate,
    macro_generation::generate_macro,
    state::State,
    tools::{status_print, Status},
    world::{action::Action, World},
};
use spingus::{sas_plan::SASPlan, term::Term};
use std::collections::HashMap;

#[derive(Debug)]
struct Replacement {
    action: Action,
    plans: Vec<SASPlan>,
}

fn generate_replacements(cache_data: &CacheData, meta_index: &usize) -> Option<Vec<Replacement>> {
    let relevant_replacements = cache_data.get(meta_index)?;
    let replacements = relevant_replacements
        .iter()
        .map(|(action, sas_plans)| {
            let action = Action::new(action.clone());
            let plans = sas_plans.to_owned();
            Replacement { action, plans }
        })
        .collect();
    Some(replacements)
}

#[derive(Debug)]
pub struct LiftedCache {
    replacements: HashMap<usize, Vec<Replacement>>,
}

impl LiftedCache {
    pub fn new(cache_data: CacheData) -> Self {
        status_print(Status::Cache, "Init Lifted Cache");
        let mut replacements: HashMap<usize, Vec<Replacement>> = HashMap::new();

        for (i, _) in World::global().meta_actions.iter().enumerate() {
            let action_replacements = generate_replacements(&cache_data, &i);

            if let Some(action_replacements) = action_replacements {
                replacements.insert(i, action_replacements);
            }
        }

        Self { replacements }
    }
}
impl Cache for LiftedCache {
    fn get_replacement(&self, meta_term: &Term, init: &State, goal: &State) -> Option<SASPlan> {
        let desired = init.diff(goal);
        let meta_index = World::global().meta_index(&meta_term.name);
        let meta_parameters = World::global().objects.indexes(&meta_term.parameters);
        let replacement_candidates = &self.replacements.get(&meta_index)?;
        for replacement in replacement_candidates.iter() {
            let action = &replacement.action;
            let fixed = find_fixed(&meta_parameters, action);
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
                for plan in replacement.plans.iter() {
                    if let Some(plan) =
                        generate_plan(&init, &replacement.action, plan, &permutation)
                    {
                        return Some(plan);
                    }
                }
            }
        }
        None
    }

    fn add_entry(&mut self, meta_term: &Term, replacement_plan: &SASPlan) {
        let meta_index = World::global().meta_index(&meta_term.name);
        let meta_action = World::global().get_action(&meta_term.name);
        let operators = replacement_plan
            .iter()
            .map(|s| {
                (
                    World::global().get_action(&s.name),
                    World::global().objects.indexes(&s.parameters),
                )
            })
            .collect();
        let (action, plan) = generate_macro(meta_action, operators);
        let replacement = Replacement {
            action,
            plans: vec![plan],
        };
        self.replacements
            .entry(meta_index)
            .or_default()
            .push(replacement);
    }
}
