library(grid)
library(gridExtra)
library(gtable)

generate_table <- function(data, outName, width, height, generateTotal) {
	innerData <- data
	if (generateTotal == TRUE){
		totalData = list("Total")
		for(c in 2:ncol(data))
			totalData <- append(totalData, sum(data[c], na.rm=TRUE))
		innerData[nrow(innerData) + 1,] <- totalData
	}

	pdf(outName, width = width, height = height)
	g <- tableGrob(innerData, rows=NULL, theme=ttheme_minimal(base_size = fontSize, family = fontFamily))
	g <- gtable_add_grob(g,
        	grobs = rectGrob(gp = gpar(fill = NA, lwd = 2)),
        	t = 1, l = 1, r = ncol(g))
	if (generateTotal == TRUE){
		g <- gtable_add_grob(g,
        		grobs = rectGrob(gp = gpar(fill = NA, lwd = 2)),
        		t = 2, b = nrow(g) - 1, l = 1, r = ncol(g))
		g <- gtable_add_grob(g,
        		grobs = rectGrob(gp = gpar(fill = NA, lwd = 2)),
      		t = nrow(g), l = 1, r = ncol(g))
	}
	else
	{
		g <- gtable_add_grob(g,
        		grobs = rectGrob(gp = gpar(fill = NA, lwd = 2)),
        		t = 2, b = nrow(g), l = 1, r = ncol(g))
	}
	grid.draw(g)

	dev.off()
	return ()
}