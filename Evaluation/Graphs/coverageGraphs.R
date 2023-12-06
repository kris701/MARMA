library(xtable)

source("src/latexTableHelpers.R")
source("src/graphNames.R")

# Handle arguments
args = commandArgs(trailingOnly=TRUE)
if (length(args) != 1) {
  stop("No results file given in arguments!", call.=FALSE)
}
dir.create(file.path("out"), showWarnings = FALSE)
data <- read.csv(args[1])
data <- rename_data(data)

tableData <- data.frame(domain=character(), Problems = integer())



for(domain in unique(data$domain))
	tableData[nrow(tableData) + 1,] <- c(domain, length(unique(data[data$domain == domain,]$problem)))

for(name in unique(data$name)){
	columnData <- c()
	for(domain in unique(data$domain))
		columnData <- append(columnData, nrow(data[data$domain == domain & data$name == name & data$solved == "true",]))
	tableData[name] <- columnData
}

totalRow <- list("Total")
for(i in 2:ncol(tableData))
	totalRow <- append(totalRow, sum(sapply(tableData[i], as.integer)))
tableData[nrow(tableData) + 1,] <- totalRow 

names(tableData)[names(tableData)=="domain"] <- "Domain"

table <- xtable(
		tableData, 
		type = "latex", 
		caption="Coverage of how many problems was solved for each domain.",
		label="table:domainCoverage"
	)
align(table) <- generateRowDefinition(ncol(tableData), TRUE)
print(table, 
	file = "out/coverageTable.tex", 
	include.rownames=FALSE,
	tabular.environment = "tabularx",
	width = "\\textwidth / 2",
	hline.after = topRowBottomRowLines(nrow(table)),
	sanitize.text.function = function(x) {x},
	latex.environments="centering",
	sanitize.colnames.function = bold,
	floating = TRUE,
	rotate.colnames = TRUE)
