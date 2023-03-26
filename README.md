# TheArchive

A [MelonLoader](https://github.com/LavaGang/MelonLoader) based [GTFO](https://gtfothegame.com/) mod that's aiming to preserve older versions by keeping them playable even once the servers are gone forever **by handling all playfab requests locally** and saving things like **progression/level completions**, **boosters** and **vanity items** onto your storage device of choice.

Compatible with many different GTFO builds, tested on all of the latest patches for each rundown.

*Also adds a bunch of neat Quality of Life features I guess ...*

## Status
This project is in a Beta state at the moment, expect some bugs and unfinished features.

### Quick Links
 * [Highlighted Features](#Features)
   * [Backported Features](#Backported-Features)
   * [Accessibility](#Accessibility)
   * [Quality of Life](#Quality-of-Life)
   * [Misc / Other](#Misc--Other)
 * [How to Install](#Installation)
 * [Where are my saves?](#Where-are-my-saves)
 * [Building the project](#Technical-stuffs)
 * [Contributing & License](#Contributing)

# Features

## Local Rundown Progression
This allows you to save your progress (including boosters and vanity items) onto your computers hard drive instead of depending on the developers servers.  
The implementation should be as close to the original game whenever a specific (rundown) version was live, to keep the experience genuine even after servers are down forever.

## In Game Mod Settings

An in-game mod settings menu for easy feature customization.  
Most Features are toggleable mid game and some even have extra options to mess with!

<p align="center">
  <img src="https://user-images.githubusercontent.com/37329066/190881761-1c0550c3-2d2e-4e74-9904-d0f439b96f24.png" alt="Mod Settings"/>
</p>

## Discord Rich presence

Fully customizable Discord Rich Presence system to show others what you're up to.  
(currently only configurable through the config file)

![Rich Presence](https://user-images.githubusercontent.com/37329066/190882400-4be7c531-f863-4c3b-a703-34020f579aad.png)

## Settings Redirect

Game settings get saved to a different location for every Rundown so you only have to change them once*.  
(*for each major version once, a version agnostic settings menu is on the TODO list)

Having to redo your settings whenever you switch to another Rundown is now a thing of the past.

---

## Backported Features

### Instant Hack Release - `[R1 - R4]`

Rundown 5 changed hacks a tiny bit by unlocking locked objects sooner than before, this patch backports this into R1 to R4.

### Center Map on Player - `[R1 - R2]`

Center the map on yourself upon opening. (Has been added officially in R3)

### Melee Cancel Backport - `[R1 - R5]`

Rundown 6 replaced the shove on right click with a simple return of the weapon to it's idle position, this patch backports this all versions before R6.

### Mine Fix Backport - `[R1 - R4]`

According to the devs, mines didn't do the intended amount of damage before R5, this fixes the mine damage.

### Terminal Zone Info - `[R1 - R5]`

Rundown 6 added the terminals key and zone info into the terminal.

### Throwables Run Fix - `[R1 - R5]`

Before R6, trying to throw throwables and then sprinting caused you to instantly throw your item.  
This patch allows you to sprint without this happening.

### Alarm Class On Doors - `[R1 - R3]`

Always shows the alarm class on security doors, this was not the case in R1 to R3 and doors did not show this information.

### R1 Ladder Fix - `[R1]`

`W` is up and `S` is down, no more weird looking up/down changes the way those buttons work on ladders.

### R1 Visual Pings - `[R1]`

On Rundown 1 builds there is no visual ping indicator that shows up after using the `PING` command inside of a terminal, there's only the sound that plays.  
This feature allows the lobby host to display a visual indicator by abusing their middle mouse ping.

---

## Accessibility

### Nickname

Change your name in game, includes a color option.

### Player Color Override

Allows you to change the colors of you and your teammates.  
Additionally allows you to colorize other players based on their nickname color.

### Loud Volume Override

Lower or mute the game during the intro sequence and while dropping down with the elevator.  
Also allows you to adjust what happens with audio whenever you tab outside of the game:
* Continue playing
* Lower Volume
* Mute

### Glass Liquid System Override

Change the resolution of the system that renders the blood splatters and other liquids on your visor or disable it entirely.

<details>
<summary>📷 Glass Liquid Override Quality Settings Overview:</summary>

### Default Quality:
<p align="center">
  <img src="https://user-images.githubusercontent.com/37329066/227805471-00d214a2-7f56-4409-86f1-39ba147f909b.png" alt="GLSQualityDefault"/>
</p>

### Worst Quality: (`VeryBad`)
<p align="center">
  <img src="https://user-images.githubusercontent.com/37329066/227805489-12613b41-8d98-4f2f-9d68-fe1298fe66ad.png" alt="GLSQualityVeryBad"/>
</p>

### Best Quality: (`Extraordinary`)
<p align="center">
  <img src="https://user-images.githubusercontent.com/37329066/227805447-95aff551-2e71-4dc6-a414-f4b316cbb750.png" alt="GLSQualityExtraordinary"/>
</p>
</details>

### Sentry Markers

Adds a player colored marker on placed down sentry guns, with who placed it and the sentries type above it.

<details>
<summary>📷 Example:</summary>

![SentryMarkers](https://user-images.githubusercontent.com/37329066/227806849-933e2a23-7bb3-4352-b028-8c35974b4e26.png)
</details>

### Disable Breathing (Infection) - `[R2 - RL]`

Removes the sound effect than can only be described as "sucking up liquid through a straw" that playes while having high infection.

### Disable Breathing (Stamina)

Removes the players breathing and panting sounds while running around.

### Disable Coughing (Infection) - `[R2 - RL]`

Removes the coughing sound effect whenever someone looses HP due to infection.

### Disable Hud Sway

Makes the in-game hud stay in place while running, jumping and looking around.

### Disable UI Mirroring

Removes the mirroring effect on UI elements.

### Disable UI Paralax

Stops the movement of UI elements in menu screens (Loadout, Rundown, ...) whenever you move your cursor around.  
(Some elements might get partially or fully cut off-screen!)

---

## Quality of Life

### Carry Item Marker

Big pickups (like `CELL`s or `FOG_TURBINE`s) get their own color as well as the item name above it.  
Also shows a marker on whoever is carrying a big pickup.

<details>
<summary>📷 All the different color variations: (Big Pickup spoilers!!)</summary>

<p align="center">
  <img src="https://user-images.githubusercontent.com/37329066/227804227-207d47a7-54cb-49f7-936f-76e4d0c5068d.png" alt="CarryItemMarker"/>
</p>
</details>

<details>
<summary>📷 Example of someone carrying a CELL:</summary>

![CarryMarker](https://user-images.githubusercontent.com/37329066/227804434-22e8de81-6884-4830-9ff6-a5a8c4616cc2.png)
</details>

### 99% Reload Fix

Fixes the bug that leaves you with one bullet short in the mag even though enough ammo is available.

### L4D Style Resource Packs

Use left and right mouse buttons to apply resource packs instead of `E`  
Left mouse = yourself  
Right mouse = other players (can be held down + hovered over a player to start the interaction)

### Situation Aware Weapon Switch

Switch to either your Melee weapon or Primary depending on if you're sneaking around or in combat after depleting all of your throwables / exit a ladder.

### Prioritize Resource Pings

Instead of pinging the lockers/boxes with your middle mouse pings it pings the resource packs instead.  
This changes the ping icon as well as the voice line your character says.

### Loadout Randomizer

Adds a button onto the loadout screen that allows you to randomize your current loadout.  
(Configurable in mod settings)

### Map Abduction Fix - `[R4 - RL]`

Ever tried typing on the map screen only to be taken to the objectives screen after hitting the `o` key on your keyboard?  
This has been fixed.

### No Dead Pings

Fixes pings (red triangles, doritos) staying on dead enemies as a result of high ping.

### No Magazine Drop Sound

Removes the *globally audible* sound that playes whenever a magazine drops on the floor after a reload.

### Reload Sound Cue

Play a sound the moment your gun has reloaded. (= bullets have entered the gun)

### Remove Story Dialog

Prevents all level-based voice events that have subtitles assigned from playing.  
(Goodbye Schaefer & Co 😥)

### Skip Elevator Animation

Automatically skips the cutscene after initiating a cage drop.  
This leads to faster load times as the game only starts building the level once the cutscene is over.  

### Unready Button - `[R1 - R5]`

Adds an unready button into the older game versions.

---

## Misc / Other

### Player Lobby Management

Open up players Steam profile or, if you're the host, kick them out of your lobby.

![PlayerLobbyManagement](https://user-images.githubusercontent.com/37329066/227807583-03305c00-323c-446c-94df-43f9aa7595cd.png)

### Loading Indicator

Displays a little indicator that shows if other players have finished loading yet.

### Show Weapon Stats

Displayes the weapons stats on the weapon select screen.  
Damage, clip size, max ammo and more

### Hud Toggle

Toggle your Hud via a key press. `(F1)`

### Process Priority

Automatically change the games process priority, potentially increasing performance by a tiny bit.

### Remove Downed Message

Remove the "You have died, check map with TAB" text at the top of your screen whenever you die.  
Allows you to see reactor progress text when dead.

### Weapon Shoot Forwards

Tries to always aim your shots into the center of your crosshair, therefore not allowing your guns to shoot the floor upon drawing and immediately firing.

### R1 SNet Revision Override

Allows you to connect to R1 build `19087` games even though you're playing on R1 build `19715`.  
Build `19087` is/was a commonly redistributed version of Rundown 1

### Other things not mentioned

Yes, there's more! (smaller things)

---

# Installation

1. Install [MelonLoader](https://github.com/LavaGang/MelonLoader#how-to-use-the-installer) into your game folder.
2. Download the latest mod version from [here](https://github.com/AuriRex/GTFO_TheArchive/releases/latest)
3. Put the dll into the `Mods` folder inside of your GTFO directory

---

# Where are my saves?

By default, all mod files get saved to `%appdata%/../LocalLow/GTFO_TheArchive/`.  
Most things like progression, boosters and vanity get saved into the `SaveData` folder inside of the previous mentioned one, neatly divided into rundown specific folders.  
The location of this `SaveData` folder can be customized by editing `TheArchive_Settings.json`'s `"CustomFileSaveLocation"` property to point to any location of your choosing. (Cloud storage like GoogleDrive, Dropbox, etc ..., recommended)  
(*Make sure to escape backslashes (`\`) in your path by doubling them (like this: `\\`), else it won't work!*)

---

# Technical stuffs

## Building the project (On Windows)

### Step 1: Getting the game assemblies for references:
* Latest Game Version
    * Install [MelonLoader](https://github.com/LavaGang/MelonLoader) and run the game once
    * Copy the `MelonLoader` folder from the game directory into `_R_LATEST/`
* R3 (steam manifest: `1993854016152145129`)
    * Obtain the last Rundown 3 build, install [MelonLoader](https://github.com/LavaGang/MelonLoader) and run the game once.
    * Copy the `MelonLoader` folder from the game directory into `_R_RD003/`
    * Create a folder called `GTFO_Data` inside of `_R_RD003/`
    * Copy the `Managed` folder from the game directory `GTFO/GTFO_Data/Managed/` into the `_R_RD003/GTFO_Data/` folder

#### Folder structure:
```
.
├── _R_LATEST/                              # Latest Version Assemblies go here
│   └── MelonLoader/
│       ├── Managed/
│       │   ├── Accessibility.dll
│       │   ├── Addons-ASM.dll
│       │   └── ...
│       └── MelonLoader.dll
├── _R_RD003/                               # Rundown 3 Assemblies go here
│   ├── GTFO_Data/
│   │   └── Managed/
│   │       ├── Accessibility.dll
│   │       ├── Addons-ASM.dll
│   │       └── ...
│   └── MelonLoader/
│       └── MelonLoader.dll
└── .../                                    # Other Project Folders / Files
```

### Step 2: Building
1. Open the solution `TheArchive.sln` in Visual Studio
2. Hit `CTRL + Shift + B` on your keyboard or alternatively use the `Build > Build Solution` menubar option
3. The project is now building and the final dll is going to be placed into the `out/` directory
## Building the project (On Linux)

Have fun, you'll figure it out.

# Contributing

Feel free to create issues and pull requests to help me improve this massive project.  
⚠ **By submitting a pull request you agree to add your code under the projects license.** (see [below](#License))

# License

Everything in [this repository](https://github.com/AuriRex/GTFO_TheArchive) is licensed under the MIT License (unless stated otherwise inside of a given source file),
**excluding** `TheArchive.Core/Resources/discord_game_sdk.dll` and all of the files inside of `TheArchive.Core/Core/DiscordApi/*`, which are copyright [Discord](https://discord.com/developers/docs/legal) and only included for convenience.
