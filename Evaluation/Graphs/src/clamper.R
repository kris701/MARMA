max_unsolved <- function(data, target) {
	highest <- max(data[,target], na.rm=TRUE)
	data[data$solved == "false",][,target] <- highest
	return (data)
}

min_unsolved <- function(data, target) {
	lowest <- min(data[,target], na.rm=TRUE)
	data[data$solved == "false",][,target] <- lowest
	return (data)
}