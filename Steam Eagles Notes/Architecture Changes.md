

# Considering
- [ ] adding equipment framework which can serve as a separation layer between `SteamEagles.Items` and `SteamEagles.Tools`
- [ ] add a new type of reactive item slot for tools, which handles
	- [ ] coordinating activation states between sibling slots (which cannot be active simultaneously)
	- [ ] loading controller prefabs and spawning instance of prefab on activation, as a child object to the slot
	- [ ] destroying or deactivating the prefab instance when tool becomes inactive
- [ ] implement generic slot state enum which defines states relevant to UI systems, and is managed by a system which inforces rules such as only one slot may be marked with the `active` at the same time. Same rule for `selected`. However they don't need to be on the same slot.  Selected means the slot that we are currently looking at, while active means the slot which is currently equipped or being used by the character.  `selected` does not have to mean `active`.   We should always have either 1 or 0 selected slots and we should always have either 1 or 0 active slots.  Selected and active can be the same slot, but they don't have to be.
	- [ ] the point of this is to make it more natural to implement logic that displays the correct state and updates the UI, and simultaneoulsy make it simplier to implement systems which are driven by the active slot


# Debugging Features
- [ ] add window to character debugger which shows the state of the player's inventories
	- [ ] implement generically so that the window can be reused for other (potentially more complex windows)