using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using SandBox.Quests.QuestBehaviors;
using TaleWorlds.CampaignSystem.SandBox.Issues;
using TaleWorlds.CampaignSystem;

namespace SC_Teaser_Quests.Patch
{
    internal class Patcher
    {
        [HarmonyPatch(typeof(HeadmanNeedsGrainIssueBehavior), "ConditionsHold")]
        internal class PatchGrainQuest
        {
            public static void Postfix(Hero issueGiver, ref bool __result)
            {
                __result = false;
                return;
            }
        }

        [HarmonyPatch(typeof(NotableWantsDaughterFoundIssueBehavior), "ConditionsHold")]
        internal class PatchHeadmansDaughterQuest
        {
            public static void Postfix(Hero issueGiver, ref bool __result)
            {
                __result = false;
                return;
            }
        }
    }
}
