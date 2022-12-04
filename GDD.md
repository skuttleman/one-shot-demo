# One Shot - Game Design Document

This game design document describes the details for a top-down hardcore stealth game. There isn't really a name for this project, but I'll be using _"One Shot"_ as a working title. All of the ideas contained herein have yet to be fleshed out. **Absolutely nothing** in this document is set in stone. I want this to be a collaborative effort. I'm eager for input from anyone more creative than I am. This should be fun for all of us to make and to play.

## Overview

Stealth can mean 100 different things to 99 different people. So, let me loosely define what _I_ think it means. Stealth is about avoidance. Stealth is about manipulating the environment. Stealth is about patience and planning. My favorite stealth games give players a large number of options to let the player engage with a rich, systemic world; double bonus points if the player's options combine well. My favorite stealth games give players non-lethal options as much as feasible. My favorite stealth games encourage the player to stay in stealth, but give the player options for escaping or - to a limited degree - fighting back when stealth is broken.

I've split this document into three sections.

- ["Important Things"](#important-things) are the ideas that I would very much like to include in this game. I'm open to discussion if there are other opinions.
- ["WIP Plans to Get Us Started"](#wip-plans-to-get-us-started) are some ideas I thought we could use as a template as we get going. Please help make these better.
- ["Additional Ideas"](#additional-ideas) are some random ideas we could take or leave. Maybe they'll fuel better ideas.

## Important Things

I'm not going to lie. My overall dream/vision for this game is ambitious. I don't want to scare you off. We can scale back as appropriate, but, if I could make any kind of stealth game, it would have the following properties:

- strong core mechanics that include fluid, kinetic player movement
- deep, responsive enemy AI that can adapt and coordinate with each other
- gameplay and level design geared toward giving the player options (I'd prefer a small number of combinable mechanics over a larger number of isolated ones)
- a fewer number of large, replayable levels with:
    - dynamic enemy patrols
    - randomized side mission objectives
    - scoring system that encourages the player to try new tactics
    - "shifting the balance of power" options (i.e. destroying power source or neutralizing base communications)
    - lots of pathways to discover
    - multiple points at which to enter and exit levels

### Gameplay

When it comes to _narrative_ vs _gameplay_, I always side with gameplay. To be clear, I very much welcome the idea of including a fantastic story. All I'm saying is that, to my tastes, a pretty-decent story with really fun gameplay is the limit of my personal hopes and dreams.

I would like a game that can be playable from beginning to end without ever requiring the player to engage with combat mechanics. There can be rare exceptions for story beats or other tailored moments, but I want this game to be - above all else - a stealth focused game.

#### Core Mechanics

I think the core mechanics should include:

- variable movement speed, affecting enemy hearing and vision range
- stance switching between standing, crouching, and crawling, also affecting enemy hearing and vision range
- binoculars w/ directional microphone for seeing/hearing further across the level
- a pistol for holding up/interrogating enemies and limited combat
- a basic melee attack for escaping combat
- ability to "hear" enemy footsteps (i.e. a visual representation on screen) when the player is not moving
- inability to see enemies through walls

We'll want to think of more mechanics to layer on top of these, but those are TBD as of now. I think those will come once we lock down story, characters, and setting.

#### Enemy AI

As is typical in stealth games, rank-and-file enemy awareness will have different states depending on player actions. There will also be a _base_ awareness level which is triggered by an enemy reporting their awareness level to central dispatch followed by dispatch communicating the awareness level to all other enemies. Some awareness levels do not apply to the base. I'm envisioning an event-driven system with a robust state machine for driving enemy awareness states as well as their ability to coordinate with surrounding enemies or call for back up. There will likely be bosses and other enemy types (yet to be designed) which follow different rules.

##### Passive State

This is the default starting state for enemies. In this state the enemy has no idea the player is around and is conducting their normal patrol. Seeing something from a distance or hearing something innocuous (like a footstep) will drive the enemy into a `curious` state.

##### Curious State

In this state the enemy's attention has been drawn. They will look toward the source of the distraction. If they continue to have their attention drawn they will progress to a `concerned` state. Otherwise they will digress back to a `passive` state.

##### Concerned State

In this state the enemy's attention has been drawn enough that they feel the need to investigate. They _may_ also choose to speak out to nearby enemies or radio to central dispatch. This will also put relevant enemies into a `concerned` state. If the enemy's concerns are abated, they may return to a `passive` state. However, if an enemy repeatedly enters a `concerned` state they may progress to an `alerted` state instead.

##### Alerted State

In this state, the enemy is aware there is an intruder, but has no idea on the player's location. Their movement is faster, their patrols will change, and their senses become more accute. The same triggers that put an enemy into a `curious` or `concerned` state from a `passive` state will cause an `alerted` enemy to enter an `alarmed` state. They will also call out or radio in _anything_ not already reported. Once an enemy enters an `alerted` state, they will not digress any lower. Certain sounds (like gunshots or explosions) will immediately transition any enemy within earshot into an `alerted` state unless they are already at a higher state.

##### Alarmed State

In this state, the enemy will move to investigate the sound/sight that `alarmed` them. If they discover the player, they will enter an `agressive` state. Otherwise, they will return to an `alerted` state.

##### Aggressive State

In this state, enemies are aware of the player's presense and location. They are in open combat and actively trying to kill the player. If the player breaks line-of-sight for long enough, enemies will progress to a `searching` state.

##### Searching State

In this state, enemies are aware of the player's presense and will patrol the area around their last known location for a time. Within the search zone, players may not be safe within zones (i.e. hiding spots) that would normally conceal them. If player is spotted, the enemy will re-enter the `aggressive` state. Otherwise, they will digress to the `alerted` state.

##### Neutralized State

If the player holds up an enemy during stealth play or knocks out an enemy during combat, the enemy will be neutralized for a short time. Afterwards, they will enter an `alerted` state (or higher if the base is at a higher state).

#### Level Design

I'm envisioning a larger, open level design that let's the player explore and search for mission targets. The main level area will be surrounded by a handful of small guard posts with a small buffer separating them from the main area. Each guard post will correspond with an entry point to the level. Individually, the guard posts will offer minimal challenge to deal with or sneak past, but, should the player trigger the base into an `aggressive` state, re-enforcements will be called in from the guard posts - giving the player a strategic option to neutralize some or all of them before tackling the main area.

## WIP Plans to Get Us Started

I thought it would be helpful to have some ideas to kick things off. I think there's a lot of opportunity to be creative in this space, and I'm not a particularly creative person. So, I welcome ideas to improve or replace the ideas in this section.

### Number of Ds

I think we can get a lot of mileage out of a 2D game. If others are keen to go 3D I'd be down, but I'd rather spend time and effort making a better 2D game then going all in on a 3D game. It's worth noting that most of the systems I've built for the POC should scale up to 3D should we decided to go that route.

### Narrative

Gear up. The story below is trope-y and cliche, but I think it could be fun if we handle it with the right tone, namely: witty, playful, and self-aware. Think _Monkey Island™_ or _Portal_.

#### Story Outline

Melanie "Mel" Cartright is a U.S. park ranger, animal rights activist, and devout pacifist. She leads a simple life trying to be a good friend and neighbor, and attempts to keep a close relationship with her anti-social twin sister, Francine (aka "Cici"). In her home one night, Mel is abducted by a group of unidentified assailants. When she comes to, she discovers that Cici has been leading a secret life as a world-renouned assassin, codenamed "One Shot". Unfortunately, Cici's most recent assignment got her captured and Mel was abducted to be used as leverage over Cici. Cici manages to create an opportunity for Mel to escape, who reluctantly fleas leaving her sister behind.

After Mel narrowly escapes, she is contacted by Dick Babbit who explains that he works with Cici and that he needs Mel's help to rescue her. In order to do so, Mel must pose as Cici so they can leverage the resources and contacts of The Syndicate - Cici and Babbit's employer - to track down the shadow organization that has abucted Cici and rescue her. Babbit insists that if they are to stay under The Syndicate's radar, Mel will have to do everything in her power to make them believe she is Cici, including the need for her to adopt Cici's trademark style of only ever firing _one shot_ per assignment. Being a pacifist, Mel doesn't think it will be a problem for her to fire at most one shot. She's far more worried that she'll be able to shoot anyone at all when the occasion requires it. But, her sister needs her.

As it turns out, Babbit had been undermining The Syndicate and using Cici to lay the groundwork for his own evil schemes all along. Cici disovered the plot and was captured while trying to thwart Babbit's plans. Mel was abducted to blackmail Cici into finishing the job, but, when Mel showed aptitude by escaping, Babbit adjusted course and started using Mel in Cici's place. Around the time Mel puts the pieces of Babbit's treachery together, Cici escapes on her own - because of course she does; she's a highly capable super assassin. Cici finds Mel, and the two of them team up to stop Babbit once and for all.

### Art

I think we can all agree that the art I've created for the POC cannot be improved upon.

#### Boss Fights

I only have vague ideas about this, but I think the boss fights should also be "stealthable".

#### HUD

I think it makes sense to provide a square aspect ration for the playable area. This will make it so levels won't be biased toward left/right vs up/down view distances. We want to provide the most important visual feedback to the player within the playspace as not to draw their eye away from it as much as possible. For this, we'll lean on the TBD art direction. Additional, ancillary information can be included in HUD elements on the left and right of the screen.

For example, short radio dialogs between characters can display closeup avatars talking to each other on either side of the screen within the HUD area without haulting gameplay.

### Music

Speed metal.

Seriously though, I don't have strong opinions, but I'm happy to lead this up if no one else wants to.

## Additional Ideas

Here are some untethered brainstorming ideas.

- **Discouraging Combat** In addition to player score penalty for firing more than one bullet, we can have killing people take a "mental toll" on Mel and she'll go mad - or something - if she kills too many enemies.
- **Difficulty Settings** If we choose to support variable difficulty settings, we could tie it to the player's loadout gear. For example:
    - equiping extra-silent boots isn't an upgrade to be unlocked, it's a difficulty lever.
- **Character Customization** If we want to allow the player to customize name, pronouns, race, sexual orientation, etc, I think we should make sure it has absolutely no impact on the gameplay and is - at most - only casually referenced within the story.
- **Enemy Senses**
    - after the player crawls through sewers the enemy can smell them for a period of time
- **Enemy Equipment** How can we include different types of detection and in what ways can the player counter them?
    - cameras
    - trip wires
    - motion sensors
    - heat sensors
    - radio communications
    - power sources
- **Additional Traversal Options**
    - crawl spaces
    - climbing gear
    - grappling hook
    - hanging from ledges
    - ziplines
