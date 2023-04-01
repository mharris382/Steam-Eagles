%%
[[Sprint 3]] #iteration #unity
%%
# Iteration 13:  preparing to pilot 
Previous Iteration: [[Iteration 12 - Jumping on Airship]]
Next Iteration: [[Iteration 14 -  Save Loading]]


## Goal
we need to be able to fly the airship around... and before that we need some supporting systems to manage the transition from flying to the normal game mode.

### Purpose
Create systems that are called by the character FSM which switch the player character *from normal mode to pilot mode* and also *from pilot mode to normal mode*

### Hypothesis/Plan

Several changes need to be made when switching to/from pilot mode.  
- the player input state needs to be changed to a different control group
- we need to switch to a camera that can see the exterior of the ship
- we need to hide the normal HUD and display an airship pilot HUD instead.
- finally we need to inject the player input into a flight control system so it blocks other players from trying to fly the ship while we are piloting it.

#### New Classes
- `PilotControlsInteractable` : interactable object which player interacts with to initiate the piloting state
- `PilotControls` :  data broker which acts as a go between for player input and the actual airship controller
	- *this will also come in handy when AI needs to fly the ship*
- `InUseControls` : disposable object that represents a single instance where an entity is using the pilot controls. 
	-  *it is possible for the controls to be taken away from the player by another system*
- `PilotControlManager` : manager object which is responsible for overseeing 
- `Airship` : data container MonoBehaviour that stores all the state information relavent to the airship
	- `isControlled` - true when there is a pilot controlling the airship


- `AirshipController`  - handles applying physics to the airship

----
## Result
![[movie_117.mp4]]




----
## Reflection



### What was learned or accomplished?
I was able to modify the player state machine to stop normal gameplay so that the player was able to fly the ship.  Done so through a airship controls object which was responsible for all communication from the pilot to the ship.  This layer of abstraction is nice because it allowed me to first implement the functionality with a null object (as pilot), then I passed in the character move input to that object turning the null piloot into a partially implemented pilot.   From there I could use the pilot controls object to implement the airship flight controller which had two modes of operation.  One mode *autopilot* if the controls object did not have a pilot assigned, and a second mode where it polled input from the pilot interface and applied forces to the airship rigidbodies in response.  

Currently the ship flight needs more ability to move vertically, but I don't think it should move vertically quite the same way that it is moving horizontally.  I also think some linear drag could be nice to add.  Anyway the flight gameplay will obviously take a lot of tweaking , but it's good that the foundation works so nicely.


### Where to go now?
At some point I'll want to really iterate hard on the flight mechanics, but that should be a lower priority.  The fact that there is flight ready, means the player is nearly ready to go explore the world outside the ship (will still need docking before then), but now we should get everything inside the ship working.  That would be the tools, machines, and the power/gas systems.

I need to also think about how and when I want to setup the rooms system so that it becomes functional in gameplay.  Atm it is mostly editor code so that it is useable in game, and I purposely held off deciding how the runtime side of that would be implemented because it is an optimization and according to POO, I should put off optimizations until they become absolutely necessary.

The Tool UI that I designed should be next up I think...
