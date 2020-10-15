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
using System.Windows.Forms;
using TaleWorlds.TwoDimension;
using TaleWorlds.CampaignSystem.SandBox;
using TaleWorlds.Diamond.AccessProvider.Test;
using TaleWorlds.SaveSystem;
using TaleWorlds.CampaignSystem.SandBox.Issues;
using NetworkMessages.FromServer;
using Messages.FromClient.ToLobbyServer;

namespace VillageBoyGoesBad
{
    class VBGBCampaignBehavior : CampaignBehaviorBase
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
                typeof(VBGBCampaignBehavior.VBGBIssue), IssueBase.IssueFrequency.VeryCommon, null));
            }
        }
        //dedicated method function for Quest availablility logic
        private bool ConditionsHold(Hero issueGiver)
        {
            return issueGiver != null &&
                    issueGiver.IsHeadman && 
                    issueGiver.CurrentSettlement != null &&
                    issueGiver.CurrentSettlement.Village.Bound.Notables.Any((Hero gl) => gl.IsGangLeader && !gl.IsOccupiedByAnEvent());

        }

        private IssueBase OnStartIssue(PotentialIssueData pid, Hero issueOwner)
        {

            Hero gangLeader = this.GetGangNotable(issueOwner);
            return new VBGBCampaignBehavior.VBGBIssue(issueOwner, gangLeader);
        }

        private Hero GetGangNotable(Hero issueOwner)
        {
            Hero result = null;
            foreach(Hero hero in from x in issueOwner.CurrentSettlement.Village.Bound.Notables
                                 where x.IsGangLeader && !x.IsOccupiedByAnEvent()
                                 select x)
            {
                result = hero;
                break;
            }
            return result;
        }
        public class VBGBCampaignBehviorIssueTypeDefiner : CampaignBehaviorBase.SaveableCampaignBehaviorTypeDefiner
        {
            public VBGBCampaignBehviorIssueTypeDefiner() : base(983218932)
            {
            }

            protected override void DefineClassTypes()
            {
                AddClassDefinition(typeof(VBGBCampaignBehavior.VBGBIssue), 1);
                AddClassDefinition(typeof(VBGBCampaignBehavior.VBGBQuest), 2);
            }
        }

        internal class VBGBIssue : IssueBase
        {
            public VBGBIssue(Hero issueOwner, Hero gangLeader) : base(issueOwner, new Dictionary<IssueEffect, float>(), CampaignTime.DaysFromNow(10f))
            {
                this._gangLeader = gangLeader;
                this._targetTown = issueOwner.CurrentSettlement.Village.Bound.Town;
            }

            // <Required overrides (abstract)
            public override TextObject Title => new TextObject("A Rouge in the Making");

            public override TextObject Description => new TextObject("Help out the quest giver!");

            protected override TextObject IssueBriefByIssueGiver
            {
                get
                {
                    TextObject result = new TextObject("Well yes, it's my son you see. He's fallen prey to the allure of {TARGET.LINK} and this is {SETTLEMENT.LINK}");

                    if (this.IssueOwner != null)
                    {
                        StringHelpers.SetCharacterProperties("TARGET", this._gangLeader.CharacterObject, null, result);
                        StringHelpers.SetSettlementProperties("SETTLEMENT", this.IssueOwner.CurrentSettlement, result);
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

                return new VBGBCampaignBehavior.VBGBQuest(questId, base.IssueOwner, this._targetTown, this._gangLeader,
                    CampaignTime.DaysFromNow(17f), RewardGold);
            }

            protected override void OnGameLoad()
            {

            }
            // </Required overrides (abstract)

                [SaveableField(10)]
            public Hero _gangLeader;

            [SaveableField(20)]
            public Town _targetTown;
        }
        //Quest class. For the most part, takes over the quest process after IssueBase.GenerateIssueQuest is called
        internal class VBGBQuest : QuestBase
        {
            public VBGBQuest(string questId, Hero questGiver, Town targetTown, Hero gangLeader, CampaignTime duration, int rewardGold) : base(questId, questGiver, duration, rewardGold)
            {
                //init Quest vars, such as 'PlayerhastalkedwithX', 'DidPlayerFindY'
                this._gangLeader = gangLeader;
                this._targetTown = targetTown;
                this.SetDialogs();
                this.InitializeQuestOnCreation();
                base.AddLog(new TextObject("The quest has begun!!! woooo!"));
            }

            protected override void RegisterEvents()
            {
                base.RegisterEvents();
                CampaignEvents.SettlementEntered.AddNonSerializedListener(this, new Action<MobileParty, Settlement, Hero>(this.EnterTargetSettlement));
                CampaignEvents.BeforeMissionOpenedEvent.AddNonSerializedListener(this, new Action(this.BeforeTownEnter));
            }

            private void EnterTargetSettlement(MobileParty party, Settlement settlement, Hero hero)
            {
                if(party != null && party.IsMainParty && settlement != null && settlement.Town == _targetTown)
                {
                    InformationManager.DisplayMessage(new InformationMessage("the headman's son must be here"));                    
                }
            }

            private LocationCharacter CreateHeadmansSon()
            {
                
                Hero troop2 = HeroCreator.CreateRelativeNotableHero(QuestGiver); //CreateSpecialHero(Extensions.GetRandomElement<CharacterObject>(from x in CharacterObject.Templates
                                                                                    //where x.Occupation == Occupation.Outlaw                                                                                                          
                                                                                    //select x));
                troop2.Name = new TextObject("Notable's son");
                this._headmansSon = troop2;
                
                //Hero troop = HeroCreator.CreateSpecialHero(CharacterObject.Templates.GetRandomElement<CharacterObject>(), null, null, null);
                AgentData agent = new AgentData(new SimpleAgentOrigin(troop2.CharacterObject));
                LocationCharacter locChar = new LocationCharacter(agent, new LocationCharacter.AddBehaviorsDelegate(SandBoxManager.Instance.AgentBehaviorManager.AddWandererBehaviors),
                "npc_common", true, LocationCharacter.CharacterRelations.Friendly, "as_human_villager_gangleader", true, false, null, false, true, true);                

                return locChar;
            }

            private void BeforeTownEnter()
            {
                if(this._targetTown != null && Settlement.CurrentSettlement == _targetTown.Settlement)
                {
                    Location locationwithId = Settlement.CurrentSettlement.LocationComplex.GetLocationWithId("center");
                    locationwithId.AddCharacter(this.CreateHeadmansSon());
                }
            }
            // Required overrides (abstract)
            public override TextObject Title => new TextObject("A Rouge in the Making");

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
                    NpcLine("TEMPLATE Good, I'm glad you've agreed to the quest. Good luck!").
                        Condition(() => Hero.OneToOneConversationHero == this.QuestGiver).
                        Consequence(QuestAcceptedConsequences).CloseDialog();
                this.DiscussDialogFlow = DialogFlow.CreateDialogFlow("quest_discuss", 100).
                    NpcLine("TEMPLATE Why are you here? Shouldn't you be questing?").
                        Condition(() => Hero.OneToOneConversationHero == this.QuestGiver);
                Campaign.Current.ConversationManager.AddDialogFlow(initalSonEncounter());
            }

            private DialogFlow initalSonEncounter()
            {
                DialogFlow resultFlow = DialogFlow.CreateDialogFlow("start", 6000).
                    NpcLine("ladie ladie laaaa, just minding my own business").
                        Condition(() => Hero.OneToOneConversationHero == _headmansSon && !_initialSonConvoComplete).
                    PlayerLine("Hey, I know you think you're cool, but you're not!!!").
                        
                    NpcLine("Hey, frick off boomer.");

                return resultFlow;
            }
            // </Required overrides

            // Optional Overrides (virtual)
            

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
                //What all should happen when the quest is complete
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

            private void SuccessComplete()
            {
                base.CompleteQuestWithSuccess();
            }

            private void FailureComplete()
            {
                base.CompleteQuestWithFail();
            }

            //Saveable Properties
            [SaveableField(10)]
            public Hero _gangLeader;

            [SaveableField(20)]
            public Town _targetTown;

            [SaveableField(30)]
            public Hero _headmansSon;

            [SaveableField(40)]
            public bool _initialSonConvoComplete;
        }
    }
}
