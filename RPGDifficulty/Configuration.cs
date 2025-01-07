using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Vintagestory.API.Common;

namespace RPGDifficulty;

#pragma warning disable CA2211
public static class Configuration
{
    private static Dictionary<string, object> LoadConfigurationByDirectoryAndName(ICoreAPI api, string directory, string name, string defaultDirectory)
    {
        string directoryPath = Path.Combine(api.DataBasePath, directory);
        string configPath = Path.Combine(api.DataBasePath, directory, $"{name}.json");
        Dictionary<string, object> loadedConfig;
        try
        {
            // Load server configurations
            string jsonConfig = File.ReadAllText(configPath);
            loadedConfig = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonConfig);
        }
        catch (DirectoryNotFoundException)
        {
            Debug.Log($"WARNING: Server configurations directory does not exist creating {name}.json and directory...");
            try
            {
                Directory.CreateDirectory(directoryPath);
            }
            catch (Exception ex)
            {
                Debug.Log($"ERROR: Cannot create directory: {ex.Message}");
            }
            Debug.Log("Loading default configurations...");
            // Load default configurations
            loadedConfig = api.Assets.Get(new AssetLocation(defaultDirectory)).ToObject<Dictionary<string, object>>();

            Debug.Log($"Configurations loaded, saving configs in: {configPath}");
            try
            {
                // Saving default configurations
                string defaultJson = JsonConvert.SerializeObject(loadedConfig, Formatting.Indented);
                File.WriteAllText(configPath, defaultJson);
            }
            catch (Exception ex)
            {
                Debug.Log($"ERROR: Cannot save default files to {configPath}, reason: {ex.Message}");
            }
        }
        catch (FileNotFoundException)
        {
            Debug.Log($"WARNING: Server configurations {name}.json cannot be found, recreating file from default");
            Debug.Log("Loading default configurations...");
            // Load default configurations
            loadedConfig = api.Assets.Get(new AssetLocation(defaultDirectory)).ToObject<Dictionary<string, object>>();

            Debug.Log($"Configurations loaded, saving configs in: {configPath}");
            try
            {
                // Saving default configurations
                string defaultJson = JsonConvert.SerializeObject(loadedConfig, Formatting.Indented);
                File.WriteAllText(configPath, defaultJson);
            }
            catch (Exception ex)
            {
                Debug.Log($"ERROR: Cannot save default files to {configPath}, reason: {ex.Message}");
            }

        }
        catch (Exception ex)
        {
            Debug.Log($"ERROR: Cannot read the server configurations: {ex.Message}");
            Debug.Log("Loading default values from mod assets...");
            // Load default configurations
            loadedConfig = api.Assets.Get(new AssetLocation(defaultDirectory)).ToObject<Dictionary<string, object>>();
        }
        return loadedConfig;
    }


    #region baseconfigs
    public static readonly Dictionary<string, double> whitelistDistance = [];
    public static readonly Dictionary<string, double> blacklistDistance = [];
    public static readonly Dictionary<string, double> whitelistHeight = [];
    public static readonly Dictionary<string, double> blacklistHeight = [];
    public static readonly Dictionary<string, double> whitelistAge = [];
    public static readonly Dictionary<string, double> blacklistAge = [];

    public static bool enableWhitelist = false;
    public static bool enableBlacklist = true;
    public static bool enableStatusIncreaseByHeight = true;
    public static double baseHarvest = 0.5;

    public static bool enableStatusVariation = true;
    public static double minimumVariableStatusAverage = 0.5;
    public static double maxVariableStatusAverage = 1.5;

    public static int increaseStatsEveryDownHeight = 10;
    public static int baseStatusHeight = 60;
    public static double lifeStatsIncreaseEveryHeight = 0.1;
    public static double damageStatsIncreaseEveryHeight = 0.1;
    public static double lootStatsIncreaseEveryHeight = 0.1;
    public static double maximumLifeStatusIncreasedByHeight = 2.0;
    public static double maximumDamageStatusIncreasedByHeight = 2.0;
    public static double maximumLootStatusIncreasedByHeight = 2.0;
    public static double levelUPExperienceIncreaseEveryHeight = 0.1;

    public static bool enableStatusIncreaseByDistance = true;
    public static int increaseStatsEveryDistance = 500;
    public static double lifeStatsIncreaseEveryDistance = 0.1;
    public static double damageStatsIncreaseEveryDistance = 0.1;
    public static double lootStatsIncreaseEveryDistance = 0.1;
    public static double maximumLifeStatusIncreasedByDistance = 10.0;
    public static double maximumDamageStatusIncreasedByDistance = 10.0;
    public static double maximumLootStatusIncreasedByDistance = 10.0;
    public static double levelUPExperienceIncreaseEveryDistance = 0.1;

    public static bool enableStatusIncreaseByAge = true;
    public static int increaseStatsEveryWorldDays = 5;
    public static double lifeStatsIncreaseEveryAge = 0.1;
    public static double damageStatsIncreaseEveryAge = 0.1;
    public static double lootStatsIncreaseEveryAge = 0.1;
    public static double maximumLifeStatusIncreasedByAge = 2.0;
    public static double maximumDamageStatusIncreasedByAge = 2.0;
    public static double maximumLootStatusIncreasedByAge = 2.0;
    public static double levelUPExperienceIncreaseEveryAge = 0.1;

    public static List<object> entitySpawnConditions = [];

    public static int levelUPSecondsPositionUpdate = 1000;
    public static bool enableExtendedLog = true;

    public static void UpdateBaseConfigurations(ICoreAPI api)
    {
        Dictionary<string, object> baseConfigs = LoadConfigurationByDirectoryAndName(
            api,
            "ModConfig/RPGDifficulty/config",
            "base",
            "rpgdifficulty:config/base.json"
        );
        { //enableWhitelist
            if (baseConfigs.TryGetValue("enableWhitelist", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: enableWhitelist is null");
                else if (value is not bool) Debug.Log($"CONFIGURATION ERROR: enableWhitelist is not boolean is {value.GetType()}");
                else enableWhitelist = (bool)value;
            else Debug.Log("CONFIGURATION ERROR: enableWhitelist not set");
        }
        { //enableBlacklist
            if (baseConfigs.TryGetValue("enableBlacklist", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: enableBlacklist is null");
                else if (value is not bool) Debug.Log($"CONFIGURATION ERROR: enableBlacklist is not boolean is {value.GetType()}");
                else enableBlacklist = (bool)value;
            else Debug.Log("CONFIGURATION ERROR: enableBlacklist not set");
        }
        { //enableStatusIncreaseByHeight
            if (baseConfigs.TryGetValue("enableStatusIncreaseByHeight", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: enableStatusIncreaseByHeight is null");
                else if (value is not bool) Debug.Log($"CONFIGURATION ERROR: enableStatusIncreaseByHeight is not boolean is {value.GetType()}");
                else enableStatusIncreaseByHeight = (bool)value;
            else Debug.Log("CONFIGURATION ERROR: enableStatusIncreaseByHeight not set");
        }
        { //increaseStatsEveryDownHeight
            if (baseConfigs.TryGetValue("increaseStatsEveryDownHeight", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: increaseStatsEveryDownHeight is null");
                else if (value is not long) Debug.Log($"CONFIGURATION ERROR: increaseStatsEveryDownHeight is not int is {value.GetType()}");
                else increaseStatsEveryDownHeight = (int)(long)value;
            else Debug.Log("CONFIGURATION ERROR: increaseStatsEveryDownHeight not set");
        }
        { //baseStatusHeight
            if (baseConfigs.TryGetValue("baseStatusHeight", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: baseStatusHeight is null");
                else if (value is not long) Debug.Log($"CONFIGURATION ERROR: baseStatusHeight is not int is {value.GetType()}");
                else baseStatusHeight = (int)(long)value;
            else Debug.Log("CONFIGURATION ERROR: baseStatusHeight not set");
        }
        { //lifeStatsIncreaseEveryHeight
            if (baseConfigs.TryGetValue("lifeStatsIncreaseEveryHeight", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: lifeStatsIncreaseEveryHeight is null");
                else if (value is not double) Debug.Log($"CONFIGURATION ERROR: lifeStatsIncreaseEveryHeight is not double is {value.GetType()}");
                else lifeStatsIncreaseEveryHeight = (double)value;
            else Debug.Log("CONFIGURATION ERROR: lifeStatsIncreaseEveryHeight not set");
        }
        { //baseHarvest
            if (baseConfigs.TryGetValue("baseHarvest", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: baseHarvest is null");
                else if (value is not double) Debug.Log($"CONFIGURATION ERROR: baseHarvest is not double is {value.GetType()}");
                else baseHarvest = (double)value;
            else Debug.Log("CONFIGURATION ERROR: baseHarvest not set");
        }
        { //enableStatusVariation
            if (baseConfigs.TryGetValue("enableStatusVariation", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: enableStatusVariation is null");
                else if (value is not bool) Debug.Log($"CONFIGURATION ERROR: enableStatusVariation is not boolean is {value.GetType()}");
                else enableStatusVariation = (bool)value;
            else Debug.Log("CONFIGURATION ERROR: enableStatusVariation not set");
        }
        { //minimumVariableStatusAverage
            if (baseConfigs.TryGetValue("minimumVariableStatusAverage", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: minimumVariableStatusAverage is null");
                else if (value is not double) Debug.Log($"CONFIGURATION ERROR: minimumVariableStatusAverage is not double is {value.GetType()}");
                else minimumVariableStatusAverage = (double)value;
            else Debug.Log("CONFIGURATION ERROR: minimumVariableStatusAverage not set");
        }
        { //maxVariableStatusAverage
            if (baseConfigs.TryGetValue("maxVariableStatusAverage", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: maxVariableStatusAverage is null");
                else if (value is not double) Debug.Log($"CONFIGURATION ERROR: maxVariableStatusAverage is not double is {value.GetType()}");
                else maxVariableStatusAverage = (double)value;
            else Debug.Log("CONFIGURATION ERROR: maxVariableStatusAverage not set");
        }
        { //damageStatsIncreaseEveryDistance
            if (baseConfigs.TryGetValue("damageStatsIncreaseEveryDistance", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: damageStatsIncreaseEveryDistance is null");
                else if (value is not double) Debug.Log($"CONFIGURATION ERROR: damageStatsIncreaseEveryDistance is not double is {value.GetType()}");
                else damageStatsIncreaseEveryDistance = (double)value;
            else Debug.Log("CONFIGURATION ERROR: damageStatsIncreaseEveryDistance not set");
        }
        { //lootStatsIncreaseEveryHeight
            if (baseConfigs.TryGetValue("lootStatsIncreaseEveryHeight", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: lootStatsIncreaseEveryHeight is null");
                else if (value is not double) Debug.Log($"CONFIGURATION ERROR: lootStatsIncreaseEveryHeight is not double is {value.GetType()}");
                else lootStatsIncreaseEveryHeight = (double)value;
            else Debug.Log("CONFIGURATION ERROR: lootStatsIncreaseEveryHeight not set");
        }
        { //maximumLifeStatusIncreasedByHeight
            if (baseConfigs.TryGetValue("maximumLifeStatusIncreasedByHeight", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: maximumLifeStatusIncreasedByHeight is null");
                else if (value is not double) Debug.Log($"CONFIGURATION ERROR: maximumLifeStatusIncreasedByHeight is not double is {value.GetType()}");
                else maximumLifeStatusIncreasedByHeight = (double)value;
            else Debug.Log("CONFIGURATION ERROR: maximumLifeStatusIncreasedByHeight not set");
        }
        { //maximumDamageStatusIncreasedByHeight
            if (baseConfigs.TryGetValue("maximumDamageStatusIncreasedByHeight", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: maximumDamageStatusIncreasedByHeight is null");
                else if (value is not double) Debug.Log($"CONFIGURATION ERROR: maximumDamageStatusIncreasedByHeight is not double is {value.GetType()}");
                else maximumDamageStatusIncreasedByHeight = (double)value;
            else Debug.Log("CONFIGURATION ERROR: maximumDamageStatusIncreasedByHeight not set");
        }
        { //maximumLootStatusIncreasedByHeight
            if (baseConfigs.TryGetValue("maximumLootStatusIncreasedByHeight", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: maximumLootStatusIncreasedByHeight is null");
                else if (value is not double) Debug.Log($"CONFIGURATION ERROR: maximumLootStatusIncreasedByHeight is not double is {value.GetType()}");
                else maximumLootStatusIncreasedByHeight = (double)value;
            else Debug.Log("CONFIGURATION ERROR: maximumLootStatusIncreasedByHeight not set");
        }
        { //levelUPExperienceIncreaseEveryHeight
            if (baseConfigs.TryGetValue("levelUPExperienceIncreaseEveryHeight", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: levelUPExperienceIncreaseEveryHeight is null");
                else if (value is not double) Debug.Log($"CONFIGURATION ERROR: levelUPExperienceIncreaseEveryHeight is not double is {value.GetType()}");
                else levelUPExperienceIncreaseEveryHeight = (double)value;
            else Debug.Log("CONFIGURATION ERROR: levelUPExperienceIncreaseEveryHeight not set");
        }
        { //enableStatusIncreaseByDistance
            if (baseConfigs.TryGetValue("enableStatusIncreaseByDistance", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: enableStatusIncreaseByDistance is null");
                else if (value is not bool) Debug.Log($"CONFIGURATION ERROR: enableStatusIncreaseByDistance is not boolean is {value.GetType()}");
                else enableStatusIncreaseByDistance = (bool)value;
            else Debug.Log("CONFIGURATION ERROR: enableStatusIncreaseByDistance not set");
        }
        { //increaseStatsEveryDistance
            if (baseConfigs.TryGetValue("increaseStatsEveryDistance", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: increaseStatsEveryDistance is null");
                else if (value is not long) Debug.Log($"CONFIGURATION ERROR: increaseStatsEveryDistance is not int is {value.GetType()}");
                else increaseStatsEveryDistance = (int)(long)value;
            else Debug.Log("CONFIGURATION ERROR: increaseStatsEveryDistance not set");
        }
        { //lifeStatsIncreaseEveryDistance
            if (baseConfigs.TryGetValue("lifeStatsIncreaseEveryDistance", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: lifeStatsIncreaseEveryDistance is null");
                else if (value is not double) Debug.Log($"CONFIGURATION ERROR: lifeStatsIncreaseEveryDistance is not double is {value.GetType()}");
                else lifeStatsIncreaseEveryDistance = (double)value;
            else Debug.Log("CONFIGURATION ERROR: lifeStatsIncreaseEveryDistance not set");
        }
        { //damageStatsIncreaseEveryDistance
            if (baseConfigs.TryGetValue("damageStatsIncreaseEveryDistance", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: damageStatsIncreaseEveryDistance is null");
                else if (value is not double) Debug.Log($"CONFIGURATION ERROR: damageStatsIncreaseEveryDistance is not double is {value.GetType()}");
                else damageStatsIncreaseEveryDistance = (double)value;
            else Debug.Log("CONFIGURATION ERROR: damageStatsIncreaseEveryDistance not set");
        }
        { //lootStatsIncreaseEveryDistance
            if (baseConfigs.TryGetValue("lootStatsIncreaseEveryDistance", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: lootStatsIncreaseEveryDistance is null");
                else if (value is not double) Debug.Log($"CONFIGURATION ERROR: lootStatsIncreaseEveryDistance is not double is {value.GetType()}");
                else lootStatsIncreaseEveryDistance = (double)value;
            else Debug.Log("CONFIGURATION ERROR: lootStatsIncreaseEveryDistance not set");
        }
        { //maximumLifeStatusIncreasedByDistance
            if (baseConfigs.TryGetValue("maximumLifeStatusIncreasedByDistance", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: maximumLifeStatusIncreasedByDistance is null");
                else if (value is not double) Debug.Log($"CONFIGURATION ERROR: maximumLifeStatusIncreasedByDistance is not double is {value.GetType()}");
                else maximumLifeStatusIncreasedByDistance = (double)value;
            else Debug.Log("CONFIGURATION ERROR: maximumLifeStatusIncreasedByDistance not set");
        }
        { //maximumDamageStatusIncreasedByDistance
            if (baseConfigs.TryGetValue("maximumDamageStatusIncreasedByDistance", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: maximumDamageStatusIncreasedByDistance is null");
                else if (value is not double) Debug.Log($"CONFIGURATION ERROR: maximumDamageStatusIncreasedByDistance is not double is {value.GetType()}");
                else maximumDamageStatusIncreasedByDistance = (double)value;
            else Debug.Log("CONFIGURATION ERROR: maximumDamageStatusIncreasedByDistance not set");
        }
        { //maximumLootStatusIncreasedByDistance
            if (baseConfigs.TryGetValue("maximumLootStatusIncreasedByDistance", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: maximumLootStatusIncreasedByDistance is null");
                else if (value is not double) Debug.Log($"CONFIGURATION ERROR: maximumLootStatusIncreasedByDistance is not double is {value.GetType()}");
                else maximumLootStatusIncreasedByDistance = (double)value;
            else Debug.Log("CONFIGURATION ERROR: maximumLootStatusIncreasedByDistance not set");
        }
        { //levelUPExperienceIncreaseEveryDistance
            if (baseConfigs.TryGetValue("levelUPExperienceIncreaseEveryDistance", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: levelUPExperienceIncreaseEveryDistance is null");
                else if (value is not double) Debug.Log($"CONFIGURATION ERROR: levelUPExperienceIncreaseEveryDistance is not double is {value.GetType()}");
                else levelUPExperienceIncreaseEveryDistance = (double)value;
            else Debug.Log("CONFIGURATION ERROR: levelUPExperienceIncreaseEveryDistance not set");
        }
        { //levelUPSecondsPositionUpdate
            if (baseConfigs.TryGetValue("levelUPSecondsPositionUpdate", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: levelUPSecondsPositionUpdate is null");
                else if (value is not long) Debug.Log($"CONFIGURATION ERROR: levelUPSecondsPositionUpdate is not int is {value.GetType()}");
                else levelUPSecondsPositionUpdate = (int)(long)value;
            else Debug.Log("CONFIGURATION ERROR: levelUPSecondsPositionUpdate not set");
        }
        { //enableStatusIncreaseByAge
            if (baseConfigs.TryGetValue("enableStatusIncreaseByAge", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: enableStatusIncreaseByAge is null");
                else if (value is not bool) Debug.Log($"CONFIGURATION ERROR: enableStatusIncreaseByAge is not boolean is {value.GetType()}");
                else enableStatusIncreaseByAge = (bool)value;
            else Debug.Log("CONFIGURATION ERROR: enableStatusIncreaseByAge not set");
        }
        { //increaseStatsEveryWorldDays
            if (baseConfigs.TryGetValue("increaseStatsEveryWorldDays", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: increaseStatsEveryWorldDays is null");
                else if (value is not long) Debug.Log($"CONFIGURATION ERROR: increaseStatsEveryWorldDays is not int is {value.GetType()}");
                else increaseStatsEveryWorldDays = (int)(long)value;
            else Debug.Log("CONFIGURATION ERROR: increaseStatsEveryWorldDays not set");
        }
        { //lifeStatsIncreaseEveryAge
            if (baseConfigs.TryGetValue("lifeStatsIncreaseEveryAge", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: lifeStatsIncreaseEveryAge is null");
                else if (value is not double) Debug.Log($"CONFIGURATION ERROR: lifeStatsIncreaseEveryAge is not double is {value.GetType()}");
                else lifeStatsIncreaseEveryAge = (double)value;
            else Debug.Log("CONFIGURATION ERROR: lifeStatsIncreaseEveryAge not set");
        }
        { //damageStatsIncreaseEveryAge
            if (baseConfigs.TryGetValue("damageStatsIncreaseEveryAge", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: damageStatsIncreaseEveryAge is null");
                else if (value is not double) Debug.Log($"CONFIGURATION ERROR: damageStatsIncreaseEveryAge is not double is {value.GetType()}");
                else damageStatsIncreaseEveryAge = (double)value;
            else Debug.Log("CONFIGURATION ERROR: damageStatsIncreaseEveryAge not set");
        }
        { //lootStatsIncreaseEveryAge
            if (baseConfigs.TryGetValue("lootStatsIncreaseEveryAge", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: lootStatsIncreaseEveryAge is null");
                else if (value is not double) Debug.Log($"CONFIGURATION ERROR: lootStatsIncreaseEveryAge is not double is {value.GetType()}");
                else lootStatsIncreaseEveryAge = (double)value;
            else Debug.Log("CONFIGURATION ERROR: lootStatsIncreaseEveryAge not set");
        }
        { //maximumLifeStatusIncreasedByAge
            if (baseConfigs.TryGetValue("maximumLifeStatusIncreasedByAge", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: maximumLifeStatusIncreasedByAge is null");
                else if (value is not double) Debug.Log($"CONFIGURATION ERROR: maximumLifeStatusIncreasedByAge is not double is {value.GetType()}");
                else maximumLifeStatusIncreasedByAge = (double)value;
            else Debug.Log("CONFIGURATION ERROR: maximumLifeStatusIncreasedByAge not set");
        }
        { //maximumDamageStatusIncreasedByAge
            if (baseConfigs.TryGetValue("maximumDamageStatusIncreasedByAge", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: maximumDamageStatusIncreasedByAge is null");
                else if (value is not double) Debug.Log($"CONFIGURATION ERROR: maximumDamageStatusIncreasedByAge is not double is {value.GetType()}");
                else maximumDamageStatusIncreasedByAge = (double)value;
            else Debug.Log("CONFIGURATION ERROR: maximumDamageStatusIncreasedByAge not set");
        }
        { //maximumLootStatusIncreasedByAge
            if (baseConfigs.TryGetValue("maximumLootStatusIncreasedByAge", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: maximumLootStatusIncreasedByAge is null");
                else if (value is not double) Debug.Log($"CONFIGURATION ERROR: maximumLootStatusIncreasedByAge is not double is {value.GetType()}");
                else maximumLootStatusIncreasedByAge = (double)value;
            else Debug.Log("CONFIGURATION ERROR: maximumLootStatusIncreasedByAge not set");
        }
        { //levelUPExperienceIncreaseEveryAge
            if (baseConfigs.TryGetValue("levelUPExperienceIncreaseEveryAge", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: levelUPExperienceIncreaseEveryAge is null");
                else if (value is not double) Debug.Log($"CONFIGURATION ERROR: levelUPExperienceIncreaseEveryAge is not double is {value.GetType()}");
                else levelUPExperienceIncreaseEveryAge = (double)value;
            else Debug.Log("CONFIGURATION ERROR: levelUPExperienceIncreaseEveryAge not set");
        }
        { //entitySpawnConditions
            if (baseConfigs.TryGetValue("entitySpawnConditions", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: entitySpawnConditions is null");
                else if (value is not Newtonsoft.Json.Linq.JArray) Debug.Log($"CONFIGURATION ERROR: entitySpawnConditions is not List<object> is {value.GetType()}");
                else entitySpawnConditions = (value as Newtonsoft.Json.Linq.JArray).ToObject<List<object>>();
            else Debug.Log("CONFIGURATION ERROR: entitySpawnConditions not set");
        }
        { //enableExtendedLog
            if (baseConfigs.TryGetValue("enableExtendedLog", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: enableExtendedLog is null");
                else if (value is not bool) Debug.Log($"CONFIGURATION ERROR: enableExtendedLog is not boolean is {value.GetType()}");
                else enableExtendedLog = (bool)value;
            else Debug.Log("CONFIGURATION ERROR: enableExtendedLog not set");
        }

        // Get whitelistDistance
        whitelistDistance.Clear();
        Dictionary<string, object> tmpwhitelistDistance = LoadConfigurationByDirectoryAndName(
            api,
            "ModConfig/RPGDifficulty/config",
            "whitelistdistance",
            "rpgdifficulty:config/whitelistdistance.json"
        );
        foreach (KeyValuePair<string, object> pair in tmpwhitelistDistance)
        {
            if (pair.Value is double value) whitelistDistance.Add(pair.Key, (double)value);
            else Debug.Log($"CONFIGURATION ERROR: whitelistDistance {pair.Key} is not double");
        }

        // Get blacklistDistance
        blacklistDistance.Clear();
        Dictionary<string, object> tmpblacklistDistance = LoadConfigurationByDirectoryAndName(
            api,
            "ModConfig/RPGDifficulty/config",
            "blacklistdistance",
            "rpgdifficulty:config/blacklistdistance.json"
        );
        foreach (KeyValuePair<string, object> pair in tmpblacklistDistance)
        {
            if (pair.Value is double value) blacklistDistance.Add(pair.Key, (double)value);
            else Debug.Log($"CONFIGURATION ERROR: whitelist {pair.Key} is not double");
        }

        // Get whitelistHeight
        whitelistHeight.Clear();
        Dictionary<string, object> tmpwhitelistHeight = LoadConfigurationByDirectoryAndName(
            api,
            "ModConfig/RPGDifficulty/config",
            "whitelistheight",
            "rpgdifficulty:config/whitelistheight.json"
        );
        foreach (KeyValuePair<string, object> pair in tmpwhitelistHeight)
        {
            if (pair.Value is double value) whitelistHeight.Add(pair.Key, (double)value);
            else Debug.Log($"CONFIGURATION ERROR: whitelistHeight {pair.Key} is not double");
        }

        // Get blacklistHeight
        blacklistHeight.Clear();
        Dictionary<string, object> tmpblacklistHeight = LoadConfigurationByDirectoryAndName(
            api,
            "ModConfig/RPGDifficulty/config",
            "blacklistheight",
            "rpgdifficulty:config/blacklistheight.json"
        );
        foreach (KeyValuePair<string, object> pair in tmpblacklistHeight)
        {
            if (pair.Value is double value) blacklistHeight.Add(pair.Key, (double)value);
            else Debug.Log($"CONFIGURATION ERROR: whitelist {pair.Key} is not double");
        }

        // Get whitelistAge
        whitelistAge.Clear();
        Dictionary<string, object> tmpwhitelistAge = LoadConfigurationByDirectoryAndName(
            api,
            "ModConfig/RPGDifficulty/config",
            "whitelistage",
            "rpgdifficulty:config/whitelistage.json"
        );
        foreach (KeyValuePair<string, object> pair in tmpwhitelistAge)
        {
            if (pair.Value is double value) whitelistAge.Add(pair.Key, (double)value);
            else Debug.Log($"CONFIGURATION ERROR: whitelistAge {pair.Key} is not double");
        }

        // Get blacklistAge
        blacklistAge.Clear();
        Dictionary<string, object> tmpblacklistAge = LoadConfigurationByDirectoryAndName(
            api,
            "ModConfig/RPGDifficulty/config",
            "blacklistage",
            "rpgdifficulty:config/blacklistage.json"
        );
        foreach (KeyValuePair<string, object> pair in tmpblacklistAge)
        {
            if (pair.Value is double value) blacklistAge.Add(pair.Key, (double)value);
            else Debug.Log($"CONFIGURATION ERROR: whitelist {pair.Key} is not double");
        }
    }

    public static int GetStatusByWorldAge(ICoreAPI serverAPI)
    => (int)serverAPI.World.Calendar.ElapsedDays / increaseStatsEveryWorldDays;

    /// Returns false for NO status increase
    /// True for status increase
    public static bool BlackWhiteListCheckForDistance(string entityCode)
    {
        // Blacklist Check
        if (enableBlacklist)
            if (blacklistDistance.TryGetValue(entityCode, out double _))
            { if (enableExtendedLog) Debug.Log($"{entityCode} is on blacklist, ignoring stats distance"); return false; }
        // Whitelist Check
        if (enableWhitelist)
            // In whitelist
            if (whitelistDistance.TryGetValue(entityCode, out double _))
            { if (enableExtendedLog) Debug.Log($"{entityCode} is on whitelist, increasing stats distance"); return true; }
            // Not in whitelist
            else { if (enableExtendedLog) Debug.Log($"{entityCode} is not on whitelist, ignoring stats distance"); return false; }
        // Check success 
        return true;
    }

    /// Returns false for NO status increase
    /// True for status increase
    public static bool BlackWhiteListCheckForHeight(string entityCode)
    {
        // Blacklist Check
        if (enableBlacklist)
            if (blacklistHeight.TryGetValue(entityCode, out double _))
            { if (enableExtendedLog) Debug.Log($"{entityCode} is on blacklist, ignoring stats height"); return false; }
        // Whitelist Check
        if (enableWhitelist)
            // In whitelist
            if (whitelistHeight.TryGetValue(entityCode, out double _))
            { if (enableExtendedLog) Debug.Log($"{entityCode} is on whitelist, increasing stats height"); return true; }
            // Not in whitelist
            else { if (enableExtendedLog) Debug.Log($"{entityCode} is not on whitelist, ignoring stats height"); return false; }
        // Check success 
        return true;
    }

    /// Returns false for NO status increase
    /// True for status increase
    public static bool BlackWhiteListCheckForAge(string entityCode)
    {
        // Blacklist Check
        if (enableBlacklist)
            if (blacklistAge.TryGetValue(entityCode, out double _))
            { if (enableExtendedLog) Debug.Log($"{entityCode} is on blacklist, ignoring stats age"); return false; }
        // Whitelist Check
        if (enableWhitelist)
            // In whitelist
            if (whitelistAge.TryGetValue(entityCode, out double _))
            { if (enableExtendedLog) Debug.Log($"{entityCode} is on whitelist, increasing stats age"); return true; }
            // Not in whitelist
            else { if (enableExtendedLog) Debug.Log($"{entityCode} is not on whitelist, ignoring stats age"); return false; }
        // Check success 
        return true;
    }
    #endregion
}