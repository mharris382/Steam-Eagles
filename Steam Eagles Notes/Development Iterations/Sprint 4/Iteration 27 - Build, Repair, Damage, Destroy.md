%%
[[Sprint 4]] #iteration #unity
%%
# Iteration 27:
Previous Iteration: [[Iteration 26 - Replacing Global Signals]]
Next Iteration: 


## Goal


### Purpose
Understand the relationship between the following game actions and the objects which those action act upon
- Game Actions
	- Build
	- Repair
	- Damage
	- Destroy
- Objects which are related (in any way) to these actions
	- Building
		- Tilemaps
		- EditableTile
			- 
	- 

### Hypothesis


----
## Result

One thing from inspection of the Building system is that the custom rule tile class `EditableTile`, which is used to render the different tiles that can be built or destroyed, must be resolved by the process that wants to do the building.  This is because the entry point is actually the Tilemaps themselves, (*through the `BuildingMap`* class API).  So far that has not been an issue, because the only system currently building/destroying is the tool system and it uses it's `Recipe` instance to resolve the Tile that will be built.  

My initial plan for repairing/damaging was to create a nested class `RepairableTile` and `DamageableTile` which each store references to the other.  

However, I now believe that relying on the actual Tile classes to implement core game logic is a bad idea.  Those classes already have quite a bit of logic already in them to handle the rule tile logic.  Additionally the use of tiles directly means that the save/load data is storing asset GUIDs, which may be one of the reasons the save game data breaks occassionally, specificially the tile data is lost.

I would like a new method for saving and loading the tiles into the tilemaps, that does not actually rely on the tiles directly, and instead relies on an abstract TileFactory which resolves the correct tile given a high level object representing the core game state.  So rather than representing the core logic as tiles, we will now represent the core logic as a `BuildingTile` which fundamentally must be resolve the TileBase (*through addressables?*).


----
## Reflection



### What was learned or accomplished?


### Where to go now?

