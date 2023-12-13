library(dplyr) 
library(grid)
library(gridExtra)
library(gtable)
library(xtable)

source("src/style.R")

# Handle arguments
args = commandArgs(trailingOnly=TRUE)
#args[1] <- "trainResults.csv"
if (length(args) != 1) {
  stop("No results file given in arguments!", call.=FALSE)
}
dir.create(file.path("out"), showWarnings = FALSE)

# Read and Prepare data
data <- read.csv(args[1])
names(data) <- c(
	"$Domain$", 
	"$P_{train}$", 
	"$P_{test}$",
	"$Macro_{total}$",
	"$Meta_{total}$",
	"$Meta_{valid}$",
	"$Meta_{repl}$",
	"$Timed Out?$"
)
data[nrow(data) + 1,] <- list(
	"Total", 
	sum(data[2]), 
	sum(data[3]),
	sum(data[4]),
	sum(data[5]),
	sum(data[6]),
	sum(data[7]),
	"-"
)

# Generate table
pdf("out/trainResult.pdf", width = 10, height = 10)
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

data <- data %>% select(-contains('Macro_{total}'))
data <- data %>% select(-contains('Timed Out?'))

table <- xtable(
		data, 
		type = "latex", 
		caption="Output of the training stage. $P_{train}$ is the amount of training problems was used. $P_{test}$ is the amount of problems to test on. $Meta_{total}$ is the total amount of meta actions that was found. $Meta_{valid}$ is the amount of meta actions that was valid and $Meta_{repl}$ is the total amount of replacements that was found for the valid meta actions.",
		label="table:train"
	)
hlines <- c(-1, 0, nrow(table) - 1, nrow(table))
align(table ) <- "|0|X|X|X|X|X|X|"
bold <- function(x){
	paste0('{\\textbf{ ', x, '}}')
}
print(table, 
	file = "out/trainResult.tex", 
	include.rownames=FALSE,
	tabular.environment = "tabularx",
	width = "\\textwidth / 2",
	hline.after = hlines,
	sanitize.text.function = function(x) {x},
	latex.environments="centering",
	sanitize.colnames.function = bold,
	floating = TRUE)