%%
[[Sprint 4]] #iteration #unity
%%
# Iteration 26: Replacing Global Signals
Previous Iteration: [[Iteration 4a]]
Next Iteration: 


## Goal
I've discovered that while the global signal bus does allow for very loose coupling, it also convolutes the logic and makes it hard to trace entry points.  Especially when the signal handler class is also being used directly, bypassing the signals.  I suppose without zenject, the global signal bus may have been the best way to handle cross-system communication; however with Zenject we have other potentially better options for these kinds of operations.


### Purpose
- [ ] Determine what global signals are currently being used for what processes
- [ ] Determine which of these signals could be replaced with a dependency *(determine what the dependency is)*
- [ ] Decide which of those signals should be replaced with an injected dependency

### Hypothesis

My hypothesis is that the following signals can be replaced
- [ ] Save Game
- [ ] Load Game

----
## Result





----
## Reflection



### What was learned or accomplished?


### Where to go now?

