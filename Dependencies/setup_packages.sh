echo == Installing packages ==
echo 
apt install cmake gcc-multilib g++-multilib flex bison python3 python2 curl automake dotnet-sdk-7.0 libfl-dev r-base
echo 
echo == Done! ==
echo 

echo == Installing Rust ==
echo 
curl --proto '=https' --tlsv1.2 https://sh.rustup.rs -sSf | sh
echo 
echo == Done! ==
echo 

echo == Installing R Packages ==
echo 
Rscript -e "install.packages(\"dplyr\");"
Rscript -e "install.packages(\"ggplot2\");"
Rscript -e "install.packages(\"gridExtra\");"
Rscript -e "install.packages(\"xtable\");"
echo 
echo == Done! ==
echo 
