library(grid)
library(gridExtra)
library(gtable)

source("src/style.R")

# Handle arguments
args = commandArgs(trailingOnly=TRUE)
args[1] <- "trainResults.csv"
if (length(args) != 1) {
  stop("No results file given in arguments!", call.=FALSE)
}

# Read and Prepare data
data <- read.csv(args[1])
names(data) <- c(
	"Domain", 
	"Training\n Problems", 
	"Testing\n Problems",
	"Total\n Meta Actions",
	"Valid\n Meta Actions",
	"Total\n Replacements",
	"Timed Out?"
)
data[nrow(data) + 1,] <- list(
	"Total", 
	sum(data[2]), 
	sum(data[3]),
	sum(data[4]),
	sum(data[5]),
	sum(data[6]),
	"-"
)

# Generate table
pdf("trainResult.pdf", width = 6.7, height = 5.7)
g <- tableGrob(data, rows=NULL, theme=ttheme_minimal(base_size = fontSize, family = fontFamily))
g <- gtable_add_grob(g,
        grobs = rectGrob(gp = gpar(fill = NA, lwd = 2)),
        t = 2, b = nrow(g) - 1, l = 1, r = ncol(g))
g <- gtable_add_grob(g,
        grobs = rectGrob(gp = gpar(fill = NA, lwd = 2)),
        t = 1, l = 1, r = ncol(g))
g <- gtable_add_grob(g,
        grobs = rectGrob(gp = gpar(fill = NA, lwd = 2)),
        t = nrow(g), l = 1, r = ncol(g))
grid.draw(g)

dev.off()