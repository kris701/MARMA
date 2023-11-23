library(dplyr) 
library(ggplot2)

source("src/style.R")
source("src/graphNames.R")
source("src/scatterPlots.R")
source("src/donutPlots.R")
source("src/coveragePlots.R")
source("src/domainBarPlots.R")
source("src/clamper.R")

# Handle arguments
args = commandArgs(trailingOnly=TRUE)
#args[1] <- "results.csv"
#args[2] <- "meta_no_cache"
#args[3] <- "meta_lifted"
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
		'numeric','numeric',
		'numeric','numeric',
		'numeric','numeric',
		'numeric', 'character', 
		'numeric', 'numeric',
		'numeric', 'numeric',
		'numeric', 'numeric'
	)
)
data <- rename_data(data)
if (nrow(data[data$name == AName,]) == 0)
	stop(paste("Column name '", args[2], "' not found in dataset!"), call.=FALSE)
if (nrow(data[data$name == BName,]) == 0)
	stop(paste("Column name '", args[3], "' not found in dataset!"), call.=FALSE)

data <- max_unsolved(data, "total_time")
data <- max_unsolved(data, "search_time")
data <- max_unsolved(data, "reconstruction_time")

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

print("Generating: Solved vs Unsolved")
generate_dounotplot(
	nrow(split(combined, combined$solved.A)$`true`),
	AName,
	nrow(split(combined, combined$solved.B)$`true`),
	BName,
	nrow(combined),
	"Solved vs. Unsolved",
	paste("out/", AName, "_vs_", BName, "_solvedUnsolved.pdf", sep = ""))

print("Generating: Cache Init times")
generate_domainBarPlot(
	finished,
	"cache_init_time.A",
	AName,
	"cache_init_time.B",
	BName,
	"Cache Init Times",
	paste("out/", AName, "_vs_", BName, "_cacheInitTime.pdf", sep = ""))

print("Generating: Cache Lookup times")
generate_domainBarPlot(
	finished,
	"cache_lookup_time.A",
	AName,
	"cache_lookup_time.B",
	BName,
	"Cache Lookup Times",
	paste("out/", AName, "_vs_", BName, "_cacheLookupTime.pdf", sep = ""))

print("Generating: Used Meta Actions (A)")
generate_domainBarPlot(
	finished,
	"meta_actions_in_plan.A",
	"Meta Actions In Plan",
	"found_in_cache.A",
	"Replacements Found",
	paste("Meta Actions vs. Replacements found (", AName, ")"),
	paste("out/", AName, "_metaActionCoverage.pdf", sep = ""))

print("Generating: Used Meta Actions (B)")
generate_domainBarPlot(
	finished,
	"meta_actions_in_plan.B",
	"Meta Actions In Plan",
	"found_in_cache.B",
	"Replacements Found",
	paste("Meta Actions vs. Replacements found (", BName, ")"),
	paste("out/", BName, "_metaActionCoverage.pdf", sep = ""))

print("Generating: Search Time Scatter")
searchData <- data.frame(x = combined$search_time.A, y = combined$search_time.B, domain = combined$domain)
generate_scatterplot(searchData , AName, BName, "Search Time", paste("out/", AName, "_vs_", BName, "_searchTime.pdf", sep = ""))

print("Generating: Total Time Scatter")
totalData <- data.frame(x = combined$total_time.A, y = combined$total_time.B, domain = combined$domain)
generate_scatterplot(totalData, AName, BName, "Total Time", paste("out/", AName, "_vs_", BName, "_totalTime.pdf", sep = ""))

print("Generating: Reconstruction Time Scatter")
reconData <- data.frame(x = combined$reconstruction_time.A, y = combined$reconstruction_time.B, domain = combined$domain)
generate_scatterplot(reconData, AName, BName, "Reconstruction Time", paste("out/", AName, "_vs_", BName, "_reconstructionTime.pdf", sep = ""))

print("Generating: Coverage plot")
generate_coveragePlot(finished$total_time.A, AName, finished$total_time.B, BName, "Coverage", paste("out/", AName, "_vs_", BName, "_coverage.pdf", sep = ""))

