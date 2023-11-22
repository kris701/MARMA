dotnet run --configuration Release --project MetaActions.Train -- \
	--domains 			Dependencies/learning-benchmarks/*/domain.pddl \
						Dependencies/downward-benchmarks/depot/domain.pddl \
						Dependencies/downward-benchmarks/driverlog/domain.pddl \
						Dependencies/downward-benchmarks/freecell/domain.pddl \
						Dependencies/downward-benchmarks/gripper/domain.pddl \
						Dependencies/downward-benchmarks/logistics98/domain.pddl \
						Dependencies/downward-benchmarks/mprime/domain.pddl \
						Dependencies/downward-benchmarks/mystery/domain.pddl \
						Dependencies/downward-benchmarks/zenotravel/domain.pddl \
	--train-problems 	Dependencies/learning-benchmarks/*/training/easy/p0*.pddl \
						Dependencies/learning-benchmarks/*/training/easy/p1*.pddl \
						Dependencies/downward-benchmarks/depot/p0*.pddl \
						Dependencies/downward-benchmarks/depot/p1*.pddl \
						Dependencies/downward-benchmarks/driverlog/p0*.pddl \
						Dependencies/downward-benchmarks/driverlog/p1*.pddl \
						Dependencies/downward-benchmarks/freecell/p0*.pddl \
						Dependencies/downward-benchmarks/freecell/p1*.pddl \
						Dependencies/downward-benchmarks/gripper/prob0*.pddl \
						Dependencies/downward-benchmarks/gripper/prob1*.pddl \
						Dependencies/downward-benchmarks/logistics98/prob0*.pddl \
						Dependencies/downward-benchmarks/logistics98/prob1*.pddl \
						Dependencies/downward-benchmarks/mprime/prob0*.pddl \
						Dependencies/downward-benchmarks/mprime/prob1*.pddl \
						Dependencies/downward-benchmarks/mystery/prob0*.pddl \
						Dependencies/downward-benchmarks/mystery/prob1*.pddl \
						Dependencies/downward-benchmarks/zenotravel/p0*.pddl \
						Dependencies/downward-benchmarks/zenotravel/p1*.pddl \
	--test-problems 	Dependencies/learning-benchmarks/*/testing/*/*.pddl \
						Dependencies/downward-benchmarks/depot/p2*.pddl \
						Dependencies/downward-benchmarks/driverlog/p2*.pddl \
						Dependencies/downward-benchmarks/freecell/probfreecell*.pddl \
						Dependencies/downward-benchmarks/gripper/prob2*.pddl \
						Dependencies/downward-benchmarks/logistics98/prob2*.pddl \
						Dependencies/downward-benchmarks/logistics98/prob3*.pddl \
						Dependencies/downward-benchmarks/mprime/prob2*.pddl \
						Dependencies/downward-benchmarks/mprime/prob3*.pddl \
						Dependencies/downward-benchmarks/mystery/prob2*.pddl \
						Dependencies/downward-benchmarks/mystery/prob3*.pddl \
						Dependencies/downward-benchmarks/zenotravel/p2*.pddl \
	--generation-strategy PDDLSharpMacros \
	--verification-strategy Weak1m \
	--multitask \
	--timelimit 120 \
	--rebuild

cp output/train/*.zip "TestingSets/all_p01-p19_PDDLSharpMacros_Weak1m_120m.zip"