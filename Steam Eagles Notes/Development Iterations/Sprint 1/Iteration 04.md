%%
[[Sprint 1]] #iteration #unity
%%
# Iteration 4: Base Building is coming back
Previous Iteration: [[Iteration 03]]
Next Iteration: [[Iteration 05]]


## Goal
I will reinstate base building tonight.  It will happen, I have 100% certainty of that.  


### Purpose


### Hypothesis
ASAP principle, I will study the original character prefab setup and figure out how to make it work without a reliance on the original prefab
![[Pasted image 20230103191411.png]]


----
## Result

it is finished, but it took more than one attempt.  I thought I had it working, but alas it was actually just luck that made it work that time.  It's actually still pretty early so hopefully I can also repair the UI and then make it so that base building works with both the controller and the kbm input, but we'll see about that.  I managed to remake the majority of the system, but reused the CellAbility.  I go rid of that inheritence nightmare and replaced it with a AbilitySet interface which basically bundles abilities together explicitly, rather than the way I was doing it previously (implicitly via inheritence).





----

### What was learned or accomplished?
proves that often the best solution is to strip out the bloat and go back to basics. Just gettin it done

### Where to go now?
still needs to 
1. add the UI into it
2. make it work with controller


however, I'm kinda bummed right now about stuff. idk i think i didn't sleep well again.   Either way, I'm gonna take, damn, it's 5:35 pm already.  I wanted to make gas work again.

I discovered that the gas tank is not losing gas, or the gas tank UI is not being updated.  Either way that bug needs to be fixed ASAP.
