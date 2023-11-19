use super::{cache_data::CacheData, generate_plan, Cache};
use crate::{
    fact::Fact,
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
    fixed: HashMap<usize, usize>,
}

fn generate_replacements(
    cache_data: &CacheData,
    meta_index: &usize,
    parameters: &Vec<usize>,
) -> Option<Vec<Replacement>> {
    let relevant_replacements = cache_data
        .iter()
        .find(|(meta_action, ..)| *meta_index == **meta_action)?
        .1;
    let replacements = relevant_replacements
        .iter()
        .map(|(action, sas_plan)| {
            let action = Action::new(action.clone());
            let plan = sas_plan.to_owned();
            let fixed = action
                .parameters
                .names
                .iter()
                .enumerate()
                .filter_map(|(i, name)| match name.to_uppercase().contains('O') {
                    true => None,
                    false => {
                        let parameter_index = name.parse::<usize>().unwrap();
                        Some((i, parameters[parameter_index] as usize))
                    }
                })
                .collect();
            Replacement {
                action,
                plan,
                fixed,
            }
        })
        .collect();
    Some(replacements)
}

#[derive(Debug)]
pub struct LiftedCache {
    replacements: HashMap<(usize, Vec<usize>), Vec<Replacement>>,
}

impl LiftedCache {
    pub fn new(
        cache_data: CacheData,
        used_meta_actions: HashMap<usize, HashSet<Vec<usize>>>,
    ) -> Self {
        status_print(Status::Cache, "Init Lifted Cache");
        let mut replacements: HashMap<(usize, Vec<usize>), Vec<Replacement>> = HashMap::new();

        for (meta_action, permutations) in used_meta_actions.into_iter() {
            for permutation in permutations.into_iter() {
                let action_replacements =
                    generate_replacements(&cache_data, &meta_action, &permutation);

                if let Some(action_replacements) = action_replacements {
                    replacements.insert((meta_action, permutation), action_replacements);
                }
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
        let replacement_candidates = &self.replacements.get(&(meta_index, meta_parameters))?;
        for replacement in replacement_candidates.iter() {
            let action = &replacement.action;
            for permutation in get_applicable_with_fixed(&action, init, &replacement.fixed) {
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
}
