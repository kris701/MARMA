library(ggplot2)

source("./src/style.R")

generate_dounotplot <- function(size1, name1, size2, name2, total, title, outName) {
	plot <- ggplot() + 
		geom_col(aes(x = 2, y = total), fill = "gray", color = "black") + 
		geom_col(aes(x = 2, y = size1, fill = name1), color = "black") + 
		geom_col(aes(x = 3, y = total), fill = "gray", color = "black") + 
		geom_col(aes(x = 3, y = size2, fill = name2), color = "black") +
		xlim(0, 3.5) + labs(x = NULL, y = NULL) + 
		ggtitle(title) + 
		labs(fill = "", color = "") +
		theme(text = element_text(size=fontSize, family=fontFamily),
			axis.ticks=element_blank(),
			axis.text.y=element_blank(),
			axis.title=element_blank(),
			legend.position="bottom") +
		coord_polar(theta = "y") 
	ggsave(plot=plot, filename=outName, width=imgWidth, height=imgHeight)
	return (plot)
}