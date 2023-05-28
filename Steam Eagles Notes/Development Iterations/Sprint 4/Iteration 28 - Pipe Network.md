%%
[[Sprint 4]] #iteration #unity
%%
# Iteration 4: Pipe Network
Previous Iteration: [[Iteration 27 - Build, Repair, Damage, Destroy]]
Next Iteration: 


## Goal
Design an API for use by external systems (aka non-pipe network systems) which covers all the system use cases for interacting with the system.  

### Purpose
1. Define the use cases for interacting with the Pipe Network
2. Determine (based on current classes) which classes will need access to the pipe network
3. Decide what scope (aka `DiContainer`) the Pipe Network should exist under

### Hypothesis
1. Use Cases
2. User Classes 
3. Single Pipe Network will exist for each Building (*`DiContainer` is `GameObjectContext` at Building level*)

----
## Result

### Define Terms

```ad-abstract
title: Pipe Network
a collection of **connected pipe network**s associated with a particular group of connected rooms *(aka Building)*

```
```ad-summary
title: Connected Pipe Network
a collection of pipe nodes which are physically connected to each other 

```

```ad-abstract
title: Pipe Fluid Flow
a Pipe Fluid Flow represents a movement of a fluid from an inflow node towards one or more outflow nodes in a connected pipe network
```


```ad-info
title: Pipe Transport Node

![[Pasted image 20230528170223.png]]

```

```ad-info
title: Inflow Node
a single inflow node represents a possible fluid flow within the pipe network. only one inflow can exist per **connected pipe network**

![[Pasted image 20230528170858.png]]
```

```ad-info
title: Outflow Node
at least one outflow node is required on connected pipe network in order to fluid flow to occur, but any number of outflow node are allowed to exist same connected pipe network

![[Pasted image 20230528171242.png]]
```



Rules:
- may only be connected to a single gas inflow node *(blocks tile placement if the action will result in the current pipe network having more than one inflow node)*
- if a connected graph of transport nodes is connected to an inflow node, there are two opto


----
## Reflection



### What was learned or accomplished?


### Where to go now?

