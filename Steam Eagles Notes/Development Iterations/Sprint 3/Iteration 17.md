%%
[[Sprint 3]] #iteration #unity
%%
# Iteration 17: Refactoring 
Previous Iteration: [[Iteration 16]]
Next Iteration: [[Iteration 18]]


## Goal
basic refactoring cycle.  

### Plan
targeted assemblies: [[Building Assembly]], Rooms



----
## Result

- [ ] identify and delete all unused classes
- [ ] group scripts in subfolder/namespace
	- [ ] **new namespace** `Building.MyEditor`
	- [ ] **new namespace** `Building.Tiles`
- [ ] group all editor code in subfolder/namespace
	- [ ] 


- `Building.Tiles`
	- `Base Classes`
		- `EditableTile`
			- *specifies that the tile is dynamic*
		- `PuzzleTile`
			- *wrapper for Unity's built-in rule tile*
		- `DamagableTile`
			- `RepairableTile` - *references the damaged version of this tile, allowing tilemap damage to be applied by exchanging this tile for the damaged version*
		- `RepairableTile`
			- `DamagableTile` - *references the repaired version of this tile*, allowing tilemap damage to be applied by exchanging this tile for the damaged version*
	- `LadderTile`
	- `PipeTile`
	- `SolidTile`
	- `WireTile`
	- `WallTile`
		- 
		- `bool isDestructable`

### Editor Code Improvements
cleaned up the editor files, separated copier and generator

- changed editor so that it now uses reflection to determine the tile options so whenever a new tile class is made, no changes will need to be made to the editor in order to provide capability to copy a tile into the new type of tile asset


----
## Reflection



### What was learned or accomplished?


### Where to go now?

