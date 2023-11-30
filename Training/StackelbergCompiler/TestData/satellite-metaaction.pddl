(:action $switch-on-calibrate 
     :parameters (?s - satellite ?i - instrument)
     :precondition (and 
                        (on_board ?i ?s)
                        (power_avail ?s)
                   )
     :effect (and 
                (calibrated ?i)
                (power_on ?i)
                (not (power_avail ?s))
            )
     )