use std::{fs, io, path::PathBuf};

pub fn file_name(path: &PathBuf) -> String {
    let file_name = path.file_name().unwrap().to_str().unwrap();
    file_name.split(".").next().unwrap().to_owned()
}

pub fn dir_name(path: &PathBuf) -> String {
    path.file_name().unwrap().to_str().unwrap().to_string()
}

pub fn dir_dirs(path: &PathBuf) -> io::Result<Vec<PathBuf>> {
    let iter = fs::read_dir(path)?;
    let mut result: Vec<PathBuf> = Vec::new();
    for item in iter {
        let path = item?.path();
        if path.is_dir() {
            result.push(path);
        }
    }
    Ok(result)
}

pub fn dir_files(path: &PathBuf) -> io::Result<Vec<PathBuf>> {
    let iter = fs::read_dir(path)?;
    let mut result: Vec<PathBuf> = Vec::new();
    for item in iter {
        let path = item?.path();
        if path.is_file() {
            result.push(path);
        }
    }
    Ok(result)
}

pub fn dir_files_by_extension(path: &PathBuf, ext: &str) -> io::Result<Vec<PathBuf>> {
    let files = dir_files(path)?;
    Ok(files
        .into_iter()
        .filter(|file| {
            if let Some(extension) = file.extension() {
                if let Some(extension) = extension.to_str() {
                    return extension == ext;
                } else {
                    eprintln!(
                        "Unexpected token in file extension: {:?}. File is ignored.",
                        extension
                    )
                }
            }
            return false;
        })
        .collect())
}

/// Matches every file in files with a set of files from other_files
/// A match occurs if its name occurs
pub fn match_files(files: Vec<PathBuf>, other_files: Vec<PathBuf>) -> Vec<(PathBuf, PathBuf)> {
    let mut matches: Vec<(PathBuf, PathBuf)> = Vec::new();

    for file in files {
        let s = file_name(&file);
        let contained = other_files
            .iter()
            .find(|f| file_name(&f).split_once('_').unwrap().0 == s)
            .cloned();
        if let Some(contained) = contained {
            matches.push((file, contained));
        }
    }

    matches
}
