library(ggplot2)

source("style.R")

generate_domainBarPlot <- function(finished, target1, name1, target2, name2, title, outName) {
	domains <- c()
	values  <- c()
	names <- c()
	for (domain in unique(finished$domain)) {
		domains <- append(domains, domain)
		values <- append(values, sum(finished[finished$domain == domain,][,target1], na.rm = TRUE))
		names <- append(names, AName)
		domains <- append(domains, domain)
		values <- append(values, sum(finished[finished$domain == domain,][,target2], na.rm = TRUE))
		names <- append(names, BName)
	}
	transform <- data.frame(domain = domains, value = values, name = names)
	plot <- ggplot(transform, aes(x = domain, y = value, fill = name)) +
		geom_bar(stat = "identity", position = 'dodge') +
		ggtitle(title) + 
		labs(fill = "", color = "") +
		theme(text = element_text(size=fontSize, family=fontFamily),
			axis.ticks=element_blank(),
			axis.title=element_blank(),
			legend.position="bottom")
	ggsave(plot=plot, filename=outName, width=imgWidth, height=imgHeight)
	return (plot)
}