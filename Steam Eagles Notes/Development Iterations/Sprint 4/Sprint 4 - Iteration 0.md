%%
[[Sprint 4]] #iteration #unity
%%
# Iteration 4.0:
Previous Iteration: n/a
Next Iteration: 


## Goal

### Purpose

fix following bugs:
- [ ] tool recipe HUD missaligned with selected recipe
- [ ] virtual cameras do not follow the player character

### Hypothesis
I believe that these both of these bugs stem from technical debt.  The tools added a lot of features and HUD dynamics.  The introduction of Extenject made it easier to implement new features however there is now the issue of managing the dependencies of systems on different scales.  One such system is camera system which toggles the active virtual camera when the player switches rooms.  This system operates on a per player instance basis.  

I think the proper solution here would be to 
1. [ ] take a look at [zenject documentation](https://github.com/Mathijs-Bakker/Extenject/tree/master/Documentation)
2. [ ] document flow of logic for current player systems
3. [ ] determine which systems are currently responsible for the bugs
4. [ ] decide how to refactor the project so that the systems can be adjusted


### Bug 2 : Virtual Camera Follow
basically we need to append some extra logic when the player room camera changes.  
> 1. *parameters*: `GameObject VCamera, Room room, Transform character`
> 2. check if camera (`VCamera`) is tagged as `Follow Cam`
> 3. if camera is tagged as `Follow Cam` then create a proxy transform and assign to vcam follow target
> 4. move proxy to player's position but clamp the position based on configuration on the `Room Camera Config`

----
## Result





----
## Reflection



### What was learned or accomplished?


### Where to go now?

