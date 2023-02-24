#systems-design #game-design 

local multiplayer has several far reaching system ramifications 

# Input Devices
_How should we handle input devices in co-op vs singleplay?_

so lets say Unity detects two connected device schemes (keyboard & mouse, and a controller).  The player has not yet chosen whether they are playing multiplayer or singleplayer.  There are several ways we could handle this situation

1. assume singleplayer has connected a second device
	1. commit to the first control scheme detected
	2. ignore second device scheme.
	3. Scheme can be swapped in pause menu
2. assume singleplayer has connected a second device
	1. switch control to the most recently used device
3. assume there is a new local player and immediately switch to local multiplayer mode
4. assume neither and prompt the player whether the new device is another player or not

So there are pros and cons to each option here.  The first two options both assume singleplayer but handle them differently.  Option 1 locks the player to a specific device, as if they were in co-op mode.  Option 2 allows a single player to switch back and forth between multiple devices instantly.

Option 1 is easier to implement and makes it more natural for the game to introduce a second local player into gameplay.  Option 2 is more complex to implement, but is better for players in singleplayer mode.  Option 3 is the simplest way to implement local co-op alongside singleplayer (this is how the game is currently implemented).  Option 4 gives the player a choice, but is also more complex to implement.


### Device Disconnect
*How should it be handled when a player's device is disconnected?*

1. handle differently in singleplayer vs local co-op
	1. in singleplayer, switch to keyboard and mouse when controller is disconnected
	2. in co-op, pause game and display device disconnect message
2. pause game and notify player that their device was disconnected

# Characters
_How should we handle player characters in singleplayer mode vs multiplayer mode.  _

1. **allow character switching in both single and multiplayer modes**
	2. both players should agree in multiplayer
2. **do not allow character switching ever**, give all abilities to builder character in singleplayer mode
3. **allow character switching in singleplayer mode**, do not allow character switching in multiplayer mode

option 1 gives players the most control, and lessens the gameplay difference between singleplayer mode and multiplayer mode (since gameplay mechanics are not modified, and one player has to do both roles, or alternatively allow an AI to control the second character in that case).  Option 2 is my least favorite since it gives the player the least control, and doesn't allow easy transition to-from multiplayer mode.  Option 3 seems less appealing but assuming that the character abilities are different (at least at first) has the advantage of forcing the keyboard & mouse player to have control of the abilities which are most natural for mouse, while the other character can have control of the abilities most natural for the controller.


# Menu Systems
_How to handle pausing in local co-op?_

## Pause Menu

1. gameplay is paused, both players share control over a single full screen menu
2. Both players have their own pause menu, gameplay is never paused
3. Both players have their own pause menu, gameplay is not paused unless both player have their pause menu open
4. gameplay is paused, only one pause menu can be open concurrently, the player who initiated the pause has control of the menu

## Selection Wheel
*If the selection wheel cannot pause gameplay in co-op, should is also not pause gameplay in singleplayer for consistency purposes?*

# Camera Systems
_How should camera systems be adjusted for local co-op vs singlepalyer gameplay?_

1. splitscreen camera static, always use splitscreen in local multiplayer mode
2. splitscreen dynamic.  when possible use one camera when both characters can be seen on the same camera (without overly distorting view) otherwise defer to splitscreen
3. Multimonitor splitscreen.  If the computer is connected to multiple monitors, display each player's camera on a different monitor (providing the same experience as singleplayer on a single machine)
4. implement all options and allow players to choose which camera mode they want from the menu

option 1 is simple to implement, but option 2 often provides the best user experience.  Option 3 is ideal, but only works if there is a second monitor.
