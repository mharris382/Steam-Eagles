%%
[[Sprint 1]] #iteration #unity
%%
# Iteration 3:
Previous Iteration: [[Iteration 02]]
Next Iteration: [[Iteration 03a]], [[Iteration 03b]]


## Goal

honestly I'm not 100% convinced that this is the right goal, but I also have to pick something or else I'll waste all my time being indecisive so here goes nothing.

### Purpose
I need to make it so that structure can be save/loaded.  Buildings will be saved and loaded using their unique id.  This means that if the same building id exists on two buildings, one of them will overwrite the other when saves occur.  I also need to verify that this method will allow me to transfer buildings between scenes without incuring the wrath of the unity tilemap bugs.

### Hypothesis

I'm going to kinda follow a test driven mindset here, first add save/load methods to the building script.
- [ ] add a save method (add button attribute)
- [ ] add a load method (add button attribute)

#### Additional Building Script Refactoring
- [x] deleting the tilemap arrays to simplify the code. 1 tilemap per layer for now
- [x] moving the editor script to a separate file
- [x] add regions to the script
	- [x] inspector fields
	- [x] fields & properties
	- [x] unity events
	- [x] methods
	- [x] editor stuff

#### Additional Building Improvements
- [ ] extract super class for building called `Structure`
	- [ ] move rigidbody and box collider properties to base class
- [ ] refactor BuildingState to StructureState

#### Building Bugfixes
- [x] building requires box collider 2d, 
	- `isTrigger = true`
	- `layer = LayerMask.NameToLayer("Triggers")`


----
## Result


##### Circular Dependency Found
I just realized there is a circular dependency between `Building` and `BuildingTilemap` in this method
```cs
public void UpdateTilemaps()  
{  
    foreach (var buildingLayer in GetAllBuildingLayers())  
            buildingLayer.UpdateTilemap(this);  
    
```

this can be fixed by extracting an abstract base class for Building and then changing the signature to accept the base class rather than the impl.


##### Other Refactoring Done
removed the update rigidbody method from building which was just setting the rb to static for pretty much no reason.

#### Need to Fix Layers
there are so many obsolete layers it's not even funny.  I need to do a complete overhaul on this layer situation and get this under control.  Should I fix it now, or finish what I started and then do it after?  Second one


----
## Reflection



### What was learned or accomplished?


### Where to go now?

Fix the layer situation