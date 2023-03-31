%%
[[Sprint 3]] #iteration #unity
%%
# Iteration 12: Jumping on Airship
Previous Iteration: [[Iteration 09 - Climbing Movement]]
Next Iteration: [[Iteration 13 - preparing to pilot]]


## Goal


### Purpose
Test out a new method for adjusting player velocity when they are aboard the airship and not grounded.

### Hypothesis
Inside the physics update, whenever we detect a building we should automatically try to adjust our rigidbody velocity so that we are moving relative to the ship.  

The player's x velocity range when they are not aboard an airship should be as follows $-maxSpeed≤velocity.x≤maxSpeed$

so logically the actual range the player can move at should be that range, offset by however fast the ship is moving on the x axis

----
## Result

![[ariship vertical velocity effects jumping.mp4]]
## Inside of Structure State
```cs
private void FixedUpdate()  
{  
    if (BuildingRigidbody != null)  
    {        //get the building's velocity and apply it to the character  
        var buildingVelocity = BuildingRigidbody.velocity;  
        var characterVelocity = State.Rigidbody.velocity;  
        var desiredVelocity = new Vector2(State.MoveX * State.config.moveSpeed, characterVelocity.y);  
        var normalVelocityMin = -State.config.moveSpeed;  
        var normalVelocityMax = State.config.moveSpeed;  
        var newVelocityMin = normalVelocityMin + buildingVelocity.x;  
        var newVelocityMax = normalVelocityMax + buildingVelocity.x;  
        var velocityResult= Mathf.Lerp(newVelocityMin, newVelocityMax,  
            Mathf.InverseLerp(normalVelocityMin, normalVelocityMax, desiredVelocity.x));  
        State.Rigidbody.velocity = new Vector2(velocityResult, characterVelocity.y);  
    }}
```

----
## Reflection



### What was learned or accomplished?
finally got horizontal aerial movement when the player is aboard the airship.

### Where to go now?
we should really add some code to fly the airship around now...
