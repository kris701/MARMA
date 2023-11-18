use core::fmt;

use crate::world::World;

#[derive(Hash, PartialEq, Eq, Clone, Copy, Debug)]
pub struct Fact {
    internal: u64,
}

impl Fact {
    pub fn new(predicate: u16, parameters: Vec<u16>) -> Self {
        debug_assert!(parameters.len() <= 3);
        let internal = predicate as u64
            + parameters
                .iter()
                .enumerate()
                .map(|(i, p)| (*p as u64) << 16 * (i + 1))
                .sum::<u64>();
        Self { internal }
    }

    pub fn predicate(&self) -> u16 {
        self.internal as u16
    }

    pub fn parameters(&self) -> Vec<u16> {
        let mut parameters: Vec<u16> = Vec::new();
        let mut index = self.internal;
        index = index >> 16;
        while index != 0 {
            parameters.push(index as u16);
            index = index >> 16;
        }
        parameters
    }

    pub fn cmp(&self, b: &Fact) -> std::cmp::Ordering {
        self.internal.cmp(&b.internal)
    }
}

impl fmt::Display for Fact {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        let predicate = self.predicate();
        let predicate = World::global().predicates.name(predicate);
        let parameters = self.parameters();
        let parameters = World::global().objects.names(&parameters);
        let mut s = format!("{}", predicate);
        for param in parameters {
            s.push_str(&format!(" {}", param));
        }
        write!(f, "{}", s)
    }
}
