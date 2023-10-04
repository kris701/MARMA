begin_version
3
end_version
begin_metric
0
end_metric
28
begin_variable
var0
-1
3
Atom at(ball1, rooma)
Atom at(ball1, roomb)
<none of those>
end_variable
begin_variable
var1
-1
3
Atom at(ball2, rooma)
Atom at(ball2, roomb)
<none of those>
end_variable
begin_variable
var2
-1
3
Atom at(ball3, rooma)
Atom at(ball3, roomb)
<none of those>
end_variable
begin_variable
var3
-1
3
Atom at(ball4, rooma)
Atom at(ball4, roomb)
<none of those>
end_variable
begin_variable
var4
-1
2
Atom at-robby(rooma)
Atom at-robby(roomb)
end_variable
begin_variable
var5
-1
5
Atom carry(ball1, left)
Atom carry(ball2, left)
Atom carry(ball3, left)
Atom carry(ball4, left)
Atom free(left)
end_variable
begin_variable
var6
-1
5
Atom carry(ball1, right)
Atom carry(ball2, right)
Atom carry(ball3, right)
Atom carry(ball4, right)
Atom free(right)
end_variable
begin_variable
var7
-1
2
Atom is-goal-at(ball1, roomb)
NegatedAtom is-goal-at(ball1, roomb)
end_variable
begin_variable
var8
-1
2
Atom is-goal-at(ball2, roomb)
NegatedAtom is-goal-at(ball2, roomb)
end_variable
begin_variable
var9
-1
2
Atom is-goal-at(ball3, roomb)
NegatedAtom is-goal-at(ball3, roomb)
end_variable
begin_variable
var10
-1
2
Atom is-goal-at(ball4, roomb)
NegatedAtom is-goal-at(ball4, roomb)
end_variable
begin_variable
var11
-1
2
Atom is-goal-at-robby(roomb)
NegatedAtom is-goal-at-robby(roomb)
end_variable
begin_variable
var12
-1
2
Atom is-goal-carry(ball1, left)
NegatedAtom is-goal-carry(ball1, left)
end_variable
begin_variable
var13
-1
2
Atom is-goal-carry(ball1, right)
NegatedAtom is-goal-carry(ball1, right)
end_variable
begin_variable
var14
-1
2
Atom is-goal-carry(ball2, left)
NegatedAtom is-goal-carry(ball2, left)
end_variable
begin_variable
var15
-1
2
Atom is-goal-carry(ball2, right)
NegatedAtom is-goal-carry(ball2, right)
end_variable
begin_variable
var16
-1
2
Atom is-goal-carry(ball3, left)
NegatedAtom is-goal-carry(ball3, left)
end_variable
begin_variable
var17
-1
2
Atom is-goal-carry(ball3, right)
NegatedAtom is-goal-carry(ball3, right)
end_variable
begin_variable
var18
-1
2
Atom is-goal-carry(ball4, left)
NegatedAtom is-goal-carry(ball4, left)
end_variable
begin_variable
var19
-1
2
Atom is-goal-carry(ball4, right)
NegatedAtom is-goal-carry(ball4, right)
end_variable
begin_variable
var20
-1
3
Atom leader-state-at(ball1, rooma)
Atom leader-state-at(ball1, roomb)
<none of those>
end_variable
begin_variable
var21
-1
3
Atom leader-state-at(ball2, rooma)
Atom leader-state-at(ball2, roomb)
<none of those>
end_variable
begin_variable
var22
-1
3
Atom leader-state-at(ball3, rooma)
Atom leader-state-at(ball3, roomb)
<none of those>
end_variable
begin_variable
var23
-1
3
Atom leader-state-at(ball4, rooma)
Atom leader-state-at(ball4, roomb)
<none of those>
end_variable
begin_variable
var24
-1
2
Atom leader-state-at-robby(rooma)
Atom leader-state-at-robby(roomb)
end_variable
begin_variable
var25
-1
5
Atom leader-state-carry(ball1, left)
Atom leader-state-carry(ball2, left)
Atom leader-state-carry(ball3, left)
Atom leader-state-carry(ball4, left)
Atom leader-state-free(left)
end_variable
begin_variable
var26
-1
5
Atom leader-state-carry(ball1, right)
Atom leader-state-carry(ball2, right)
Atom leader-state-carry(ball3, right)
Atom leader-state-carry(ball4, right)
Atom leader-state-free(right)
end_variable
begin_variable
var27
-1
2
Atom leader-turn()
NegatedAtom leader-turn()
end_variable
14
begin_mutex_group
4
0 0
0 1
5 0
6 0
end_mutex_group
begin_mutex_group
4
1 0
1 1
5 1
6 1
end_mutex_group
begin_mutex_group
4
2 0
2 1
5 2
6 2
end_mutex_group
begin_mutex_group
4
3 0
3 1
5 3
6 3
end_mutex_group
begin_mutex_group
2
4 0
4 1
end_mutex_group
begin_mutex_group
5
5 0
5 1
5 2
5 3
5 4
end_mutex_group
begin_mutex_group
5
6 0
6 1
6 2
6 3
6 4
end_mutex_group
begin_mutex_group
4
20 0
20 1
25 0
26 0
end_mutex_group
begin_mutex_group
4
21 0
21 1
25 1
26 1
end_mutex_group
begin_mutex_group
4
22 0
22 1
25 2
26 2
end_mutex_group
begin_mutex_group
4
23 0
23 1
25 3
26 3
end_mutex_group
begin_mutex_group
2
24 0
24 1
end_mutex_group
begin_mutex_group
5
25 0
25 1
25 2
25 3
25 4
end_mutex_group
begin_mutex_group
5
26 0
26 1
26 2
26 3
26 4
end_mutex_group
begin_state
0
0
0
0
0
4
4
1
1
1
1
1
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
0
0
0
4
4
0
end_state
begin_goal
13
7 0
8 0
9 0
10 0
11 0
12 0
13 0
14 0
15 0
16 0
17 0
18 0
19 0
end_goal
105
begin_operator
attack_drop ball1 rooma left
2
4 0
27 1
2
0 0 -1 0
0 5 0 4
0
end_operator
begin_operator
attack_drop ball1 rooma right
2
4 0
27 1
2
0 0 -1 0
0 6 0 4
0
end_operator
begin_operator
attack_drop ball1 roomb left
2
4 1
27 1
2
0 0 -1 1
0 5 0 4
0
end_operator
begin_operator
attack_drop ball1 roomb right
2
4 1
27 1
2
0 0 -1 1
0 6 0 4
0
end_operator
begin_operator
attack_drop ball2 rooma left
2
4 0
27 1
2
0 1 -1 0
0 5 1 4
0
end_operator
begin_operator
attack_drop ball2 rooma right
2
4 0
27 1
2
0 1 -1 0
0 6 1 4
0
end_operator
begin_operator
attack_drop ball2 roomb left
2
4 1
27 1
2
0 1 -1 1
0 5 1 4
0
end_operator
begin_operator
attack_drop ball2 roomb right
2
4 1
27 1
2
0 1 -1 1
0 6 1 4
0
end_operator
begin_operator
attack_drop ball3 rooma left
2
4 0
27 1
2
0 2 -1 0
0 5 2 4
0
end_operator
begin_operator
attack_drop ball3 rooma right
2
4 0
27 1
2
0 2 -1 0
0 6 2 4
0
end_operator
begin_operator
attack_drop ball3 roomb left
2
4 1
27 1
2
0 2 -1 1
0 5 2 4
0
end_operator
begin_operator
attack_drop ball3 roomb right
2
4 1
27 1
2
0 2 -1 1
0 6 2 4
0
end_operator
begin_operator
attack_drop ball4 rooma left
2
4 0
27 1
2
0 3 -1 0
0 5 3 4
0
end_operator
begin_operator
attack_drop ball4 rooma right
2
4 0
27 1
2
0 3 -1 0
0 6 3 4
0
end_operator
begin_operator
attack_drop ball4 roomb left
2
4 1
27 1
2
0 3 -1 1
0 5 3 4
0
end_operator
begin_operator
attack_drop ball4 roomb right
2
4 1
27 1
2
0 3 -1 1
0 6 3 4
0
end_operator
begin_operator
attack_drop_goal ball1 rooma left
4
4 0
20 0
25 4
27 1
2
0 0 -1 0
0 5 0 4
0
end_operator
begin_operator
attack_drop_goal ball1 rooma right
4
4 0
20 0
26 4
27 1
2
0 0 -1 0
0 6 0 4
0
end_operator
begin_operator
attack_drop_goal ball1 roomb left
4
4 1
20 1
25 4
27 1
3
0 0 -1 1
0 5 0 4
0 7 -1 0
0
end_operator
begin_operator
attack_drop_goal ball1 roomb right
4
4 1
20 1
26 4
27 1
3
0 0 -1 1
0 6 0 4
0 7 -1 0
0
end_operator
begin_operator
attack_drop_goal ball2 rooma left
4
4 0
21 0
25 4
27 1
2
0 1 -1 0
0 5 1 4
0
end_operator
begin_operator
attack_drop_goal ball2 rooma right
4
4 0
21 0
26 4
27 1
2
0 1 -1 0
0 6 1 4
0
end_operator
begin_operator
attack_drop_goal ball2 roomb left
4
4 1
21 1
25 4
27 1
3
0 1 -1 1
0 5 1 4
0 8 -1 0
0
end_operator
begin_operator
attack_drop_goal ball2 roomb right
4
4 1
21 1
26 4
27 1
3
0 1 -1 1
0 6 1 4
0 8 -1 0
0
end_operator
begin_operator
attack_drop_goal ball3 rooma left
4
4 0
22 0
25 4
27 1
2
0 2 -1 0
0 5 2 4
0
end_operator
begin_operator
attack_drop_goal ball3 rooma right
4
4 0
22 0
26 4
27 1
2
0 2 -1 0
0 6 2 4
0
end_operator
begin_operator
attack_drop_goal ball3 roomb left
4
4 1
22 1
25 4
27 1
3
0 2 -1 1
0 5 2 4
0 9 -1 0
0
end_operator
begin_operator
attack_drop_goal ball3 roomb right
4
4 1
22 1
26 4
27 1
3
0 2 -1 1
0 6 2 4
0 9 -1 0
0
end_operator
begin_operator
attack_drop_goal ball4 rooma left
4
4 0
23 0
25 4
27 1
2
0 3 -1 0
0 5 3 4
0
end_operator
begin_operator
attack_drop_goal ball4 rooma right
4
4 0
23 0
26 4
27 1
2
0 3 -1 0
0 6 3 4
0
end_operator
begin_operator
attack_drop_goal ball4 roomb left
4
4 1
23 1
25 4
27 1
3
0 3 -1 1
0 5 3 4
0 10 -1 0
0
end_operator
begin_operator
attack_drop_goal ball4 roomb right
4
4 1
23 1
26 4
27 1
3
0 3 -1 1
0 6 3 4
0 10 -1 0
0
end_operator
begin_operator
attack_move rooma roomb
1
27 1
1
0 4 0 1
0
end_operator
begin_operator
attack_move roomb rooma
1
27 1
1
0 4 1 0
0
end_operator
begin_operator
attack_move_goal rooma roomb
2
24 1
27 1
2
0 4 0 1
0 11 -1 0
0
end_operator
begin_operator
attack_move_goal roomb rooma
2
24 0
27 1
1
0 4 1 0
0
end_operator
begin_operator
attack_move_goal roomb roomb
3
4 1
24 1
27 1
1
0 11 -1 0
0
end_operator
begin_operator
attack_pick ball1 rooma left
2
4 0
27 1
2
0 0 0 2
0 5 4 0
0
end_operator
begin_operator
attack_pick ball1 rooma right
2
4 0
27 1
2
0 0 0 2
0 6 4 0
0
end_operator
begin_operator
attack_pick ball1 roomb left
2
4 1
27 1
2
0 0 1 2
0 5 4 0
0
end_operator
begin_operator
attack_pick ball1 roomb right
2
4 1
27 1
2
0 0 1 2
0 6 4 0
0
end_operator
begin_operator
attack_pick ball2 rooma left
2
4 0
27 1
2
0 1 0 2
0 5 4 1
0
end_operator
begin_operator
attack_pick ball2 rooma right
2
4 0
27 1
2
0 1 0 2
0 6 4 1
0
end_operator
begin_operator
attack_pick ball2 roomb left
2
4 1
27 1
2
0 1 1 2
0 5 4 1
0
end_operator
begin_operator
attack_pick ball2 roomb right
2
4 1
27 1
2
0 1 1 2
0 6 4 1
0
end_operator
begin_operator
attack_pick ball3 rooma left
2
4 0
27 1
2
0 2 0 2
0 5 4 2
0
end_operator
begin_operator
attack_pick ball3 rooma right
2
4 0
27 1
2
0 2 0 2
0 6 4 2
0
end_operator
begin_operator
attack_pick ball3 roomb left
2
4 1
27 1
2
0 2 1 2
0 5 4 2
0
end_operator
begin_operator
attack_pick ball3 roomb right
2
4 1
27 1
2
0 2 1 2
0 6 4 2
0
end_operator
begin_operator
attack_pick ball4 rooma left
2
4 0
27 1
2
0 3 0 2
0 5 4 3
0
end_operator
begin_operator
attack_pick ball4 rooma right
2
4 0
27 1
2
0 3 0 2
0 6 4 3
0
end_operator
begin_operator
attack_pick ball4 roomb left
2
4 1
27 1
2
0 3 1 2
0 5 4 3
0
end_operator
begin_operator
attack_pick ball4 roomb right
2
4 1
27 1
2
0 3 1 2
0 6 4 3
0
end_operator
begin_operator
attack_pick_goal ball1 rooma left
3
4 0
25 0
27 1
3
0 0 0 2
0 5 4 0
0 12 -1 0
0
end_operator
begin_operator
attack_pick_goal ball1 rooma right
3
4 0
26 0
27 1
3
0 0 0 2
0 6 4 0
0 13 -1 0
0
end_operator
begin_operator
attack_pick_goal ball1 roomb left
3
4 1
25 0
27 1
3
0 0 1 2
0 5 4 0
0 12 -1 0
0
end_operator
begin_operator
attack_pick_goal ball1 roomb right
3
4 1
26 0
27 1
3
0 0 1 2
0 6 4 0
0 13 -1 0
0
end_operator
begin_operator
attack_pick_goal ball2 rooma left
3
4 0
25 1
27 1
3
0 1 0 2
0 5 4 1
0 14 -1 0
0
end_operator
begin_operator
attack_pick_goal ball2 rooma right
3
4 0
26 1
27 1
3
0 1 0 2
0 6 4 1
0 15 -1 0
0
end_operator
begin_operator
attack_pick_goal ball2 roomb left
3
4 1
25 1
27 1
3
0 1 1 2
0 5 4 1
0 14 -1 0
0
end_operator
begin_operator
attack_pick_goal ball2 roomb right
3
4 1
26 1
27 1
3
0 1 1 2
0 6 4 1
0 15 -1 0
0
end_operator
begin_operator
attack_pick_goal ball3 rooma left
3
4 0
25 2
27 1
3
0 2 0 2
0 5 4 2
0 16 -1 0
0
end_operator
begin_operator
attack_pick_goal ball3 rooma right
3
4 0
26 2
27 1
3
0 2 0 2
0 6 4 2
0 17 -1 0
0
end_operator
begin_operator
attack_pick_goal ball3 roomb left
3
4 1
25 2
27 1
3
0 2 1 2
0 5 4 2
0 16 -1 0
0
end_operator
begin_operator
attack_pick_goal ball3 roomb right
3
4 1
26 2
27 1
3
0 2 1 2
0 6 4 2
0 17 -1 0
0
end_operator
begin_operator
attack_pick_goal ball4 rooma left
3
4 0
25 3
27 1
3
0 3 0 2
0 5 4 3
0 18 -1 0
0
end_operator
begin_operator
attack_pick_goal ball4 rooma right
3
4 0
26 3
27 1
3
0 3 0 2
0 6 4 3
0 19 -1 0
0
end_operator
begin_operator
attack_pick_goal ball4 roomb left
3
4 1
25 3
27 1
3
0 3 1 2
0 5 4 3
0 18 -1 0
0
end_operator
begin_operator
attack_pick_goal ball4 roomb right
3
4 1
26 3
27 1
3
0 3 1 2
0 6 4 3
0 19 -1 0
0
end_operator
begin_operator
attack_reach-goal 
1
27 0
13
0 7 -1 0
0 8 -1 0
0 9 -1 0
0 10 -1 0
0 11 -1 0
0 12 -1 0
0 13 -1 0
0 14 -1 0
0 15 -1 0
0 16 -1 0
0 17 -1 0
0 18 -1 0
0 19 -1 0
0
end_operator
begin_operator
fix_drop ball1 rooma left
2
24 0
27 0
2
0 20 -1 0
0 25 0 4
0
end_operator
begin_operator
fix_drop ball1 rooma right
2
24 0
27 0
2
0 20 -1 0
0 26 0 4
0
end_operator
begin_operator
fix_drop ball1 roomb left
2
24 1
27 0
2
0 20 -1 1
0 25 0 4
0
end_operator
begin_operator
fix_drop ball1 roomb right
2
24 1
27 0
2
0 20 -1 1
0 26 0 4
0
end_operator
begin_operator
fix_drop ball2 rooma left
2
24 0
27 0
2
0 21 -1 0
0 25 1 4
0
end_operator
begin_operator
fix_drop ball2 rooma right
2
24 0
27 0
2
0 21 -1 0
0 26 1 4
0
end_operator
begin_operator
fix_drop ball2 roomb left
2
24 1
27 0
2
0 21 -1 1
0 25 1 4
0
end_operator
begin_operator
fix_drop ball2 roomb right
2
24 1
27 0
2
0 21 -1 1
0 26 1 4
0
end_operator
begin_operator
fix_drop ball3 rooma left
2
24 0
27 0
2
0 22 -1 0
0 25 2 4
0
end_operator
begin_operator
fix_drop ball3 rooma right
2
24 0
27 0
2
0 22 -1 0
0 26 2 4
0
end_operator
begin_operator
fix_drop ball3 roomb left
2
24 1
27 0
2
0 22 -1 1
0 25 2 4
0
end_operator
begin_operator
fix_drop ball3 roomb right
2
24 1
27 0
2
0 22 -1 1
0 26 2 4
0
end_operator
begin_operator
fix_drop ball4 rooma left
2
24 0
27 0
2
0 23 -1 0
0 25 3 4
0
end_operator
begin_operator
fix_drop ball4 rooma right
2
24 0
27 0
2
0 23 -1 0
0 26 3 4
0
end_operator
begin_operator
fix_drop ball4 roomb left
2
24 1
27 0
2
0 23 -1 1
0 25 3 4
0
end_operator
begin_operator
fix_drop ball4 roomb right
2
24 1
27 0
2
0 23 -1 1
0 26 3 4
0
end_operator
begin_operator
fix_move rooma roomb
1
27 0
1
0 24 0 1
0
end_operator
begin_operator
fix_move roomb rooma
1
27 0
1
0 24 1 0
0
end_operator
begin_operator
fix_pass-turn 
0
42
1 20 0 7 -1 0
1 20 2 7 -1 0
1 21 0 8 -1 0
1 21 2 8 -1 0
1 22 0 9 -1 0
1 22 2 9 -1 0
1 23 0 10 -1 0
1 23 2 10 -1 0
1 24 0 11 -1 0
1 25 1 12 -1 0
1 25 2 12 -1 0
1 25 3 12 -1 0
1 25 4 12 -1 0
1 26 1 13 -1 0
1 26 2 13 -1 0
1 26 3 13 -1 0
1 26 4 13 -1 0
1 25 0 14 -1 0
1 25 2 14 -1 0
1 25 3 14 -1 0
1 25 4 14 -1 0
1 26 0 15 -1 0
1 26 2 15 -1 0
1 26 3 15 -1 0
1 26 4 15 -1 0
1 25 0 16 -1 0
1 25 1 16 -1 0
1 25 3 16 -1 0
1 25 4 16 -1 0
1 26 0 17 -1 0
1 26 1 17 -1 0
1 26 3 17 -1 0
1 26 4 17 -1 0
1 25 0 18 -1 0
1 25 1 18 -1 0
1 25 2 18 -1 0
1 25 4 18 -1 0
1 26 0 19 -1 0
1 26 1 19 -1 0
1 26 2 19 -1 0
1 26 4 19 -1 0
0 27 0 1
0
end_operator
begin_operator
fix_pick ball1 rooma left
2
24 0
27 0
2
0 20 0 2
0 25 4 0
0
end_operator
begin_operator
fix_pick ball1 rooma right
2
24 0
27 0
2
0 20 0 2
0 26 4 0
0
end_operator
begin_operator
fix_pick ball1 roomb left
2
24 1
27 0
2
0 20 1 2
0 25 4 0
0
end_operator
begin_operator
fix_pick ball1 roomb right
2
24 1
27 0
2
0 20 1 2
0 26 4 0
0
end_operator
begin_operator
fix_pick ball2 rooma left
2
24 0
27 0
2
0 21 0 2
0 25 4 1
0
end_operator
begin_operator
fix_pick ball2 rooma right
2
24 0
27 0
2
0 21 0 2
0 26 4 1
0
end_operator
begin_operator
fix_pick ball2 roomb left
2
24 1
27 0
2
0 21 1 2
0 25 4 1
0
end_operator
begin_operator
fix_pick ball2 roomb right
2
24 1
27 0
2
0 21 1 2
0 26 4 1
0
end_operator
begin_operator
fix_pick ball3 rooma left
2
24 0
27 0
2
0 22 0 2
0 25 4 2
0
end_operator
begin_operator
fix_pick ball3 rooma right
2
24 0
27 0
2
0 22 0 2
0 26 4 2
0
end_operator
begin_operator
fix_pick ball3 roomb left
2
24 1
27 0
2
0 22 1 2
0 25 4 2
0
end_operator
begin_operator
fix_pick ball3 roomb right
2
24 1
27 0
2
0 22 1 2
0 26 4 2
0
end_operator
begin_operator
fix_pick ball4 rooma left
2
24 0
27 0
2
0 23 0 2
0 25 4 3
0
end_operator
begin_operator
fix_pick ball4 rooma right
2
24 0
27 0
2
0 23 0 2
0 26 4 3
0
end_operator
begin_operator
fix_pick ball4 roomb left
2
24 1
27 0
2
0 23 1 2
0 25 4 3
0
end_operator
begin_operator
fix_pick ball4 roomb right
2
24 1
27 0
2
0 23 1 2
0 26 4 3
0
end_operator
0
