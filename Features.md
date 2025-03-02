# Features

  * [Accessibility](#accessibility)
  * [Archive Core](#archive-core)
  * [Backport](#backport)
  * [Cosmetic](#cosmetic)
  * [Discord / Steam Presence](#discord--steam-presence)
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

### Disable Breathing (Infection) - `[R2-RL]`

Disables the infection "drinking-straw-sucking" sounding sound.

### Disable Breathing (Stamina)

Disables the player breathing and panting due to running around or enemy encounters in pre-R6 builds.

### Disable Coughing (Infection) - `[R2-RL]`

Disables the cough sound effect whenever a player looses HP due to infection.

(SoundEvent="PLAY_COUGHSOFT01")

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

### Loud Volume Override

Lower the game volume during loud sections:
 - game intro
 - elevator drop

Adjust alt-tab sound behavior.

### Nickname

Nickname related settings.

Change your in game nickname, handy color picker included!

### Player Color Override

Override the built in player colors.

### Sentry Markers

Add hud markers onto placed down sentry guns and tweak how those are shown.


## Archive Core

### Update Notifier - `[R5-RL]`

Shows a popup whenever a new version is available.


## Backport

### Alarm Class on Security Doors - `[R1-R3]`

Add alarm classes to security door interaction texts

### Center Map on Player - `[R1-R2]`

Center the map on yourself upon opening.

### Don't Pause Audio On Unfocus - `[R1]`

Audio in R1 was completely paused whenever the game lost focus resulting in sounds piling up and playing on re-focus.

### Modern Melee Charge Cancel - `[R1-R5]`

Returns the hammer back to neutral instead of shoving whenever you're charging and alt-fire is pressed.

### R2+ Like Ladders - `[R1]`

Fix ladder movement so that W is always upwards and S always downwards, no matter where you're looking.

### R5+ Like Instant Hack Release - `[R1-R4]`

Change hacking minigame to be more in line with newest version of the game
Minigame finishes and hack disappears instantly

### R5+ Mines - `[R1-R4]`

Change explosion code to work like after the R5 update.
Mines = more effective

(Might cause desync!)

### R6+ Terminal Key / Zone Info - `[R1-R5]`

Adds the following text at the start of every terminal:
"Welcome to TERMINAL_XYZ, located in ZONE_XY"
(except for reactor terminals ...)

### Throwables Run / Fall Fix - `[R1-R5]`

Prevents you from accidentally throwing your C-Foam nade / Glowsticks whenever you start running / jumping

### Unready Button - `[R1-R5]`

Allows you to unready in the lobby.

### Visual Ping Indicators in R1 - `[R1]`

Visualize terminal pings in R1 by abusing the local players Middle-Mouse-Ping.
(Only works as Host)


## Cosmetic

### Bot Customization - `[R6-RL]`

Customize your bots - Change their name and Vanity

Adds the Apparel button to bots if you're host.
(Bot clothing only works if dropping from lobby atm!)

### Enable old Hammers - `[R6-RL]`

Re-enable the pre-R6 Hammers:
Maul, Gavel, Sledge and Mallet

### Glowsticks! - `[A1-RL]`

Costomize your glow-y little friends!

Allows you to change the built in glowstick type and/or customize the color to your liking, or color it based on the player who threw the glowstick.

### Vanity Dirt Control - `[R6-RL]`

Set all vanity items (clothes) dirt amount.

### Weapon FOV Adjustments

Adjust the Field of View of weapons, consumables and big items.


## Discord / Steam Presence

### Discord Rich Presence

Show the current game state in detail on discord.

### Steam Rich Presence Tweaks

Set a custom text for Steams' presence system.


## Fixes

### 99% Reload Fix

Fixes the bug that leaves you with one bullet short in the mag.
(Currently only for IL2CPP builds)

### Bio Stuck Sound Fix

Stops the tagging progress sound after unwielding the tracker, just in case the sound gets stuck.

### Interaction Fix

Prevents resource packs from getting interrupted from other interacts.
Most notably: running past lockers etc

(Text might sometimes disappear)

### Map Chat Abduction Fix - `[R4-A5]`

Prevent a switch to the Objectives Screen whenever the chat is open and the 'o' key is pressed.

(Thanks for fixing this in A6 Alex! <3)

### Map Pan Unclamp

Remove the MMB Map panning restrictions.
Makes you able to zoom in on far out zones.

### No Dead Pings

Prevents bio pings (/\) on dead enemies.

### Weapons shoot forward - `[R1-A5]`

Patches weapons to always shoot into the center of your crosshair.
Makes shotgun draw & insta-shoot not shoot the floor

### Kill Indicator Fix - `[R6-RL]`

Fixes client orange kill indicators to make them consistent.

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

### Hud Toggle (F1)

Keybind to toggle parts of the HUD

### Loading Indicator

Displays a little indicator that shows if other players have finished loading yet.

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
 - TheArchive v0.5.87

### Weapon Picker Tweaks - `[R6-RL]`

Allows you to Favorite and Hide Gear in the weapon picker.


## Misc

### Alt Tab Counter

Counts the ammount of times that the game went out of focus. (ALT + TAB)

### Copy Lobby ID Format

Customize copied lobby code from the 'Copy Lobby ID'-Button on the loadout and settings screens with a custom format.

### Mute Speak - `[R6-RL]`

Binds a few voice lines to keyboard keys.

Arrow keys
[P, L, K, J, H] toggleable by hitting F8; off by default
Hold [Right Control] for alternate lines

### Process Priority

Set the games process priority.

This does the same thing as opening up Taskmanager, going into the 'Details' tab and right clicking on GTFO.exe > [Set Priority]

Warning! Your system might lag / stutter while the game is loading if set to AboveNormal or higher!

### R1 connect to 19087 games - `[R1]`

Makes you able to join players who are playing on R1 build 19087 even though you're on 19715.

### Remove Chat Restrictions - `[R4-RL]`

Allows the usage of '>' and '<' characters in chat.

(Also enables TextMeshPro RichText tags to be used in chat, don't do stupid things!)

### Rundown 8 Reminder - `[A6]`

Reminds you to turn off "Remove Story Dialog" for whenever Rundown 8 drops!

### Weapon Model Toggle (F2)

Forces the held item to be hidden.
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

Removes the globalally audible sound whenever a magazine drops on the floor after a reload.

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

### Anti Spawn - `[R6-RL]`

Prevents clients from spawning in enemies.

### Player Lobby Management

Allows you to open a players steam profile by clicking on their name as well as kick and ban players as host.

