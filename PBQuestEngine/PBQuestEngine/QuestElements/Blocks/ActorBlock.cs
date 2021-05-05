using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SandBox.Source.Missions.Handlers;
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
using System.Windows.Forms;
//using TaleWorlds.TwoDimension;
using TaleWorlds.CampaignSystem.SandBox;
using TaleWorlds.Diamond.AccessProvider.Test;
using TaleWorlds.SaveSystem;
using TaleWorlds.ObjectSystem;
using TaleWorlds.CampaignSystem.SandBox.Issues;
using NetworkMessages.FromServer;
using Messages.FromClient.ToLobbyServer;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using System.Runtime.Remoting.Messaging;
using TaleWorlds.Library;
using System.Data.Common;
using PBQuestEngine;

namespace PBQuestEngine.Blocks
{
    class ActorBlock : IGameplayBlock, IQuestElement
    {
        public ActorBlock(CultureObject culture, bool isFemale, bool isHero = false, String name = "tempName", Occupation occupation = Occupation.Wanderer)
        {

        }

        public CharacterObject characterObject;
        public Agent agent;
        public LocationCharacter locationCharacter;
    }
}
