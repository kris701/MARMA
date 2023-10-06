echo == Installing packages ==
echo 
apt install cmake gcc-multilib g++-multilib flex bison python3 python2 curl automake
echo 
echo == Done! ==
echo 

echo == Installing Rust ==
echo 
curl --proto '=https' --tlsv1.2 https://sh.rustup.rs -sSf | sh
echo 
echo == Done! ==
echo 

echo == Installing Fast-Downward ==
echo 
git clone https://github.com/aibasel/downward fast-downward
cd fast-downward
python3 build.py
cd ..
echo 
echo == Done! ==
echo 

echo == Installing Stackelberg Planner ==
echo 
git clone -n --depth=1 --filter=tree:0 https://gitlab.com/atorralba_planners/stackelberg-planner-sls stackelberg-planner
cd stackelberg-planner
git sparse-checkout set --no-cone src
git checkout
cd src
sed -e s/-Werror//g -i preprocess/Makefile
sed -e s/-Werror//g -i search/Makefile
bash build_all -j
cd ..
cd ..
echo 
echo == Done! ==
echo 

echo == Installing Benchmarks ==
echo 
git clone https://github.com/aibasel/downward-benchmarks downward-benchmarks-master
echo 
echo == Done! ==
echo 

echo == Installing VAL ==
echo 
git clone https://github.com/KCL-Planning/VAL.git VAL
cd VAL
cmake build .
make -j
cd ..
echo 
echo == Done! ==
echo 
