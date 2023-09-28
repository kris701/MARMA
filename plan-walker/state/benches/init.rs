use criterion::{black_box, criterion_group, criterion_main, Criterion};

use shared::io::file::read_file;
use std::ffi::OsString;

use parsing::domain::Domain;
use parsing::problem::Problem;
use state::instance::Instance;

use criterion::BenchmarkId;

fn init(domain: &Domain, problem: &Problem) {
    Instance::new(domain.to_owned(), problem.to_owned());
}

pub fn criterion_benchmark(c: &mut Criterion) {
    let mut group = c.benchmark_group("state init");
    for size in [1, 2, 4, 8, 16, 32].iter() {
        group.bench_with_input(BenchmarkId::from_parameter(size), size, |b, &size| {
            let domain_path: OsString = Into::into("benches/data/satellite/domain.pddl");
            let problem_path: OsString =
                Into::into(format!("benches/data/satellite/instance-{}.pddl", size));
            let domain = Domain::from(&domain_path);
            let problem = Problem::from(&problem_path);
            b.iter(|| init(&domain, &problem));
        });
    }
    group.finish();
}

criterion_group!(benches, criterion_benchmark);
criterion_main!(benches);
