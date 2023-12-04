use crate::{state::State, world::action::Action};
use clap::ValueEnum;
use once_cell::sync::OnceCell;
use std::{
    collections::HashMap,
    sync::atomic::{AtomicUsize, Ordering},
};

mod naive;

static LEGAL_OPERATORS: AtomicUsize = AtomicUsize::new(0);

fn increment_legal() {
    LEGAL_OPERATORS.fetch_add(1, Ordering::SeqCst);
}

pub fn legal_count() -> usize {
    LEGAL_OPERATORS.load(Ordering::SeqCst)
}

#[derive(Debug, Copy, Clone, PartialEq, Default, ValueEnum)]
pub enum InstantiationMethod {
    #[default]
    Naive,
    Grounded,
}
pub static INSTANTIATION_METHOD: OnceCell<InstantiationMethod> = OnceCell::new();

pub fn instantiate<'a>(
    action: &'a Action,
    state: &'a State,
    fixed: &'a HashMap<usize, usize>,
) -> Option<impl Iterator<Item = Vec<usize>> + 'a> {
    match INSTANTIATION_METHOD
        .get()
        .expect("instantiation method uninitialised")
    {
        InstantiationMethod::Naive => naive::get_applicable_with_fixed(action, state, fixed),
        InstantiationMethod::Grounded => todo!(),
    }
}
