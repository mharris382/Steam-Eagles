%%
[[Sprint 3]] #iteration #unity
%%
# Iteration 22: The Rooms Graph
Previous Iteration:  [[Iteration 21 - Authoring the Rooms Graph]]
Next Iteration: 


## Goal
I've been putting off implementing a rooms graph feature for a couple of reasons, but primarily I hadn't decided how I wanted it to work internally.

### Purpose
I've decided to implement the room tracking system without using 2D or 3D Physics.  Instead I'm going to build a room graph and follow a particular algorithm to determine which room a character is currently inside.

### Hypothesis

To determine which room a character is in...
1. first check if we checked this character on the previous tracker update
	1. if we did not then we need to search every room
2. if we did determine where this character was on the previous check then chances are they are still in that room
3. check if the character is inside the room they were in on the last check
4. if they are not, chances are that they are in a neighboring room.
	1. recursively search all neighbors of current room until find the target room, using a ***Breadth First Search strategy***

```ad-question
title: Why BFS Search?
collapse: open
we want to search rooms in order of their relative proximity to the last room.  So we want to check all neighbors of the current room, before we begin to check neighbors of neighbors
```


----
## Result





----
## Reflection



### What was learned or accomplished?


### Where to go now?

