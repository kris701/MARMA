(:action down#board#up
  :parameters (?f1 - floor ?f2 - floor ?p - passenger)
  :precondition 
  (and 
    (lift-at ?f2) 
    (above ?f1 ?f2)
    (origin ?p ?f1)
  )
  :effect 
  (and 
    (boarded ?p)
    (not (origin ?p ?f1))
  )
)
