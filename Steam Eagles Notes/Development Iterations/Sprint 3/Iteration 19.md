%%
[[Sprint 3]] #iteration #unity
%%
# Iteration 19: Damage System Visual
Previous Iteration: 
Next Iteration: 


## Goal

The system that decides when to deal damage during a storm is working, except there is no way to tell how well it is working in-game because we can't see it.  Additionally it needs to know where it is able to target.  


### Hypothesis
For that we will need to build a special tilemap graph that knows about 2 tilemaps: the solid tilemap and the wall tilemap.  Well it shouldn't need to know about them directly, so might as well create an interface here.

![[Building Interfaces]]


this should give us all the tilemap information that we need to implement not only damage but repairs as well.

However the damage system is going to need access to the graphs that indicate connectivity of the space.

This needs to be done using the `TilemapGraph` class.

i need a list of all possible targets.

~~i also need a record of past targets~~


----
## Result





----
## Reflection



### What was learned or accomplished?


### Where to go now?

