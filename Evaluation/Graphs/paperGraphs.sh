echo "== Generating MARMA vs FD Reconstruction =="
Rscript reconstructionGraphs.R results.csv meta_exact meta_no_cache
echo "== Generating FD vs FD + meta =="
Rscript reconstructionGraphs.R results.csv fast_downward fast_downward_meta
echo "== Generating coverage table =="
Rscript coverageGraphs.R results.csv
echo "== Generating IPC table =="
Rscript ipcGraphs.R ipc.csv
echo "== Generating training info table =="
Rscript trainGraphs.R trainresult.csv
