use std::path::PathBuf;

pub fn file_name(path: &PathBuf) -> String {
    let file_name = path.file_name().unwrap().to_str().unwrap();
    file_name.split(".").next().unwrap().to_owned()
}
