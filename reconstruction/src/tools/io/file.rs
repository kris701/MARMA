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

fn sort_files(files: Vec<PathBuf>) -> Vec<PathBuf> {
    let mut files = files;
    files.sort_by(|a, b| {
        a.file_name()
            .unwrap()
            .len()
            .cmp(&b.file_name().unwrap().len())
    });
    files
}

pub fn match_files(files: Vec<PathBuf>, sub_files: Vec<PathBuf>) -> Vec<(PathBuf, PathBuf)> {
    let mut pairs: Vec<(PathBuf, PathBuf)> = Vec::new();

    let files = sort_files(files);
    let mut sub_files = sort_files(sub_files);
    let mut unmatched_files: Vec<PathBuf> = Vec::new();

    for file in files.into_iter() {
        let mut found = false;
        for (i, sub_file) in sub_files.iter().enumerate() {
            if file_name(&file).contains(&file_name(&sub_file)) {
                found = true;
                pairs.push((file.clone(), sub_file.to_owned()));
                sub_files.remove(i);
                break;
            }
        }
        if !found {
            unmatched_files.push(file);
        }
    }

    for file in unmatched_files.into_iter().chain(sub_files.into_iter()) {
        eprintln!("WARNING: Could not find match for file {:?}", file);
    }

    pairs
}

pub fn read_pairs(files: Vec<(PathBuf, PathBuf)>) -> io::Result<Vec<(String, String)>> {
    let mut pairs: Vec<(String, String)> = Vec::new();

    for (a, b) in files {
        pairs.push((fs::read_to_string(a)?, fs::read_to_string(b)?));
    }

    Ok(pairs)
}
