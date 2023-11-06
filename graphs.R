library(dplyr) 
library(ggplot2)
	
# Split the data into "meta" and "normal" data
# Also only consider data where there is both a meta and normal version of a domain+problem
data <- read.csv("results.csv")
splitData <- split(data, data$isMeta)
metaData <- merge(splitData$true, splitData$false, by = c("domain", "problem"), suffixes=c("", ".y"))
metaData <- metaData %>% select(-contains('.y'))
normData <- merge(splitData$false, splitData$true, by = c("domain", "problem"), suffixes=c("", ".y"))
normData <- normData %>% select(-contains('.y'))
combined <- merge(metaData, normData, by = c("domain", "problem"), suffixes=c(".meta", ".norm"))

# Generate Search Time Scatterplot
jpeg(file="searchtime.jpeg")
ggplot(combined, aes(x=searchTime.meta, y=searchTime.norm)) + 
	geom_point(shape=1, color = "red") +
	geom_abline(intercept = 0, slope = 1, color = "blue") +
	scale_x_continuous(trans=scales::pseudo_log_trans(base = 10)) +
	scale_y_continuous(trans=scales::pseudo_log_trans(base = 10)) +
	ggtitle("Search Time") + 
	xlab("With Reconstruction") +
	ylab("Without Reconstruction") + 
	theme(text = element_text(size=15),
		axis.text.x = element_text(angle=90, hjust=1)
	) +
	coord_equal(
		xlim=c(0, max(combined$searchTime.meta, combined$searchTime.norm)),
		ylim=c(0, max(combined$searchTime.meta, combined$searchTime.norm)),
	)
dev.off()

# Generate Total Time Scatterplot
jpeg(file="totaltime.jpeg")
ggplot(combined, aes(x=totalTime.meta, y=totalTime.norm)) + 
	geom_point(shape=1, color = "red") +
	geom_abline(intercept = 0, slope = 1, color = "blue") +
	scale_x_continuous(trans=scales::pseudo_log_trans(base = 10)) +
	scale_y_continuous(trans=scales::pseudo_log_trans(base = 10)) +
	ggtitle("Total Time") + 
	xlab("With Reconstruction") +
	ylab("Without Reconstruction") + 
	theme(text = element_text(size=15),
		axis.text.x = element_text(angle=90, hjust=1)
	) +
	coord_equal(
		xlim=c(0, max(combined$totalTime.meta, combined$totalTime.norm)),
		ylim=c(0, max(combined$totalTime.meta, combined$totalTime.norm)),
	)
dev.off()

# Generate Coverage plot
metaSearchTime <- lapply(list(combined$searchTime.meta), sort)[[1]]
normSearchTime <- lapply(list(combined$searchTime.norm), sort)[[1]]
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

jpeg(file="coverage.jpeg")
ggplot() +
	geom_line(data=metaCoverageData, aes(y=coverage,x= time,colour="With Reconstruction")) +
	geom_line(data=normCoverageData, aes(y=coverage,x= time,colour="Without Reconstruction")) +
	scale_color_manual(name = "Legend", values = c("With Reconstruction" = "red", "Without Reconstruction" = "blue")) +
	ggtitle("Coverage") + 
	xlab("Time") +
	ylab("Problems Solved") + 
	theme(text = element_text(size=15),
		axis.text.x = element_text(angle=90, hjust=1)
	)
dev.off()


