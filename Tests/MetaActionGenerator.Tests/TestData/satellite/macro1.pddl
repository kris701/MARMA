(:action turn_to_mcr_take_image_mcr_turn_to
:parameters ( ?s - satellite ?d_new - direction ?d_prev - direction ?ix3 - instrument ?mx4 - mode)
:precondition (and (pointing ?s ?d_prev)(calibrated ?ix3)(on_board ?ix3 ?s)(supports ?ix3 ?mx4)(power_on ?ix3)(stag_have_image ?d_new ?mx4)(stag_pointing ?s ?d_prev))
:effect (and (have_image ?d_new ?mx4)(pointing ?s ?d_prev)(not (pointing ?s ?d_new)))
)