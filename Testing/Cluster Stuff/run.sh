#!/bin/bash
DATA_DIR="$1"
RESULT_DIR="$2"

mkdir -p ${RESULT_DIR};

U=$(whoami)

for DOMAIN in $(ls "${DATA_DIR}") ; do 
    DOMAIN_DIR="${DATA_DIR}${DOMAIN}"
    echo "Reading dir ${DOMAIN_DIR}";
    PROBLEMS_DIR=$DOMAIN_DIR"/problems"
    echo "Find problems in ${PROBLEMS_DIR}"
    PROBLEM_COUNT=$(find $PROBLEMS_DIR -maxdepth 1 -type f|wc -l)
    echo "Found ${PROBLEM_COUNT} problems"
    CMD="sbatch --array=1-$PROBLEM_COUNT --job-name=$DOMAIN "$(pwd)/scripts/instance_runner.sh" ${U} ${DOMAIN_DIR} ${RESULT_DIR}"
    echo "Spawning task ${CMD}"
    eval "${CMD}"
done
