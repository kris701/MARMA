(define
	(domain miconic)
	(:requirements :strips :typing)
	(:types
		passenger - object
		floor - object
	)
	(:predicates
		(origin ?person - passenger ?floor - floor)
		(destin ?person - passenger ?floor - floor)
		(above ?floor1 - floor ?floor2 - floor)
		(boarded ?person - passenger)
		(served ?person - passenger)
		(lift-at ?floor - floor)
	)
	(:action board
		:parameters (?f - floor ?p - passenger)
		:precondition 
			(and
				(lift-at ?f)
				(origin ?p ?f)
			)
		:effect 
			(and
				(boarded ?p)
				(not (origin ?p ?f))
			)
	)
	(:action depart
		:parameters (?f - floor ?p - passenger)
		:precondition 
			(and
				(lift-at ?f)
				(destin ?p ?f)
				(boarded ?p)
			)
		:effect 
			(and
				(not (boarded ?p))
				(served ?p)
			)
	)
	(:action up
		:parameters (?f1 - floor ?f2 - floor)
		:precondition 
			(and
				(lift-at ?f1)
				(above ?f1 ?f2)
			)
		:effect 
			(and
				(lift-at ?f2)
				(not (lift-at ?f1))
			)
	)
	(:action down
		:parameters (?f1 - floor ?f2 - floor)
		:precondition 
			(and
				(lift-at ?f1)
				(above ?f2 ?f1)
			)
		:effect 
			(and
				(lift-at ?f2)
				(not (lift-at ?f1))
			)
	)
	(:action $meta_1
		:parameters (?f1 - floor ?f2 - floor)
		:precondition 
			(and
				(lift-at ?f1)
				(above ?f1 ?f2)
			)
		:effect 
			(and
				(lift-at ?f1)
				(not (lift-at ?f2))
			)
	)
	(:action $meta_2
		:parameters (?f1 - floor ?f2 - floor ?px3 - passenger)
		:precondition 
			(and
				(lift-at ?f1)
				(above ?f1 ?f2)
				(destin ?px3 ?f2)
				(boarded ?px3)
			)
		:effect 
			(and
				(served ?px3)
				(lift-at ?f1)
				(not (boarded ?px3))
				(not (lift-at ?f2))
			)
	)
	(:action $meta_3
		:parameters (?f1 - floor ?f2 - floor ?px2 - passenger)
		:precondition 
			(and
				(lift-at ?f1)
				(above ?f1 ?f2)
				(origin ?px2 ?f2)
			)
		:effect 
			(and
				(boarded ?px2)
				(lift-at ?f1)
				(not (origin ?px2 ?f2))
				(not (lift-at ?f2))
			)
	)
	(:action $meta_6
		:parameters (?px2 - passenger)
		:precondition 
			(and (boarded ?px2))
		:effect 
			(and
				(served ?px2)
				(not (boarded ?px2))
			)
	)
	(:action $meta_4
		:parameters (?f1 - floor ?f2 - floor)
		:precondition 
			(and
				(lift-at ?f1)
				(above ?f1 ?f2)
			)
		:effect 
			(and
				(lift-at ?f1)
				(not (lift-at ?f2))
			)
	)
	(:action $meta_5
		:parameters (?f2 - floor ?px2 - passenger)
		:precondition 
			(and
				(destin ?px2 ?f2)
				(boarded ?px2)
			)
		:effect 
			(and
				(served ?px2)
				(not (boarded ?px2))
			)
	)
	(:action $meta_11
		:parameters (?f2 - floor ?px2 - passenger)
		:precondition 
			(and (origin ?px2 ?f2))
		:effect 
			(and
				(boarded ?px2)
				(not (origin ?px2 ?f2))
			)
	)
)
