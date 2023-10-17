use std::{
    ffi::OsString,
    fs::{self, File},
    io::Write,
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

pub fn write_file(path: &OsString, content: String) {
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
