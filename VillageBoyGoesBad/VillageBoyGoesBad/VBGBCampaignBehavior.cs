﻿using System;
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
                    CampaignTime.DaysFromNow(10f), RewardGold);
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
                this.SetGameMenus();
                this.relationGainReward = 10;
                
                TextObject newLog = new TextObject("{QUESTGIVER.LINK}, a headman from {QUESTGIVERSETTLEMENT.LINK}, has asked you to speak to his son over at {TARGETTOWN.LINK}. {GANGLEADER.LINK} has convinced him to join his crew and his father believes he is way over his head.");
                StringHelpers.SetCharacterProperties("QUESTGIVER", this.QuestGiver.CharacterObject, this.QuestGiver.FirstName, newLog, false);
                StringHelpers.SetSettlementProperties("QUESTGIVERSETTLEMENT", this.QuestGiver.HomeSettlement, newLog, false);
                StringHelpers.SetSettlementProperties("TARGETTOWN", this._targetTown.Settlement, newLog, false);
                StringHelpers.SetCharacterProperties("GANGLEADER", this._gangLeader.CharacterObject, null, newLog, false);

                
                base.AddLog(newLog);                                
            }

            protected override void InitializeQuestOnGameLoad()
            {
                this.SetDialogs();
                this.SetGameMenus();
            }

            protected override void RegisterEvents()
            {
                base.RegisterEvents();
                CampaignEvents.SettlementEntered.AddNonSerializedListener(this, new Action<MobileParty, Settlement, Hero>(this.EnterTargetSettlement));
                CampaignEvents.BeforeMissionOpenedEvent.AddNonSerializedListener(this, new Action(this.BeforeTownEnter));
                CampaignEvents.OnMissionStartedEvent.AddNonSerializedListener(this, new Action<IMission>(this.OnMissionStarted));
            }

            private void SetGameMenus()
            {
                base.AddGameMenu("pb_vbgb_son_unconsious", new TextObject("After a few moments, Notable's son stands up and comes to his senses."), null);
                base.AddGameMenuOption("pb_vbgb_son_unconsious", "pb_vbgb_son_unconsious_opiton", new TextObject("Talk with notables son"), 
                    delegate(MenuCallbackArgs args)
                    {
                        args.optionLeaveType = GameMenuOption.LeaveType.Conversation;
                        return true;
                    }, 
                    delegate {                        
                        Location locationwithId = Settlement.CurrentSettlement.LocationComplex.GetLocationWithId("tavern");
                        locationwithId.AddCharacter(this.createSonLocCharacter(this._headmansSon));
                        PlayerEncounter.LocationEncounter.CreateAndOpenMissionController(Settlement.CurrentSettlement.LocationComplex.GetLocationWithId("tavern"),
                 null, _headmansSon.CharacterObject);
                    });
                base.AddGameMenu("pb_vbgb_player_unconsious", new TextObject("After a few moments, you come to your senses."), null);
                base.AddGameMenuOption("pb_vbgb_player_unconsious", "pb_vbgb_player_unconsious_opiton", new TextObject("Talk with notables son"),
                    delegate (MenuCallbackArgs args)
                    {
                        args.optionLeaveType = GameMenuOption.LeaveType.Conversation;
                        return true;
                    },
                    delegate {
                        Location locationwithId = Settlement.CurrentSettlement.LocationComplex.GetLocationWithId("tavern");
                        locationwithId.AddCharacter(this.createSonLocCharacter(this._headmansSon));
                        PlayerEncounter.LocationEncounter.CreateAndOpenMissionController(Settlement.CurrentSettlement.LocationComplex.GetLocationWithId("tavern"),
                 null, _headmansSon.CharacterObject);
                    });
            }

            private void OnMissionStarted(IMission iMission)
            {
                Mission.Current.AddMissionBehaviour(new VBGBMissionFightHandler());
                Mission.Current.RemoveMissionBehaviour(Mission.Current.GetMissionBehaviour<MissionFightHandler>());
                Mission.Current.RemoveMissionBehaviour(Mission.Current.GetMissionBehaviour<LeaveMissionLogic>());
                Mission.Current.MissionBehaviours.ForEach(x => InformationManager.DisplayMessage(new InformationMessage(x.ToString())));
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

            private LocationCharacter createHeadmansSon()
            {
                InformationManager.DisplayMessage(new InformationMessage(this.QuestGiver.Name.ToString()));
                //Hero troop2 = HeroCreator.CreateRelativeNotableHero(this.QuestGiver);

                Hero son = HeroCreator.CreateSpecialHero((from x in CharacterObject.Templates
                                                             where this.QuestGiver.Culture == x.Culture && 
                                                             x.Occupation == Occupation.Wanderer &&
                                                             !x.IsFemale
                                                             select x).GetRandomElement<CharacterObject>());
                son.Name = new TextObject("Notable's son");

                this._headmansSon = son;

                return this.createSonLocCharacter(son);
                //Hero troop = HeroCreator.CreateSpecialHero(CharacterObject.Templates.GetRandomElement<CharacterObject>(), null, null, null);
                //AgentData agent = new AgentData(new SimpleAgentOrigin(son.CharacterObject));
                //LocationCharacter locChar = new LocationCharacter(agent, new LocationCharacter.AddBehaviorsDelegate(SandBoxManager.Instance.AgentBehaviorManager.AddWandererBehaviors),
                //"npc_common", true, LocationCharacter.CharacterRelations.Friendly, "as_human_villager_gangleader", true, false, null, false, true, true);
                //this._headmansSon.CivilianEquipment.AddEquipmentToSlotWithoutAgent(EquipmentIndex.WeaponItemBeginSlot, new EquipmentElement(MBObjectManager.Instance.GetObject<ItemObject>("short_sword_t3"), null));
                //return locChar;
            }

            private LocationCharacter createSonLocCharacter(Hero son)
            {
                AgentData agent = new AgentData(new SimpleAgentOrigin(son.CharacterObject));
                LocationCharacter locChar = new LocationCharacter(agent, new LocationCharacter.AddBehaviorsDelegate(SandBoxManager.Instance.AgentBehaviorManager.AddWandererBehaviors),
                "npc_common", true, LocationCharacter.CharacterRelations.Friendly, "as_human_villager_gangleader", true, false, null, false, true, true);
                this._headmansSon.CivilianEquipment.AddEquipmentToSlotWithoutAgent(EquipmentIndex.WeaponItemBeginSlot, new EquipmentElement(MBObjectManager.Instance.GetObject<ItemObject>("short_sword_t3"), null));
                return locChar;
            }

            private LocationCharacter createGangMember()
            {
                //InformationManager.DisplayMessage(new InformationMessage(this.QuestGiver.Name.ToString()));
                //Hero troop2 = HeroCreator.CreateRelativeNotableHero(this.QuestGiver);

                Hero hero = HeroCreator.CreateSpecialHero((from x in CharacterObject.Templates
                                                          where this.QuestGiver.Culture == x.Culture && 
                                                            x.Occupation == Occupation.GangLeader &&
                                                            !x.IsFemale
                                                          select x).GetRandomElement<CharacterObject>());

                hero.Name = new TextObject("Gang Member");

                this._gangMember1 = hero;

                //Hero troop = HeroCreator.CreateSpecialHero(CharacterObject.Templates.GetRandomElement<CharacterObject>(), null, null, null);
                AgentData agent = new AgentData(new SimpleAgentOrigin(hero.CharacterObject));
                LocationCharacter locChar = new LocationCharacter(agent, new LocationCharacter.AddBehaviorsDelegate(SandBoxManager.Instance.AgentBehaviorManager.AddWandererBehaviors),
                "npc_common", true, LocationCharacter.CharacterRelations.Neutral, "as_human_villager_gangleader", true, false, null, false, true, true);

                return locChar;
            }

            private void BeforeTownEnter()
            {
                if(this._targetTown != null && Settlement.CurrentSettlement == _targetTown.Settlement && this._headmansSon == null)
                {                    
                    Location locationwithId = Settlement.CurrentSettlement.LocationComplex.GetLocationWithId("tavern");
                    locationwithId.AddCharacter(this.createHeadmansSon());
                    locationwithId.AddCharacter(this.createGangMember());
                    locationwithId.AddCharacter(this.createGangMember());
                }
            }            
            
            //Required overrides (abstract)
            public override TextObject Title => new TextObject("A Rouge in the Making");

            public override bool IsRemainingTimeHidden => false;

            
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
                Campaign.Current.ConversationManager.AddDialogFlow(playerTeamWonFightDialog());
            }

            private DialogFlow initalSonEncounter()
            {
                DialogFlow resultFlow = DialogFlow.CreateDialogFlow("start", 6000).
                    NpcLine("Hello stranger, can I help you?").
                        Condition(() => Hero.OneToOneConversationHero == this._headmansSon && !this._initialSonConvoComplete && !this._playerTeamWon).
                    BeginPlayerOptions().
                        PlayerOption("Yes you can, name. Your father sent me").
                        PlayerOption("You don't know what you're getting into").EndPlayerOptions().
                    NpcLine("Don't talk to me like a child. You sound like my father").
                    BeginPlayerOptions().
                        PlayerOption("I don't mean to but please just listen to me.").
                            NpcLine("If you promise to make it quick fine.").
                            NpcLine("Go on... what is it.").
                                Consequence(new ConversationSentence.OnConsequenceDelegate(son_persuasion_delegate_init)).GotoDialogState("pb_vbgb_son_persuassion").
                    PlayerOption("You're coming home with me, now. Come on, don't make this difficult.").EndPlayerOptions().
                    NpcLine("What? what do you think you're trying to pull?!");

                resultFlow.AddDialogLine("pb_vbgb_son_convo", "pb_vbgb_son_persuassion", "pb_vbgb_son_persuassion_options", "Well?", null, null, this);
                resultFlow.AddDialogLine("pb_vbgb_son_reaction", "pb_vbgb_son_persuassion_attempt", "pb_vbgb_son_persuassion", "{PERSUASION_REACTION}", null, null, this);

                resultFlow.AddPlayerLine("pb_vbgb_son_persuassion_player_option_1", "pb_vbgb_son_persuassion_options", "pb_vbgb_son_persuassion_attempt", "player option 1",null, 
                    delegate { this._task.Options[0].BlockTheOption(true); }, this, 100, 
                    new ConversationSentence.OnClickableConditionDelegate(persuasion_option_clickable));
                resultFlow.AddPlayerLine("pb_vbgb_son_persuassion_player_option_2", "pb_vbgb_son_persuassion_options", "pb_vbgb_son_persuassion_attempt", "player option 2", null, null, this);
                resultFlow.AddPlayerLine("pb_vbgb_son_persuassion_player_option_3", "pb_vbgb_son_persuassion_options", "pb_vbgb_son_persuassion_attempt", "player option 3", null, null, this);


                resultFlow.AddDialogLine("dialogtest", "test_test_testbadboi", "test_output", "yoooo let's go it worked", null, null, this);
                resultFlow.AddPlayerLine("playertest", "test_output", "player_output", "yeaaaa, alright see ya!", null, null, this);
                resultFlow.AddDialogLine("yadayada", "player_output", "begin_fight", "no.... fight me!!", null, 
                    new ConversationSentence.OnConsequenceDelegate(fight_son_convo_consequence), this);

                return resultFlow;
            }

            private void son_persuasion_delegate_init()
            {
                ConversationManager.StartPersuasion(2, 1, 0f, 2f, 3f, 0f, PersuasionDifficulty.Medium);
                this._task = new PersuasionTask(0);

                TextObject Line = new TextObject("I suppose...");
                PersuasionOptionArgs option1 = new PersuasionOptionArgs(DefaultSkills.Charm, DefaultTraits.Mercy,
                    TraitEffect.Positive, PersuasionArgumentStrength.Normal, false, Line);
                this._task.AddOptionToTask(option1);

                Line = new TextObject("I suppose...");
                PersuasionOptionArgs option2 = new PersuasionOptionArgs(DefaultSkills.Leadership, DefaultTraits.Mercy,
                    TraitEffect.Positive, PersuasionArgumentStrength.Normal, false, Line);
                this._task.AddOptionToTask(option2);

                Line = new TextObject("I suppose...");
                PersuasionOptionArgs option3 = new PersuasionOptionArgs(DefaultSkills.Roguery, DefaultTraits.Mercy,
                    TraitEffect.Positive, PersuasionArgumentStrength.Normal, false, Line);
                this._task.AddOptionToTask(option3);
            }

            private bool persuasion_option_clickable(out TextObject hintText)
            {
                hintText = new TextObject("no no no!", null);
                if (this._task.Options.Any<PersuasionOptionArgs>())
                {
                    hintText = TextObject.Empty;
                    return !this._task.Options[0].IsBlocked;
                }
                return false;
            }

            private DialogFlow playerTeamWonFightDialog()
            {
                DialogFlow resultDialog = DialogFlow.CreateDialogFlow("start", 6000).
                    NpcLine("Oh my god we did it...").
                        Condition(() => Hero.OneToOneConversationHero == this._headmansSon && this._playerTeamWon).
                    PlayerLine("So you'll go back to daddy, yea?").
                    NpcLine("dolphinately bro", null, null).
                        Consequence(delegate
                        { 
                            Campaign.Current.ConversationManager.ConversationEndOneShot += this.vicotry_conversation_consequence;
                            
                            //PlayerEncounter.LeaveSettlement();
                        }).CloseDialog(); //GotoDialogState("close_window");

                return resultDialog;
            }
            // </Required overrides

            private void vicotry_conversation_consequence()
            {                                                
                Mission.Current.SetMissionMode(MissionMode.StartUp, false);
                //GameMenu.SwitchToMenu("town");
                base.CompleteQuestWithSuccess();
            }

            private void fight_son_convo_consequence()
            {
                Campaign.Current.ConversationManager.ConversationEndOneShot += this.PlayerFightsGang;
            }

            private void PlayerFightsGang()
            {                
                InformationManager.DisplayMessage(new InformationMessage("made it to the playerfightsondelegate"));
                this._sonAgent = (Agent)MissionConversationHandler.Current.ConversationManager.ConversationAgents.First((IAgent x) =>
                    x.Character != null && x.Character == this._headmansSon.CharacterObject);

                //Agent gangMemberAgent = (Agent)Mission.Current.GetNearbyAgents(Agent.Main.Position.AsVec2, 100f);
                InformationManager.DisplayMessage(new InformationMessage("made it passed searching for the agent, his state is: "+this._sonAgent.State.ToString()));
                //Mission.Current.Agents.First((IAgent x) => x.Character == this._villageThug.CharacterObject);
                
                List<Agent> playerSideAgents = new List<Agent>
                {
                    Agent.Main,
                    this._sonAgent
                };

                List<Agent> opponentSideAgents = new List<Agent>();                
                foreach(Agent agent in Mission.Current.GetNearbyAgents(Agent.Main.Position.AsVec2, 100f))
                {
                    if(agent.Name == "Gang Member")
                    {
                        opponentSideAgents.Add(agent);
                    }
                }

                InformationManager.DisplayMessage(new InformationMessage(Mission.Current.GetMissionBehaviour<VBGBMissionFightHandler>().ToString()));
                                              
                Mission.Current.GetMissionBehaviour<VBGBMissionFightHandler>().StartCustomFight(playerSideAgents, opponentSideAgents, false, false, false,
                    new VBGBMissionFightHandler.OnFightEndDelegate(this.AfterFightAction), true, null, null, null, null);
                                
                InformationManager.DisplayMessage(new InformationMessage("made it passed start custom fight!"));                
            }

            private void AfterFightAction(bool isplayersidewon)
            {
                if(isplayersidewon)
                {
                    bool playerActive = Mission.Current.MainAgent != null && Mission.Current.MainAgent.State == AgentState.Active;
                    bool sonActive = this._sonAgent != null && this._sonAgent.State == AgentState.Active;
                    this._playerTeamWon = true;
                    
                    if(!playerActive || !sonActive) //agents are down
                    {
                        Mission.Current.NextCheckTimeEndMission = 0f;
                        Mission.Current.EndMission();
                        if(playerActive)
                        {
                            Campaign.Current.GameMenuManager.SetNextMenu("pb_vbgb_son_unconsious");
                        }
                        else
                        {
                            Campaign.Current.GameMenuManager.SetNextMenu("pb_vbgb_player_unconsious");
                        }
                        //playerActive == true ? Campaign.Current.GameMenuManager.SetNextMenu("pb_vbgb_son_unconsious") : Campaign.Current.GameMenuManager.SetNextMenu("pb_vbgb_player_unconsious");
                        
                        return;
                    } 
                        else
                    {
                        InformationManager.DisplayMessage(new InformationMessage("you won! Stats: " + Mission.Current.MainAgent.State + Mission.Current.MainAgent.Team + ", and your partner: " + this._sonAgent.State));
                        Campaign.Current.ConversationManager.SetupAndStartMissionConversation(this._sonAgent, Mission.Current.MainAgent, false);
                        return;
                    }
                    //if(Mission.Current.MainAgent.State != AgentState.Active || this._sonAgent.State != AgentState.Active)
                    //{                    
                    //InformationManager.ShowInquiry(new InquiryData("test", "content", false, false, null, null, null, null)); //could do a game menu instead
                    //PlayerEncounter.LeaveSettlement();
                    //PlayerEncounter.
                    //LeaveSettlementAction.ApplyForCharacterOnly(Hero.MainHero);
                    //return;
                    //this._sonAgent.State = AgentState.Active;
                    //}
                    //base.CompleteQuestWithSuccess();

                } else if (!isplayersidewon)
                {
                    this._playerTeamWon = false;
                    Mission.Current.NextCheckTimeEndMission = 0f;
                    Mission.Current.EndMission();
                    Campaign.Current.GameMenuManager.SetNextMenu("settlement_player_unconscious");                    
                    InformationManager.DisplayMessage(new InformationMessage("you lost..."));
                    base.CompleteQuestWithFail();
                    return;
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
                InformationManager.DisplayMessage(new InformationMessage("OnCanceled has fired"));
                base.OnCanceled();
            }
            public override void OnFailed()
            {
                base.AddLog(new TextObject("you did NOT do it..."));
                ChangeRelationAction.ApplyPlayerRelation(this.QuestGiver, -this.relationGainReward);
                this.QuestGiver.AddPower(-20f);
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
                base.AddLog(new TextObject("you did it!!"));
                GainRenownAction.Apply(Hero.MainHero, 5f);
                ChangeRelationAction.ApplyPlayerRelation(this.QuestGiver, this.relationGainReward);
                GiveGoldAction.ApplyBetweenCharacters(this.QuestGiver, Hero.MainHero, 1000);
                this.QuestGiver.AddPower(20f);
                base.OnCompleteWithSuccess();
            }
            protected override void OnFinalize()
            {
                InformationManager.DisplayMessage(new InformationMessage("OnFinalize has fired"));
                if (this._playerTeamWon)
                { Campaign.Current.GameMenuManager.SetNextMenu("town"); }
                base.OnFinalize();
            }
            protected override void OnStartQuest()
            {
                InformationManager.DisplayMessage(new InformationMessage("OnStartQuest has fired"));
                base.OnStartQuest();
            }
            protected override void OnTimedOut()
            {
                base.AddLog(new TextObject("you did NOT do it... TOO SLOW"));
                ChangeRelationAction.ApplyPlayerRelation(this.QuestGiver, -this.relationGainReward);
                this.QuestGiver.AddPower(-20f);
                base.OnTimedOut();
            }
            // </Optional Overrides

            // <Delegates
            private void QuestAcceptedConsequences()
            {
                base.StartQuest();
            }

            //Properties
            private PersuasionTask _task;

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
            public bool _playerTeamWon;

            [SaveableField(60)]
            public Agent _sonAgent;

            [SaveableField(70)]
            public Hero _gangMember1;

            [SaveableField(80)]
            public Hero _gangMember2;

            [SaveableField(90)]
            public int relationGainReward;
        }
    }
}
