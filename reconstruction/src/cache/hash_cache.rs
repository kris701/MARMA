use super::{cache_data::CacheData, generate_plan, Cache};
use crate::{
    fact::Fact,
    operator::{extract_from_action, generate_operators_by_candidates},
    state::State,
    tools::{status_print, Status},
    world::{action::Action, World},
};
use spingus::{sas_plan::SASPlan, term::Term};
use std::collections::{HashMap, HashSet};

#[derive(Debug)]
pub struct HashCache {
    /// All macros that can potentially be used
    lifted_macros: Vec<(Action, SASPlan)>,
    /// Grounded macros
    entries: Vec<(u16, Vec<u16>)>,
    /// A map from effect to those entries that fulfill it
    effect_map: HashMap<Vec<(Fact, bool)>, Vec<usize>>,
}

impl HashCache {
    pub fn new(cache_data: CacheData, used_meta_actions: HashMap<u16, HashSet<Vec<u16>>>) -> Self {
        status_print(Status::Cache, "Init Hash Cache");
        let mut lifted_macros: Vec<(Action, SASPlan)> = Vec::new();
        let mut entries: Vec<(u16, Vec<u16>)> = Vec::new();
        let mut effect_map: HashMap<Vec<(Fact, bool)>, Vec<usize>> = HashMap::new();

        for (meta_index, macros) in cache_data.into_iter() {
            for (macro_action, plan) in macros.into_iter() {
                let macro_index = lifted_macros.len();
                let macro_action = Action::new(macro_action);
                if !used_meta_actions.contains_key(&meta_index) {
                    continue;
                }
                for meta_permutation in used_meta_actions[&meta_index].iter() {
                    let candidates: Vec<Vec<u16>> = macro_action
                        .parameters
                        .names
                        .iter()
                        .zip(macro_action.parameters.types.iter())
                        .map(|(name, type_id)| match name.to_uppercase().contains('O') {
                            true => World::global().objects.iterate_with_type(type_id).collect(),
                            false => {
                                let parameter_index = name.parse::<usize>().unwrap();
                                vec![meta_permutation[parameter_index]]
                            }
                        })
                        .collect();
                    for (operator, permutation) in
                        generate_operators_by_candidates(&macro_action, candidates)
                    {
                        let entry_index = entries.len();
                        entries.push((macro_index as u16, permutation));
                        let effect = operator.get_effect();
                        effect_map.entry(effect).or_default().push(entry_index);
                    }
                }
                lifted_macros.push((macro_action, plan));
            }
        }

        println!("Cache entries: {}", entries.len());

        Self {
            lifted_macros,
            entries,
            effect_map,
        }
    }
}
impl Cache for HashCache {
    fn get_replacement(&self, _meta_term: &Term, init: &State, goal: &State) -> Option<SASPlan> {
        let desired = init.diff(goal);
        let candidates: &Vec<usize> = self.effect_map.get(&desired)?;
        for candidate in candidates.iter() {
            let (macro_index, parameters) = &self.entries[*candidate];
            let (macro_action, plan) = &self.lifted_macros[*macro_index as usize];
            let operator = extract_from_action(&parameters, &macro_action).unwrap();
            if init.is_legal(&operator) {
                return Some(generate_plan(&macro_action, plan, parameters));
            }
        }
        None
    }
}
