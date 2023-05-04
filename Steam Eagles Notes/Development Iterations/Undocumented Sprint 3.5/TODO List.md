- [x] add `ToolSwapController` (on prefab root~~?~~.)
- [x] check build tool has tool assigned
- [x] hook up recipe UI to recipe tool generically

- [x] fix build tool bug
- [x] build tool recipe switching
- [x] build/craft tool consume inventory items
- [x] failure message system (need a way to tell the player why the building cannot be placed, either because not enough items or the placement is invalid for some reason. Implement this so that by default it only has those two messages but the tool child class is able to overwrite the failure message and failure logic)