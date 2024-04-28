using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using HarmonyLib;
using LevelUP;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
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
    }

    public override void Start(ICoreAPI api)
    {
        base.Start(api);
        Debug.LoadLogger(api.Logger);
        Debug.Log("Running on Version: 1.0.0");

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

    private void IncreaseEntityStats(Entity entity)
    {
        // Ignore non creature
        if (!entity.IsCreature) return;

        // Function for increasing entity stats
        void increaseStats()
        {
            int statsIncrease = 0;
            // Stats increasing
            {
                double entityX = entity.Pos.X - serverAPI.World.DefaultSpawnPosition.X;
                double entityZ = entity.Pos.Z - serverAPI.World.DefaultSpawnPosition.Z;
                if (entityX < 0) entityX = Math.Abs(entityX);
                if (entityZ < 0) entityZ = Math.Abs(entityZ);
                while (true)
                {
                    if (entityX <= Configuration.increaseStatsEveryDistance && entityZ <= Configuration.increaseStatsEveryDistance) break;

                    // Reduce X
                    if (entityX > Configuration.increaseStatsEveryDistance)
                    {
                        entityX -= Configuration.increaseStatsEveryDistance;
                        statsIncrease++;
                    }
                    // Reduce Z
                    if (entityZ > Configuration.increaseStatsEveryDistance)
                    {
                        entityZ -= Configuration.increaseStatsEveryDistance;
                        statsIncrease++;
                    }
                }
            }
            // Changing Health Stats
            EntityBehaviorHealth entityLifeStats = entity.GetBehavior<EntityBehaviorHealth>();
            if (entityLifeStats != null)
            {
                if (Configuration.enableExtendedLog)
                    Debug.Log($"{entity.Code} increasing max health in: {(int)Math.Round(entityLifeStats.BaseMaxHealth * (Configuration.lifeStatsIncreaseEveryDistance * statsIncrease))}, damage: {Configuration.damageStatsIncreaseEveryDistance * statsIncrease}");

                // Increase entity max health
                entityLifeStats.BaseMaxHealth += (int)Math.Round(entityLifeStats.BaseMaxHealth * (Configuration.lifeStatsIncreaseEveryDistance * statsIncrease));
                entityLifeStats.MaxHealth += (int)Math.Round(entityLifeStats.MaxHealth * (Configuration.lifeStatsIncreaseEveryDistance * statsIncrease));
                // Increase tntiy actual health
                entityLifeStats.Health += (int)Math.Round(entityLifeStats.Health * (Configuration.lifeStatsIncreaseEveryDistance * statsIncrease));
            }
            entity.Attributes.SetDouble("RPGDifficultyDamageStatsIncrease", Configuration.damageStatsIncreaseEveryDistance * statsIncrease);
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