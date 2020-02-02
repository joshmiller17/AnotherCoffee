README

Assumptions baked into the script JSON:
- for choices, if no option is picked after the timeout time, goto <event_id>_0
- for dialogue options chosen, goto <event_id>_<index> UNLESS there is an explicit goto
- effects like awkward and tension are deltas, as in += x