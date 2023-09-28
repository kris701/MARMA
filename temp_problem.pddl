(define (problem strips-gripper-x-1)

(:domain gripper-strips)

(:objects fix_rooma
 fix_roomb
 fix_ball4
 fix_ball3
 fix_ball2
 fix_ball1
 fix_left
 fix_right
 attack_rooma
 attack_roomb
 attack_ball4
 attack_ball3
 attack_ball2
 attack_ball1
 attack_left
 attack_right
)

(:init (fix_room fix_rooma)
 (fix_room fix_roomb)
 (fix_ball fix_ball4)
 (fix_ball fix_ball3)
 (fix_ball fix_ball2)
 (fix_ball fix_ball1)
 (fix_at-robby fix_rooma)
 (fix_free fix_left)
 (fix_free fix_right)
 (fix_at fix_ball4 fix_rooma)
 (fix_at fix_ball3 fix_rooma)
 (fix_at fix_ball2 fix_rooma)
 (fix_at fix_ball1 fix_rooma)
 (fix_gripper fix_left)
 (fix_gripper fix_right)
 (attack_room attack_rooma)
 (attack_room attack_roomb)
 (attack_ball attack_ball4)
 (attack_ball attack_ball3)
 (attack_ball attack_ball2)
 (attack_ball attack_ball1)
 (attack_at-robby attack_rooma)
 (attack_free attack_left)
 (attack_free attack_right)
 (attack_at attack_ball4 attack_rooma)
 (attack_at attack_ball3 attack_rooma)
 (attack_at attack_ball2 attack_rooma)
 (attack_at attack_ball1 attack_rooma)
 (attack_gripper attack_left)
 (attack_gripper attack_right)
)

(:goal (and))

)