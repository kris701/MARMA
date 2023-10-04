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
				(when
					(leader-state-pointing ?s ?d_new)
					(is-goal-pointing ?s ?d_new)
				)

				(when
					(not
						(leader-state-pointing ?s ?d_prev)
					)
					(is-goal-pointing ?s ?d_prev)
				)

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
				(when
					(leader-state-power_on ?i)
					(is-goal-power_on ?i)
				)

				(when
					(not
						(leader-state-calibrated ?i)
					)
					(is-goal-calibrated ?i)
				)

				(when
					(not
						(leader-state-power_avail ?s)
					)
					(is-goal-power_avail ?s)
				)

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
				(when
					(not
						(leader-state-power_on ?i)
					)
					(is-goal-power_on ?i)
				)

				(when
					(leader-state-power_avail ?s)
					(is-goal-power_avail ?s)
				)

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
				(when
					(leader-state-calibrated ?i)
					(is-goal-calibrated ?i)
				)

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
				(when
					(leader-state-have_image ?d ?m)
					(is-goal-have_image ?d ?m)
				)

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
			(and
				(not
					(leader-turn)
				)
				(forall ( ?i - instrument ?s - satellite)
					(when
						(not
							(leader-state-on_board ?i ?s)
						)
						(is-goal-on_board ?i ?s)
					)

				)

				(forall ( ?i - instrument ?m - mode)
					(when
						(not
							(leader-state-supports ?i ?m)
						)
						(is-goal-supports ?i ?m)
					)

				)

				(forall ( ?s - satellite ?d - direction)
					(when
						(not
							(leader-state-pointing ?s ?d)
						)
						(is-goal-pointing ?s ?d)
					)

				)

				(forall ( ?s - satellite)
					(when
						(not
							(leader-state-power_avail ?s)
						)
						(is-goal-power_avail ?s)
					)

				)

				(forall ( ?i - instrument)
					(when
						(not
							(leader-state-power_on ?i)
						)
						(is-goal-power_on ?i)
					)

				)

				(forall ( ?i - instrument)
					(when
						(not
							(leader-state-calibrated ?i)
						)
						(is-goal-calibrated ?i)
					)

				)

				(forall ( ?d - direction ?m - mode)
					(when
						(not
							(leader-state-have_image ?d ?m)
						)
						(is-goal-have_image ?d ?m)
					)

				)

				(forall ( ?i - instrument ?d - direction)
					(when
						(not
							(leader-state-calibration_target ?i ?d)
						)
						(is-goal-calibration_target ?i ?d)
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
				(is-goal-on_board instrument0 satellite0)
				(is-goal-supports instrument0 image1)
				(is-goal-supports instrument0 spectrograph2)
				(is-goal-supports instrument0 thermograph0)
				(is-goal-pointing satellite0 star0)
				(is-goal-pointing satellite0 groundstation1)
				(is-goal-pointing satellite0 groundstation2)
				(is-goal-pointing satellite0 phenomenon3)
				(is-goal-pointing satellite0 phenomenon4)
				(is-goal-pointing satellite0 star5)
				(is-goal-pointing satellite0 phenomenon6)
				(is-goal-power_avail satellite0)
				(is-goal-power_on instrument0)
				(is-goal-calibrated instrument0)
				(is-goal-have_image star0 image1)
				(is-goal-have_image star0 spectrograph2)
				(is-goal-have_image star0 thermograph0)
				(is-goal-have_image groundstation1 image1)
				(is-goal-have_image groundstation1 spectrograph2)
				(is-goal-have_image groundstation1 thermograph0)
				(is-goal-have_image groundstation2 image1)
				(is-goal-have_image groundstation2 spectrograph2)
				(is-goal-have_image groundstation2 thermograph0)
				(is-goal-have_image phenomenon3 image1)
				(is-goal-have_image phenomenon3 spectrograph2)
				(is-goal-have_image phenomenon3 thermograph0)
				(is-goal-have_image phenomenon4 image1)
				(is-goal-have_image phenomenon4 spectrograph2)
				(is-goal-have_image phenomenon4 thermograph0)
				(is-goal-have_image star5 image1)
				(is-goal-have_image star5 spectrograph2)
				(is-goal-have_image star5 thermograph0)
				(is-goal-have_image phenomenon6 image1)
				(is-goal-have_image phenomenon6 spectrograph2)
				(is-goal-have_image phenomenon6 thermograph0)
				(is-goal-calibration_target instrument0 star0)
				(is-goal-calibration_target instrument0 groundstation1)
				(is-goal-calibration_target instrument0 groundstation2)
				(is-goal-calibration_target instrument0 phenomenon3)
				(is-goal-calibration_target instrument0 phenomenon4)
				(is-goal-calibration_target instrument0 star5)
				(is-goal-calibration_target instrument0 phenomenon6)
			)
	)

)
