use super::{cache_data::CacheData, Cache};
use crate::{fact::Fact, state::State, world::action::Action, successor_genrator::r#static::generate_statically_with_fixed};
use spingus::{sas_plan::SASPlan, term::Term};
use std::collections::{HashMap, HashSet};

#[derive(Debug, Hash, PartialEq, Eq)]
struct Effect {
    pos: Vec<Fact>,
    neg: Vec<Fact>,
}

#[derive(Debug)]
struct Entry {
    meta_action: usize,
    permutation: Vec<usize>,
    macro_index: usize,
}

#[derive(Debug)]
pub struct HashCache {
    macros: Vec<(Action, SASPlan)>,
    entries: Vec<Entry>,
    effect_map: HashMap<Effect, Vec<usize>>,
}

impl HashCache {
    pub fn new(
        cache_data: CacheData,
        used_meta_actions: HashMap<usize, HashSet<Vec<usize>>>,
    ) -> Self {
        let mut macros: Vec<(Action, SASPlan)> = Vec::new();
        let mut entries: Vec<Entry> = Vec::new();
        let mut effect_map: HashMap<Effect, Vec<usize>> = HashMap::new();

        for (meta_index, permutations) in used_meta_actions.into_iter() {
            for macro_entry in cache_data.get(&meta_index).or(Some(&vec![])).unwrap() {
                let macro_index = macros.len();
                for permutation in generate_statically_with_fixed(macro_entry.0, fixed)
            }
        }

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
        meta_term: &Term,
        init: &State,
        goal: &State,
    ) -> Option<spingus::sas_plan::SASPlan> {
        todo!()
    }
}
