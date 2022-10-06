# TheArchive

A [MelonLoader](https://github.com/LavaGang/MelonLoader) based [GTFO](https://gtfothegame.com/) mod that's aiming to preserve older versions by keeping them playable even once the servers are gone forever **by handling all playfab requests locally** and saving things like **progression/level completions**, **boosters** and **vanity items** onto your storage device of choice.

Compatible with many different GTFO builds, tested on all of the latest patches for each rundown.

### Quick Links
 * [Highlighted Features](#Features)
   * [Backported Features](#Backported-Features)
   * [Accessibility](#Accessibility)
   * [Quality of Life](#Quality-of-Life)
   * [Misc / Other](#Misc--Other)
 * [How to Install](#Installation)
 * [Where are my saves?](#Where-are-my-saves)
 * [Building the project](#Technical-stuffs)

# Features

## Local Rundown Progression
This allows you to save your progress (including boosters and vanity items) onto your computers hard drive instead of depending on the developers servers.  
The implementation should be as close to the original game whenever a specific (rundown) version was live, to keep the experience genuine even after servers are down forever.

## In Game Mod Settings

An in-game mod settings menu for easy feature customization.  
Most Features are toggleable mid game and some even have extra options to mess with!

![Mod Settings](https://user-images.githubusercontent.com/37329066/190881761-1c0550c3-2d2e-4e74-9904-d0f439b96f24.png)

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

Rundown 1 does not have a visual ping indicator that shows up nowadays after using the `PING` command inside of a terminal but merely the ping sound.  
This allows the lobby host to enable a visual indicator by abusing their middle mouse ping.

---

## Accessibility

### Nickname

Change your name in game, includes a color option.

### Player Color Override

Allows you to change the colors of you and your teammates.

---

## Quality of Life

### Prioritize Resource Pings

Instead of pinging the lockers/boxes with your middle mouse pings it pings the resource packs instead.  
This changes the ping icon as well as the voice line your character says.

### Loadout Randomizer

Adds a button onto the loadout screen that allows you to randomize your current loadout.  
(Configurable in mod settings)

### Map Abduction Fix - `[R4 - RL]`

Ever tried typing on the map screen only to be taken to the objectives screen after hitting the `o` key on your keyboard?  
This has been fixed.

### Unready Button - `[R1 - R5]`

Adds an unready button into the older game versions.

---

## Misc / Other

### Player Info

Allows you to click a players name in the lobby or the map screen which opens up their steam profile page in your default browser.

### Hud Toggle

Toggle your Hud via a key press.

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
* R5 (steam manifest: `2154682358008197814`)
    * Obtain the last Rundown 5 build, install [MelonLoader](https://github.com/LavaGang/MelonLoader) and run the game once.
    * Copy the `MelonLoader` folder from the game directory into `_R_RD005/`
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
├── _R_RD005/                               # Rundown 5 Assemblies go here
│   └── MelonLoader/
│       ├── Managed/
│       │   ├── Accessibility.dll
│       │   ├── Addons-ASM.dll
│       │   └── ...
│       └── MelonLoader.dll
└── .../                                    # Other Project Folders / Files
```

### Step 2: Building
1. Open the solution `TheArchive.sln` in Visual Studio
2. Hit `CTRL + Shift + B` on your keyboard or alternatively use the `Build > Build Solution` menubar option
3. The project is now building and the final dll is going to be placed into the `out/` directory
## Building the project (On Linux)

Have fun, you'll figure it out.

# License

Everything in [this repository](https://github.com/AuriRex/GTFO_TheArchive) is licensed under the MIT License (unless stated otherwise inside of a given source file),
**excluding** `TheArchive.Core/Resources/discord_game_sdk.dll` and all of the files inside of `TheArchive.Core/Core/DiscordApi/*`, which are copyright [Discord](https://discord.com/developers/docs/legal) and only included for convenience.
