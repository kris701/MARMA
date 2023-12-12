use crate::{plan::Plan, world::action::Action};
use clap::ValueEnum;
use once_cell::sync::OnceCell;

mod grounded;
mod lifted;

#[derive(Debug, Copy, Clone, PartialEq, Default, ValueEnum)]
pub enum MacroMethod {
    #[default]
    Lifted,
    Grounded,
}

pub static MACRO_METHOD: OnceCell<MacroMethod> = OnceCell::new();

pub fn generate_macro(meta_action: &Action, plan: &Plan) -> (Action, Plan) {
    let macro_method = MACRO_METHOD.get().expect("macro method uninitialised");
    match macro_method {
        MacroMethod::Grounded => grounded::generate_macro(meta_action, plan),
        MacroMethod::Lifted => todo!(),
    }
}
