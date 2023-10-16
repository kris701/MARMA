(:action pick_mcr_move_mcr_drop
:parameters ( ?r - robot ?obj - object ?room - room ?g - gripper ?tox4 - room)
:precondition (and (at ?obj ?room)(at-robby ?r ?room)(free ?r ?g)(stai_at ?obj ?room)(stai_free ?r ?g)(stag_at ?obj ?tox4))
:effect (and (at-robby ?r ?tox4)(at ?obj ?tox4)(free ?r ?g)(not (at ?obj ?room))(not (at-robby ?r ?room))(not (carry ?r ?obj ?g)))
)