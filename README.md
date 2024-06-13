# RPG Difficulty
Monster and Creatures increases their status when you get away from the spawn

What happens if increase by distance and height is enabled?

The first status to be changed is the entity health by the distance, after that the height status health will be increased, for example a entity have 10 HP by default -> 100% increase by distance -> 20 HP -> 100% increase by height -> 40 HP, the same works for damage.

Because the loot is already a percentage, the difference in height and distance will just be a sum between this two, for example a default loot drop have 0.5 -> 0.5 increased by distanced -> 1.0 -> 0.5 increased by height -> 1.5.

Features:
- Blacklist
- Whitelist
- Life status increase
- Damage status increase
- Loot drop increase
- Increase by distance
- Increase by height
- [Level UP](https://mods.vintagestory.at/levelup) Mod Compatibility

Future features:
- Level UP Mod increase experience kill based on distance

### Observations
RPG Difficulty simple changes the base max health when new entity spawn

For the damage system to work correctly it is necessary to change the native function "ReceiveDamage", so mods that change the damage function will not work with RPG Difficulty

Make a backup of the world before adding this mod, any error can drastically increase the life of a creature which can ruin your gameplay

### Considerations
This mod changes some native functions and can break easily throught updates.

By default the blacklist with "game:player" is enabled, because of course we dont wanna get the players to receive the buffs from distance when entering in the server, but if you want feel free to remove it.

Adding lower values to the stats increase every... can cause performance problems in high numbers of status on low end cpus, this calculations is made every time a entity spawn in the world, but of course only if entity has a health status, if the entity doesn't have a health status is ignored by the mod, like anything that move and is not alive.

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
