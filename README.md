# TheArchive

A collection of mods for the game [GTFO](https://gtfothegame.com/), that add a ton of Quality of Life and cosmetic features to the game without overstepping the games original design.

~~Not only compatible with the **latest release** on steam but also with many of the **older GTFO builds**, tested on all the latest patches for each (old) rundown.~~  

> [!CAUTION]  
> Legacy game builds are NOT supported on the latest releases anymore.  
> There are legacy builds of the mod still available for use with `MelonLoader 0.5.7` [from here](https://github.com/AuriRex/GTFO_TheArchive/releases/tag/v0.7.1-beta).  
> [Legacy Source Code Branch](https://github.com/AuriRex/GTFO_TheArchive/tree/legacy)

## README Links

> [!IMPORTANT]  
> The project has been split up!  
> Check the respective folders in this repo for more in-depth info about each project.

* [TheArchive.Core](TheArchive.Core/README.md)
* [TheArchive.Essentials](TheArchive.Essentials/README.md)
* [TheArchive.RichPresence](TheArchive.RichPresence/README.md)

## What this does
Improve the game via Quality of Life and cosmetic additions without disrupting the experience of other players.  
All mechanics are still kept vanilla and in spirit of the games original design.

## What this does NOT do
This ***does not*** give you access to the game or any of the old versions, you have to have bought the game on steam!  
This ***does not*** give any player an unfair advantage or trivialize the game, you have to bring your own skill.

---

## Custom file save location?

By default, all mod files get saved to `%appdata%/../LocalLow/GTFO_TheArchive/`.  
You can customize the save folder by editing `TheArchive_Settings.json`'s `"CustomFileSaveLocation"` property to point to any location of your choosing.

> [!TIP]  
> *Make sure to escape backslashes (`\`) in your path by doubling them (like this: `\\`), else it won't work!*

---

# Technical stuffs

## Building the project (On Windows)

### Getting references set up
* Create a profile in r2modman / Gale *(or use an already existing one)*
* Make sure [Clonesoft Json](https://thunderstore.io/c/gtfo/p/AuriRex/Clonesoft_Json/) is installed in this profile.
* Run the game at least once from this profile to generate the interop assemblies.
* Create a file named `GameFolder.props` in the project root folder, next to `TheArchive.sln`, with the path to your profile folder:
```xml
<Project>
    <PropertyGroup>
        <GameFolder>C:\Users\username\AppData\Roaming\r2modmanPlus-local\GTFO\profiles\MyCoolProfile3</GameFolder>
    </PropertyGroup>
</Project>
```

### Building
1. Open the solution `TheArchive.sln` in Visual Studio or JetBrains Rider
2. Build the solution
3. Profit

## Building the project (On Linux)

Should be the same as on Windows (using JetBrains Rider)

## Custom MSBuild Tasks

This project uses a custom MSBuild task, located in the separate [BuildTasks](BuildTasks/BuildTasks.sln) solution, to generate the `manifest.json` for Thunderstore based on information inside each projects' respective `.csproj` file.

# License

Everything in [this repository](https://github.com/AuriRex/GTFO_TheArchive) is licensed under the MIT License ***(unless stated otherwise inside a given source file)***,  
**excluding** `TheArchive.RichPresence/DiscordGameSDK/discord_game_sdk.dll` and all the files inside of `TheArchive.RichPresence/Core/DiscordApi/*`, which are copyright [Discord](https://discord.com/developers/docs/legal) and only included for convenience.
