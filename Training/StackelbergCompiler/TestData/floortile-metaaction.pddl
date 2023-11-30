(:action $meta_5
	:parameters (?r - robot ?yx3 - tile ?xx4 - tile)
	:precondition 
		(and
			(robot-at ?r ?xx4)
			(up ?yx3 ?xx4)
			(clear ?yx3)
		)
	:effect 
		(and (not (clear ?yx3)))
)
