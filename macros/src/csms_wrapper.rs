use std::{
    fs::{self, remove_dir_all},
    path::PathBuf,
    process::Command,
};

use spingus::domain::{action::Action, parse_domain};

use crate::script_writing::generate_script;

enum ScriptType {
    CSM(String),
    //CCSM(CSMType),
    //MUM,
    //BloMa,
}

fn generate_temp_dir(temp_path: &PathBuf, domain_path: &PathBuf, problems_path: &PathBuf) {
    let problems: Vec<PathBuf> = fs::read_dir(problems_path)
        .unwrap()
        .map(|p| p.unwrap().path())
        .collect();
    let (learning, validation) = problems.split_at(problems.len());

    let _ = remove_dir_all(temp_path);
    let _ = fs::create_dir(temp_path);
    let _ = fs::copy(domain_path, temp_path.join("domain.pddl"));
    let learning_path = temp_path.join("learn");
    let _ = fs::create_dir(&learning_path);
    learning.iter().for_each(|p| {
        let _ = fs::copy(p, &learning_path.join(p.file_name().unwrap()));
    });
    let validation_path = temp_path.join("testing");
    let _ = fs::create_dir(&validation_path);
    validation.iter().for_each(|p| {
        let _ = fs::copy(p, &validation_path.join(p.file_name().unwrap()));
    });
}

fn run(
    fastdownward_path: &PathBuf,
    script_path: &PathBuf,
    scripts_path: &PathBuf,
    temp_path: &PathBuf,
    script_type: &ScriptType,
) -> Option<String> {
    match script_type {
        ScriptType::CSM(csm_type) => {
            generate_script(fastdownward_path, script_path);

            let learn_path = scripts_path.join("./learn-csm.sh");
            if !learn_path.is_file() {
                panic!("Could not find: {}", learn_path.to_str().unwrap());
            }
            let out = Command::new("./learn-csm.sh")
                .current_dir(scripts_path)
                .args(&[
                    fs::canonicalize(temp_path).unwrap().to_str().unwrap(),
                    ".temp.sh",
                    &csm_type,
                ])
                .output();
            match out {
                Err(err) => {
                    panic!("csms gave error: {}", err);
                }
                _ => {}
            }
            let domain_name = format!("domain_{}.pddl", csm_type);
            let domain_path = temp_path.join(domain_name);
            if domain_path.exists() {
                return Some(fs::read_to_string(domain_path).unwrap());
            } else {
                return None;
            }
        }
    }
}

pub fn generate_macros(
    fastdownward_path: &PathBuf,
    csms_path: &PathBuf,
    domain_path: &PathBuf,
    problems_path: &PathBuf,
    temp_path: &PathBuf,
) -> Vec<Action> {
    let scripts_path = csms_path.join("scripts");
    let script_path = scripts_path.join(".temp.sh");
    let mut enhanced_domains: Vec<String> = vec![];
    for script_type in [
        ScriptType::CSM("csm".to_owned()),
        ScriptType::CSM("ncsm".to_owned()),
        ScriptType::CSM("acsm".to_owned()),
        ScriptType::CSM("ancsm".to_owned()),
    ] {
        println!("Generating temp dir...");
        generate_temp_dir(&temp_path, domain_path, problems_path);
        println!("Generating macros...");
        let enhanced_domain = run(
            fastdownward_path,
            &script_path,
            &scripts_path,
            &temp_path,
            &script_type,
        );
        if let Some(domain) = enhanced_domain {
            enhanced_domains.push(domain);
        }
    }
    enhanced_domains
        .iter()
        .flat_map(|s| {
            let domain = parse_domain(s).unwrap();
            domain
                .actions
                .iter()
                .filter_map(|a| {
                    if a.name.contains("_mcr_") {
                        return Some(a.to_owned());
                    } else {
                        return None;
                    }
                })
                .collect::<Vec<Action>>()
        })
        .collect::<Vec<Action>>()
}
