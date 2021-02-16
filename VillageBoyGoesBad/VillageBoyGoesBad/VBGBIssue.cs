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
using TaleWorlds.TwoDimension;
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

namespace VillageBoyGoesBad
{
    class VBGBIssue : IssueBase
    {
        public VBGBIssue(Hero issueOwner, Hero gangLeader) : base(issueOwner, CampaignTime.DaysFromNow(10f))
        {
            this._gangLeader = gangLeader;
            this._targetTown = issueOwner.CurrentSettlement.Village.Bound.Town;
        }

        // <Required overrides (abstract)
        public override TextObject Title => new TextObject("A Rouge in the Making");

        public override TextObject Description => new TextObject("Help out the quest giver!");

        protected override TextObject IssueBriefByIssueGiver //3-Update quest acceptance text
        {
            get
            {
                //this._isFriendsWithGang = this._gangLeader.GetRelationWithPlayer() >= 10;                    

                TextObject result = new TextObject("Well yes, it's my son you see. He's fallen prey to the allure of {TARGET.LINK} and this is {SETTLEMENT.LINK}");

                if (this.IssueOwner != null)
                {
                    StringHelpers.SetCharacterProperties("TARGET", this._gangLeader.CharacterObject, null, result);
                    StringHelpers.SetSettlementProperties("SETTLEMENT", this.IssueOwner.CurrentSettlement, result);
                }
                return result;

            }
        }

        protected override TextObject IssueAcceptByPlayer //3-Update quest acceptance text
        {
            get
            {
                return new TextObject("I see, well such is the way of the naive.");
            }
        }

        protected override TextObject IssueQuestSolutionExplanationByIssueGiver //3-Update quest acceptance text
        {
            get
            {
                TextObject returnResult = new TextObject("True as that may be, I care not for him to find out his missteps the hard way. You seem to be the independent type- perhaps you could " +
                    "convince him the way of crime can only lead to self destruction. I'm willing to pay you too, {REWARD_GOLD}{GOLD_ICON}");
                returnResult.SetTextVariable("REWARD_GOLD", this.RewardGold);
                returnResult.SetTextVariable("GOLD_ICON", "{=!}<img src=\"Icons\\Coin@2x\">");
                return returnResult;
            }
        }

        protected override TextObject IssueQuestSolutionAcceptByPlayer //3-Update quest acceptance text
        {
            get
            {
                if (this._isFriendsWithGang)
                {
                    TextObject result = new TextObject("I can get your son back. Infact, I am fairly well aquainted with {GANGLEADER.LINK}. I'm sure I can work something out with them.");
                    StringHelpers.SetCharacterProperties("GANGLEADER", this._gangLeader.CharacterObject, null, result, false);
                    return result;
                }
                else
                {
                    return new TextObject("That much I can do. I will talk with your son.");
                }
            }
        }

        protected override bool IsThereAlternativeSolution => false;

        protected override bool IsThereLordSolution => false; //not sure what this is..

        public override IssueBase.IssueFrequency GetFrequency() //VeryCommon, Common, Rare
        {
            return IssueBase.IssueFrequency.VeryCommon;
        }

        public override bool IssueStayAliveConditions() //not sure what this is
        {
            return true;
        }
        //Not sure what the difference is between this and the "oncheckforissues" logic. Does this allow the quest to still generate but you just can't see it?
        protected override bool CanPlayerTakeQuestConditions(Hero issueGiver, out PreconditionFlags flag, out Hero relationHero, out SkillObject skill)
        {
            bool flag2 = issueGiver.GetRelationWithPlayer() >= -10;
            flag = (flag2 ? IssueBase.PreconditionFlags.None : IssueBase.PreconditionFlags.Relation);
            relationHero = issueGiver;
            skill = null;

            return flag2;
        }

        protected override void CompleteIssueWithTimedOutConsequences()
        {
        }

        protected override int RewardGold => (int)(350f + 1500f * base.IssueDifficultyMultiplier);

        //When the quest is generated and params are passed into the Quest instance.
        protected override QuestBase GenerateIssueQuest(string questId)
        {
            //InformationManager.DisplayMessage(new InformationMessage("difficulty is: "+base.IssueDifficultyMultiplier));

            return new VBGBQuest(questId, base.IssueOwner, this._targetTown, this._gangLeader, this._isFriendsWithGang,
                CampaignTime.DaysFromNow(10f), this.RewardGold);
        }

        protected override void OnGameLoad()
        {

        }
        // </Required overrides (abstract)

        public bool _isFriendsWithGang => this._gangLeader.GetRelationWithPlayer() >= 10;

        [SaveableField(10)]
        public Hero _gangLeader;

        [SaveableField(20)]
        public Town _targetTown;



    }
        
}
