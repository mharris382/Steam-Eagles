#systems-design 
seealso: [[Player Assembly]], [[Player Systems]], [[Player Assembly Class Diagram.canvas]]

The current game manager setup is buggy and clunky to work with.  There are several data containers which are used to bind the various player systems to a specific local player instance.  This system currently relies heavily on the input system for initialization.   


# Required Changes / Improvements
- character should be able to exist in a scene prior to being bound to an input device
- characters should be able to be spawned, with or without an input player instance having been assigned
- player-character assignments should not depend on an input device (in other words,  before the player has hit a button we should still know which character each player controls)
	- player-character assignments should be persistent and saveable/loadable
	- in the case where players have not been assigned a character, the game manager should initialize the [[character selection menu]]


