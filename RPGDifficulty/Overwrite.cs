using System;
using HarmonyLib;
using LevelUP;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace RPGDifficulty;
class Overwrite
{
    static bool levelUPCompatibility = false;
    public Harmony overwriter;
    public void OverwriteNativeFunctions(ICoreAPI api)
    {
        // Level UP Compatibility
        api.ModLoader.Mods.Foreach((mod) => { if (mod.FileName == "LevelUP") levelUPCompatibility = true; });
        if (levelUPCompatibility)
        {
            Debug.Log("Level UP Mod has been detected");
            if (!Harmony.HasAnyPatches("rpgdifficulty_damage_levelup"))
            {
                overwriter = new Harmony("rpgdifficulty_damage_levelup");
                overwriter.PatchCategory("rpgdifficulty_damage_levelup");
                Debug.Log("Damage interaction has been overwrited");
            }
            else
            {
                Debug.Log("Damage interaction overwriter has already patched, probably by the singleplayer server");
            }
        }
        else
        {
            Debug.Log("Level UP Mod not been detected");
            if (!Harmony.HasAnyPatches("rpgdifficulty_damage_standalone"))
            {
                overwriter = new Harmony("rpgdifficulty_damage_standalone");
                overwriter.PatchCategory("rpgdifficulty_damage_standalone");
                Debug.Log("Damage interaction has been overwrited");
            }
            else
            {
                Debug.Log("Damage interaction overwriter has already patched, probably by the singleplayer server");
            }
        }
    }
}

#pragma warning disable IDE0060
[HarmonyPatchCategory("rpgdifficulty_damage_levelup")]
class LevelUPCompatibility
{
    // Overwrite Damage Interaction
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Entity), "ReceiveDamage")]
    public static void ReceiveDamage(Entity __instance, DamageSource damageSource, float damage)
    {
        if (damageSource.SourceEntity == null || damageSource.GetCauseEntity() == null) return;

        EntityAgent entityDamaged;
        if (damageSource.GetCauseEntity() is EntityAgent)
            entityDamaged = damageSource.GetCauseEntity() as EntityAgent;
        else if (damageSource.SourceEntity is EntityAgent)
            entityDamaged = damageSource.SourceEntity as EntityAgent;
        else return;

        // Blacklist Check
        if (Configuration.enableBlacklist)
            if (Configuration.blacklist.TryGetValue(entityDamaged.Code.ToString(), out double _))
            { if (Configuration.enableExtendedLog) Debug.Log($"{entityDamaged.Code} is on blacklist, ignoring damage"); return; }
        // Whitelist Check
        if (Configuration.enableWhitelist)
            // In whitelist
            if (Configuration.whitelist.TryGetValue(entityDamaged.Code.ToString(), out double _))
            { if (Configuration.enableExtendedLog) Debug.Log($"{entityDamaged.Code} is on whitelist, increasing damage"); }
            // Not in whitelist
            else { if (Configuration.enableExtendedLog) Debug.Log($"{entityDamaged.Code} is not on whitelist, ignoring damage"); return; }

        float oldDamage = damage;
        // Increase the damage
        damage += (float)(damage * __instance.Attributes.GetDouble("RPGDifficultyDamageStatsIncreaseDistance"));
        damage += (float)(damage * __instance.Attributes.GetDouble("RPGDifficultyDamageStatsIncreaseHeight"));

        if (Configuration.enableExtendedLog)
            Debug.Log($"{entityDamaged.Code} damage increased by {damage - oldDamage}");

        // Add this damage at final calculation of level up
        if (__instance.Stats["LevelUP_DamageInteraction_Compatibility_ExtendDamageFinish_ReceiveDamage"] == null)
        {
            // Simple create new stats if not exist
            __instance.Stats.Set("LevelUP_DamageInteraction_Compatibility_ExtendDamageFinish_ReceiveDamage", "DamageFinish", (float)(damage * __instance.Attributes.GetDouble("RPGDifficultyDamageStatsIncreaseDistance")), true);
        }
        else
        {
            // Some other mod has already created the a finish calculation variable lets get it
            float entityAdditionalDamage = __instance.Stats.GetBlended("LevelUP_DamageInteraction_Compatibility_ExtendDamageFinish_ReceiveDamage");
            // We set now the variable as the: previous additional damage from other mod plus ours new damage
            __instance.Stats["LevelUP_DamageInteraction_Compatibility_ExtendDamageFinish_ReceiveDamage"].Set("DamageFinish", entityAdditionalDamage + (float)(damage * __instance.Attributes.GetDouble("RPGDifficultyDamageStatsIncreaseDistance")), true);
        }
    }
}

#pragma warning disable IDE0060
[HarmonyPatchCategory("rpgdifficulty_damage_standalone")]
class DamageInteraction
{
    #region damage
    // Overwrite Damage Interaction
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Entity), "ReceiveDamage")]
    public static bool ReceiveDamage(Entity __instance, DamageSource damageSource, float damage)
    {
        if (damageSource.SourceEntity == null || damageSource.GetCauseEntity() == null) return true;

        EntityAgent entityDamaged;
        if (damageSource.GetCauseEntity() is EntityAgent)
            entityDamaged = damageSource.GetCauseEntity() as EntityAgent;
        else if (damageSource.SourceEntity is EntityAgent)
            entityDamaged = damageSource.SourceEntity as EntityAgent;
        else return true;

        // Blacklist Check
        if (Configuration.enableBlacklist)
            if (Configuration.blacklist.TryGetValue(entityDamaged.Code.ToString(), out double _))
            { if (Configuration.enableExtendedLog) Debug.Log($"{entityDamaged.Code} is on blacklist, ignoring damage"); return true; }
        // Whitelist Check
        if (Configuration.enableWhitelist)
            // In whitelist
            if (Configuration.whitelist.TryGetValue(entityDamaged.Code.ToString(), out double _))
            { if (Configuration.enableExtendedLog) Debug.Log($"{entityDamaged.Code} is on whitelist, increasing damage"); }
            // Not in whitelist
            else { if (Configuration.enableExtendedLog) Debug.Log($"{entityDamaged.Code} is not on whitelist, ignoring damage"); return true; }

        float oldDamage = damage;
        // Increase the damage
        damage += (float)(damage * __instance.Attributes.GetDouble("RPGDifficultyDamageStatsIncreaseDistance"));
        damage += (float)(damage * __instance.Attributes.GetDouble("RPGDifficultyDamageStatsIncreaseHeight"));

        if (Configuration.enableExtendedLog)
            Debug.Log($"{entityDamaged.Code} damage increased by {damage - oldDamage}");

        #region native
        if ((!__instance.Alive || __instance.IsActivityRunning("invulnerable")) && damageSource.Type != EnumDamageType.Heal)
        {
            return false;
        }
        if (__instance.ShouldReceiveDamage(damageSource, damage))
        {
            foreach (EntityBehavior behavior in __instance.SidedProperties.Behaviors)
            {
                behavior.OnEntityReceiveDamage(damageSource, ref damage);
            }
            if (damageSource.Type != EnumDamageType.Heal && damage > 0f)
            {
                __instance.WatchedAttributes.SetInt("onHurtCounter", __instance.WatchedAttributes.GetInt("onHurtCounter") + 1);
                __instance.WatchedAttributes.SetFloat("onHurt", damage);
                if (damage > 0.05f)
                {
                    __instance.AnimManager.StartAnimation("hurt");
                }
            }
            if (damageSource.GetSourcePosition() != null)
            {
                Vec3d dir = (__instance.SidedPos.XYZ - damageSource.GetSourcePosition()).Normalize();
                dir.Y = 0.699999988079071;
                float factor = damageSource.KnockbackStrength * GameMath.Clamp((1f - __instance.Properties.KnockbackResistance) / 10f, 0f, 1f);
                __instance.WatchedAttributes.SetFloat("onHurtDir", (float)Math.Atan2(dir.X, dir.Z));
                __instance.WatchedAttributes.SetDouble("kbdirX", dir.X * (double)factor);
                __instance.WatchedAttributes.SetDouble("kbdirY", dir.Y * (double)factor);
                __instance.WatchedAttributes.SetDouble("kbdirZ", dir.Z * (double)factor);
            }
            else
            {
                __instance.WatchedAttributes.SetDouble("kbdirX", 0.0);
                __instance.WatchedAttributes.SetDouble("kbdirY", 0.0);
                __instance.WatchedAttributes.SetDouble("kbdirZ", 0.0);
                __instance.WatchedAttributes.SetFloat("onHurtDir", -999f);
            }
            return damage > 0f;
        }
        return false;
        #endregion end
    }
    #endregion

    #region harvest
    // Overwrite Knife Harvesting
    [HarmonyPrefix]
    [HarmonyPatch(typeof(EntityBehaviorHarvestable), "SetHarvested")]
    public static void SetHarvestedKnifeStart(EntityBehaviorHarvestable __instance, IPlayer byPlayer, float dropQuantityMultiplier = 1f)
    {
        // if (!Configuration.enableLevelKnife) return;

        // // Receive the droprate from other mods
        // float compatibilityDroprate = byPlayer.Entity.Stats.GetBlended("LevelUP_BlockInteraction_Compatibility_ExtendHarvestDrop_SetHarvestedKnife");

        // // Check if is from the server
        // if (byPlayer is IServerPlayer && __instance.entity.World.Side == EnumAppSide.Server)
        // {
        //     IServerPlayer player = byPlayer as IServerPlayer;
        //     // Earny xp by harvesting entity
        //     instance.serverAPI?.OnClientMessage(player, "Knife_Harvest_Entity");

        //     // Store the old drop rate
        //     player.Entity.Stats.Set("old_animalLootDropRate", "old_animalLootDropRate", player.Entity.Stats.GetBlended("animalLootDropRate"));

        //     // Get the final droprate
        //     float dropRate = Configuration.KnifeGetHarvestMultiplyByLevel(player.Entity.WatchedAttributes.GetInt("LevelUP_Level_Knife")) + compatibilityDroprate;

        //     // Increasing entity drop rate
        //     player.Entity.Stats.Set("animalLootDropRate", "animalLootDropRate", dropRate);
        //     if (Configuration.enableExtendedLog)
        //         Debug.Log($"{player.PlayerName} harvested any entity with knife, multiply drop: {dropRate}");
        // }
        // // Single player treatment and lan treatment
        // else if (instance.clientAPI != null && instance.clientAPI.api.IsSinglePlayer)
        // {
        //     instance.clientAPI.channel.SendPacket($"Knife_Harvest_Entity&lanplayername={byPlayer.PlayerName}");

        //     // Store the old drop rate
        //     byPlayer.Entity.Stats.Set("old_animalLootDropRate", "old_animalLootDropRate", byPlayer.Entity.Stats.GetBlended("animalLootDropRate"));

        //     // Get the final droprate
        //     float dropRate = Configuration.KnifeGetHarvestMultiplyByLevel(byPlayer.Entity.WatchedAttributes.GetInt("LevelUP_Level_Knife")) + compatibilityDroprate;

        //     // Increasing entity drop rate
        //     byPlayer.Entity.Stats.Set("animalLootDropRate", "animalLootDropRate", dropRate);
        //     if (Configuration.enableExtendedLog)
        //         Debug.Log($"{byPlayer.PlayerName} harvested any entity with knife, multiply drop: {dropRate}");
        // }
    }
    // Overwrite Knife Harvesting
    [HarmonyPostfix]
    [HarmonyPatch(typeof(EntityBehaviorHarvestable), "SetHarvested")]
    public static void SetHarvestedKnifeFinish(EntityBehaviorHarvestable __instance, IPlayer byPlayer, float dropQuantityMultiplier = 1f)
    {
        // if (!Configuration.enableLevelKnife || byPlayer == null) return;

        // // Check if the old drop rate exist
        // if (byPlayer.Entity.Stats.GetBlended("old_animalLootDropRate") == 0.0f) return;

        // // Check if is from the server
        // if (byPlayer is IServerPlayer && __instance.entity.World.Side == EnumAppSide.Server)
        // {
        //     IServerPlayer player = byPlayer as IServerPlayer;

        //     // Reload old drop rate
        //     player.Entity.Stats.Set("animalLootDropRate", "animalLootDropRate", player.Entity.Stats.GetBlended("old_animalLootDropRate"));
        // }
        // // Singleplayer/Lan compatibility
        // else if (instance.clientAPI != null && instance.clientAPI.api.IsSinglePlayer)
        // {
        //     byPlayer.Entity.Stats.Set("animalLootDropRate", "animalLootDropRate", byPlayer.Entity.Stats.GetBlended("old_animalLootDropRate"));
        // }

        // byPlayer.Entity.Stats.Remove("LevelUP_BlockInteraction_Compatibility_ExtendHarvestDrop_SetHarvestedKnife", "HarvestStart");

    }
    #endregion
}