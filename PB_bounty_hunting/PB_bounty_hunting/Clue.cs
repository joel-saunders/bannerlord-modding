using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using SandBox.Source.Missions.Handlers;
using TaleWorlds.MountAndBlade;
using TaleWorlds.CampaignSystem;
using SandBox;
//using SandBox.Quests;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.CharacterDevelopment.Managers;
using TaleWorlds.CampaignSystem.Conversation.Persuasion;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.CampaignSystem.Actions;
using Helpers;
using TaleWorlds.CampaignSystem.Overlay;
//using System.Windows.Forms;
using TaleWorlds.TwoDimension;
using TaleWorlds.CampaignSystem.SandBox;
using TaleWorlds.Diamond.AccessProvider.Test;
using TaleWorlds.SaveSystem;
using TaleWorlds.CampaignSystem.SandBox.Issues;
using NetworkMessages.FromServer;

namespace PB_bounty_hunting
{
    class Clue
    {
        public enum ClueType
        {
            Location = 0,
            TimeofDay = 1,
            Culture = 2
        }
        public Clue(ClueType type, BountyHuntingCampaignBehavior.BountyHuntingQuest quest)
        {
            this.Type = type;
            switch (this.Type)
            {
                case ClueType.Culture:
                    this.clueDescriptionDialog = "The target was apparently " + quest.Target.culture.ToString();
                    break;
                case ClueType.Location:
                    this.clueDescriptionDialog = "The crime happened at "+quest.Target.locationofCrime.Name.ToString()+" so they would have been coming from that direction";
                    break;
                case ClueType.TimeofDay:
                    this.clueDescriptionDialog = "It was around "+quest.Target.timeOfDayAvailable.ToString()+" so the perp would have arrived shortly after then.";
                    break;
            }
    }

        public ClueType Type;
        public string clueDescriptionDialog;
    }
}
