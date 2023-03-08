
![[Player Assembly Class Diagram.canvas]]


the main job of the player assembly is to manage the and coordinate player object lifetimes by tying the creation of the player instance to the connection of a new input device.  The logic of this is that by pressing a button on their input device (either a controller, or KBM), the player will have made their input device preference known to the game and knowing that we are able to recieve input from that player.

```ad-attention
title: Attention: Device Disconnect
Device disconnects do need to be handled, but are a lower priority, as the game can be playtested without the system needing to handle disconnects.  
```

As for the destruction of the player, there are only a few situations where the player should be entirely destroyed, furthermore the conditions depend on *which* player we are talking about.   The only real time we need ot destroy a player is if the player requests to exit the game from a menu.  Otherwise a player object would remain alive until the application shuts down. 

The lifetime of the player is tied to the input device, however this is not the only object relevant to the player assembly.  Once the game has been started there are several lifetimes which must be accounted for: the Character, the Camera, and the UI.  The UI has not fully been integrated into the player assembly at the moment.  The other two systems have been built as fundamental components of this system.  


