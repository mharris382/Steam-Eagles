- [x] write code to handle dependency management for tool 
	- [x] room tracking
	- [x] character access (inputs)
	- [x] inventory access (pickups?)
- [x] write code that processes input into a destruct tool target position (position where we will place the destruct physics query)
- [x] use physics query to get all physical colliders that might be destructable
- [x] create an IDestruct interface

```cs
public interface IDestruct
{
	public bool TryToDestructFromHit(DestructParams hit);
}
```

