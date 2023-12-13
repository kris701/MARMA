recon_names <- function(name) { 
  	if (name == "fast_downward") return ("FD")
  	if (name == "fast_downward_meta") return ("FDM")
  	if (name == "meta_no_cache") return ("FDR")
  	if (name == "meta_hashed") return ("MARMA(H)")
  	if (name == "meta_exact") return ("MARMA")
  	if (name == "meta_exact_iterative") return ("MARMA(it)")
  	if (name == "meta_exact_iterative_no_cache") return ("MARMA(it. only)")
	return (name)
}

rename_data <- function(data) { 
	data[data=="fast_downward"] <- "FD"
	names(data)[names(data)=="fast_downward"] <- "FD"
	data[data=="fast_downward_meta"] <- "FDM"
	names(data)[names(data)=="fast_downward_meta"] <- "FDM"
	data[data=="meta_no_cache"] <- "FDR"
	names(data)[names(data)=="meta_no_cache"] <- "FDR"
	data[data=="meta_hashed"] <- "MARMA(H)"
	names(data)[names(data)=="meta_hashed"] <- "MARMA(H)"
	data[data=="meta_exact"] <- "MARMA"
	names(data)[names(data)=="meta_exact"] <- "MARMA"
	data[data=="meta_exact_iterative"] <- "MARMA(it)"
	names(data)[names(data)=="meta_exact_iterative"] <- "MARMA(it)"
	data[data=="meta_exact_iterative_no_cache"] <- "MARMA(it. only)"
	names(data)[names(data)=="meta_exact_iterative_no_cache"] <- "MARMA(it. only)"
	return (data)
}

