(:action $tele-pick
       :parameters (?obj - ball ?room - room ?gripper - gripper)
       :precondition  (and 
                (at ?obj ?room) (free ?gripper))
       :effect (and (carry ?obj ?gripper)
            (not (at ?obj ?room)) 
            (not (free ?gripper)))


            )