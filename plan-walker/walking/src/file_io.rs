use std::{
    ffi::OsString,
    fs::{self, File},
    io::Write,
    path::Path,
};

use crate::time::run_time;

pub fn read_file(path: &OsString) -> String {
    println!(
        "{} Reading {}...",
        run_time(),
        Path::new(path).file_name().unwrap().to_str().unwrap()
    );
    match fs::read_to_string(path) {
        Ok(c) => c,
        Err(err) => panic!(
            "Could not read file: \"{}\"\nError: {}",
            path.to_str().unwrap(),
            err
        ),
    }
}

pub fn write_file(path: &OsString, content: String) {
    println!(
        "{} Creating {}...",
        run_time(),
        Path::new(path).file_name().unwrap().to_str().unwrap()
    );
    match File::create(path) {
        Ok(mut output) => match write!(output, "{}", content) {
            Ok(_) => {}
            Err(err) => panic!(
                "Could write to file: \"{}\"\nError: {}",
                path.to_str().unwrap(),
                err
            ),
        },
        Err(err) => panic!(
            "Could create file: \"{}\"\nError: {}",
            path.to_str().unwrap(),
            err
        ),
    };
}
