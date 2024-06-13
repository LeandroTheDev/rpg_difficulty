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
    public static bool enableStatusIncreaseByHeight = true;
    public static int increaseStatsEveryDownHeight = 10;
    public static int baseStatusHeight = 60;
    public static double lifeStatsIncreaseEveryHeight = 0.1;
    public static double damageStatsIncreaseEveryHeight = 0.1;
    public static double lootStatsIncreaseEveryHeight = 0.1;
    public static double levelUPExperienceIncreaseEveryHeight = 0.1;
    public static bool enableStatusIncreaseByDistance = true;
    public static int increaseStatsEveryDistance = 500;
    public static double lifeStatsIncreaseEveryDistance = 0.1;
    public static double damageStatsIncreaseEveryDistance = 0.1;
    public static double lootStatsIncreaseEveryDistance = 0.1;
    public static double levelUPExperienceIncreaseEveryDistance = 0.1;
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
        { //levelUPExperienceIncreaseEveryDistance
            if (baseConfigs.TryGetValue("levelUPExperienceIncreaseEveryDistance", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: levelUPExperienceIncreaseEveryDistance is null");
                else if (value is not double) Debug.Log($"CONFIGURATION ERROR: levelUPExperienceIncreaseEveryDistance is not double is {value.GetType()}");
                else levelUPExperienceIncreaseEveryDistance = (double)value;
            else Debug.Log("CONFIGURATION ERROR: levelUPExperienceIncreaseEveryDistance not set");
        }
        { //enableExtendedLog
            if (baseConfigs.TryGetValue("enableExtendedLog", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: enableExtendedLog is null");
                else if (value is not bool) Debug.Log($"CONFIGURATION ERROR: enableExtendedLog is not boolean is {value.GetType()}");
                else enableExtendedLog = (bool)value;
            else Debug.Log("CONFIGURATION ERROR: enableExtendedLog not set");
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
        Debug.Log($"CONFIG: enableStatusIncreaseByHeight, value: {enableStatusIncreaseByHeight}");
        Debug.Log($"CONFIG: increaseStatsEveryDownHeight, value: {increaseStatsEveryDownHeight}");
        Debug.Log($"CONFIG: baseStatusHeight, value: {baseStatusHeight}");
        Debug.Log($"CONFIG: lifeStatsIncreaseEveryHeight, value: {lifeStatsIncreaseEveryHeight}");
        Debug.Log($"CONFIG: damageStatsIncreaseEveryHeight, value: {damageStatsIncreaseEveryHeight}");
        Debug.Log($"CONFIG: lootStatsIncreaseEveryHeight, value: {lootStatsIncreaseEveryHeight}");
        Debug.Log($"CONFIG: levelUPExperienceIncreaseEveryHeight, value: {levelUPExperienceIncreaseEveryHeight}");
        Debug.Log($"CONFIG: enableStatusIncreaseByDistance, value: {enableStatusIncreaseByDistance}");
        Debug.Log($"CONFIG: increaseStatsEveryDistance, value: {increaseStatsEveryDistance}");
        Debug.Log($"CONFIG: lifeStatsIncreaseEveryDistance, value: {lifeStatsIncreaseEveryDistance}");
        Debug.Log($"CONFIG: damageStatsIncreaseEveryDistance, value: {damageStatsIncreaseEveryDistance}");
        Debug.Log($"CONFIG: lootStatsIncreaseEveryDistance, value: {lootStatsIncreaseEveryDistance}");
        Debug.Log($"CONFIG: levelUPExperienceIncreaseEveryDistance, value: {levelUPExperienceIncreaseEveryDistance}");
        Debug.Log($"CONFIG: enableExtendedLog, value: {enableExtendedLog}");
    }
    #endregion
}