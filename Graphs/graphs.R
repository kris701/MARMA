library(dplyr) 
library(ggplot2)

source("style.R")
source("graphNames.R")
source("scatterPlots.R")
source("donutPlots.R")
source("coveragePlots.R")
source("domainBarPlots.R")

# Handle arguments
args = commandArgs(trailingOnly=TRUE)
#args[1] <- "results.csv"
#args[2] <- "meta_lifted"
#args[3] <- "meta_hashed"
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
		'numeric','numeric',
		'numeric','numeric',
		'numeric','numeric'
	)
)
data <- rename_data(data)

# Split data
AData = data[data$name == AName,]
BData = data[data$name == BName,]

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
	paste(AName, "_vs_", BName, "_solvedUnsolved.pdf"))

print("Generating: Cache Init times")
generate_domainBarPlot(
	finished,
	"cache_init_time.A",
	AName,
	"cache_init_time.B",
	BName,
	"Cache Init Times",
	paste(AName, "_vs_", BName, "_cacheInitTime.pdf"))

print("Generating: Cache Lookup times")
generate_domainBarPlot(
	finished,
	"cache_lookup_time.A",
	AName,
	"cache_lookup_time.B",
	BName,
	"Cache Lookup Times",
	paste(AName, "_vs_", BName, "_cacheLookupTime.pdf"))

print("Generating: Used Meta Actions (A)")
generate_domainBarPlot(
	finished,
	"meta_actions_in_plan.A",
	"Meta Actions In Plan",
	"found_in_cache.A",
	"Replacements Found",
	paste("Meta Actions in plan vs. Replacements found (", AName, ")"),
	paste(AName, "_metaActionCoverage.pdf"))

print("Generating: Used Meta Actions (B)")
generate_domainBarPlot(
	finished,
	"meta_actions_in_plan.B",
	"Meta Actions In Plan",
	"found_in_cache.B",
	"Replacements Found",
	paste("Meta Actions in plan vs. Replacements found (", BName, ")"),
	paste(BName, "_metaActionCoverage.pdf"))

print("Generating: Search Time Scatter")
generate_scatterplot(finished$search_time.A, AName, finished$search_time.B, BName, "Search Time", paste(AName, "_vs_", BName, "_searchTime.pdf"))

print("Generating: Total Time Scatter")
generate_scatterplot(finished$total_time.A, AName, finished$total_time.B, BName, "Total Time", paste(AName, "_vs_", BName, "_totalTime.pdf"))

print("Generating: Reconstruction Time Scatter")
generate_scatterplot(finished$reconstruction_time.A, AName, finished$reconstruction_time.B, BName, "Reconstruction Time", paste(AName, "_vs_", BName, "_reconstructionTime.pdf"))

print("Generating: Plan Length Scatter")
generate_scatterplot(finished$final_plan_length.A, AName, finished$final_plan_length.B, BName, "Final Plan Length", paste(AName, "_vs_", BName, "_finalPlanLength.pdf"))

print("Generating: Meta Plan Length Scatter")
generate_scatterplot(finished$meta_plan_length.A, AName, finished$meta_plan_length.B, BName, "Meta Plan Length", paste(AName, "_vs_", BName, "_metaPlanLength.pdf"))

print("Generating: Coverage plot")
generate_coveragePlot(finished$total_time.A, AName, finished$total_time.B, BName, "Coverage", "test.pdf")

