recon_names <- function(name) { 
  	if (name == "fast_downward") return ("FD")
  	if (name == "fast_downward_meta") return ("FDM")
  	if (name == "meta_no_cache") return ("FDR")
  	if (name == "meta_hashed") return ("MARMA(H)")
  	if (name == "meta_lifted") return ("MARMA(L)")
  	if (name == "meta_lifted_iterative") return ("MARMA(L, itt)")
  	if (name == "meta_lifted_iterative_no_cache") return ("MARMA(L, itt. only)")
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
	data[data=="meta_lifted"] <- "MARMA(L)"
	names(data)[names(data)=="meta_lifted"] <- "MARMA(L)"
	data[data=="meta_lifted_iterative"] <- "MARMA(L, itt)"
	names(data)[names(data)=="meta_lifted_iterative"] <- "MARMA(L, itt)"
	data[data=="meta_lifted_iterative_no_cache"] <- "MARMA(L, itt. only)"
	names(data)[names(data)=="meta_lifted_iterative_no_cache"] <- "MARMA(L, itt. only)"
	return (data)
}

