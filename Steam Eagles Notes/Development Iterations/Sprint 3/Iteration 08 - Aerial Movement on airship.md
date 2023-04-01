%%
[[Sprint 3]] #iteration #unity
%%
# Iteration 08: Aerial Movement on airship
Previous Iteration: [[Iteration 07 - New Player Movement Mode on airship]]
Next Iteration: [[Iteration 09 - Climbing Movement]]


## Goal
Fix the character's aerial movement when they are onboard the airship.

### Purpose
Figure out how to apply the technique used to solve the grounded movement in the previous iteration to aerial movement.

### Hypothesis
do the same thing that worked for grounded movement for aerial movement


----
## Result
This did work. I didnt' actually spend too long, so it might have been my own faultin implementing not the idea itself.  However, not only did it not function correctly, the working ground movement also ended up broken. In fact the character became completely unable to move at all.  So of course I stashed the changes and now we are back to where we started




----
## Reflection
I didn't actually do a reflection for this work, because since I failed, my first instinct was to say, "Oh that didn't work, I guess I have to try again before I fill out the iteration document."  I didn't really feel like trying again was a top priority over some of the design problems , so I didn't fill out the form. 




### What was learned or accomplished?
However, it just now occured to me that I still should have filled out the form because doing so would have helped me realize that there was a better, faster solution to this issue.  



### Where to go now?
The issue I was trying to solve here, was not in fact jumping, or even aerial movement at all.  Those mechanics are fundamental to execution based platform challenges, but the core gameplay of Steam Eagles isn't actually execution based, it's building based (base building, hahaha pun).  The real problem I was trying to address was vertical movement within the airship.  

The whole fixed joint solution works very naturally for grounded movement, but conflicts with gravity; making it less natural for jumping and falling.  However, climbing is an alternative form of vertical movement which does not conflict naturally with the fixed joint solution...


