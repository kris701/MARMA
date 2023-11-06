(:action up#depart#down
  :parameters (?f1 - floor ?f2 - floor ?p - passenger)
  :precondition 
  (and 
    (lift-at ?f1) 
    (above ?f1 ?f2)
    (destin ?p ?f2)
    (boarded ?p)
  )
  :effect 
  (and 
    (not (boarded ?p))
    (served ?p)
  )
)
