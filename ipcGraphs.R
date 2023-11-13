library(dplyr) 
library(ggplot2)

imgWidth <- 8
imgHeight <- 9

data <- read.csv("results.csv", header = T, sep = ",", colClasses=c('character','numeric'))

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
ggsave(plot=plot, filename="ipcScore.pdf", width=imgWidth, height=imgHeight)
