%%
[[Sprint 1]]
#ui/ui 
%%
# Iteration 1:
Previous Iteration:
Next Iteration: [[Iteration 02]]

See [[2022-11-08 |Reflection from that day]]

## Goal

### Purpose
- Make UIs for the Pipes and Turbine, with a focus on UX

### Hypothesis

The UI needs to be both intutive and satisfying. Since completing a pipe network means connecting pipe segments from an input to an output, the most important part is making sure the player knows whether their network is completed and which way the steam is flowing in the pipes. 

Secondary -- but sill important -- is that it's nice to look at. Seeing these connections will be a large part of the game. So the UI needs to make the player feel like they are contributing. This can be accomplished through:
	- color
	- movement
	- animation(s)

----
## Result

### The Pipe Network
My original plan involved arrows and honestly would've looked really bad and stupid. Max came up with the idea of animating a gradient along a line renderer. The gradient would move in the direction of the flow. Much prettier and cooler than arrow heads. 
This is what we ended up with. It's hard to tell, but there are yellow lines running from the output to the input.
![[Pipe flow visualization mk.1.png]]

### The Turbine
I made a sketch of a possible turbine
![[turbine-with-notes.png]]


----
## Reflection

### What was learned or accomplished?
- how to make an animated shader for the line renderer
- Over exaggerating is **very** important for designing game UX
- The player needs to feel like they did something huge in reward for their efforts

### Where to go now?

