(define
	(problem strips-gripper-x-1)

	(:domain gripper-strips)

	(:objects
		rooma
		roomb
		ball4
		ball3
		ball2
		ball1
		left
		right
	)

	(:init
		(room rooma)
		(room roomb)
		(ball ball4)
		(ball ball3)
		(ball ball2)
		(ball ball1)
		(at-robby rooma)
		(free left)
		(free right)
		(at ball4 rooma)
		(at ball3 rooma)
		(at ball2 rooma)
		(at ball1 rooma)
		(gripper left)
		(gripper right)
		(attack_room rooma)
		(attack_room roomb)
		(attack_ball ball4)
		(attack_ball ball3)
		(attack_ball ball2)
		(attack_ball ball1)
		(attack_at-robby rooma)
		(attack_free left)
		(attack_free right)
		(attack_at ball4 rooma)
		(attack_at ball3 rooma)
		(attack_at ball2 rooma)
		(attack_at ball1 rooma)
		(attack_gripper left)
		(attack_gripper right)
	)

	(:goal
		(and
			(is-goal-at ball4 roomb)
			(is-goal-at ball3 roomb)
			(is-goal-at ball2 roomb)
			(is-goal-at ball1 roomb)
		)
	)

)
