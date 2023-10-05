(:action meta-pick
       :parameters (?obj - ball ?room - room ?gripper - gripper)
       :precondition  (and 
			    (at ?obj ?room) (at-robby ?room))
       :effect (and (carry ?obj ?gripper)
		    (not (at ?obj ?room))))