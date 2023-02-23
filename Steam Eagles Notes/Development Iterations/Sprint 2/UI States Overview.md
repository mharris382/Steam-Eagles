I usually don't like having a assembly that has it's hands in every jar, but I think the best way to integrate the various game states is through a UI controller, which can act as a coordinator for all the other game and UI systems.  Basically it's a UI state machine, but I'm going to go for explict coupling here, rather than the loose coupling I've been using for this project, mainly so that the UI controller code can explicitly tell the various systems when they should be active or not.   



![[UI States.canvas]]

```ad-note
title: Color Key
**New Game vs Load Game**
- blue lines indicate new game flow from main menu
- green lines indicate a load game flow from main menu
**Singleplayer vs Multiplayer**
- orange indicates start points for injection of a second player
- red indicates points at which second player is deleted (return to singleplayer mode)
```

# UI State Machine Overview
so a state machine to coordinate the multitude of UI systems that need to exist seems like a good idea.  I started implementing that, but immediately ran into questions about what systems it should directly interact with.  The first that came to mind were the systems defined in the [[Player Assembly]]

# Main Menu State

## 


# Pause Menu

## Multiplayer Menu

## Character Selection Menu