using System;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;
using Newtonsoft.Json.Linq;
using Vintagestory.API.Common.Entities;
using Vintagestory.Server;
using System.Reflection;
using System.Threading.Tasks;

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
    [HarmonyPatch(typeof(AiTaskMeleeAttack), MethodType.Constructor, [typeof(EntityAgent), typeof(JsonObject), typeof(JsonObject)])]
    [HarmonyPostfix]
    [HarmonyPriority(Priority.VeryHigh)]
    public static void LoadConfig(AiTaskMeleeAttack __instance, EntityAgent entity, JsonObject taskConfig, JsonObject aiConfig)
    {
        if (!entity.Alive) return;

        // Check if should spawn entity
        if (!Initialization.ShouldEntitySpawn(entity))
        {
            Debug.LogDebug($"Entity removed by ShouldEntitySpawn: {entity.GetName()}");

            Initialization.serverAPI?.World.DespawnEntity(entity, new()
            {
                Reason = EnumDespawnReason.Removed
            });
            return;
        }

        // Checking if the entity already have the calculation
        if (!entity.Attributes.GetBool("RPGDifficultyAlreadySet"))
        {
            Debug.LogDebug($"Calculating entity status: {entity.Code}");
            Initialization.SetEntityStats(entity);
        }

        // Single player / Lan treatment
        if (entity.SidedProperties == null) return;

        #region health
        if (!entity.Attributes.GetBool("RPGDifficultyHealthAlreadySet"))
        {
            EntityBehaviorHealth entityLifeStats = entity.GetBehavior<EntityBehaviorHealth>();

            void updateEntityHealth()
            {
                double healthPercentage = 0;
                healthPercentage += entity.Attributes.GetDouble("RPGDifficultyHealthStatsIncreaseDistance");
                healthPercentage += entity.Attributes.GetDouble("RPGDifficultyHealthStatsIncreaseHeight");
                healthPercentage += entity.Attributes.GetDouble("RPGDifficultyHealthStatsIncreaseAge");
                if (healthPercentage > 0)
                {

                    float oldBaseMaxHealth = entityLifeStats.BaseMaxHealth;
                    float oldMaxHealth = entityLifeStats.MaxHealth;
                    float oldHealth = entityLifeStats.Health;

                    entityLifeStats.BaseMaxHealth += (int)Math.Round(entityLifeStats.BaseMaxHealth * healthPercentage);
                    if (Configuration.enableStatusVariation)
                        entityLifeStats.BaseMaxHealth *= (float)entity.Attributes.GetDouble("RPGDifficultyStatusVariation");
                    entityLifeStats.MaxHealth += (int)Math.Round(entityLifeStats.MaxHealth * healthPercentage);
                    if (Configuration.enableStatusVariation)
                        entityLifeStats.MaxHealth *= (float)entity.Attributes.GetDouble("RPGDifficultyStatusVariation");
                    entityLifeStats.Health += (int)Math.Round(entityLifeStats.Health * healthPercentage);
                    if (Configuration.enableStatusVariation)
                        entityLifeStats.Health *= (float)entity.Attributes.GetDouble("RPGDifficultyStatusVariation");

                    if (entityLifeStats.Health < 1)
                    {
                        Debug.LogError("------------------------");
                        Debug.LogError($"ERROR: Entity health calculations goes really wrong: {entity.GetName()}, ");
                        Debug.LogError($"Distance: {entity.Attributes.GetDouble("RPGDifficultyHealthStatsIncreaseDistance")}");
                        Debug.LogError($"Height: {entity.Attributes.GetDouble("RPGDifficultyHealthStatsIncreaseHeight")}");
                        Debug.LogError($"Age: {entity.Attributes.GetDouble("RPGDifficultyHealthStatsIncreaseAge")}");
                        Debug.LogError($"Health Percentage: {healthPercentage}");
                        Debug.LogError($"Base Max Health: {entityLifeStats.BaseMaxHealth}");
                        Debug.LogError($"Max Health: {entityLifeStats.MaxHealth}");
                        Debug.LogError($"Health: {entityLifeStats.Health}");
                        Debug.LogError($"Old Base Max Health: {oldBaseMaxHealth}");
                        Debug.LogError($"Old Max Health: {oldMaxHealth}");
                        Debug.LogError($"Old Health: {oldHealth}");
                    }
                    else
                    {
                        Debug.LogDebug($"[LoadConfig] {entity.Code} health updated to: {entityLifeStats.MaxHealth}");
                        // Health status can only be set once, otherwise will be updated every world start or entity reload
                        entity.Attributes.SetBool("RPGDifficultyHealthAlreadySet", true);
                    }
                }
            }

            // Check existance
            if (entityLifeStats != null)
            {
                if (
                    entityLifeStats.BaseMaxHealth > 0f &&
                    entityLifeStats.MaxHealth > 0f &&
                    entityLifeStats.Health > 0f
                )
                {
                    updateEntityHealth();
                }
                else
                {
                    // Entity health is not set yet for some reason, we wait it...
                    Task.Run(async () =>
                    {
                        // Changing Health Stats
                        EntityBehaviorHealth entityLifeStats = entity.GetBehavior<EntityBehaviorHealth>();

                        for (int i = 0; i < 5; i++)
                        {
                            await Task.Delay(500);

                            if (
                                entityLifeStats.BaseMaxHealth > 0f &&
                                entityLifeStats.MaxHealth > 0f &&
                                entityLifeStats.Health > 0f
                            )
                            {
                                updateEntityHealth();
                                break;
                            }

                            if (i == 4)
                            {
                                Debug.LogError($"Could not setup entity health after 5 tries: {entity.GetName()}");
                            }
                        }
                    });
                }
            }
        }
        #endregion

        #region damage
        float damage = taskConfig["damage"].AsFloat(2f);
        if (damage >= 0f)
        {

            // Increase the damage
            damage += (float)(damage * entity.Attributes.GetDouble("RPGDifficultyDamageStatsIncreaseDistance"));
            damage += (float)(damage * entity.Attributes.GetDouble("RPGDifficultyDamageStatsIncreaseHeight"));
            damage += (float)(damage * entity.Attributes.GetDouble("RPGDifficultyDamageStatsIncreaseAge"));

            // Variation
            if (Configuration.enableStatusVariation)
                damage *= (float)entity.Attributes.GetDouble("RPGDifficultyStatusVariation");

            FieldInfo protectedDamage = AccessTools.Field(typeof(AiTaskMeleeAttack), "damage");
            protectedDamage.SetValue(__instance, damage);

            Debug.LogDebug($"[LoadConfig] Entity damage updated to: {protectedDamage.GetValue(__instance)}");
        }
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