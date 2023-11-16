recon_names <- function(name) { 
  	if (name == "fast_downward") return ("Fast Downward")
  	if (name == "fast_downward_meta") return ("Fast Downward (Meta)")
  	if (name == "meta_no_cache") return ("FD Reconstruction")
  	if (name == "meta_hashed") return ("MARMA (Hashed)")
  	if (name == "meta_lifted") return ("MARMA (Lifted)")
	return (name)
}

rename_data <- function(data) { 
	data[data=="fast_downward"] <- "Fast Downward"
	data[data=="fast_downward_meta"] <- "Fast Downward (Meta)"
	data[data=="meta_no_cache"] <- "FD Reconstruction"
	data[data=="meta_hashed"] <- "MARMA (Hashed)"
	data[data=="meta_lifted"] <- "MARMA (Lifted)"
	return (data)
}

