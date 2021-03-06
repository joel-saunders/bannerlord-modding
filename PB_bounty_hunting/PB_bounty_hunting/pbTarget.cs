﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using SandBox.Source.Missions;
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
using TaleWorlds.ObjectSystem;
using TaleWorlds.Library;

namespace PB_bounty_hunting
{
    class pbTarget
    {        
        public pbTarget(BountyHuntingCampaignBehavior.BountyHuntingQuest quest)
        {
            this.locationofCrime = quest.QuestGiver.CurrentSettlement;
            this.timeOfDayAvailable = BountyHuntingCampaignBehavior.BountyHuntingQuest.TimeOfDay.GetRandomElement<string>();
            this.culture = quest.QuestGiver.Culture;
            this.characterObject = (CharacterObject.Templates.GetRandomElement<CharacterObject>());
            
        }

        

        public CharacterObject characterObject;
        public Settlement locationofCrime;
        public Settlement hiddenLocation;
        public string timeOfDayAvailable;
        public CultureObject culture;
        
    }
}
