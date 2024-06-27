# RPG Difficulty
Monster and Creatures increases their status when you get away from the spawn or when you get deeper in the caves

Features:
- Blacklist
- Whitelist
- Life status increase
- Damage status increase
- Loot drop increase
- Increase by distance
- Increase by height
- [Level UP](https://mods.vintagestory.at/levelup) Mod Compatibility
- [Level UP](https://mods.vintagestory.at/levelup) Mod increase experience kill based on distance

Future features:
- Level UP Mod increase experience kill based on distance

### Observations
RPG Difficulty simple changes the base max health when new entity spawn

For the damage system to work correctly it is necessary to change the native function "ReceiveDamage", so mods that change the damage function will not work with RPG Difficulty

For the harvest system to work correctly it is neccessary to modify the native function "SetHarvested", this just creates a simple prefix to change the player "animalLootDropRate" status, and probably is compatibile with everthing.

Make a backup of the world before adding this mod, any error can drastically increase the life of a creature which can ruin your gameplay.

This mods changes the "animalLootDropRate", this can cause problems if you are removing the mod, if you are using the [Level UP](https://mods.vintagestory.at/levelup) theres a special section for that, if not consider harvesting any entity in the world spawn X:0 Y:110 Z:0 to reset the player status before removing the mod.

### Considerations
This mod changes some native functions and can break easily throught updates.

By default the blacklist with "game:player" is enabled, because of course we dont wanna get the players to receive the buffs from distance when entering in the server, but if you want feel free to remove it.

Adding lower values to the stats increase every... can cause performance problems in high numbers of status on low end cpus, this calculations is made every time a entity spawn in the world, but of course only if entity has a health status, if the entity doesn't have a health status is ignored by the mod, like anything that move and is not alive (arrows,  rocks, buttlerfly (yeah buttlerfly doesn't have health)).

The configuration enableExtended logs can cause performances problems, because a lot of things in the mod is constantly logging, if the mod is very stable in your world/modpack please consider desabling it in configurations

### About RPG Difficulty
RPG Difficulty is open source project and can easily be accessed on the github, all contents from this mod is completly free.

If you want to contribute into the project you can access the project github and make your pull request.

You are free to fork the project and make your own version of RPG Difficulty, as long the name is changed.

Inspirations: 
- Minecraft RpgDifficulty mod

### Building
Learn more about vintage story modding in [Linux](https://github.com/LeandroTheDev/arch_linux/wiki/Games#vintage-story-modding) or [Windows](https://wiki.vintagestory.at/index.php/Modding:Setting_up_your_Development_Environment)

Download the mod template for vintage story with name RPGDifficulty and paste all contents from this project in there

> Linux

Make a symbolic link for fast tests
- ln -s /path/to/project/Releases/rpgdifficulty/* /path/to/game/Mods/RPGDifficulty/

Execute the comamnd ./build.sh, consider having setup everthing from vintage story ide before

> Windows

Just open the visual studio with RPGDifficulty.sln

FTM License
