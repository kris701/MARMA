use nom::character::complete::char;
use nom::multi::many1;
use nom::sequence::delimited;

use crate::shared::spaced;
use crate::{
    shared::remove_comments,
    term::{parse_term, Term},
};

#[derive(Debug, PartialEq, Clone)]
pub enum StepType {
    Normal,
    Meta,
}

#[derive(Debug, PartialEq, Clone)]
pub struct SASPlan {
    pub steps: Vec<(Term, StepType)>,
}

fn generate_params(term: &Term) -> String {
    let mut s = "".to_string();
    term.parameters
        .iter()
        .for_each(|p| s.push_str(&format!(" {}", p)));
    s
}
impl SASPlan {
    pub fn has_meta(&self) -> bool {
        self.steps.iter().any(|s| match s.1 {
            StepType::Normal => false,
            StepType::Meta => true,
        })
    }

    pub fn meta_pos(&self) -> Option<usize> {
        match self
            .steps
            .iter()
            .position(|p| !matches!(p.1, StepType::Normal))
        {
            Some(l) => Some(l),
            None => None,
        }
    }

    pub fn to_string(&self) -> String {
        let mut s = "".to_string();

        self.steps
            .iter()
            .for_each(|t| s.push_str(&format!("({}{})\n", t.0.name, generate_params(&t.0))));
        s.push_str(&format!("; cost = {} (unit cost)", self.steps.len()));
        s
    }
}

fn get_meta(input: &str) -> Vec<usize> {
    input
        .lines()
        .enumerate()
        .filter(|(_, s)| s.contains('$'))
        .map(|(i, _)| i)
        .collect()
}

pub fn parse_sas(input: &str) -> Result<SASPlan, String> {
    let clean = remove_comments(input);
    let meta_actions = get_meta(&clean);
    let (_, step_values) =
        match many1(delimited(spaced(char('(')), parse_term, spaced(char(')'))))(&clean) {
            Ok(it) => it,
            Err(err) => return Err(err.to_string()),
        };
    let steps = step_values
        .iter()
        .enumerate()
        .map(|(i, s)| match meta_actions.contains(&i) {
            true => (s.to_owned(), StepType::Meta),
            false => (s.to_owned(), StepType::Normal),
        })
        .collect();

    Ok(SASPlan { steps })
}
