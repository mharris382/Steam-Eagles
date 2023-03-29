%%
[[Sprint 3]] #iteration #unity
%%
# Iteration 14:  Save Loading
Previous Iteration: [[Iteration 13]]
Next Iteration: [[Iteration 15]]


I've been feeling very anxious today for some reason, and haven't been able to get much work done.  Every time I sat down to begin working, I just ended up not being able to start on either the persistance manager or the airship pilot systems.  As for the airship pilot systems, I haven't quite figured out what the UI for that is going to be, partially because I haven't yet made a prototype for that.  Well I did at one point make a prototype for balloon physics, but not for the actual player interface.


## Goal
create persistence manager
### Purpose

### Hypothesis
save files using a folder for each save

----
## Result
![[continue button pt1.mp4]]
![[continue button pt2.mp4]]





----
## Reflection
wrote a test script that would allow me to save my player's position with a button press.  That test script will be replaced shortly with save events that will be triggered (initially) just from the pause menu.


### What was learned or accomplished?
I need to refactor the player input so that it is more decoupled from the game systems.  Currently the player doesn't get assigned until a button is pressed in play mode.  I need to make it so that player device assignments are allowed to happen from the main menu, and then when the game is started and a gameplay scene is loaded the game manager will ask the device manager for either 1 or 2 devices depending on whether the player is in single or coop modes.  Since this will be able to be changed from the pause menu, the game manager may at a later time poll the device manager for players in the case where a button for second device hasnt been detected yet.

### Where to go now?

I just started on that bug, but I think I should hold off on it, and get to work on the inventory stuff.  I already started working on the foundations for the backend, so now I need to make the frontend in unity.  

I think I'm gonna need to start from scratch on this one mostly, tho I can use the UI setup scene as a reference point.

Before I actually start the next iteration I should get my ui mockup sketches out on paper.  That way they are right in front of me when I start.