%%
[[Sprint 3]] #iteration #unity
%%
# Iteration 11: Persistence Manager
Previous Iteration: [[Iteration 10]]
Next Iteration: [[Iteration 12]]


## Goal

All the various game save data (such as spawn positions) must be linked to a single game save.  However, a single game save may more than one actual save state associated with it.  What this means is that each save file may end up with it's own folder in the save file path.  

### Purpose
Make the new game and load game buttons functional in the main menu scene.

### Hypothesis
So basically what we want to do is first check to see if we have any save files already on this machine.   If we do not, the load game button needs to be disabled.  

If the player chooses the new game button then we need to initialize the process of starting a new game and then loading the appropriate scene(s).

 ```ad-note
title: Note on Multiplayer

For the time being we will not be concerned with any multiplayer functionality and assume the player is playing in singleplayer mode.  Eventually there will be a second menu where the player chooses to launch in co-op or singleplayer mode.
```

Once the game has started we will need to also add a save game button to the pause menu.   That menu will show the player the list of saves, and the player can choose to overwrite a previous save or create a new one.  

The save file should give the player some information about where that save is in the playthrough. This can include:
- ~~the character being controlled by player 1 (p2 doesn't matter for this part)~~
- **how long this save file has been played**
- **The current primary quest**
- The states of each character showing the tools in their toolbelt, health (inventory?)
- name of the tools each character has equipped in their toolbelt

### Information to load into the scene
- editable tilemaps
- character controlled by player 1 + player character position
- (if has 2 inputs) character controlled by player 2 + player 2 character position
- item stacks in character's inventory
- tools in toolbelt
- position of the airship
- **machines built on the ship**
- pipe network state
- gas simulation state

----
## Result





----
## Reflection



### What was learned or accomplished?


### Where to go now?

