library(dplyr) 
library(ggplot2)

source("style.R")

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
		theme(text = element_text(size=fontSize, family=fontFamily),
			axis.text.x = element_text(angle=90, hjust=1),
			legend.position="bottom"
		)
	ggsave(plot=plot, filename=outName, width=imgWidth, height=imgHeight)
	return (plot)
}