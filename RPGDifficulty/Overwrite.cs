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
    public bool levelUPCompatibility = false;
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
        // Getting the entity that does the damage
        EntityAgent entityDamageSource;
        if (damageSource.GetCauseEntity() is EntityAgent)
            entityDamageSource = damageSource.GetCauseEntity() as EntityAgent;
        else if (damageSource.SourceEntity is EntityAgent)
            entityDamageSource = damageSource.SourceEntity as EntityAgent;
        else return;

        float oldDamage = damage;
        // Increase the damage
        damage += (float)(damage * entityDamageSource.Attributes.GetDouble("RPGDifficultyDamageStatsIncreaseDistance"));
        damage += (float)(damage * entityDamageSource.Attributes.GetDouble("RPGDifficultyDamageStatsIncreaseHeight"));

        // Empty status just continue
        if (damage - oldDamage == 0f) return;

        if (Configuration.enableExtendedLog)
            Debug.Log($"{entityDamageSource.Code} damage increased by {damage - oldDamage}");

        // Check for compatibilities
        if (__instance.Attributes.GetFloat("LevelUP_DamageInteraction_Compatibility_ExtendDamageFinish_ReceiveDamage") == 0.0f)
        {
            // Simple create new stats if not exist
            __instance.Attributes.SetFloat("LevelUP_DamageInteraction_Compatibility_ExtendDamageFinish_ReceiveDamage", damage - oldDamage);
        }
        else
        {
            // Some other mod has already created the compatibility, lets get the value
            float entityAdditionalDamage = __instance.Attributes.GetFloat("LevelUP_DamageInteraction_Compatibility_ExtendDamageFinish_ReceiveDamage");
            // We set now the variable as the: previous additional damage from other mod plus ours new damage
            __instance.Attributes.SetFloat("LevelUP_DamageInteraction_Compatibility_ExtendDamageFinish_ReceiveDamage", entityAdditionalDamage + (damage - oldDamage));
        }
    }

    // Overwrite Knife Harvesting
    [HarmonyPrefix]
    [HarmonyPatch(typeof(EntityBehaviorHarvestable), "SetHarvested")]
    public static void SetHarvestedKnifeStart(EntityBehaviorHarvestable __instance, IPlayer byPlayer, float dropQuantityMultiplier = 1f)
    {
        // Check if player exist and options is enabled
        if (byPlayer != null && Configuration.lootStatsIncreaseEveryDistance == 0 && Configuration.lootStatsIncreaseEveryHeight == 0) return;

        // Get the final droprate
        float dropRateIncrease = (float)(__instance.entity.Attributes.GetDouble("RPGDifficultyLootStatsIncreaseDistance") + __instance.entity.Attributes.GetDouble("RPGDifficultyLootStatsIncreaseHeight"));

        // Checking if not exist any compatibility yet
        if (byPlayer.Entity.Attributes.GetFloat("LevelUP_BlockInteraction_Compatibility_ExtendHarvestDrop_SetHarvestedKnife") == 0f)
        {
            // Simple create new stats if not exist
            byPlayer.Entity.Attributes.SetFloat("LevelUP_BlockInteraction_Compatibility_ExtendHarvestDrop_SetHarvestedKnife", dropRateIncrease);
        }
        else
        {
            // Some other mod has already created the compatibility, lets get the value
            float entityDropRate = byPlayer.Entity.Attributes.GetFloat("LevelUP_BlockInteraction_Compatibility_ExtendHarvestDrop_SetHarvestedKnife");
            // We set now the variable as the: previous additional droprate from other mod plus ours new droprate
            byPlayer.Entity.Attributes.SetFloat("LevelUP_BlockInteraction_Compatibility_ExtendHarvestDrop_SetHarvestedKnife", entityDropRate + dropRateIncrease);
        }

        if (Configuration.enableExtendedLog)
            Debug.Log($"{byPlayer.PlayerName} harvested any entity with knife, increasing multiply by: {dropRateIncrease}");
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
        // Getting the entity that does the damage
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
        damage += (float)(damage * entityDamaged.Attributes.GetDouble("RPGDifficultyDamageStatsIncreaseDistance"));
        damage += (float)(damage * entityDamaged.Attributes.GetDouble("RPGDifficultyDamageStatsIncreaseHeight"));

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
        // Check if player exist and options is enabled
        if (byPlayer != null && Configuration.lootStatsIncreaseEveryDistance == 0 && Configuration.lootStatsIncreaseEveryHeight == 0) return;

        // Get the final droprate
        float dropRate = (float)Configuration.baseHarvest + (float)__instance.entity.Attributes.GetDouble("RPGDifficultyLootStatsIncreaseDistance") + (float)byPlayer.Entity.Attributes.GetDouble("RPGDifficultyLootStatsIncreaseHeight");

        // Increasing entity drop rate
        byPlayer.Entity.Stats.Set("animalLootDropRate", "animalLootDropRate", dropRate);
        if (Configuration.enableExtendedLog)
            Debug.Log($"{byPlayer.PlayerName} harvested any entity with knife, multiply drop: {dropRate} base: {Configuration.baseHarvest}");
    }
    #endregion
}