library(dplyr) 
library(ggplot2)

imgWidth <- 8
imgHeight <- 9
	
# Get both data files
data1 <- read.csv("norm.csv", header = T, sep = ",", colClasses=c('character','character','character','character','numeric','numeric','numeric'))
data1Name = data1["name"][1,]
data2All <- read.csv("meta.csv", header = T, sep = ",", colClasses=c('character','character','character','character','numeric','numeric','numeric'))

names <- unique(data2All["name"])
names <- names[names != "" & names != data1Name,]
for (i in names)
{
	data2 <- data2All[data2All$name == i,]
	data2Name = data2["name"][1,]
	print("Generating for:")
	print(data2Name)

	# Stitch data
	combined <- merge(data1, data2, by = c("domain", "problem"), suffixes=c(".norm", ".meta"))
	combined <- combined %>% select(-contains('name.norm'))
	combined <- combined %>% select(-contains('name.meta'))
	finished <- split(combined, combined$solved.meta)$`true`
	finished <- split(finished, finished$solved.norm)$`true`
	finished <- finished %>% select(-contains('solved.norm'))
	finished <- finished %>% select(-contains('solved.meta'))

	print("Generating: Solved vs Unsolved")
	# Solved vs Not solved donut
	plot <- ggplot() + 
		geom_col(aes(x = 2, y = nrow(combined)), fill = "gray", color = "black") + 
		geom_col(aes(x = 2, y = nrow(split(combined, combined$solved.meta)$`true`), fill = data2Name), color = "black") + 
		geom_col(aes(x = 3, y = nrow(combined)), fill = "gray", color = "black") + 
		geom_col(aes(x = 3, y = nrow(split(combined, combined$solved.norm)$`true`), fill = data1Name), color = "black") +
		xlim(0, 3.5) + labs(x = NULL, y = NULL) + 
		ggtitle("Solved vs. Unsolved") + 
		labs(fill = "", color = "") +
		theme(text = element_text(size=15, family="serif"),
			axis.ticks=element_blank(),
			axis.text.y=element_blank(),
			axis.title=element_blank(),
			legend.position="bottom") +
		coord_polar(theta = "y") 
	 plot
	ggsave(plot=plot, filename=paste(data2Name, "solvedvsunsolved.pdf"), width=imgWidth, height=imgHeight)

	print("Generating: Search Time Scatter")
	# Generate Search Time Scatterplot
	plot <- ggplot(finished, aes(x=search_time.meta, y=search_time.norm, color=domain)) + 
		geom_point(size=2) +
		geom_abline(intercept = 0, slope = 1, color = "black") +
		  scale_x_log10(
			limits=c(min(finished$search_time.meta,finished$search_time.norm),max(finished$search_time.meta,finished$search_time.norm)),
			labels = scales::trans_format("log10", scales::math_format(10^.x))) +
		  scale_y_log10(
			limits=c(min(finished$search_time.meta,finished$search_time.norm),max(finished$search_time.meta,finished$search_time.norm)),
			labels = scales::trans_format("log10", scales::math_format(10^.x))) +
		ggtitle("Search Time") + 
		labs(shape = "", color = "") +
		xlab(data2Name) +
		ylab(data1Name) + 
		theme(text = element_text(size=15, family="serif"),
			axis.text.x = element_text(angle=90, hjust=1),
			legend.position="bottom"
		)
	# plot
	ggsave(plot=plot, filename=paste(data2Name, "searchTime.pdf"), width=imgWidth, height=imgHeight)

	print("Generating: Total Time Scatter")
	# Generate Total Time Scatterplot
	plot <- ggplot(finished, aes(x=total_time.meta, y=total_time.norm, color=domain)) + 
		geom_point(size=2) +
		geom_abline(intercept = 0, slope = 1, color = "black") +
		  scale_x_log10(
			limits=c(min(finished$total_time.meta,finished$total_time.norm),max(finished$total_time.meta,finished$total_time.norm)),
			labels = scales::trans_format("log10", scales::math_format(10^.x))
		) +
		  scale_y_log10(
			limits=c(min(finished$total_time.meta,finished$total_time.norm),max(finished$total_time.meta,finished$total_time.norm)),
			labels = scales::trans_format("log10", scales::math_format(10^.x))
		) +
		ggtitle("Total Time") + 
		labs(shape = "", color = "") +
		xlab(data2Name) +
		ylab(data1Name) + 
		theme(text = element_text(size=15, family="serif"),
			axis.text.x = element_text(angle=90, hjust=1),
			legend.position="bottom"
		)
	# plot
	ggsave(plot=plot, filename=paste(data2Name, "totalTime.pdf"), width=imgWidth, height=imgHeight)

	print("Generating: Coverage plot")
	# Generate Coverage plot
	metaSearchTime <- lapply(list(finished$total_time.meta), sort)[[1]]
	normSearchTime <- lapply(list(finished$total_time.norm), sort)[[1]]
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
		geom_line(data=metaCoverageData, aes(y=coverage,x= time,colour=data2Name)) +
		geom_line(data=normCoverageData, aes(y=coverage,x= time,colour=data1Name)) +
		  scale_x_log10(
			limits=c(min(finished$total_time.meta,finished$total_time.norm),max(finished$total_time.meta,finished$total_time.norm)),
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
	ggsave(plot=plot, filename=paste(data2Name, "coverage.pdf"), width=imgWidth, height=imgHeight)
}

