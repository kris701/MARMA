#!/bin/bash
#SBATCH --output=/nfs/home/student.aau.dk/jmdh19/slurm-output/instance_runner-%A_%a.out  # Redirect the output stream to this file (%A_%a is the job's array-id and index)
#SBATCH --error=/nfs/home/student.aau.dk/jmdh19/slurm-output/instance_runner-%A_%a.err   # Redirect the error stream to this file (%A_%a is the job's array-id and index)
#SBATCH --partition=naples,dhabi,rome  # If you need run-times to be consistent across tests, you may need to restrict to one partition.
#SBATCH --mem=1G  # Memory limit that slurm allocatesAa
#SBATCH --mail-type=FAIL  # Type of email notification: BEGIN,END,FAIL,ALL
#SBATCH --mail-user=jmdh19@student.aau.dk

U="$1"

SCRATCH_DIRECTORY=/scratch/${U}
mkdir -p ${SCRATCH_DIRECTORY}
cd ${SCRATCH_DIRECTORY}

DOMAIN_DIR="$2"
echo "Domain dir: ${DOMAIN_DIR}"
RESULT_DIR="$3"
DOMAIN_NAME=${DOMAIN_DIR##*/}
echo "Domain name: ${DOMAIN_NAME}"
DOMAIN="${DOMAIN_DIR}/domain.pddl"
META_DOMAIN="${DOMAIN_DIR}/meta_domain.pddl"
CACHE="${DOMAIN_DIR}/cache"
PROBLEM_NAME="p${SLURM_ARRAY_TASK_ID}"
PROBLEM="${DOMAIN_DIR}/problems/${PROBLEM_NAME}.pddl"


FD_PATH="/nfs/home/student.aau.dk/jmdh19/bin/downward/fast-downward.py"
MS_PATH="/nfs/home/student.aau.dk/jmdh19/bin/meta_solver"
FD_RUNNER="/nfs/home/student.aau.dk/jmdh19/scripts/fd_runner.sh"
MS_RUNNER="/nfs/home/student.aau.dk/jmdh19/scripts/ms_runner.sh"

sbatch --job-name=${DOMAIN_NAME}_${PROBLEM_NAME}_fd ${FD_RUNNER} ${FD_PATH} ${DOMAIN} ${PROBLEM} ${RESULT_DIR}
sbatch --job-name=${DOMAIN_NAME}_${PROBLEM_NAME}_ms ${MS_RUNNER} ${FD_PATH} ${MS_PATH} ${DOMAIN} ${PROBLEM} ${META_DOMAIN} ${CACHE} ${RESULT_DIR}
