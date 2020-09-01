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

namespace radiantQuestTemplate
{
    class RQTCampaignBehavior : CampaignBehaviorBase
    {
        //Event regestration. Examples, AddGameMenu; MissionTickEvent; OnPlayerBattleEndEvent; PartyVisibilityChangedEvent; OnUnitRecruitedEvent; KingdomCreatedEvent
        public override void RegisterEvents()
        {
            CampaignEvents.OnCheckForIssueEvent.AddNonSerializedListener(this, new Action<IssueArgs>(this.OnCheckForIssues));
        }

        public override void SyncData(IDataStore dataStore)
        {   
            //do nothing..
        }

        public void OnCheckForIssues(IssueArgs issueArgs)
        {
            //Sets the Quest issue as a potential quest. First we call separate function to determine if the quest should be set as a potential quest
            if (ConditionsHold(issueArgs.IssueOwner))
            {
                issueArgs.SetPotentialIssueData(new PotentialIssueData(new Func<PotentialIssueData, Hero, IssueBase>(this.OnStartIssue),
                typeof(RQTCampaignBehavior.RQTIssue), IssueBase.IssueFrequency.Common, null));
            }
        }
        //dedicated method function for Quest availablility logic
        private bool ConditionsHold(Hero issueGiver)
        {
            return issueGiver != null;
        }

        private IssueBase OnStartIssue(PotentialIssueData pid, Hero issueOwner)
        {
            return new RQTCampaignBehavior.RQTIssue(issueOwner);
        }

        public class RQTCampaignBehviorIssueTypeDefiner : CampaignBehaviorBase.SaveableCampaignBehaviorTypeDefiner
        {
            public RQTCampaignBehviorIssueTypeDefiner () : base(0983218932)
            {
            }

            protected override void DefineClassTypes()
            {
                AddClassDefinition(typeof(RQTCampaignBehavior.RQTIssue), 1);
                AddClassDefinition(typeof(RQTCampaignBehavior.RQTQuest), 1);
            }
        }

        internal class RQTIssue : IssueBase
        {
            public RQTIssue(Hero issueOwner) : base(issueOwner, new Dictionary<IssueEffect, float>(), CampaignTime.DaysFromNow(10f))
            {
            }

            // <Required overrides (abstract)
            public override TextObject Title => new TextObject("Template Quest Title");

            public override TextObject Description => new TextObject("Help out the quest giver!");

            protected override TextObject IssueBriefByIssueGiver
            {
                get
                {
                    TextObject result = new TextObject("This is the first dialoge after the player asks I've " +
                        "heard you have an issue... I'm {TARGET.LINK} and this is {SETTLEMENT.LINK}");
                     
                    if (this.IssueOwner != null)
                    {
                        StringHelpers.SetCharacterProperties("TARGET", this.IssueOwner.CharacterObject, null, result, false);                        
                        StringHelpers.SetSettlementProperties("SETTLEMENT", this.IssueOwner.HomeSettlement, result);
                    }
                    return result;

                }
            }

            protected override TextObject IssueAcceptByPlayer
            {
                get
                {
                    return new TextObject("This is the seconed dialogue. The players response to the Quest giver's issue.");
                }
            }

            protected override TextObject IssueQuestSolutionExplanationByIssueGiver
            {
                get
                {
                    return new TextObject("This is the third dialoge. It is said by the quest giver");
                }
            }

            protected override TextObject IssueQuestSolutionAcceptByPlayer
            {
                get
                {
                    return new TextObject("This is the 4th dialoge. Said by the player, it confirms the acceptance of the quest.");
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
            //When the quest is generated and params are passed into the Quest instance.
            protected override QuestBase GenerateIssueQuest(string questId)
            {
                InformationManager.DisplayMessage(new InformationMessage("***Quest is generated"));

                return new RQTCampaignBehavior.RQTQuest(questId, base.IssueOwner,
                    CampaignTime.DaysFromNow(17f), RewardGold);
            }

            protected override void OnGameLoad()
            {

            }
            // </Required overrides (abstract)
        }
        //Quest class. For the most part, takes over the quest process after IssueBase.GenerateIssueQuest is called
        internal class RQTQuest : QuestBase
        {
            public RQTQuest(string questId, Hero questGiver, CampaignTime duration, int rewardGold) : base(questId, questGiver, duration, rewardGold)
            {
                //init Quest vars, such as 'PlayerhastalkedwithX', 'DidPlayerFindY'
                this.SetDialogs();
                this.InitializeQuestOnCreation();
                base.AddLog(new TextObject("The quest has begun!!! woooo!"));
            }

            // Required overrides (abstract)
            public override TextObject Title => new TextObject("Quest Title");

            public override bool IsRemainingTimeHidden => false;

            protected override void InitializeQuestOnGameLoad()
            {
                this.SetDialogs();
            }
            //there are a couple DialogFlows QuestBase has that you'll want to set here. In addition, whatever other dialog flows you have should also
            //be called here. Have them in separate methods for simplicity.
            protected override void SetDialogs()
            {
                this.OfferDialogFlow = DialogFlow.CreateDialogFlow("issue_classic_quest_start", 100).
                    NpcLine("Good, I'm glad you've agreed to the quest. Good luck!").
                        Condition(() => Hero.OneToOneConversationHero == this.QuestGiver).
                        Consequence(QuestAcceptedConsequences).CloseDialog();
                this.DiscussDialogFlow = DialogFlow.CreateDialogFlow("quest_discuss", 100).
                    NpcLine("Why are you here? Shouldn't you be questing?").
                        Condition(() => Hero.OneToOneConversationHero == this.QuestGiver);                        
                //Campaign.Current.ConversationManager.AddDialogFlow(dialogflowmethod);
            }
            // </Required overrides

            // Optional Overrides (virtual)
            protected override void RegisterEvents()
            {
                base.RegisterEvents();
            }

            public override bool IsQuestGiverHidden => false;
            public override bool IsSpecialQuest => false; //who knows :shrug emoji
            public override int GetCurrentProgress()
            {
                return base.GetCurrentProgress();
            }
            public override int GetMaxProgress()
            {
                return base.GetMaxProgress();
            }
            public override string GetPrefabName()
            {
                return base.GetPrefabName();
            }
            public override void OnCanceled()
            {
                base.OnCanceled();
            }
            public override void OnFailed()
            {
                base.OnFailed();
            }
            public override bool QuestPreconditions()
            {
                return base.QuestPreconditions();
            }
            protected override void OnBeforeTimedOut(ref bool completeWithSuccess)
            {
                base.OnBeforeTimedOut(ref completeWithSuccess);
            }
            protected override void OnBetrayal()
            {
                base.OnBetrayal();
            }
            protected override void OnCompleteWithSuccess()
            {
                base.OnCompleteWithSuccess();
            }
            protected override void OnFinalize()
            {
                base.OnFinalize();
            }
            protected override void OnStartQuest()
            {
                base.OnStartQuest();
            }
            protected override void OnTimedOut()
            {
                base.OnTimedOut();
            }            
            // </Optional Overrides

            // <Delegates
            private void QuestAcceptedConsequences()
            {
                base.StartQuest();
            }
        }
    }
}
