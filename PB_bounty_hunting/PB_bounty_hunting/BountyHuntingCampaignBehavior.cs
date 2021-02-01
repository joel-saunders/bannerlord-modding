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

namespace PB_bounty_hunting
{
    class BountyHuntingCampaignBehavior : CampaignBehaviorBase 
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
                typeof(BountyHuntingCampaignBehavior.BountyHuntingIssue), IssueBase.IssueFrequency.VeryCommon, null)); 
            }
        }
        //dedicated method function for Quest availablility logic
        private bool ConditionsHold(Hero issueGiver) //2-Define Issue conditions
        {
            return issueGiver.IsHeadman && !issueGiver.IsOccupiedByAnEvent() && issueGiver.CurrentSettlement.IsVillage &&
                                issueGiver.CurrentSettlement.Village.TradeBound.BoundVillages.Any((Village settl) => !settl.Settlement.IsUnderRaid && !settl.Settlement.IsRaided && settl != issueGiver.CurrentSettlement.Village && settl.Settlement.Notables.Any((Hero headman) => headman.IsHeadman && !headman.IsOccupiedByAnEvent()));
        }

        private IssueBase OnStartIssue(PotentialIssueData pid, Hero issueOwner)
        {
            InformationManager.DisplayMessage(new InformationMessage("Bounty Hunting Quest, Culture: " + issueOwner.Culture.ToString()));
            return new BountyHuntingCampaignBehavior.BountyHuntingIssue(issueOwner);             
        }

        public class BountyHuntingCampaignBehviorIssueTypeDefiner : CampaignBehaviorBase.SaveableCampaignBehaviorTypeDefiner 
        {
            public BountyHuntingCampaignBehviorIssueTypeDefiner() : base(0832638932) 
            {
            }

            protected override void DefineClassTypes()
            {
                AddClassDefinition(typeof(BountyHuntingCampaignBehavior.BountyHuntingIssue), 1); 
                AddClassDefinition(typeof(BountyHuntingCampaignBehavior.BountyHuntingQuest), 2); 
            }
        }
        public enum TargetWanted
        {
            DeadorAlive = 0,
            Dead = 1,
            Alive = 2
        };
        public enum TargetCrime
        {
            Thief = 0,
            Murder = 1,
            Deserter = 2
        }
        
        internal class BountyHuntingIssue : IssueBase 
        {
            public BountyHuntingIssue(Hero issueOwner) : base(issueOwner, CampaignTime.DaysFromNow(10f)) 
            {
                this._targetWanted = SetTargetWanted();
                this._targetCrime = SetTargetCrime();
            }

            private TargetWanted SetTargetWanted()
            {
                TargetWanted[] options =
                { 
                    TargetWanted.Alive,
                    TargetWanted.Dead,
                    TargetWanted.DeadorAlive
                };

                return options.GetRandomElement<TargetWanted>();
            }

            private TargetCrime SetTargetCrime()
            {
                TargetCrime[] options =
                {
                    TargetCrime.Deserter,
                    TargetCrime.Murder,
                    TargetCrime.Thief
                };

                return options.GetRandomElement<TargetCrime>();
            }

            // <Required overrides (abstract)
            public override TextObject Title => new TextObject("Bounty Hunting"); //4-Update Quest naming

            public override TextObject Description => new TextObject("Help out the quest giver!"); //4-Update Quest naming

            protected override TextObject IssueBriefByIssueGiver //3-Update quest acceptance text
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
            protected override bool CanPlayerTakeQuestConditions(Hero issueGiver, out PreconditionFlags flag, out Hero relationHero, out SkillObject skill) //5-Done
            {
                relationHero = issueGiver;
                skill = null;
                flag = IssueBase.PreconditionFlags.None;
                if (issueGiver.GetRelationWithPlayer() < -10)
                {
                    flag = IssueBase.PreconditionFlags.Relation;
                }
                if (Hero.MainHero.Clan.IsAtWarWith(issueGiver.Clan))
                {
                    flag = IssueBase.PreconditionFlags.AtWar;
                }
                return flag == IssueBase.PreconditionFlags.None;
            }

            protected override void CompleteIssueWithTimedOutConsequences()
            {
            }

            //When the quest is generated and params are passed into the Quest instance.
            protected override QuestBase GenerateIssueQuest(string questId)
            {
                InformationManager.DisplayMessage(new InformationMessage("***Quest is generated"));

                return new BountyHuntingCampaignBehavior.BountyHuntingQuest(_targetCrime, _targetWanted, questId, base.IssueOwner, 
                    CampaignTime.DaysFromNow(17f), RewardGold);
            }

            protected override void OnGameLoad()
            {

            }

            [SaveableField(10)]
            public TargetWanted _targetWanted;

            [SaveableField(20)]
            public TargetCrime _targetCrime;
            // </Required overrides (abstract)
        }
        //Quest class. For the most part, takes over the quest process after IssueBase.GenerateIssueQuest is called
        internal class BountyHuntingQuest : QuestBase 
        {
            public BountyHuntingQuest(TargetCrime targetcrime, TargetWanted targetwanted, string questId, Hero questGiver, CampaignTime duration, int rewardGold) : base(questId, questGiver, duration, rewardGold) 
            {
                //init Quest vars, such as 'PlayerhastalkedwithX', 'DidPlayerFindY'
                this._targetCrime = targetcrime;
                this._targetWanted = targetwanted;
                this._travellerVillage = SetTravellerVillage();
                this._target = GenerateTarget(this);
                this.GenerateCluesAndIntel();
                this._travellers = GenerateTravellers();
                this.SetDialogs();
                this.InitializeQuestOnCreation();
                InformationManager.DisplayMessage(new InformationMessage(this._travellerVillage.ToString()));
                base.AddLog(new TextObject("The quest has begun!!! woooo!")); //4-Update Quest naming
            }            
            public enum TimeOfDay
            {
                DayOnly = 0,
                NightOnly = 1,
                Anytime = 2
            }
            public enum Clue
            {
                Location = 0,
                TimeofDay = 1,
                Culture = 2
            }

            public enum Intel
            {
                Alibi = 0,
                Destination = 1,
                Background = 2
            }

            private  void OnBeforeMissionOpen()
            {
                if (Settlement.CurrentSettlement == this._travellerVillage.Settlement)
                {
                    if (this._travellerLocationCharacters == null)
                    {
                        //init locChars
                        this._travellerLocationCharacters = new List<LocationCharacter>();
                        foreach (pbTraveller traveller in this._travellers)
                        {
                            AgentData travellerAgent = new AgentData(new SimpleAgentOrigin(traveller.travellerHero.CharacterObject));
                            LocationCharacter travellerLocChar = new LocationCharacter(travellerAgent, new LocationCharacter.AddBehaviorsDelegate(SandBoxManager.Instance.AgentBehaviorManager.AddWandererBehaviors), "npc_common", true, LocationCharacter.CharacterRelations.Neutral, "as_human_villager_gangleader", true, false, null, false, false, true);
                            this._travellerLocationCharacters.Add(travellerLocChar);
                        }
                    }

                    //just add locChars
                    foreach (LocationCharacter locChar in this._travellerLocationCharacters)
                    {
                        Location locationWithId = Settlement.CurrentSettlement.LocationComplex.GetListOfLocations().GetRandomElement<Location>();
                        locationWithId.AddCharacter(locChar);
                    }
                }
            }

            private Village SetTravellerVillage()
            {
                return this.QuestGiver.CurrentSettlement.Village.TradeBound.BoundVillages.Where((Village settl) => settl.VillageState != Village.VillageStates.BeingRaided && settl.VillageState != Village.VillageStates.Looted && settl != base.QuestGiver.CurrentSettlement.Village).GetRandomElement<Village>();                
            }

            private pbTarget GenerateTarget(BountyHuntingQuest quest)
            {
                pbTarget result = new pbTarget(quest);

                return result;
            }

            private void GenerateCluesAndIntel()
            {
                this._availableClues = new List<Clue> { Clue.Culture, Clue.Location, Clue.TimeofDay };

                //this.cluesKnown = new Dictionary<Clue, bool>();
                //this.cluesKnown.Add(Clue.Culture, true);
                //this.cluesKnown.Add(Clue.Location, true);
                //this.cluesKnown.Add(Clue.TimeofDay, true);

                this._intelOnTarget = new List<Intel> { Intel.Alibi, Intel.Background, Intel.Destination };
            }

            private List<pbTraveller> GenerateTravellers()
            {
                List<pbTraveller> result = new List<pbTraveller>();

                int travellerCount = 4;

                for(int i = 0; i < travellerCount; i++ )
                {
                    if (this._availableClues.Count > 0)
                    {
                        Clue travellerClue = this._availableClues.GetRandomElement<Clue>();
                        result.Add(new pbTraveller(this, travellerClue));
                        this._availableClues.Remove(travellerClue);
                    }
                    else
                    {
                        result.Add(new pbTraveller(this));
                    }
                }

                return result;
            }
            // Required overrides (abstract)
            public override TextObject Title => new TextObject("Bounty Hunting"); //4-Update Quest naming

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
                    NpcLine("TEMPLATE: Good, I'm glad you've agreed to the quest. Good luck!").
                        Condition(() => Hero.OneToOneConversationHero == this.QuestGiver).
                        Consequence(QuestAcceptedConsequences).
                    NpcLine("Go to "+this._travellerVillage.Name+"And speak with travellers there.").CloseDialog();
                this.DiscussDialogFlow = DialogFlow.CreateDialogFlow("quest_discuss", 100). //3-Update quest acceptance text
                    NpcLine("TEMPLATE: Why are you here? Shouldn't you be questing?").
                        Condition(() => Hero.OneToOneConversationHero == this.QuestGiver);
                //Campaign.Current.ConversationManager.AddDialogFlow(dialogflowmethod);
                this.GenerateTravellerDialogs();
            }
            // </Required overrides

            private void GenerateTravellerDialogs()
            {
                int idIncrease = 0;
                switch(this._targetCrime)
                {
                    case TargetCrime.Deserter:
                    case TargetCrime.Murder:
                    case TargetCrime.Thief:
                        foreach (pbTraveller traveller in this._travellers)
                        {
                            idIncrease++;
                            String dialogStateId = "pbquestBountyHunting_" + base.StringId + "_" + idIncrease.ToString();
                            Campaign.Current.ConversationManager.AddDialogFlow(TravellerDialog(traveller, dialogStateId));
                        }
                        break;
                }
                    
            }

            private DialogFlow TravellerDialog(pbTraveller traveller, string uniqueDialogId)
            {


                DialogFlow resultFlow = DialogFlow.CreateDialogFlow("start", 600).
                    BeginNpcOptions().
                        NpcOption("Hello there, how can I help you?", () => Hero.OneToOneConversationHero == traveller.travellerHero && !Hero.OneToOneConversationHero.HasMet).
                        PlayerLine("Hey stranger, I'm on the hunt for someone").NpcLine("How can I help?").GotoDialogState("player_asks_about_clues_" + uniqueDialogId).
                        NpcOption("Hey again, " + Hero.MainHero.Name.ToString() + " have any more questions?", () => Hero.OneToOneConversationHero == traveller.travellerHero && Hero.OneToOneConversationHero.HasMet).
                            GotoDialogState("player_asks_about_clues_" + uniqueDialogId);

                string travellerClueOptionsId = "player_asks_about_clues_" + uniqueDialogId;

                DialogFlow clueOptionFlow;
                foreach (Clue clue in _availableClues)
                {
                    clueOptionFlow = DialogFlow.CreateDialogFlow(travellerClueOptionsId, 600).BeginPlayerOptions().
                        PlayerOption("Yea, I know..yada "+travellerClueOptionsId).
                            Condition(() => !traveller.cluesAskedtoTraveller.Contains(clue)).BeginNpcOptions().
                        NpcOption("I know this!", ()=> traveller.clueTrigger == clue).
                        NpcOption("Doesn't ring any bells", () => traveller.clueTrigger != clue).NpcLine("Anything else?").GotoDialogState(travellerClueOptionsId);
                }

                return resultFlow;
            }

            // Optional Overrides (virtual)
            protected override void RegisterEvents()
            {
                base.RegisterEvents();
                CampaignEvents.WarDeclared.AddNonSerializedListener(this, new Action<IFaction, IFaction>(this.OnWarDeclared));
                CampaignEvents.VillageBeingRaided.AddNonSerializedListener(this, new Action<Village>(this.OnVillageRaid));
                CampaignEvents.BeforeMissionOpenedEvent.AddNonSerializedListener(this, new Action(this.OnBeforeMissionOpen));
            }

            private void OnWarDeclared(IFaction faction1, IFaction faction2)
            {
                if (Hero.MainHero.MapFaction.IsAtWarWith(base.QuestGiver.CurrentSettlement.MapFaction))
                {
                    base.AddLog(new TextObject("Due to the war, this quest can no longer be completed."));
                    base.CompleteQuestWithCancel(null);
                }
            }

            private void OnVillageRaid(Village village)
            {
                if (base.QuestGiver.CurrentSettlement == village.Settlement)
                {
                    base.AddLog(new TextObject(village.Name.ToString() + " is being raided. The quest can no longer be completed."));
                    base.CompleteQuestWithCancel(null);
                }
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

            [SaveableField(10)]
            public TargetWanted _targetWanted;

            [SaveableField(20)]
            public TargetCrime _targetCrime;

            [SaveableField(30)]
            public Village _travellerVillage;

            [SaveableField(40)]
            public List<pbTraveller> _travellers;

            [SaveableField(50)]
            public pbTarget _target;

            [SaveableField(60)]
            public List<Clue> _availableClues;

            [SaveableField(70)] 
            public List<Intel> _intelOnTarget;

            [SaveableField(80)]
            public List<LocationCharacter> _travellerLocationCharacters;

            //[SaveableField(90)]
            //public Dictionary<Clue, bool> cluesKnown;
        }
    }
}
