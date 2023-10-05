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
		(leader-state-power_avail satellite0)
		(leader-state-pointing satellite0 phenomenon6)
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
		(leader-turn)
	)

	(:goal
		(and
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
			(not
				(leader-turn)
			)
		)
	)

)
