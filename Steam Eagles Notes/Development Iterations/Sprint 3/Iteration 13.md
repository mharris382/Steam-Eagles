%%
[[Sprint 3]] #iteration #unity
%%
# Iteration 13:  preparing to pilot 
Previous Iteration: [[Iteration 12]]
Next Iteration: 


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





----
## Reflection



### What was learned or accomplished?


### Where to go now?

