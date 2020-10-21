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

            protected override TextObject IssueBriefByIssueGiver //3-Update quest acceptance text
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
                    return new TextObject("True as that may be, I care not for him to find out his missteps the hard way. You seem to be the independent type- perhaps you could convince him the way of crime can only lead to self destruction.");
                }
            }

            protected override TextObject IssueQuestSolutionAcceptByPlayer //3-Update quest acceptance text
            {
                get
                {
                    return new TextObject("That much I can do. I will talk with your son.");
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
                
                
                TextObject newLog = new TextObject("{QUESTGIVER.LINK}, a headman from {QUESTGIVERSETTLEMENT.LINK}, has asked you to speak to his son over at {TARGETTOWN.LINK}. {GANGLEADER.LINK} has convinced him to join his crew and his father believes he is way over his head.");
                StringHelpers.SetCharacterProperties("QUESTGIVER", this.QuestGiver.CharacterObject, this.QuestGiver.FirstName, newLog, false);
                StringHelpers.SetSettlementProperties("QUESTGIVERSETTLEMENT", this.QuestGiver.HomeSettlement, newLog, false);
                StringHelpers.SetSettlementProperties("TARGETTOWN", this._targetTown.Settlement, newLog, false);
                StringHelpers.SetCharacterProperties("GANGLEADER", this._gangLeader.CharacterObject, null, newLog, false);

                base.AddLog(newLog);
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

                //if (this._headmansSon == null) 
                //{ 
                //    this.CreateHeadmansSon(); 
                //}
            }

            private LocationCharacter CreateHeadmansSon()
            {
                InformationManager.DisplayMessage(new InformationMessage(this.QuestGiver.Name.ToString()));
                //Hero troop2 = HeroCreator.CreateRelativeNotableHero(this.QuestGiver);

                Hero troop2 = HeroCreator.CreateSpecialHero((from x in CharacterObject.Templates
                                                             where this.QuestGiver.Culture == x.Culture && x.Occupation == Occupation.GangLeader select x).GetRandomElement<CharacterObject>());

                troop2.Name = new TextObject("Notable's son");
                
                this._headmansSon = troop2;
                
                //Hero troop = HeroCreator.CreateSpecialHero(CharacterObject.Templates.GetRandomElement<CharacterObject>(), null, null, null);
                AgentData agent = new AgentData(new SimpleAgentOrigin(troop2.CharacterObject));
                LocationCharacter locChar = new LocationCharacter(agent, new LocationCharacter.AddBehaviorsDelegate(SandBoxManager.Instance.AgentBehaviorManager.AddWandererBehaviors),
                "npc_common", true, LocationCharacter.CharacterRelations.Neutral, "as_human_villager_gangleader", true, false, null, false, true, true);

                return locChar;
                //Location locationwithId = _targetTown.Settlement.LocationComplex.GetLocationWithId("center");
                //locationwithId.AddCharacter(locChar);
            }

            private void BeforeTownEnter()
            {
                if(this._targetTown != null && Settlement.CurrentSettlement == _targetTown.Settlement && this._headmansSon == null)
                {
                    Location locationwithId = Settlement.CurrentSettlement.LocationComplex.GetLocationWithId("tavern");
                    locationwithId.AddCharacter(this.CreateHeadmansSon());
                }
            }
            //Required overrides (abstract)
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
                this.OfferDialogFlow = DialogFlow.CreateDialogFlow("issue_classic_quest_start", 100). //3-Update quest acceptance text
                    NpcLine("Excellent. I don't know how to repay you beyond what money I can muster.").
                        Condition(() => Hero.OneToOneConversationHero == this.QuestGiver).
                        Consequence(QuestAcceptedConsequences).
                    PlayerLine("That will be more than sufficient.").CloseDialog();
                this.DiscussDialogFlow = DialogFlow.CreateDialogFlow("quest_discuss", 100). //3-Update quest acceptance text
                    NpcLine("My son, have you spoke with him yet?").
                        Condition(() => Hero.OneToOneConversationHero == this.QuestGiver).
                    PlayerLine("Still working that.").
                    NpcLine("Please hurry. time is of the essence.");
                Campaign.Current.ConversationManager.AddDialogFlow(initalSonEncounter());
            }

            private DialogFlow initalSonEncounter()
            {
                DialogFlow resultFlow = DialogFlow.CreateDialogFlow("start", 6000).
                    NpcLine("ladie ladie laaaa, just minding my own business").
                        Condition(() => Hero.OneToOneConversationHero == this._headmansSon && !this._initialSonConvoComplete).
                    PlayerLine("Hey, I know you think you're cool, but you're not!!!").
                        
                    NpcLine("Hey, frick off boomer.").GotoDialogState("test_test_testbadboi");

                resultFlow.AddDialogLine("dialogtest", "test_test_testbadboi", "test_output", "yoooo let's go it worked", null, null, this);
                resultFlow.AddPlayerLine("playertest", "test_output", "player_output", "yeaaaa, alright see ya!", null, null, this);
                resultFlow.AddDialogLine("yadayada", "player_output", "let's fight", "no.... fight me!!", null, 
                    new ConversationSentence.OnConsequenceDelegate(fight_son_convo_consequence), this);

                return resultFlow;
            }
            // </Required overrides

            private void fight_son_convo_consequence()
            {
                Campaign.Current.ConversationManager.ConversationEndOneShot += this.PlayerFightsSon;
            }

            private void PlayerFightsSon()
            {
                InformationManager.DisplayMessage(new InformationMessage("made it to the playerfightsondelegate"));
                this._sonAgent = (Agent)MissionConversationHandler.Current.ConversationManager.ConversationAgents.First((IAgent x) =>
                    x.Character != null && x.Character == this._headmansSon.CharacterObject);
                InformationManager.DisplayMessage(new InformationMessage("made it passed searching for the agent, his state is: "+this._sonAgent.State.ToString()));
                //Mission.Current.Agents.First((IAgent x) => x.Character == this._villageThug.CharacterObject);
                
                List<Agent> playerSideAgents = new List<Agent>
                {
                    Agent.Main
                };

                List<Agent> opponentSideAgents = new List<Agent>
                {
                    this._sonAgent
                };

                //MissionFightHandler missionHandler = new MissionFightHandler();
                //missionHandler.AddAgentToSide(this._sonAgent, false);
                //missionHandler.StartCustomFight();
                MissionFightHandler.StartBrawl();
                

                //Mission.Current.GetMissionBehaviour<MissionFightHandler>().StartCustomFight(playerSideAgents, opponentSideAgents, true, false, false,
                //    new MissionFightHandler.OnFightEndDelegate(this.AfterFightAction), false, null, null, null, null);
                //InformationManager.DisplayMessage(new InformationMessage("made it passed start custom fight!"));                
            }

            private void AfterFightAction(bool isplayersidewon)
            {
                if(isplayersidewon)
                {
                    this._playerBeatSon = true;
                    InformationManager.DisplayMessage(new InformationMessage("you won!"));
                    base.CompleteQuestWithSuccess();
                }
            }

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

            [SaveableField(50)]
            public bool _playerBeatSon;

            [SaveableField(60)]
            public Agent _sonAgent;
        }
    }
}
