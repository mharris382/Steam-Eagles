

Setup Steps
1. [ ] add `ToolRecipeHUDController` to scene in HUD root of new **recipe HUD**
	1. [ ] assign test tool
	2. [ ] create `HUDFancyScrollView`
		1. [ ] add `Scroller`
2. create prefab for `HUDFancyCellRecipe`
	1. [x] assign prefab for `HUDElementRecipeComponent`
	2. [x] assign image for icon
	3. [x] assign text for count
3. [ ] create animator for `HUDFancyCellRecipe`
4. create container go and add `HUDFancyScrollView`
5. create prefab for `HUDElementRecipeComponent`
	1. [x] add icon
	2. [x] add text
6. [x] add switch recipe input control as digital axis
	- [x] modify `PlayerRecipeHUDController` so that it reads switch recipe and updates the selected item accordingly
7. 