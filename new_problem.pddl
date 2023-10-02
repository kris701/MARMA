(define
	(problem strips-sat-x-1)

	(:domain satellite)

	(:objects
		satellite0 - satellite
		instrument0 - instrument
		image1 - mode
		spectrograph2 - mode
		thermograph0 - mode
		star0 - direction
		groundstation1 - direction
		groundstation2 - direction
		phenomenon3 - direction
		phenomenon4 - direction
		star5 - direction
		phenomenon6 - direction
	)

	(:init
		(supports instrument0 thermograph0)
		(calibration_target instrument0 groundstation2)
		(on_board instrument0 satellite0)
		(power_avail satellite0)
		(pointing satellite0 phenomenon6)
		(fix_supports instrument0 thermograph0)
		(fix_calibration_target instrument0 groundstation2)
		(fix_on_board instrument0 satellite0)
		(fix_power_avail satellite0)
		(fix_pointing satellite0 phenomenon6)
	)

	(:goal
		(and
			(is-goal-have_image phenomenon4 thermograph0)
			(is-goal-have_image star5 thermograph0)
			(is-goal-have_image phenomenon6 thermograph0)
		)
	)

)
