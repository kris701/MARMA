
	(:action move-col-to-col
		:parameters (?card ?oldcard ?newcard - card)
		:precondition (and 
			(faceup ?card)
			(clear ?newcard)
			(canstack ?card ?newcard)
			(on ?card ?oldcard))
		:effect(and
			(on ?card ?newcard)
			(clear ?oldcard)
			(faceup ?oldcard)
			(not (on ?card ?oldcard))
			(not (clear ?newcard))))