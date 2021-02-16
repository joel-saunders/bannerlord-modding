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
    class VBGBQuest : QuestBase
    {
        public VBGBQuest(string questId, Hero questGiver, Town targetTown, Hero gangLeader, bool friendsWithGang, CampaignTime duration, int rewardGold) : base(questId, questGiver, duration, rewardGold)
        {            
            this.CreateQuestNPCs();
            this._gangLeader = gangLeader;
            this._targetTown = targetTown;
            this.SetDialogs();
            this.InitializeQuestOnCreation();
            this.SetGameMenus();
            this._relationGainReward = 10;
            this._gangRelationSufficient = friendsWithGang;
            this._goldReward = rewardGold;            

            TextObject newLog = new TextObject("{QUESTGIVER.LINK}, a headman from {QUESTGIVERSETTLEMENT.LINK}, has asked you to speak to his son over at {TARGETTOWN.LINK}. {GANGLEADER.LINK} has convinced him to join his crew and his father believes he is way over his head. In exchange, " + QuestGiver.FirstName + " will pay you " + base.RewardGold + " for your help.");
            StringHelpers.SetCharacterProperties("QUESTGIVER", this.QuestGiver.CharacterObject, this.QuestGiver.FirstName, newLog, false);
            StringHelpers.SetSettlementProperties("QUESTGIVERSETTLEMENT", this.QuestGiver.HomeSettlement, newLog, false);
            StringHelpers.SetSettlementProperties("TARGETTOWN", this._targetTown.Settlement, newLog, false);
            StringHelpers.SetCharacterProperties("GANGLEADER", this._gangLeader.CharacterObject, null, newLog, false);

            base.AddTrackedObject(this._targetTown.Settlement);
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
            CampaignEvents.WarDeclared.AddNonSerializedListener(this, new Action<IFaction, IFaction>(this.OnWarDeclared));
            CampaignEvents.VillageBeingRaided.AddNonSerializedListener(this, new Action<Village>(this.OnVillageRaid));
            CampaignEvents.SettlementEntered.AddNonSerializedListener(this, new Action<MobileParty, Settlement, Hero>(this.EnterTargetSettlement));
            CampaignEvents.BeforeMissionOpenedEvent.AddNonSerializedListener(this, new Action(this.BeforeTownEnter));
            CampaignEvents.OnMissionStartedEvent.AddNonSerializedListener(this, new Action<IMission>(this.OnMissionStarted));
            CampaignEvents.AfterDailyTickEvent.AddNonSerializedListener(this, new Action(this.OnAfterDailyTick));
            //CampaignEvents.HeroRelationChanged.AddNonSerializedListener(this, new Action<Hero, Hero, int, bool>(this.OnHeroRelationChanged));
        }

        private void OnWarDeclared(IFaction faction1, IFaction faction2)
        {
            if (Hero.MainHero.MapFaction.IsAtWarWith(base.QuestGiver.CurrentSettlement.MapFaction))
            {
                base.AddLog(new TextObject("canceled due to WARRRR"));
                base.CompleteQuestWithCancel(null);
            }
        }

        private void OnVillageRaid(Village village)
        {
            if (base.QuestGiver.CurrentSettlement == village.Settlement)
            {
                base.AddLog(new TextObject("canceled due to Raiding :)"));
                base.CompleteQuestWithCancel(null);
            }
        }
        private void SetGameMenus()
        {
            base.AddGameMenu("pb_vbgb_son_unconsious", new TextObject("After a few moments, " + this._headmansSon.FirstName + "stands up and comes to his senses."), null);
            base.AddGameMenuOption("pb_vbgb_son_unconsious", "pb_vbgb_son_unconsious_opiton", new TextObject("Talk with notables son"),
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
            //Mission.Current.RemoveMissionBehaviour(Mission.Current.GetMissionBehaviour<MissionFightHandler>());
            if (this._tavernLocationId != null &&
                Hero.MainHero.CurrentSettlement != null &&
                Hero.MainHero.CurrentSettlement.LocationComplex.GetLocationOfCharacter(Hero.MainHero) ==
                Hero.MainHero.CurrentSettlement.LocationComplex.GetLocationWithId("tavern") && 
                this._tavernLocationId == Settlement.CurrentSettlement.LocationComplex.GetLocationWithId("tavern"))
            {
                VBGBMissionFightHandler missionBehavior = new VBGBMissionFightHandler(new Action<Agent, int>(this.OnAgentHit));
                Mission.Current.AddMissionBehaviour(missionBehavior);
                Mission.Current.RemoveMissionBehaviour(Mission.Current.GetMissionBehaviour<LeaveMissionLogic>());
                Mission.Current.MissionBehaviours.ForEach(x => InformationManager.DisplayMessage(new InformationMessage(x.ToString())));
            }
            
            //InformationManager.DisplayMessage(new InformationMessage(Mission.Current.MainAgent.GetWieldedItemIndex(Agent.HandIndex.MainHand).ToString()));
        }

        private void OnAfterDailyTick()
        {
            if (this._companionPathChosen && this._companionSolutionCompletionDate.IsPast && !base.IsOngoing)
            {
                this.CompleteQuestWithCompanionSolution();
                
            }
        }

        private void CompleteQuestWithCompanionSolution()
        {
            InformationManager.DisplayMessage(new InformationMessage("It worked! jfdoiasjfoapds"));
            MobileParty.MainParty.MemberRoster.AddToCounts(this._rogueCompanionForAlternativeSolution.CharacterObject, 1);
            base.CompleteQuestWithSuccess();
        }

        private void OnHeroRelationChanged(Hero player, Hero gangLeader, int relationChange, bool idkyet)
        {
            InformationManager.DisplayMessage(new InformationMessage("first Hero arg: " + player.Name + ", second Hero arg: " + gangLeader.Name));
            if (player == Hero.MainHero || gangLeader == Hero.MainHero)
            {
                InformationManager.DisplayMessage(new InformationMessage("first Hero arg: " + player.Name + ", second Hero arg: " + gangLeader.Name));
            }
        }

        private void CreateQuestNPCs()
        {
            this.createHeadmansSon();
            this._gangMemberLocChar1 = this.createGangMember();
            this._gangMemberLocChar2 = this.createGangMember();
        }

        private void OnAgentHit(Agent agentHit, int damage)
        {
            InformationManager.DisplayMessage(new InformationMessage("Son has been hit"));
            if (agentHit == this._sonAgent && this._playerBeatSon && base.IsOngoing && agentHit.Health <= (float)damage + 70f)
            {
                Mission.Current.GetMissionBehaviour<MissionFightHandler>().EndFight();
                if (this._sonAgent.Position.DistanceSquared(Agent.Main.Position) < 35)
                {
                    Campaign.Current.ConversationManager.SetupAndStartMissionConversation(this._sonAgent, Mission.Current.MainAgent, false);
                    return;
                }
                else
                {
                    Mission.Current.SetMissionMode(MissionMode.StartUp, false);
                    base.CompleteQuestWithSuccess();
                }


            }
        }
        private void EnterTargetSettlement(MobileParty party, Settlement settlement, Hero hero)
        {
            if (party != null && party.IsMainParty && settlement != null && settlement.Town == this._targetTown)
            {
                InformationManager.DisplayMessage(new InformationMessage("the headman's son must be here"));
            }
        }

        private void createHeadmansSon()
        {
            //Hero troop2 = HeroCreator.CreateRelativeNotableHero(this.QuestGiver);

            Hero son = HeroCreator.CreateSpecialHero((from x in CharacterObject.Templates
                                                      where this.QuestGiver.Culture == x.Culture &&
                                                      x.Occupation == Occupation.Wanderer &&
                                                      !x.IsFemale
                                                      select x).GetRandomElement<CharacterObject>());
            TextObject newName = new TextObject(QuestGiver.FirstName.ToString() + "'s son, " + son.FirstName);
            son.Name = newName;

            this._headmansSon = son;
            this._headmansSonLocChar = this.createSonLocCharacter(son);
            //return this._headmansSonLocChar;
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
            "npc_common", true, LocationCharacter.CharacterRelations.Friendly, "as_human_villager_in_tavern", true, false, null, false, true, true);
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

            //this._gangMember1 = hero;

            //Hero troop = HeroCreator.CreateSpecialHero(CharacterObject.Templates.GetRandomElement<CharacterObject>(), null, null, null);
            AgentData agent = new AgentData(new SimpleAgentOrigin(hero.CharacterObject));
            LocationCharacter locChar = new LocationCharacter(agent, new LocationCharacter.AddBehaviorsDelegate(SandBoxManager.Instance.AgentBehaviorManager.AddWandererBehaviors),
            "npc_common", true, LocationCharacter.CharacterRelations.Neutral, "as_human_villager_gangleader", true, false, null, false, true, true);

            return locChar;
        }

        private void BeforeTownEnter()
        {
            if (this._targetTown != null && Settlement.CurrentSettlement == this._targetTown.Settlement)
            {
                if (this._headmansSon == null)
                {
                    this._tavernLocationId = Settlement.CurrentSettlement.LocationComplex.GetLocationWithId("tavern");                    
                }
                else if (this._headmansSon != null)
                {
                    InformationManager.DisplayMessage(new InformationMessage("It fires: " + this._headmansSonLocChar.ToString()));
                    this._tavernLocationId = Settlement.CurrentSettlement.LocationComplex.GetLocationWithId("tavern");
                    
                }
                this._tavernLocationId.AddCharacter(this._headmansSonLocChar);
                this._tavernLocationId.AddCharacter(this._gangMemberLocChar1);
                this._tavernLocationId.AddCharacter(this._gangMemberLocChar2);
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
            Campaign.Current.ConversationManager.AddDialogFlow(playerBeatupSonDialog());
            Campaign.Current.ConversationManager.AddDialogFlow(gangLeaderDiscussion());
        }

        private DialogFlow gangLeaderDiscussion()
        {
            //TextObject npcCostLine = new TextObject("Sure, but it'll cost ya. {GOLD_COST} {GOLD_ICON}");
            //npcCostLine.SetTextVariable("GOLD_COST", this.gangLeaderPayoffNeeded.ToString());
            //npcCostLine.SetTextVariable("GOLD_ICON", "{=!}<img src=\"Icons\\Coin@2x\">");
            //DialogFlow resultFlow = DialogFlow.CreateDialogFlow("hero_main_options", 6000).BeginPlayerOptions().
            //    PlayerOption("hey hey, could I have your new guy?").
            //        Condition(() => Hero.OneToOneConversationHero == this._gangLeader && base.IsOngoing).
            //        ClickableCondition(new ConversationSentence.OnClickableConditionDelegate(gang_leader_altern_path_clickable_delegate)).BeginNpcOptions().
            //    NpcOption(npcCostLine, () => this.gangLeaderPayoffNeeded > 0).BeginPlayerOptions().
            //        PlayerOption("Fine, take it.").Consequence(delegate { base.CompleteQuestWithSuccess(); }).NpcLine("it's a deal! Take the brat away").CloseDialog().
            //        PlayerOption("on second thought, it's not worth it").NpcLine("Fine by me.").NpcLine("Anything else?").GotoDialogState("hero_main_options").
            //    NpcOption("Naa, I don't like you", ()=> this.gangLeaderPayoffNeeded <= 0).PlayerLine("Absolutely! Thank you").Consequence(delegate { base.CompleteQuestWithSuccess(); });
            //return resultFlow;


            TextObject npcCostLine = new TextObject("If you cover the cost of {GOLD_COST} {GOLD_ICON} the boy can go free. Alternatively, if you have a companion that could help me out...");
            //npcCostLine.SetTextVariable("GOLD_COST", this.gangLeaderPayoffNeeded.ToString());
            //npcCostLine.SetTextVariable("GOLD_ICON", "{=!}<img src=\"Icons\\Coin@2x\">");

            DialogFlow resultFlow = DialogFlow.CreateDialogFlow("hero_main_options", 600).
                PlayerLine("I would like to discuess " + this._headmansSon.FirstName + " and his position in your businesses?").Condition(() => Hero.OneToOneConversationHero == this._gangLeader && !this._companionPathChosen && base.IsOngoing).Consequence(setup_available_companions_consequence_delegate).BeginNpcOptions().
                NpcOption("Sure. It will however cost you, one way or the other.", () => this._gangLeader.GetRelationWithPlayer() >= -10 && this._gangLeader.GetRelationWithPlayer() < 20).
                    NpcLine(npcCostLine).BeginPlayerOptions().
                        PlayerOption("I do have a companion whose services I could lend to you.").ClickableCondition(player_has_companion_rouge_enough).
                            NpcLine("I'll need them for 5 days. Who is it that you're sending me?").GotoDialogState("pb_alternative_solution_rogue_companion_list").GoBackToDialogState("pb_alternative_solution_rogue_companion_list_RETURN").

                            NpcLine("Of course, I'll have my men tell him he isn't fit for this life").Consequence(alternative_solution_begins).NpcLine("Anything Else?").GotoDialogState("hero_main_options").
                        PlayerOption("We have a deal. I'll pay and the son walks free.").ClickableCondition(player_can_afford_gang_leader_payoff).
                            NpcLine("Excellent, he's all yours. Happy doing business with you.").Consequence(player_completes_quest_by_payment).
                            NpcLine("Anything else I can help you with, friend?").GotoDialogState("hero_main_options").
                        PlayerOption("No, I'm not willing to pay that.").NpcLine("Fine by me.").NpcLine("Anything Else?").GotoDialogState("hero_main_options").
                NpcOption("Naaaa, I don't like you", () => this._gangLeader.GetRelationWithPlayer() < -10).NpcLine("Anything else?").GotoDialogState("hero_main_options");


            return resultFlow;
        }

        private void alternative_solution_begins()
        {
            base.AddLog(new TextObject("You sent " + this._rogueCompanionForAlternativeSolution.Name.ToString() + " to help " + this._gangLeader.Name.ToString() + " in order to have " + this.QuestGiver.Name + "'s son, " + this._headmansSon.FirstName + ", released. This should take around 3 days."));
            this._rogueCompanionForAlternativeSolution.AddEventForOccupiedHero(base.StringId);
            this._rogueCompanionForAlternativeSolution.ChangeState(Hero.CharacterStates.Disabled);
            if (this.QuestDueTime < CampaignTime.Days(3f))
            {
                this.ChangeQuestDueTime(CampaignTime.Days(4f));
                //add log of saying time was extended
            }
            this._companionPathChosen = true;
            this._companionSolutionCompletionDate = CampaignTime.DaysFromNow(3f);

            //if(this._rogueCompanionForAlternativeSolution.)

            Location companionCharLocation = LocationComplex.Current.GetLocationOfCharacter(this._rogueCompanionForAlternativeSolution);
            LocationCharacter companionLocChar = companionCharLocation.GetLocationCharacter(this._rogueCompanionForAlternativeSolution);

            MobileParty.MainParty.MemberRoster.AddToCounts(this._rogueCompanionForAlternativeSolution.CharacterObject, -1);
            
            //MobileParty.MainParty.MemberRoster.RemoveTroop
            LocationComplex.Current.ChangeLocation(companionLocChar, companionCharLocation, null);
        }
        private void player_completes_quest_by_payment()
        {
            GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, this._gangLeader, this.gangLeaderPayoffNeeded);
            base.CompleteQuestWithSuccess();
        }
        private bool player_has_companion_rouge_enough(out TextObject explanation)
        {
            //explanation = new TextObject("you got enough");
            //return true;
            InformationManager.DisplayMessage(new InformationMessage("1"));
            bool flag = false;
            explanation = new TextObject("You do not have a companion high with enough rogeury skill.");
            this._eligibleCompanionsForRogeury = new List<Hero>();

            InformationManager.DisplayMessage(new InformationMessage("2"));
            foreach (TroopRosterElement troop in MobileParty.MainParty.MemberRoster)
            {
                if (troop.Character.IsHero && !troop.Character.HeroObject.IsOccupiedByAnEvent() && troop.Character.GetSkillValue(DefaultSkills.Roguery) > 30)
                {
                    this._eligibleCompanionsForRogeury.Add(troop.Character.HeroObject);
                    if (!flag)
                    {
                        flag = true;
                        explanation = new TextObject("You have a companion with enough Rogeury skill.");
                    }
                }
            }
            InformationManager.DisplayMessage(new InformationMessage("3"));
            if (this._eligibleCompanionsForRogeury.Count > 0)
            {
                //this.CreateDialogsForEachAvailableRogueCompanions(this._eligibleCompanionsForRogeury);
            }
            InformationManager.DisplayMessage(new InformationMessage("4"));
            return flag;
        }
        private void setup_available_companions_consequence_delegate()
        {
            this._eligibleCompanionsForRogeury = new List<Hero>();
            foreach (TroopRosterElement troop in MobileParty.MainParty.MemberRoster)
            {
                if (troop.Character.IsHero && !troop.Character.HeroObject.IsOccupiedByAnEvent() && troop.Character.GetSkillValue(DefaultSkills.Roguery) > 30)
                {
                    this._eligibleCompanionsForRogeury.Add(troop.Character.HeroObject);
                }
            }
            if (this._eligibleCompanionsForRogeury.Count > 0)
            {
                this.CreateDialogsForEachAvailableRogueCompanions(this._eligibleCompanionsForRogeury);
            }
        }
        private void CreateDialogsForEachAvailableRogueCompanions(List<Hero> rogueCompanions)
        {
            foreach (Hero companion in rogueCompanions)
            {
                this._currentCompanion = companion.GetSkillValue(DefaultSkills.Roguery).ToString();
                DialogFlow dialog = DialogFlow.CreateDialogFlow("pb_alternative_solution_rogue_companion_list", 1000).GoBackToDialogState("pb_alternative_solution_rogue_companion_list").
                    PlayerLine(companion.Name.ToString()).ClickableCondition(clickable_companion_skill_explanation).Consequence(delegate { this._rogueCompanionForAlternativeSolution = companion; }).NpcLine("Works for me.").PlayerLine("And " + this._headmansSon.FirstName + " is sent home right?").GotoDialogState("pb_alternative_solution_rogue_companion_list_RETURN");

                Campaign.Current.ConversationManager.AddDialogFlow(dialog);
            }

            DialogFlow endDialog = DialogFlow.CreateDialogFlow("pb_alternative_solution_rogue_companion_list", 500).GoBackToDialogState("pb_alternative_solution_rogue_companion_list").
                PlayerLine("Actually I'll need to recondisder lending anyone to you for now.").NpcLine("Doesn't make a differnce to me. Just don't waste my time.").NpcLine("Anything Else?").GotoDialogState("hero_main_options");
            Campaign.Current.ConversationManager.AddDialogFlow(endDialog);
        }

        private string _currentCompanion;
        private bool clickable_companion_skill_explanation(out TextObject explanation)
        {

            explanation = new TextObject("Roguery Skill: "+ this._currentCompanion);
            return true;
        }
        private bool player_can_afford_gang_leader_payoff(out TextObject explanation)
        {
            if (Hero.MainHero.Gold >= this.gangLeaderPayoffNeeded)
            {
                explanation = new TextObject("You can afford the amount.");
                return true;
            }

            explanation = new TextObject("You don't have enough gold.");
            return false;
        }

        private bool gang_leader_altern_path_clickable_delegate(out TextObject explanation)
        {
            if (this._gangLeader.GetRelationWithPlayer() >= -10)
            {
                explanation = new TextObject("Your relation is high enough!");
                return true;
            }
            else
            {
                explanation = new TextObject("You are not close enough friends to ask.");
                return false;
            }

        }

        private DialogFlow initalSonEncounter()
        {
            DialogFlow resultFlow = DialogFlow.CreateDialogFlow("start", 6000).
                BeginNpcOptions().
                    NpcOption("Hello stranger, can I help you?", (() => Hero.OneToOneConversationHero == this._headmansSon && !this._initialSonConvoComplete && !this._playerTeamWon && this._sonPersuasionFailed == false && !this._playerBeatSon)).GotoDialogState("pb_vbgb_soniniticonvo").
                    NpcOption("You again, look you're not going to change my mind so save us both the time and leave.", () => Hero.OneToOneConversationHero == this._headmansSon && this._sonPersuasionFailed == true && !this._playerBeatSon).CloseDialog().GoBackToDialogState("pb_vbgb_soniniticonvo").
                BeginPlayerOptions().
                    PlayerOption("Yes you can, name. Your father sent me").GotoDialogState("pb_vbgb_player_options_end_1").
                    PlayerOption("You don't know what you're getting into").GotoDialogState("pb_vbgb_player_options_end_1").GoBackToDialogState("pb_vbgb_player_options_end_1").
                BeginNpcOptions().
                    NpcOption("Don't talk to me like a child. You sound like my father", () => !this._sonPersuasionFailed).GotoDialogState("pb_vbgb_player_options2").
                    NpcOption("I've found my people, and you're not changing that!", () => this._sonPersuasionFailed).GotoDialogState("pb_vbgb_player_options2").GoBackToDialogState("pb_vbgb_player_options2").
                BeginPlayerOptions().
                    PlayerOption("I don't mean to but please just listen to me.").Condition(() => !this._sonPersuasionFailed).
                        NpcLine("If you promise to make it quick fine.").
                        NpcLine("Go on... what is it.").
                            Consequence(new ConversationSentence.OnConsequenceDelegate(son_persuasion_delegate_init)).GotoDialogState("pb_vbgb_son_persuasion").
                    PlayerOption("You're coming home with me, now. Come on, don't make this difficult.").
                        NpcLine("What? what do you think you're trying to pull?!").
                        PlayerLine("Fine, I'll kick your ass then.").Consequence(new ConversationSentence.OnConsequenceDelegate(player_fights_son_delegate)).CloseDialog().
                    PlayerOption("this isn't worth it... your father can keep his money, you're not worth the trouble.").
                        Condition(() => this._sonPersuasionFailed).
                        Consequence(new ConversationSentence.OnConsequenceDelegate(player_gives_up)).CloseDialog().
                    PlayerOption("Maybe I'll just have a word with " + _gangLeader.EncyclopediaLinkWithName + " and see if I can't convince them.").CloseDialog();


            resultFlow.AddDialogLine("pb_vbgb_son_convo", "pb_vbgb_son_persuasion", "pb_vbgb_son_persuasion_options", "Well?",
                delegate { return !ConversationManager.GetPersuasionProgressSatisfied() && !ConversationManager.GetPersuasionIsFailure() && !_task.Options.All((PersuasionOptionArgs x) => x.IsBlocked); },
                null, this);
            resultFlow.AddDialogLine("pb_vbgb_son_reaction", "pb_vbgb_son_persuasion_attempt", "pb_vbgb_son_persuasion", "{PERSUASION_REACTION}",
                delegate {
                    PersuasionOptionResult result = ConversationManager.GetPersuasionChosenOptions().Last<Tuple<PersuasionOptionArgs, PersuasionOptionResult>>().Item2;
                    MBTextManager.SetTextVariable("PERSUASION_REACTION", "Go on..", false);
                    return true;
                }, null, this);

            resultFlow.AddPlayerLine("pb_vbgb_son_persuasion_player_option_1", "pb_vbgb_son_persuasion_options", "pb_vbgb_son_persuasion_attempt", "Your father loves you, " + this._headmansSon.Name + ". I know it doesn't always seem like it but he just wants you safe and home with family.", null,
                delegate { this._task.Options[0].BlockTheOption(true); }, this, 100,
                new ConversationSentence.OnClickableConditionDelegate(persuasion_option_clickable_1),
                new ConversationSentence.OnPersuasionOptionDelegate(persuasion_option_persuasion_1));
            resultFlow.AddPlayerLine("pb_vbgb_son_persuasion_player_option_2", "pb_vbgb_son_persuasion_options", "pb_vbgb_son_persuasion_attempt", "You may think you've got things figured out. I've been here before, in your situation. It's a rough life and you're leaving behind a better one.", null,
                delegate { this._task.Options[1].BlockTheOption(true); }, this, 100,
                new ConversationSentence.OnClickableConditionDelegate(persuasion_option_clickable_2),
                new ConversationSentence.OnPersuasionOptionDelegate(persuasion_option_persuasion_2));
            resultFlow.AddPlayerLine("pb_vbgb_son_persuasion_player_option_3", "pb_vbgb_son_persuasion_options", "pb_vbgb_son_persuasion_attempt", "Look, you're coming home whether you want to or not. So what will it be, are you coming home with a broken nose?", null,
                delegate { this._task.Options[2].BlockTheOption(true); }, this, 100,
                new ConversationSentence.OnClickableConditionDelegate(persuasion_option_clickable_3),
                new ConversationSentence.OnPersuasionOptionDelegate(persuasion_option_persuasion_3));

            resultFlow.AddDialogLine("pb_vbgb_son_persuasion_success", "pb_vbgb_son_persuasion", "pb_vbgb_convo_success", "you've convinced me!",
                new ConversationSentence.OnConditionDelegate(ConversationManager.GetPersuasionProgressSatisfied),
                delegate { ConversationManager.EndPersuasion(); }, this);
            resultFlow.AddDialogLine("pb_vbgb_son_persuasion_failure", "pb_vbgb_son_persuasion", "pb_vbgb_player_options_end_1", "I don't know who you think you are but you can take your leave and mind your own business.",
                delegate { return _task.Options.All((PersuasionOptionArgs x) => x.IsBlocked); },
                delegate {
                    ConversationManager.EndPersuasion();
                    this._sonPersuasionFailed = true;
                }, this);

            resultFlow.GoBackToDialogState("pb_vbgb_convo_success").
                NpcLine("but what do we do now?").PlayerLine("let's fine them!").NpcLine("ok, let's do it!").Consequence(new ConversationSentence.OnConsequenceDelegate(fight_son_convo_consequence)).CloseDialog();

            return resultFlow;
        }

        private void player_gives_up()
        {
            base.CompleteQuestWithFail();
            base.AddLog(new TextObject("you gave up..."));
        }
        private void son_persuasion_delegate_init()
        {
            ConversationManager.StartPersuasion(2f, 1f, 0f, 2f, 1f, 0f, PersuasionDifficulty.Medium);
            this._task = new PersuasionTask(0);

            TextObject Line = new TextObject("I suppose...1");
            PersuasionOptionArgs option1 = new PersuasionOptionArgs(DefaultSkills.Leadership, DefaultTraits.Valor,
                TraitEffect.Positive, PersuasionArgumentStrength.Easy, false, Line);
            this._task.AddOptionToTask(option1);

            Line = new TextObject("I suppose...2");
            PersuasionOptionArgs option2 = new PersuasionOptionArgs(DefaultSkills.Roguery, DefaultTraits.Honor,
                TraitEffect.Negative, PersuasionArgumentStrength.Normal, false, Line);
            this._task.AddOptionToTask(option2);

            Line = new TextObject("I suppose...3");
            PersuasionOptionArgs option3 = new PersuasionOptionArgs(DefaultSkills.Roguery, DefaultTraits.Mercy,
                TraitEffect.Negative, PersuasionArgumentStrength.Hard, true, Line);
            this._task.AddOptionToTask(option3);
        }

        private bool persuasion_option_clickable_1(out TextObject hintText)
        {
            hintText = new TextObject("no no no!", null);
            if (this._task.Options.Any<PersuasionOptionArgs>())
            {
                hintText = TextObject.Empty;
                return !this._task.Options[0].IsBlocked;
            }
            return false;
        }

        private PersuasionOptionArgs persuasion_option_persuasion_1()
        {
            return this._task.Options.ElementAt(0);
        }

        private bool persuasion_option_clickable_2(out TextObject hintText)
        {
            hintText = new TextObject("no no no!", null);
            if (this._task.Options.Any<PersuasionOptionArgs>())
            {
                hintText = TextObject.Empty;
                return !this._task.Options[1].IsBlocked;
            }
            return false;
        }

        private PersuasionOptionArgs persuasion_option_persuasion_2()
        {
            return this._task.Options.ElementAt(1);
        }

        private bool persuasion_option_clickable_3(out TextObject hintText)
        {
            hintText = new TextObject("no no no!", null);
            if (this._task.Options.Any<PersuasionOptionArgs>())
            {
                hintText = TextObject.Empty;
                return !this._task.Options[2].IsBlocked;
            }
            return false;
        }

        private PersuasionOptionArgs persuasion_option_persuasion_3()
        {
            return this._task.Options.ElementAt(2);
        }

        private DialogFlow playerTeamWonFightDialog()
        {
            DialogFlow resultDialog = DialogFlow.CreateDialogFlow("start", 6000).BeginNpcOptions().
                NpcOption("Oh my god we did it...", () => Hero.OneToOneConversationHero == this._headmansSon && this._playerTeamWon && !base.IsFinalized).
                    PlayerLine("So you're going to head straight home now, right?").
                    NpcLine("Yes yes, I don't think I'll be heading back in town anytime soon.", null, null).
                    PlayerLine("I'm glad to hear it.").
                    Consequence(delegate
                    { Campaign.Current.ConversationManager.ConversationEndOneShot += this.vicotry_conversation_consequence; }).CloseDialog().
                NpcOption("I'll head home right away!", () => Hero.OneToOneConversationHero == this._headmansSon && this._playerTeamWon && base.IsFinalized).CloseDialog(); //GotoDialogState("close_window");

            return resultDialog;
        }
        // </Required overrides

        private void vicotry_conversation_consequence()
        {
            //Mission.Current.SetMissionMode(MissionMode.StartUp, false);
            //GameMenu.SwitchToMenu("town");
            base.CompleteQuestWithSuccess();
        }

        private void fight_son_convo_consequence()
        {
            Campaign.Current.ConversationManager.ConversationEndOneShot += this.PlayerFightsGang;
        }

        private DialogFlow playerBeatupSonDialog()
        {
            DialogFlow resultDialog = DialogFlow.CreateDialogFlow("start", 6000).
                NpcLine("Ouch, fine I'll go home.[ib:closed][if:idle_angry][rb:negative]").Condition(() => Hero.OneToOneConversationHero == this._headmansSon && this._playerBeatSon).
                PlayerLine("Pack up your stuff!").
                NpcLine("I'm ready.. let's go").Consequence(delegate { Campaign.Current.ConversationManager.ConversationEndOneShot += this.player_fight_son_dialog_complete_quest; }).CloseDialog();

            return resultDialog;
        }

        private void player_fight_son_dialog_complete_quest()
        {
            //this._sonAgent.ActionSet;
            Mission.Current.SetMissionMode(MissionMode.StartUp, false);
            //this._sonAgent.SetLookAgent(null);
            //this._sonAgent.SetWatchState(AgentAIStateFlagComponent.WatchState.Alarmed);
            this._sonAgent.GetComponent<CampaignAgentComponent>().AgentNavigator.GetActiveBehaviorGroup(); //AddBehaviorGroup<AlarmedBehaviorGroup>();
                                                                                                           //GetBehaviorGroup<AlarmedBehaviorGroup>().;

            //AlarmedBehaviorGroup behGroup = new AlarmedBehaviorGroup(this._sonAgent.GetComponent<CampaignAgentComponent>().AgentNavigator, Mission.Current);                

            //this._sonAgent.GetComponent<CampaignAgentComponent>().GetActiveBehaviorGroup<AlarmedBehaviorGroup>().Navigator.ToString();

            //foreach (AgentComponent cmp in this._sonAgent.Components)
            //{
            //    //foreach(GroupByBehavior gBeh in cmp)
            //    InformationManager.DisplayMessage(new InformationMessage(cmp.ToString()));
            //}

            InformationManager.DisplayMessage(new InformationMessage(this._sonAgent.GetComponent<CampaignAgentComponent>().AgentNavigator.GetActiveBehaviorGroup().ToString()));

            //AlarmedBehaviorGroup.AlarmAgent(this._sonAgent);

            //if (this._headmansSonLocChar.AlarmedActionSetCode == null)
            //{
            //    InformationManager.DisplayMessage(new InformationMessage("alarmed set codes are null"));
            //}
            //else
            //{
            //    InformationManager.DisplayMessage(new InformationMessage(this._headmansSonLocChar.AlarmedActionSetCode.ToString()));
            //}

            //if (this._headmansSonLocChar.ActionSetCode == null)
            //{
            //    InformationManager.DisplayMessage(new InformationMessage("alarmed set codes are null"));
            //}
            //else
            //{
            //    InformationManager.DisplayMessage(new InformationMessage(this._headmansSonLocChar.ActionSetCode.ToString()));
            //}

            //this._headmansSonLocChar.AlarmedActionSetCode.;
            //this._headmansSonLocChar
            //InformationManager.DisplayMessage(new InformationMessage("action set codes?"));



            base.CompleteQuestWithSuccess();
        }

        private void PlayerFightsGang()
        {
            InformationManager.DisplayMessage(new InformationMessage("made it to the playerfightsondelegate"));
            this._sonAgent = (Agent)MissionConversationHandler.Current.ConversationManager.ConversationAgents.First((IAgent x) =>
                x.Character != null && x.Character == this._headmansSon.CharacterObject);

            //Agent gangMemberAgent = (Agent)Mission.Current.GetNearbyAgents(Agent.Main.Position.AsVec2, 100f);
            InformationManager.DisplayMessage(new InformationMessage("made it passed searching for the agent, his state is: " + this._sonAgent.State.ToString()));
            //Mission.Current.Agents.First((IAgent x) => x.Character == this._villageThug.CharacterObject);

            List<Agent> playerSideAgents = new List<Agent>
                {
                    Agent.Main,
                    this._sonAgent
                };

            List<Agent> opponentSideAgents = new List<Agent>();
            foreach (Agent agent in Mission.Current.GetNearbyAgents(Agent.Main.Position.AsVec2, 100f))
            {
                if (agent.Name == "Gang Member")
                {
                    opponentSideAgents.Add(agent);
                }
            }

            //InformationManager.DisplayMessage(new InformationMessage("Hitter count"+Mission()));

            Mission.Current.GetMissionBehaviour<MissionFightHandler>().StartCustomFight(playerSideAgents, opponentSideAgents, false, false, false,
                new MissionFightHandler.OnFightEndDelegate(this.AfterFightGangAction), true, null, null, null, null);

            InformationManager.DisplayMessage(new InformationMessage(Mission.Current.MainAgent.GetWieldedItemIndex(Agent.HandIndex.MainHand).ToString()));
            //Mission.Current.MainAgent.ChangeWeaponHitPoints(Mission.Current.MainAgent.GetWieldedItemIndex(Agent.HandIndex.MainHand),100);


        }

        private void AfterFightGangAction(bool isplayersidewon)
        {
            if (isplayersidewon)
            {
                bool playerActive = Mission.Current.MainAgent != null && Mission.Current.MainAgent.State == AgentState.Active;
                bool sonActive = this._sonAgent != null && this._sonAgent.State == AgentState.Active;
                this._playerTeamWon = true;

                if (!playerActive || !sonActive) //agents are down
                {
                    Mission.Current.NextCheckTimeEndMission = 0f;
                    Mission.Current.EndMission();
                    if (playerActive)
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

            }
            else if (!isplayersidewon)
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

        private void player_fights_son_delegate()
        {
            this._playerBeatSon = true;
            this._sonAgent = (Agent)MissionConversationHandler.Current.ConversationManager.ConversationAgents.First((IAgent x) =>
                x.Character != null && x.Character == this._headmansSon.CharacterObject);

            List<Agent> playerSide = new List<Agent>
                {
                    Agent.Main
                };

            List<Agent> opponentSide = new List<Agent>
                {
                    this._sonAgent
                };

            Mission.Current.GetMissionBehaviour<MissionFightHandler>().StartCustomFight(playerSide, opponentSide, false, true, true,
                new MissionFightHandler.OnFightEndDelegate(this.AfterFightSonAction));
        }

        private void AfterFightSonAction(bool isPlayerSideWon)
        {
            if (isPlayerSideWon)
            {
                this._sonAgent.SetAgentFacialAnimation(Agent.FacialAnimChannel.Low, "talking_sad", true);

            }
            else
            {

                base.AddLog(new TextObject("You failed to intimidate " + QuestGiver.Name.ToString() + "'s son"));
                base.CompleteQuestWithFail();
            }
        }

        // Optional Overrides (virtual)

        public override void OnFailed()
        {
            base.AddLog(new TextObject("you did NOT do it..."));
            ChangeRelationAction.ApplyPlayerRelation(this.QuestGiver, -this._relationGainReward);
            this.QuestGiver.AddPower(-20f);
            base.OnFailed();
        }
        protected override void OnCompleteWithSuccess()
        {
            base.AddLog(new TextObject("you did it!!"));
            if (this._tavernLocationId != null && this._headmansSon != null && this._tavernLocationId.ContainsCharacter(this._headmansSon))
            {
                this._tavernLocationId.RemoveLocationCharacter(this._headmansSonLocChar);
            }
            if (this._tavernLocationId != null && this._headmansSon != null && this._tavernLocationId.ContainsCharacter(this._gangMemberLocChar1))
            {
                this._tavernLocationId.RemoveLocationCharacter(this._gangMemberLocChar1);
            }
            if (this._tavernLocationId != null && this._headmansSon != null && this._tavernLocationId.ContainsCharacter(this._gangMemberLocChar2))
            {
                this._tavernLocationId.RemoveLocationCharacter(this._gangMemberLocChar2);
            }
            GainRenownAction.Apply(Hero.MainHero, 5f);
            ChangeRelationAction.ApplyPlayerRelation(this.QuestGiver, this._relationGainReward);
            GiveGoldAction.ApplyBetweenCharacters(this.QuestGiver, Hero.MainHero, this._goldReward);
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
            //InformationManager.DisplayMessage(new InformationMessage("OnStartQuest has fired"));
            base.OnStartQuest();
        }
        protected override void OnTimedOut()
        {
            base.AddLog(new TextObject("You were too slow in rescuing " + this.QuestGiver.Name.ToString() + "'s son from a life of crime."));
            ChangeRelationAction.ApplyPlayerRelation(this.QuestGiver, -this._relationGainReward);
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

        private int gangLeaderPayoffNeeded
        {
            get
            {
                return (int)(1000f - 25f * this._gangLeader.GetRelationWithPlayer());
            }
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
        public bool _playerTeamWon;

        [SaveableField(60)]
        public Agent _sonAgent;

        [SaveableField(70)]
        public LocationCharacter _gangMemberLocChar1;

        [SaveableField(80)]
        public LocationCharacter _gangMemberLocChar2;

        [SaveableField(90)]
        public int _relationGainReward;

        [SaveableField(100)]
        public LocationCharacter _headmansSonLocChar;

        [SaveableField(110)]
        public bool _sonPersuasionFailed;

        [SaveableField(120)]
        public bool _gangLeaderTalkedTo;

        [SaveableField(130)]
        public bool _playerBeatSon;

        [SaveableField(140)]
        public bool _gangRelationSufficient;

        [SaveableField(150)]
        public Location _tavernLocationId;

        [SaveableField(160)]
        public int _goldReward;

        public List<Hero> _eligibleCompanionsForRogeury;

        [SaveableField(170)]
        public Hero _rogueCompanionForAlternativeSolution;

        [SaveableField(180)]
        private bool _companionPathChosen;

        [SaveableField(190)]
        private CampaignTime _companionSolutionCompletionDate;

        
    }
}
