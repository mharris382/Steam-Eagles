I am reimplementing the Storm system since last time it did not go so well.  I really want to see those damaged wall tiles in action, and the system is pretty well isolated so I think I should be able to implement this system safely.


The biggest architecture difference between this time and last time is that I am now using a DI framework so hooking up the dependencies should be easier.

### Storm System Dependencies
the only real external dependency storms have is on the building.  the building is already being installed but storms should be at the scene context level not the building gameobject context level.  So the storm system will need to have the buildings that it targets injected.

However I think there the actual ship damage is itself a storm subsystem so we should be able to implement the storm factory without needing the building, then have a special storm sub-system specifically responsible for dealing damage to the building, and that system will need to recieve a building instance and a storm to function

