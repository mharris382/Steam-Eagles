
- [ ] review my SFX asset library
	- [ ] Ultimate SFX
	- [ ] Ultimate SFX Bundle
	- [ ] Monster Sounds
	- [ ] Human Voices
- [ ] check for storm sounds on [FreeSound.org](FreeSound.org)
- [ ] create a list of important game SFX
	- [ ] decide on SFX groupings based on the type of in-game logic the sound uses (*looping, one-shot, room specific, ect.*)
- [ ] choose a list of actual audio assets that we want to use as proof of concept (for audio integration testing)
	- [ ] must have at least one sound for every SFX grouping choosen



# Audio Groupings

### Audio Brain Dump


- **feedbacks**
	- [ ] item picked up
		- [ ] rare component (*hyperglass*)
		- [ ] basic components (*scrap, copper pipe*)
	- [ ] tile deconstructed (*by tool*)
	- [ ] tile broken (*by storm/damage*)
	- [ ] machine built (*by tool*)
	- [ ] tile built (*by tool*)
	- [ ] tool usage error feedback
		- [ ] tried to build machine on an invalid placement
		- [ ] tried to build tile invalid placement
	- [ ] elevator feedbacks
		- [ ] elevator button pressed
		- [ ] elevator arrived at destination
		- [ ] elevator shut-down (*due to lack of power*)
		- [ ] elevator power restored


```ad-question
collapse: open

 warnings may be considered different than feedbacks because warnings are not necessarily triggered by player action while feedbacks are triggered by player actions
```
- [ ] **warning sounds**
	- [ ] Storm Events
		- [ ] Ship Damage
			- [ ] Ship Took Storm Damage (*raw event*)
				- [ ] Types of Damage *(ordered in terms of estimated severity)*
				- [ ] Wall Damage
				- [ ] Machine Damage *(still functional)*
				- [ ] Pipe Damage
				- [ ] Machine Damage *(non-functional)*
				- [ ] Size of Damage
			- [ ] Ship Took Critical Damage (*ship functionality was interupted*)
		- [ ] Storm Intensity
		- [ ] Storm Type
			- [ ] Spring Shower
			- [ ] Thunderstorm
			- [ ] Tornado
			- [ ] Huricane
	- [ ] 