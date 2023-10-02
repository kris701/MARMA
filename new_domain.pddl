(define
	(domain gripper-strips)

	(:predicates
		(room ?r)
		(ball ?b)
		(gripper ?g)
		(at-robby ?r)
		(at ?b ?r)
		(free ?g)
		(carry ?o ?g)
		(leader-state-room ?r)
		(leader-state-ball ?b)
		(leader-state-gripper ?g)
		(leader-state-at-robby ?r)
		(leader-state-at ?b ?r)
		(leader-state-free ?g)
		(leader-state-carry ?o ?g)
		(is-goal-room ?r)
		(is-goal-ball ?b)
		(is-goal-gripper ?g)
		(is-goal-at-robby ?r)
		(is-goal-at ?b ?r)
		(is-goal-free ?g)
		(is-goal-carry ?o ?g)
	)

	(:action attack_move
		:parameters ( ?from ?to)
		:precondition 
			(and
				(leader-state-room ?from)
				(leader-state-room ?to)
				(leader-state-at-robby ?from)
			)
		:effect 
			(and
				(leader-state-at-robby ?to)
				(not
					(leader-state-at-robby ?from)
				)
			)
	)

	(:action fix_move
		:parameters ( ?from ?to)
		:precondition 
			(and
				(room ?from)
				(room ?to)
				(at-robby ?from)
			)
		:effect 
			(and
				(at-robby ?to)
				(not
					(at-robby ?from)
				)
				(when
					(leader-state-at-robby ?to)
					(is-goal-at-robby ?to)
				)

				(when
					(not
						(leader-state-at-robby ?from)
					)
					(is-goal-at-robby ?from)
				)

			)
	)

	(:action attack_pick
		:parameters ( ?obj ?room ?gripper)
		:precondition 
			(and
				(leader-state-ball ?obj)
				(leader-state-room ?room)
				(leader-state-gripper ?gripper)
				(leader-state-at ?obj ?room)
				(leader-state-at-robby ?room)
				(leader-state-free ?gripper)
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
			)
	)

	(:action fix_pick
		:parameters ( ?obj ?room ?gripper)
		:precondition 
			(and
				(ball ?obj)
				(room ?room)
				(gripper ?gripper)
				(at ?obj ?room)
				(at-robby ?room)
				(free ?gripper)
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
				(when
					(leader-state-carry ?obj ?gripper)
					(is-goal-carry ?obj ?gripper)
				)

				(when
					(not
						(leader-state-at ?obj ?room)
					)
					(is-goal-at ?obj ?room)
				)

				(when
					(not
						(leader-state-free ?gripper)
					)
					(is-goal-free ?gripper)
				)

			)
	)

	(:action attack_drop
		:parameters ( ?obj ?room ?gripper)
		:precondition 
			(and
				(leader-state-ball ?obj)
				(leader-state-room ?room)
				(leader-state-gripper ?gripper)
				(leader-state-carry ?obj ?gripper)
				(leader-state-at-robby ?room)
			)
		:effect 
			(and
				(leader-state-at ?obj ?room)
				(leader-state-free ?gripper)
				(not
					(leader-state-carry ?obj ?gripper)
				)
			)
	)

	(:action fix_drop
		:parameters ( ?obj ?room ?gripper)
		:precondition 
			(and
				(ball ?obj)
				(room ?room)
				(gripper ?gripper)
				(carry ?obj ?gripper)
				(at-robby ?room)
			)
		:effect 
			(and
				(at ?obj ?room)
				(free ?gripper)
				(not
					(carry ?obj ?gripper)
				)
				(when
					(leader-state-at ?obj ?room)
					(is-goal-at ?obj ?room)
				)

				(when
					(leader-state-free ?gripper)
					(is-goal-free ?gripper)
				)

				(when
					(not
						(leader-state-carry ?obj ?gripper)
					)
					(is-goal-carry ?obj ?gripper)
				)

			)
	)

)
