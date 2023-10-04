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
		(leader-turn)
	)

	(:action fix_move
		:parameters ( ?from ?to)
		:precondition 
			(and
				(room ?from)
				(room ?to)
				(leader-state-at-robby ?from)
				(leader-turn)
			)
		:effect 
			(and
				(leader-state-at-robby ?to)
				(not
					(leader-state-at-robby ?from)
				)
			)
	)

	(:action attack_move
		:parameters ( ?from ?to)
		:precondition 
			(and
				(room ?from)
				(room ?to)
				(at-robby ?from)
				(not
					(leader-turn)
				)
			)
		:effect 
			(and
				(at-robby ?to)
				(not
					(at-robby ?from)
				)
			)
	)

	(:action attack_move_goal
		:parameters ( ?from ?to)
		:precondition 
			(and
				(room ?from)
				(room ?to)
				(at-robby ?from)
				(not
					(leader-turn)
				)
				(leader-state-at-robby ?to)
			)
		:effect 
			(and
				(at-robby ?to)
				(not
					(at-robby ?from)
				)
				(is-goal-at-robby ?to)
			)
	)

	(:action fix_pick
		:parameters ( ?obj ?room ?gripper)
		:precondition 
			(and
				(ball ?obj)
				(room ?room)
				(gripper ?gripper)
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
			)
	)

	(:action attack_pick
		:parameters ( ?obj ?room ?gripper)
		:precondition 
			(and
				(ball ?obj)
				(room ?room)
				(gripper ?gripper)
				(at ?obj ?room)
				(at-robby ?room)
				(free ?gripper)
				(not
					(leader-turn)
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
			)
	)

	(:action attack_pick_goal
		:parameters ( ?obj ?room ?gripper)
		:precondition 
			(and
				(ball ?obj)
				(room ?room)
				(gripper ?gripper)
				(at ?obj ?room)
				(at-robby ?room)
				(free ?gripper)
				(not
					(leader-turn)
				)
				(leader-state-carry ?obj ?gripper)
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
			)
	)

	(:action fix_drop
		:parameters ( ?obj ?room ?gripper)
		:precondition 
			(and
				(ball ?obj)
				(room ?room)
				(gripper ?gripper)
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
			)
	)

	(:action attack_drop
		:parameters ( ?obj ?room ?gripper)
		:precondition 
			(and
				(ball ?obj)
				(room ?room)
				(gripper ?gripper)
				(carry ?obj ?gripper)
				(at-robby ?room)
				(not
					(leader-turn)
				)
			)
		:effect 
			(and
				(at ?obj ?room)
				(free ?gripper)
				(not
					(carry ?obj ?gripper)
				)
			)
	)

	(:action attack_drop_goal
		:parameters ( ?obj ?room ?gripper)
		:precondition 
			(and
				(ball ?obj)
				(room ?room)
				(gripper ?gripper)
				(carry ?obj ?gripper)
				(at-robby ?room)
				(not
					(leader-turn)
				)
				(leader-state-at ?obj ?room)
				(leader-state-free ?gripper)
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
			)
	)

	(:action fix_meta_meta-pick
		:parameters ( ?obj ?room ?gripper)
		:precondition 
			(and
				(ball ?obj)
				(room ?room)
				(leader-state-at ?obj ?room)
				(leader-state-at-robby ?room)
				(leader-turn)
			)
		:effect 
			(and
				(leader-state-carry ?obj ?gripper)
				(not
					(leader-state-at ?obj ?room)
				)
			)
	)

	(:action fix_pass-turn
		:parameters ()
		:precondition 
			(leader-turn)
		:effect 
			(and
				(not
					(leader-turn)
				)
				(forall ( ?r)
					(when
						(not
							(leader-state-at-robby ?r)
						)
						(is-goal-at-robby ?r)
					)

				)

				(forall ( ?b ?r)
					(when
						(not
							(leader-state-at ?b ?r)
						)
						(is-goal-at ?b ?r)
					)

				)

				(forall ( ?g)
					(when
						(not
							(leader-state-free ?g)
						)
						(is-goal-free ?g)
					)

				)

				(forall ( ?o ?g)
					(when
						(not
							(leader-state-carry ?o ?g)
						)
						(is-goal-carry ?o ?g)
					)

				)

			)
	)

	(:action attack_reach-goal
		:parameters ()
		:precondition 
			(leader-turn)
		:effect 
			(and
				(room rooma)
				(room roomb)
				(room ball4)
				(room ball3)
				(room ball2)
				(room ball1)
				(room left)
				(room right)
				(ball rooma)
				(ball roomb)
				(ball ball4)
				(ball ball3)
				(ball ball2)
				(ball ball1)
				(ball left)
				(ball right)
				(gripper rooma)
				(gripper roomb)
				(gripper ball4)
				(gripper ball3)
				(gripper ball2)
				(gripper ball1)
				(gripper left)
				(gripper right)
				(is-goal-at-robby rooma)
				(is-goal-at-robby roomb)
				(is-goal-at-robby ball4)
				(is-goal-at-robby ball3)
				(is-goal-at-robby ball2)
				(is-goal-at-robby ball1)
				(is-goal-at-robby left)
				(is-goal-at-robby right)
				(is-goal-at rooma rooma)
				(is-goal-at rooma roomb)
				(is-goal-at rooma ball4)
				(is-goal-at rooma ball3)
				(is-goal-at rooma ball2)
				(is-goal-at rooma ball1)
				(is-goal-at rooma left)
				(is-goal-at rooma right)
				(is-goal-at roomb rooma)
				(is-goal-at roomb roomb)
				(is-goal-at roomb ball4)
				(is-goal-at roomb ball3)
				(is-goal-at roomb ball2)
				(is-goal-at roomb ball1)
				(is-goal-at roomb left)
				(is-goal-at roomb right)
				(is-goal-at ball4 rooma)
				(is-goal-at ball4 roomb)
				(is-goal-at ball4 ball4)
				(is-goal-at ball4 ball3)
				(is-goal-at ball4 ball2)
				(is-goal-at ball4 ball1)
				(is-goal-at ball4 left)
				(is-goal-at ball4 right)
				(is-goal-at ball3 rooma)
				(is-goal-at ball3 roomb)
				(is-goal-at ball3 ball4)
				(is-goal-at ball3 ball3)
				(is-goal-at ball3 ball2)
				(is-goal-at ball3 ball1)
				(is-goal-at ball3 left)
				(is-goal-at ball3 right)
				(is-goal-at ball2 rooma)
				(is-goal-at ball2 roomb)
				(is-goal-at ball2 ball4)
				(is-goal-at ball2 ball3)
				(is-goal-at ball2 ball2)
				(is-goal-at ball2 ball1)
				(is-goal-at ball2 left)
				(is-goal-at ball2 right)
				(is-goal-at ball1 rooma)
				(is-goal-at ball1 roomb)
				(is-goal-at ball1 ball4)
				(is-goal-at ball1 ball3)
				(is-goal-at ball1 ball2)
				(is-goal-at ball1 ball1)
				(is-goal-at ball1 left)
				(is-goal-at ball1 right)
				(is-goal-at left rooma)
				(is-goal-at left roomb)
				(is-goal-at left ball4)
				(is-goal-at left ball3)
				(is-goal-at left ball2)
				(is-goal-at left ball1)
				(is-goal-at left left)
				(is-goal-at left right)
				(is-goal-at right rooma)
				(is-goal-at right roomb)
				(is-goal-at right ball4)
				(is-goal-at right ball3)
				(is-goal-at right ball2)
				(is-goal-at right ball1)
				(is-goal-at right left)
				(is-goal-at right right)
				(is-goal-free rooma)
				(is-goal-free roomb)
				(is-goal-free ball4)
				(is-goal-free ball3)
				(is-goal-free ball2)
				(is-goal-free ball1)
				(is-goal-free left)
				(is-goal-free right)
				(is-goal-carry rooma rooma)
				(is-goal-carry rooma roomb)
				(is-goal-carry rooma ball4)
				(is-goal-carry rooma ball3)
				(is-goal-carry rooma ball2)
				(is-goal-carry rooma ball1)
				(is-goal-carry rooma left)
				(is-goal-carry rooma right)
				(is-goal-carry roomb rooma)
				(is-goal-carry roomb roomb)
				(is-goal-carry roomb ball4)
				(is-goal-carry roomb ball3)
				(is-goal-carry roomb ball2)
				(is-goal-carry roomb ball1)
				(is-goal-carry roomb left)
				(is-goal-carry roomb right)
				(is-goal-carry ball4 rooma)
				(is-goal-carry ball4 roomb)
				(is-goal-carry ball4 ball4)
				(is-goal-carry ball4 ball3)
				(is-goal-carry ball4 ball2)
				(is-goal-carry ball4 ball1)
				(is-goal-carry ball4 left)
				(is-goal-carry ball4 right)
				(is-goal-carry ball3 rooma)
				(is-goal-carry ball3 roomb)
				(is-goal-carry ball3 ball4)
				(is-goal-carry ball3 ball3)
				(is-goal-carry ball3 ball2)
				(is-goal-carry ball3 ball1)
				(is-goal-carry ball3 left)
				(is-goal-carry ball3 right)
				(is-goal-carry ball2 rooma)
				(is-goal-carry ball2 roomb)
				(is-goal-carry ball2 ball4)
				(is-goal-carry ball2 ball3)
				(is-goal-carry ball2 ball2)
				(is-goal-carry ball2 ball1)
				(is-goal-carry ball2 left)
				(is-goal-carry ball2 right)
				(is-goal-carry ball1 rooma)
				(is-goal-carry ball1 roomb)
				(is-goal-carry ball1 ball4)
				(is-goal-carry ball1 ball3)
				(is-goal-carry ball1 ball2)
				(is-goal-carry ball1 ball1)
				(is-goal-carry ball1 left)
				(is-goal-carry ball1 right)
				(is-goal-carry left rooma)
				(is-goal-carry left roomb)
				(is-goal-carry left ball4)
				(is-goal-carry left ball3)
				(is-goal-carry left ball2)
				(is-goal-carry left ball1)
				(is-goal-carry left left)
				(is-goal-carry left right)
				(is-goal-carry right rooma)
				(is-goal-carry right roomb)
				(is-goal-carry right ball4)
				(is-goal-carry right ball3)
				(is-goal-carry right ball2)
				(is-goal-carry right ball1)
				(is-goal-carry right left)
				(is-goal-carry right right)
			)
	)

)
