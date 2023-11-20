use super::{cache_data::CacheData, find_fixed, generate_plan, Cache};
use crate::{
    fact::Fact, state::State, successor_genrator::r#static::generate_statically_with_fixed,
    world::action::Action,
};
use itertools::Itertools;
use spingus::{sas_plan::SASPlan, term::Term};
use std::collections::{HashMap, HashSet};

#[derive(Debug)]
struct Entry {
    meta_index: usize,
    permutation: Vec<usize>,
    macro_index: usize,
}

#[derive(Debug)]
pub struct HashCache {
    macros: Vec<(Action, SASPlan)>,
    entries: Vec<Entry>,
    effect_map: HashMap<Vec<(Fact, bool)>, Vec<usize>>,
}

impl HashCache {
    pub fn new(
        cache_data: CacheData,
        used_meta_actions: HashMap<usize, HashSet<Vec<usize>>>,
    ) -> Self {
        let mut macros: Vec<(Action, SASPlan)> = Vec::new();
        let mut entries: Vec<Entry> = Vec::new();
        let mut effect_map: HashMap<Vec<(Fact, bool)>, Vec<usize>> = HashMap::new();

        for (meta_index, permutations) in used_meta_actions.into_iter() {
            if !cache_data.contains_key(&meta_index) {
                continue;
            }
            for (macro_action, plan) in cache_data[&meta_index].iter() {
                let macro_action = Action::new(macro_action.clone());
                let macro_index = macros.len();
                for meta_permutation in permutations.iter() {
                    let fixed = find_fixed(meta_permutation, &macro_action);
                    for permutation in generate_statically_with_fixed(&macro_action, &fixed) {
                        let entry_index = entries.len();
                        let effect = macro_action
                            .effect
                            .iter()
                            .map(|a| {
                                let predicate = a.predicate;
                                let corresponding: Vec<usize> =
                                    a.parameters.iter().map(|p| permutation[*p]).collect();
                                (Fact::new(predicate, corresponding), a.value)
                            })
                            .sorted_by(|a, b| a.0.cmp(&b.0))
                            .collect();
                        effect_map.entry(effect).or_default().push(entry_index);
                        entries.push(Entry {
                            meta_index,
                            permutation,
                            macro_index,
                        });
                    }
                }
                macros.push((macro_action, plan.to_owned()));
            }
        }

        println!("cache has {} entries", entries.len());

        Self {
            macros,
            entries,
            effect_map,
        }
    }
}

impl Cache for HashCache {
    fn get_replacement(
        &self,
        _meta_term: &Term,
        init: &State,
        goal: &State,
    ) -> Option<spingus::sas_plan::SASPlan> {
        let desired = init.diff(goal);
        let candidates: &Vec<usize> = self.effect_map.get(&desired)?;
        for candidate in candidates.iter() {
            let entry = &self.entries[*candidate];
            let (macro_action, plan) = &self.macros[entry.macro_index];
            if init.is_legal(macro_action, &entry.permutation) {
                return Some(generate_plan(macro_action, plan, &entry.permutation));
            }
        }
        None
    }
}
