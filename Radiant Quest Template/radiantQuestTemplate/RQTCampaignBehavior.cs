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
    class RQTCampaignBehavior : CampaignBehaviorBase //1-Update class name
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
                typeof(RQTCampaignBehavior.RQTIssue), IssueBase.IssueFrequency.VeryCommon, null)); //1-Update class name
            }
        }
        //dedicated method function for Quest availablility logic
        private bool ConditionsHold(Hero issueGiver) //2-Define Issue conditions
        {
            return issueGiver != null;
        }

        private IssueBase OnStartIssue(PotentialIssueData pid, Hero issueOwner)
        {
            InformationManager.DisplayMessage(new InformationMessage("Quest genereated at: "+issueOwner.CurrentSettlement.ToString()));
            return new RQTCampaignBehavior.RQTIssue(issueOwner); //1-Update class name            
        }

        public class RQTCampaignBehviorIssueTypeDefiner : CampaignBehaviorBase.SaveableCampaignBehaviorTypeDefiner //1-Update class name
        {
            public RQTCampaignBehviorIssueTypeDefiner () : base(0983218932) //1-Update class name
            {
            }

            protected override void DefineClassTypes()
            {
                AddClassDefinition(typeof(RQTCampaignBehavior.RQTIssue), 1); //1-Update class name
                AddClassDefinition(typeof(RQTQuest), 2); //1-Update class name
            }
        }

        internal class RQTIssue : IssueBase //1-Update class name
        {
            public RQTIssue(Hero issueOwner) : base(issueOwner, CampaignTime.DaysFromNow(10f)) //1-Update class name
            {
            }

            // <Required overrides (abstract)
            public override TextObject Title => GameTexts.FindText("quest_dialog_title");

            public override TextObject Description => GameTexts.FindText("quest_dialog_description");

            protected override TextObject IssueBriefByIssueGiver //3-Update quest acceptance text
            {
                get
                {
                    TextObject result = GameTexts.FindText("quest_dialog_issuebrief");


                    if (this.IssueOwner != null)
                    {
                        StringHelpers.SetCharacterProperties("TARGET", this.IssueOwner.CharacterObject, null, result, false);                        
                        StringHelpers.SetSettlementProperties("SETTLEMENT", this.IssueOwner.HomeSettlement, result);
                    }
                    return result;

                }
            }

            protected override TextObject IssueAcceptByPlayer //3-Update quest acceptance text
            {
                get
                {
                    return new TextObject("This is the seconed dialogue. The players response to the Quest giver's issue.");
                }
            }

            protected override TextObject IssueQuestSolutionExplanationByIssueGiver //3-Update quest acceptance text
            {
                get
                {
                    return new TextObject("This is the third dialoge. It is said by the quest giver");
                }
            }

            protected override TextObject IssueQuestSolutionAcceptByPlayer //3-Update quest acceptance text
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
            protected override bool CanPlayerTakeQuestConditions(Hero issueGiver, out PreconditionFlags flag, out Hero relationHero, out SkillObject skill) //5-Define quest requirements for taking
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

                return new RQTQuest(questId, base.IssueOwner, //1-Update class name
                    CampaignTime.DaysFromNow(17f), RewardGold);
            }

            protected override void OnGameLoad()
            {

            }
            // </Required overrides (abstract)
        }
        //Quest class. For the most part, takes over the quest process after IssueBase.GenerateIssueQuest is called
        
    }
}
