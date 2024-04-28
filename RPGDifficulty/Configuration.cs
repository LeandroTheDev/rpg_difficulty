using System;
using System.Collections.Generic;
using RPGDifficulty;
using Vintagestory.API.Common;

namespace LevelUP;

#pragma warning disable CA2211
public static class Configuration
{
    #region baseconfigs
    public static readonly Dictionary<string, double> whitelist = [];
    public static readonly Dictionary<string, double> blacklist = [];
    public static bool enableWhitelist = false;
    public static bool enableBlacklist = true;
    public static int increaseStatsEveryDistance = 500;
    public static double lifeStatsIncreaseEveryDistance = 0.1;
    public static double damageStatsIncreaseEveryDistance = 0.1;
    public static bool enableExtendedLog = true;

    public static void UpdateBaseConfigurations(ICoreAPI api)
    {
        Dictionary<string, object> baseConfigs = api.Assets.Get(new AssetLocation("rpgdifficulty:config/base.json")).ToObject<Dictionary<string, object>>();
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

        // Get whitelist
        whitelist.Clear();
        Dictionary<string, object> tmpwhitelist = api.Assets.Get(new AssetLocation("rpgdifficulty:config/whitelist.json")).ToObject<Dictionary<string, object>>();
        foreach (KeyValuePair<string, object> pair in tmpwhitelist)
        {
            if (pair.Value is double value) whitelist.Add(pair.Key, (double)value);
            else Debug.Log($"CONFIGURATION ERROR: whitelist {pair.Key} is not double");
        }

        // Get blacklist
        blacklist.Clear();
        Dictionary<string, object> tmpblacklist = api.Assets.Get(new AssetLocation("rpgdifficulty:config/blacklist.json")).ToObject<Dictionary<string, object>>();
        foreach (KeyValuePair<string, object> pair in tmpblacklist)
        {
            if (pair.Value is double value) blacklist.Add(pair.Key, (double)value);
            else Debug.Log($"CONFIGURATION ERROR: whitelist {pair.Key} is not double");
        }   
    }

    public static void LogConfigurations()
    {
        Debug.Log($"CONFIG: enableWhitelist, value: {enableWhitelist}");
        Debug.Log($"CONFIG: enableBlacklist, value: {enableBlacklist}");
        Debug.Log($"CONFIG: increaseStatsEveryDistance, value: {increaseStatsEveryDistance}");
        Debug.Log($"CONFIG: lifeStatsIncreaseEveryDistance, value: {lifeStatsIncreaseEveryDistance}");
        Debug.Log($"CONFIG: damageStatsIncreaseEveryDistance, value: {damageStatsIncreaseEveryDistance}");
    }
    #endregion
}