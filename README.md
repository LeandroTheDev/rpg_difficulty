# RPG Difficulty
Monster and Creatures increases their status getting away from the spawn, getting deeper in the caves and by the world aging, highly configurable

Features:
- Blacklist
- Whitelist
- Life status increase
- Damage status increase
- Loot drop status increase
- Increase by distance
- Increase by height
- Increase by world age
- Status variations
- Condition to spawn based on distance/height/age
- [Level UP](https://mods.vintagestory.at/levelup) Mod increase experience based on distance/height/age
- [SpawnersAPI](https://mods.vintagestory.at/spawnersapi) Mod compatibility to ignore spawn conditions

Everything is configurable, check the [wiki](https://github.com/LeandroTheDev/rpg_difficulty/wiki) for more informations about configurations

### Observations
For the health system to work, we change the base max health when new entity spawn, ``for ever``, the  entity will always have that health on the world even if the mod is removed

For the damage system to work correctly the mod ``overwrites`` the damage configuration from assets and increase it, but if you remove the mod this ``overwrite`` will no longer exist, and the entity will have the previously damage from configuration.

For the harvest system to work correctly the mod ``overwrites`` the harvest system and increase the multiply result, if the mod is removed the additional multiply will no longer exist.

The mod is very compatible with other mods, any incompatibility you can contact me.

Make a backup of the world before adding this mod, any error can drastically increase the life of a creature which can ruin your gameplay.

### Considerations
By default the blacklist with "game:player" is enabled, because of course we dont wanna get the players to receive the buffs from distance when entering in the server, but if you want feel free to remove it.

Adding lower values to the stats increase every... can cause performance problems in high numbers of status on low end cpus, this calculations is made every time a entity creature spawn in the world.

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
