recon_names <- function(name) { 
  	if (name == "fast_downward") return ("Fast Downward")
  	if (name == "fast_downward_meta") return ("Fast Downward (Meta)")
  	if (name == "meta_no_cache") return ("FD Reconstruction")
  	if (name == "meta_hashed") return ("MARMA (Hashed)")
  	if (name == "meta_lifted") return ("MARMA (Lifted)")
  	if (name == "meta_lifted_iterative") return ("MARMA (Lifted, it)")
  	if (name == "meta_lifted_iterative_no_cache") return ("MARMA (Lifted, itt. only)")
	return (name)
}

rename_data <- function(data) { 
	data[data=="fast_downward"] <- "Fast Downward"
	data[data=="fast_downward_meta"] <- "Fast Downward (Meta)"
	data[data=="meta_no_cache"] <- "FD Reconstruction"
	data[data=="meta_hashed"] <- "MARMA (Hashed)"
	data[data=="meta_lifted"] <- "MARMA (Lifted)"
	data[data=="meta_lifted_iterative"] <- "MARMA (Lifted, it)"
	data[data=="meta_lifted_iterative_no_cache"] <- "MARMA (Lifted, itt. only)"
	return (data)
}

