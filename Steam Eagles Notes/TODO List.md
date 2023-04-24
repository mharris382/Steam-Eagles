- [ ] add `ToolSwapController` (on prefab root~~?~~.)
- [ ] check build tool has tool assigned
- [ ] hook up recipe UI to recipe tool generically

- [ ] fix build tool bug
- [ ] build tool recipe switching
- [ ] build/craft tool consume inventory items
- [ ] failure message system (need a way to tell the player why the building cannot be placed, either because not enough items or the placement is invalid for some reason. Implement this so that by default it only has those two messages but the tool child class is able to overwrite the failure message and failure logic)