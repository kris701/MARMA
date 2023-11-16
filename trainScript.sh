dotnet run --configuration Release --project MetaActions.Train -- \
	--domains 			Dependencies/learning-benchmarks/*/domain.pddl \
						Dependencies/downward-benchmarks/depot/domain.pddl \
						Dependencies/downward-benchmarks/driverlog/domain.pddl \
						Dependencies/downward-benchmarks/freecell/domain.pddl \
						Dependencies/downward-benchmarks/gripper/domain.pddl \
						Dependencies/downward-benchmarks/logistics98/domain.pddl \
						Dependencies/downward-benchmarks/movie/domain.pddl \
						Dependencies/downward-benchmarks/mprime/domain.pddl \
						Dependencies/downward-benchmarks/mystery/domain.pddl \
						Dependencies/downward-benchmarks/philosophers/domain.pddl \
						Dependencies/downward-benchmarks/storage/domain.pddl \
						Dependencies/downward-benchmarks/zenotravel/domain.pddl \
	--train-problems 	Dependencies/learning-benchmarks/*/training/easy/p0*.pddl \
						Dependencies/downward-benchmarks/depot/p0*.pddl \
						Dependencies/downward-benchmarks/driverlog/p0*.pddl \
						Dependencies/downward-benchmarks/freecell/p0*.pddl \
						Dependencies/downward-benchmarks/gripper/p0*.pddl \
						Dependencies/downward-benchmarks/logistics98/prob0*.pddl \
						Dependencies/downward-benchmarks/movie/prob0*.pddl \
						Dependencies/downward-benchmarks/mprime/prob0*.pddl \
						Dependencies/downward-benchmarks/mystery/prob0*.pddl \
						Dependencies/downward-benchmarks/philosophers/p0*.pddl \
						Dependencies/downward-benchmarks/storage/p0*.pddl \
						Dependencies/downward-benchmarks/zenotravel/p0*.pddl \
	--test-problems 	Dependencies/learning-benchmarks/*/testing/*/*.pddl \
						Dependencies/downward-benchmarks/depot/p1*.pddl \
						Dependencies/downward-benchmarks/depot/p2*.pddl \
						Dependencies/downward-benchmarks/driverlog/p1*.pddl \
						Dependencies/downward-benchmarks/driverlog/p2*.pddl \
						Dependencies/downward-benchmarks/freecell/p1*.pddl \
						Dependencies/downward-benchmarks/freecell/probfreecell*.pddl \
						Dependencies/downward-benchmarks/gripper/p1*.pddl \
						Dependencies/downward-benchmarks/gripper/p2*.pddl \
						Dependencies/downward-benchmarks/logistics98/prob1*.pddl \
						Dependencies/downward-benchmarks/logistics98/prob2*.pddl \
						Dependencies/downward-benchmarks/logistics98/prob3*.pddl \
						Dependencies/downward-benchmarks/movie/prob1*.pddl \
						Dependencies/downward-benchmarks/movie/prob2*.pddl \
						Dependencies/downward-benchmarks/movie/prob3*.pddl \
						Dependencies/downward-benchmarks/mprime/prob1*.pddl \
						Dependencies/downward-benchmarks/mprime/prob2*.pddl \
						Dependencies/downward-benchmarks/mprime/prob3*.pddl \
						Dependencies/downward-benchmarks/mystery/prob1*.pddl \
						Dependencies/downward-benchmarks/mystery/prob2*.pddl \
						Dependencies/downward-benchmarks/mystery/prob3*.pddl \
						Dependencies/downward-benchmarks/philosophers/p1*.pddl \
						Dependencies/downward-benchmarks/philosophers/p2*.pddl \
						Dependencies/downward-benchmarks/philosophers/p3*.pddl \
						Dependencies/downward-benchmarks/philosophers/p4*.pddl \
						Dependencies/downward-benchmarks/storage/p1*.pddl \
						Dependencies/downward-benchmarks/storage/p2*.pddl \
						Dependencies/downward-benchmarks/storage/p3*.pddl \
						Dependencies/downward-benchmarks/zenotravel/p1*.pddl \
						Dependencies/downward-benchmarks/zenotravel/p2*.pddl \
	--method PDDLSharpMacros \
	--multitask