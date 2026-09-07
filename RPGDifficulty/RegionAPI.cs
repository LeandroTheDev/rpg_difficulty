using System;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Server;

namespace RPGDifficulty;

public class RegionInfo
{
    public int RegionX { get; init; }
    public int RegionZ { get; init; }
    public int Level { get; init; }
    public bool IsInitialRegion { get; init; }
    public string Zone { get; init; }
}

public static class RegionAPI
{
    /// <summary>
    /// Fired when a player crosses into a different zone (climate or forest boundary).
    /// </summary>
    public static event Action<IServerPlayer, RegionInfo, RegionInfo> OnPlayerEnterRegion;

    private static ICoreServerAPI _api;
    private static readonly Dictionary<string, string> _playerZones = new();

    internal static void Initialize(ICoreServerAPI api)
    {
        _api = api;
        api.Event.PlayerNowPlaying += OnPlayerJoin;
        api.Event.PlayerDisconnect += OnPlayerLeave;
        api.Event.RegisterGameTickListener(CheckPlayerRegions, Configuration.StatusRegion.regionCheckIntervalMs);
    }

    private static void OnPlayerJoin(IServerPlayer player)
    {
        string zone = RegionSystem.GetZoneAtPosition(player.Entity.Pos.X, player.Entity.Pos.Z);
        _playerZones[player.PlayerUID] = zone;
    }

    private static void OnPlayerLeave(IServerPlayer player)
    {
        _playerZones.Remove(player.PlayerUID);
    }

    private static void CheckPlayerRegions(float dt)
    {
        foreach (IPlayer playerBase in _api.World.AllOnlinePlayers)
        {
            if (playerBase is not IServerPlayer player) continue;
            if (player.Entity == null) continue;

            double posX = player.Entity.Pos.X;
            double posZ = player.Entity.Pos.Z;
            string newZone = RegionSystem.GetZoneAtPosition(posX, posZ);

            if (!_playerZones.TryGetValue(player.PlayerUID, out string currentZone))
            {
                _playerZones[player.PlayerUID] = newZone;
                continue;
            }

            if (currentZone == newZone) continue;

            RegionInfo oldInfo = BuildRegionInfo(posX, posZ, currentZone);
            RegionInfo newInfo = BuildRegionInfo(posX, posZ, newZone);

            _playerZones[player.PlayerUID] = newZone;

            RPGDifficultyModSystem.Logger.LogDebug(
                $"[RegionAPI] {player.PlayerName} entered zone={newZone} level={newInfo.Level} initialRegion={newInfo.IsInitialRegion}"
            );

            OnPlayerEnterRegion?.Invoke(player, oldInfo, newInfo);
        }
    }

    private static RegionInfo BuildRegionInfo(double posX, double posZ, string zone)
    {
        int regionSize = _api.World.BlockAccessor.RegionSize;
        int regionX = (int)(posX / regionSize);
        int regionZ = (int)(posZ / regionSize);

        bool isInitial = false;
        EntityPos spawnPos = _api.World.DefaultSpawnPosition;
        if (spawnPos != null)
        {
            double dx = posX - spawnPos.X;
            double dz = posZ - spawnPos.Z;
            isInitial = Math.Sqrt(dx * dx + dz * dz) <= Configuration.StatusRegion.initialRegionRadius;
        }

        return new RegionInfo
        {
            RegionX = regionX,
            RegionZ = regionZ,
            Zone = zone,
            Level = RegionSystem.GetRegionLevel(posX, posZ, zone),
            IsInitialRegion = isInitial,
        };
    }
}
