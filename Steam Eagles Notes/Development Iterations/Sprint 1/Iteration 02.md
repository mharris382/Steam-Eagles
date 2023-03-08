%%
[[Sprint 1]] #iteration #unity
%%
# Iteration 2:
Previous Iteration: [[Iteration 01]]
Next Iteration: [[Iteration 03]]


## Result

![[Pasted image 20221231220644.png]]



----
## Reflection

The project is kinda all over the place right now.  I went into today wanting to rebuild the selection UI, but ended up working on the island UE project instead, which honestly isn't that bad.  I think that's a really good project for me to work on for a portfolio piece and if I want to land a job working with UE instead of Unity.  

The family did a survivor binge today, and I wanted to spend the day with the family so I did.  hold on, it's a big vote.

OK so I didn't accomplish what I wanted to accomplish.  I did find a couple interesting open source C# finite state machines that look really good.  I definetly need a reliable finite state machine for this game.  It is a massive video game now, in terms of sheer scope.  The fact that it is 2D has some advantages and disadvantages, but the primary advantage and one of the big reasons I'm not going to even consider switching is performance.  The fluid simulation will be much more manageable in 2D.  The disadvantages are that I need to be very careful in organizing my scenes, the 3d perspective camera makes it easier to manage 3D scenes


### What was learned or accomplished?

#### Unity Grid/Tilemap Bug
first of all tilemaps can become unreliable when you copy them in unity.  So basically what happened is I took the dock building from the boat scene and moved it to a new scene which was meant to be part of a streamed open world setup, however the tilemap dissappears in the scene when you hit play mode.  This wasn't the first time I experienced this bug, but I found online that it occurs when you duplicate a grid/tilemap(s) in Unity.  There was a [workaround mentioned in the bug report][1], but I have a much better idea.

With the building structure, I already have structure for the tilemaps, and since they need to be edited/saved/loaded at runtime anyway, I can build that functionality into the editor.  From there I should be able to add a save button that saves the current tilemaps to an external file, and another one which loads from an external file and writes that to the scene.    Doing so has a couple of nice advantages. 
1.  allows me to create new building/grid/tilemaps and not have to ever duplicate them in unity
2.  allows me to test the save/load functionality from the editor

[1]: https://issuetracker.unity3d.com/issues/tilemap-painted-tilemap-disappears-from-scene-view-when-dragging-the-scene-view-to-the-right

### Where to go now?

I feel like I'm trying to stay afloat but my ship has collapsed and now I'm jumping from one piece of driftwood to another, wanting to fix that piece, but then standing on that piece I see the other pieces and switch to those.  Like I want to fix the ship, but every time I try to focus on fixing it, either the mess is overwhelming, I get distracted with vfx or other stuff outside of life.   

#### UE

it's the first 5 minutes of 2023, and I need to figure out what I want to do with this year.  I know I want to get a job, and even though I'm probably waaay more qualified to work in Unity than I am to work in UE5; working in UE is what I'd rather be doing.  I already know Unity well enough to know that it won't be able to compete with UE in certain categories.  The games I want to make throughout my 30s are going to probably focus on UE tech not unity's tech.  However, I'm pretty much an expert with Unity and C#, and I'm not quite the same with UE and C++.   I should ask dad about this I think.

#### Steam Eagles
As for Steam Eagles, I would like to continue with it; but I really need to get some things laid out first.  I can't just run headfirst into this project, because I will keep running in circles.  The really cool thing about it, is that if I can get my shit together and really solidify the gameplay and the code; then I can let loose on the vfx and shaders.  In 2D i'll kick ass at that; but if I can't get the gameplay solid first, there's no point to making the vfx.   

Getting the gameplay there is no easy feat.  The gameplay is complex and I think I need to simplify it big-time.   First thing to cut down is interactions, no async interactions for now; the thing I did with the boat is perfect.  

Next is the character controller.  It's fine for now, just gotta fix the wall climb bug with a slope check.  Beyond that, I think everything else is fine for now.


#### Mixed Eggs EU
This will be a multi-year project and I'm not going to be putting all my eggs in this basket, that's where the UE work comes into play.  I think overall today was really good, I did that UE tutorial on landscape materials which was very helpful and gave me a better sense of how to make landscape materials in UE (which is really just a regular material with a couple landscape specific nodes).  I still need to check if it works with layers, but i am fairly confident it will work.

I also got a heightmap imported from gaia that I used in UE to generate a landscape.  The imported map is a bit rough but overall worked really well.  I did have to adjust the scale of the landscape.

The other thing that I learned about creating landscape materials is this depth fade technique where you use 2 different tiling sizes based on distance and blend between them.  The thing that was weird was that he used a multiply node and I used a divide, but I think the issue had to do with the landscape scale.  He mentioned something about knowing the resolution of the tilemap as being helpful for determining the proper UV sizes, but I pretty much just eyeballed it.

#### Really wanna dive into Compute shaders... like super super bad
I really want to start working a lot with compute shaders, and this gas sim is the perfect place for that