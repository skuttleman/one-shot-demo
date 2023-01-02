# POC

I want to make a game.

You can check out the super rough POC [here](https://one-shot-2ba80.web.app) to give you a quick idea of the kind of game I want to build. Other than the rendered sprites, everything about the POC is actually a 3D game. I originally thought it would be easier to model it in 3D space to allow for some verticality in level design, but it turns out that Unity isn't particularly great at mixing 2D and 3D components so I had to rebuild it as a _fully_ 3D game. This leaves us with an early decision point about the scope of this project.

Should we...

- downscope to a 2D game (severly simplifying or removing any level verticality)
- upshift to a 3D game (not sure how much longer it is to create 3D assets vs 2D assets, but I'll bet it'll be a huge scope increase)
- move forward with building an invisible 3D game with a 2D game drawn over top of it

## Controls

I recommend playing with a gamepad. There are mouse/keyboard controls but they're harder to use - especially looking/aiming with the mouse.

### Gamepad

- LEFT STICK:        move
- RIGHT STICK:       face a direction
- LEFT STICK CLICK:  tap to sprint while moving
- RIGHT STICK CLICK: activate TBD
- A:                 change stance / drop from hanging position
- A (double tap):    dive to crawling position from running/crouching
- Y:                 climb up from ledge hang
- RIGHT BUMPER:      activate scope
- LEFT TRIGGER:      aim weapon
- RIGHT TRIGGER:     fires when aiming / punches when not aiming

### Mouse + Keyboard

- W,A,S,D:            move
- MOUSE:              face a direction
- SHIFT:              tap to sprint while moving
- TILDE (~):          activate TBD
- SPACE:              change stance / drop from hanging position
- SPACE (double tap): dive to crawling position from running/crouching
- E:                  climb up from ledge hang
- F:                  activate scope
- RIGHT CLICK:        aim weapon
- LEFT CLICK:         fires when aiming / punches when not aiming

## What's There

Bugs. Jenkiness. The graphics are crappy, but hopefully it's good enough for you to tell that there's a two story building to run around in. There are windows and doors, and some stairs to the partial roof of the first floor from the south side around to the west of the building. Another set of stairs is inside. The arrows above the stairs point in the direction of "up-the-stairs". There's some basic moving/hiding/line-of-sight features to give a sense of where this would go. There's no objective or win/lose state. Just mess about. I think it's better to let you explore what's there rather than try to explain it.

## Game Design Document

When you're done playing with the POC, read through the [game design document](/GDD.md) for a more detailed explanation of what I want to build.

Let's do this! Who's with me?
