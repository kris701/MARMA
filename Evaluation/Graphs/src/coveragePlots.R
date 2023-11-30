library(dplyr) 
library(ggplot2)

source("./src/style.R")

generate_coveragePlot <- function(list1, name1, list2, name2, title, outName) {
	metaSearchTime <- lapply(list(list2), sort)[[1]]
	normSearchTime <- lapply(list(list1), sort)[[1]]
	highestValue <- max(metaSearchTime, normSearchTime)
	metaSearchTime <- metaSearchTime[metaSearchTime != highestValue]
	normSearchTime <- normSearchTime[normSearchTime != highestValue]

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
		geom_line(data=metaCoverageData, aes(y=coverage,x= time,colour=name2)) +
		geom_line(data=normCoverageData, aes(y=coverage,x= time,colour=name1)) +
		  scale_x_log10(
			limits=c(min(list1,list2),max(list1,list2)),
			labels = scales::trans_format("log10", scales::math_format(10^.x))) +
		ggtitle(title) + 
		xlab("Time (s)") +
		ylab("Problems Solved") + 
		theme(text = element_text(size=fontSize, family=fontFamily),
			axis.text.x = element_text(angle=90, hjust=1),
			legend.position = "bottom",
			legend.title = element_blank()
		)
	ggsave(plot=plot, filename=outName, width=imgWidth, height=imgHeight)
	return (plot)
}