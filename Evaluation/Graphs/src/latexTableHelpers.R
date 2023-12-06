bold <- function(x){
	paste0('{\\textbf{ ', x, '}}')
}

topRowLines <- function(rows) 
	c(-1, 0, rows)
topRowBottomRowLines <- function(rows) 
	c(-1, 0, rows - 1, rows)

generateRowDefinition <- function(columnCount, ignoreFirst = FALSE) {
	source <- "|"
	if (ignoreFirst == TRUE)
		source <- "|0|"
	for (i in 1:columnCount)
		source <- paste(source, "X|", sep="")
	return (source)
}
