(:action unstack_mcr_stack
    :parameters ( ?ob - object ?underob - object ?underobx2 - object )
    :precondition (and (on ?ob ?underob) (clear ?ob) (arm-empty) (clear ?underobx2))
    :effect (and (clear ?underob) (arm-empty) (clear ?ob) (on ?ob ?underobx2) (not (on ?ob ?underob)) (not (clear ?underobx2)) (not (holding ?ob)))
)