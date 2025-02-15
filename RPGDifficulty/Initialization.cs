﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Server;
namespace RPGDifficulty;

public class Initialization : ModSystem
{
    readonly Overwrite overwriter = new();
    static internal ICoreServerAPI serverAPI;
    public static EntityPos DefaultSpawnPosition { get; private set; }
    public override void StartServerSide(ICoreServerAPI api)
    {
        base.StartServerSide(api);
        serverAPI = api;
        // Create the timer only with levelup compatibility
        if (overwriter.levelUPCompatibility &&
            Configuration.levelUPExperienceIncreaseEveryDistance != 0.0 &&
            Configuration.levelUPExperienceIncreaseEveryHeight != 0.0)
        {
            Debug.Log("Experience mechanic enabled, initializing LevelUP Compatibility");
            api.Event.Timer(OnTimerElapsed, Configuration.levelUPSecondsPositionUpdate);
            Debug.Log($"Updating player experience multiplier every {Configuration.levelUPSecondsPositionUpdate} second");
            api.Event.PlayerDisconnect += PlayerDisconnectedLevelUP;
        }

        // Timer to get world spawn position
        {
            var timer = new System.Timers.Timer(200)
            {
                AutoReset = true,
                Enabled = true
            };
            timer.Elapsed += (_, _) =>
            {
                try
                {
                    DefaultSpawnPosition = api.World.DefaultSpawnPosition;
                    timer.Stop();
                    timer.Dispose();
                }
                catch (Exception) { }
            };
        }
    }

    private void PlayerDisconnectedLevelUP(IServerPlayer player)
    {
        player.Entity.Attributes.RemoveAttribute("LevelUP_Server_Instance_ExperienceMultiplier_IncreaseExp");
    }

    private void OnTimerElapsed()
    {
        foreach (IPlayer player in serverAPI.World.AllOnlinePlayers)
        {
            int statsIncreaseDistance = 0;
            int statsIncreaseHeight = 0;
            int statsIncreaseAge = 0;
            // Stats increasing
            {
                // Coordinates
                double entityX = player.Entity.Pos.X - serverAPI.World.DefaultSpawnPosition.X;
                double entityZ = player.Entity.Pos.Z - serverAPI.World.DefaultSpawnPosition.Z;
                double entityY = player.Entity.Pos.Y;

                // XZ Coordinates translations
                if (entityX < 0) entityX = Math.Abs(entityX);
                if (entityZ < 0) entityZ = Math.Abs(entityZ);

                // Distance calculation
                if (Configuration.enableStatusIncreaseByDistance)
                {
                    while (true)
                    {
                        if (entityX <= Configuration.increaseStatsEveryDistance && entityZ <= Configuration.increaseStatsEveryDistance) break;

                        // Reduce X
                        if (entityX > Configuration.increaseStatsEveryDistance)
                        {
                            entityX -= Configuration.increaseStatsEveryDistance;
                            statsIncreaseDistance++;
                        }
                        // Reduce Z
                        if (entityZ > Configuration.increaseStatsEveryDistance)
                        {
                            entityZ -= Configuration.increaseStatsEveryDistance;
                            statsIncreaseDistance++;
                        }
                    }
                }

                // Height Calculation
                if (Configuration.enableStatusIncreaseByHeight)
                {
                    while (true)
                    {
                        if (entityY >= Configuration.baseStatusHeight || (entityY + Configuration.increaseStatsEveryDownHeight) >= Configuration.baseStatusHeight) break;

                        entityY += Configuration.increaseStatsEveryDownHeight;
                        statsIncreaseHeight++;
                    }
                }

                // Age Calculation
                if (Configuration.enableStatusIncreaseByAge)
                {
                    statsIncreaseAge = Configuration.GetStatusByWorldAge(serverAPI);
                }
            }
            // Set global experience for LevelUP compatibility layer
            player.Entity.Attributes.SetFloat("LevelUP_Server_Instance_ExperienceMultiplier_IncreaseExp", (float)((Configuration.levelUPExperienceIncreaseEveryDistance * statsIncreaseDistance) + (Configuration.levelUPExperienceIncreaseEveryHeight * statsIncreaseHeight) + (Configuration.levelUPExperienceIncreaseEveryAge * statsIncreaseAge)));
        }
    }

    public override void Start(ICoreAPI api)
    {
        base.Start(api);
        Debug.LoadLogger(api.Logger);
        Debug.Log($"Running on Version: {Mod.Info.Version}");

        // Overwrite native functions
        overwriter.OverwriteNativeFunctions();
    }

    public override void AssetsLoaded(ICoreAPI api)
    {
        Configuration.UpdateBaseConfigurations(api);
        Debug.Log("Configuration set");
    }

    public override double ExecuteOrder()
    {
        return 0.5;
    }


    public override void Dispose()
    {
        base.Dispose();
        overwriter.instance?.UnpatchAll();
    }

    public static void SetEntityStats(Entity entity)
    {
        // Disclcaimer: for some reason the spawn position takes way too long to load, so in first loads we need to ignore it unfurtunally
        // serverAPI.World.DefaultSpawnPosition.X
        if (DefaultSpawnPosition == null) return;

        // Ignore non creature
        if (!entity.IsCreature) return;

        bool increaseByDistance = Configuration.BlackWhiteListCheckForDistance(entity.Code.ToString());
        bool increaseByHeight = Configuration.BlackWhiteListCheckForHeight(entity.Code.ToString());
        bool increaseByAge = Configuration.BlackWhiteListCheckForAge(entity.Code.ToString());

        entity.Attributes.SetBool("RPGDifficultyAlreadySet", true);

        // Function for increasing entity stats
        void increaseStats()
        {
            int statsIncreaseDistance = 0;
            int statsIncreaseHeight = 0;
            int statsIncreaseAge = Configuration.GetStatusByWorldAge(serverAPI);
            // Stats increasing
            {
                // Coordinates
                double entityX = entity.Pos.X - DefaultSpawnPosition.X;
                double entityZ = entity.Pos.Z - DefaultSpawnPosition.Z;
                double entityY = entity.Pos.Y;

                // XZ Coordinates translations
                if (entityX < 0) entityX = Math.Abs(entityX);
                if (entityZ < 0) entityZ = Math.Abs(entityZ);

                entity.Attributes.SetDouble("RPGDifficultyEntitySpawnDistance", entityX + entityZ);
                entity.Attributes.SetDouble("RPGDifficultyEntitySpawnHeight", entityY);
                entity.Attributes.SetDouble("RPGDifficultyEntitySpawnAge", (int)serverAPI.World.Calendar.ElapsedDays);

                // Distance calculation
                if (Configuration.enableStatusIncreaseByDistance)
                {
                    while (true)
                    {
                        if (entityX <= Configuration.increaseStatsEveryDistance && entityZ <= Configuration.increaseStatsEveryDistance) break;

                        // Reduce X
                        if (entityX > Configuration.increaseStatsEveryDistance)
                        {
                            entityX -= Configuration.increaseStatsEveryDistance;
                            statsIncreaseDistance++;
                        }
                        // Reduce Z
                        if (entityZ > Configuration.increaseStatsEveryDistance)
                        {
                            entityZ -= Configuration.increaseStatsEveryDistance;
                            statsIncreaseDistance++;
                        }
                    }
                }

                // Height Calculation
                if (Configuration.enableStatusIncreaseByHeight)
                {
                    while (true)
                    {
                        if (entityY >= Configuration.baseStatusHeight || (entityY + Configuration.increaseStatsEveryDownHeight) >= Configuration.baseStatusHeight) break;

                        entityY += Configuration.increaseStatsEveryDownHeight;
                        statsIncreaseHeight++;
                    }
                }
            }

            // Verification if is a creature and alive
            if (entity.IsCreature && entity.Alive)
            {
                // Getting variation
                double variation = 0;
                if (Configuration.enableStatusVariation)
                {
                    Random random = new();
                    variation = Configuration.minimumVariableStatusAverage + (Configuration.maxVariableStatusAverage - Configuration.minimumVariableStatusAverage) * random.NextDouble();
                    variation = Math.Round(variation, 2);
                    entity.Attributes.SetDouble("RPGDifficultyStatusVariation", variation);
                }

                double healthDistance = Configuration.lifeStatsIncreaseEveryDistance * statsIncreaseDistance;
                if (healthDistance > Configuration.maximumLifeStatusIncreasedByDistance)
                    healthDistance = Configuration.maximumLifeStatusIncreasedByDistance;

                double healthHeight = Configuration.lifeStatsIncreaseEveryHeight * statsIncreaseHeight;
                if (healthHeight > Configuration.maximumLifeStatusIncreasedByHeight)
                    healthHeight = Configuration.maximumLifeStatusIncreasedByHeight;

                double healthAge = Configuration.lifeStatsIncreaseEveryAge * statsIncreaseAge;
                if (healthAge > Configuration.maximumLifeStatusIncreasedByAge)
                    healthAge = Configuration.maximumLifeStatusIncreasedByAge;

                // Setting health variables
                if (increaseByDistance)
                    entity.Attributes.SetDouble("RPGDifficultyHealthStatsIncreaseDistance", healthDistance);
                if (increaseByHeight)
                    entity.Attributes.SetDouble("RPGDifficultyHealthStatsIncreaseHeight", healthHeight);
                if (increaseByAge)
                    entity.Attributes.SetDouble("RPGDifficultyHealthStatsIncreaseAge", healthAge);

                double damageDistance = Configuration.damageStatsIncreaseEveryDistance * statsIncreaseDistance;
                if (damageDistance > Configuration.maximumDamageStatusIncreasedByDistance)
                    damageDistance = Configuration.maximumDamageStatusIncreasedByDistance;

                double damageHeight = Configuration.damageStatsIncreaseEveryHeight * statsIncreaseHeight;
                if (damageHeight > Configuration.maximumDamageStatusIncreasedByHeight)
                    damageHeight = Configuration.maximumDamageStatusIncreasedByHeight;

                double damageAge = Configuration.damageStatsIncreaseEveryAge * statsIncreaseAge;
                if (damageAge > Configuration.maximumDamageStatusIncreasedByAge)
                    damageAge = Configuration.maximumDamageStatusIncreasedByAge;

                // Setting damage variables
                if (increaseByDistance)
                    entity.Attributes.SetDouble("RPGDifficultyDamageStatsIncreaseDistance", damageDistance);
                if (increaseByHeight)
                    entity.Attributes.SetDouble("RPGDifficultyDamageStatsIncreaseHeight", damageHeight);
                if (increaseByAge)
                    entity.Attributes.SetDouble("RPGDifficultyDamageStatsIncreaseAge", damageAge);

                double lootDistance = Configuration.lootStatsIncreaseEveryDistance * statsIncreaseDistance;
                if (lootDistance > Configuration.maximumLootStatusIncreasedByDistance)
                    lootDistance = Configuration.maximumLootStatusIncreasedByDistance;

                double lootHeight = Configuration.lootStatsIncreaseEveryHeight * statsIncreaseHeight;
                if (lootHeight > Configuration.maximumLootStatusIncreasedByHeight)
                    lootHeight = Configuration.maximumLootStatusIncreasedByHeight;

                double lootAge = Configuration.lootStatsIncreaseEveryAge * statsIncreaseAge;
                if (lootAge > Configuration.maximumLootStatusIncreasedByAge)
                    lootAge = Configuration.maximumLootStatusIncreasedByAge;

                // Setting damage variables
                if (increaseByDistance)
                    entity.Attributes.SetDouble("RPGDifficultyLootStatsIncreaseDistance", lootDistance);
                if (increaseByHeight)
                    entity.Attributes.SetDouble("RPGDifficultyLootStatsIncreaseHeight", lootHeight);
                if (increaseByAge)
                    entity.Attributes.SetDouble("RPGDifficultyLootStatsIncreaseAge", lootAge);

                if (Configuration.enableExtendedLog)
                    Debug.Log($"{entity.Code} health percentage: {healthDistance + healthHeight + healthAge} damage percentage: {damageDistance + damageHeight + damageAge} loot percentage: {lootDistance + lootHeight + lootAge}, variation: {variation}");
            }
        }

        // List Check
        if (increaseByDistance || increaseByHeight || increaseByAge)
            increaseStats();
    }

    public static bool ShouldEntitySpawn(Entity entity)
    {
        // Swiping every condition
        foreach (object conditionsObject in Configuration.entitySpawnConditions)
        {
            // Check if the condition is a valid object
            if (conditionsObject is JObject conditions)
            {
                try
                {
                    Dictionary<string, object> conditionsDict = conditions.ToObject<Dictionary<string, object>>();

                    // Check if the spawn condition is for this entity
                    if (conditionsDict.TryGetValue("code", out object code))
                        if (code.ToString() != entity.Code.ToString())
                            continue;

                    // Check SpawnersApi condition
                    if (conditionsDict.TryGetValue("ignoreConditionsForSpawnersAPI", out object ignoreConditionsForSpawnersAPI))
                        if (bool.Parse(ignoreConditionsForSpawnersAPI.ToString()) && entity.Attributes.GetBool("SpawnersAPI_Is_From_Spawner"))
                            continue;

                    // Getting the entity condition values
                    double distance = entity.Attributes.GetDouble("RPGDifficultyEntitySpawnDistance", -1);
                    double height = entity.Attributes.GetDouble("RPGDifficultyEntitySpawnHeight", -1);
                    double age = entity.Attributes.GetDouble("RPGDifficultyEntitySpawnAge", -1);

                    // Distance check
                    if (conditionsDict.TryGetValue("minimumDistanceToSpawn", out object minimumDistanceToSpawn) &&
                        conditionsDict.TryGetValue("maximumDistanceToSpawn", out object maximumDistanceToSpawn))
                    {
                        if ((long)minimumDistanceToSpawn != -1)
                            if (distance < (long)minimumDistanceToSpawn)
                            {
                                if (Configuration.enableExtendedLog)
                                    Debug.Log($"not in minimum distance: {minimumDistanceToSpawn}, actual: {distance}");
                                return false;
                            }
                        if ((long)maximumDistanceToSpawn != -1)
                            if (distance > (long)maximumDistanceToSpawn)
                            {
                                if (Configuration.enableExtendedLog)
                                    Debug.Log($"not in maximum distance: {maximumDistanceToSpawn}, actual: {distance}");
                                return false;
                            }
                    }

                    // Height check
                    if (conditionsDict.TryGetValue("minimumHeightToSpawn", out object minimumHeightToSpawn) &&
                        conditionsDict.TryGetValue("maximumHeightToSpawn", out object maximumHeightToSpawn))
                    {
                        if ((long)minimumHeightToSpawn != -1)
                            if (height < (long)minimumHeightToSpawn)
                            {
                                if (Configuration.enableExtendedLog)
                                    Debug.Log($"not in minimum height: {minimumHeightToSpawn}, actual: {height}");
                                return false;
                            }
                        if ((long)maximumHeightToSpawn != -1)
                            if (height > (long)maximumHeightToSpawn)
                            {
                                if (Configuration.enableExtendedLog)
                                    Debug.Log($"not in maximum height: {maximumHeightToSpawn}, actual: {height}");
                                return false;
                            }
                    }

                    // Age check
                    if (conditionsDict.TryGetValue("minimumAgeToSpawn", out object minimumAgeToSpawn) &&
                        conditionsDict.TryGetValue("maximumAgeToSpawn", out object maximumAgeToSpawn))
                    {
                        if ((long)minimumAgeToSpawn != -1)
                            if (age < (long)minimumAgeToSpawn)
                            {
                                if (Configuration.enableExtendedLog)
                                    Debug.Log($"not in minimum age: {minimumAgeToSpawn}, actual: {age}");
                                return false;
                            }
                        if ((long)maximumAgeToSpawn != -1)
                            if (age > (long)maximumAgeToSpawn)
                            {
                                if (Configuration.enableExtendedLog)
                                    Debug.Log($"not in maximum age: {maximumAgeToSpawn}, actual: {age}");
                                return false;
                            }
                    }
                }
                catch (Exception ex)
                {
                    Debug.Log($"ERROR: Something crashed the spawn condition, probably some mistake in base.json \"entitySpawnConditions\", exception: {ex.Message}");
                }
            }
            else Debug.Log($"ERROR: Spawn condition is not a JObject, it is {conditionsObject.GetType()}");
        }
        return true;
    }
}

public class Debug
{
    private static readonly OperatingSystem system = Environment.OSVersion;
    static private ILogger loggerForNonTerminalUsers;

    static public void LoadLogger(ILogger logger) => loggerForNonTerminalUsers = logger;
    static public void Log(string message)
    {
        // Check if is linux or other based system and if the terminal is active for the logs to be show
        if (system.Platform == PlatformID.Unix || system.Platform == PlatformID.Other || Environment.UserInteractive)
            // Based terminal users
            Console.WriteLine($"{DateTime.Now:d.M.yyyy HH:mm:ss} [RPGDifficulty] {message}");
        else
            // Unbased non terminal users
            loggerForNonTerminalUsers?.Log(EnumLogType.Notification, $"[RPGDifficulty] {message}");
    }
}