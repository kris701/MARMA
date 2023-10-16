use std::collections::HashMap;

pub struct IndexMap {
    map: HashMap<String, usize>,
    indexes: Vec<String>,
}

impl IndexMap {
    pub fn new() -> Self {
        Self {
            map: HashMap::new(),
            indexes: vec![],
        }
    }

    pub fn count(&self) -> usize {
        self.indexes.len()
    }

    pub fn insert(&mut self, s: String) {
        self.map.insert(s.to_owned(), self.indexes.len());
        self.indexes.push(s);
    }

    pub fn index(&self, s: &str) -> usize {
        self.map[s]
    }

    pub fn index_checked(&self, s: &str) -> Option<usize> {
        self.map.get(s).copied()
    }

    pub fn at(&self, i: usize) -> &str {
        &self.indexes[i]
    }

    pub fn at_checked(&self, i: usize) -> Option<&str> {
        if self.indexes.len() < i {
            return None;
        }
        Some(self.at(i))
    }
}
