use std::collections::HashMap;

pub struct Types {
    index_map: HashMap<String, usize>,
    parent: Vec<Option<usize>>,
    _children: Vec<Vec<usize>>,
}

impl Types {
    pub fn new(types: spingus::domain::types::Types) -> Self {
        let mut index_map: HashMap<String, usize> = HashMap::new();
        let mut parent: Vec<Option<usize>> = Vec::new();
        let mut children: Vec<Vec<usize>> = Vec::new();

        fn get_type_index(
            index_map: &mut HashMap<String, usize>,
            parent: &mut Vec<Option<usize>>,
            children: &mut Vec<Vec<usize>>,
            name: String,
        ) -> (bool, usize) {
            match index_map.contains_key(&name) {
                true => (false, index_map[&name]),
                false => {
                    let index = index_map.len();
                    index_map.insert(name, index);
                    parent.push(None);
                    children.push(vec![]);
                    (true, index)
                }
            }
        }

        for t in types {
            let (_, type_index) =
                get_type_index(&mut index_map, &mut parent, &mut children, t.name);
            let mut type_children: Vec<usize> = Vec::new();
            for child in t.sub_types {
                let (_, child_index) =
                    get_type_index(&mut index_map, &mut parent, &mut children, child);
                parent[child_index] = Some(type_index);
                type_children.push(child_index);
            }
            children[type_index] = type_children;
        }

        Self {
            index_map,
            parent,
            _children: children,
        }
    }

    pub fn get_index(&self, name: &str) -> &usize {
        &self.index_map.get(name).unwrap()
    }

    pub fn get_parent(&self, type_index: usize) -> &Option<usize> {
        &self.parent[type_index]
    }

    pub fn is_of_type(&self, type_index: usize, wished: usize) -> bool {
        if type_index == wished {
            return true;
        }
        match self.get_parent(type_index) {
            Some(parent) => return self.is_of_type(*parent, wished),
            None => return false,
        }
    }
}
