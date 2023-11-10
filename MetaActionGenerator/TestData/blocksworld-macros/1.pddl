(:action unstack_mcr_putdown
    :parameters ( ?ob - object ?underob - object )
    :precondition (and (on ?ob ?underob) (clear ?ob) (arm-empty))
    :effect (and (clear ?underob) (clear ?ob) (arm-empty) (on-table ?ob) (not (on ?ob ?underob)) (not (holding ?ob)))
)