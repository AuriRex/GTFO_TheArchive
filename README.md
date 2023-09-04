# TheArchive

A massive mod for the game [GTFO](https://gtfothegame.com/), that adds a ton of Quality of Life and cosmetic features to the game without overstepping the games original design.

Not only compatible with the **latest release** on steam but also with many of the **older GTFO builds**, tested on all of the latest patches for each (old) rundown.  
Additionally this mod is trying to preserve older versions by keeping them playable even once the servers are gone forever **by handling all playfab requests locally** and saving things like **progression/level completions**, **boosters** and **vanity items** onto your storage device of choice for old versions.

## What this does
Improve the game via Quality of Life and cosmetic additions without disrupting the experience of other players.  
All mechanics are still kept vanilla and in spirit of the games original design.

## What this **doesn't** do
This ***does not*** give you access to the game or any of the old versions, you have to have bought the game on steam!  
This ***does not*** give any player an unfair advantage or trivialize the game, you have to bring your own skill.

## Status
This project is in a Beta state at the moment, overall it's pretty stable but expect some bugs and unfinished features.  

If you encounter any bugs while playing with mods installed make sure to remove your mods first and reproduce the issue without any installed before asking the games devs for support.

### Quick Links
 * [Highlighted Features](#features)
 * [How to Install](#installation)
 * [Where are my saves?](#where-are-my-saves)
 * [Building the project](#technical-stuffs)
 * [Contributing & License](#contributing)

# Features

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

## Re-Added Old Hammers - `[R6 - RL]`

All 4 of the old, pre rundown 6 melee weapons, are back:  
Maul, Gavel, Mallet and Sledge can be enabled in the mod settings menu and will be added to the melee weapons menu on the loadout screen.

## Local Rundown Progression
This allows you to save your progress (including boosters and vanity items) onto your computers hard drive instead of depending on the developers servers.  
The implementation should be as close to the original game whenever a specific (rundown) version was live, to keep the experience genuine even after servers are down forever.

---

### Loud Volume Override

Lower or mute the game during the intro sequence and while dropping down with the elevator.  
Also allows you to adjust what happens with audio whenever you tab outside of the game:
* Continue playing
* Lower Volume
* Mute

### Player Lobby Management

Open up players Steam profile or, if you're the host, kick them out of your lobby.

![PlayerLobbyManagement](https://user-images.githubusercontent.com/37329066/227807583-03305c00-323c-446c-94df-43f9aa7595cd.png)

### Carry Item Marker

Big pickups (like `CELL`s or `FOG_TURBINE`s) get their own color as well as the item name above it.  
Also shows a marker on whoever is carrying a big pickup.

![CarryMarker](https://user-images.githubusercontent.com/37329066/227804434-22e8de81-6884-4830-9ff6-a5a8c4616cc2.png)

<details>
<summary>📷 All the different color variations: (Big Pickup spoilers!!)</summary>

<p align="center">
  <img src="https://user-images.githubusercontent.com/37329066/227804227-207d47a7-54cb-49f7-936f-76e4d0c5068d.png" alt="CarryItemMarker"/>
</p>
</details>

### Glass Liquid System Override

Change the resolution of the system that renders the blood splatters and other liquids on your visor or disable it entirely.  
Disabling the system entirely prevents/"fixes" the so called "Void Bug" from happening, where sometimes a blob of darkness, the big black blob consumes your entire screen, making you unable to see anything for a few seconds up to minutes at a time.

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

### Show Weapon Stats

Displayes the weapons stats on the weapon select screen.  
Damage, clip size, max ammo and more

### Glowsticks! - `[A1 - RL]`

Change the base color to any of the available ones (Green, Yellow, Orange or Red) which syncs to other players!  
And/Or override the color locally (for yourself only) based on one fixed color or based on who threw the glowstick.

### Loading Indicator

Displays a little indicator that shows if other players have finished loading yet.

### Nickname

Change your name in game, includes a color option.

### Player Color Override

Allows you to change the colors of you and your teammates.  
Additionally allows you to colorize other players based on their nickname color.

### 99% Reload Fix

Fixes the bug that leaves you with one bullet short in the mag even though enough ammo is available.

### L4D Style Resource Packs

Use left and right mouse buttons to apply resource packs instead of `E`  
Left mouse = yourself  
Right mouse = other players (can be held down + hovered over a player to start the interaction)

### Disable Hud Sway

Makes the in-game hud stay in place while running, jumping and looking around.

### Disable UI Mirroring

Removes the mirroring effect on UI elements.

### Disable UI Paralax

Stops the movement of UI elements in menu screens (Loadout, Rundown, ...) whenever you move your cursor around.  
(Some elements might get partially or fully cut off-screen!)

### No Dead Pings

Fixes pings (red triangles, doritos) staying on dead enemies as a result of high ping.

### Remove Story Dialog

Prevents all level-based voice events that have subtitles assigned from playing.  
(Goodbye Schaefer & Co 😥)

### Skip Elevator Animation

Automatically skips the cutscene after initiating a cage drop.  
This leads to faster load times as the game only starts building the level once the cutscene is over.  

### Bot Customization - `[R6 - RL]`

Customize your Bots appearance as host and change their names. (Syncs to other players!)

### Process Priority

Automatically change the games process priority, potentially increasing performance by a tiny bit.

### Other things not mentioned

Yes, there's (probably) more! (I usually forget to update this readme)

---

# Installation

1. Install [MelonLoader](https://github.com/LavaGang/MelonLoader#how-to-use-the-installer) into your game folder.  
   Make sure to install **MelonLoader version `0.5.7`**, not anything older or newer (for now!)
2. **Launch the game once to generate files & folders** and once in the main menu close it again.  
   (This could take a little longer the first time around)
3. Download the latest mod version from [here](https://github.com/AuriRex/GTFO_TheArchive/releases/latest). (it's called `TheArchive.Core.dll`)
4. Put the dll into the `Mods` folder inside of your GTFO directory  
   (In Steam: `[Right Click on GTFO]` > `[Manage >]` > `[Browse local files]`)
5. Launch the game again, you're done!  
   (Check if the mod is installed by navigating to the games `Settings` menu, there should be a button labeled `Mod Settings` in the bottom left.)

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
