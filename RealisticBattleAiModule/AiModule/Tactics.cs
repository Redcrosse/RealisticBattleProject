﻿using HarmonyLib;
using SandBox.Missions.MissionLogics;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.GauntletUI.Mission.Singleplayer;
using TaleWorlds.MountAndBlade.ViewModelCollection.HUD.FormationMarker;
using static TaleWorlds.Core.ItemObject;

namespace RBMAI
{
    public static class Tactics
    {
        public class AgentDamageDone
        {
            public float damageDone = 0f;
            public FormationClass initialClass = FormationClass.Unset;
            public bool isAttacker = false;
        }

        public static Dictionary<Agent, AgentDamageDone> agentDamage = new Dictionary<Agent, AgentDamageDone>();

        [HarmonyPatch(typeof(Mission))]
        [HarmonyPatch("OnAgentHit")]
        private class CustomBattleAgentLogicOnAgentHitPatch
        {
            private static void Postfix(Agent affectedAgent, Agent affectorAgent, in Blow b, in AttackCollisionData collisionData, bool isBlocked, float damagedHp)
            {
                if (affectedAgent != null && affectorAgent != null && affectedAgent.IsActive() && affectedAgent.IsHuman && affectorAgent.IsHuman && !collisionData.AttackBlockedWithShield)
                {
                    if (!affectorAgent.IsHuman && affectorAgent.RiderAgent != null)
                    {
                        affectorAgent = affectorAgent.RiderAgent;
                    }
                    if (affectorAgent != null && affectorAgent.Team != null)
                    {
                        AgentDamageDone damageDone;
                        if (agentDamage.TryGetValue(affectorAgent, out damageDone))
                        {
                            agentDamage[affectorAgent].damageDone += damagedHp;
                        }
                        else
                        {
                            AgentDamageDone add = new AgentDamageDone();
                            if (affectorAgent.IsRangedCached && !affectorAgent.HasMount)
                            {
                                add.initialClass = FormationClass.Ranged;
                            }
                            else if (affectorAgent.IsRangedCached && affectorAgent.HasMount)
                            {
                                add.initialClass = FormationClass.HorseArcher;
                            }
                            else if (!affectorAgent.IsRangedCached && affectorAgent.HasMount)
                            {
                                add.initialClass = FormationClass.Cavalry;
                            }
                            else if (!affectorAgent.IsRangedCached && !affectorAgent.HasMount && affectorAgent.IsHuman)
                            {
                                add.initialClass = FormationClass.Infantry;
                            }
                            add.isAttacker = affectorAgent.Team.IsAttacker;
                            add.damageDone = damagedHp;
                            agentDamage[affectorAgent] = add;
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Team))]
        [HarmonyPatch("Tick")]
        private class OverrideTick
        {
            private static int i = 0;

            private static void Postfix(Team __instance)
            {
                //if (__instance.Banner != null)
                //{
                //    if (i == 300)
                //    {
                //        if (__instance.IsAttacker)
                //        {
                //            float archersDamageDone = 0;
                //            float haDamageDone = 0;
                //            float cavDamageDone = 0;
                //            float infDamageDone = 0;
                //            foreach (KeyValuePair<Agent, AgentDamageDone> entry in agentDamage)
                //            {
                //                if (entry.Value.isAttacker)
                //                {
                //                    if (entry.Value.initialClass == FormationClass.Ranged)
                //                    {
                //                        archersDamageDone += entry.Value.damageDone;
                //                    }
                //                    if (entry.Value.initialClass == FormationClass.HorseArcher)
                //                    {
                //                        haDamageDone += entry.Value.damageDone;
                //                    }
                //                    if (entry.Value.initialClass == FormationClass.Cavalry)
                //                    {
                //                        cavDamageDone += entry.Value.damageDone;
                //                    }
                //                    if (entry.Value.initialClass == FormationClass.Infantry)
                //                    {
                //                        infDamageDone += entry.Value.damageDone;
                //                    }
                //                }
                //            }
                //            InformationManager.DisplayMessage(new InformationMessage("ATK ARC:" + archersDamageDone));
                //            InformationManager.DisplayMessage(new InformationMessage("ATK HA :" + haDamageDone));
                //            InformationManager.DisplayMessage(new InformationMessage("ATK CAV:" + cavDamageDone));
                //            InformationManager.DisplayMessage(new InformationMessage("ATK INF:" + infDamageDone));
                //        }
                //        else
                //        {
                //            float archersDamageDone = 0;
                //            float haDamageDone = 0;
                //            float cavDamageDone = 0;
                //            float infDamageDone = 0;
                //            foreach (KeyValuePair<Agent, AgentDamageDone> entry in agentDamage)
                //            {
                //                if (!entry.Value.isAttacker)
                //                {
                //                    if (entry.Value.initialClass == FormationClass.Ranged)
                //                    {
                //                        archersDamageDone += entry.Value.damageDone;
                //                    }
                //                    if (entry.Value.initialClass == FormationClass.HorseArcher)
                //                    {
                //                        haDamageDone += entry.Value.damageDone;
                //                    }
                //                    if (entry.Value.initialClass == FormationClass.Cavalry)
                //                    {
                //                        cavDamageDone += entry.Value.damageDone;
                //                    }
                //                    if (entry.Value.initialClass == FormationClass.Infantry)
                //                    {
                //                        infDamageDone += entry.Value.damageDone;
                //                    }
                //                }
                //            }
                //            InformationManager.DisplayMessage(new InformationMessage("DEF ARC:" + archersDamageDone));
                //            InformationManager.DisplayMessage(new InformationMessage("DEF HA :" + haDamageDone));
                //            InformationManager.DisplayMessage(new InformationMessage("DEF CAV:" + cavDamageDone));
                //            InformationManager.DisplayMessage(new InformationMessage("DEF INF:" + infDamageDone));
                //        }
                //        i = 0;
                //    }
                //    else
                //    {
                //        i++;
                //    }
                //}
                //    if (__instance.IsAttacker)
                //    {
                //        FieldInfo _currentTacticField = typeof(TeamAIComponent).GetField("_currentTactic", BindingFlags.NonPublic | BindingFlags.Instance);
                //        _currentTacticField.DeclaringType.GetField("_currentTactic");
                //        TacticComponent _currentTactic = (TacticComponent)_currentTacticField.GetValue(__instance.TeamAI);

                //        //InformationManager.DisplayMessage(new InformationMessage("Attacker " + __instance.TeamIndex + " : " + _currentTactic));

                //        FieldInfo _availableTacticsField = typeof(TeamAIComponent).GetField("_availableTactics", BindingFlags.NonPublic | BindingFlags.Instance);
                //        _availableTacticsField.DeclaringType.GetField("_availableTactics");
                //        List<TacticComponent> _availableTactics = (List<TacticComponent>)_availableTacticsField.GetValue(__instance.TeamAI);
                //        foreach (TacticComponent tc in _availableTactics)
                //        {
                //            MethodInfo method = typeof(TacticComponent).GetMethod("GetTacticWeight", BindingFlags.NonPublic | BindingFlags.Instance);
                //            method.DeclaringType.GetMethod("GetTacticWeight");
                //            float weight = (float)method.Invoke(tc, new object[] { });
                //            //InformationManager.DisplayMessage(new InformationMessage(tc + ": " + weight));
                //        }

                //        foreach (Formation formation in __instance.Formations)
                //        {
                //            if (formation.QuerySystem.IsMeleeFormation)
                //            {
                //                //InformationManager.DisplayMessage(new InformationMessage("infantry: " + formation.AI.ActiveBehavior));
                //            }
                //            else if (formation.QuerySystem.IsCavalryFormation)
                //            {
                //                //InformationManager.DisplayMessage(new InformationMessage("cavalry: " + formation.AI.ActiveBehavior));
                //            }
                //            else if (formation.QuerySystem.IsRangedCavalryFormation)
                //            {
                //                InformationManager.DisplayMessage(new InformationMessage("HA:" + formation.AI.ActiveBehavior));
                //            }
                //            else if (formation.QuerySystem.IsRangedFormation)
                //            {
                //                //InformationManager.DisplayMessage(new InformationMessage("ranged: " + formation.AI.ActiveBehavior + " " + formation.QuerySystem.MissileRange));
                //            }
                //        }
                //    }
                //    else
                //    {
                //        FieldInfo _currentTacticField = typeof(TeamAIComponent).GetField("_currentTactic", BindingFlags.NonPublic | BindingFlags.Instance);
                //        _currentTacticField.DeclaringType.GetField("_currentTactic");
                //        TacticComponent _currentTactic = (TacticComponent)_currentTacticField.GetValue(__instance.TeamAI);

                //        //InformationManager.DisplayMessage(new InformationMessage("Defender " + __instance.TeamIndex + " : " + _currentTactic));

                //        FieldInfo _availableTacticsField = typeof(TeamAIComponent).GetField("_availableTactics", BindingFlags.NonPublic | BindingFlags.Instance);
                //        _availableTacticsField.DeclaringType.GetField("_availableTactics");
                //        List<TacticComponent> _availableTactics = (List<TacticComponent>)_availableTacticsField.GetValue(__instance.TeamAI);
                //        foreach (TacticComponent tc in _availableTactics)
                //        {
                //            MethodInfo method = typeof(TacticComponent).GetMethod("GetTacticWeight", BindingFlags.NonPublic | BindingFlags.Instance);
                //            method.DeclaringType.GetMethod("GetTacticWeight");
                //            float weight = (float)method.Invoke(tc, new object[] { });
                //            //InformationManager.DisplayMessage(new InformationMessage(tc + ": " + weight));
                //        }

                //        foreach (Formation formation in __instance.Formations)
                //        {
                //            if (formation.QuerySystem.IsMeleeFormation)
                //            {
                //                //InformationManager.DisplayMessage(new InformationMessage("infantry: " + formation.AI.ActiveBehavior));
                //            }
                //            else if (formation.QuerySystem.IsCavalryFormation)
                //            {
                //                //InformationManager.DisplayMessage(new InformationMessage("cavalry: " + formation.AI.ActiveBehavior));
                //            }
                //            else if (formation.QuerySystem.IsRangedCavalryFormation)
                //            {
                //                InformationManager.DisplayMessage(new InformationMessage("HA:" + formation.AI.ActiveBehavior));
                //            }
                //            else if (formation.QuerySystem.IsRangedFormation)
                //            {
                //                //InformationManager.DisplayMessage(new InformationMessage("ranged: " + formation.AI.ActiveBehavior + " " + formation.QuerySystem.MissileRange));
                //            }
                //        }
                //    }
                //    i = 0;
                //}
                //else
                //{
                //    i++;
                //}
            }
        }

        [HarmonyPatch(typeof(TeamAIGeneral))]
        private class OverrideTeamAIGeneral
        {
            [HarmonyPostfix]
            [HarmonyPatch("OnUnitAddedToFormationForTheFirstTime")]
            private static void PostfixOnUnitAddedToFormationForTheFirstTime(Formation formation)
            {
                formation.QuerySystem.Expire();
                formation.AI.AddAiBehavior(new RBMBehaviorArcherSkirmish(formation));
                formation.AI.AddAiBehavior(new RBMBehaviorForwardSkirmish(formation));
                formation.AI.AddAiBehavior(new RBMBehaviorInfantryAttackFlank(formation));
                formation.AI.AddAiBehavior(new RBMBehaviorCavalryCharge(formation));
                formation.AI.AddAiBehavior(new RBMBehaviorEmbolon(formation));
                formation.AI.AddAiBehavior(new RBMBehaviorArcherFlank(formation));
                formation.AI.AddAiBehavior(new RBMBehaviorHorseArcherSkirmish(formation));
            }
        }

        [HarmonyPatch(typeof(MissionFormationMarkerTargetVM))]
        [HarmonyPatch("Refresh")]
        private class OverrideRefresh
        {
            private static string chooseIcon(Formation formation)
            {
                if (formation != null)
                {
                    if (formation.QuerySystem.IsInfantryFormation)
                    {
                        return TargetIconType.Special_Swordsman.ToString();
                    }
                    if (formation.QuerySystem.IsRangedFormation)
                    {
                        return TargetIconType.Archer_Heavy.ToString();
                    }
                    if (formation.QuerySystem.IsRangedCavalryFormation)
                    {
                        return TargetIconType.HorseArcher_Light.ToString();
                    }
                    if (formation.QuerySystem.IsCavalryFormation && !RBMAI.Utilities.CheckIfMountedSkirmishFormation(formation, 0.6f))
                    {
                        return TargetIconType.Cavalry_Light.ToString();
                    }
                    if (formation.QuerySystem.IsCavalryFormation && RBMAI.Utilities.CheckIfMountedSkirmishFormation(formation, 0.6f))
                    {
                        return TargetIconType.Special_JavelinThrower.ToString();
                    }
                }
                return TargetIconType.None.ToString();
            }

            private static void Postfix(MissionFormationMarkerTargetVM __instance)
            {
                __instance.FormationType = chooseIcon(__instance.Formation);
            }
        }

        [HarmonyPatch(typeof(CampaignMissionComponent))]
        [HarmonyPatch("EarlyStart")]
        public class CampaignMissionComponentPatch
        {
            public static void Postfix()
            {
                AgentPostures.values.Clear();
                PostureLogic.agentsToDropShield.Clear();
                PostureLogic.agentsToDropWeapon.Clear();
                PostureLogic.agentsToChangeFormation.Clear();
                RBMAiPatcher.DoPatching();
                agentDamage.Clear();
            }
        }

        private static float originalDefenderPower = 0f;
        private static float originalAttackerPower = 0f;

        [HarmonyPatch(typeof(MissionCombatantsLogic))]
        [HarmonyPatch("EarlyStart")]
        public class EarlyStartPatch
        {
            public static void Postfix(ref IBattleCombatant ____attackerLeaderBattleCombatant, ref IBattleCombatant ____defenderLeaderBattleCombatant)
            {
                Frontline.aiDecisionCooldownDict.Clear();
                agentDamage.Clear();
                RBMAiPatcher.DoPatching();
                AgentAi.OnTickAsAIPatch.itemPickupDistanceStorage.Clear();
                PostureLogic.agentsToChangeFormation.Clear();
                PostureLogic.agentsToDropWeapon.Clear();
                PostureLogic.agentsToDropShield.Clear();
                AgentPostures.values.Clear();
                originalDefenderPower = 0f;
                originalAttackerPower = 0f;
                if (Mission.Current.Teams.Any())
                {
                    if (Mission.Current.MissionTeamAIType == Mission.MissionTeamAITypeEnum.FieldBattle)
                    {
                        foreach (Team team in Mission.Current.Teams.Where((Team t) => t.HasTeamAi).ToList())
                        {
                            
                            if (team.Side == BattleSideEnum.Attacker)
                            {
                                team.ClearTacticOptions();
                                if (____attackerLeaderBattleCombatant?.BasicCulture?.GetCultureCode() == CultureCode.Empire)
                                {
                                    team.AddTacticOption(new RBMTacticEmbolon(team));
                                }
                                else
                                {
                                    //team.AddTacticOption(new TacticFrontalCavalryCharge(team));
                                }
                                if (____attackerLeaderBattleCombatant?.BasicCulture?.GetCultureCode() == CultureCode.Aserai || ____attackerLeaderBattleCombatant?.BasicCulture?.GetCultureCode() == CultureCode.Darshi)
                                {
                                    team.AddTacticOption(new RBMTacticAttackSplitSkirmishers(team));
                                }
                                if (____attackerLeaderBattleCombatant?.BasicCulture?.GetCultureCode() == CultureCode.Sturgia || ____attackerLeaderBattleCombatant?.BasicCulture?.GetCultureCode() == CultureCode.Nord)
                                {
                                    team.AddTacticOption(new RBMTacticAttackSplitInfantry(team));
                                }
                                if (____attackerLeaderBattleCombatant?.BasicCulture?.GetCultureCode() == CultureCode.Battania)
                                {
                                    team.AddTacticOption(new RBMTacticAttackSplitArchers(team));
                                }
                                team.AddTacticOption(new TacticFullScaleAttack(team));
                                team.AddTacticOption(new TacticCoordinatedRetreat(team));
                            }
                            if (team.Side == BattleSideEnum.Defender)
                            {
                                team.ClearTacticOptions();
                                if (____defenderLeaderBattleCombatant?.BasicCulture?.GetCultureCode() == CultureCode.Battania)
                                {
                                    team.AddTacticOption(new RBMTacticDefendSplitArchers(team));
                                }
                                team.AddTacticOption(new TacticDefensiveEngagement(team));
                                team.AddTacticOption(new TacticDefensiveLine(team));
                                if (____defenderLeaderBattleCombatant?.BasicCulture?.GetCultureCode() == CultureCode.Sturgia)
                                {
                                    team.AddTacticOption(new RBMTacticDefendSplitInfantry(team));
                                }
                                team.AddTacticOption(new TacticFullScaleAttack(team));
                                team.AddTacticOption(new TacticCoordinatedRetreat(team));
                            }
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(TacticCoordinatedRetreat))]
        private class OverrideTacticCoordinatedRetreat
        {
            [HarmonyPrefix]
            [HarmonyPatch("GetTacticWeight")]
            private static bool PrefixGetTacticWeight(ref TacticCoordinatedRetreat __instance, ref float __result)
            {
                float defenderPower = 0f;
                float attackerPower = 0f;

                if (Mission.Current != null)
                {
                    if(originalDefenderPower == 0f)
                    {
                        originalDefenderPower = Mission.Current.Teams.Where(team => team.IsDefender).Sum(team =>
                        {
                            float power = team.QuerySystem.BattlePowerLogic.GetTotalTeamPower(team);
                            return power;
                        });
                    }

                    if (originalAttackerPower == 0f)
                    {
                        originalAttackerPower = Mission.Current.Teams.Where(team => team.IsAttacker).Sum(team =>
                        {
                            float power = team.QuerySystem.BattlePowerLogic.GetTotalTeamPower(team);
                            return power;
                        });
                    }
                    
                    defenderPower = Mission.Current.Teams.Where(team => team.IsDefender).Sum(team =>
                    {
                        float power = 0f;
                        foreach (Agent agent in team.ActiveAgents)
                        {
                            power += agent.CharacterPowerCached;
                        }
                        return power;
                    });
                    attackerPower = Mission.Current.Teams.Where(team => team.IsAttacker).Sum(team =>
                    {
                        float power = 0f;
                        foreach (Agent agent in team.ActiveAgents)
                        {
                            power += agent.CharacterPowerCached;
                        }
                        return power;
                    });

                    if (originalDefenderPower < defenderPower)
                    {
                        originalDefenderPower = defenderPower;
                    }
                    if (originalAttackerPower < attackerPower)
                    {
                        originalAttackerPower = attackerPower;
                    }
                }
                if (__instance.Team.IsDefender)
                {
                    if (__instance.Team.IsDefender && originalDefenderPower > 0f && 
                        (defenderPower / originalDefenderPower <= 0.15f) && 
                        __instance.Team.QuerySystem.RemainingPowerRatio < 0.5f)
                    {
                        foreach (Formation formation in __instance.Team.FormationsIncludingEmpty.Where((Formation f) => f.CountOfUnits > 0).ToList())
                        {
                            formation.AI.ResetBehaviorWeights();
                            formation.AI.SetBehaviorWeight<BehaviorRetreat>(100f);
                        }
                        __result = 1000f;
                        return false;
                    }
                }
                else
                {
                    if (__instance.Team.IsAttacker && originalAttackerPower > 0f && 
                        (attackerPower / originalAttackerPower <= 0.15f) && 
                        __instance.Team.QuerySystem.RemainingPowerRatio < 0.5f)
                    {
                        foreach (Formation formation in __instance.Team.FormationsIncludingEmpty.Where((Formation f) => f.CountOfUnits > 0).ToList())
                        {
                            formation.AI.ResetBehaviorWeights();
                            formation.AI.SetBehaviorWeight<BehaviorRetreat>(100f);
                        }
                        __result = 1000f;
                        return false;
                    }
                }
                __result = 0f;
                return false;
            }
        }

        [HarmonyPatch(typeof(TacticFrontalCavalryCharge))]
        private class OverrideTacticFrontalCavalryCharge
        {
            [HarmonyPostfix]
            [HarmonyPatch("Advance")]
            private static void PostfixAdvance(ref Formation ____cavalry)
            {
                if (____cavalry != null)
                {
                    ____cavalry.AI.SetBehaviorWeight<BehaviorVanguard>(1.5f);
                    ____cavalry.AI.SetBehaviorWeight<RBMBehaviorForwardSkirmish>(1f);
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch("Attack")]
            private static void PostfixAttack(ref Formation ____mainInfantry, ref Formation ____cavalry, ref Formation ____archers)
            {
                if (____cavalry != null)
                {
                    ____cavalry.AI.ResetBehaviorWeights();
                    ____cavalry.AI.SetBehaviorWeight<BehaviorMountedSkirmish>(1f);
                    ____cavalry.AI.SetBehaviorWeight<RBMBehaviorCavalryCharge>(1f);
                    ____cavalry.AI.SetBehaviorWeight<RBMBehaviorForwardSkirmish>(1f);
                }
                if (____archers != null)
                {
                    ____archers.AI.ResetBehaviorWeights();
                    ____archers.AI.SetBehaviorWeight<RBMBehaviorArcherSkirmish>(1f);
                    ____archers.AI.SetBehaviorWeight<BehaviorSkirmishLine>(0f);
                    ____archers.AI.SetBehaviorWeight<BehaviorScreenedSkirmish>(0f);
                }
                RBMAI.Utilities.FixCharge(ref ____mainInfantry);
            }

            [HarmonyPostfix]
            [HarmonyPatch("HasBattleBeenJoined")]
            private static void PostfixHasBattleBeenJoined(Formation ____cavalry, bool ____hasBattleBeenJoined, ref bool __result)
            {
                __result = RBMAI.Utilities.HasBattleBeenJoined(____cavalry, ____hasBattleBeenJoined, 125f);
            }

            [HarmonyPostfix]
            [HarmonyPatch("GetTacticWeight")]
            private static void PostfixGetTacticWeight(ref float __result)
            {
                __result *= 0.75f;
            }
        }

        [HarmonyPatch(typeof(TacticRangedHarrassmentOffensive))]
        private class OverrideTacticRangedHarrassmentOffensive
        {
            [HarmonyPostfix]
            [HarmonyPatch("Advance")]
            private static void PostfixAdvance(ref Formation ____archers, ref Formation ____mainInfantry, ref Formation ____rightCavalry, ref Formation ____leftCavalry, ref Formation ____rangedCavalry)
            {
                if (____rightCavalry != null)
                {
                    ____rightCavalry.AI.ResetBehaviorWeights();
                    ____rightCavalry.AI.SetBehaviorWeight<BehaviorProtectFlank>(1f).FlankSide = FormationAI.BehaviorSide.Right;
                }
                if (____leftCavalry != null)
                {
                    ____leftCavalry.AI.ResetBehaviorWeights();
                    ____leftCavalry.AI.SetBehaviorWeight<BehaviorProtectFlank>(1f).FlankSide = FormationAI.BehaviorSide.Left;
                }
                if (____rangedCavalry != null)
                {
                    ____rangedCavalry.AI.ResetBehaviorWeights();
                    TacticRangedHarrassmentOffensive.SetDefaultBehaviorWeights(____rangedCavalry);
                    ____rangedCavalry.AI.SetBehaviorWeight<BehaviorScreenedSkirmish>(1f);
                    ____rangedCavalry.AI.SetBehaviorWeight<BehaviorMountedSkirmish>(1f);
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch("Attack")]
            private static void PostfixAttack(ref Formation ____archers, ref Formation ____mainInfantry, ref Formation ____rightCavalry, ref Formation ____leftCavalry, ref Formation ____rangedCavalry)
            {
                if (____rightCavalry != null)
                {
                    ____rightCavalry.AI.ResetBehaviorWeights();
                    TacticComponent.SetDefaultBehaviorWeights(____rightCavalry);
                    ____rightCavalry.AI.SetBehaviorWeight<BehaviorMountedSkirmish>(1f);
                    ____rightCavalry.AI.SetBehaviorWeight<RBMBehaviorCavalryCharge>(1f);
                }
                if (____leftCavalry != null)
                {
                    ____leftCavalry.AI.ResetBehaviorWeights();
                    TacticComponent.SetDefaultBehaviorWeights(____leftCavalry);
                    ____leftCavalry.AI.SetBehaviorWeight<BehaviorMountedSkirmish>(1f);
                    ____leftCavalry.AI.SetBehaviorWeight<RBMBehaviorCavalryCharge>(1f);
                }
                if (____rangedCavalry != null)
                {
                    ____rangedCavalry.AI.ResetBehaviorWeights();
                    TacticRangedHarrassmentOffensive.SetDefaultBehaviorWeights(____rangedCavalry);
                    ____rangedCavalry.AI.SetBehaviorWeight<BehaviorMountedSkirmish>(1f);
                }
                if (____archers != null)
                {
                    ____archers.AI.ResetBehaviorWeights();
                    ____archers.AI.SetBehaviorWeight<RBMBehaviorArcherSkirmish>(1f);
                    ____archers.AI.SetBehaviorWeight<BehaviorSkirmishLine>(0f);
                    ____archers.AI.SetBehaviorWeight<BehaviorScreenedSkirmish>(0f);
                }
                RBMAI.Utilities.FixCharge(ref ____mainInfantry);
            }

            [HarmonyPostfix]
            [HarmonyPatch("HasBattleBeenJoined")]
            private static void PostfixHasBattleBeenJoined(Formation ____mainInfantry, bool ____hasBattleBeenJoined, ref bool __result)
            {
                __result = RBMAI.Utilities.HasBattleBeenJoined(____mainInfantry, ____hasBattleBeenJoined);
            }

            [HarmonyPostfix]
            [HarmonyPatch("ManageFormationCounts")]
            private static void PostfixManageFormationCounts(ref Formation ____leftCavalry, ref Formation ____rightCavalry)
            {
                if (____leftCavalry != null && ____rightCavalry != null && ____leftCavalry.IsAIControlled && ____rightCavalry.IsAIControlled)
                {
                    List<Agent> mountedSkirmishersList = new List<Agent>();
                    List<Agent> mountedMeleeList = new List<Agent>();
                    ____leftCavalry.ApplyActionOnEachUnitViaBackupList(delegate (Agent agent)
                    {
                        bool ismountedSkrimisher = false;
                        for (EquipmentIndex equipmentIndex = EquipmentIndex.WeaponItemBeginSlot; equipmentIndex < EquipmentIndex.NumAllWeaponSlots; equipmentIndex++)
                        {
                            if (agent.Equipment != null && !agent.Equipment[equipmentIndex].IsEmpty)
                            {
                                if (agent.Equipment[equipmentIndex].Item.Type == ItemTypeEnum.Thrown && agent.MountAgent != null)
                                {
                                    ismountedSkrimisher = true;
                                    break;
                                }
                            }
                        }
                        if (ismountedSkrimisher)
                        {
                            mountedSkirmishersList.Add(agent);
                        }
                        else
                        {
                            mountedMeleeList.Add(agent);
                        }
                    });

                    ____rightCavalry.ApplyActionOnEachUnitViaBackupList(delegate (Agent agent)
                    {
                        bool ismountedSkrimisher = false;
                        for (EquipmentIndex equipmentIndex = EquipmentIndex.WeaponItemBeginSlot; equipmentIndex < EquipmentIndex.NumAllWeaponSlots; equipmentIndex++)
                        {
                            if (agent.Equipment != null && !agent.Equipment[equipmentIndex].IsEmpty)
                            {
                                if (agent.Equipment[equipmentIndex].Item.Type == ItemTypeEnum.Thrown && agent.MountAgent != null)
                                {
                                    ismountedSkrimisher = true;
                                    break;
                                }
                            }
                        }
                        if (ismountedSkrimisher)
                        {
                            mountedSkirmishersList.Add(agent);
                        }
                        else
                        {
                            mountedMeleeList.Add(agent);
                        }
                    });
                    int j = 0;
                    int cavalryCount = ____leftCavalry.CountOfUnits + ____rightCavalry.CountOfUnits;
                    foreach (Agent agent in mountedSkirmishersList.ToList())
                    {
                        if (j < cavalryCount / 2)
                        {
                            agent.Formation = ____leftCavalry;
                        }
                        else
                        {
                            agent.Formation = ____rightCavalry;
                        }
                        j++;
                    }
                    foreach (Agent agent in mountedMeleeList.ToList())
                    {
                        if (j < cavalryCount / 2)
                        {
                            agent.Formation = ____leftCavalry;
                        }
                        else
                        {
                            agent.Formation = ____rightCavalry;
                        }
                        j++;
                    }
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch("GetTacticWeight")]
            private static void PostfixGetTacticWeight(ref TacticRangedHarrassmentOffensive __instance, ref float __result)
            {
                if (__instance.Team?.Leader?.Character?.Culture?.GetCultureCode() == CultureCode.Khuzait)
                {
                    __result *= 1.1f;
                }
                else
                {
                    __result *= 0.6f;
                }
            }
        }

        [HarmonyPatch(typeof(TacticComponent))]
        private class OverrideTacticComponent
        {
            [HarmonyPrefix]
            [HarmonyPatch("SetDefaultBehaviorWeights")]
            private static bool PrefixSetDefaultBehaviorWeights(ref Formation f)
            {
                if (f != null)
                {
                    if (f.QuerySystem.IsRangedFormation)
                    {
                        f.AI.SetBehaviorWeight<BehaviorCharge>(0.2f);
                    }
                    else
                    {
                        f.AI.SetBehaviorWeight<BehaviorCharge>(1f);
                    }
                    f.AI.SetBehaviorWeight<BehaviorPullBack>(0f);
                    f.AI.SetBehaviorWeight<BehaviorStop>(0f);
                    f.AI.SetBehaviorWeight<BehaviorReserve>(0f);
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(TacticComponent))]
        private class ManageFormationCountsPatch
        {
            [HarmonyPrefix]
            [HarmonyPatch("ManageFormationCounts", new Type[] { typeof(int), typeof(int), typeof(int), typeof(int) })]
            private static bool PrefixSetDefaultBehaviorWeights(ref TacticComponent __instance, ref int infantryCount, ref int rangedCount, ref int cavalryCount, ref int rangedCavalryCount)
            {
                if (Mission.Current != null && Mission.Current.IsFieldBattle)
                {
                    foreach (Agent agent in __instance.Team.ActiveAgents)
                    {
                        if (agent != null && agent.IsHuman && !agent.IsRunningAway)
                        {
                            EquipmentIndex wieldedItemIndex = agent.GetWieldedItemIndex(Agent.HandIndex.MainHand);
                            bool isRanged = (wieldedItemIndex != EquipmentIndex.None && agent.Equipment.HasRangedWeapon(WeaponClass.Arrow) && agent.Equipment.GetAmmoAmount(wieldedItemIndex) > 5) || (wieldedItemIndex != EquipmentIndex.None && agent.Equipment.HasRangedWeapon(WeaponClass.Bolt) && agent.Equipment.GetAmmoAmount(wieldedItemIndex) > 5);
                            if (agent.HasMount && isRanged)
                            {
                                if (__instance.Team.GetFormation(FormationClass.HorseArcher) != null && __instance.Team.GetFormation(FormationClass.HorseArcher).IsAIControlled && agent.Formation != null && agent.Formation.IsAIControlled)
                                {
                                    agent.Formation = __instance.Team.GetFormation(FormationClass.HorseArcher);
                                }
                            }
                            if (agent.HasMount && !isRanged)
                            {
                                if (__instance.Team.GetFormation(FormationClass.Cavalry) != null && __instance.Team.GetFormation(FormationClass.Cavalry).IsAIControlled && agent.Formation != null && agent.Formation.IsAIControlled)
                                {
                                    agent.Formation = __instance.Team.GetFormation(FormationClass.Cavalry);
                                }
                            }
                            if (!agent.HasMount && isRanged)
                            {
                                if (__instance.Team.GetFormation(FormationClass.Ranged) != null && __instance.Team.GetFormation(FormationClass.Ranged).IsAIControlled && agent.Formation != null && agent.Formation.IsAIControlled)
                                {
                                    agent.Formation = __instance.Team.GetFormation(FormationClass.Ranged);
                                }
                            }
                            if (!agent.HasMount && !isRanged)
                            {
                                if (__instance.Team.GetFormation(FormationClass.Infantry) != null && __instance.Team.GetFormation(FormationClass.Infantry).IsAIControlled && agent.Formation != null && agent.Formation.IsAIControlled)
                                {
                                    agent.Formation = __instance.Team.GetFormation(FormationClass.Infantry);
                                }
                            }
                        }
                    }
                }
                if (Mission.Current.MainAgent != null && Mission.Current.PlayerTeam != null && Mission.Current.IsSiegeBattle)
                {
                    Mission.Current.MainAgent.Formation = Mission.Current.PlayerTeam.GetFormation(FormationClass.Infantry);
                }
                return true;
            }

            [HarmonyPatch(typeof(MissionGauntletOrderOfBattleUIHandler))]
            private class MissionGauntletOrderOfBattleUIHandlerPatch
            {
                [HarmonyPostfix]
                [HarmonyPatch("OnDeploymentFinish")]
                private static void PostfixOnDeploymentFinish()
                {
                    if (Mission.Current != null)
                    {
                        if (Mission.Current.MainAgent != null && Mission.Current.PlayerTeam != null && Mission.Current.IsSiegeBattle)
                        {
                            Mission.Current.MainAgent.Formation = Mission.Current.PlayerTeam.GetFormation(FormationClass.Infantry);

                        }
                    }
                }
            }

        }
    }
}