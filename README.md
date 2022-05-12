# TheArchive

A [MelonLoader](https://github.com/LavaGang/MelonLoader) based [GTFO](https://gtfothegame.com/) mod that's aiming to preserve older versions by keeping them playable even once the servers are gone forever **by handling all playfab requests locally** and saving things like **progression/level completions**, **boosters** and **vanity items** onto your storage device of choice.

(Also includes a few Quality of life tweaks, which are toggleable)

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