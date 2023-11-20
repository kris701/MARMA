use super::{cache_data::CacheData, find_fixed, generate_plan, Cache};
use crate::{
    fact::Fact,
    macro_generation::generate_macro,
    state::State,
    successor_genrator::get_applicable_with_fixed,
    tools::{status_print, Status},
    world::{action::Action, World},
};
use spingus::{sas_plan::SASPlan, term::Term};
use std::collections::{HashMap, HashSet};

#[derive(Debug)]
struct Replacement {
    action: Action,
    plan: SASPlan,
}

fn generate_replacements(cache_data: &CacheData, meta_index: &usize) -> Option<Vec<Replacement>> {
    let relevant_replacements = cache_data.get(meta_index)?;
    let replacements = relevant_replacements
        .iter()
        .map(|(action, sas_plan)| {
            let action = Action::new(action.clone());
            let plan = sas_plan.to_owned();
            Replacement { action, plan }
        })
        .collect();
    Some(replacements)
}

#[derive(Debug)]
pub struct LiftedCache {
    replacements: HashMap<usize, Vec<Replacement>>,
}

impl LiftedCache {
    pub fn new(
        cache_data: CacheData,
        used_meta_actions: HashMap<usize, HashSet<Vec<usize>>>,
    ) -> Self {
        status_print(Status::Cache, "Init Lifted Cache");
        let mut replacements: HashMap<usize, Vec<Replacement>> = HashMap::new();

        for (meta_action, _) in used_meta_actions.into_iter() {
            let action_replacements = generate_replacements(&cache_data, &meta_action);

            if let Some(action_replacements) = action_replacements {
                replacements.insert(meta_action, action_replacements);
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
            for permutation in get_applicable_with_fixed(&action, init, &fixed) {
                let mut eff_neg: HashSet<Fact> = HashSet::new();
                let mut eff_pos: HashSet<Fact> = HashSet::new();
                for atom in action.effect.iter() {
                    let corresponding: Vec<usize> =
                        atom.parameters.iter().map(|p| permutation[*p]).collect();
                    let fact = Fact::new(atom.predicate, corresponding);
                    match atom.value {
                        true => eff_pos.insert(fact),
                        false => eff_neg.insert(fact),
                    };
                }
                if desired.iter().any(|(i, v)| match v {
                    true => !eff_pos.contains(&i),
                    false => !eff_neg.contains(&i),
                }) {
                    continue;
                }
                return Some(generate_plan(
                    &replacement.action,
                    &replacement.plan,
                    &permutation,
                ));
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
        let replacement = Replacement { action, plan };
        self.replacements
            .entry(meta_index)
            .or_default()
            .push(replacement);
    }
}
