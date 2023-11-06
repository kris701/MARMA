(:action down#depart#up
  :parameters (?f1 - floor ?f2 - floor ?p - passenger)
  :precondition 
  (and 
    (lift-at ?f2) 
    (above ?f1 ?f2)
    (destin ?p ?f1)
    (boarded ?p)
  )
  :effect 
  (and 
    (not (boarded ?p))
    (served ?p)
  )
)
