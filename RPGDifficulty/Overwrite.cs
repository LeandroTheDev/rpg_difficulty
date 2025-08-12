using System;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;
using Newtonsoft.Json.Linq;
using Vintagestory.API.Common.Entities;
using Vintagestory.Server;

namespace RPGDifficulty;

class Overwrite
{
    public Harmony instance;
    public void OverwriteNativeFunctions()
    {
        if (!Harmony.HasAnyPatches("rpgdifficulty"))
        {
            instance = new Harmony("rpgdifficulty");
            instance.PatchCategory("rpgdifficulty");
            Debug.Log("Damage interaction has been overwrited");
        }
        else
        {
            Debug.Log("RPGDifficulty overwriter has already patched, probably by the singleplayer server");
        }
    }
}

#pragma warning disable IDE0060
[HarmonyPatchCategory("rpgdifficulty")]
class DamageInteraction
{
    // Overwrite Damage Interaction
    [HarmonyPatch(typeof(AiTaskMeleeAttack))]
    [HarmonyPatch(MethodType.Constructor)]
    [HarmonyPatch(new Type[] { typeof(EntityAgent), typeof(JsonObject), typeof(JsonObject) })]
    [HarmonyPrefix]
    public static void LoadConfig(AiTaskMeleeAttack __instance, EntityAgent entity, ref JsonObject taskConfig, JsonObject aiConfig)
    {
        if (__instance.entity == null) return;
        if (__instance.entity.Attributes == null) return;
        if (__instance.entity.Attributes.GetBool("RPGDifficultyAlreadyDeployed")) return;
        else __instance.entity.Attributes.SetBool("RPGDifficultyAlreadyDeployed", true);

        // Check if should spawn entity
        if (!Initialization.ShouldEntitySpawn(__instance.entity))
        {
            Debug.LogDebug($"Entity removed by ShouldEntitySpawn: {__instance.entity.GetName()}");

            Initialization.serverAPI?.World.DespawnEntity(__instance.entity, new()
            {
                Reason = EnumDespawnReason.Removed
            });
            return;
        }

        // Checking if the entity already have the calculation
        if (!__instance.entity.Attributes.GetBool("RPGDifficultyAlreadySet"))
            Initialization.SetEntityStats(__instance.entity);

        // Single player / Lan treatment
        if (__instance.entity.SidedProperties == null) return;

        #region health
        // Changing Health Stats
        EntityBehaviorHealth entityLifeStats = __instance.entity.GetBehavior<EntityBehaviorHealth>();
        // Check existance
        if (entityLifeStats != null)
        {
            double healthPercentage = 0;
            healthPercentage += __instance.entity.Attributes.GetDouble("RPGDifficultyHealthStatsIncreaseDistance");
            healthPercentage += __instance.entity.Attributes.GetDouble("RPGDifficultyHealthStatsIncreaseHeight");
            healthPercentage += __instance.entity.Attributes.GetDouble("RPGDifficultyHealthStatsIncreaseAge");

            if (healthPercentage > 0)
            {

                entityLifeStats.BaseMaxHealth += (int)Math.Round(entityLifeStats.BaseMaxHealth * healthPercentage);
                if (Configuration.enableStatusVariation)
                    entityLifeStats.BaseMaxHealth *= (float)__instance.entity.Attributes.GetDouble("RPGDifficultyStatusVariation");
                entityLifeStats.MaxHealth += (int)Math.Round(entityLifeStats.MaxHealth * healthPercentage);
                if (Configuration.enableStatusVariation)
                    entityLifeStats.MaxHealth *= (float)__instance.entity.Attributes.GetDouble("RPGDifficultyStatusVariation");
                entityLifeStats.Health += (int)Math.Round(entityLifeStats.Health * healthPercentage);
                if (Configuration.enableStatusVariation)
                    entityLifeStats.Health *= (float)__instance.entity.Attributes.GetDouble("RPGDifficultyStatusVariation");

                if (entityLifeStats.Health < 1)
                {
                    Debug.LogError("------------------------");
                    Debug.LogError($"ERROR: Entity health calculations goes really wrong: {__instance.entity.GetName()}, ");
                    Debug.LogError($"Distance: {__instance.entity.Attributes.GetDouble("RPGDifficultyHealthStatsIncreaseDistance")}");
                    Debug.LogError($"Height: {__instance.entity.Attributes.GetDouble("RPGDifficultyHealthStatsIncreaseHeight")}");
                    Debug.LogError($"Age: {__instance.entity.Attributes.GetDouble("RPGDifficultyHealthStatsIncreaseAge")}");
                }
            }
        }
        #endregion

        #region damage
        float damage = taskConfig["damage"].AsFloat();
        if (damage == 0f) return;

        // Increase the damage
        damage += (float)(damage * __instance.entity.Attributes.GetDouble("RPGDifficultyDamageStatsIncreaseDistance"));
        damage += (float)(damage * __instance.entity.Attributes.GetDouble("RPGDifficultyDamageStatsIncreaseHeight"));
        damage += (float)(damage * __instance.entity.Attributes.GetDouble("RPGDifficultyDamageStatsIncreaseAge"));

        // Variation
        if (Configuration.enableStatusVariation)
            damage *= (float)__instance.entity.Attributes.GetDouble("RPGDifficultyStatusVariation");

        string data = taskConfig.Token?.ToString();

        // Parsing the readonly object into editable object
        JObject jsonObject;
        try
        {
            jsonObject = JObject.Parse(data);
        }
        catch (Exception ex)
        {
            Debug.LogDebug($"Invalid json for entity: {__instance.entity.Code}, exception: {ex.Message}");
            return;
        }

        // Checking if damage exist
        if (jsonObject.TryGetValue("damage", out JToken _))
            // Redefining the damage
            jsonObject["damage"] = damage;

        // Updating the json
        taskConfig = new(JToken.Parse(jsonObject.ToString()));
        #endregion
    }

    // Overwrite Entity Spawn, why not use server api event?
    // Because I prefer the entity to be removed before it is even present in the world
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ServerMain), "SpawnEntity", [typeof(Entity), typeof(EntityProperties)])]
    public static bool SpawnEntity(Entity entity, EntityProperties type)
    {
        if (!Initialization.ShouldEntitySpawn(entity))
        {
            Debug.LogDebug($"Entity removed by ShouldEntitySpawn: {entity.GetName()}");
            return false;
        }

        // Checking if the entity already have the calculation
        if (!entity.Attributes.GetBool("RPGDifficultyAlreadySet"))
            Initialization.SetEntityStats(entity);

        return true;
    }

    // Overwrite Knife Harvesting
    [HarmonyPrefix]
    [HarmonyPatch(typeof(EntityBehaviorHarvestable), "SetHarvested")]
    public static void SetHarvestedKnifeStart(EntityBehaviorHarvestable __instance, IPlayer byPlayer, ref float dropQuantityMultiplier)
    {
        // Check if player exist and options is enabled
        if (byPlayer != null && Configuration.lootStatsIncreaseEveryDistance == 0 && Configuration.lootStatsIncreaseEveryHeight == 0) return;

        // Get the final droprate
        float dropRate = (float)Configuration.baseHarvest + (float)__instance.entity.Attributes.GetDouble("RPGDifficultyLootStatsIncreaseDistance");
        dropRate += (float)__instance.entity.Attributes.GetDouble("RPGDifficultyLootStatsIncreaseHeight");
        dropRate += (float)__instance.entity.Attributes.GetDouble("RPGDifficultyLootStatsIncreaseAge");

        dropQuantityMultiplier += dropRate;

        if (Configuration.enableStatusVariation)
            dropQuantityMultiplier *= (float)__instance.entity.Attributes.GetDouble("RPGDifficultyStatusVariation");

        Debug.LogDebug($"{byPlayer.PlayerName} harvested any entity with knife, multiply drop: {dropRate} base: {Configuration.baseHarvest}");
    }
}