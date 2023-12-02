use std::collections::HashMap;

use clap::ValueEnum;
use itertools::Itertools;
use once_cell::sync::OnceCell;

use crate::world::action::Action;

use spingus::sas_plan::SASPlan;

mod grounded;
mod lifted;

#[derive(Debug, Copy, Clone, PartialEq, Default, ValueEnum)]
pub enum MacroMethod {
    #[default]
    Lifted,
    Grounded,
}

pub static MACRO_METHOD: OnceCell<MacroMethod> = OnceCell::new();

pub fn generate_macro(
    meta_action: &Action,
    operators: Vec<(&Action, Vec<usize>)>,
) -> (Action, SASPlan) {
    let macro_method = MACRO_METHOD.get().expect("macro method uninitialised");
    match macro_method {
        MacroMethod::Lifted => todo!(),
        MacroMethod::Grounded => grounded::generate_macro(meta_action, operators),
    }
}
