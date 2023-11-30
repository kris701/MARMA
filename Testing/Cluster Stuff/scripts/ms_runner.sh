#!/bin/bash
#SBATCH --time=0:30:00
#SBATCH --partition=naples
#SBATCH --mem=4000
#SBATCH --mail-type=FAIL  # Type of email notification: BEGIN,END,FAIL,ALL
#SBATCH --mail-user=jmdh19@student.aau.dk
#SBATCH --output=/nfs/home/student.aau.dk/jmdh19/slurm-output/ms_runner-%A.out  # Redirect the output stream to this file (%A_%a is the job's array-id and index)
#SBATCH --error=/nfs/home/student.aau.dk/jmdh19/slurm-output/ms_runner-%A.err   # Redirect the error stream to this file (%A_%a is the job's array-id and index)

FAST_DOWNWARD="$1"
META_SOLVER="$2"
DOMAIN="$3"
DOMAIN_NAME="$(dirname "$DOMAIN")"
DOMAIN_NAME=${DOMAIN_NAME##*/}
PROBLEM="$4"
PROBLEM_NAME=${PROBLEM##*/}
META_DOMAIN="$5"
CACHE="$6"
RESULT_DIR="$7"
RESULT_FILE="${RESULT_DIR}/${SLURM_JOBID}"

PD=$(pwd)

PROCESS_DIRECTORY=${PD}/${SLURM_JOBID}
mkdir -p ${PROCESS_DIRECTORY}
cd ${PROCESS_DIRECTORY}

MS="${META_SOLVER} -d ${DOMAIN} -m ${META_DOMAIN} -p ${PROBLEM} -f ${FAST_DOWNWARD} -c ${CACHE}"
MS="${MS} >> /dev/null"

t1=${EPOCHREALTIME/[^0-9]/}
t1=${t1%???}
eval "${MS}"
EXIT_CODE=$?
t2=${EPOCHREALTIME/[^0-9]/}
t2=${t2%???}
MS_TIME=$(($t2 - $t1))

OUTPUT="true, ${DOMAIN_NAME}, ${PROBLEM_NAME}, ${MS_TIME}, ${MS_TIME}, false"
echo ${OUTPUT} > ${RESULT_FILE}

cd ${PD}
[ -d "${SLURM_JOBID}" ] && rm -r ${SLURM_JOBID}

exit ${EXIT_CODE}

