using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        if (api.ModLoader.IsModEnabled("levelup"))
        {
            Task.Run(() =>
            {
                Debug.Log("LevelUP is enabled, registering 'OnExperienceIncrease' event");
                LevelUP.Server.ExperienceEvents.OnExperienceIncrease += LevelUPOnExperienceIncrease;
            });
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

    private static void LevelUPOnExperienceIncrease(IPlayer player, string type, ref ulong amount)
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
                statsIncreaseDistance = (int)(Math.Floor(entityX / Configuration.increaseStatsEveryDistance) +
                                              Math.Floor(entityZ / Configuration.increaseStatsEveryDistance));
            }

            // Height Calculation
            if (Configuration.enableStatusIncreaseByHeight)
            {
                double heightDifference = Configuration.baseStatusHeight - entityY;
                if (heightDifference > 0)
                {
                    statsIncreaseHeight = (int)Math.Floor(heightDifference / Configuration.increaseStatsEveryDownHeight);
                }
            }


            // Age Calculation
            if (Configuration.enableStatusIncreaseByAge)
            {
                statsIncreaseAge = Configuration.GetStatusByWorldAge(serverAPI);
            }
        }

        Debug.LogDebug($"[EXPERIENCE] Before: {amount}");
        // Increasing experience gain
        amount += (ulong)Math.Round(amount *
            (
                (Configuration.levelUPExperienceIncreaseEveryDistance * statsIncreaseDistance) +
                (Configuration.levelUPExperienceIncreaseEveryHeight * statsIncreaseHeight) +
                (Configuration.levelUPExperienceIncreaseEveryAge * statsIncreaseAge)
            ));
        Debug.LogDebug($"[EXPERIENCE] After: {amount}");
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
        return 1.1;
    }


    public override void Dispose()
    {
        base.Dispose();
        overwriter.instance?.UnpatchAll();
    }

    private static readonly Random random = new();
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
            int statsIncreaseAge = 0;

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
                    statsIncreaseDistance = (int)(Math.Floor(entityX / Configuration.increaseStatsEveryDistance) +
                                                  Math.Floor(entityZ / Configuration.increaseStatsEveryDistance));
                }

                // Height Calculation
                if (Configuration.enableStatusIncreaseByHeight)
                {
                    double heightDifference = Configuration.baseStatusHeight - entityY;
                    if (heightDifference > 0)
                    {
                        statsIncreaseHeight = (int)Math.Floor(heightDifference / Configuration.increaseStatsEveryDownHeight);
                    }
                }

                // Age Calculation
                if (Configuration.enableStatusIncreaseByAge)
                {
                    statsIncreaseAge = Configuration.GetStatusByWorldAge(serverAPI);
                }
            }

            // Verification if is a creature and alive
            if (entity.IsCreature && entity.Alive)
            {
                // Getting variation
                double variation = 0;
                if (Configuration.enableStatusVariation)
                {
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

                Debug.LogDebug($"{entity.Code} health percentage: {healthDistance + healthHeight + healthAge} damage percentage: {damageDistance + damageHeight + damageAge} loot percentage: {lootDistance + lootHeight + lootAge}, variation: {variation}");
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
                                Debug.LogDebug($"not in minimum distance: {minimumDistanceToSpawn}, actual: {distance}");
                                return false;
                            }
                        if ((long)maximumDistanceToSpawn != -1)
                            if (distance > (long)maximumDistanceToSpawn)
                            {
                                Debug.LogDebug($"not in maximum distance: {maximumDistanceToSpawn}, actual: {distance}");
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
                                Debug.LogDebug($"not in minimum height: {minimumHeightToSpawn}, actual: {height}");
                                return false;
                            }
                        if ((long)maximumHeightToSpawn != -1)
                            if (height > (long)maximumHeightToSpawn)
                            {
                                Debug.LogDebug($"not in maximum height: {maximumHeightToSpawn}, actual: {height}");
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
                                Debug.LogDebug($"not in minimum age: {minimumAgeToSpawn}, actual: {age}");
                                return false;
                            }
                        if ((long)maximumAgeToSpawn != -1)
                            if (age > (long)maximumAgeToSpawn)
                            {
                                Debug.LogDebug($"not in maximum age: {maximumAgeToSpawn}, actual: {age}");
                                return false;
                            }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"ERROR: Something crashed the spawn condition, probably some mistake in base.json \"entitySpawnConditions\", exception: {ex.Message}");
                }
            }
            else Debug.LogError($"ERROR: Spawn condition is not a JObject, it is {conditionsObject.GetType()}");
        }
        return true;
    }
}

public class Debug
{
    static private ILogger logger;

    static public void LoadLogger(ILogger _logger) => logger = _logger;
    static public void Log(string message)
    {
        logger?.Log(EnumLogType.Notification, $"[RPGDifficulty] {message}");
    }
    static public void LogDebug(string message)
    {
        if (Configuration.enableExtendedLog)
            logger?.Log(EnumLogType.Debug, $"[RPGDifficulty] {message}");
    }
    static public void LogWarn(string message)
    {
        logger?.Log(EnumLogType.Warning, $"[RPGDifficulty] {message}");
    }
    static public void LogError(string message)
    {
        logger?.Log(EnumLogType.Error, $"[RPGDifficulty] {message}");
    }
}