# One Shot - Game Design Document

## THIS DOCUMENT IS WIP

This is an extremely WIP game design document. The details are mostly vague. Many details are missing entirely. This document will evolve and change over time as we figure out what works and what doesn't.

## Game Overview

This design document describes the details for **One Shot**, a top-down, sandbox-style stealth action game.

## Story and Narrative

Gear up. The story synopsis below is trope-y and cliche, but it could be fun if we handle it with the right tone (i.e. witty, playful, and highly self-aware). Think _Monkey Island™_ or _Portal_.

### Synopsis

Melanie "Mel" Cartwright is a U.S. park ranger, animal rights activist, and devout pacifist. She leads a simple life trying to be a good friend and neighbor, and attempts to keep a close relationship with her anti-social twin sister, Francine (aka "Cici"). In her home one night, Mel is abducted by a group of unidentified assailants. When she comes to, she finds herself locked in a cell with Cici. She learns that Cici has been leading a secret life as a world-renowned assassin, codenamed "One Shot". Unfortunately, Cici's most recent assignment got her captured and Mel was abducted to be used as leverage over Cici. Cici manages to create an opportunity for Mel to escape, who reluctantly fleas leaving her sister behind.

After Mel narrowly escapes, she is contacted by Dick Babbit who explains that he works with Cici and that he needs Mel's help to rescue her. In order to do so, Mel must pose as Cici so they can leverage the contacts and resources of Furtive Solutions (FS) - Cici and Babbit's employer - to track down the shadow organization that has abucted Cici, and rescue her. Babbit insists that if they are to stay under FS's radar, Mel will have to convince them she is actually Cici, including the need for her to adopt Cici's trademark style of killing no one but her target.

"Am I going to have to _kill_ people?" Mel asks, trepidly. "Cici," she mutters to herself, "what have you gotten me into."

"Possibly. But don't worry. They'll definitely kill you if you don't kill them."

"That's comforting."

Being a pacifist, Mel doesn't think it will be a problem for her to only kill one target. She's far more worried that she'll be able to shoot anyone at all when the occasion requires it. But, her sister needs her.

However, Babbit has his own reasons for staying under FS's radar. He had been undermining them and using Cici to lay the groundwork for establishing his own evil organization, Plunderbund Inc. Cici disovered the plot and was captured while trying to thwart Babbit's plans. Mel was abducted to blackmail Cici into finishing the job, but, when Mel showed aptitude by escaping, Babbit adjusted course and started using Mel in Cici's place. Around the time Mel puts the pieces of Babbit's treachery together, Cici escapes on her own - because of course she does; she's a highly capable super assassin. Cici finds Mel, and the two of them team up to stop Babbit before it's too late.

## Gameplay

The main structure of the game will feature a series of large "bases" to infiltrate. As each level is completed, the area is unlocked for main mission replay as well as additional randomized side challenges. Completing levels is the primary mechanism for progressing the story.

### Objectives

The gameplay objectives should encourage player freedom by dictating _what_ must be done, but never _how_ it is to be done. The location of mission objectives will be randomized and unknown to the player when the mission begins. The mission objective locations will need to be discovered through finding intel, interrogating enemies, or simply searching the level. Mission objectives can present themselves as multi-part. For example: a mission objective is inside a building which requires a key for entry, adding an additional objective of finding/acquiring the key.

### Mission/challenge Structure

Some example mission objectives:
- interrogate enemies for information
- obtain documents
- plant misinformation
- rescue detainees
- sabotage enemy equipment
- steal equipment prototype
- neutralize key base personell
- neutralize _all_ base personell

### Systemic Opportunities

We'll provide the player with some exploitable, systemic opportunities for sneaking through the level unseen so they can dynamically adjust the difficult to suit their tastes (like grinding).

- Rotating patrols
    Each level has multiple patrol areas/paths for guards to monitor. The patrols will be assigned randomly. Periodically, a guard will have their patrol reassigned causing them to navigate to a different part of the level.
- Shift change
    On a repeating interval, `n` guards will change shifts at once. The start of this procedure should be obvious to the player and will mean there will temporarily be `n` extra enemies in play, followed by `n` fewer enemies (while the shift change occurs out of level bounds), and finally back to the starting number of enemies. But they will be heading to/from a predictable location for the shift change.
- Destroy/sabotage power substations
    Doing this will knock out lights and other electronic security equipment until the backup generator comes online. All enemies are elevated to "alert" status (if not already higher).
- Destroy/sabotage base communications
    This disables enemies' ability to communicate/coordinate with the rest of the base. All enemies are elevated to "alert" status (if not already higher).
- Neutralize base communications officer
    This is a hard-to-find/hard-to-get-to enemy who moves about the level. Neutralizing them will allow you to hijack base comms and feed enemies misinformation (i.e. false player location, trigger a shift change, etc.).
    Enemies will eventually figure out the ruse and stop taking false instructions.

### Game story progression

Narrative and character information will be relayed to the player through in-game conversations. The story beats will primarily be delivered via radio comms between Mel and Babbit. Character progression will mostly come in the form of imaginary conversations between Mel and Cici (as Mel's alterego). This allows the player to build a relationship with Mel and empathy for Cici.

Meatier story/character moments can happen much the same way, but with the aid of on-screen avatars to punctuate the converstations. These will be less frequent and only take place at the beginning/end of levels and/or at key moments within a level (i.e. a mission objective is updated).

The most critical story beats will be delivered through interstitial, skippable cutscenes that will bookend levels. The cutscenes will display as panel-style storyboards with simple parallaxing and minimal (if any) animations. Some examples of what I mean are: Infamous, Metal Gear Solid Peacewalker, or Gravity Rush.

## Mechanics

In earnest, this game is mechanically a spiritual successor to Metal Gear Solid V with some simplifications and adaptations made to accomdate for a narrower scope and fixed camera perspective.

### Core Mechanics

#### Player Stance

During normal play, the player will move about the space in different stances. Each stance effects how easily the player is seen, how loud the player's movement is, and how well the player can see the level around them.

- Crouching
    - The default stance allows the player to move and see around the level at the risk of being seen or heard by the enemy
    - scope/aim/melee/shooting can be done while staying still or moving in this stance
    - player can see over shorter obstacles/windows/etc
- Crawling
    - The player is difficult to see and very difficult to hear in this stance at the cost of their movement and viewport being less favorable
    - scope/aim/melee/shooting can _only_ be done while staying still in this stance
    - player cannot see over short obstacles into windows
- Running
    - enemies will see and hear running player from very far away
    - primarily used for retreating from combat or covering large distances quickly
    - if the player stops moving they will transition the back to a crouching stance
    - scope/aim/looking while running will transition the player back to a crouching stance

#### Player Movement Speed

Crouching and Crawling will each feature a _fast_ and _slow_ movement speed option. The player will be slightly harder to see and _much_ harder to hear when moving slowly.

#### Looking

While crouching, the player can move and look in two different directions. This is true for general movement, aiming, or scoping. Facing a direction that is different than the movement direction will effect movement speed.

#### Scope

The scope is used to scout the playable area ahead at a greater distance. The scope is equipped with a directional microphone so footsteps and converstations can be heard from further away as well.

#### Peeking

This will allow players to see over certain obstacles or around corners without exposing themselves.

#### Gun

The player is equipped with a standard issue sidearm which can be used for a number of purposes
- shooting near enemies to distract/confuse them
- destroying lights/cameras and other smaller equipment
- holding up/interrogating guards

We will prevent the player from killing enemies outside of combat. Within combat, the player will have a stress meter (life gauge) which will rise more quickly if they stand and fight vs run away. If the stress meter maxes out, the player loses consiousness (fail state),

#### Basic Vertical Movement

- stairs
- ledge shimmying
- jumping/diving/falling from a roof/higher level
- ladders
- drainpipes
- certain climbable obstacles (i.e. crates)

### Equipment

Furtive Solutions is well funded and highly invested in providing state-of-the-art tools and gadgets to enable their field agents to complete their assignments within FS's high standards of excellence. Expect some cool surprises from R&D.

#### Time Bender Distender aka TBD

Yes. It's just a placeholder name. TBD is a device built into Mel's suit which emits a pulse that slows down time for a few seconds. Usage economics (i.e. ammo, cooldown, cost, penalty) are TBD.... see what I did there?

## Enemies

### Enemy Types

- basic patrolling enemy
- snipers
    - stationary
    - see long distances
- hunters
    - can see and follow player's footsteps
    - will search in "safe zones"

### Awareness States

As is typical in stealth games, rank-and-file enemy awareness will have different states depending on player actions. There will also be a _base awareness level_ which is triggered by an enemy reporting their awareness level to the base communications officer (or allied doppleganger) followed by dispatch communicating the awareness level to all other enemies. Some awareness levels do not apply to the base. I'm envisioning an event-driven system with a robust state machine for driving enemy awareness states as well as their ability to coordinate with surrounding enemies or call for back up. What follows is a high level rough draft of the enemy awareness system.

#### PASSIVE

This is the default starting state for enemies. In this state the enemy has no idea the player is around and is conducting their normal patrol. Seeing something from a distance or hearing something innocuous (like a footstep) will drive the enemy into a `CURIOUS` state.

#### CURIOUS

In this state the player has begun to draw the enemy's attention. They will look toward the source of the distraction. If they continue to have their attention drawn they will progress to a `INVESTIGATING` state. Otherwise they will digress back to a `PASSIVE` state.

#### INVESTIGATING

In this state the enemy's attention has been drawn enough that they feel the need to investigate. They _may_ also choose to speak out to nearby enemies or radio to central dispatch. This will also put relevant enemies into a `INVESTIGATING` state. If the enemy positively identifies the player, they will enter an `AGRESSIVE` state. If the enemy's concerns are abated, they may return to a `PASSIVE` state. However, if an enemy repeatedly enters an `INVESTIGATING` state they may progress to an `ALERT` state instead.

#### ALERT

In this state, the enemy is aware there is an intruder, but has no idea on the player's location. Their movements are faster, their patrols will change, and their vision and hearing become more sensitive with a broader range. The same triggers that put an enemy into a `CURIOUS` or `INVESTIGATING` state from a `PASSIVE` state will cause an `ALERT` enemy to enter an `ALERT_INVESTIGATING` state. Certain sounds (like gunshots or explosions) will immediately transition any enemy within earshot into an `ALERT` state unless they are already at a higher state.

After a time of remaining undected, enemies and the base will return from an `ALERT` state to a `PASSIVE` state.

#### ALERT_INVESTIGATING

In this state, the enemy will move to investigate the sound/sight that drew their attention. If they discover the player, they will enter an `AGRESSIVE` state. Otherwise, they will return to an `ALERT` state.

#### AGRESSIVE

In this state, enemies are aware of the player's presense and location. They are in open combat and actively trying to kill the player. If the player breaks line-of-sight for long enough, enemies will progress to a `SEARCHING` state.

#### SEARCHING

In this state, enemies are aware of the player's presense and will patrol the area around their last known location for a time. Enemies will search hiding spots within the search area. If the player is spotted, the enemy will re-enter the `AGRESSIVE` state. Otherwise, they will digress to the `ALERT` state.
