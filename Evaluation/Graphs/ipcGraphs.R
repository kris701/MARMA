library(dplyr) 
library(ggplot2)

source("src/style.R")
source("src/graphNames.R")

# Handle arguments
args = commandArgs(trailingOnly=TRUE)
if (length(args) != 1) {
  stop("No results file given in arguments!", call.=FALSE)
}
dir.create(file.path("out"), showWarnings = FALSE)

data <- read.csv(args[1], header = T, sep = ",", colClasses=c('character','numeric'))

plot <- ggplot(data, aes(x = reorder(name, -score), y = score, fill = name)) + 
	geom_col() + 
	ggtitle("IPC Score") + 
	labs(fill = "", color = "") +
	theme(text = element_text(size=15, family="serif"),
		axis.ticks=element_blank(),
		axis.text.x = element_text(angle=20, hjust=1),
		axis.title=element_blank(),
		legend.position="none")
plot
ggsave(plot=plot, filename=paste("out/", "ipcScore.pdf", sep=""), width=imgWidth, height=imgHeight)
