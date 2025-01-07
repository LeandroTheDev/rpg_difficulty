using System;
using HarmonyLib;
using LevelUP;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;
using Newtonsoft.Json.Linq;
using Vintagestory.API.Common.Entities;
using Vintagestory.Server;
using System.Collections.Generic;

namespace RPGDifficulty;
class Overwrite
{
    public bool levelUPCompatibility = false;
    public Harmony instance;
    public void OverwriteNativeFunctions(ICoreAPI api)
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
    [HarmonyPrefix]
    [HarmonyPatch(typeof(AiTaskMeleeAttack), "LoadConfig")]
    public static void LoadConfig(AiTaskMeleeAttack __instance, ref JsonObject taskConfig, JsonObject aiConfig)
    {
        if (__instance.entity == null) return;
        if (__instance.entity.Attributes == null) return;
        if (__instance.entity.Attributes.GetBool("RPGDifficultyAlreadyDeployed")) return;
        else __instance.entity.Attributes.SetBool("RPGDifficultyAlreadyDeployed", true);

        // Single player / Lan treatment
        if (__instance.entity.SidedProperties == null) return;

        #region health
        // Changing Health Stats
        EntityBehaviorHealth entityLifeStats = __instance.entity.GetBehavior<EntityBehaviorHealth>();
        float oldBaseMaxHealth = 0f;
        // Check existance, for example buttlerfly doesn't have a life status
        if (entityLifeStats != null)
        {
            oldBaseMaxHealth = entityLifeStats.BaseMaxHealth;
            double healthPercentage = 0;
            healthPercentage += __instance.entity.Attributes.GetDouble("RPGDifficultyHealthStatsIncreaseDistance");
            healthPercentage += __instance.entity.Attributes.GetDouble("RPGDifficultyHealthStatsIncreaseHeight");
            healthPercentage += __instance.entity.Attributes.GetDouble("RPGDifficultyHealthStatsIncreaseAge");

            entityLifeStats.BaseMaxHealth += (int)Math.Round(entityLifeStats.BaseMaxHealth * healthPercentage);
            if (Configuration.enableStatusVariation)
                entityLifeStats.BaseMaxHealth *= (float)__instance.entity.Attributes.GetDouble("RPGDifficultyStatusVariation");
            entityLifeStats.MaxHealth += (int)Math.Round(entityLifeStats.MaxHealth * healthPercentage);
            if (Configuration.enableStatusVariation)
                entityLifeStats.MaxHealth *= (float)__instance.entity.Attributes.GetDouble("RPGDifficultyStatusVariation");
            entityLifeStats.Health += (int)Math.Round(entityLifeStats.Health * healthPercentage);
            if (Configuration.enableStatusVariation)
                entityLifeStats.Health *= (float)__instance.entity.Attributes.GetDouble("RPGDifficultyStatusVariation");
        }
        #endregion

        #region damage
        float damage = taskConfig["damage"].AsFloat();
        int damageTier = taskConfig["damageTier"].AsInt();
        if (damage == 0f) return;

        // Increase the damage
        damage += (float)(damage * __instance.entity.Attributes.GetDouble("RPGDifficultyDamageStatsIncreaseDistance"));
        damage += (float)(damage * __instance.entity.Attributes.GetDouble("RPGDifficultyDamageStatsIncreaseHeight"));
        damage += (float)(damage * __instance.entity.Attributes.GetDouble("RPGDifficultyDamageStatsIncreaseAge"));

        // Variation
        if (Configuration.enableStatusVariation)
            damage *= (float)__instance.entity.Attributes.GetDouble("RPGDifficultyStatusVariation");

        // Increase the damage tier
        if (Configuration.increaseDamageTier)
            for (int i = 0; i < damage; i++)
                if (i % Configuration.damageTierIncreaseEveryDamage == 0)
                    damageTier++;

        string data = taskConfig.Token?.ToString();

        // Parsing the readonly object into editable object
        JObject jsonObject;
        try
        {
            jsonObject = JObject.Parse(data);
        }
        catch (Exception ex)
        {
            if (Configuration.enableExtendedLog)
                Debug.Log($"Invalid json for entity: {__instance.entity.Code}, exception: {ex.Message}");
            return;
        }

        // Checking if damage exist
        if (jsonObject.TryGetValue("damage", out JToken _))
        {
            // Redefining the damage
            jsonObject["damage"] = damage;
            jsonObject["damageTier"] = damageTier;
        }

        // Updating the json
        taskConfig = new(JToken.Parse(jsonObject.ToString()));
        #endregion

        #region rpgoverlay
        if (Configuration.rpgOverlayOverwriteLevel)
        {
            int additionalLevels = 0;
            bool reducedAdditionalLevels = false;
            double healthDifference = entityLifeStats.BaseMaxHealth - oldBaseMaxHealth;
            // Check if the health is less
            if (healthDifference < 0)
            {
                healthDifference = Math.Abs(healthDifference);
                reducedAdditionalLevels = true;
            }

            for (int i = 0; i < (int)healthDifference; i++)
            {
                // If i is multiply of rpgOverlayIncreaseLevelEveryAdditionalHP
                if (i % Configuration.rpgOverlayIncreaseLevelEveryAdditionalHP == 0)
                {
                    // increasing the level
                    additionalLevels++;
                }
            }

            if (additionalLevels != 0)
            {
                // Transforms the additionaLevels in negative if is to be reduced
                if (reducedAdditionalLevels)
                {
                    additionalLevels = -additionalLevels;
                }

                // Checking if not exist any compatibility yet
                if (__instance.entity.WatchedAttributes.GetInt("RPGOverlayAddOrReduceLevels") == 0)
                {
                    // Simple create new level if not exist
                    __instance.entity.WatchedAttributes.SetInt("RPGOverlayAddOrReduceLevels", additionalLevels);
                }
                else
                {
                    // Some other mod has already created the compatibility, lets get the value
                    int previousAdditionalLevels = __instance.entity.WatchedAttributes.GetInt("RPGOverlayAddOrReduceLevels");
                    // We set now the variable as the: previous level from other mod plus ours new level
                    __instance.entity.WatchedAttributes.SetInt("RPGOverlayAddOrReduceLevels", additionalLevels + previousAdditionalLevels);
                }
            }
        }
        #endregion
    }

    // Overwrite Entity Spawn, why not use server api event?
    // Because I prefer the entity to be removed before it is even present in the world
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ServerMain), "SpawnEntity", [typeof(Entity), typeof(EntityProperties)])]
    public static bool SpawnEntity(Entity entity, EntityProperties type)
    {
        // Checking if the entity already have the calculation
        if (!entity.Attributes.GetBool("RPGDifficultyAlreadySet"))
            Initialization.SetEntityStats(entity);

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
                                if (Configuration.enableExtendedLog)
                                    Debug.Log($"not in minimum distance: {minimumDistanceToSpawn}, actual: {distance}");
                                return false;
                            }
                        if ((long)maximumDistanceToSpawn != -1)
                            if (distance > (long)maximumDistanceToSpawn)
                            {
                                if (Configuration.enableExtendedLog)
                                    Debug.Log($"not in maximum distance: {maximumDistanceToSpawn}, actual: {distance}");
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
                                if (Configuration.enableExtendedLog)
                                    Debug.Log($"not in minimum height: {minimumHeightToSpawn}, actual: {height}");
                                return false;
                            }
                        if ((long)maximumHeightToSpawn != -1)
                            if (height > (long)maximumHeightToSpawn)
                            {
                                if (Configuration.enableExtendedLog)
                                    Debug.Log($"not in maximum height: {maximumHeightToSpawn}, actual: {height}");
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
                                if (Configuration.enableExtendedLog)
                                    Debug.Log($"not in minimum age: {minimumAgeToSpawn}, actual: {age}");
                                return false;
                            }
                        if ((long)maximumAgeToSpawn != -1)
                            if (age > (long)maximumAgeToSpawn)
                            {
                                if (Configuration.enableExtendedLog)
                                    Debug.Log($"not in maximum age: {maximumAgeToSpawn}, actual: {age}");
                                return false;
                            }
                    }
                }
                catch (Exception ex)
                {
                    Debug.Log($"ERROR: Something crashed the spawn condition, probably some mistake in base.json \"entitySpawnConditions\", exception: {ex.Message}");
                }
            }
            else Debug.Log($"ERROR: Spawn condition is not a JObject, it is {conditionsObject.GetType()}");
        }

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

        if (Configuration.enableExtendedLog)
            Debug.Log($"{byPlayer.PlayerName} harvested any entity with knife, multiply drop: {dropRate} base: {Configuration.baseHarvest}");
    }
}