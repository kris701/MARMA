library(dplyr) 
library(ggplot2)

imgWidth <- 4
imgHeight <- 5

recon_names <- function(name) { 
  	if (name == "fast_downward") return ("Fast Downward")
  	if (name == "fast_downward_meta") return ("Fast Downward (Meta)")
  	if (name == "meta_no_cache") return ("FD Reconstruction")
  	if (name == "meta_hashed") return ("MARMA (Hashed)")
  	if (name == "meta_lifted") return ("MARMA (Lifted)")
	return (name)
}

generate_scatterplot <- function(list1, list2, title, outName) {
	plot <- ggplot(finished, aes(x=list1, y=list2, color=domain)) + 
		geom_point(size=2) +
		geom_abline(intercept = 0, slope = 1, color = "black") +
		  scale_x_log10(
			limits=c(min(list1, list2),max(list1, list2)),
			labels = scales::trans_format("log10", scales::math_format(10^.x))
		) +
		  scale_y_log10(
			limits=c(min(list1, list2),max(list1, list2)),
			labels = scales::trans_format("log10", scales::math_format(10^.x))
		) +
		ggtitle(title) + 
		labs(shape = "", color = "") +
		xlab(BName) +
		ylab(AName) + 
		theme(text = element_text(size=15, family="serif"),
			axis.text.x = element_text(angle=90, hjust=1),
			legend.position="bottom"
		)
	ggsave(plot=plot, filename=outName, width=imgWidth, height=imgHeight)
	return (plot)
}

# Handle arguments
args = commandArgs(trailingOnly=TRUE)
args[1] <- "results.csv"
args[2] <- "meta_no_cache"
args[3] <- "meta_hashed"
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
data[data=="fast_downward"] <- "Fast Downward"
data[data=="fast_downward_meta"] <- "Fast Downward (Meta)"
data[data=="meta_no_cache"] <- "FD Reconstruction"
data[data=="meta_hashed"] <- "MARMA (Hashed)"
data[data=="meta_lifted"] <- "MARMA (Lifted)"

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
	# Solved vs Not solved donut
	plot <- ggplot() + 
		geom_col(aes(x = 2, y = nrow(combined)), fill = "gray", color = "black") + 
		geom_col(aes(x = 2, y = nrow(split(combined, combined$solved.B)$`true`), fill = BName), color = "black") + 
		geom_col(aes(x = 3, y = nrow(combined)), fill = "gray", color = "black") + 
		geom_col(aes(x = 3, y = nrow(split(combined, combined$solved.A)$`true`), fill = AName), color = "black") +
		xlim(0, 3.5) + labs(x = NULL, y = NULL) + 
		ggtitle("Solved vs. Unsolved") + 
		labs(fill = "", color = "") +
		theme(text = element_text(size=15, family="serif"),
			axis.ticks=element_blank(),
			axis.text.y=element_blank(),
			axis.title=element_blank(),
			legend.position="bottom") +
		coord_polar(theta = "y") 
	# plot
	ggsave(plot=plot, filename=paste(BName, "solvedvsunsolved.pdf"), width=imgWidth, height=imgHeight)

	print("Generating: Search Time Scatter")
	generate_scatterplot(finished$search_time.A, finished$search_time.B, "Search Time", paste(AName, "_vs_", BName, "_searchTime.pdf"))

	print("Generating: Total Time Scatter")
	generate_scatterplot(finished$total_time.A, finished$total_time.B, "Total Time", paste(AName, "_vs_", BName, "_totalTime.pdf"))

	print("Generating: Plan Length Scatter")
	generate_scatterplot(finished$final_plan_length.A, finished$final_plan_length.B, "Final Plan Length", paste(AName, "_vs_", BName, "_finalPlanLength.pdf"))

	print("Generating: Meta Plan Length Scatter")
	generate_scatterplot(finished$meta_plan_length.A, finished$meta_plan_length.B, "Meta Plan Length", paste(AName, "_vs_", BName, "_metaPlanLength.pdf"))

	print("Generating: Coverage plot")
	# Generate Coverage plot
	metaSearchTime <- lapply(list(finished$total_time.B), sort)[[1]]
	normSearchTime <- lapply(list(finished$total_time.A), sort)[[1]]
	highestValue <- max(metaSearchTime, normSearchTime)

	metaUnique <- unique(metaSearchTime)
	metaCounter <- c()
	normUnique <- unique(normSearchTime)
	normCounter <- c()

	dups <- duplicated(metaSearchTime)
	last <- 0
	for (i in 1:length(metaSearchTime)){
		if (dups[i] == TRUE)
		{
			metaCounter[last] <- metaCounter[last] + 1
		}
		else
		{
			if (last == 0){
				metaCounter[1] <- 1
				last <- 1
			} else {
				last <- last + 1
				metaCounter[last] <- metaCounter[last - 1] + 1
			}
		}
	}
	metaCoverageData <- data.frame(time=metaUnique, coverage=metaCounter)

	dups <- duplicated(normSearchTime)
	last <- 0
	for (i in 1:length(normSearchTime)){
		if (dups[i] == TRUE)
		{
			normCounter[last] <- normCounter[last] + 1
		}
		else
		{
			if (last == 0){
				normCounter[1] <- 1
				last <- 1
			} else {
				last <- last + 1
				normCounter[last] <- normCounter[last - 1] + 1
			}
		}
	}
	normCoverageData <- data.frame(time=normUnique, coverage=normCounter)

	plot <- ggplot() +
		geom_line(data=metaCoverageData, aes(y=coverage,x= time,colour=BName)) +
		geom_line(data=normCoverageData, aes(y=coverage,x= time,colour=AName)) +
		  scale_x_log10(
			limits=c(min(finished$total_time.B,finished$total_time.A),max(finished$total_time.B,finished$total_time.A)),
			labels = scales::trans_format("log10", scales::math_format(10^.x))) +
		ggtitle("Coverage") + 
		xlab("Time") +
		ylab("Problems Solved") + 
		theme(text = element_text(size=15, family="serif"),
			axis.text.x = element_text(angle=90, hjust=1),
			legend.position = "bottom",
			legend.title = element_blank()
		)
	# plot
	ggsave(plot=plot, filename=paste(BName, "coverage.pdf"), width=imgWidth, height=imgHeight)


