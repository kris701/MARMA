(define
	(domain gripper-strips)

	(:types
		room
		ball
		gripper
	)

	(:predicates
		(at-robby ?r - room)
		(at ?b - ball ?r - room)
		(free ?g - gripper)
		(carry ?o - ball ?g - gripper)
		(leader-state-at-robby ?r - room)
		(leader-state-at ?b - ball ?r - room)
		(leader-state-free ?g - gripper)
		(leader-state-carry ?o - ball ?g - gripper)
		(is-goal-at-robby ?r - room)
		(is-goal-at ?b - ball ?r - room)
		(is-goal-free ?g - gripper)
		(is-goal-carry ?o - ball ?g - gripper)
		(leader-turn)
	)

	(:action fix_move
		:parameters ( ?from - room ?to - room)
		:precondition 
			(and
				(leader-state-at-robby ?from)
				(leader-turn)
			)
		:effect 
			(and
				(leader-state-at-robby ?to)
				(not
					(leader-state-at-robby ?from)
				)
				(at-robby ?to)
				(not
					(at-robby ?from)
				)
			)
	)

	(:action attack_move_0
		:parameters ( ?from - room ?to - room)
		:precondition 
			(and
				(at-robby ?from)
				(not
					(leader-turn)
				)
				(leader-state-at-robby ?to)
				(leader-state-at-robby ?from)
			)
		:effect 
			(and
				(at-robby ?to)
				(not
					(at-robby ?from)
				)
				(is-goal-at-robby ?to)
				(not
					(is-goal-at-robby ?from)
				)
			)
	)

	(:action attack_move_1
		:parameters ( ?from - room ?to - room)
		:precondition 
			(and
				(at-robby ?from)
				(not
					(leader-turn)
				)
				(leader-state-at-robby ?to)
				(not
					(leader-state-at-robby ?from)
				)
			)
		:effect 
			(and
				(at-robby ?to)
				(not
					(at-robby ?from)
				)
				(is-goal-at-robby ?to)
				(is-goal-at-robby ?from)
			)
	)

	(:action attack_move_2
		:parameters ( ?from - room ?to - room)
		:precondition 
			(and
				(at-robby ?from)
				(not
					(leader-turn)
				)
				(not
					(leader-state-at-robby ?to)
				)
				(leader-state-at-robby ?from)
			)
		:effect 
			(and
				(at-robby ?to)
				(not
					(at-robby ?from)
				)
				(not
					(is-goal-at-robby ?to)
				)
				(not
					(is-goal-at-robby ?from)
				)
			)
	)

	(:action attack_move_3
		:parameters ( ?from - room ?to - room)
		:precondition 
			(and
				(at-robby ?from)
				(not
					(leader-turn)
				)
				(not
					(leader-state-at-robby ?to)
				)
				(not
					(leader-state-at-robby ?from)
				)
			)
		:effect 
			(and
				(at-robby ?to)
				(not
					(at-robby ?from)
				)
				(not
					(is-goal-at-robby ?to)
				)
				(is-goal-at-robby ?from)
			)
	)

	(:action fix_pick
		:parameters ( ?obj - ball ?room - room ?gripper - gripper)
		:precondition 
			(and
				(leader-state-at ?obj ?room)
				(leader-state-at-robby ?room)
				(leader-state-free ?gripper)
				(leader-turn)
			)
		:effect 
			(and
				(leader-state-carry ?obj ?gripper)
				(not
					(leader-state-at ?obj ?room)
				)
				(not
					(leader-state-free ?gripper)
				)
				(carry ?obj ?gripper)
				(not
					(at ?obj ?room)
				)
				(not
					(free ?gripper)
				)
			)
	)

	(:action attack_pick_0
		:parameters ( ?obj - ball ?room - room ?gripper - gripper)
		:precondition 
			(and
				(at ?obj ?room)
				(at-robby ?room)
				(free ?gripper)
				(not
					(leader-turn)
				)
				(leader-state-carry ?obj ?gripper)
				(leader-state-at ?obj ?room)
				(leader-state-free ?gripper)
			)
		:effect 
			(and
				(carry ?obj ?gripper)
				(not
					(at ?obj ?room)
				)
				(not
					(free ?gripper)
				)
				(is-goal-carry ?obj ?gripper)
				(not
					(is-goal-at ?obj ?room)
				)
				(not
					(is-goal-free ?gripper)
				)
			)
	)

	(:action attack_pick_1
		:parameters ( ?obj - ball ?room - room ?gripper - gripper)
		:precondition 
			(and
				(at ?obj ?room)
				(at-robby ?room)
				(free ?gripper)
				(not
					(leader-turn)
				)
				(leader-state-carry ?obj ?gripper)
				(leader-state-at ?obj ?room)
				(not
					(leader-state-free ?gripper)
				)
			)
		:effect 
			(and
				(carry ?obj ?gripper)
				(not
					(at ?obj ?room)
				)
				(not
					(free ?gripper)
				)
				(is-goal-carry ?obj ?gripper)
				(not
					(is-goal-at ?obj ?room)
				)
				(is-goal-free ?gripper)
			)
	)

	(:action attack_pick_2
		:parameters ( ?obj - ball ?room - room ?gripper - gripper)
		:precondition 
			(and
				(at ?obj ?room)
				(at-robby ?room)
				(free ?gripper)
				(not
					(leader-turn)
				)
				(leader-state-carry ?obj ?gripper)
				(not
					(leader-state-at ?obj ?room)
				)
				(leader-state-free ?gripper)
			)
		:effect 
			(and
				(carry ?obj ?gripper)
				(not
					(at ?obj ?room)
				)
				(not
					(free ?gripper)
				)
				(is-goal-carry ?obj ?gripper)
				(is-goal-at ?obj ?room)
				(not
					(is-goal-free ?gripper)
				)
			)
	)

	(:action attack_pick_3
		:parameters ( ?obj - ball ?room - room ?gripper - gripper)
		:precondition 
			(and
				(at ?obj ?room)
				(at-robby ?room)
				(free ?gripper)
				(not
					(leader-turn)
				)
				(leader-state-carry ?obj ?gripper)
				(not
					(leader-state-at ?obj ?room)
				)
				(not
					(leader-state-free ?gripper)
				)
			)
		:effect 
			(and
				(carry ?obj ?gripper)
				(not
					(at ?obj ?room)
				)
				(not
					(free ?gripper)
				)
				(is-goal-carry ?obj ?gripper)
				(is-goal-at ?obj ?room)
				(is-goal-free ?gripper)
			)
	)

	(:action attack_pick_4
		:parameters ( ?obj - ball ?room - room ?gripper - gripper)
		:precondition 
			(and
				(at ?obj ?room)
				(at-robby ?room)
				(free ?gripper)
				(not
					(leader-turn)
				)
				(not
					(leader-state-carry ?obj ?gripper)
				)
				(leader-state-at ?obj ?room)
				(leader-state-free ?gripper)
			)
		:effect 
			(and
				(carry ?obj ?gripper)
				(not
					(at ?obj ?room)
				)
				(not
					(free ?gripper)
				)
				(not
					(is-goal-carry ?obj ?gripper)
				)
				(not
					(is-goal-at ?obj ?room)
				)
				(not
					(is-goal-free ?gripper)
				)
			)
	)

	(:action attack_pick_5
		:parameters ( ?obj - ball ?room - room ?gripper - gripper)
		:precondition 
			(and
				(at ?obj ?room)
				(at-robby ?room)
				(free ?gripper)
				(not
					(leader-turn)
				)
				(not
					(leader-state-carry ?obj ?gripper)
				)
				(leader-state-at ?obj ?room)
				(not
					(leader-state-free ?gripper)
				)
			)
		:effect 
			(and
				(carry ?obj ?gripper)
				(not
					(at ?obj ?room)
				)
				(not
					(free ?gripper)
				)
				(not
					(is-goal-carry ?obj ?gripper)
				)
				(not
					(is-goal-at ?obj ?room)
				)
				(is-goal-free ?gripper)
			)
	)

	(:action attack_pick_6
		:parameters ( ?obj - ball ?room - room ?gripper - gripper)
		:precondition 
			(and
				(at ?obj ?room)
				(at-robby ?room)
				(free ?gripper)
				(not
					(leader-turn)
				)
				(not
					(leader-state-carry ?obj ?gripper)
				)
				(not
					(leader-state-at ?obj ?room)
				)
				(leader-state-free ?gripper)
			)
		:effect 
			(and
				(carry ?obj ?gripper)
				(not
					(at ?obj ?room)
				)
				(not
					(free ?gripper)
				)
				(not
					(is-goal-carry ?obj ?gripper)
				)
				(is-goal-at ?obj ?room)
				(not
					(is-goal-free ?gripper)
				)
			)
	)

	(:action attack_pick_7
		:parameters ( ?obj - ball ?room - room ?gripper - gripper)
		:precondition 
			(and
				(at ?obj ?room)
				(at-robby ?room)
				(free ?gripper)
				(not
					(leader-turn)
				)
				(not
					(leader-state-carry ?obj ?gripper)
				)
				(not
					(leader-state-at ?obj ?room)
				)
				(not
					(leader-state-free ?gripper)
				)
			)
		:effect 
			(and
				(carry ?obj ?gripper)
				(not
					(at ?obj ?room)
				)
				(not
					(free ?gripper)
				)
				(not
					(is-goal-carry ?obj ?gripper)
				)
				(is-goal-at ?obj ?room)
				(is-goal-free ?gripper)
			)
	)

	(:action fix_drop
		:parameters ( ?obj - ball ?room - room ?gripper - gripper)
		:precondition 
			(and
				(leader-state-carry ?obj ?gripper)
				(leader-state-at-robby ?room)
				(leader-turn)
			)
		:effect 
			(and
				(leader-state-at ?obj ?room)
				(leader-state-free ?gripper)
				(not
					(leader-state-carry ?obj ?gripper)
				)
				(at ?obj ?room)
				(free ?gripper)
				(not
					(carry ?obj ?gripper)
				)
			)
	)

	(:action attack_drop_0
		:parameters ( ?obj - ball ?room - room ?gripper - gripper)
		:precondition 
			(and
				(carry ?obj ?gripper)
				(at-robby ?room)
				(not
					(leader-turn)
				)
				(leader-state-at ?obj ?room)
				(leader-state-free ?gripper)
				(leader-state-carry ?obj ?gripper)
			)
		:effect 
			(and
				(at ?obj ?room)
				(free ?gripper)
				(not
					(carry ?obj ?gripper)
				)
				(is-goal-at ?obj ?room)
				(is-goal-free ?gripper)
				(not
					(is-goal-carry ?obj ?gripper)
				)
			)
	)

	(:action attack_drop_1
		:parameters ( ?obj - ball ?room - room ?gripper - gripper)
		:precondition 
			(and
				(carry ?obj ?gripper)
				(at-robby ?room)
				(not
					(leader-turn)
				)
				(leader-state-at ?obj ?room)
				(leader-state-free ?gripper)
				(not
					(leader-state-carry ?obj ?gripper)
				)
			)
		:effect 
			(and
				(at ?obj ?room)
				(free ?gripper)
				(not
					(carry ?obj ?gripper)
				)
				(is-goal-at ?obj ?room)
				(is-goal-free ?gripper)
				(is-goal-carry ?obj ?gripper)
			)
	)

	(:action attack_drop_2
		:parameters ( ?obj - ball ?room - room ?gripper - gripper)
		:precondition 
			(and
				(carry ?obj ?gripper)
				(at-robby ?room)
				(not
					(leader-turn)
				)
				(leader-state-at ?obj ?room)
				(not
					(leader-state-free ?gripper)
				)
				(leader-state-carry ?obj ?gripper)
			)
		:effect 
			(and
				(at ?obj ?room)
				(free ?gripper)
				(not
					(carry ?obj ?gripper)
				)
				(is-goal-at ?obj ?room)
				(not
					(is-goal-free ?gripper)
				)
				(not
					(is-goal-carry ?obj ?gripper)
				)
			)
	)

	(:action attack_drop_3
		:parameters ( ?obj - ball ?room - room ?gripper - gripper)
		:precondition 
			(and
				(carry ?obj ?gripper)
				(at-robby ?room)
				(not
					(leader-turn)
				)
				(leader-state-at ?obj ?room)
				(not
					(leader-state-free ?gripper)
				)
				(not
					(leader-state-carry ?obj ?gripper)
				)
			)
		:effect 
			(and
				(at ?obj ?room)
				(free ?gripper)
				(not
					(carry ?obj ?gripper)
				)
				(is-goal-at ?obj ?room)
				(not
					(is-goal-free ?gripper)
				)
				(is-goal-carry ?obj ?gripper)
			)
	)

	(:action attack_drop_4
		:parameters ( ?obj - ball ?room - room ?gripper - gripper)
		:precondition 
			(and
				(carry ?obj ?gripper)
				(at-robby ?room)
				(not
					(leader-turn)
				)
				(not
					(leader-state-at ?obj ?room)
				)
				(leader-state-free ?gripper)
				(leader-state-carry ?obj ?gripper)
			)
		:effect 
			(and
				(at ?obj ?room)
				(free ?gripper)
				(not
					(carry ?obj ?gripper)
				)
				(not
					(is-goal-at ?obj ?room)
				)
				(is-goal-free ?gripper)
				(not
					(is-goal-carry ?obj ?gripper)
				)
			)
	)

	(:action attack_drop_5
		:parameters ( ?obj - ball ?room - room ?gripper - gripper)
		:precondition 
			(and
				(carry ?obj ?gripper)
				(at-robby ?room)
				(not
					(leader-turn)
				)
				(not
					(leader-state-at ?obj ?room)
				)
				(leader-state-free ?gripper)
				(not
					(leader-state-carry ?obj ?gripper)
				)
			)
		:effect 
			(and
				(at ?obj ?room)
				(free ?gripper)
				(not
					(carry ?obj ?gripper)
				)
				(not
					(is-goal-at ?obj ?room)
				)
				(is-goal-free ?gripper)
				(is-goal-carry ?obj ?gripper)
			)
	)

	(:action attack_drop_6
		:parameters ( ?obj - ball ?room - room ?gripper - gripper)
		:precondition 
			(and
				(carry ?obj ?gripper)
				(at-robby ?room)
				(not
					(leader-turn)
				)
				(not
					(leader-state-at ?obj ?room)
				)
				(not
					(leader-state-free ?gripper)
				)
				(leader-state-carry ?obj ?gripper)
			)
		:effect 
			(and
				(at ?obj ?room)
				(free ?gripper)
				(not
					(carry ?obj ?gripper)
				)
				(not
					(is-goal-at ?obj ?room)
				)
				(not
					(is-goal-free ?gripper)
				)
				(not
					(is-goal-carry ?obj ?gripper)
				)
			)
	)

	(:action attack_drop_7
		:parameters ( ?obj - ball ?room - room ?gripper - gripper)
		:precondition 
			(and
				(carry ?obj ?gripper)
				(at-robby ?room)
				(not
					(leader-turn)
				)
				(not
					(leader-state-at ?obj ?room)
				)
				(not
					(leader-state-free ?gripper)
				)
				(not
					(leader-state-carry ?obj ?gripper)
				)
			)
		:effect 
			(and
				(at ?obj ?room)
				(free ?gripper)
				(not
					(carry ?obj ?gripper)
				)
				(not
					(is-goal-at ?obj ?room)
				)
				(not
					(is-goal-free ?gripper)
				)
				(is-goal-carry ?obj ?gripper)
			)
	)

	(:action fix_meta_$tele-pick
		:parameters ( ?obj - ball ?room - room ?gripper - gripper)
		:precondition 
			(and
				(leader-state-at ?obj ?room)
				(leader-state-free ?gripper)
				(leader-turn)
			)
		:effect 
			(and
				(leader-state-carry ?obj ?gripper)
				(not
					(is-goal-carry ?obj ?gripper)
				)
				(not
					(leader-state-at ?obj ?room)
				)
				(not
					(is-goal-at ?obj ?room)
				)
				(not
					(leader-state-free ?gripper)
				)
				(not
					(is-goal-free ?gripper)
				)
				(not
					(leader-turn)
				)
			)
	)

)
