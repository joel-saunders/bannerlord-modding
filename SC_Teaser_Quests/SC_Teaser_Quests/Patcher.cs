using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using SandBox.Quests.QuestBehaviors;
using TaleWorlds.CampaignSystem;

namespace SC_Teaser_Quests.Patch
{
    internal class Patcher
    {
        [HarmonyPatch(typeof(NotableWantsDaughterFoundIssueBehavior), "ConditionsHold")]
        internal class PatchNotablesDaughterQuest
        {
            //public static void Postfix(Hero issuGiver, ref bool __result)
            //{
            //    __result = false;
            //    return;
            //}
        }
    }
}
