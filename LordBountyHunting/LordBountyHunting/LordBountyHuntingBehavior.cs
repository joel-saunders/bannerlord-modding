using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SandBox.Source.Missions;
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
using TaleWorlds.ObjectSystem;
using TaleWorlds.Library;
using SandBox.Source.Missions.Handlers;

namespace LordBountyHunting
{
    class LordBountyHuntingBehavior : CampaignBehaviorBase //1-Update class name
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
                typeof(LordBountyHuntingBehavior.LordBountyHuntingIssue), IssueBase.IssueFrequency.VeryCommon, null)); //1-Update class name
            }
        }
        //dedicated method function for Quest availablility logic
        private bool ConditionsHold(Hero issueGiver) //2-Define Issue conditions
        {
            return issueGiver.IsHeadman && !issueGiver.IsOccupiedByAnEvent() && issueGiver.CurrentSettlement.IsVillage && 
                                issueGiver.CurrentSettlement.Village.TradeBound.BoundVillages.Any((Village settl)=> !settl.Settlement.IsUnderRaid && !settl.Settlement.IsRaided && settl != issueGiver.CurrentSettlement.Village && settl.Settlement.Notables.Any((Hero headman)=> headman.IsHeadman && !headman.IsOccupiedByAnEvent())); 
        }

        private IssueBase OnStartIssue(PotentialIssueData pid, Hero issueOwner)
        {
            InformationManager.DisplayMessage(new InformationMessage("Quest genereated, owner: "+ issueOwner.ToString()));

            return new LordBountyHuntingBehavior.LordBountyHuntingIssue(issueOwner); //1-Update class name            

        }
        public enum TargetWanted
        {
            DeadorAlive = 0,
            Dead = 1,
            Alive = 2
        };

        public enum TargetsCrime
        {
            Thief = 0,
            Murder = 1,
            Deserter = 2
        }        

        public class LordBountyHuntingTypeDefiner : CampaignBehaviorBase.SaveableCampaignBehaviorTypeDefiner //1-Update class name
        {
            public LordBountyHuntingTypeDefiner() : base(098321891) //1-Update class name
            {
            }

            protected override void DefineClassTypes()
            {
                AddClassDefinition(typeof(LordBountyHuntingBehavior.LordBountyHuntingIssue), 1); //1-Update class name
                AddClassDefinition(typeof(LordBountyHuntingBehavior.LordBountyHuntingQuest), 2); //1-Update class name
                AddClassDefinition(typeof(pbSuspect), 3); //1-Update class name
            }

            protected override void DefineStructTypes()
            {
                base.AddStructDefinition(typeof(pbSuspect.pbSuspectProperties), 4);
            }
        }

        internal class LordBountyHuntingIssue : IssueBase //1-Update class name
        {
            public LordBountyHuntingIssue(Hero issueOwner) : base(issueOwner, CampaignTime.DaysFromNow(10f)) //1-Update class name
            {
                this.questGoal = DecideTargetGoal();
                this.questTargetCrime = DecideTargetsCrime();
                
                switch (this.questGoal)
                {
                    case TargetWanted.Alive:
                        this._1IssueBriefQuestGoal = new TextObject("Alive: the target is wanted alive");
                        break;
                    case TargetWanted.Dead:
                        this._1IssueBriefQuestGoal = new TextObject("Dead: don't let them see the light of day.. any more.");
                        break;
                    case TargetWanted.DeadorAlive:
                        this._1IssueBriefQuestGoal = new TextObject("Dead OR Alive: do what ya want man.");
                        break;
                }

                switch (this.questTargetCrime)
                {
                    case TargetsCrime.Deserter:
                        this._1IssueBriefQuestCrime = new TextObject("For their crimes of Desertion");
                        break;
                    case TargetsCrime.Murder:
                        this._1IssueBriefQuestCrime = new TextObject("For their crimes of MURDER");
                        break;
                    case TargetsCrime.Thief:
                        this._1IssueBriefQuestCrime = new TextObject("For their crimes of Theft");
                        break;
                }
                InformationManager.DisplayMessage(new InformationMessage("Wanted: "+this.questGoal.ToString()));
                InformationManager.DisplayMessage(new InformationMessage("For the crime of: " + this.questTargetCrime.ToString()));
                
            }

            public TargetWanted DecideTargetGoal()
            {
                Random rand = new Random();
                float[] weights = 
                { 
                    1,
                    1,
                    1
                };                

                //int randomSelection = rand.Next((int)weights.Sum());
                float randomSelection = (float)rand.NextDouble() * weights.Sum();
                float weightProgress = 0;
                InformationManager.DisplayMessage(new InformationMessage("random weight: "+randomSelection.ToString()));
                foreach (var weight in weights.Select((value, i) => new { i, value }))
                {
                    weightProgress += weight.value;
                    if(weightProgress >= randomSelection)
                    {
                        return (TargetWanted)weight.i;
                    }
                }
                return TargetWanted.Dead;
            }

            public TargetsCrime DecideTargetsCrime()
            {
                TargetsCrime[] crimes =
                {
                    TargetsCrime.Deserter,
                    TargetsCrime.Murder,
                    TargetsCrime.Thief
                };

                return crimes.GetRandomElement<TargetsCrime>();
            }
            
            public void DecideTargetCharacteristics()
            {
                Random rand = new Random();
                if(this.questTargetCrime == TargetsCrime.Deserter)
                {
                    this._targetIsFemale = false;
                }
                else
                {
                    this._targetIsFemale = rand.Next(0, 5) == 2;
                }
            }

            // <Required overrides (abstract)
            public override TextObject Title => new TextObject("A Lord's Bounty"); //4- Done!

            public override TextObject Description => new TextObject("Bring the bad guy to justice!"); //4- Done!

            protected override TextObject IssueBriefByIssueGiver //3-Update quest acceptance text
            {
                get
                {
                    return new TextObject(this._1IssueBriefQuestGoal.ToString()+"..."+this._1IssueBriefQuestCrime);

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
                if(issueGiver.GetRelationWithPlayer() < -10)
                {
                    flag = IssueBase.PreconditionFlags.Relation;
                }
                if(Hero.MainHero.Clan.IsAtWarWith(issueGiver.Clan))
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

                return new LordBountyHuntingBehavior.LordBountyHuntingQuest(this.IssueDifficultyMultiplier, this._targetIsFemale, this.questGoal, this.questTargetCrime, questId, base.IssueOwner, //1-Update class name
                    CampaignTime.DaysFromNow(17f), RewardGold);
            }

            protected override void OnGameLoad()
            {

            }
            // </Required overrides (abstract)
            [SaveableField(10)]
            public TargetWanted questGoal;

            [SaveableField(20)]
            public TargetsCrime questTargetCrime;

            [SaveableField(30)]
            public TextObject _1IssueBriefQuestGoal  = new TextObject("you shouldn't see this..");

            [SaveableField(40)]
            public TextObject _2IssueAcceptByPlayer;

            [SaveableField(50)]
            public TextObject _1IssueBriefQuestCrime = new TextObject("you DEFINITELY shouldn't see this..");

            [SaveableField(60)]
            public bool _targetIsFemale;

        }
        //Quest class. For the most part, takes over the quest process after IssueBase.GenerateIssueQuest is called
        internal class LordBountyHuntingQuest : QuestBase //1-Update class name
        {
            public LordBountyHuntingQuest(float questDifficulty, bool targetisFemale, TargetWanted questGoal, TargetsCrime questTargetCrime, string questId, Hero questGiver, CampaignTime duration, int rewardGold) : base(questId, questGiver, duration, rewardGold) //1-Update class name
            {
                //init Quest vars, such as 'PlayerhastalkedwithX', 'DidPlayerFindY'
                this._questGoal = questGoal;
                this._questTargetCrime = questTargetCrime;
                this._targetIsFemale = targetisFemale;
                this._questDifficulty = questDifficulty;
                this.PrepTargetVillage();                
                this.CreateTargetCharacter();
                this.CreateTravellers();
                //this.PrepTargetDialog();
                this.SetDialogs();
                this.InitializeQuestOnCreation();
                TextObject log = new TextObject("Go look for the target at: {TARGET_SETTLEMENT.LINK}");
                StringHelpers.SetSettlementProperties("TARGET_SETTLEMENT", this.TargetVillage.Settlement, log);
                base.AddLog(log); //4- Done!
                this._counterJournalLog = base.AddDiscreteLog(new TextObject("testtt"), new TextObject("Travellers met"), 0, this._suspectList.Count, new TextObject("Short Text"));
            }            

            // Required overrides (abstract)
            public override TextObject Title => new TextObject("A Lord's Bounty"); //4- Done!

            public override bool IsRemainingTimeHidden => false;

            protected override void InitializeQuestOnGameLoad()
            {
                this.SetDialogs();

            }
            //there are a couple DialogFlows QuestBase has that you'll want to set here. In addition, whatever other dialog flows you have should also
            //be called here. Have them in separate methods for simplicity.
            protected override void SetDialogs()
            {
                TextObject villageText = new TextObject("{VILLAGE.LINK}");
                TextObject targetName = new TextObject("{TARGET}");
                targetName.SetCharacterProperties("TARGET",this._targetChar);
                StringHelpers.SetSettlementProperties("VILLAGE", this.TargetVillage.Settlement, villageText);

                this.OfferDialogFlow = DialogFlow.CreateDialogFlow("issue_classic_quest_start", 200). //3-Update quest acceptance text
                    NpcLine("TEMPLATE: Good, I'm glad you've agreed to the quest. Go look through those travelling through "+villageText+" and look for "+targetName+", and if you have any issues, talk with the Headman there").
                        Condition(() => Hero.OneToOneConversationHero == this.QuestGiver).
                        Consequence(QuestAcceptedConsequences).CloseDialog();
                this.DiscussDialogFlow = DialogFlow.CreateDialogFlow("quest_discuss", 100). //3-Update quest acceptance text
                    NpcLine("TEMPLATE: Why are you here? Shouldn't you be questing?").
                        Condition(() => Hero.OneToOneConversationHero == this.QuestGiver);
                this.prepDialogProperties();
                this.CreateSuspectDialogs();
                Campaign.Current.ConversationManager.AddDialogFlow(this.villageHeadmanDialog());
                
            }
            // </Required overrides

            private void CreateSuspectDialogs()
            {
                int idIncrease = 0;
                switch (this._questTargetCrime)
                {
                    case TargetsCrime.Deserter:                        
                        foreach (pbSuspect susp in this._suspectList)
                        {
                            idIncrease++;
                            String dialogStateId = "pbquestBountyHunting_" + base.StringId +"_"+ idIncrease.ToString();
                            susp.dialogId = dialogStateId;
                            Campaign.Current.ConversationManager.AddDialogFlow(TargetDeserterDialog(susp));
                        }
                        break;
                    case TargetsCrime.Murder:                        
                        foreach (pbSuspect susp in this._suspectList)
                        {
                            idIncrease++;
                            String dialogStateId = "pbquestBountyHunting_" + base.StringId + "_" + idIncrease.ToString();
                            susp.dialogId = dialogStateId;
                            Campaign.Current.ConversationManager.AddDialogFlow(TargetMurdererDialog(susp));
                        }
                        break;
                    case TargetsCrime.Thief:
                        foreach (pbSuspect susp in this._suspectList)
                        {
                            idIncrease++;
                            String dialogStateId = "pbquestBountyHunting_" + base.StringId + "_" + idIncrease.ToString();
                            susp.dialogId = dialogStateId;
                            Campaign.Current.ConversationManager.AddDialogFlow(TargetThiefDialog(susp, dialogStateId));                            
                        }
                        break;
                }
            }
            private void prepDialogProperties()
            {
                //List validated empty

                //loop

                //    headman : grab random
                //        !knowntoheadman
                //    otherT: out of headman AND 
                //        get random where 

                List<pbSuspect> validatedTravellers = new List<pbSuspect>();
                ;
                bool headmansTurn = true;

                while(validatedTravellers.Count + 2 <= this._suspectList.Count)
                {
                    if(headmansTurn)
                    {
                        pbSuspect suspect = this._suspectList.Where((pbSuspect sus) => sus.CharObject != this._targetChar && !sus.knownToHeadman && !sus.knownToTraveller).GetRandomElement();
                        suspect.knownToHeadman = true;
                        validatedTravellers.Add(suspect);
                        
                    }
                    else
                    {
                        pbSuspect suspectTHATKnows = this._suspectList.Where((pbSuspect sus) => sus.CharObject != this._targetChar && (sus.knownToHeadman || sus.knownToTraveller)).GetRandomElement();

                        suspectTHATKnows.knowsThisTraveller = this._suspectList.Where((pbSuspect sus) => sus.CharObject != this._targetChar && !sus.knownToHeadman && !sus.knownToTraveller && sus != suspectTHATKnows).GetRandomElement();

                        suspectTHATKnows.knowsThisTraveller.knownToTraveller = true;
                        validatedTravellers.Add(suspectTHATKnows.knowsThisTraveller);

                    }
                    headmansTurn = !headmansTurn;
                }

                //int travellersKnowntoHeadmanCount = (int)(this._suspectList.Count - 2) / 2;

                //int travellersKnowntoOtherTravellersCount = this._suspectList.Count - 2 - travellersKnowntoHeadmanCount;

                //List<pbSuspect> travellersKnowntoHeadman = new List<pbSuspect>();

                //for (int i = 0; i < travellersKnowntoHeadmanCount; i++)
                //{
                //    //Set Headman's "friends"
                //    pbSuspect suspect = this._suspectList.Where((pbSuspect sus) => sus.CharObject != this._targetChar && !sus.knownToHeadman).GetRandomElement();
                //    travellersKnowntoHeadman.Add(suspect);
                //    suspect.knownToHeadman = true;
                //}

                //Collection of Travellers that other travellers know of
                //List<pbSuspect> travellersKnowntoOtherTravellers = new List<pbSuspect>(this._suspectList.Where((pbSuspect sus) => sus.CharObject != this._targetChar && !sus.knownToHeadman));

                //Make it so there will be 2 travellers that can't be validated. These must be validated by their properties.
                //if(travellersKnowntoOtherTravellers.Count > 1)
                //{
                //    travellersKnowntoOtherTravellers.Remove(travellersKnowntoOtherTravellers.GetRandomElement());
                //}

                //bool firstLoop = true;

                //foreach (pbSuspect sus in travellersKnowntoOtherTravellers)
                //{
                //    pbSuspect speakingTraveller;
                //    if (firstLoop)
                //    {
                //        this._suspectList.Where((pbSuspect sus2) => sus2.CharObject != this._targetChar && sus2.knownToHeadman && sus2 != sus && sus.knowsThisTraveller != sus2).GetRandomElement().knowsThisTraveller = sus;
                //    }
                //    else
                //    {
                //        this._suspectList.Where((pbSuspect sus2) => sus2.CharObject != this._targetChar && sus2.knowsThisTraveller == null && sus2 != sus && sus.knowsThisTraveller != sus2).GetRandomElement().knowsThisTraveller = sus;
                //    }


                //}

                //for (int i = 0; i < travellersWhoKnowEachOther; i++)
                //{
                //    travellersKnowntoOtherTravellers.Add(this._suspectList.Where((pbSuspect sus) => sus.CharObject != this._targetChar && !sus.knownToHeadman).GetRandomElement());

                //    this._suspectList.Where((pbSuspect sus) => sus.CharObject != this._targetChar && !sus.knownToHeadman).GetRandomElement().knowntoOtherTravellers = true;
                //}

                foreach (pbSuspect sus in this._suspectList)
                {
                    TextObject headmanResponse = new TextObject();
                    if (sus.knownToHeadman) //TO-DO: Add property to pbSuspect to determine the headman's response.
                    {
                        headmanResponse = sus.dialogs.dialogHeadmanAffirmation;

                    }
                    else
                    {
                        headmanResponse = sus.dialogs.dialogHeadmanUnknown;
                    }
                    DialogFlow resultFlow = DialogFlow.CreateDialogFlow("pb_bounty_traveller_options", 1000).GoBackToDialogState("pb_bounty_traveller_options").
                        PlayerLine("What about " + sus.Name).Condition(() => sus.introductionDone).NpcLine("What did they tell you?").
                        PlayerLine(sus.dialogs.dialogPlayerDescription).
                        NpcLine(headmanResponse).NpcLine("Anything else?").GotoDialogState("pb_bounty_traveller_options");
                    Campaign.Current.ConversationManager.AddDialogFlow(resultFlow);
                }
                DialogFlow resultFlow2 = DialogFlow.CreateDialogFlow("pb_bounty_traveller_options", 1000).GoBackToDialogState("pb_bounty_traveller_options").PlayerLine("I think I'm good actually").NpcLine("Alright, well just let me know if I can help").CloseDialog();
                Campaign.Current.ConversationManager.AddDialogFlow(resultFlow2);
            }

            private DialogFlow TargetDeserterDialog(pbSuspect susp)
            {
                DialogFlow resultFlow = DialogFlow.CreateDialogFlow("start", 600).NpcLine("Hey there!").Condition(() => this._targetChar != null && (CharacterObject.OneToOneConversationCharacter == this._targetChar || this.travellerHeros.Contains(Hero.OneToOneConversationHero)) && !base.IsFinalized).BeginPlayerOptions().
                    PlayerOption("why did you DESERT.").CloseDialog().
                    PlayerOption("complete quest").Consequence(delegate { base.CompleteQuestWithSuccess(); }).CloseDialog();

                return resultFlow;
            }
            private DialogFlow TargetMurdererDialog(pbSuspect susp)
            {
                DialogFlow resultFlow = DialogFlow.CreateDialogFlow("start", 600).NpcLine("Hey there!").Condition(() => this._targetChar != null && (CharacterObject.OneToOneConversationCharacter == this._targetChar || this.travellerHeros.Contains(Hero.OneToOneConversationHero)) && !base.IsFinalized).BeginPlayerOptions().
                    PlayerOption("why did you Murder.").CloseDialog().
                    PlayerOption("complete quest").Consequence(delegate { base.CompleteQuestWithSuccess(); }).CloseDialog();

                return resultFlow;
            }
            private DialogFlow TargetThiefDialog(pbSuspect susp, String dialogId)
            {
                TextObject theOtherTraveller = new TextObject("blank");
                if(susp.knowsThisTraveller != null)
                {
                    theOtherTraveller = susp.knowsThisTraveller.Name;
                }

                DialogFlow resultFlow = DialogFlow.CreateDialogFlow("start", 600).BeginNpcOptions().
                    NpcOption("Hey there!", () => CharacterObject.OneToOneConversationCharacter == susp.CharObject && !susp.introductionDone && !base.IsFinalized).GotoDialogState(dialogId).
                    NpcOption("Hello again!", () => CharacterObject.OneToOneConversationCharacter == susp.CharObject && !base.IsFinalized).GotoDialogState(dialogId).GoBackToDialogState(dialogId).
                    BeginPlayerOptions().
                    //Ask basic questions
                        PlayerOption("Hello stranger, what's your name?").Condition(() => !this.GetSuspect(CharacterObject.OneToOneConversationCharacter).introductionDone).Consequence(delegate
                        {
                            this.GetSuspect(CharacterObject.OneToOneConversationCharacter).CompleteIntroduction();
                        }).
                        NpcLine("Sure, my name is " + susp.Name).PlayerLine("And what brings you to this place?").
                        NpcLine(susp.dialogs.dialogBackground).Consequence(delegate { 
                            this._counterJournalLog.UpdateCurrentProgress(this._suspectList.Where((pbSuspect sus) => sus.introductionDone).Count()); }).
                        NpcLine("Is there anything else?").GotoDialogState(dialogId).
                        PlayerOption("Hello again blank, what is it again that youre doing here?").Condition(() => this.GetSuspect(CharacterObject.OneToOneConversationCharacter).introductionDone).NpcLine(susp.dialogs.dialogBackground).
                        NpcLine("Is there anything else?").GotoDialogState(dialogId).
                    //State what your here doing
                        PlayerOption("So you know someone here that can help me?").Condition(()=> susp.knowsThisTraveller != null).NpcLine("Yes, "+theOtherTraveller+" is someone you can trust.").
                    //Acuse suspect of being the criminal
                        PlayerOption("You know why I'm here, blank. Don't make this difficult").BeginNpcOptions().
                            NpcOption("Please don't hurt me, I'll come quietly", () => CharacterObject.OneToOneConversationCharacter == this._targetChar && this.GetSuspect(this._targetChar).Properties.personality == pbSuspect.PersonalityType.Scared).BeginPlayerOptions().
                                PlayerOption("I need your head!").Consequence(delegate { this.fight_traveller_consequence(CharacterObject.OneToOneConversationCharacter); }).CloseDialog().
                                PlayerOption("Sorry, I must have been mistaken.").NpcLine("Oh, uh.. ok. Is there anything else?").GotoDialogState(dialogId).EndPlayerOptions().
                            NpcOption("What are you talking about?", () => this._suspectList.Contains(GetSuspect(CharacterObject.OneToOneConversationCharacter))).BeginPlayerOptions().
                                PlayerOption("Don't play stupid, defend yourself!").Consequence(delegate { this.fight_traveller_consequence(CharacterObject.OneToOneConversationCharacter); }).CloseDialog().
                                PlayerOption("Sorry, I must have been mistaken.").NpcLine("Oh, uh.. ok. Is there anything else?").GotoDialogState(dialogId).EndPlayerOptions().EndNpcOptions().
                    //Leave conversation
                        PlayerOption("Never mind, I need to go now.").CloseDialog().
                    //debug complete quest
                        PlayerOption("complete quest").Consequence(delegate { base.CompleteQuestWithSuccess(); }).CloseDialog();

                return resultFlow;
            }            

            private void fight_traveller_consequence(CharacterObject charOb)
            {
                this._chosenChar = charOb;
                Campaign.Current.ConversationManager.ConversationEndOneShot += this.PlayerFightsTraveller;
            }

            private void PlayerFightsTraveller()
            {
                Agent targetAgent = (Agent)MissionConversationHandler.Current.ConversationManager.ConversationAgents.First((IAgent x) =>
                    x.Character != null);                

                List<Agent> playerSideAgents = new List<Agent>
                {
                    Agent.Main
                };

                List<Agent> opponentSideAgents = new List<Agent>
                {
                    targetAgent
                };

                Mission.Current.GetMissionBehaviour<MissionFightHandler>().StartCustomFight(playerSideAgents, opponentSideAgents, true, false, false, new MissionFightHandler.OnFightEndDelegate(this.AfterTravellerFightAction));
            }

            private void AfterTravellerFightAction(bool isplayersidewon)
            {
                if(isplayersidewon)
                {
                    if(this._targetChar == this._chosenChar)
                    {
                        Mission.Current.SetMissionMode(MissionMode.StartUp, false);
                        base.AddLog(new TextObject("you defeated the target!"));
                        base.CompleteQuestWithSuccess();
                    }
                    else
                    {
                        Mission.Current.SetMissionMode(MissionMode.StartUp, false);
                        base.AddLog(new TextObject("Uh oh... you defeated the wrong person."));
                        base.CompleteQuestWithFail();
                    }
                }
                else
                {
                    Mission.Current.SetMissionMode(MissionMode.StartUp, false);
                    base.AddLog(new TextObject("you were defeated."));
                    base.CompleteQuestWithFail();
                }
            }

            private DialogFlow villageHeadmanDialog()
            {
                DialogFlow resultFlow = DialogFlow.CreateDialogFlow("hero_main_options", 600).BeginPlayerOptions().
                    PlayerOption("I'm actually looking for a fugitive that is believed to be your village. Do you mind if I ask you a few questions?").
                        Condition(() => Hero.OneToOneConversationHero == this.targetVillageHeadman && !this._villageHeadmanInitialConvComplete && !base.IsFinalized).Consequence( delegate { this._villageHeadmanInitialConvComplete = true; }).
                        NpcLine("Sure of course, I'd love to help.").
                        NpcLine("Do you have any suspects yet?").BeginPlayerOptions().
                        PlayerOption("No not yet. Any recomendations?").Condition(()=> !this._suspectList.Any((pbSuspect sus)=> sus.introductionDone)).NpcLine("Go talk to travellers").PlayerLine("ok will do!").CloseDialog().
                        PlayerOption("Yes, could I see if you know them?").Condition(() => this._suspectList.Any((pbSuspect sus) => sus.introductionDone)).EndPlayerOptions().NpcLine("Of course, who do you want to know about?").
                        GotoDialogState("pb_bounty_traveller_options").
                    PlayerOption("And again... If I could bug you").
                        Condition(() => Hero.OneToOneConversationHero == this.targetVillageHeadman && this._villageHeadmanInitialConvComplete && !base.IsFinalized).
                        NpcLine("Of course, who do you want to know about?").GotoDialogState("pb_bounty_traveller_options");

                return resultFlow;
            }

            

            // Optional Overrides (virtual)
            protected override void RegisterEvents()
            {
                base.RegisterEvents();
                CampaignEvents.WarDeclared.AddNonSerializedListener(this, new Action<IFaction, IFaction>(this.OnWarDeclared));
                CampaignEvents.VillageBeingRaided.AddNonSerializedListener(this, new Action<Village>(this.OnVillageRaid));
                CampaignEvents.SettlementEntered.AddNonSerializedListener(this, new Action<MobileParty, Settlement, Hero>(this.OnSettlementEntered));
                //CampaignEvents.OnMissionStartedEvent.AddNonSerializedListener(this, new Action<IMission>(this.OnMissionStarted));
                CampaignEvents.AfterMissionStarted.AddNonSerializedListener(this, new Action<IMission>(this.OnMissionStarted));
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

            private void OnSettlementEntered(MobileParty party, Settlement settlement, Hero hero)
            {

                InformationManager.DisplayMessage(new InformationMessage( Campaign.Current.VisualTrackerManager.CheckTracked(this._targetChar).ToString()));

                if (party != null && this.TargetVillage != null && party.IsMainParty && settlement.IsVillage && settlement.Village == this.TargetVillage)
                {
                    InformationManager.DisplayMessage(new InformationMessage("Hey, this seems to be the right village. TEST"));
                    if (this._targetLocChar != null)
                    {
                        //base.RemoveTrackedObject(this._targetChar);
                        this._targetLocationId.AddCharacter(this._targetLocChar);
                        foreach(LocationCharacter locCh in this.traverllerLocChars)
                        {
                            //Location randomLocation = settlement.LocationComplex.GetListOfLocations().Where((Location locComp) => locComp != this._targetLocationId).GetRandomElement<Location>();

                            

                            this._targetLocationId.AddCharacter(locCh);
                            InformationManager.DisplayMessage(new InformationMessage("traveller foreach fired, Name: "+locCh.Character.Name.ToString()));
                        }                        
                    }
                    //else
                    //{
                    //    this._targetLocationId = settlement.LocationComplex.GetListOfLocations().GetRandomElement<Location>();
                    //    this._targetLocationId.AddCharacter(this._targetLocChar);
                    //}
                }
            }

            private void OnMissionStarted(IMission imission)
            {
                //Mission.Current.MissionBehaviours.ForEach(x => InformationManager.DisplayMessage(new InformationMessage(x.ToString())));

                Mission.Current.GetMissionBehaviour<VisualTrackerMissionBehavior>()._currentTrackedObjects.ForEach(x => InformationManager.DisplayMessage(new InformationMessage("the tracked's name: "+x.Name.ToString())));

                //foreach (TrackedObject in Mission.Current.GetMissionBehaviour<VisualTrackerMissionBehavior>()._currentTrackedObjects);
            }

            private void CreateTargetCharacter()
            {                
                Random rand = new Random();
                //bool targetIsSoldier = this._questTargetCrime == TargetsCrime.Deserter;
                bool targetIsRanged = false; //need to determine any way/reason to implement this.
                int targetAge = rand.Next(targetMinAge, targetMaxAge);
                int weaponTier;
                this._suspectList = new List<pbSuspect>();
                //Hero testhero = new Hero();
                
                //HeroCreator.CreateBasicHero(charOb, testhero);


                if (this._questDifficulty < .2f)
                {
                    weaponTier = 0; //tier 1
                } else if (this._questDifficulty < .4f)
                {
                    weaponTier = 2;
                }
                else
                {
                    weaponTier = 3;
                }

                //Dictionary <ItemObject.ItemTypeEnum, float> availableWeaponTypes = new Dictionary<ItemObject.ItemTypeEnum, float>()
                //    {
                //        { ItemObject.ItemTypeEnum.OneHandedWeapon, 1.0f},
                //        { ItemObject.ItemTypeEnum.TwoHandedWeapon, 1.0f},
                //        { ItemObject.ItemTypeEnum.Polearm, 1.0f},
                //        { ItemObject.ItemTypeEnum.Thrown, 1.0f}
                //    };

                //rand.Next(0, (int)availableWeaponTypes.Values.Sum());

                //foreach(CharacterObject charTemp in CharacterObject.Templates)
                //{
                //    InformationManager.DisplayMessage(new InformationMessage(charTemp.StringId));
                //}                

                List<ItemObject.ItemTypeEnum> weaponTypes = new List<ItemObject.ItemTypeEnum>()
                    {
                        ItemObject.ItemTypeEnum.OneHandedWeapon,
                        ItemObject.ItemTypeEnum.TwoHandedWeapon,
                        ItemObject.ItemTypeEnum.Polearm
                    };

                //Pick random weapon type
                ItemObject.ItemTypeEnum targetWeaponType = weaponTypes.GetRandomElement<ItemObject.ItemTypeEnum>();                

                //ItemObject targetWeapon = new ItemObject((from temp in ItemObject.All
                //                                          where
                //                                          temp.WeaponComponent != null &&
                //                                          temp.ItemType == targetWeaponType &&
                //                                          temp.Tier == (ItemObject.ItemTiers)weaponTier
                //                                          //temp.Culture == base.QuestGiver.Culture
                //                                          select temp).GetRandomElement<ItemObject>());

                pbSuspect suspect = new pbSuspect(this);
                this._targetChar = suspect.CharObject;

                this._targetChar.Name = new TextObject("Traveller");
                    //NameGenerator.Current.GenerateHeroFirstName(this._targetChar, false);
                //this._targetChar.AddEventForOccupiedHero(base.StringId);

                this._suspectList.Add(suspect);

                //EquipmentElement targetWeaponEquipment = new EquipmentElement(MBObjectManager.Instance.GetObject<ItemObject>(targetWeapon.StringId), null);

                //float weaponSkillToAdd = //targetWeapon.Item.RelevantSkill
                //this._targetChar.SetSkillValue(targetWeapon.RelevantSkill, 200); //this._questDifficulty);

                //give weapon to target
                //this._targetChar.Equipment.AddEquipmentToSlotWithoutAgent(EquipmentIndex.WeaponItemBeginSlot, targetWeaponEquipment);
                //this._targetChar.CivilianEquipment.AddEquipmentToSlotWithoutAgent(EquipmentIndex.WeaponItemBeginSlot, targetWeaponEquipment);

                InformationManager.DisplayMessage(new InformationMessage("item in 0 slot: "+this._targetChar.Equipment.GetEquipmentFromSlot((EquipmentIndex)0).ToString()));

                InformationManager.DisplayMessage(new InformationMessage("Target Background: "+suspect.Properties.background.ToString()));
                InformationManager.DisplayMessage(new InformationMessage("Target is a woman: "+suspect.Properties.isFemale.ToString()));
                InformationManager.DisplayMessage(new InformationMessage("Target Personality: "+suspect.Properties.personality.ToString()));
                InformationManager.DisplayMessage(new InformationMessage("Target's Weapon: "+targetWeaponType.ToString()));
                InformationManager.DisplayMessage(new InformationMessage("Hair: " + suspect.CharObject.HairTags));

                AgentData agentData = new AgentData(new SimpleAgentOrigin(this._targetChar));
                
                this._targetLocChar = new LocationCharacter(agentData, new LocationCharacter.AddBehaviorsDelegate(SandBoxManager.Instance.AgentBehaviorManager.AddWandererBehaviors),
                "npc_common", true, LocationCharacter.CharacterRelations.Neutral, "as_human_villager_gangleader", true, false, null, false, false, true);

                this._targetLocationId = this.TargetVillage.Settlement.LocationComplex.GetListOfLocations().GetRandomElement<Location>();                
                
            }
            
            private void CreateTravellers()
            {
                int travellerQuantity = 4;//MBRandom.Random.Next(1, 5);

                this.traverllerLocChars = new List<LocationCharacter>();                

                for (int i = 0; travellerQuantity > i; i++)
                {
                    int targetAge = MBRandom.Random.Next(targetMinAge, targetMaxAge);
                    bool travellerFemale = MBRandom.Random.Next(0, 2) == 1;
                    int weaponTier;

                    //Hero testhero = new Hero();

                    //HeroCreator.CreateBasicHero(charOb, testhero);


                    if (this._questDifficulty < .2f)
                    {
                        weaponTier = 0; //tier 1
                    }
                    else if (this._questDifficulty < .4f)
                    {
                        weaponTier = 2;
                    }
                    else
                    {
                        weaponTier = 3;
                    }

                    //Hero travellerHero = HeroCreator.CreateSpecialHero((from charTemp in CharacterObject.All
                    //                                                  where
                    //                           charTemp.Culture == base.QuestGiver.Culture &&
                    //                           charTemp.Occupation == Occupation.Wanderer &&
                    //                           charTemp.IsFemale == travellerFemale
                    //                                                  select charTemp).GetRandomElement<CharacterObject>());

                    pbSuspect suspect = new pbSuspect(this, GetSuspect(this._targetChar));
                    this._suspectList.Add(suspect);

                    InformationManager.DisplayMessage(new InformationMessage("Hair: " + suspect.CharObject.HairTags));
                    CharacterObject travellerCharacter = suspect.CharObject;

                    travellerCharacter.Name = new TextObject("Traveller");

                    //base.RemoveTrackedObject(travellerHero);
                    //Campaign.Current.VisualTrackerManager.RemoveTrackedObject(travellerHero);                                        

                    AgentData travellerAgent = new AgentData(new SimpleAgentOrigin(travellerCharacter));

                    LocationCharacter travellerLocChar = new LocationCharacter(travellerAgent, new LocationCharacter.AddBehaviorsDelegate(SandBoxManager.Instance.AgentBehaviorManager.AddWandererBehaviors),
                "npc_common", true, LocationCharacter.CharacterRelations.Neutral, "as_human_villager_gangleader", true, false, null, false, false, true);

                    this.traverllerLocChars.Add(travellerLocChar);
                }
            }

            private void PrepTargetVillage()
            {                
                this.TargetVillage = this.QuestGiver.CurrentSettlement.Village.TradeBound.BoundVillages.Where((Village settl) => settl.VillageState != Village.VillageStates.BeingRaided && settl.VillageState != Village.VillageStates.Looted && settl != base.QuestGiver.CurrentSettlement.Village).GetRandomElement<Village>();

                this.targetVillageHeadman = this.TargetVillage.Settlement.Notables.First((Hero x) => x.IsHeadman);
                InformationManager.DisplayMessage(new InformationMessage("Go to: "+this.TargetVillage.Name.ToString()));
            }

            private pbSuspect GetSuspect(CharacterObject charOb)
            {
                return this._suspectList.Find((pbSuspect sus) => sus.CharObject == charOb);
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

            enum QuestWeaponType
            {
                testthing = ItemObject.ItemTypeEnum.TwoHandedWeapon
            }
            int targetMinAge
            {
                get => 16;
            }

            int targetMaxAge
            {
                get => 42;
            }            
            
            [SaveableField(10)]
            public Village TargetVillage;

            [SaveableField(20)]
            public CharacterObject _targetChar;

            [SaveableField(30)]
            public LocationCharacter _targetLocChar;

            [SaveableField(40)]
            public Location _targetLocationId;

            [SaveableField(50)]
            public TargetWanted _questGoal;

            [SaveableField(60)]
            public TargetsCrime _questTargetCrime;

            [SaveableField(70)]
            public bool _targetIsFemale;

            [SaveableField(80)]
            public float _questDifficulty;

            [SaveableField(90)]
            public bool targetForceCivilianEquip;

            [SaveableField(100)]
            public DialogFlow targetDialogFlow;

            [SaveableField(110)]
            public List<LocationCharacter> traverllerLocChars;

            [SaveableField(120)]
            public List<Hero> travellerHeros;

            [SaveableField(130)]
            public List<pbSuspect> _suspectList;

            [SaveableField(140)]
            public TextObject _targetName;

            [SaveableField(150)]
            public CharacterObject _chosenChar;

            [SaveableField(160)]
            private bool _villageHeadmanInitialConvComplete;

            [SaveableField(170)]
            private Hero targetVillageHeadman;

            [SaveableField(180)]
            private JournalLog _counterJournalLog;
        }
    }
}
