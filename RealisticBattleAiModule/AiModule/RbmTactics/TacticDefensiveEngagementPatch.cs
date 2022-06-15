﻿using HarmonyLib;
using RealisticBattleAiModule.AiModule.RbmBehaviors;
using TaleWorlds.MountAndBlade;

namespace RealisticBattleAiModule.AiModule.RbmTactics
{
    [HarmonyPatch(typeof(TacticDefensiveEngagement))]
    class TacticDefensiveEngagementPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("HasBattleBeenJoined")]
        static bool PrefixHasBattleBeenJoined(Formation ____mainInfantry, bool ____hasBattleBeenJoined, ref bool __result)
        {
            __result = Utilities.HasBattleBeenJoined(____mainInfantry, ____hasBattleBeenJoined);
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch("Defend")]
        static void PostfixDefend(ref Formation ____archers, ref Formation ____mainInfantry)
        {
            if (____archers != null)
            {
                ____archers.AI.SetBehaviorWeight<BehaviorSkirmish>(0f);
                ____archers.AI.SetBehaviorWeight<BehaviorSkirmishLine>(0f);
                ____archers.AI.SetBehaviorWeight<BehaviorScreenedSkirmish>(1f);
                ____archers.AI.SetBehaviorWeight<BehaviorRegroup>(1.75f);
            }
            if (____mainInfantry != null)
            {
                ____mainInfantry.AI.SetBehaviorWeight<BehaviorRegroup>(1.75f);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("Engage")]
        static void PostfixAttack(ref Formation ____archers, ref Formation ____mainInfantry, ref Formation ____rightCavalry, ref Formation ____leftCavalry)
        {
            if (____archers != null)
            {
                ____archers.AI.ResetBehaviorWeights();
                ____archers.AI.SetBehaviorWeight<RBMBehaviorArcherSkirmish>(1f);
                ____archers.AI.SetBehaviorWeight<BehaviorSkirmishLine>(0f);
                ____archers.AI.SetBehaviorWeight<BehaviorScreenedSkirmish>(0f);
            }
            if (____rightCavalry != null)
            {
                ____rightCavalry.AI.ResetBehaviorWeights();
                ____rightCavalry.AI.SetBehaviorWeight<BehaviorMountedSkirmish>(1f);
                ____rightCavalry.AI.SetBehaviorWeight<BehaviorCharge>(1f);
                //____rightCavalry.AI.SetBehaviorWeight<RBMBehaviorForwardHorseSkirmish>(1f);
                ____rightCavalry.AI.SetBehaviorWeight<BehaviorTacticalCharge>(1f);
                //____rightCavalry.AI.SetBehaviorWeight<BehaviorPullBack>(0f);
            }
            if (____leftCavalry != null)
            {
                ____leftCavalry.AI.ResetBehaviorWeights();
                ____leftCavalry.AI.SetBehaviorWeight<BehaviorMountedSkirmish>(1f);
                ____leftCavalry.AI.SetBehaviorWeight<BehaviorCharge>(1f);
                //____leftCavalry.AI.SetBehaviorWeight<RBMBehaviorForwardHorseSkirmish>(1f);
                ____leftCavalry.AI.SetBehaviorWeight<BehaviorTacticalCharge>(1f);
                //____leftCavalry.AI.SetBehaviorWeight<BehaviorPullBack>(0f);
            }
            Utilities.FixCharge(ref ____mainInfantry);
        }
    }
}
