#game-design 

seealso: [[Types of Fun]]

# Gameplay Modes

**Normal Mode**: gameplay priority is combat, exploration, resource gathering, and destruction

**Construction Mode**: gameplay is about base building, constructing buildings inside the airship, and using the airship as a form of self-expression

**Vehicle Modes**: the vehicle modes will in some ways tie together the exploration and construction modes (particularly the Airship Vehicle Mode)

ok this makes sense as the next steps.   a few additions to each mode:  inventories should be accessible from normal mode, construction mode, and vehicle mode.  Vehicles should have a shared inventory which is accessible from (a) vehicle mode and (b) a buildable object (like a dumbwaiter hatch). 

Construction mode should allow players to view recipes that they currently possess, there wont be enough recipes to justify a whole multitab window like satisfactory, so I think we will have it setup where the recipes can be cycled and the category can be cycled (for controller optimization).  Recipe categories will be for now the block recipes, and the machine recipes.  there should be a way to get more detailed information about the recipes that you currently posses (i'll come back to that)

For exploration mode (aka normal mode), destruction will be a key tool.  We'll start with that tool in the primary tool slot.  The gas tanks are the other tool which doesn't operate quite like the rest.  Players will need to view their passive items as well as their tools.  Some tools should not be managed as items, as doing so clutters up the inventory and makes it possible to forget or even lose an important item (happened to me in stardew valley).  Managing inventory space should really be about managing the resource items, not trying to choose between your abilities (aka tools) and your resource space.   So what if we did this.... the first slot of the equipment slots is locked to your build tool?  fuck this is difficult. OK no tool items.

No that's stupid.  ok tool items, that includes bombs, gas tanks, and fun stuff like that.
Lets see, maybe we make things limited with tools. Where you can only carry around 1 or 2 tools when you leave to go exploring.  But we have special inventory objects which display your tools.  In construction mode you can designate a spot for each tool in a room.  If you have a spot for that tool, you can then easily store and pickup the tool; and the tool now also functions as a decoration in the room.  Oh if we did that we could also add that same function on vehicles; allowing you to bring more items with you provided you have a vehicle.

ok so lets say we have the construction button, then we do a button modifier with that button which enters into a special mode where you get to give the currently equipped tool a storage spot.  All valid storage spots would then be highlighted.  There are three possible states
1. the storage spot is valid and unused (green)
2. the storage spot is valid but already designated to another tool (red)
3. the storage spot is not valid for that tool (grey)

ah this mechanic is beautiful, the same mechanic can be used to attach gas tanks to the machines when you want to either fill up the tanks or deposit gas into the tanks
