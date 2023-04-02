%%
[[Sprint 3]] #iteration #unity
%%
# Iteration 24: 
Previous Iteration: [[Iteration 22 - The Rooms Graph]]
Next Iteration: 


## Goal
be able to write code which can 
1. query the room that an entity is inside
2. react to notifications when an entity changes rooms

### Purpose
This is a highly valuable system which can now be implemented efficiently using the same data structure used to draw the rooms graph

### Hypothesis
First I will create a TrackedEntity POCO, which will store events and data about it's current room state which will be updated by the room tracking system.

There should probably be only 1 room tracking system per game.   That tracking system should first identify the building that an entity is inside, then identify the room.  Once that information is known, the room location will be reevaluated on each frame while the building will only be reevaluated if the position cannot be quantified to any room.  If this is the case, but the entity is still inside the room, it should be moved to a special null room object


```cs
public interface IRoom
{
	string name { get; }
	GameObject cameraGO {get;}
}
```


----
## Result


crap I just spent like 2 hours making a dope editor window with Odin.  not gonna lie, this will come in handy but that wasn't the task I set out to accomplish.  

# Building Manager Window
the window has two main pages: 

## Tilemaps Table
![[Pasted image 20230402144500.png]]
## Rooms Table
currently pretty basic but very easy to extend and customize this.  The editor window is a prefered way to handle custom editors for the most part, with the exception being if we need to intercept scene view events for handles.  (Even that can be accomplished i believe)
![[Pasted image 20230402144555.png]]

Also took my TilemapBuilderParameters and created a location which defines what each type of tilemap should have for creating them from code. 

----
## Reflection



### What was learned or accomplished?


### Where to go now?

