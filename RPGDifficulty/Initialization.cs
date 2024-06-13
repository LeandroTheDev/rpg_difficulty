using System;
using LevelUP;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace RPGDifficulty;

public class RPGDifficultyModSystem : ModSystem
{
    readonly Overwrite overwriter = new();
    ICoreServerAPI serverAPI;
    public override void StartServerSide(ICoreServerAPI api)
    {
        base.StartServerSide(api);
        serverAPI = api;
        // Event instaciation
        api.Event.OnEntitySpawn += IncreaseEntityStats;
        // Create the timer only with levelup compatibility
        if (overwriter.levelUPCompatibility && Configuration.levelUPExperienceIncreaseEveryDistance != 0.0 && Configuration.levelUPExperienceIncreaseEveryHeight != 0.0)
        {
            Debug.Log("Experience mechanic enabled, initializing LevelUP Compatibility");
            api.Event.Timer(OnTimerElapsed, 1000);
            api.Event.PlayerDisconnect += PlayerDisconnected;
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
            }
            // Set global experience for LevelUP compatibility layer
            player.Entity.Attributes.SetFloat("LevelUP_Server_Instance_ExperienceMultiplier_IncreaseExp", (float)((Configuration.levelUPExperienceIncreaseEveryDistance * statsIncreaseDistance) + (Configuration.levelUPExperienceIncreaseEveryHeight * statsIncreaseHeight)));
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
        if (Configuration.enableExtendedLog) Configuration.LogConfigurations();
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

    private void IncreaseEntityStats(Entity entity)
    {
        // Ignore non creature
        if (!entity.IsCreature) return;

        // Function for increasing entity stats
        void increaseStats()
        {
            int statsIncreaseDistance = 0;
            int statsIncreaseHeight = 0;
            // Stats increasing
            {
                // Coordinates
                double entityX = entity.Pos.X - serverAPI.World.DefaultSpawnPosition.X;
                double entityZ = entity.Pos.Z - serverAPI.World.DefaultSpawnPosition.Z;
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
            // Changing Health Stats
            EntityBehaviorHealth entityLifeStats = entity.GetBehavior<EntityBehaviorHealth>();
            // Check existance, for example buttlerfly doesn't have a life status
            if (entityLifeStats != null)
            {
                // Increase entity max health
                entityLifeStats.BaseMaxHealth += (int)Math.Round(entityLifeStats.BaseMaxHealth * (Configuration.lifeStatsIncreaseEveryDistance * statsIncreaseDistance));
                entityLifeStats.BaseMaxHealth += (int)Math.Round(entityLifeStats.BaseMaxHealth * (Configuration.lifeStatsIncreaseEveryHeight * statsIncreaseHeight));
                entityLifeStats.MaxHealth += (int)Math.Round(entityLifeStats.MaxHealth * (Configuration.lifeStatsIncreaseEveryDistance * statsIncreaseDistance));
                entityLifeStats.MaxHealth += (int)Math.Round(entityLifeStats.MaxHealth * (Configuration.lifeStatsIncreaseEveryHeight * statsIncreaseHeight));
                // Increase entity actual health
                entityLifeStats.Health += (int)Math.Round(entityLifeStats.Health * (Configuration.lifeStatsIncreaseEveryDistance * statsIncreaseDistance));
                entityLifeStats.Health += (int)Math.Round(entityLifeStats.Health * (Configuration.lifeStatsIncreaseEveryHeight * statsIncreaseHeight));
                entity.Attributes.SetDouble("RPGDifficultyDamageStatsIncreaseDistance", Configuration.damageStatsIncreaseEveryDistance * statsIncreaseDistance);
                entity.Attributes.SetDouble("RPGDifficultyDamageStatsIncreaseHeight", Configuration.damageStatsIncreaseEveryHeight * statsIncreaseHeight);
                entity.Attributes.SetDouble("RPGDifficultyLootStatsIncreaseDistance", Configuration.lootStatsIncreaseEveryDistance * statsIncreaseDistance);
                entity.Attributes.SetDouble("RPGDifficultyLootStatsIncreaseHeight", Configuration.lootStatsIncreaseEveryHeight * statsIncreaseHeight);


                if (Configuration.enableExtendedLog)
                {
                    Debug.Log($"{entity.EntityId}");
                    Debug.Log($"{entity.Code} increasing max health in: {entityLifeStats.BaseMaxHealth} damage percentage: {(Configuration.damageStatsIncreaseEveryDistance * statsIncreaseDistance) + (Configuration.damageStatsIncreaseEveryHeight * statsIncreaseHeight)} loot percentage: {(Configuration.lootStatsIncreaseEveryDistance * statsIncreaseDistance) + (Configuration.lootStatsIncreaseEveryHeight * statsIncreaseHeight)}");
                }
            }
        }

        // Blacklist Check
        if (Configuration.enableBlacklist)
            if (Configuration.blacklist.TryGetValue(entity.Code.ToString(), out double _))
            { if (Configuration.enableExtendedLog) Debug.Log($"{entity.Code} is on blacklist, ignoring stats"); return; }
        // Whitelist Check
        if (Configuration.enableWhitelist)
            // In whitelist
            if (Configuration.whitelist.TryGetValue(entity.Code.ToString(), out double _))
            { if (Configuration.enableExtendedLog) Debug.Log($"{entity.Code} is on whitelist, increasing stats"); increaseStats(); }
            // Not in whitelist
            else { if (Configuration.enableExtendedLog) Debug.Log($"{entity.Code} is not on whitelist, ignoring stats"); return; }
        // No whitelist check and entity not blacklisted
        else increaseStats();
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
        if ((system.Platform == PlatformID.Unix || system.Platform == PlatformID.Other) && Environment.UserInteractive)
            // Based terminal users
            Console.WriteLine($"{DateTime.Now:d.M.yyyy HH:mm:ss} [RPGDifficulty] {message}");
        else
            // Unbased non terminal users
            loggerForNonTerminalUsers?.Log(EnumLogType.Notification, $"[RPGDifficulty] {message}");
    }
}