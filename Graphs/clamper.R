max_unsolved <- function(data, target) {
	highest <- max(data[,target], na.rm=TRUE)
	data[data$solved == "false",][,target] <- highest
	return (data)
}