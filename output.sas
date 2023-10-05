begin_version
3
end_version
begin_metric
0
end_metric
30
begin_variable
var0
-1
2
Atom calibrated(instrument0)
NegatedAtom calibrated(instrument0)
end_variable
begin_variable
var1
-1
2
Atom have_image(groundstation1, thermograph0)
NegatedAtom have_image(groundstation1, thermograph0)
end_variable
begin_variable
var2
-1
2
Atom have_image(groundstation2, thermograph0)
NegatedAtom have_image(groundstation2, thermograph0)
end_variable
begin_variable
var3
-1
2
Atom have_image(phenomenon3, thermograph0)
NegatedAtom have_image(phenomenon3, thermograph0)
end_variable
begin_variable
var4
-1
2
Atom have_image(phenomenon4, thermograph0)
NegatedAtom have_image(phenomenon4, thermograph0)
end_variable
begin_variable
var5
-1
2
Atom have_image(phenomenon6, thermograph0)
NegatedAtom have_image(phenomenon6, thermograph0)
end_variable
begin_variable
var6
-1
2
Atom have_image(star0, thermograph0)
NegatedAtom have_image(star0, thermograph0)
end_variable
begin_variable
var7
-1
2
Atom have_image(star5, thermograph0)
NegatedAtom have_image(star5, thermograph0)
end_variable
begin_variable
var8
-1
2
Atom is-goal-calibrated(instrument0)
NegatedAtom is-goal-calibrated(instrument0)
end_variable
begin_variable
var9
-1
2
Atom is-goal-power_on(instrument0)
NegatedAtom is-goal-power_on(instrument0)
end_variable
begin_variable
var10
-1
2
Atom leader-state-calibrated(instrument0)
NegatedAtom leader-state-calibrated(instrument0)
end_variable
begin_variable
var11
-1
2
Atom leader-state-have_image(groundstation1, thermograph0)
NegatedAtom leader-state-have_image(groundstation1, thermograph0)
end_variable
begin_variable
var12
-1
2
Atom leader-state-have_image(groundstation2, thermograph0)
NegatedAtom leader-state-have_image(groundstation2, thermograph0)
end_variable
begin_variable
var13
-1
2
Atom leader-state-have_image(phenomenon3, thermograph0)
NegatedAtom leader-state-have_image(phenomenon3, thermograph0)
end_variable
begin_variable
var14
-1
2
Atom leader-state-have_image(phenomenon4, thermograph0)
NegatedAtom leader-state-have_image(phenomenon4, thermograph0)
end_variable
begin_variable
var15
-1
2
Atom leader-state-have_image(phenomenon6, thermograph0)
NegatedAtom leader-state-have_image(phenomenon6, thermograph0)
end_variable
begin_variable
var16
-1
2
Atom leader-state-have_image(star0, thermograph0)
NegatedAtom leader-state-have_image(star0, thermograph0)
end_variable
begin_variable
var17
-1
2
Atom leader-state-have_image(star5, thermograph0)
NegatedAtom leader-state-have_image(star5, thermograph0)
end_variable
begin_variable
var18
-1
7
Atom leader-state-pointing(satellite0, groundstation1)
Atom leader-state-pointing(satellite0, groundstation2)
Atom leader-state-pointing(satellite0, phenomenon3)
Atom leader-state-pointing(satellite0, phenomenon4)
Atom leader-state-pointing(satellite0, phenomenon6)
Atom leader-state-pointing(satellite0, star0)
Atom leader-state-pointing(satellite0, star5)
end_variable
begin_variable
var19
-1
2
Atom leader-state-power_avail(satellite0)
Atom leader-state-power_on(instrument0)
end_variable
begin_variable
var20
-1
2
Atom leader-turn()
NegatedAtom leader-turn()
end_variable
begin_variable
var21
-1
2
Atom pointing(satellite0, groundstation1)
NegatedAtom pointing(satellite0, groundstation1)
end_variable
begin_variable
var22
-1
2
Atom pointing(satellite0, groundstation2)
NegatedAtom pointing(satellite0, groundstation2)
end_variable
begin_variable
var23
-1
2
Atom pointing(satellite0, phenomenon3)
NegatedAtom pointing(satellite0, phenomenon3)
end_variable
begin_variable
var24
-1
2
Atom pointing(satellite0, phenomenon4)
NegatedAtom pointing(satellite0, phenomenon4)
end_variable
begin_variable
var25
-1
2
Atom pointing(satellite0, phenomenon6)
NegatedAtom pointing(satellite0, phenomenon6)
end_variable
begin_variable
var26
-1
2
Atom pointing(satellite0, star0)
NegatedAtom pointing(satellite0, star0)
end_variable
begin_variable
var27
-1
2
Atom pointing(satellite0, star5)
NegatedAtom pointing(satellite0, star5)
end_variable
begin_variable
var28
-1
2
Atom power_avail(satellite0)
NegatedAtom power_avail(satellite0)
end_variable
begin_variable
var29
-1
2
Atom power_on(instrument0)
NegatedAtom power_on(instrument0)
end_variable
2
begin_mutex_group
7
18 0
18 1
18 2
18 3
18 4
18 5
18 6
end_mutex_group
begin_mutex_group
2
19 0
19 1
end_mutex_group
begin_state
1
1
1
1
1
1
1
1
0
0
1
1
1
1
1
1
1
1
4
0
0
1
1
1
1
0
1
1
0
1
end_state
begin_goal
3
8 0
9 0
20 1
end_goal
158
begin_operator
attack_calibrate satellite0 instrument0 groundstation2
3
20 1
22 0
29 0
1
0 0 -1 0
0
end_operator
begin_operator
attack_calibrate_goal satellite0 instrument0 groundstation2
3
20 1
22 0
29 0
2
0 0 -1 0
0 8 1 0
0
end_operator
begin_operator
attack_reach-goal 
0
1
0 20 0 1
0
end_operator
begin_operator
attack_switch_off instrument0 satellite0
1
20 1
2
0 28 -1 0
0 29 0 1
0
end_operator
begin_operator
attack_switch_off_goal instrument0 satellite0
2
19 0
20 1
2
0 28 -1 0
0 29 0 1
0
end_operator
begin_operator
attack_switch_on instrument0 satellite0
1
20 1
3
0 0 -1 1
0 28 0 1
0 29 -1 0
0
end_operator
begin_operator
attack_switch_on_goal instrument0 satellite0
1
20 1
4
0 0 -1 1
0 9 1 0
0 28 0 1
0 29 -1 0
0
end_operator
begin_operator
attack_take_image satellite0 groundstation1 instrument0 thermograph0
4
0 0
20 1
21 0
29 0
1
0 1 -1 0
0
end_operator
begin_operator
attack_take_image satellite0 groundstation2 instrument0 thermograph0
4
0 0
20 1
22 0
29 0
1
0 2 -1 0
0
end_operator
begin_operator
attack_take_image satellite0 phenomenon3 instrument0 thermograph0
4
0 0
20 1
23 0
29 0
1
0 3 -1 0
0
end_operator
begin_operator
attack_take_image satellite0 phenomenon4 instrument0 thermograph0
4
0 0
20 1
24 0
29 0
1
0 4 -1 0
0
end_operator
begin_operator
attack_take_image satellite0 phenomenon6 instrument0 thermograph0
4
0 0
20 1
25 0
29 0
1
0 5 -1 0
0
end_operator
begin_operator
attack_take_image satellite0 star0 instrument0 thermograph0
4
0 0
20 1
26 0
29 0
1
0 6 -1 0
0
end_operator
begin_operator
attack_take_image satellite0 star5 instrument0 thermograph0
4
0 0
20 1
27 0
29 0
1
0 7 -1 0
0
end_operator
begin_operator
attack_turn_to satellite0 groundstation1 groundstation2
1
20 1
2
0 21 -1 0
0 22 0 1
0
end_operator
begin_operator
attack_turn_to satellite0 groundstation1 phenomenon3
1
20 1
2
0 21 -1 0
0 23 0 1
0
end_operator
begin_operator
attack_turn_to satellite0 groundstation1 phenomenon4
1
20 1
2
0 21 -1 0
0 24 0 1
0
end_operator
begin_operator
attack_turn_to satellite0 groundstation1 phenomenon6
1
20 1
2
0 21 -1 0
0 25 0 1
0
end_operator
begin_operator
attack_turn_to satellite0 groundstation1 star0
1
20 1
2
0 21 -1 0
0 26 0 1
0
end_operator
begin_operator
attack_turn_to satellite0 groundstation1 star5
1
20 1
2
0 21 -1 0
0 27 0 1
0
end_operator
begin_operator
attack_turn_to satellite0 groundstation2 groundstation1
1
20 1
2
0 21 0 1
0 22 -1 0
0
end_operator
begin_operator
attack_turn_to satellite0 groundstation2 phenomenon3
1
20 1
2
0 22 -1 0
0 23 0 1
0
end_operator
begin_operator
attack_turn_to satellite0 groundstation2 phenomenon4
1
20 1
2
0 22 -1 0
0 24 0 1
0
end_operator
begin_operator
attack_turn_to satellite0 groundstation2 phenomenon6
1
20 1
2
0 22 -1 0
0 25 0 1
0
end_operator
begin_operator
attack_turn_to satellite0 groundstation2 star0
1
20 1
2
0 22 -1 0
0 26 0 1
0
end_operator
begin_operator
attack_turn_to satellite0 groundstation2 star5
1
20 1
2
0 22 -1 0
0 27 0 1
0
end_operator
begin_operator
attack_turn_to satellite0 phenomenon3 groundstation1
1
20 1
2
0 21 0 1
0 23 -1 0
0
end_operator
begin_operator
attack_turn_to satellite0 phenomenon3 groundstation2
1
20 1
2
0 22 0 1
0 23 -1 0
0
end_operator
begin_operator
attack_turn_to satellite0 phenomenon3 phenomenon4
1
20 1
2
0 23 -1 0
0 24 0 1
0
end_operator
begin_operator
attack_turn_to satellite0 phenomenon3 phenomenon6
1
20 1
2
0 23 -1 0
0 25 0 1
0
end_operator
begin_operator
attack_turn_to satellite0 phenomenon3 star0
1
20 1
2
0 23 -1 0
0 26 0 1
0
end_operator
begin_operator
attack_turn_to satellite0 phenomenon3 star5
1
20 1
2
0 23 -1 0
0 27 0 1
0
end_operator
begin_operator
attack_turn_to satellite0 phenomenon4 groundstation1
1
20 1
2
0 21 0 1
0 24 -1 0
0
end_operator
begin_operator
attack_turn_to satellite0 phenomenon4 groundstation2
1
20 1
2
0 22 0 1
0 24 -1 0
0
end_operator
begin_operator
attack_turn_to satellite0 phenomenon4 phenomenon3
1
20 1
2
0 23 0 1
0 24 -1 0
0
end_operator
begin_operator
attack_turn_to satellite0 phenomenon4 phenomenon6
1
20 1
2
0 24 -1 0
0 25 0 1
0
end_operator
begin_operator
attack_turn_to satellite0 phenomenon4 star0
1
20 1
2
0 24 -1 0
0 26 0 1
0
end_operator
begin_operator
attack_turn_to satellite0 phenomenon4 star5
1
20 1
2
0 24 -1 0
0 27 0 1
0
end_operator
begin_operator
attack_turn_to satellite0 phenomenon6 groundstation1
1
20 1
2
0 21 0 1
0 25 -1 0
0
end_operator
begin_operator
attack_turn_to satellite0 phenomenon6 groundstation2
1
20 1
2
0 22 0 1
0 25 -1 0
0
end_operator
begin_operator
attack_turn_to satellite0 phenomenon6 phenomenon3
1
20 1
2
0 23 0 1
0 25 -1 0
0
end_operator
begin_operator
attack_turn_to satellite0 phenomenon6 phenomenon4
1
20 1
2
0 24 0 1
0 25 -1 0
0
end_operator
begin_operator
attack_turn_to satellite0 phenomenon6 star0
1
20 1
2
0 25 -1 0
0 26 0 1
0
end_operator
begin_operator
attack_turn_to satellite0 phenomenon6 star5
1
20 1
2
0 25 -1 0
0 27 0 1
0
end_operator
begin_operator
attack_turn_to satellite0 star0 groundstation1
1
20 1
2
0 21 0 1
0 26 -1 0
0
end_operator
begin_operator
attack_turn_to satellite0 star0 groundstation2
1
20 1
2
0 22 0 1
0 26 -1 0
0
end_operator
begin_operator
attack_turn_to satellite0 star0 phenomenon3
1
20 1
2
0 23 0 1
0 26 -1 0
0
end_operator
begin_operator
attack_turn_to satellite0 star0 phenomenon4
1
20 1
2
0 24 0 1
0 26 -1 0
0
end_operator
begin_operator
attack_turn_to satellite0 star0 phenomenon6
1
20 1
2
0 25 0 1
0 26 -1 0
0
end_operator
begin_operator
attack_turn_to satellite0 star0 star5
1
20 1
2
0 26 -1 0
0 27 0 1
0
end_operator
begin_operator
attack_turn_to satellite0 star5 groundstation1
1
20 1
2
0 21 0 1
0 27 -1 0
0
end_operator
begin_operator
attack_turn_to satellite0 star5 groundstation2
1
20 1
2
0 22 0 1
0 27 -1 0
0
end_operator
begin_operator
attack_turn_to satellite0 star5 phenomenon3
1
20 1
2
0 23 0 1
0 27 -1 0
0
end_operator
begin_operator
attack_turn_to satellite0 star5 phenomenon4
1
20 1
2
0 24 0 1
0 27 -1 0
0
end_operator
begin_operator
attack_turn_to satellite0 star5 phenomenon6
1
20 1
2
0 25 0 1
0 27 -1 0
0
end_operator
begin_operator
attack_turn_to satellite0 star5 star0
1
20 1
2
0 26 0 1
0 27 -1 0
0
end_operator
begin_operator
attack_turn_to_goal satellite0 groundstation1 groundstation2
2
18 0
20 1
2
0 21 -1 0
0 22 0 1
0
end_operator
begin_operator
attack_turn_to_goal satellite0 groundstation1 phenomenon3
2
18 0
20 1
2
0 21 -1 0
0 23 0 1
0
end_operator
begin_operator
attack_turn_to_goal satellite0 groundstation1 phenomenon4
2
18 0
20 1
2
0 21 -1 0
0 24 0 1
0
end_operator
begin_operator
attack_turn_to_goal satellite0 groundstation1 phenomenon6
2
18 0
20 1
2
0 21 -1 0
0 25 0 1
0
end_operator
begin_operator
attack_turn_to_goal satellite0 groundstation1 star0
2
18 0
20 1
2
0 21 -1 0
0 26 0 1
0
end_operator
begin_operator
attack_turn_to_goal satellite0 groundstation1 star5
2
18 0
20 1
2
0 21 -1 0
0 27 0 1
0
end_operator
begin_operator
attack_turn_to_goal satellite0 groundstation2 groundstation1
2
18 1
20 1
2
0 21 0 1
0 22 -1 0
0
end_operator
begin_operator
attack_turn_to_goal satellite0 groundstation2 phenomenon3
2
18 1
20 1
2
0 22 -1 0
0 23 0 1
0
end_operator
begin_operator
attack_turn_to_goal satellite0 groundstation2 phenomenon4
2
18 1
20 1
2
0 22 -1 0
0 24 0 1
0
end_operator
begin_operator
attack_turn_to_goal satellite0 groundstation2 phenomenon6
2
18 1
20 1
2
0 22 -1 0
0 25 0 1
0
end_operator
begin_operator
attack_turn_to_goal satellite0 groundstation2 star0
2
18 1
20 1
2
0 22 -1 0
0 26 0 1
0
end_operator
begin_operator
attack_turn_to_goal satellite0 groundstation2 star5
2
18 1
20 1
2
0 22 -1 0
0 27 0 1
0
end_operator
begin_operator
attack_turn_to_goal satellite0 phenomenon3 groundstation1
2
18 2
20 1
2
0 21 0 1
0 23 -1 0
0
end_operator
begin_operator
attack_turn_to_goal satellite0 phenomenon3 groundstation2
2
18 2
20 1
2
0 22 0 1
0 23 -1 0
0
end_operator
begin_operator
attack_turn_to_goal satellite0 phenomenon3 phenomenon4
2
18 2
20 1
2
0 23 -1 0
0 24 0 1
0
end_operator
begin_operator
attack_turn_to_goal satellite0 phenomenon3 phenomenon6
2
18 2
20 1
2
0 23 -1 0
0 25 0 1
0
end_operator
begin_operator
attack_turn_to_goal satellite0 phenomenon3 star0
2
18 2
20 1
2
0 23 -1 0
0 26 0 1
0
end_operator
begin_operator
attack_turn_to_goal satellite0 phenomenon3 star5
2
18 2
20 1
2
0 23 -1 0
0 27 0 1
0
end_operator
begin_operator
attack_turn_to_goal satellite0 phenomenon4 groundstation1
2
18 3
20 1
2
0 21 0 1
0 24 -1 0
0
end_operator
begin_operator
attack_turn_to_goal satellite0 phenomenon4 groundstation2
2
18 3
20 1
2
0 22 0 1
0 24 -1 0
0
end_operator
begin_operator
attack_turn_to_goal satellite0 phenomenon4 phenomenon3
2
18 3
20 1
2
0 23 0 1
0 24 -1 0
0
end_operator
begin_operator
attack_turn_to_goal satellite0 phenomenon4 phenomenon6
2
18 3
20 1
2
0 24 -1 0
0 25 0 1
0
end_operator
begin_operator
attack_turn_to_goal satellite0 phenomenon4 star0
2
18 3
20 1
2
0 24 -1 0
0 26 0 1
0
end_operator
begin_operator
attack_turn_to_goal satellite0 phenomenon4 star5
2
18 3
20 1
2
0 24 -1 0
0 27 0 1
0
end_operator
begin_operator
attack_turn_to_goal satellite0 phenomenon6 groundstation1
2
18 4
20 1
2
0 21 0 1
0 25 -1 0
0
end_operator
begin_operator
attack_turn_to_goal satellite0 phenomenon6 groundstation2
2
18 4
20 1
2
0 22 0 1
0 25 -1 0
0
end_operator
begin_operator
attack_turn_to_goal satellite0 phenomenon6 phenomenon3
2
18 4
20 1
2
0 23 0 1
0 25 -1 0
0
end_operator
begin_operator
attack_turn_to_goal satellite0 phenomenon6 phenomenon4
2
18 4
20 1
2
0 24 0 1
0 25 -1 0
0
end_operator
begin_operator
attack_turn_to_goal satellite0 phenomenon6 star0
2
18 4
20 1
2
0 25 -1 0
0 26 0 1
0
end_operator
begin_operator
attack_turn_to_goal satellite0 phenomenon6 star5
2
18 4
20 1
2
0 25 -1 0
0 27 0 1
0
end_operator
begin_operator
attack_turn_to_goal satellite0 star0 groundstation1
2
18 5
20 1
2
0 21 0 1
0 26 -1 0
0
end_operator
begin_operator
attack_turn_to_goal satellite0 star0 groundstation2
2
18 5
20 1
2
0 22 0 1
0 26 -1 0
0
end_operator
begin_operator
attack_turn_to_goal satellite0 star0 phenomenon3
2
18 5
20 1
2
0 23 0 1
0 26 -1 0
0
end_operator
begin_operator
attack_turn_to_goal satellite0 star0 phenomenon4
2
18 5
20 1
2
0 24 0 1
0 26 -1 0
0
end_operator
begin_operator
attack_turn_to_goal satellite0 star0 phenomenon6
2
18 5
20 1
2
0 25 0 1
0 26 -1 0
0
end_operator
begin_operator
attack_turn_to_goal satellite0 star0 star5
2
18 5
20 1
2
0 26 -1 0
0 27 0 1
0
end_operator
begin_operator
attack_turn_to_goal satellite0 star5 groundstation1
2
18 6
20 1
2
0 21 0 1
0 27 -1 0
0
end_operator
begin_operator
attack_turn_to_goal satellite0 star5 groundstation2
2
18 6
20 1
2
0 22 0 1
0 27 -1 0
0
end_operator
begin_operator
attack_turn_to_goal satellite0 star5 phenomenon3
2
18 6
20 1
2
0 23 0 1
0 27 -1 0
0
end_operator
begin_operator
attack_turn_to_goal satellite0 star5 phenomenon4
2
18 6
20 1
2
0 24 0 1
0 27 -1 0
0
end_operator
begin_operator
attack_turn_to_goal satellite0 star5 phenomenon6
2
18 6
20 1
2
0 25 0 1
0 27 -1 0
0
end_operator
begin_operator
attack_turn_to_goal satellite0 star5 star0
2
18 6
20 1
2
0 26 0 1
0 27 -1 0
0
end_operator
begin_operator
fix_calibrate satellite0 instrument0 groundstation2
3
18 1
19 1
20 0
2
0 0 -1 0
0 10 -1 0
0
end_operator
begin_operator
fix_meta_$switch-on-calibrate satellite0 instrument0
0
5
0 8 -1 1
0 9 -1 1
0 10 -1 0
0 19 0 1
0 20 0 1
0
end_operator
begin_operator
fix_switch_off instrument0 satellite0
1
20 0
3
0 19 1 0
0 28 -1 0
0 29 -1 1
0
end_operator
begin_operator
fix_switch_on instrument0 satellite0
1
20 0
5
0 0 -1 1
0 10 -1 1
0 19 0 1
0 28 -1 1
0 29 -1 0
0
end_operator
begin_operator
fix_take_image satellite0 groundstation1 instrument0 thermograph0
4
10 0
18 0
19 1
20 0
2
0 1 -1 0
0 11 -1 0
0
end_operator
begin_operator
fix_take_image satellite0 groundstation2 instrument0 thermograph0
4
10 0
18 1
19 1
20 0
2
0 2 -1 0
0 12 -1 0
0
end_operator
begin_operator
fix_take_image satellite0 phenomenon3 instrument0 thermograph0
4
10 0
18 2
19 1
20 0
2
0 3 -1 0
0 13 -1 0
0
end_operator
begin_operator
fix_take_image satellite0 phenomenon4 instrument0 thermograph0
4
10 0
18 3
19 1
20 0
2
0 4 -1 0
0 14 -1 0
0
end_operator
begin_operator
fix_take_image satellite0 phenomenon6 instrument0 thermograph0
4
10 0
18 4
19 1
20 0
2
0 5 -1 0
0 15 -1 0
0
end_operator
begin_operator
fix_take_image satellite0 star0 instrument0 thermograph0
4
10 0
18 5
19 1
20 0
2
0 6 -1 0
0 16 -1 0
0
end_operator
begin_operator
fix_take_image satellite0 star5 instrument0 thermograph0
4
10 0
18 6
19 1
20 0
2
0 7 -1 0
0 17 -1 0
0
end_operator
begin_operator
fix_turn_to satellite0 groundstation1 groundstation1
2
18 0
20 0
1
0 21 -1 0
0
end_operator
begin_operator
fix_turn_to satellite0 groundstation1 groundstation2
1
20 0
3
0 18 1 0
0 21 -1 0
0 22 -1 1
0
end_operator
begin_operator
fix_turn_to satellite0 groundstation1 phenomenon3
1
20 0
3
0 18 2 0
0 21 -1 0
0 23 -1 1
0
end_operator
begin_operator
fix_turn_to satellite0 groundstation1 phenomenon4
1
20 0
3
0 18 3 0
0 21 -1 0
0 24 -1 1
0
end_operator
begin_operator
fix_turn_to satellite0 groundstation1 phenomenon6
1
20 0
3
0 18 4 0
0 21 -1 0
0 25 -1 1
0
end_operator
begin_operator
fix_turn_to satellite0 groundstation1 star0
1
20 0
3
0 18 5 0
0 21 -1 0
0 26 -1 1
0
end_operator
begin_operator
fix_turn_to satellite0 groundstation1 star5
1
20 0
3
0 18 6 0
0 21 -1 0
0 27 -1 1
0
end_operator
begin_operator
fix_turn_to satellite0 groundstation2 groundstation1
1
20 0
3
0 18 0 1
0 21 -1 1
0 22 -1 0
0
end_operator
begin_operator
fix_turn_to satellite0 groundstation2 groundstation2
2
18 1
20 0
1
0 22 -1 0
0
end_operator
begin_operator
fix_turn_to satellite0 groundstation2 phenomenon3
1
20 0
3
0 18 2 1
0 22 -1 0
0 23 -1 1
0
end_operator
begin_operator
fix_turn_to satellite0 groundstation2 phenomenon4
1
20 0
3
0 18 3 1
0 22 -1 0
0 24 -1 1
0
end_operator
begin_operator
fix_turn_to satellite0 groundstation2 phenomenon6
1
20 0
3
0 18 4 1
0 22 -1 0
0 25 -1 1
0
end_operator
begin_operator
fix_turn_to satellite0 groundstation2 star0
1
20 0
3
0 18 5 1
0 22 -1 0
0 26 -1 1
0
end_operator
begin_operator
fix_turn_to satellite0 groundstation2 star5
1
20 0
3
0 18 6 1
0 22 -1 0
0 27 -1 1
0
end_operator
begin_operator
fix_turn_to satellite0 phenomenon3 groundstation1
1
20 0
3
0 18 0 2
0 21 -1 1
0 23 -1 0
0
end_operator
begin_operator
fix_turn_to satellite0 phenomenon3 groundstation2
1
20 0
3
0 18 1 2
0 22 -1 1
0 23 -1 0
0
end_operator
begin_operator
fix_turn_to satellite0 phenomenon3 phenomenon3
2
18 2
20 0
1
0 23 -1 0
0
end_operator
begin_operator
fix_turn_to satellite0 phenomenon3 phenomenon4
1
20 0
3
0 18 3 2
0 23 -1 0
0 24 -1 1
0
end_operator
begin_operator
fix_turn_to satellite0 phenomenon3 phenomenon6
1
20 0
3
0 18 4 2
0 23 -1 0
0 25 -1 1
0
end_operator
begin_operator
fix_turn_to satellite0 phenomenon3 star0
1
20 0
3
0 18 5 2
0 23 -1 0
0 26 -1 1
0
end_operator
begin_operator
fix_turn_to satellite0 phenomenon3 star5
1
20 0
3
0 18 6 2
0 23 -1 0
0 27 -1 1
0
end_operator
begin_operator
fix_turn_to satellite0 phenomenon4 groundstation1
1
20 0
3
0 18 0 3
0 21 -1 1
0 24 -1 0
0
end_operator
begin_operator
fix_turn_to satellite0 phenomenon4 groundstation2
1
20 0
3
0 18 1 3
0 22 -1 1
0 24 -1 0
0
end_operator
begin_operator
fix_turn_to satellite0 phenomenon4 phenomenon3
1
20 0
3
0 18 2 3
0 23 -1 1
0 24 -1 0
0
end_operator
begin_operator
fix_turn_to satellite0 phenomenon4 phenomenon4
2
18 3
20 0
1
0 24 -1 0
0
end_operator
begin_operator
fix_turn_to satellite0 phenomenon4 phenomenon6
1
20 0
3
0 18 4 3
0 24 -1 0
0 25 -1 1
0
end_operator
begin_operator
fix_turn_to satellite0 phenomenon4 star0
1
20 0
3
0 18 5 3
0 24 -1 0
0 26 -1 1
0
end_operator
begin_operator
fix_turn_to satellite0 phenomenon4 star5
1
20 0
3
0 18 6 3
0 24 -1 0
0 27 -1 1
0
end_operator
begin_operator
fix_turn_to satellite0 phenomenon6 groundstation1
1
20 0
3
0 18 0 4
0 21 -1 1
0 25 -1 0
0
end_operator
begin_operator
fix_turn_to satellite0 phenomenon6 groundstation2
1
20 0
3
0 18 1 4
0 22 -1 1
0 25 -1 0
0
end_operator
begin_operator
fix_turn_to satellite0 phenomenon6 phenomenon3
1
20 0
3
0 18 2 4
0 23 -1 1
0 25 -1 0
0
end_operator
begin_operator
fix_turn_to satellite0 phenomenon6 phenomenon4
1
20 0
3
0 18 3 4
0 24 -1 1
0 25 -1 0
0
end_operator
begin_operator
fix_turn_to satellite0 phenomenon6 phenomenon6
2
18 4
20 0
1
0 25 -1 0
0
end_operator
begin_operator
fix_turn_to satellite0 phenomenon6 star0
1
20 0
3
0 18 5 4
0 25 -1 0
0 26 -1 1
0
end_operator
begin_operator
fix_turn_to satellite0 phenomenon6 star5
1
20 0
3
0 18 6 4
0 25 -1 0
0 27 -1 1
0
end_operator
begin_operator
fix_turn_to satellite0 star0 groundstation1
1
20 0
3
0 18 0 5
0 21 -1 1
0 26 -1 0
0
end_operator
begin_operator
fix_turn_to satellite0 star0 groundstation2
1
20 0
3
0 18 1 5
0 22 -1 1
0 26 -1 0
0
end_operator
begin_operator
fix_turn_to satellite0 star0 phenomenon3
1
20 0
3
0 18 2 5
0 23 -1 1
0 26 -1 0
0
end_operator
begin_operator
fix_turn_to satellite0 star0 phenomenon4
1
20 0
3
0 18 3 5
0 24 -1 1
0 26 -1 0
0
end_operator
begin_operator
fix_turn_to satellite0 star0 phenomenon6
1
20 0
3
0 18 4 5
0 25 -1 1
0 26 -1 0
0
end_operator
begin_operator
fix_turn_to satellite0 star0 star0
2
18 5
20 0
1
0 26 -1 0
0
end_operator
begin_operator
fix_turn_to satellite0 star0 star5
1
20 0
3
0 18 6 5
0 26 -1 0
0 27 -1 1
0
end_operator
begin_operator
fix_turn_to satellite0 star5 groundstation1
1
20 0
3
0 18 0 6
0 21 -1 1
0 27 -1 0
0
end_operator
begin_operator
fix_turn_to satellite0 star5 groundstation2
1
20 0
3
0 18 1 6
0 22 -1 1
0 27 -1 0
0
end_operator
begin_operator
fix_turn_to satellite0 star5 phenomenon3
1
20 0
3
0 18 2 6
0 23 -1 1
0 27 -1 0
0
end_operator
begin_operator
fix_turn_to satellite0 star5 phenomenon4
1
20 0
3
0 18 3 6
0 24 -1 1
0 27 -1 0
0
end_operator
begin_operator
fix_turn_to satellite0 star5 phenomenon6
1
20 0
3
0 18 4 6
0 25 -1 1
0 27 -1 0
0
end_operator
begin_operator
fix_turn_to satellite0 star5 star0
1
20 0
3
0 18 5 6
0 26 -1 1
0 27 -1 0
0
end_operator
begin_operator
fix_turn_to satellite0 star5 star5
2
18 6
20 0
1
0 27 -1 0
0
end_operator
0
