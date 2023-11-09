library(dplyr) 
library(ggplot2)

imgWidth <- 9
imgHeight <- 8
	
# Split the data into "meta" and "normal" data
# Also only consider data where there is both a meta and normal version of a domain+problem
data <- read.csv("results.csv")
splitData <- split(data, data$isMeta)
metaData <- merge(splitData$true, splitData$false, by = c("domain", "problem"), suffixes=c("", ".y"))
metaData <- metaData %>% select(-contains('.y'))
normData <- merge(splitData$false, splitData$true, by = c("domain", "problem"), suffixes=c("", ".y"))
normData <- normData %>% select(-contains('.y'))
combined <- merge(metaData, normData, by = c("domain", "problem"), suffixes=c(".meta", ".norm"))
combined <- combined[,!grepl("isMeta",names(combined))]

# Generate Search Time Scatterplot
plot <- ggplot(combined, aes(x=searchTime.meta, y=searchTime.norm, shape=domain, color=domain)) + 
	geom_point(size=2) +
	geom_abline(intercept = 0, slope = 1, color = "black") +
      scale_x_log10(
		limits=c(min(combined$searchTime.meta,combined$searchTime.norm),max(combined$searchTime.meta,combined$searchTime.norm)),
		labels = scales::trans_format("log10", scales::math_format(10^.x))) +
      scale_y_log10(
		limits=c(min(combined$searchTime.meta,combined$searchTime.norm),max(combined$searchTime.meta,combined$searchTime.norm)),
		labels = scales::trans_format("log10", scales::math_format(10^.x))) +
	ggtitle("Search Time") + 
	labs(shape = "Domains", color = "Domains") +
	xlab("With Reconstruction") +
	ylab("Without Reconstruction") + 
	theme(text = element_text(size=15, family="serif"),
		axis.text.x = element_text(angle=90, hjust=1)
	)
# plot
ggsave(plot=plot, filename="searchTime.pdf", width=imgWidth, height=imgHeight)

# Generate Total Time Scatterplot
plot <- ggplot(combined, aes(x=totalTime.meta, y=totalTime.norm, shape=domain, color=domain)) + 
	geom_point(size=2) +
	geom_abline(intercept = 0, slope = 1, color = "black") +
      scale_x_log10(
		limits=c(min(combined$totalTime.meta,combined$totalTime.norm),max(combined$totalTime.meta,combined$totalTime.norm)),
		labels = scales::trans_format("log10", scales::math_format(10^.x))) +
      scale_y_log10(
		limits=c(min(combined$totalTime.meta,combined$totalTime.norm),max(combined$totalTime.meta,combined$totalTime.norm)),
		labels = scales::trans_format("log10", scales::math_format(10^.x))) +
	ggtitle("Total Time") + 
	labs(shape = "Domains", color = "Domains") +
	xlab("With Reconstruction") +
	ylab("Without Reconstruction") + 
	theme(text = element_text(size=15, family="serif"),
		axis.text.x = element_text(angle=90, hjust=1)
	)
# plot
ggsave(plot=plot, filename="totalTime.pdf", width=imgWidth, height=imgHeight)

# Generate Coverage plot
finished <- split(combined, combined$wasSolutionFound.meta)$` true`
finished <- split(finished, finished$wasSolutionFound.norm)$` true`
metaSearchTime <- lapply(list(finished$totalTime.meta), sort)[[1]]
normSearchTime <- lapply(list(finished$totalTime.norm), sort)[[1]]
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
	geom_line(data=metaCoverageData, aes(y=coverage,x= time,colour="With Reconstruction")) +
	geom_line(data=normCoverageData, aes(y=coverage,x= time,colour="Without Reconstruction")) +
	scale_color_manual(name = "Legend", values = c("With Reconstruction" = "red", "Without Reconstruction" = "blue")) +
	ggtitle("Coverage") + 
	xlab("Time") +
	ylab("Problems Solved") + 
	theme(text = element_text(size=15),
		axis.text.x = element_text(angle=90, hjust=1)
	)
plot
ggsave(plot=plot, filename="coverage.pdf", width=imgWidth, height=imgHeight)


