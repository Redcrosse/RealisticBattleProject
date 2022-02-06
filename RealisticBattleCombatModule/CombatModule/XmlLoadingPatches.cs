﻿using HarmonyLib;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using TaleWorlds.ObjectSystem;

namespace RealisticBattleCombatModule.CombatModule
{
    class XmlLoadingPatches
    {
        [HarmonyPatch(typeof(MBObjectManager))]
        [HarmonyPatch("MergeTwoXmls")]
        class MergeTwoXmlsPatch
        {
            static bool Prefix(XmlDocument xmlDocument1, XmlDocument xmlDocument2, ref XmlDocument __result)
            {
                XDocument originalXml = MBObjectManager.ToXDocument(xmlDocument1);
                XDocument mergedXml = MBObjectManager.ToXDocument(xmlDocument2);

                List<XElement> nodesToRemoveArray = new List<XElement>();

                if (RBMCMConfig.dict["Global.TroopOverhaulActive"] == 0 && xmlDocument2.BaseURI.Contains("unit_overhaul"))
                {
                    __result = MBObjectManager.ToXmlDocument(originalXml);
                    return false;
                }

                bool isShoulderShiledsEnabled = false;
                if (RBMCMConfig.dict["Global.PassiveShoulderShields"] == 0)
                {
                    isShoulderShiledsEnabled = false;
                }
                else if (RBMCMConfig.dict["Global.PassiveShoulderShields"] == 1)
                {
                    isShoulderShiledsEnabled = true;
                }

                bool isBetterArrowVisualsEnabled = false;
                if (RBMCMConfig.dict["Global.BetterArrowVisuals"] == 0)
                {
                    isBetterArrowVisualsEnabled = false;
                }
                else if (RBMCMConfig.dict["Global.BetterArrowVisuals"] == 1)
                {
                    isBetterArrowVisualsEnabled = true;
                }

                foreach (XElement origNode in originalXml.Root.Elements())
                {
                    if (origNode.Name == "CraftedItem" && xmlDocument2.BaseURI.Contains("RealisticBattle"))
                    {
                        foreach (XElement mergedNode in mergedXml.Root.Elements())
                        {
                            if (mergedNode.Name == "CraftedItem")
                            {
                                if (origNode.Attribute("id").Value.Equals(mergedNode.Attribute("id").Value))
                                {
                                    nodesToRemoveArray.Add(origNode);
                                }
                            }
                        }
                    }

                    if (origNode.Name == "Item" && xmlDocument2.BaseURI.Contains("RealisticBattle"))
                    {
                        foreach (XElement mergedNode in mergedXml.Root.Elements())
                        {
                            if (mergedNode.Name == "Item")
                            {
                                if (origNode.Attribute("id").Value.Equals(mergedNode.Attribute("id").Value))
                                {
                                    nodesToRemoveArray.Add(origNode);
                                }

                                if (isBetterArrowVisualsEnabled && (mergedNode.Attribute("Type").Value.Equals("Arrows") || mergedNode.Attribute("Type").Value.Equals("Bolts")))
                                {
                                    mergedNode.Attribute("flying_mesh").Value = mergedNode.Attribute("mesh").Value;
                                }
                            }
                        }
                    }

                    if (origNode.Name == "NPCCharacter" && xmlDocument2.BaseURI.Contains("RealisticBattle"))
                    {
                        foreach (XElement nodeEquip in origNode.Elements())
                        {
                            if (nodeEquip.Name == "Equipments")
                            {
                                foreach (XElement nodeEquipRoster in nodeEquip.Elements())
                                {
                                    if (nodeEquipRoster.Name == "EquipmentRoster")
                                    {
                                        foreach (XElement mergedNode in mergedXml.Root.Elements())
                                        {
                                            if (origNode.Attribute("id").Value.Equals(mergedNode.Attribute("id").Value))
                                            {
                                                foreach (XElement mergedNodeEquip in mergedNode.Elements())
                                                {
                                                    if (mergedNodeEquip.Name == "Equipments")
                                                    {
                                                        foreach (XElement mergedNodeRoster in mergedNodeEquip.Elements())
                                                        {
                                                            if (mergedNodeRoster.Name == "EquipmentRoster")
                                                            {
                                                                if (!nodesToRemoveArray.Contains(origNode))
                                                                {
                                                                    nodesToRemoveArray.Add(origNode);
                                                                }
                                                                foreach (XElement equipmentNode in mergedNodeRoster.Elements())
                                                                {
                                                                    if (equipmentNode.Name == "equipment")
                                                                    {
                                                                        if (equipmentNode.Attribute("id") != null && equipmentNode.Attribute("id").Value.Contains("shield_shoulder") && !isShoulderShiledsEnabled)
                                                                        {
                                                                            equipmentNode.Attribute("id").Value = equipmentNode.Attribute("id").Value.Replace("shield_shoulder", "shield");
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (nodesToRemoveArray.Count > 0)
                {
                    foreach (XElement node in nodesToRemoveArray)
                    {
                        node.Remove();
                    }
                }

                originalXml.Root.Add(mergedXml.Root.Elements());
                __result = MBObjectManager.ToXmlDocument(originalXml);
                return false;
            }
        }

    }
}
