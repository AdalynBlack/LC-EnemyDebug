# !!Deprecation Notice!!
I've been working with [Giosuel](https://github.com/giosuel) for a while now to port all the features of this mod over to [Imperium](https://github.com/giosuel/imperium). For now, this mod is in a sort of "limbo" state, as some features have yet to be ported, while others have, but this mod will slowly stop receiving support, and will be deprecated as soon as Imperium reaches feature parity

Currently ported features:
- Line of Sight Visualization

Unported Features:
- Generic Debug Visualizers
- Generic Gizmo Visualizers
- Enemy-specific visualizers
  - Brackens
  - Hoarding Bugs
  - Eyeless Dogs
  - Baboon Hawks
  - Butlers
  - Coilheads
- Debug Variable Watchers

# Enemy Debug
Allows you to debug enemies in-game through the use of a custom gizmo system
Some gizmos shown are the result of existing gizmo draw calls that the game uses, while others are custom to help with understanding the underlying mechanics of enemies. Custom gizmos are uniquely colored to help make the system easier to understand

## Compatibility
This mod should be compatible with most mods, as it modifies very little of the game's code, but breakages may occur. Any issues that occur may be fixed with time, but as the main focus of this project is debugging, compatibility issues may take a while to be fixed

## Building

### Variables:
This repo expects two different environment variables to be set as follows:
LC\_PATH: This should point to your Lethal Company installation. For example, "C:\Program Files (x86)\Steam\steamapps\common\Lethal Company" would be the value of this for the default steam installation
PROFILE\_PATH: This variable should point to the plugins folder for the profile you are using for testing. Building the repo using any configuration other than the Release config will automatically put the compiled DLL into your plugins folder


### Configurations:
There are three configurations for this project, those being Debug, Release, and Launch. The Debug configuration will create the standard debug files, and will also copy the DLL into your chosen plugin folder. The Release configuration will automatically locate all necessary files for a Thunderstore zip, and create the corresponding zip file. This can make it easier for other to install the compiled mod, and also can be directly uploaded to Thunderstore. Lastly, the Launch configuration. It acts just like the Debug configuration, but also launches the game automatically right after copying the mod into your profile
