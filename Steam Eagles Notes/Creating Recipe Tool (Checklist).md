- [ ] extend recipe data item to include an asset reference to it's custom data type
	- [ ] created `RecipeInstance` which stores asset reference to recipe data
		- [ ] assign prefabs as addressables for
			- [ ] hg engine
				- [ ] assign recipe
			- [ ] turbine
				- [ ] assign recipe
			- [ ] vent
				- [ ] assign recipe
	- [x] assign asset refs for 
		- [x] solid tiles
		- [x] pipe tiles
		- [x] wire tiles
	- [x] added string for address inside EditableTile 
		- [x] add to pipe tile
		- [x] add to solid tile(s)
		- [x] add to wire tile


![[Pasted image 20230414160144.png]]



that was all just prereq for actually connecting the systems involved 

1 (build/craft)
**systems:** tool → *recipe → machine → gameobject instance*41 ← entity ← save/load
**systems:** tool → *recipe → tile → building tilemap* ← save/load


2 (destruct)
	**systems:** tool → tilemap → tile → recipe *(→ pickups)*
	**systems:** tool → gameobject instance → machine → recipe *(→ pickups)*

3 (pickups)
	**systems:** pickup → entity → item → inventory (→ UI )

4 (recipe/inventory)
 **data:** recipe → item → inventory
 **UI/UX**: tool → recipe → UI

5 (machine/tilemap networks)
machine tiles
machine processing systems

steam network manager