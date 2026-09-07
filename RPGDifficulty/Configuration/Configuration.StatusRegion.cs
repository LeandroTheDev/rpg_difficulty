using System.Collections.Generic;
using OpenConfiguration;
using Vintagestory.API.Common;

namespace RPGDifficulty;

public class ZoneLevelRange
{
    public int min = 0;
    public int max = 15;
}

public class StatusRegionConfigurations
{
    public bool enableRegionSystem = true;

    // Forest density (0..1) threshold above which terrain is classified as forest
    public float forestDensityThreshold = 0.5f;

    // Altitude threshold in blocks above which terrain is classified as mountain
    public int mountainAltitudeThreshold = 160;

    // Level ranges per zone. Key = zone name.
    // Zone names: "forest", "fertile", "mountain", "arctic"
    // For sand/gravel zones, rock type is appended: "sand-claystone", "gravel-granite", etc.
    // Fallback order: "sand-claystone" → "sand" → default
    public Dictionary<string, ZoneLevelRange> zoneLevelRanges = new()
    {
        ["forest"]   = new() { min = 5, max = 20 },
        ["fertile"]  = new() { min = 5, max = 20 },
        ["sand"]     = new() { min = 5, max = 20 },
        ["gravel"]   = new() { min = 5, max = 20 },
        ["mountain"] = new() { min = 15, max = 30 },
        ["arctic"]   = new() { min = 20, max = 40 },
    };

    // Fallback level range used when no zone entry matches
    public ZoneLevelRange defaultLevelRange = new() { min = 0, max = 15 };

    // Per-stat modifier applied per level point in a region
    public double lifeModifierPerLevel = 0.05;
    public double damageModifierPerLevel = 0.02;
    public double lootModifierPerLevel = 0.02;
    public double levelUPExperienceModifierPerLevel = 0.03;

    // Interval in milliseconds to check if a player crossed into a new region
    public int regionCheckIntervalMs = 500;

    // Spawn protection: radius in blocks where level is restricted
    public int initialRegionRadius = 3500;

    // Difficulty level range restricted for the initial/spawn region
    public int initialRegionMinLevel = 0;
    public int initialRegionMaxLevel = 15;

    // Distance bands: every distanceBandSizeInBlocks beyond spawn, level range shifts up
    public bool enableDistanceBands = false;
    public int distanceBandSizeInBlocks = 2000;
    public int levelMinPerBand = 1;
    public int levelMaxPerBand = 2;

    // Cap for distance band levels; -1 = disabled
    public int distanceBandMaxLevelCap = -1;
}

public static partial class Configuration
{
    public static StatusRegionConfigurations StatusRegion = new();

    private static void LoadStatusRegion(ICoreAPI api)
        => StatusRegion = ConfigManager.LoadModConfig<StatusRegionConfigurations>(api, "RPGDifficulty", "region", RPGDifficultyModSystem.Logger);
}
