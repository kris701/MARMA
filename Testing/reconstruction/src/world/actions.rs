use super::{
    action::{translate_action, Action},
    objects::Objects,
    predicates::Predicates,
    types::Types,
};

pub struct Actions {
    primitive: Vec<Action>,
    meta: Vec<Action>,
}

impl Actions {
    pub fn primitive_count(&self) -> usize {
        self.primitive.len()
    }

    pub fn meta_count(&self) -> usize {
        self.meta.len()
    }

    pub fn index(&self, name: &str) -> usize {
        self.primitive
            .iter()
            .chain(self.meta.iter())
            .position(|a| a.name == name)
            .expect(&format!("Undeclared action: {}", name))
    }

    pub fn is_meta(&self, index: usize) -> bool {
        index >= self.primitive.len()
    }

    pub fn get(&self, index: usize) -> &Action {
        match self.is_meta(index) {
            true => &self.meta[index - self.primitive_count()],
            false => &self.primitive[index],
        }
    }

    pub fn name(&self, index: usize) -> &str {
        let action = self.get(index);
        &action.name
    }

    pub fn iterate_primitive(&self) -> impl Iterator<Item = &Action> {
        self.primitive.iter()
    }

    pub fn iterate_meta(&self) -> impl Iterator<Item = &Action> {
        self.meta.iter()
    }
}

pub(crate) fn translate_actions(
    types: &Types,
    predicates: &Predicates,
    objects: &Objects,
    primitive: spingus::domain::action::Actions,
    meta: spingus::domain::action::Actions,
) -> Actions {
    let translate = |actions: spingus::domain::action::Actions| -> Vec<Action> {
        actions
            .into_iter()
            .map(|a| translate_action(types, predicates, objects, a))
            .collect()
    };
    let primitive = translate(primitive);
    let meta = translate(meta);
    Actions { primitive, meta }
}
