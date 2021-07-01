using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Helpers;
using MountAndBlade.CampaignBehaviors;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace PBQuestEngine.Utils
{
    public static class PBConsoleCommands
    {
        [CommandLineFunctionality.CommandLineArgumentFunction("Test_worked!", "TEST")]
        public static string TempTestMethod(List<string> strings)
        {
            return "it works!!!...?";
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("CheckHeros", "MissionUtils")]
        public static string PrintHerosInMission(List<string> strings)
        {
            String returnString = "";
            IEnumerable<Agent> heroAgents = new List<Agent>();
            heroAgents = Mission.Current.Agents.Where<Agent>((Agent agen) => agen.CheckTracked(agen.Character));



            foreach (Agent agen in heroAgents)
            {
                returnString += agen.Name.ToString();
            }

            return returnString;
        }

    }
}
