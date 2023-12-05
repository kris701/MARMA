library(dplyr) 
library(ggplot2)
library(xtable)

source("src/style.R")
source("src/graphNames.R")

# Handle arguments
args = commandArgs(trailingOnly=TRUE)
if (length(args) != 1) {
  stop("No results file given in arguments!", call.=FALSE)
}
dir.create(file.path("out"), showWarnings = FALSE)

data <- read.csv(args[1], header = T, sep = ",", colClasses=c('character','numeric'))
data <- rename_data(data)

plot <- ggplot(data, aes(x = reorder(name, -score), y = score, fill = name)) + 
	geom_col() + 
	ggtitle("IPC Score") + 
	labs(fill = "", color = "") +
	theme(text = element_text(size=15, family="serif"),
		axis.ticks=element_blank(),
		axis.text.x = element_text(angle=20, hjust=1),
		axis.title=element_blank(),
		legend.position="none")
ggsave(plot=plot, filename=paste("out/", "ipcScore.pdf", sep=""), width=imgWidth, height=imgHeight)

table <- xtable(
		data, 
		type = "latex", 
		caption="IPC Score for all the methods",
		label="table:ipcScore"
	)
names(table) <- c(
	"$Method$", 
	"$IPC Score$"
)
hlines <- c(-1, 0, nrow(table))
align(table ) <- "|0|X|X|"
bold <- function(x){
	paste0('{\\textbf{ ', x, '}}')
}
print(table, 
	file = "out/ipcScore.tex", 
	include.rownames=FALSE,
	tabular.environment = "tabularx",
	width = "\\textwidth / 2",
	hline.after = hlines,
	sanitize.text.function = function(x) {x},
	latex.environments="centering",
	sanitize.colnames.function = bold,
	floating = TRUE)