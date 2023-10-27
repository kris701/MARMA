use std::{
    ffi::OsString,
    fs::{self},
    path::PathBuf,
};

pub fn read_file(path: &OsString) -> String {
    match fs::read_to_string(path) {
        Ok(c) => c,
        Err(err) => panic!(
            "Could not read file: \"{}\"\nError: {}",
            path.to_str().unwrap(),
            err
        ),
    }
}

pub fn file_name(path: &PathBuf) -> String {
    let file_name = path.file_name().unwrap().to_str().unwrap();
    file_name.split(".").next().unwrap().to_owned()
}
