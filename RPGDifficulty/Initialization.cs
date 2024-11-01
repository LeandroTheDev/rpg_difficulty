﻿using System;
using LevelUP;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Server;
using Vintagestory.GameContent;
namespace RPGDifficulty;

public class Initialization : ModSystem
{
    readonly Overwrite overwriter = new();
    ICoreServerAPI serverAPI;
    public static EntityPos DefaultSpawnPosition { get; private set; }
    public override void StartServerSide(ICoreServerAPI api)
    {
        base.StartServerSide(api);
        serverAPI = api;
        // Create the timer only with levelup compatibility
        if (overwriter.levelUPCompatibility && Configuration.levelUPExperienceIncreaseEveryDistance != 0.0 && Configuration.levelUPExperienceIncreaseEveryHeight != 0.0)
        {
            Debug.Log("Experience mechanic enabled, initializing LevelUP Compatibility");
            api.Event.Timer(OnTimerElapsed, Configuration.levelUPSecondsPositionUpdate);
            Debug.Log($"Updating player experience multiplier every {Configuration.levelUPSecondsPositionUpdate} second");
            api.Event.PlayerDisconnect += PlayerDisconnected;
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

    private void PlayerDisconnected(IServerPlayer player)
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
        overwriter.OverwriteNativeFunctions(api);
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
        overwriter.overwriter?.UnpatchAll();
    }

    public static void IncreaseEntityStats(Entity entity)
    {
        // Disclcaimer: for some reason the spawn position takes way too long to load, so in first loads we need to ignore it unfurtunally
        // entity.Api.World.DefaultSpawnPosition.X
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
            int statsIncreaseAge = Configuration.GetStatusByWorldAge(entity.Api);
            // Stats increasing
            {
                // Coordinates
                double entityX = entity.Pos.X - DefaultSpawnPosition.X;
                double entityZ = entity.Pos.Z - DefaultSpawnPosition.Z;
                double entityY = entity.Pos.Y;

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
            }

            // Single player / Lan treatment
            if (entity.SidedProperties == null) return;

            // Changing Health Stats
            EntityBehaviorHealth entityLifeStats = entity.GetBehavior<EntityBehaviorHealth>();
            // Check existance, for example buttlerfly doesn't have a life status
            if (entityLifeStats != null)
            {
                // Increase entity max health
                float oldBaseMaxHealth = entityLifeStats.BaseMaxHealth;

                // Increasing entity max values
                if (increaseByDistance)
                    entityLifeStats.BaseMaxHealth += (int)Math.Round(entityLifeStats.BaseMaxHealth * (Configuration.lifeStatsIncreaseEveryDistance * statsIncreaseDistance));
                if (increaseByHeight)
                    entityLifeStats.BaseMaxHealth += (int)Math.Round(entityLifeStats.BaseMaxHealth * (Configuration.lifeStatsIncreaseEveryHeight * statsIncreaseHeight));
                if (increaseByAge)
                    entityLifeStats.BaseMaxHealth += (int)Math.Round(entityLifeStats.BaseMaxHealth * (Configuration.lifeStatsIncreaseEveryAge * statsIncreaseAge));
                if (increaseByDistance)
                    entityLifeStats.MaxHealth += (int)Math.Round(entityLifeStats.MaxHealth * (Configuration.lifeStatsIncreaseEveryDistance * statsIncreaseDistance));
                if (increaseByHeight)
                    entityLifeStats.MaxHealth += (int)Math.Round(entityLifeStats.MaxHealth * (Configuration.lifeStatsIncreaseEveryHeight * statsIncreaseHeight));
                if (increaseByAge)
                    entityLifeStats.MaxHealth += (int)Math.Round(entityLifeStats.MaxHealth * (Configuration.lifeStatsIncreaseEveryAge * statsIncreaseAge));

                // Increasing actual entities health
                if (increaseByDistance)
                    entityLifeStats.Health += (int)Math.Round(entityLifeStats.Health * (Configuration.lifeStatsIncreaseEveryDistance * statsIncreaseDistance));
                if (increaseByHeight)
                    entityLifeStats.Health += (int)Math.Round(entityLifeStats.Health * (Configuration.lifeStatsIncreaseEveryHeight * statsIncreaseHeight));
                if (increaseByAge)
                    entityLifeStats.Health += (int)Math.Round(entityLifeStats.Health * (Configuration.lifeStatsIncreaseEveryAge * statsIncreaseAge));

                // Getting variation
                double variation = 0;
                if (Configuration.enableStatusVariation)
                {
                    Random random = new();
                    variation = Configuration.minimumVariableStatusAverage + (Configuration.maxVariableStatusAverage - Configuration.minimumVariableStatusAverage) * random.NextDouble();
                    variation = Math.Round(variation, 2);
                    entity.Attributes.SetDouble("RPGDifficultyStatusVariation", variation);

                    // Changing health after variation calculation
                    entityLifeStats.BaseMaxHealth *= (float)variation;
                    entityLifeStats.MaxHealth *= (float)variation;
                    entityLifeStats.Health *= (float)variation;
                }

                // Setting compatibility variables
                if (increaseByDistance)
                    entity.Attributes.SetDouble("RPGDifficultyDamageStatsIncreaseDistance", Configuration.damageStatsIncreaseEveryDistance * statsIncreaseDistance);
                if (increaseByHeight)
                    entity.Attributes.SetDouble("RPGDifficultyDamageStatsIncreaseHeight", Configuration.damageStatsIncreaseEveryHeight * statsIncreaseHeight);
                if (increaseByAge)
                    entity.Attributes.SetDouble("RPGDifficultyDamageStatsIncreaseAge", Configuration.damageStatsIncreaseEveryAge * statsIncreaseAge);
                if (increaseByDistance)
                    entity.Attributes.SetDouble("RPGDifficultyLootStatsIncreaseDistance", Configuration.lootStatsIncreaseEveryDistance * statsIncreaseDistance);
                if (increaseByHeight)
                    entity.Attributes.SetDouble("RPGDifficultyLootStatsIncreaseHeight", Configuration.lootStatsIncreaseEveryHeight * statsIncreaseHeight);
                if (increaseByAge)
                    entity.Attributes.SetDouble("RPGDifficultyLootStatsIncreaseAge", Configuration.lootStatsIncreaseEveryAge * statsIncreaseAge);

                // RPGOverlay compatibility
                if (Configuration.rpgOverlayOverwriteLevel)
                {
                    int additionalLevels = 0;
                    bool reducedAdditionalLevels = false;
                    double healthDifference = entityLifeStats.BaseMaxHealth - oldBaseMaxHealth;
                    // Check if the health is less
                    if (healthDifference < 0)
                    {
                        healthDifference = Math.Abs(healthDifference);
                        reducedAdditionalLevels = true;
                    }

                    for (int i = 0; i < (int)healthDifference; i++)
                    {
                        // If i is multiply of rpgOverlayIncreaseLevelEveryAdditionalHP
                        if (i % Configuration.rpgOverlayIncreaseLevelEveryAdditionalHP == 0)
                        {
                            // increasing the level
                            additionalLevels++;
                        }
                    }

                    if (additionalLevels != 0)
                    {
                        // Transforms the additionaLevels in negative if is to be reduced
                        if (reducedAdditionalLevels)
                        {
                            additionalLevels = -additionalLevels;
                        }

                        // Checking if not exist any compatibility yet
                        if (entity.WatchedAttributes.GetInt("RPGOverlayAddOrReduceLevels") == 0)
                        {
                            // Simple create new level if not exist
                            entity.WatchedAttributes.SetInt("RPGOverlayAddOrReduceLevels", additionalLevels);
                        }
                        else
                        {
                            // Some other mod has already created the compatibility, lets get the value
                            int previousAdditionalLevels = entity.WatchedAttributes.GetInt("RPGOverlayAddOrReduceLevels");
                            // We set now the variable as the: previous level from other mod plus ours new level
                            entity.WatchedAttributes.SetInt("RPGOverlayAddOrReduceLevels", additionalLevels + previousAdditionalLevels);
                        }
                    }
                }

                if (Configuration.enableExtendedLog)
                    Debug.Log($"{entity.Code} changing max health in: {entityLifeStats.BaseMaxHealth - oldBaseMaxHealth} damage percentage: {(Configuration.damageStatsIncreaseEveryDistance * statsIncreaseDistance) + (Configuration.damageStatsIncreaseEveryHeight * statsIncreaseHeight)} loot percentage: {(Configuration.lootStatsIncreaseEveryDistance * statsIncreaseDistance) + (Configuration.lootStatsIncreaseEveryHeight * statsIncreaseHeight)}, variation: {variation}");
            }
        }

        // List Check
        if (increaseByDistance || increaseByHeight || increaseByAge)
            increaseStats();
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