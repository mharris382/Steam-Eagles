last iterations (*undocumented*): 
![[TODO List]]
# Objective of Sprint 4

### Create Content to begin Social Media
i have no clue

### Refactor System Architecture for Long Term Sustainability

Current Problem Areas (lots of bugs): 
 - Tool Systems
 - UI Systems
	 - Input Systems

Areas that are pretty solid
- Building System + Room Tracking
- Entities Management + Save/Load

Areas that are partially solid 
- Character Controller Systems (*Movement*)
- Player Management Systems 
- Buildables

Areas that are very weak
- Tools
- Items/Inventories
- UI
- Gas Sim

these groupings are based on gut feeling alone, going off of the fact I've worked on them all for such a long time

problem with extenject is that I get distracted and create installers first, since they are the starting point.  However I also need to know what I am installing and more importantly why it is being created.  What problem am I solving with this installer.   I created a global installer which currently creates just the pause menu from a prefab.  I was starting here  because currently there is a bug with the pause menu and player HUD.  

Oh *duh*! I also need to install the player HUD as a prefab as well.  Currently those were added in the main menu scene as a child of the GameManager object.  They are in fact prefabs so it makes sense that they are injected as such.

created a new system called PCSystem since apparently I will be creating a type of system which just listens for Player Characters being created then creates a system instance for that local player character.  This type of system will only really need to exist in the game world while the PCTracker will exist in the project context so it's ultra safe in terms reliability *(PCTracker is guaranteed to exist for all scene contexts)*. 

To test this system I created a new parallax system which will have the responsiblity of tracking all parallax objects that exist in the world and creating a duplicate version that will be visible to the player, while the other version will become hidden (the version that actually exists).  This is also incredibly useful because the parallax version will actual clone a non-parllax version which will be used to determine the actual (x,y) position of the object (not the parallax offset position).

I created this system using the Tickable and LateTickable interfaces so that this system could be used to spawn transform jobs so that all parallax transformations can be multithreaded automatically. This is one job per frame so the job must be synced on late update (*hence the late tickable*).    

