library(dplyr) 
library(ggplot2)
library(grid)
library(gridExtra)
library(gtable)
library(xtable)

source("src/style.R")
source("src/graphNames.R")
source("src/scatterPlots.R")
source("src/donutPlots.R")
source("src/coveragePlots.R")
source("src/domainBarPlots.R")
source("src/tables.R")
source("src/clamper.R")
source("src/latexTableHelpers.R")

# Handle arguments
args = commandArgs(trailingOnly=TRUE)
#args[1] <- "results.csv"
#args[2] <- "meta_lifted_iterative"
#args[3] <- "meta_no_cache"
if (length(args) != 3) {
  stop("3 arguments must be supplied! The source data file, and one for each target reconstruction type", call.=FALSE)
}
AName <- recon_names(args[2])
BName <- recon_names(args[3])

# Read data file
data <- read.csv(
	args[1], 
	header = T, 
	sep = ",", 
	colClasses = c(
		'character','character',
		'character','character',
		'character',
		'numeric','numeric',
		'numeric','numeric',
		'numeric','numeric',
		'numeric','numeric',
		'numeric','numeric',
		'numeric','numeric',
		'numeric','numeric',
		'numeric','numeric'
	)
)
data <- rename_data(data)
if (nrow(data[data$name == AName,]) == 0)
	stop(paste("Column name '", args[2], "' not found in dataset!"), call.=FALSE)
if (nrow(data[data$name == BName,]) == 0)
	stop(paste("Column name '", args[3], "' not found in dataset!"), call.=FALSE)

data <- max_unsolved(data, "total_time")
data <- max_unsolved(data, "solution_time")
data <- max_unsolved(data, "reconstruction_time")
data <- min_unsolved(data, "meta_actions_in_plan")

# Split data
AData = data[data$name == AName,]
BData = data[data$name == BName,]
if (nrow(AData[AData$solved == 'true',]) == 0)
	stop(paste("Method '", args[2], "' have no solved instances!"), call.=FALSE)
if (nrow(BData[BData$solved == 'true',]) == 0)
	stop(paste("Method '", args[3], "' have no solved instances!"), call.=FALSE)

dir.create(file.path("out"), showWarnings = FALSE)

combined <- merge(AData, BData, by = c("domain", "problem"), suffixes=c(".A", ".B"))
combined <- combined %>% select(-contains('name.A'))
combined <- combined %>% select(-contains('name.B'))
finished <- split(combined, combined$solved.A)$`true`
finished <- split(finished, finished$solved.B)$`true`
finished <- finished %>% select(-contains('solved.A'))
finished <- finished %>% select(-contains('solved.B'))
containsMeta <- combined[!(combined$meta_actions_in_plan.A == 0 & combined$meta_actions_in_plan.B == 0),]

print("Generating: Solved vs Unsolved")
generate_dounotplot(
	nrow(split(combined, combined$solved.A)$`true`),
	AName,
	nrow(split(combined, combined$solved.B)$`true`),
	BName,
	nrow(combined),
	"Solved vs. Unsolved",
	paste("out/solvedUnsolved_", AName, "_vs_", BName, ".pdf", sep = ""))

print("Generating: Cache Init times")
generate_domainBarPlot(
	finished,
	"cache_init_time.A",
	AName,
	"cache_init_time.B",
	BName,
	"Cache Init Times (s)",
	paste("out/cacheInitTime_", AName, "_vs_", BName, ".pdf", sep = ""))

print("Generating: Cache Lookup times")
generate_domainBarPlot(
	finished,
	"cache_lookup_time.A",
	AName,
	"cache_lookup_time.B",
	BName,
	"Cache Lookup Times (s)",
	paste("out/cacheLookupTime_", AName, "_vs_", BName, ".pdf", sep = ""))

print("Generating: Used Meta Actions info")
tableData <- containsMeta %>% select(
	contains('domain'), 
	contains('meta_actions_in_plan.A'), 
	contains('found_in_cache.A'),
	contains('operator_count.A')
)
newTableData <- data.frame(domain=character(), meta_actions_in_plan.A = integer(), found_in_cache.A = integer(), operator_count.A = integer(), coverage = character())
for(domain in unique(tableData$domain)){
	newTableData[nrow(newTableData) + 1,] <- list(
		domain,
		sum(tableData[tableData$domain == domain,]$meta_actions_in_plan.A, na.rm=TRUE),
		sum(tableData[tableData$domain == domain,]$found_in_cache.A, na.rm=TRUE),
		sum(tableData[tableData$domain == domain,]$operator_count.A, na.rm=TRUE),
		paste(round((sum(tableData[tableData$domain == domain,]$found_in_cache.A, na.rm=TRUE) / sum(tableData[tableData$domain == domain,]$meta_actions_in_plan.A, na.rm=TRUE)) * 100, 0), "\\%", sep="")
	)
}
tableData <- newTableData
names(tableData) <- c(
	"$Domain$", 
	"$Meta_{plan}$", 
	"$C_{found}$",
	"$O_{cache}$",
	"$C_{perc}$"
)
generate_table(
	tableData,
	paste("out/reconstructionTable_", AName, ".pdf", sep = ""),
	10,
	10,
	FALSE
)
tableData <- tableData %>% select(-contains('$O_{cache}$'))
tableData[nrow(tableData) + 1,] <- list(
	"Total", 
	sum(tableData[2]), 
	sum(tableData[3]),
	"-"
)
table <- xtable(
		tableData, 
		type = "latex", 
		caption="Across all the problems in each domain, $Meta_{plan}$ is how many meta actions was in the plans, $C_{found}$ is how many replacements could be found in cache.",
		label="table:metaCoverage"
	)
hlines <- c(-1, 0, nrow(table) - 1, nrow(table))
align(table ) <- "|0|X|X|X|X|"
bold <- function(x){
	paste0('{\\textbf{ ', x, '}}')
}
print(table, 
	file = paste("out/reconstructionTable_", AName, ".tex", sep = ""), 
	include.rownames=FALSE,
	tabular.environment = "tabularx",
	width = "\\textwidth / 2",
	hline.after = hlines,
	sanitize.text.function = function(x) {x},
	latex.environments="centering",
	sanitize.colnames.function = bold,
	floating = TRUE)

print("Generating: Search Time Scatter")
sideA <- containsMeta$solution_time.A
sideB <- containsMeta$solution_time.B
sideDomains <- containsMeta$domain
if (AName == "FD" || AName == "FDM")
{
	sideA <- combined[combined$meta_actions_in_plan.B > 0,]$solution_time.A
	sideB <- combined[combined$meta_actions_in_plan.B > 0,]$solution_time.B
	sideDomains <- combined[combined$meta_actions_in_plan.B > 0,]$domain
}
if (BName == "FD" || BName == "FDM")
{
	sideA <- combined[combined$meta_actions_in_plan.A > 0,]$solution_time.A
	sideB <- combined[combined$meta_actions_in_plan.A > 0,]$solution_time.B
	sideDomains <- combined[combined$meta_actions_in_plan.A > 0,]$domain
}
if ((AName == "FD" || AName == "FDM") && (BName == "FD" || BName == "FDM"))
{
	sideA <- combined$solution_time.A
	sideB <- combined$solution_time.B
	sideDomains <- combined$domain
}
searchData <- data.frame(x = sideA, y = sideB, domain = sideDomains)
generate_scatterplot(searchData, AName, BName, "Search Time (s)", paste("out/searchTime_", AName, "_vs_", BName, ".pdf", sep = ""))

print("Generating: Total Time Scatter")
sideA <- containsMeta$total_time.A
sideB <- containsMeta$total_time.B
sideDomains <- containsMeta$domain
if (AName == "FD" || AName == "FDM")
{
	sideA <- combined[combined$meta_actions_in_plan.B > 0,]$total_time.A
	sideB <- combined[combined$meta_actions_in_plan.B > 0,]$total_time.B
	sideDomains <- combined[combined$meta_actions_in_plan.B > 0,]$domain
}
if (BName == "FD" || BName == "FDM")
{
	sideA <- combined[combined$meta_actions_in_plan.A > 0,]$total_time.A
	sideB <- combined[combined$meta_actions_in_plan.A > 0,]$total_time.B
	sideDomains <- combined[combined$meta_actions_in_plan.A > 0,]$domain
}
if ((AName == "FD" || AName == "FDM") && (BName == "FD" || BName == "FDM"))
{
	sideA <- combined$total_time.A
	sideB <- combined$total_time.B
	sideDomains <- combined$domain
}
totalData <- data.frame(x = sideA, y = sideB, domain = sideDomains)
generate_scatterplot(totalData, AName, BName, "Total Time (s)", paste("out/totalTime_", AName, "_vs_", BName, ".pdf", sep = ""))

print("Generating: Reconstruction Time Scatter")
sideA <- containsMeta$reconstruction_time.A
sideB <- containsMeta$reconstruction_time.B
sideDomains <- containsMeta$domain
if (AName == "FD" || AName == "FDM")
{
	sideA <- combined[combined$meta_actions_in_plan.B > 0,]$reconstruction_time.A
	sideB <- combined[combined$meta_actions_in_plan.B > 0,]$reconstruction_time.B
	sideDomains <- combined[combined$meta_actions_in_plan.B > 0,]$domain
}
if (BName == "FD" || BName == "FDM")
{
	sideA <- combined[combined$meta_actions_in_plan.A > 0,]$reconstruction_time.A
	sideB <- combined[combined$meta_actions_in_plan.A > 0,]$reconstruction_time.B
	sideDomains <- combined[combined$meta_actions_in_plan.A > 0,]$domain
}
if ((AName == "FD" || AName == "FDM") && (BName == "FD" || BName == "FDM"))
{
	sideA <- combined$reconstruction_time.A
	sideB <- combined$reconstruction_time.B
	sideDomains <- combined$domain
}
totalData <- data.frame(x = sideA, y = sideB, domain = sideDomains)
generate_scatterplot(totalData, AName, BName, "Reconstruction Time (s)", paste("out/reconstructionTime_", AName, "_vs_", BName, ".pdf", sep = ""))

print("Generating: Coverage plot")
maxTime <- max(combined$total_time.A, combined$total_time.B)
coverageData <- combined[!(combined$total_time.A == maxTime & combined$total_time.B == maxTime),]
generate_coveragePlot(coverageData$total_time.A, AName, coverageData$total_time.B, BName, "Coverage", paste("out/fullCoverage_", AName, "_vs_", BName, ".pdf", sep = ""))
