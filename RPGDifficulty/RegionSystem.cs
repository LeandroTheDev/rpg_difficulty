using System;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace RPGDifficulty;

public static class RegionSystem
{
    private static ICoreServerAPI _api;

    public static void Initialize(ICoreServerAPI api)
    {
        _api = api;
    }

    /// <summary>
    /// Determines the zone at a position using only worldgen-fixed data (climate maps, rock strata, terrain height).
    /// Never reads the current surface block, so player modifications do not affect the result.
    /// Zone names: "forest", "fertile", "mountain", "arctic", "sand-{rocktype}", "gravel-{rocktype}"
    /// </summary>
    public static string GetZoneAtPosition(double posX, double posZ)
    {
        int x = (int)posX;
        int z = (int)posZ;
        int chunkSize = GlobalConstants.ChunkSize;

        IMapChunk mapChunk = _api.World.BlockAccessor.GetMapChunkAtBlockPos(new BlockPos(x, 0, z));
        if (mapChunk == null) return "unknown";

        int localX = GameMath.Mod(x, chunkSize);
        int localZ = GameMath.Mod(z, chunkSize);

        int worldgenY = mapChunk.WorldGenTerrainHeightMap[localZ * chunkSize + localX];

        ClimateCondition climate = _api.World.BlockAccessor.GetClimateAt(
            new BlockPos(x, worldgenY, z),
            EnumGetClimateMode.WorldGenValues
        );
        if (climate == null) return "unknown";

        if (worldgenY >= Configuration.StatusRegion.mountainAltitudeThreshold)
            return "mountain";

        if (climate.WorldGenTemperature <= -16f)
            return "arctic";

        // 0.25 matches the maxFertility of L1 Infertile Soil in blocklayers.json.
        // The 0.22–0.25 range is ambiguous (sub-chunk noise picks soil or sand), so we treat it as infertile.
        if (climate.Fertility >= 0.25f && climate.ForestDensity >= Configuration.StatusRegion.forestDensityThreshold)
            return "forest";

        if (climate.Fertility >= 0.25f)
            return "fertile";

        string rockType = GetRockType(mapChunk, localX, localZ);

        if (climate.WorldGenTemperature >= 8f)
            return $"sand-{rockType}";

        return $"gravel-{rockType}";
    }

    public static int GetRegionLevel(double posX, double posZ, string zone = null)
    {
        if (!Configuration.StatusRegion.enableRegionSystem) return 0;

        zone ??= GetZoneAtPosition(posX, posZ);

        int minLevel = Configuration.StatusRegion.initialRegionMinLevel;
        int maxLevel = Configuration.StatusRegion.initialRegionMaxLevel;
        bool isInitialRegion = false;
        double distanceFromSpawn = -1;

        EntityPos spawnPos = _api.World.DefaultSpawnPosition;
        if (spawnPos != null)
        {
            double dx = posX - spawnPos.X;
            double dz = posZ - spawnPos.Z;
            distanceFromSpawn = Math.Sqrt(dx * dx + dz * dz);

            if (distanceFromSpawn <= Configuration.StatusRegion.initialRegionRadius)
            {
                isInitialRegion = true;
            }
            else
            {
                var range = GetZoneLevelRange(zone);
                minLevel = range.min;
                maxLevel = range.max;

                if (Configuration.StatusRegion.enableDistanceBands)
                {
                    int band = (int)(distanceFromSpawn / Configuration.StatusRegion.distanceBandSizeInBlocks);
                    minLevel += band * Configuration.StatusRegion.levelMinPerBand;
                    maxLevel += band * Configuration.StatusRegion.levelMaxPerBand;

                    int cap = Configuration.StatusRegion.distanceBandMaxLevelCap;
                    if (cap >= 0)
                    {
                        maxLevel = Math.Min(maxLevel, cap);
                        minLevel = Math.Min(minLevel, cap);
                    }
                }
            }
        }

        // Use world seed + position to get a deterministic level within the range
        int chunkSize = GlobalConstants.ChunkSize;
        int chunkX = (int)Math.Floor(posX / chunkSize);
        int chunkZ = (int)Math.Floor(posZ / chunkSize);
        int seed = _api.World.Seed ^ (chunkX * 123456789) ^ (chunkZ * 987654321);
        int level = new Random(seed).Next(minLevel, maxLevel + 1);

        RPGDifficultyModSystem.Logger.LogDebug(
            $"[Region] Zone={zone} Chunk=({chunkX},{chunkZ}) | " +
            $"DistanceFromSpawn={distanceFromSpawn:F0} | " +
            $"InitialRegion={isInitialRegion} | " +
            $"LevelRange=[{minLevel}-{maxLevel}] | " +
            $"Level={level}"
        );

        return level;
    }

    private static ZoneLevelRange GetZoneLevelRange(string zone)
    {
        var ranges = Configuration.StatusRegion.zoneLevelRanges;

        if (ranges.TryGetValue(zone, out var exact)) return exact;

        // Fallback: strip rock type suffix (e.g. "sand-claystone" → "sand")
        int dash = zone.IndexOf('-');
        if (dash > 0)
        {
            string baseZone = zone[..dash];
            if (ranges.TryGetValue(baseZone, out var fallback)) return fallback;
        }

        return Configuration.StatusRegion.defaultLevelRange;
    }

    public static string GetDebugInfo(double posX, double posZ)
    {
        int x = (int)posX;
        int z = (int)posZ;
        int chunkSize = GlobalConstants.ChunkSize;

        IMapChunk mapChunk = _api.World.BlockAccessor.GetMapChunkAtBlockPos(new BlockPos(x, 0, z));
        if (mapChunk == null) return "mapChunk is null";

        int localX = GameMath.Mod(x, chunkSize);
        int localZ = GameMath.Mod(z, chunkSize);
        int worldgenY = mapChunk.WorldGenTerrainHeightMap[localZ * chunkSize + localX];

        ClimateCondition climate = _api.World.BlockAccessor.GetClimateAt(
            new BlockPos(x, worldgenY, z),
            EnumGetClimateMode.WorldGenValues
        );

        string rockType = GetRockType(mapChunk, localX, localZ);
        string zone = GetZoneAtPosition(posX, posZ);
        int level = GetRegionLevel(posX, posZ, zone);

        return $"Zone={zone} | Level={level}\n" +
               $"WorldgenY={worldgenY} (mountain&gt;={Configuration.StatusRegion.mountainAltitudeThreshold})\n" +
               $"Temperature={climate?.WorldGenTemperature:F2} (arctic&lt;=-16, sand&gt;=8)\n" +
               $"Fertility={climate?.Fertility:F3} (fertile/forest&gt;=0.25)\n" +
               $"ForestDensity={climate?.ForestDensity:F3} (forest&gt;={Configuration.StatusRegion.forestDensityThreshold})\n" +
               $"Rainfall={climate?.WorldgenRainfall:F3}\n" +
               $"RockType={rockType}";
    }

    private static string GetRockType(IMapChunk mapChunk, int localX, int localZ)
    {
        if (mapChunk.TopRockIdMap == null) return "unknown";
        int chunkSize = GlobalConstants.ChunkSize;
        int rockBlockId = mapChunk.TopRockIdMap[localZ * chunkSize + localX];
        Block rockBlock = _api.World.GetBlock(rockBlockId);
        if (rockBlock?.Code == null) return "unknown";
        // SecondCodePart() returns the full path when there's only one dash (e.g. "rock-claystone").
        // We want the part after the first dash: "rock-claystone" → "claystone".
        string path = rockBlock.Code.Path;
        int dash = path.IndexOf('-');
        return dash >= 0 ? path[(dash + 1)..] : path;
    }
}
