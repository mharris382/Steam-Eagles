This is a new organization system which ties character actions (or abilities) to the item/inventory system. 

If Items system provides us with nouns, then the tool system provides us the verbs.  

This has the benefit of leveraging the item/inventory UI for ability management, making it easier to conceptually understand the tool system.   Additionally we area also able to leverage the persistence features of the item/inventory system to save the tool locations in the form of items.  

## Toolbelt as an Inventory
Making the toolbelt a specialized type of inventory allows us to save the loadout just like any other inventory.  It also allows us to display the toolbelt UI using existing inventory UI features (which might need some tweaking).

# The Non-Inventory Stuff
OK so we covered what aspects of tools are tied to the item/inventory system.  Now we need to address the more important question: *what do tools do that items don't do*?

Well, if tools provide verbs, then the answer to that is that tools fundamentally do everything and items fundamentally do nothing.

So what I think is that if tools do stuff, then even the interaction system itself would be a subset of the tool system.  If every verb that is not movement or UI related is fundamentally a tool;  Then that means all interaction are tools?  No that's not right because an interaction involves two actors.  The tool is only 1 side of an interaction.  

OK but tools and interactions are clearly related.  The tool is the instigator, the world (i.e. the environment) is the decider.  

In other words, the tool tells the world what it wants to do and the world tells the tool what happens.  
```ad-note
title: Note on UI
collapse: open
The above statement excludes the UI/UX component of the interaction, which involves both parties, but doesn't actually take part in the system from a mechanical perspective.  It merely makes the system's inner workings understandable and discoverable to players.
```

So from one perspective the tool should always begin the interaction, while the environment (or mediator system polling the environment) dictates whether the action is succesful or not.

### Addressing the UI component of this system
While the UI is not actively taking part in the manipulation of the system state, it must have complete access to the entirety of the system state.  More importantly, the UI must know what can or cannot happen if the player were to use the tool from this system state.  Furthermore the UI must sometimes explain why a particular outcome can or cannot happen, or if more than one outcome is available and what other actions the player can take to reach those other outcomes.

The UI is possibly the most tricky component in this system.  I can see why it would often be considered an afterthought by programmer or system designers; but the tool UI is 
- fundamentally volatile (likely to change)
- probably unique to each action or interaction
- largely dictates the player's experience with the game, or rather dictates whether the gameplay is seamless or tedious for the player

# Reflection

**Items/Inventory** - what does the tool system **NOT** need to worry about
- saving/loading
- equipping/unequipping
- non-tool specific UI functionality

What does the tool system need to worry about
- doing everything? pretty much yeah

What do we know about the tool system abstractly
- it will be highly linked to environment
- it must be linked to the character system and the player/UI system but not at the same place
- ~~only one tool item would be equipped at a time~~
- each tool item should coorespond to an entry/exit point for that tool's functionality in the world

```ad-important
OK so big thing I just realized on UI.  The tool UI will be more closely linked to the player system than the rest of the tool system, since when AI use a tool the UI that involves would be only feedback related.  

However, the AI might need a different version of the same information that the UI will need for decision making.  Essentially the AI and the UI must understand the entire state of possibility whereas the actual tool itself only needs to invoke the action when told to do so. 
```

essentially what this means is that every tool will have it's own assembly that is implemented specifically for that tool.  Most of the code in the tool root assembly will be abstract or very simple.

