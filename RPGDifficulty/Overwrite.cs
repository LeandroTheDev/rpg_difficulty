using System;
using HarmonyLib;
using LevelUP;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;
using Newtonsoft.Json.Linq;

namespace RPGDifficulty;
class Overwrite
{
    public bool levelUPCompatibility = false;
    public Harmony overwriter;
    public void OverwriteNativeFunctions(ICoreAPI api)
    {
        if (!Harmony.HasAnyPatches("rpgdifficulty"))
        {
            overwriter = new Harmony("rpgdifficulty");
            overwriter.PatchCategory("rpgdifficulty");
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

        // Checking if the entity already have the calculation
        if (!__instance.entity.Attributes.GetBool("RPGDifficultyAlreadySet"))
            Initialization.IncreaseEntityStats(__instance.entity);

        #region Damage
        float damage = taskConfig["damage"].AsFloat();
        if (damage == 0f) return;

        // Increase the damage
        damage += (float)(damage * __instance.entity.Attributes.GetDouble("RPGDifficultyDamageStatsIncreaseDistance"));
        damage += (float)(damage * __instance.entity.Attributes.GetDouble("RPGDifficultyDamageStatsIncreaseHeight"));
        damage += (float)(damage * __instance.entity.Attributes.GetDouble("RPGDifficultyDamageStatsIncreaseAge"));

        if(Configuration.enableStatusVariation)
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
            if (Configuration.enableExtendedLog)
                Debug.Log($"Invalid json for entity: {__instance.entity.Code}, exception: {ex.Message}");
            return;
        }

        // Checking if damage exist
        if (jsonObject.TryGetValue("damage", out JToken _))
        {
            // Redefining the damage
            jsonObject["damage"] = damage;
        }

        // Updating the json
        taskConfig = new(JToken.Parse(jsonObject.ToString()));
        #endregion
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

        if(Configuration.enableStatusVariation)
            dropQuantityMultiplier *= (float)__instance.entity.Attributes.GetDouble("RPGDifficultyStatusVariation");

        if (Configuration.enableExtendedLog)
            Debug.Log($"{byPlayer.PlayerName} harvested any entity with knife, multiply drop: {dropRate} base: {Configuration.baseHarvest}");
    }

}