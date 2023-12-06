#library(dplyr) 
library(xtable)

source("src/style.R")
source("src/graphNames.R")
source("src/latexTableHelpers.R")

# Handle arguments
args = commandArgs(trailingOnly=TRUE)
if (length(args) != 1) {
  stop("No results file given in arguments!", call.=FALSE)
}
dir.create(file.path("out"), showWarnings = FALSE)

data <- read.csv(args[1])
data <- data[,-ncol(data)]
data <- rename_data(data)
names(data)[names(data)=="domain"] <- "Domain"

totalRow <- list("Total")
for(i in 2:ncol(data))
	totalRow <- append(totalRow, sum(sapply(data[i], as.numeric)))
data[nrow(data) + 1,] <- totalRow 

table <- xtable(
		data, 
		type = "latex", 
		caption="IPC Score for all the methods",
		label="table:ipcScore"
	)
align(table ) <- generateRowDefinition(ncol(table), TRUE)
print(table, 
	file = "out/ipcScore.tex", 
	include.rownames=FALSE,
	tabular.environment = "tabularx",
	width = "\\textwidth / 2",
	hline.after = topRowBottomRowLines(nrow(data)),
	sanitize.text.function = function(x) {x},
	latex.environments="centering",
	sanitize.colnames.function = bold,
	floating = TRUE,
	rotate.colnames = TRUE)