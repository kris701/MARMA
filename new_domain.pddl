(define
	(domain satellite)

	(:requirements :strips :equality :typing)

	(:types
		satellite
		direction
		instrument
		mode
	)

	(:predicates
		(on_board ?i - instrument ?s - satellite)
		(supports ?i - instrument ?m - mode)
		(pointing ?s - satellite ?d - direction)
		(power_avail ?s - satellite)
		(power_on ?i - instrument)
		(calibrated ?i - instrument)
		(have_image ?d - direction ?m - mode)
		(calibration_target ?i - instrument ?d - direction)
		(leader-state-on_board ?i - instrument ?s - satellite)
		(leader-state-supports ?i - instrument ?m - mode)
		(leader-state-pointing ?s - satellite ?d - direction)
		(leader-state-power_avail ?s - satellite)
		(leader-state-power_on ?i - instrument)
		(leader-state-calibrated ?i - instrument)
		(leader-state-have_image ?d - direction ?m - mode)
		(leader-state-calibration_target ?i - instrument ?d - direction)
		(is-goal-on_board ?i - instrument ?s - satellite)
		(is-goal-supports ?i - instrument ?m - mode)
		(is-goal-pointing ?s - satellite ?d - direction)
		(is-goal-power_avail ?s - satellite)
		(is-goal-power_on ?i - instrument)
		(is-goal-calibrated ?i - instrument)
		(is-goal-have_image ?d - direction ?m - mode)
		(is-goal-calibration_target ?i - instrument ?d - direction)
		(leader-turn)
	)

	(:action fix_turn_to
		:parameters ( ?s - satellite ?d_new - direction ?d_prev - direction)
		:precondition 
			(and
				(leader-state-pointing ?s ?d_prev)
				(leader-turn)
			)
		:effect 
			(and
				(leader-state-pointing ?s ?d_new)
				(not
					(leader-state-pointing ?s ?d_prev)
				)
			)
	)

	(:action attack_turn_to
		:parameters ( ?s - satellite ?d_new - direction ?d_prev - direction)
		:precondition 
			(and
				(pointing ?s ?d_prev)
				(not
					(leader-turn)
				)
			)
		:effect 
			(and
				(pointing ?s ?d_new)
				(not
					(pointing ?s ?d_prev)
				)
			)
	)

	(:action attack_turn_to_goal
		:parameters ( ?s - satellite ?d_new - direction ?d_prev - direction)
		:precondition 
			(and
				(pointing ?s ?d_prev)
				(not
					(leader-turn)
				)
				(leader-state-pointing ?s ?d_new)
				(not
					(leader-state-pointing ?s ?d_prev)
				)
			)
		:effect 
			(and
				(pointing ?s ?d_new)
				(not
					(pointing ?s ?d_prev)
				)
				(is-goal-pointing ?s ?d_new)
				(is-goal-pointing ?s ?d_prev)
			)
	)

	(:action fix_switch_on
		:parameters ( ?i - instrument ?s - satellite)
		:precondition 
			(and
				(leader-state-on_board ?i ?s)
				(leader-state-power_avail ?s)
				(leader-turn)
			)
		:effect 
			(and
				(leader-state-power_on ?i)
				(not
					(leader-state-calibrated ?i)
				)
				(not
					(leader-state-power_avail ?s)
				)
			)
	)

	(:action attack_switch_on
		:parameters ( ?i - instrument ?s - satellite)
		:precondition 
			(and
				(on_board ?i ?s)
				(power_avail ?s)
				(not
					(leader-turn)
				)
			)
		:effect 
			(and
				(power_on ?i)
				(not
					(calibrated ?i)
				)
				(not
					(power_avail ?s)
				)
			)
	)

	(:action attack_switch_on_goal
		:parameters ( ?i - instrument ?s - satellite)
		:precondition 
			(and
				(on_board ?i ?s)
				(power_avail ?s)
				(not
					(leader-turn)
				)
				(leader-state-power_on ?i)
				(not
					(leader-state-calibrated ?i)
				)
				(not
					(leader-state-power_avail ?s)
				)
			)
		:effect 
			(and
				(power_on ?i)
				(not
					(calibrated ?i)
				)
				(not
					(power_avail ?s)
				)
				(is-goal-power_on ?i)
				(is-goal-calibrated ?i)
				(is-goal-power_avail ?s)
			)
	)

	(:action fix_switch_off
		:parameters ( ?i - instrument ?s - satellite)
		:precondition 
			(and
				(leader-state-on_board ?i ?s)
				(leader-state-power_on ?i)
				(leader-turn)
			)
		:effect 
			(and
				(not
					(leader-state-power_on ?i)
				)
				(leader-state-power_avail ?s)
			)
	)

	(:action attack_switch_off
		:parameters ( ?i - instrument ?s - satellite)
		:precondition 
			(and
				(on_board ?i ?s)
				(power_on ?i)
				(not
					(leader-turn)
				)
			)
		:effect 
			(and
				(not
					(power_on ?i)
				)
				(power_avail ?s)
			)
	)

	(:action attack_switch_off_goal
		:parameters ( ?i - instrument ?s - satellite)
		:precondition 
			(and
				(on_board ?i ?s)
				(power_on ?i)
				(not
					(leader-turn)
				)
				(not
					(leader-state-power_on ?i)
				)
				(leader-state-power_avail ?s)
			)
		:effect 
			(and
				(not
					(power_on ?i)
				)
				(power_avail ?s)
				(is-goal-power_on ?i)
				(is-goal-power_avail ?s)
			)
	)

	(:action fix_calibrate
		:parameters ( ?s - satellite ?i - instrument ?d - direction)
		:precondition 
			(and
				(leader-state-on_board ?i ?s)
				(leader-state-calibration_target ?i ?d)
				(leader-state-pointing ?s ?d)
				(leader-state-power_on ?i)
				(leader-turn)
			)
		:effect 
			(and
				(leader-state-calibrated ?i)
			)
	)

	(:action attack_calibrate
		:parameters ( ?s - satellite ?i - instrument ?d - direction)
		:precondition 
			(and
				(on_board ?i ?s)
				(calibration_target ?i ?d)
				(pointing ?s ?d)
				(power_on ?i)
				(not
					(leader-turn)
				)
			)
		:effect 
			(and
				(calibrated ?i)
			)
	)

	(:action attack_calibrate_goal
		:parameters ( ?s - satellite ?i - instrument ?d - direction)
		:precondition 
			(and
				(on_board ?i ?s)
				(calibration_target ?i ?d)
				(pointing ?s ?d)
				(power_on ?i)
				(not
					(leader-turn)
				)
				(leader-state-calibrated ?i)
			)
		:effect 
			(and
				(calibrated ?i)
				(is-goal-calibrated ?i)
			)
	)

	(:action fix_take_image
		:parameters ( ?s - satellite ?d - direction ?i - instrument ?m - mode)
		:precondition 
			(and
				(leader-state-calibrated ?i)
				(leader-state-on_board ?i ?s)
				(leader-state-supports ?i ?m)
				(leader-state-power_on ?i)
				(leader-state-pointing ?s ?d)
				(leader-state-power_on ?i)
				(leader-turn)
			)
		:effect 
			(and
				(leader-state-have_image ?d ?m)
			)
	)

	(:action attack_take_image
		:parameters ( ?s - satellite ?d - direction ?i - instrument ?m - mode)
		:precondition 
			(and
				(calibrated ?i)
				(on_board ?i ?s)
				(supports ?i ?m)
				(power_on ?i)
				(pointing ?s ?d)
				(power_on ?i)
				(not
					(leader-turn)
				)
			)
		:effect 
			(and
				(have_image ?d ?m)
			)
	)

	(:action attack_take_image_goal
		:parameters ( ?s - satellite ?d - direction ?i - instrument ?m - mode)
		:precondition 
			(and
				(calibrated ?i)
				(on_board ?i ?s)
				(supports ?i ?m)
				(power_on ?i)
				(pointing ?s ?d)
				(power_on ?i)
				(not
					(leader-turn)
				)
				(leader-state-have_image ?d ?m)
			)
		:effect 
			(and
				(have_image ?d ?m)
				(is-goal-have_image ?d ?m)
			)
	)

	(:action fix_meta_$switch-on-calibrate
		:parameters ( ?s - satellite ?i - instrument)
		:precondition 
			(and
				(leader-state-on_board ?i ?s)
				(leader-state-power_avail ?s)
				(leader-turn)
			)
		:effect 
			(and
				(leader-state-calibrated ?i)
				(leader-state-power_on ?i)
				(not
					(leader-state-power_avail ?s)
				)
			)
	)

	(:action fix_pass-turn
		:parameters ()
		:precondition 
			(leader-turn)
		:effect 
			(not
				(leader-turn)
			)
	)

	(:action attack_reach-goal
		:parameters ()
		:precondition 
			(leader-turn)
		:effect 
			(and
				(have_image phenomenon4 thermograph0)
				(have_image star5 thermograph0)
				(have_image phenomenon6 thermograph0)
			)
	)

)
