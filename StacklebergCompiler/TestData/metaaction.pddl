(:action meta-pick
       :parameters (?obj ?room)
       :precondition  (and  (ball ?obj) (room ?room)
			    (at ?obj ?room) (at-robby ?room))
       :effect (and (carry ?obj ?gripper)
		    (not (at ?obj ?room))))