%%
[[Sprint 3]] #iteration #unity
%%
# Iteration 07: New Player Movement Mode on airship
Previous Iteration: 
Next Iteration: 


## Goal
Ensure that when a player is standing on a surface, they remain attached to that surface until they jump or fall off the edge, regardless of how the connected dynamic rigidbody moves.  

### Purpose

![[movie_107.mp4]]
Fix this behavior when airship is moving vertically.  Character should not bounce at all when they are standing on a surface.  

### Hypothesis

Several solutions to this have already been attempted, none of which perfectly solved the problem.  The main issue is that the character rigidbody must always remain dynamic or it will break the physics of the airship (which is also dynamic).   The ideal solution would be to take the player character out of dynamic physics but since that is not possible, I came up with a way to accomplish the same thing while still keeping the character's body dynamic.  By adding a fixed joint to the player's rigidbody which is attached to the airship, the body won't be able to move dynamically.  Then instead of moving the character by setting the velocity, the character will be moved by moving the joint's connected anchor position.

An unscripted test in the editor showed promising results from this method already.  
![[movie_109.mp4]]
This is merely the results of me sliding the connected anchor position's x value in the inspector.  

This will be a major change in the character's movement system, though I don't think it needs to replace the current system entirely.  Instead I plan on creating 2 new state machines in the character's FSM.  The current movement system will be entirely nested in one, the other will contain the new movement system implementation.

Several parts of the original movement code may still be used in the new movement system, as they don't rely on the rigidbody to function and the logic will be the same.
- code raycasting to detect ground and slopes will be the same
- code computing the desired velocity vector will be the same
- *now that I think about it really the only change will be the way that the velocity is applied to the body*




----
## Result





----
## Reflection



### What was learned or accomplished?


### Where to go now?

