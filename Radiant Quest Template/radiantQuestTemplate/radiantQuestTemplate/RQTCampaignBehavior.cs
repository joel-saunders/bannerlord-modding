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

namespace radiantQuestTemplate
{
    class RQTCampaignBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.OnCheckForIssueEvent.AddNonSerializedListener(this, new Action<IssueArgs>(this.OnCheckForIssues));
        }

        public override void SyncData(IDataStore dataStore)
        {           
        }

        public void OnCheckForIssues(IssueArgs issueArgs)
        {
            if (ConditionsHold(issueArgs.IssueOwner))
            {
                issueArgs.SetPotentialIssueData(new PotentialIssueData(new Func<PotentialIssueData, Hero, IssueBase>(this.OnStartIssue),
                typeof(), IssueBase.IssueFrequency.Common, null));
            }
        }

        private bool ConditionsHold(Hero issueGiver)
        {
            return true;
        }

        private IssueBase OnStartIssue(PotentialIssueData pid, Hero issueOwner)
        {
            //Hero issueTarget = issueOwner;
            return...
        }

        public class RQTCampaignBehviorIssueTypeDefiner : CampaignBehaviorBase.SaveableCampaignBehaviorTypeDefiner
        {
            public RQTCampaignBehviorIssueTypeDefiner () : base(0983218932)
            {
            }

            protected override void DefineClassTypes()
            {
                //add issue classes
            }
        }

        internal class RQTIssue : IssueBase
        {
            public RQTIssue(Hero issueOwner) : base(issueOwner, new Dictionary<IssueEffect, float>(), CampaignTime.DaysFromNow(10f))
            {
            }

            public override TextObject Title => new TextObject("Template Quest Title");

            public override TextObject Description => new TextObject("Help out the quest giver!");

            protected override TextObject IssueBriefByIssueGiver
            {
                get
                {
                    TextObject result = new TextObject("This is the first dialoge after the player asks I've heard you have an issue..." +
                        "");
                     
                    if (this.IssueOwner != null)
                    {
                        StringHelpers.SetCharacterProperties("TARGET", this.IssueOwner.CharacterObject, null, result, false);
                        //MBTextManager.SetTextVariable("SETTLEMENT", this.IssueSettlement.ToString());
                        StringHelpers.SetSettlementProperties("SETTLEMENT", this.IssueOwner.HomeSettlement, result);
                    }
                    return result;

                }
            }

            protected override TextObject IssueAcceptByPlayer
            {
                get
                {
                    return new TextObject("What are you needing help with though, I'm not trying to fight a gang war.");
                }
            }

            protected override TextObject IssueQuestSolutionExplanationByIssueGiver
            {
                get
                {
                    return new TextObject("No, no nothing of the sort. In fact, all I'm actually looking for is information. Perhaps if they " +
                        "were to get somewhat inebraited, they may become a little too comfortable talking about their business dealings with you.");
                }
            }

            protected override TextObject IssueQuestSolutionAcceptByPlayer
            {
                get
                {
                    return new TextObject("Get them drunk and let them do the rest? I can handle this.");
                }
            }

            protected override bool IsThereAlternativeSolution => throw new NotImplementedException();

            protected override bool IsThereLordSolution => throw new NotImplementedException();

            public override IssueBase.IssueFrequency GetFrequency()
            {
                return IssueBase.IssueFrequency.Common;
            }

            public override bool IssueStayAliveConditions()
            {
                return true;
            }

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

            protected override QuestBase GenerateIssueQuest(string questId)
            {
                InformationManager.DisplayMessage(new InformationMessage("***Quest is generated"));

                return new RQTCampaignBehavior.RQTCampaignBehaviorIssueQuest(questId, base.IssueOwner, this._targetHero,
                    CampaignTime.DaysFromNow(17f), RewardGold);
            }

            protected override void OnGameLoad()
            {
                throw new NotImplementedException();
            }
        }
    }
}
