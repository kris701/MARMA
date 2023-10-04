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
		(leader-state-supports instrument0 thermograph0)
		(leader-state-calibration_target instrument0 groundstation2)
		(leader-state-on_board instrument0 satellite0)
		(leader-state-power_avail satellite0)
		(leader-state-pointing satellite0 phenomenon6)
		(leader-turn)
	)

	(:goal
		(and
			(have_image phenomenon4 thermograph0)
			(have_image star5 thermograph0)
			(have_image phenomenon6 thermograph0)
		)
	)

)
