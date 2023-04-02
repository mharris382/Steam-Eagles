The status api is a simple yet extraordinarily powerful idea that can be used to define nearly every dynamic attributes in the world, and implement complex logic systems via simple status rules.

From there the status system would function as the driver for many reactive systems.


# Definitions
## Status
a status is minimally a keyword that is associated with an entry in the global status database.  

```cs
[System.Serializable]
public class StatusDefinition
{
	public string statusKeyword = "New Status";
	public EntityType entityTargetType;
}
```

### EntityTypes
```cs
public enum EntityType
{
	AIRSHIP,
	CHARACTER
}
```

## Entities
Anything in the world that is able to be associated with a list of statuses.  

```cs

```

# Reactive Streams
the power of the status system comes from the ability to add a status to an entity and that status implicitly enables/disables other statuses which the entity has but are not active.  When a status is added to an entity it will remain in the entities internal list of statuses, but that does not necessarily mean the entity will have the status active.  If the entity has a different status that blocks the first status, the blocked status becomes deactivated.  This is functionally equivalent to the blocked status being removed, except that since it is not actually removed, if the blocking status is removed, the blocked status will become unblocked and reactivated.  

To the outside classes, it would seem that the status were explicitly added/removed, when in reality that status was only added once, and a blocking status was added/removed.