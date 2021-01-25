using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SandBox.Source.Missions.Handlers;
using TaleWorlds.MountAndBlade;
using TaleWorlds.CampaignSystem;
using SandBox;
using SandBox.Quests;
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

namespace GatherInformation
{
    class GatherInformationBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            
            CampaignEvents.OnCheckForIssueEvent.AddNonSerializedListener(this, new Action<IssueArgs>(this.OnCheckForIssues));
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(this.AddGameMenuItems));
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        
        public void OnCheckForIssues(IssueArgs issueArgs)
        {
            if (ConditionsHold(issueArgs.IssueOwner))
            {
                issueArgs.SetPotentialIssueData(new PotentialIssueData(new Func<PotentialIssueData, Hero, IssueBase>(this.OnStartIssue),
                typeof(GatherInformationBehavior.GatherInformationIssue), IssueBase.IssueFrequency.Common, null));
            }
        }

        private bool ConditionsHold(Hero issueGiver)
        {
            return issueGiver.IsGangLeader && issueGiver.CurrentSettlement.Notables.Any((Hero qg) => issueGiver != qg && qg.IsGangLeader);
        }

        private IssueBase OnStartIssue(PotentialIssueData pid, Hero issueOwner)
        {
            Hero issueTarget = this.GetTargetNotable(issueOwner);
            this._gatherInformationIssue = new GatherInformationBehavior.GatherInformationIssue(issueOwner, issueTarget);
            return this._gatherInformationIssue;
        }
        
        protected void AddGameMenuItems(CampaignGameStarter campaign)
        {
            campaign.AddGameMenu("pb_tavern_drink", "Tavern", new OnInitDelegate(menu_init_delegate));
            campaign.AddGameMenuOption("pb_tavern_drink", "test_drink", "Have a drink",
                new GameMenuOption.OnConditionDelegate(this.Drink_menu_condition),
                new GameMenuOption.OnConsequenceDelegate(this.Drink_menu_consequence), false, 8, false);
            campaign.AddWaitGameMenu("pb_tavern_drink_wait", "Drinking",
                new OnInitDelegate(drink_menu_wait_init), null, null,
                new OnTickDelegate(drink_menu_wait_tick),
                GameMenu.MenuAndOptionType.WaitMenuShowOnlyProgressOption,
                GameOverlays.MenuOverlayType.None, 2f);
        }

        private void PayForDrink()
        {
            GiveGoldAction.ApplyForCharacterToSettlement(Hero.MainHero, Settlement.CurrentSettlement, this._drink_cost, true);

            TextObject leaderName = Campaign.Current.MainParty.Leader.GetName();
            TextObject message = new TextObject(leaderName + " Pays: {GOLD_ICON}{DRINK_COST}");
            message.SetTextVariable("GOLD_ICON", "<img src=\"Icons\\Coin@2x\">");
            message.SetTextVariable("DRINK_COST", _drink_cost);
            InformationManager.DisplayMessage(new InformationMessage(message.ToString()));
        }

        private static void setInputQuest(GatherInformationIssueQuest quest)
        {
            _inputQuest = quest;
        }

        public Hero GetTargetNotable(Hero issueOwner)
        {
            Hero result = null;
            foreach (Hero hero in from x in issueOwner.CurrentSettlement.Notables
                                  where x.IsGangLeader && !x.IsOccupiedByAnEvent() && !x.OwnedCommonAreas.IsEmpty<CommonArea>()
                                  select x)
            {
                if (hero != issueOwner)
                {
                    result = hero;
                    break;
                }
            }

            return result;
        }

        // ----------------Game Menu Delegates----------------

        private void menu_init_delegate(MenuCallbackArgs args)
        {

        }

        private bool Drink_menu_condition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.RansomAndBribe;
            args.Tooltip = new TextObject("Yo, have a drink!"); 
            return true;
        }

        private void Drink_menu_consequence(MenuCallbackArgs args)
        {
            this.PayForDrink();
            _timecontrolstamp = CampaignTime.Now;
            GameMenu.SwitchToMenu("pb_tavern_drink_wait");
        }

        private void drink_menu_wait_init(MenuCallbackArgs args)
        {
            args.MenuContext.GameMenu.SetMenuAsWaitMenuAndInitiateWaiting();
            Campaign.Current.TimeControlMode = CampaignTimeControlMode.UnstoppableFastForwardForPartyWaitTime;
        }

        private void drink_menu_wait_tick(MenuCallbackArgs args, CampaignTime dt)
        {

            if (_timecontrolstamp + CampaignTime.Hours(wait_time) < CampaignTime.Now)
            {
                Campaign.Current.TimeControlMode = CampaignTimeControlMode.Stop;
                
                GameMenu.SwitchToMenu("town");
                InformationManager.ShowInquiry(new InquiryData("A night at the tavern", "After several hours, your target is starting to slur their words...",
                    true, false, "Accept", null, new Action(this.enter_tavern_action), null), true);
            }
        }

        private void enter_tavern_action()
        {
            Location locationWithId = Settlement.CurrentSettlement.LocationComplex.GetLocationWithId("tavern");
            AgentData agentdata = new AgentData(_inputQuest._targetNotable.CharacterObject);
            LocationCharacter locChar = new LocationCharacter(agentdata, new LocationCharacter.AddBehaviorsDelegate(SandBoxManager.Instance.AgentBehaviorManager.AddIndoorWandererBehaviors), 
                "npc_common", true, LocationCharacter.CharacterRelations.Friendly, "as_human_villager_gangleader", true);
            locationWithId.AddCharacter(locChar);
            PlayerEncounter.LocationEncounter.CreateAndOpenMissionController(locationWithId, null, _inputQuest._targetNotable.CharacterObject, null);
            
        }

        private int _drink_cost = 10;

        private float wait_time = 4f;

        private static GatherInformationIssueQuest _inputQuest;

        private CampaignTime _timecontrolstamp;

        private GatherInformationBehavior.GatherInformationIssue _gatherInformationIssue;

        public class GatherInformationIssueTypeDefiner : CampaignBehaviorBase.SaveableCampaignBehaviorTypeDefiner
        {
            public GatherInformationIssueTypeDefiner() : base(404691001)
            {
            }

            protected override void DefineClassTypes()
            {
                AddClassDefinition(typeof(GatherInformationBehavior.GatherInformationIssue), 1);
                AddClassDefinition(typeof(GatherInformationBehavior.GatherInformationIssueQuest), 2);
            }
        }
        internal class GatherInformationIssue : IssueBase
        {
            public GatherInformationIssue(Hero issueOwner, Hero issueTarget) : base(issueOwner, CampaignTime.DaysFromNow(17f))
            {
                this._targetHero = issueTarget;
            }
            protected override bool IsThereAlternativeSolution
            {
                get
                {                    
                    return false;
                }
            }

            protected override bool IsThereLordSolution
            {
                get
                {
                    return false;
                }
            }

            protected override int RewardGold
            {
                get
                {
                    return (int)1000;
                }
            }

            public override TextObject Title
            {
                get
                {
                    return new TextObject("Gather Information");
                }
            }

            public override TextObject Description
            {
                get
                {
                    return new TextObject("Help out a local Gang Leader to get an edge on a rival.");
                }
            }

            protected override void OnGameLoad()
            {
            }

            protected override QuestBase GenerateIssueQuest(string questId)
            {
                InformationManager.DisplayMessage(new InformationMessage("***Quest is generated"));
                
                return new GatherInformationBehavior.GatherInformationIssueQuest(questId, base.IssueOwner, this._targetHero, 
                    CampaignTime.DaysFromNow(17f), RewardGold);
            }

            public override IssueBase.IssueFrequency GetFrequency()
            {
                return IssueBase.IssueFrequency.Common;
            }

            protected override TextObject IssueBriefByIssueGiver
            {
                get
                {
                    TextObject result = new TextObject("You have heard correctly. Trying to keep the streets of {SETTLEMENT.LINK} under control has become increasingly difficult with " +
                        "{TARGET.LINK} and their ilk.");
                    
                    if (this._targetHero != null)
                    {
                        StringHelpers.SetCharacterProperties("TARGET", this._targetHero.CharacterObject, null, result, false);
                        //MBTextManager.SetTextVariable("SETTLEMENT", this.IssueSettlement.ToString());
                        StringHelpers.SetSettlementProperties("SETTLEMENT", this.IssueOwner.HomeSettlement, result);
                    }
                    return result;

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

            public override bool IssueStayAliveConditions()
            {                
                return true;                
            }

            protected override TextObject IssueAcceptByPlayer
            {
                get
                {
                    return new TextObject("What are you needing help with though, I'm not trying to fight a gang war.");
                }
            }

            protected override void CompleteIssueWithTimedOutConsequences()
            {
                
            }

            protected override bool CanPlayerTakeQuestConditions(Hero issueGiver, out PreconditionFlags flag, out Hero relationHero, out SkillObject skill)
            {
                bool flag2 = issueGiver.GetRelationWithPlayer() >= -10;
                flag = (flag2 ? IssueBase.PreconditionFlags.None : IssueBase.PreconditionFlags.Relation);
                relationHero = issueGiver;
                skill = null;
                
                return flag2;
            }

            [SaveableField(10)]
            public Hero _targetHero;
        }

        internal class GatherInformationIssueQuest : QuestBase
        {
            public GatherInformationIssueQuest(string questId, Hero questGiver, Hero questTarget, CampaignTime duration, int rewardGold) : 
                base(questId, questGiver, duration, rewardGold)
            {
                _questcomplete = false;                
                _targetNotable = questTarget;
                this.SetDialogs();
                this.InitializeQuestOnCreation();
                base.AddLog(StartQuestLogText);
            }

            protected override void RegisterEvents()
            {
                CampaignEvents.SettlementEntered.AddNonSerializedListener(this, new Action<MobileParty, Settlement, Hero>(this.OnSettlementEntered));
                CampaignEvents.BeforeMissionOpenedEvent.AddNonSerializedListener(this, new Action (this.OnBeforeMissionOpened));
                CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, new Action(this.OnDailyTick));
            }

            private void OnSettlementEntered(MobileParty party, Settlement settlement, Hero hero)
            {
                if(party != null && this._targetVillage != null && party.IsMainParty && settlement.IsVillage && settlement.Village == this._targetVillage)
                {
                    InformationManager.DisplayMessage(new InformationMessage("hello... welcome to the right village."));
                }

                if(party != null && party.IsMainParty && _questGiverNeedsHelp && settlement.IsTown && settlement == base.QuestGiver.HomeSettlement)
                {
                    InformationManager.ShowInquiry(new InquiryData("A noise from a nearby house..", "As you walk into the town looking for your friend, you hear some yelling from a house.",
                        true, false, "Enter the House", null, new Action(enter_town_action), null), true);
                }
            }

            private void enter_town_action()
            {
                Location locationWithId = Settlement.CurrentSettlement.LocationComplex.GetLocationWithId("house_1");
                AgentData questGiverAgentData = new AgentData(this.QuestGiver.CharacterObject);
                AgentData targetAgentData = new AgentData(this._targetNotable.CharacterObject);
                LocationCharacter locChar = new LocationCharacter(questGiverAgentData, new LocationCharacter.AddBehaviorsDelegate(SandBoxManager.Instance.AgentBehaviorManager.AddWandererBehaviors),
                    "npc_common", true, LocationCharacter.CharacterRelations.Friendly, "as_human_villager_gangleader", true);
                LocationCharacter locChar2 = new LocationCharacter(targetAgentData, new LocationCharacter.AddBehaviorsDelegate(SandBoxManager.Instance.AgentBehaviorManager.AddWandererBehaviors),
                    "npc_common", true, LocationCharacter.CharacterRelations.Friendly, "as_human_villager_gangleader", true);
                locationWithId.AddCharacter(locChar);
                locationWithId.AddCharacter(locChar2);
                InformationManager.DisplayMessage(new InformationMessage(locChar2.Character.Name.ToString()));
                PlayerEncounter.LocationEncounter.CreateAndOpenMissionController(locationWithId, null, this.QuestGiver.CharacterObject, null);
            }

            private void OnBeforeMissionOpened()
            {
                if(this._targetVillage != null && Settlement.CurrentSettlement == this._targetVillage.Settlement)
                {
                    Location locationwithId = Settlement.CurrentSettlement.LocationComplex.GetLocationWithId("village_center");
                    locationwithId.AddCharacter(this.CreateVillageThug());
                }

                
            }

            private void OnDailyTick()
            {
                if(this._waitingToHearFromQuestOwner)
                {
                    InformationManager.DisplayMessage(new InformationMessage("you are waiting now! fun!"));
                    if(this._waitForQGStartTimeStamp.ElapsedDaysUntilNow > 3.0f)
                    {
                        InformationManager.ShowInquiry(new InquiryData("The 'Guards' have arrived..", "A messanger approaches, claiming he works for "+base.QuestGiver.Name.ToString()+" and that they could use your help.", 
                            true, false, "Accept",null,null,null), true);
                        _waitingToHearFromQuestOwner = false;
                        _questGiverNeedsHelp = true;
                    }
                }
                else
                {
                    InformationManager.DisplayMessage(new InformationMessage("you are NOT waiting... sad"));
                }
            }

            protected override void OnCompleteWithSuccess()
            {
                ApplySuccessRewards();
            }

            public override void OnFailed()
            {
                base.OnFailed();
            }

            protected override void OnFinalize()
            {
                this._questcomplete = true;
                base.OnFinalize();
                CampaignEvents.RemoveListeners(this);
            }

            public void BarterSuccess()
            {
                base.CompleteQuestWithSuccess();
            }
            private LocationCharacter CreateVillageThug()
            {
                Hero troop2 = HeroCreator.CreateSpecialHero(Extensions.GetRandomElement<CharacterObject>(from x in CharacterObject.Templates
                                                                                                         where x.Occupation == Occupation.Outlaw select x));
                troop2.Name = new TextObject("Suspicious Merchant");
                
                Hero troop = HeroCreator.CreateSpecialHero(CharacterObject.Templates.GetRandomElement<CharacterObject>(), null, null, null);
                AgentData agent = new AgentData(new SimpleAgentOrigin (troop2.CharacterObject));
                LocationCharacter locChar = new LocationCharacter(agent, new LocationCharacter.AddBehaviorsDelegate(SandBoxManager.Instance.AgentBehaviorManager.AddWandererBehaviors),
                "npc_common", true, LocationCharacter.CharacterRelations.Friendly, "as_human_villager_gangleader", true, false, null, false, true, true);
                _villageThug = troop2;

                return locChar;
            }
            

            public TextObject StartQuestLogText
            {
                get {
                    TextObject result = new TextObject("You agreed to help {QG.LINK} find out about {TARGET.LINK}'s plans. Wait till nightfall to try and convince " +
                        "{TARGET.LINK} for a round at the Tavern. Perhaps they will be clumpsy enough to spill the beans...");
                    StringHelpers.SetCharacterProperties("TARGET", _targetNotable.CharacterObject, null, result);
                    StringHelpers.SetCharacterProperties("QG", QuestGiver.CharacterObject, null, result);
                    return result;
                }
            }

            public override bool IsRemainingTimeHidden
            {
                get
                {
                    return false;
                }
            }

            public override TextObject Title
            {
                get
                {
                    return new TextObject("Gathering Information");
                }
            }
            protected override void InitializeQuestOnGameLoad()
            {
                _questcomplete = false;
                this.SetDialogs();

            }

            protected override void SetDialogs()
            {
                this.OfferDialogFlow = DialogFlow.CreateDialogFlow("issue_classic_quest_start", 100).
                    NpcLine("Good, now begone!!", null, null).
                    Condition(() => Hero.OneToOneConversationHero == this.QuestGiver && !this._questcomplete).
                    Consequence(QuestAcceptedConsequences).CloseDialog();
                this.DiscussDialogFlow = DialogFlow.CreateDialogFlow("quest_discuss", 100).NpcLine("get to work, dude!").Condition(()
                    => Hero.OneToOneConversationHero == this.QuestGiver && !this._questcomplete);

                Campaign.Current.ConversationManager.AddDialogFlow(initialTargetInteraction());
                Campaign.Current.ConversationManager.AddDialogFlow(tavernPersuasionAttempt());
                Campaign.Current.ConversationManager.AddDialogFlow(VillageThugDialogFlow());
                Campaign.Current.ConversationManager.AddDialogFlow(QuestGiverAfterTavernSuccess());
                Campaign.Current.ConversationManager.AddDialogFlow(QuestGiverNeedsHelp());
            }

            private DialogFlow initialTargetInteraction()
            {
                TextObject playerText = new TextObject("I'm feeling rather parched, would you care to accompany me to the Tavern?");
                TextObject npcText = new TextObject("Well I- I'm not sure..");
                DialogFlow initiateWithTarget = DialogFlow.CreateDialogFlow("hero_main_options", 1000).
                    PlayerLine(playerText).
                        Condition(() => (Hero.OneToOneConversationHero == _targetNotable) && !(CampaignMission.Current.Location.StringId == "tavern")).
                    NpcLine(npcText).BeginNpcOptions().
                    NpcOption("Sorry, I'm a bit busy now. Maybe after nightfall", 
                        new ConversationSentence.OnConditionDelegate(is_day_condition)).
                        CloseDialog().
                    NpcOption("And why would I go with you?",
                        new ConversationSentence.OnConditionDelegate(is_night_condition)).EndNpcOptions().
                    PlayerLine("Cuz it would be fun!").
                    NpcLine("Fair enough... let's go!").
                        Consequence(notable_drink_success_consequence);

                return initiateWithTarget;
            }

            
            private DialogFlow tavernPersuasionAttempt()
            {
                DetermineQuestVillage();
                TextObject questTargetVillageText = new TextObject("{VILLAGE}");
                    questTargetVillageText.SetTextVariable("VILLAGE", this._targetVillage.Settlement.EncyclopediaLinkWithName);
                TextObject questGiverText = new TextObject("{QG.LINK}");
                    StringHelpers.SetCharacterProperties("QG", base.QuestGiver.CharacterObject,null,questGiverText);
                DialogFlow result = DialogFlow.CreateDialogFlow("start", 150).
                    NpcLine("Aaahahaha, you're absolutely right... Oh my, I may have overindulged.").
                        Condition(() => Hero.OneToOneConversationHero == this._targetNotable && !this._questcomplete && CampaignMission.Current.Location.StringId == "tavern").
                    PlayerLine("So how are things with the business?").
                    NpcLine("Well let's just business is about to improve dramatically, Once my shipment of gaurd uniforms arrive.").
                    NpcLine("Wait, I'm getting ahead of myself..").
                    BeginPlayerOptions().
                        PlayerOption("No, please go on. I'd love to hear more.").GotoDialogState("pb_drink_asked").GoBackToDialogState("pb_notable_drink_success_output").
                            NpcLine("So anyways, my colleague at "+ questTargetVillageText + " is waiting for the brigands I hired to deliver the uniforms.").CloseDialog().
                        PlayerOption("Sorry, but I'll have to pass.").EndPlayerOptions().CloseDialog();

                result.AddDialogLine("pb_notable_drinks_persuasion_start", "pb_drink_asked", "pb_drink_continue", "Take no offense but why should I trust you?",null,
                    new ConversationSentence.OnConsequenceDelegate(begin_persuasion_consequence), this, 150, null);
                //^Persuasion starts

                result.AddDialogLine("pb_notable_drinks_options", "pb_drink_continue", "pb_persuasion_options", "Well?",
                    new ConversationSentence.OnConditionDelegate(notable_drink_persuasion_in_progress_condition), null, this, 100, null);

                //Player Persuasions
                result.AddPlayerLine("pb_drink_persuasion_start", "pb_persuasion_options", "pb_persuasion_attempt", "Between you and I, I'd give anything to see "+questGiverText+"'s head on a spike.", null,
                    new ConversationSentence.OnConsequenceDelegate(ConsequencePersuasionOption1), this, 100,
                    new ConversationSentence.OnClickableConditionDelegate(ClickableConditionPersuasion1),
                    new ConversationSentence.OnPersuasionOptionDelegate(SetPersuasionOption1));
                result.AddPlayerLine("pb_drink_persuasion_start2", "pb_persuasion_options", "pb_persuasion_attempt", "Frankly I wish to learn from you. I don't mean to pry, I'm just eager.", null,
                    new ConversationSentence.OnConsequenceDelegate(ConsequencePersuasionOption2), this, 100,
                    new ConversationSentence.OnClickableConditionDelegate(ClickableConditionPersuasion2),
                    new ConversationSentence.OnPersuasionOptionDelegate(SetPersuasionOption2));
                result.AddPlayerLine("pb_drink_persuasion_start2", "pb_persuasion_options", "pb_persuasion_attempt", "I meant not to press you on it, relax. Just having a conversation.", null,
                    new ConversationSentence.OnConsequenceDelegate(ConsequencePersuasionOption3), this, 100,
                    new ConversationSentence.OnClickableConditionDelegate(ClickableConditionPersuasion3),
                    new ConversationSentence.OnPersuasionOptionDelegate(SetPersuasionOption3));

                //NPC responses
                result.AddDialogLine("pb_npc_persuasion_success", "pb_persuasion_attempt", "pb_drink_continue", "{PERSUASION_REACTION}",
                    new ConversationSentence.OnConditionDelegate(persuasion_attempted_condition), null, this, 100, null);

                //Success!!
                result.AddDialogLine("pb_notable_persuaded_to_drink", "pb_drink_continue", "pb_notable_drink_success_output", "ha, of course, think nothing of my hesitation. I blame the spinning room..",
                    new ConversationSentence.OnConditionDelegate(ConversationManager.GetPersuasionProgressSatisfied),
                    new ConversationSentence.OnConsequenceDelegate(tavern_target_notable_persuassion_success), this, 200, null);
                

                //Failure...
                result.AddDialogLine("pb_notable_drinks_persuasion_failed", "pb_drink_continue", "pb_notable_drinks_fail_output", "No, no it wouldn't interest you anyways. I've probably had engough to drink.",
                    new ConversationSentence.OnConditionDelegate(notable_drink_fail_condition),
                    new ConversationSentence.OnConsequenceDelegate(tavern_target_notable_persuassion_failed), this, 50, null);
                
                return result;
            }

            private DialogFlow QuestGiverAfterTavernSuccess()
            {
                DialogFlow result = DialogFlow.CreateDialogFlow("start", 150).
                    NpcLine("Well, any luck yet?").
                        Condition(() => Hero.OneToOneConversationHero == this.QuestGiver && tavernPerssuasionFailed).
                    PlayerLine("Somewhat, I found out that they plan to sneak in some guard uniforms into the city! I couldn't get any more out of him though.").
                    NpcLine("I see.. well we'll have to figure that out, huh?").Consequence(() => tavernPerssuasionFailed = false).
                    NpcLine("How about this, I'll lay low until my men notice new gaurds, then I'll send word for you.").
                        Consequence(new ConversationSentence.OnConsequenceDelegate(start_wait_for_quest_giver_consequence)).CloseDialog();

                return result;
            }

            private DialogFlow QuestGiverNeedsHelp()
            {
                DialogFlow result = DialogFlow.CreateDialogFlow("start", 150).
                    NpcLine("Ahhhh help meee.").
                        Condition(() => Hero.OneToOneConversationHero == this.QuestGiver && _questGiverNeedsHelp).
                        Consequence(new ConversationSentence.OnConsequenceDelegate(qg_needs_help_init_consequence)).
                    NpcLine("Heeeere I am!", new ConversationSentence.OnMultipleConversationConsequenceDelegate(this.IsTargetNotable),
                    new ConversationSentence.OnMultipleConversationConsequenceDelegate(this.IsQuestGiver)).CloseDialog();

                return result;
            }
            
            private DialogFlow VillageThugDialogFlow()
            {
                DialogFlow result = DialogFlow.CreateDialogFlow("start", 150).
                    NpcLine("Well stranger, can I help you with anything.").
                        Condition(() => this._villageThug != null && Hero.OneToOneConversationHero == this._villageThug && !this._questcomplete).
                    PlayerLine("Well how about you start by telling me where the good stuff is?").
                    NpcLine("It's in your thoughts!").
                    NpcLine("Why do you think I'd betray my dude.").
                    GotoDialogState("pb_village_thug_start");
                //Options start
                result.AddDialogLine("pb_npc_village_thug_ID", "pb_village_thug_start", "pb_vilalge_thug_player_options", "How could I possibly do such a thing", null,null,this);

                //Persuade!
                result.AddPlayerLine("pb_village_thug_optionsID_two", "pb_vilalge_thug_player_options", "pb_village_thug_option_two", "I'll convince you!", 
                    new ConversationSentence.OnConditionDelegate(village_thug_player_can_attempt_persuasion), null, this);
                result.AddDialogLine("pb_npc_village_thug_persuade_option", "pb_village_thug_option_two", "pb_npc_persuade_option_output", "Oh yea? go ahead and try lol.", null, 
                    new ConversationSentence.OnConsequenceDelegate(init_village_persuasion_consequence), this);
                result.AddDialogLine("pb_village_thug_persuasion_start_id", "pb_npc_persuade_option_output", "pb_village_thug_persuasion_options", "I'm listening..", 
                    new ConversationSentence.OnConditionDelegate(village_persuasion_options_condition), null, this);
                
                result.AddDialogLine("pb_village_thug_persuasion_response_id", "pb_player_option_output", "pb_npc_persuade_option_output", "I see..", null, null, this);

                result.AddPlayerLine("pb_village_persuasion_attempt_one", "pb_village_thug_persuasion_options", "pb_player_option_output", "What if I told you they an asshole", null, 
                    new ConversationSentence.OnConsequenceDelegate(vilalge_persuasion_option_one_consequence), this, 100, 
                    new ConversationSentence.OnClickableConditionDelegate(village_persuasion_option_one_clickable_condition), 
                    new ConversationSentence.OnPersuasionOptionDelegate(village_persuasion_option_one_persuasion));
                result.AddPlayerLine("pb_village_persuasion_attempt_two", "pb_village_thug_persuasion_options", "pb_player_option_output", "You don't want to be doing this", null,
                    new ConversationSentence.OnConsequenceDelegate(vilalge_persuasion_option_two_consequence), this, 100,
                    new ConversationSentence.OnClickableConditionDelegate(village_persuasion_option_two_clickable_condition),
                    new ConversationSentence.OnPersuasionOptionDelegate(village_persuasion_option_two_persuasion));
                result.AddPlayerLine("pb_village_persuasion_attempt_three", "pb_village_thug_persuasion_options", "pb_player_option_output", "Just like, be better, my dude", null,
                    new ConversationSentence.OnConsequenceDelegate(vilalge_persuasion_option_three_consequence), this, 100,
                    new ConversationSentence.OnClickableConditionDelegate(village_persuasion_option_three_clickable_condition),
                    new ConversationSentence.OnPersuasionOptionDelegate(village_persuasion_option_three_persuasion));

                result.AddDialogLine("pb_village_persuasion_failed", "pb_npc_persuade_option_output", "pb_village_thug_start", "Sorry, but NO", 
                    new ConversationSentence.OnConditionDelegate(village_persuasion_fail_condition), 
                    new ConversationSentence.OnConsequenceDelegate(village_persuasion_fail_consequence), this, 150);
                result.AddDialogLine("pb_village_persuasion_success", "pb_npc_persuade_option_output", "pb_village_thug_persuasion_success_end", "Ya know, maybe you have a point.", 
                    new ConversationSentence.OnConditionDelegate(village_persuasion_success_condition), 
                    new ConversationSentence.OnConsequenceDelegate(village_persuasion_success_consequence), this, 151);

                //Barter!
                result.AddPlayerLine("pb_village_thug_optionsID_three", "pb_vilalge_thug_player_options", "pb_village_thug_option_three", "I'll barter for it!", null, null, this);
                result.AddDialogLine("pb_npc_village_thug_barter_option", "pb_village_thug_option_three", "pb_npc_barter_option_output", "Ha! this is going to have to be some nice butter.", null, null, this);
                result.AddPlayerLine("pb_village_thug_barter_id", "pb_npc_barter_option_output", "pb_barter_starter","Oh... it will be.", null, null, this);
                result.AddDialogLine("pb_village_thug_bartering_id", "pb_barter_starter", "pb_barter_complete", "bartering", null,
                    new ConversationSentence.OnConsequenceDelegate(village_persuasion_barter_starter), this);
                result.AddDialogLine("pb_npc_village_thug_barter_complete_id", "pb_barter_starter", "pb_barter_complete", "it was a pleasure doing business with you.", null,
                    new ConversationSentence.OnConsequenceDelegate(village_thug_barter_option_complete), this);

                //Fight!
                result.AddPlayerLine("pb_village_thug_optionsID_one", "pb_vilalge_thug_player_options", "pb_village_thug_option_one", "I'll fight you!", null, null, this);
                result.AddDialogLine("pb_npc_village_thug_fight_option", "pb_village_thug_option_one", "pb_npc_fight_option_output", "wow, you asshole", null,
                    new ConversationSentence.OnConsequenceDelegate(player_fights_village_thug_consequence), this);

                //Give up...
                result.AddPlayerLine("pb_village_thug_optionsID_quit", "pb_vilalge_thug_player_options", "pb_village_thug_option_quit", "This isn't worth the effort.", null, null, this);
                result.AddDialogLine("pb_village_thug_optionsID_quit_response", "pb_village_thug_option_quit", "pb_village_thug_end_convo", "Now that's the idea. Move along now.", null,
                    new ConversationSentence.OnConsequenceDelegate(player_gives_up_on_thug), this, 100, null);


                return result;
            }

            // delegates --- temp location
            private void init_village_persuasion_consequence()
            {
                this._thugPersuadeAttempted = true;
                ConversationManager.StartPersuasion(4, 1, 0f, 2f, 3f, 0f, PersuasionDifficulty.Medium);

                _task = new PersuasionTask(0);
                TextObject Line = new TextObject("I suppose...");
                PersuasionOptionArgs option1 = new PersuasionOptionArgs(DefaultSkills.Charm, DefaultTraits.Generosity,
                    TraitEffect.Positive, PersuasionArgumentStrength.Normal, false, Line);
                this._task.AddOptionToTask(option1);
                Line = new TextObject("I suppose...");
                PersuasionOptionArgs option2 = new PersuasionOptionArgs(DefaultSkills.Leadership, DefaultTraits.Mercy,
                    TraitEffect.Positive, PersuasionArgumentStrength.Normal, false, Line);
                this._task.AddOptionToTask(option2);
                Line = new TextObject("I suppose...");
                PersuasionOptionArgs option3 = new PersuasionOptionArgs(DefaultSkills.Roguery, DefaultTraits.Valor,
                    TraitEffect.Positive, PersuasionArgumentStrength.Normal, false, Line);
                this._task.AddOptionToTask(option3);
            }

            private bool village_persuasion_options_condition()
            {
                return !ConversationManager.GetPersuasionProgressSatisfied() && !ConversationManager.GetPersuasionIsFailure() 
                    && !_task.Options.All((PersuasionOptionArgs x) => x.IsBlocked);
            }

            private void vilalge_persuasion_option_one_consequence()
            {
                this._task.Options[0].BlockTheOption(true);
            }

            private bool village_persuasion_option_one_clickable_condition(out TextObject hintText)
            {
                hintText = new TextObject("no no no!", null);
                if (_task.Options.Any<PersuasionOptionArgs>())
                {
                    hintText = TextObject.Empty;
                    return !_task.Options[0].IsBlocked;
                }
                return false;
            }

            private PersuasionOptionArgs village_persuasion_option_one_persuasion()
            {
                return this._task.Options.ElementAt(0);
            }

            private void vilalge_persuasion_option_two_consequence()
            {
                this._task.Options[1].BlockTheOption(true);
            }

            private bool village_persuasion_option_two_clickable_condition(out TextObject hintText)
            {
                hintText = new TextObject("no no no!", null);
                if (_task.Options.Any<PersuasionOptionArgs>())
                {
                    hintText = TextObject.Empty;
                    return !_task.Options[1].IsBlocked;
                }
                return false;
            }

            private PersuasionOptionArgs village_persuasion_option_two_persuasion()
            {
                return this._task.Options.ElementAt(1);
            }

            private void vilalge_persuasion_option_three_consequence()
            {
                this._task.Options[2].BlockTheOption(true);
            }

            private bool village_persuasion_option_three_clickable_condition(out TextObject hintText)
            {
                hintText = new TextObject("no no no!", null);
                if (_task.Options.Any<PersuasionOptionArgs>())
                {
                    hintText = TextObject.Empty;
                    return !_task.Options[2].IsBlocked;
                }
                return false;
            }

            private PersuasionOptionArgs village_persuasion_option_three_persuasion()
            {
                return this._task.Options.ElementAt(2);
            }

            private bool village_persuasion_fail_condition()
            {
                return _task.Options.All((PersuasionOptionArgs x) => x.IsBlocked);
            }

            private void village_persuasion_fail_consequence()
            {
                ConversationManager.EndPersuasion();
            }

            private bool village_persuasion_success_condition()
            {
                return ConversationManager.GetPersuasionProgressSatisfied();
            }

            private void village_persuasion_success_consequence()
            {
                ConversationManager.EndPersuasion();
                base.CompleteQuestWithSuccess();
            }
            //End temp delegate section

            private void ApplyFailureRewards()
            {
                ChangeRelationAction.ApplyPlayerRelation(this.QuestGiver, -10);
            }

            private void ApplySuccessRewards()
            {
                ChangeRelationAction.ApplyPlayerRelation(this.QuestGiver, 10);
            }

            public void InitPersuasionTask()
            {
                _task = new PersuasionTask(0);
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

            private void DetermineQuestVillage()
            {
                //find a bound settlement that is not raided and track it. Also initialize an enter settlement event?
                foreach(Village v in _targetNotable.HomeSettlement.BoundVillages)
                {
                    if(v.VillageState != Village.VillageStates.BeingRaided && v.VillageState != Village.VillageStates.Looted)
                    {
                        _targetVillage = v;
                        break;
                    }
                }
            }

            private void PlayerFightsVillageThug()
            {
                Agent thug = (Agent)MissionConversationHandler.Current.ConversationManager.ConversationAgents.First((IAgent x) =>
                    x.Character != null && x.Character == this._villageThug.CharacterObject);

                //Mission.Current.Agents.First((IAgent x) => x.Character == this._villageThug.CharacterObject);

                List<Agent> playerSideAgents = new List<Agent>
                {
                    Agent.Main
                };

                List<Agent> opponentSideAgents = new List<Agent>
                {
                    thug
                };

                Mission.Current.GetMissionBehaviour<MissionFightHandler>().StartCustomFight(playerSideAgents, opponentSideAgents, true, false, false, 
                    new MissionFightHandler.OnFightEndDelegate(this.AfterFightAction));
            }
            //--------------------Delegates--------------------
            private void QuestAcceptedConsequences()
            {
                base.StartQuest();
            }
            
            protected bool is_target_notable_condition()
            {
                InformationManager.DisplayMessage(new InformationMessage("target condition fires"));
                return Hero.OneToOneConversationHero == _targetNotable;
            }

            private bool notable_drink_persuasion_in_progress_condition()
            {
                return !ConversationManager.GetPersuasionProgressSatisfied() && !ConversationManager.GetPersuasionIsFailure() && !_task.Options.All((PersuasionOptionArgs x) => x.IsBlocked);
            }

            private bool persuasion_attempted_condition()
            {
                PersuasionOptionResult result = ConversationManager.GetPersuasionChosenOptions().Last<Tuple<PersuasionOptionArgs, PersuasionOptionResult>>().Item2;
                MBTextManager.SetTextVariable("PERSUASION_REACTION", "ya ya, ok continue.", false);
                return true;
            }

            private bool notable_drink_fail_condition()
            {
                return _task.Options.All((PersuasionOptionArgs x) => x.IsBlocked);
            }

            private void begin_persuasion_consequence()
            {
                ConversationManager.StartPersuasion(2, 1, 0f, 2f, 3f, 0f, PersuasionDifficulty.Medium);
                
                this.InitPersuasionTask();
            }

            private bool is_night_condition()
            {
                return CampaignTime.Now.IsNightTime && !(CampaignMission.Current.Location.StringId == "tavern"); 
            }

            private bool is_day_condition()
            {
                return !CampaignTime.Now.IsNightTime;
            }

            private void notable_drink_success_consequence()
            {
                ConversationManager.EndPersuasion();
                foreach (QuestBase q in Campaign.Current.QuestManager.Quests)
                {
                    if(q is GatherInformationIssueQuest)
                    { 
                        if (q.Id == this.Id)
                        {
                            GatherInformationBehavior.setInputQuest((GatherInformationIssueQuest)q);
                        
                        }
                    }
                }
                Campaign.Current.GameMenuManager.SetNextMenu("pb_tavern_drink");
                Campaign.Current.ConversationManager.ConversationEndOneShot +=
                Mission.Current.EndMission;
                //ApplySuccessRewards();
                //base.CompleteQuestWithSuccess();
            }

            private void notable_drink_fail_consequence()
            {
                ConversationManager.EndPersuasion();
                ApplyFailureRewards();
                base.CompleteQuestWithFail(new TextObject("You failed to convince the target to head to the Tavern."));
            }

            private PersuasionOptionArgs SetPersuasionOption1()
            {
                return this._task.Options.ElementAt(0);
            }

            private PersuasionOptionArgs SetPersuasionOption2()
            {
                return this._task.Options.ElementAt(1);
            }

            private PersuasionOptionArgs SetPersuasionOption3()
            {
                return this._task.Options.ElementAt(2);
            }

            private void ConsequencePersuasionOption1()
            {
                this._task.Options[0].BlockTheOption(true);
            }

            private void ConsequencePersuasionOption2()
            {
                this._task.Options[1].BlockTheOption(true);
            }

            private void ConsequencePersuasionOption3()
            {
                this._task.Options[2].BlockTheOption(true);
            }

            private bool ClickableConditionPersuasion1(out TextObject hintText)
            {
                hintText = new TextObject("no no no!", null);
                if (_task.Options.Any<PersuasionOptionArgs>())
                {
                    hintText = TextObject.Empty;
                    return !_task.Options[0].IsBlocked;
                }
                return false;
            }

            private bool ClickableConditionPersuasion2(out TextObject hintText)
            {
                hintText = new TextObject("no no no!", null);
                if (_task.Options.Any<PersuasionOptionArgs>())
                {
                    hintText = TextObject.Empty;
                    return !_task.Options[1].IsBlocked;
                }
                return false;
            }

            private bool ClickableConditionPersuasion3(out TextObject hintText)
            {
                hintText = new TextObject("no no no!", null);
                if (_task.Options.Any<PersuasionOptionArgs>())
                {
                    hintText = TextObject.Empty;
                    return !_task.Options[2].IsBlocked;
                }
                return false;
            }

            private void target_tavern_persuasion_success_consequence()
            {

            }

            private void tavern_target_notable_persuassion_success()
            {
                //process finding / determining a bound village
                ConversationManager.EndPersuasion();
                
                base.AddTrackedObject(_targetVillage.Settlement);
                TextObject logAdd = new TextObject("{!TARGET.LINK} was stupid enough to brag of his plans to have a shipment of guard uniforms delivered to his colleague in the Village of {VILLAGE.LINK}.");                
                StringHelpers.SetSettlementProperties("VILLAGE", _targetVillage.Settlement);
                StringHelpers.SetCharacterProperties("TARGET", _targetNotable.CharacterObject);
                base.AddLog(logAdd);
            }

            private void player_fights_village_thug_consequence()
            {
                Campaign.Current.ConversationManager.ConversationEndOneShot += this.PlayerFightsVillageThug;
            }

            private void player_gives_up_on_thug()
            {
                CompleteQuestWithFail();
            }

            private bool village_thug_player_can_attempt_persuasion()
            {
                return !_thugPersuadeAttempted;
            }

            private void AfterFightAction(bool isplayersidewon)
            {
                if(isplayersidewon)
                {
                    base.CompleteQuestWithSuccess();
                }
                else
                {
                    base.CompleteQuestWithFail();
                }
            }

            private void village_persuasion_barter_starter()
            {
                BarterManager barterManager = BarterManager.Instance;
                Hero mainHero = Hero.MainHero;
                Hero OneToOneConversationHero = Hero.OneToOneConversationHero;
                PartyBase mainParty = PartyBase.MainParty;
                //PartyBase otherParty = Hero.OneToOneConversationHero.PartyBelongedTo.Party;
                Barterable[] barters = new Barterable[1];
                barters[0] = new GatherInformationBarterable(OneToOneConversationHero, null, this);
                barterManager.StartBarterOffer(mainHero, OneToOneConversationHero, mainParty, null, null, null, 0, false, barters);
            }

            private void tavern_target_notable_persuassion_failed()
            {
                tavernPerssuasionFailed = true;
                ConversationManager.EndPersuasion();
                base.AddLog(new TextObject("The Target has told you of a plot sneak Guard uniforms into the city! Go back and talk to ya friend!"));
            }

            private void village_thug_barter_option_complete()
            {
                //Campaign.Current.ConversationManager.EndConversation();
            }

            private void start_wait_for_quest_giver_consequence()
            {
                base.AddLog(new TextObject("QG has told you to lay low for the time being, he will send word for you when needed."));
                _waitingToHearFromQuestOwner = true;
                _waitForQGStartTimeStamp = CampaignTime.Now;
            }

            private void qg_needs_help_init_consequence()
            {
                List<Agent> agentsToAdd = new List<Agent>();
                foreach (Agent agent in Mission.Current.Agents)
                {
                    if(agent.Character == this._targetNotable.CharacterObject)
                    {
                        agentsToAdd.Add(agent);
                        Campaign.Current.ConversationManager.AddConversationAgents(agentsToAdd, true);
                        break;
                    }
                    
                }
                
            }

            private bool IsMainHero(IAgent agent)
            {
                return agent.Character == CharacterObject.PlayerCharacter;
            }

            private bool IsQuestGiver(IAgent agent)
            {
                return agent.Character == this.QuestGiver.CharacterObject;
            }

            private bool IsTargetNotable(IAgent agent)
            {
                return agent.Character == this._targetNotable.CharacterObject;
            }

            //vars
            private PersuasionTask _task;
            private bool _questcomplete;
            
            private bool _isQuestTargetVillage;
            
            private bool _thugPersuadeAttempted;
            private bool _thugBarterAttempt;

            [SaveableField(10)]
            public Hero _targetNotable;

            [SaveableField(20)]
            private Village _targetVillage;

            [SaveableField(30)]
            private Hero _villageThug;

            [SaveableField(40)]
            private bool tavernPerssuasionFailed;

            [SaveableField(50)]
            private bool _waitingToHearFromQuestOwner;

            [SaveableField(60)]
            private CampaignTime _waitForQGStartTimeStamp;

            [SaveableField(70)]
            private bool _questGiverNeedsHelp;
        }
    }
}
