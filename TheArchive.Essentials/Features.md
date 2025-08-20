# TheArchive.Essentials Features

* [Accessibility](#accessibility)
* [Audio](#audio)
* [Cosmetic](#cosmetic)
* [Fixes](#fixes)
* [HUD / UI](#hud--ui)
* [Misc](#misc)
* [Quality of Life](#quality-of-life)
* [Security / Anti Cheat](#security--anti-cheat)


## Accessibility

### Chat Tweaks

Use (Up/Down) arrow keys to cycle through the last sent messages.
CTRL+C copies and CTRL+V pastes into the chat box

### Disable Ambient Particles

Disable the little floating dust particles in the air.

### Disable Downed Sound

Removes the droning sound that playes whenever you're downed.

### Disable HUD Sway

Disables the in-game HUD sway while walking / jumping around.

### Disable UI Mirroring

Removes the mirroring effect of UI elements.

### Disable UI Paralax

Disables the Paralax/Moving of UI elements in menu screens.
(Some elements might get partially or fully cut off-screen!)

(Only partially works on R4://EXT builds, might fix later.)

### Glass Liquid System Override - `[R2-RL]`

Adjust the games "Glass Liquid System"
The thing that renders the blood splatters etc on your visor.

### Nickname

Nickname related settings.

Change your in game nickname, handy color picker included!

### Player Color Override

Override the built in player colors.

### Sentry Markers

Add hud markers onto placed down sentry guns and tweak how those are shown.


## Audio

### Disable Artifact Sound Loop - `[R5-RL]`

Removes the Artifacts idle audio loop.

### Disable Breathing (Infection) - `[R2-RL]`

Disables the infection "drinking-straw-sucking" sounding sound.

### Disable Breathing (Stamina)

Disables the player breathing and panting due to running around or enemy encounters in pre-R6 builds.

### Disable Coughing (Infection) - `[R2-RL]`

Disables the cough sound effect whenever a player looses HP due to infection.

(SoundEvent="PLAY_COUGHSOFT01")

### Disable Respawn Sack Audio - `[R5-RL]`

Prevents Respawn Sacks from emitting audio.

### Disable Spitter Audio - `[R2-RL]`

Completely Removes all Audio from spitters.

Keep in mind that you won't get any auditory warnings before it's too late

### Loud Volume Override

Lower the game volume during loud sections:
- game intro
- elevator drop

Adjust alt-tab sound behavior.


## Cosmetic

### Bot Customization - `[R6-RL]`

Customize your bots - Change their name and Vanity

Adds the Apparel button to bots if you're host.
(Bot clothing only works if dropping from lobby atm!)

### Disable Bullet Tracers

Removes Bullet Tracer Effects

### Enable old Hammers - `[R6-A6]`

Re-enable the pre-R6 Hammers:
Maul, Gavel, Sledge and Mallet

### Glowsticks! - `[A1-RL]`

Customize your glow-y little friends!

Allows you to change the built in glowstick type and/or customize the color to your liking, or color it based on the player who threw the glowstick.

### Vanity Dirt Control - `[R6-RL]`

Set all vanity items (clothes) dirt amount.

### Weapon FOV Adjustments

Adjust the Field of View of weapons, consumables and big items.


## Fixes

### 99% Reload Fix

Fixes the bug that leaves you with one bullet short in the mag.
(Currently only for IL2CPP builds)

### Bio Stuck Sound Fix

Stops the tagging progress sound after unwielding the tracker, just in case the sound gets stuck.

### Bio Tracker Small Red Dots - `[R6-RL]`

Fixes tiny red dots on the bio tracker.

### Decay IRF NRE Fix

Fixes enemies with invalid IRFs spamming the console on death.

Specifically 'tank_boss'

### Interaction Fix

Prevents resource packs from getting interrupted from other interacts.
Most notably: running past lockers etc

(Text might sometimes disappear)

### Kill Indicator Fix - `[R6-RL]`

Fixes orange kill indicators not being consistent for clients.

### Map Chat Abduction Fix - `[R4-A5]`

Prevent a switch to the Objectives Screen whenever the chat is open and the 'o' key is pressed.

(Thanks for fixing this in A6 Alex! <3)

### Map Pan Unclamp

Remove the MMB Map panning restrictions.
Makes you able to zoom in on far out zones.

### No Dead Pings

Prevents bio pings (/\) on dead enemies.

### Pouncer ScreenFX Stuck Fix - `[R7-RL]`

(WIP) Prevents the pouncer tentacles from getting stuck on screen.

### Weapons shoot forward - `[R1-A5]`

Patches weapons to always shoot into the center of your crosshair.
Makes shotgun draw & insta-shoot not shoot the floor


## HUD / UI

### Bio Ping Colors

Customize the color of Bio Tracker Pings as well as the blobs on its display.

Single color, does not differentiate between enemies.

### Carry Item Marker - `[R2-RL]`

Adds a marker for whenever someone carries a big pickup like CELLs or FOG_TURBINEs

Additionally colorizes the marker based on what item it is.

### Combat Indicator

Displays the current drama state of the game.
(Above the health bar, right side)

Basically a visual representation of what the music is doing.

### Detailed Expedition Display

Adds the current Rundown Number into the Header as well as onto the Map, Objectives and Success screens.

### Display Sentry Type

Display the Sentry Type (Sniper, Burst, Auto, Shotgun) for remote players instead of the nondescript "Sentry Gun" on the map screen.

### Don't Hide Loadout UI - `[R6-RL]`

Keep loadout visible after readying up / in expedition

### Enhanced Expedition Timer

A more accurate mission timer.

### Fix Multi-Revive UI - `[R6-RL]`

Fix revive progress visually resetting whenever multiple revives are going on at the same time.

### Flashlight Icon Colors - `[R5-RL]`

Customize the flashlight on/off indicator colors.

### Hud Pro Mode (Disable HUD)

Force disable ALL HUD layers unless re-enabled via the submenu.

Main purpose is for video production

### Hud Toggle

Keybind to toggle parts of the HUD

### Loading Indicator

Displays a little indicator that shows if other players have finished loading yet.

### Log Visualizer - `[R8]`

Missing some logs for that Achievement, huh?

### Remove Downed Message

Completely removes the 'You are dead, use TAB to check map' message.

### Results Screen Tweaks

Tweak the Expedition Fail/Success screens!

### Scan HUD Tweaks - `[R6-RL]`

Adds an overall alarm class counter to the HUD message for door alarms etc

### Show Weapon Stats

Adds weapon statistics such as damage, clip size and reload speed (and more if applicable) on the weapon select screen.

### Watermark Tweaks

Configurable to either show your current position, a timer or the mod version in the bottom right:
- X:24 Y:2 Z:-46
- Timer showing elapsed mission time
- TheArchive v2025.1.0

### Weapon Picker Tweaks - `[R6-RL]`

Allows you to Favorite and Hide Gear in the weapon picker.


## Misc

### AdBlock - `[R8]`

Removes the Den of Wolves button from the rundown screen.

### Alt Tab Counter

Counts the amount of times that the game went out of focus. (ALT + TAB)

### Mute Speak - `[R6-RL]`

Binds a few voice lines to keyboard keys.

Arrow keys
[P, L, K, J, H] toggleable by hitting F8; off by default
Hold [Right Control] for alternate lines

### Process Priority

Set the games process priority.

This does the same thing as opening up Taskmanager, going into the 'Details' tab and right clicking on GTFO.exe > [Set Priority]

Warning! Your system might lag / stutter while the game is loading if set to AboveNormal or higher!

### Remove Chat Restrictions - `[R4-RL]`

Allows the usage of '>' and '<' characters in chat.

(Also enables TextMeshPro RichText tags to be used in chat, don't do stupid things!)

### Weapon Model Toggle

Forces the held item to be hidden.
Intended for taking pictures.
(Warning! This makes you unable to use or switch items until unhidden!)


## Quality of Life

### L4D Style Resource Packs

Use left and right mouse buttons to apply resource packs instead of E.

Left mouse = yourself
Right mouse = other players

[R4+] You're able to hold down M2 and it will start applying to a receiver under your croshair if in range automatically

/!\ Make sure to disable the vanilla game setting Gameplay > Separate Use Keybinds for this Feature to work!

### Last Used Gear Switcher

Allows you to swap between the last two used weapons via a keypress

### Loadout Randomizer

Adds a Loadout Randomizer button onto the loadout screen.
Select which gear to randomize via the settings below.

### No Magazine Drop Sound - `[R7-RL]`

Removes the globally audible sound whenever a magazine drops on the floor after a reload.

### Prioritize Resource Pings - `[R1-A5]`

Resource Packs will be prioritized and show up with special icons and trigger voice lines when pinged by Middle-Mouse-Pings.
(Yes, disinfect is ammo apparently)

### Reload Sound Cue

Play a sound cue on reload the moment the bullets have entered your gun.

### Remove Story Dialog - `[R6-RL]`

Removes all level-based voice events that come with subtitles.
aka Schaeffer-be-gone

### See NavMarkers in Chat

Prevent enemy pings from hiding whenever the chat is open.

### Situation Aware Weapon Switch

Switch to either your Melee weapon or Primary depending on if you're sneaking around or in combat after depleting all of your throwables, exit a ladder or place down a sentry gun etc.

### Skip Elevator Animation

Automatically skips the elevator intro animation sequence without having to hold down a button.

### Skip Intro

Automatically presses inject at the start of the game

### Sort Boosters - `[R6-RL]`

Sorts your booster inventory by type and alphabetically


## Security / Anti Cheat

### Anti Booster Hack

Prevents clients from using modified boosters.

### Anti Gear Hack

Prevents clients from using modified gear.

### Anti Spawn - `[R6-RL]`

Prevents clients from spawning in enemies.

### Player Lobby Management

Allows you to open a players steam profile by clicking on their name as well as kick and ban players as host.

