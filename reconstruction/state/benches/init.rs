use std::fs;
use std::path::{Path, PathBuf};

use criterion::{criterion_group, criterion_main, Criterion};

use spingus::domain::{parse_domain, Domain};
use spingus::problem::{parse_problem, Problem};
use state::instance::Instance;

use criterion::BenchmarkId;

fn init(domain: &Domain, problem: &Problem) {
    Instance::new(domain.to_owned(), problem.to_owned());
}

pub fn criterion_benchmark(c: &mut Criterion) {
    let mut group = c.benchmark_group("state init");
    for size in [1, 2, 4, 8, 16, 32].iter() {
        group.bench_with_input(BenchmarkId::from_parameter(size), size, |b, &size| {
            let domain_path = Path::new("benches/data/satellite/domain.pddl");
            let problem_path = format!("benches/data/satellite/instance-{}.pddl", size);
            let problem_path = Path::new(&problem_path);
            let domain_content = fs::read_to_string(domain_path).unwrap();
            let problem_content = fs::read_to_string(problem_path).unwrap();
            let domain = parse_domain(&domain_content).unwrap();
            let problem = parse_problem(&problem_content).unwrap();
            b.iter(|| init(&domain, &problem));
        });
    }
    group.finish();
}

criterion_group!(benches, criterion_benchmark);
criterion_main!(benches);
