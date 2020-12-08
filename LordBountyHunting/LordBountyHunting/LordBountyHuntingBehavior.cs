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
using TaleWorlds.ObjectSystem;
using TaleWorlds.Library;

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
                                issueGiver.CurrentSettlement.Village.TradeBound.BoundVillages.Any((Village settl)=> !settl.Settlement.IsUnderRaid && !settl.Settlement.IsRaided && settl != issueGiver.CurrentSettlement.Village); 
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
            }
        }

        internal class LordBountyHuntingIssue : IssueBase //1-Update class name
        {
            public LordBountyHuntingIssue(Hero issueOwner) : base(issueOwner, new Dictionary<IssueEffect, float>(), CampaignTime.DaysFromNow(10f)) //1-Update class name
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
            public LordBountyHuntingQuest(float questDifficulty, bool targetisFemail, TargetWanted questGoal, TargetsCrime questTargetCrime, string questId, Hero questGiver, CampaignTime duration, int rewardGold) : base(questId, questGiver, duration, rewardGold) //1-Update class name
            {
                //init Quest vars, such as 'PlayerhastalkedwithX', 'DidPlayerFindY'
                this._questGoal = questGoal;
                this._questTargetCrime = questTargetCrime;
                this._targetIsFemale = targetisFemail;
                this._questDifficulty = questDifficulty;
                this.PrepTargetVillage();
                this.CreateTargetCharacter();
                this.PrepTargetDialog();
                this.SetDialogs();
                this.InitializeQuestOnCreation();
                TextObject log = new TextObject("Go look for the target at: {TARGET_SETTLEMENT.LINK}");
                StringHelpers.SetSettlementProperties("TARGET_SETTLEMENT", this.TargetVillage.Settlement, log);
                base.AddLog(log); //4- Done!
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
                this.OfferDialogFlow = DialogFlow.CreateDialogFlow("issue_classic_quest_start", 100). //3-Update quest acceptance text
                    NpcLine("TEMPLATE: Good, I'm glad you've agreed to the quest. Good luck!").
                        Condition(() => Hero.OneToOneConversationHero == this.QuestGiver).
                        Consequence(QuestAcceptedConsequences).CloseDialog();
                this.DiscussDialogFlow = DialogFlow.CreateDialogFlow("quest_discuss", 100). //3-Update quest acceptance text
                    NpcLine("TEMPLATE: Why are you here? Shouldn't you be questing?").
                        Condition(() => Hero.OneToOneConversationHero == this.QuestGiver);
                Campaign.Current.ConversationManager.AddDialogFlow(this.targetDialogFlow);
            }
            // </Required overrides

            private void PrepTargetDialog()
            {
                switch(this._questTargetCrime)
                {
                    case TargetsCrime.Deserter:
                        this.targetDialogFlow = TargetDeserterDialog();
                        break;
                    case TargetsCrime.Murder:
                        this.targetDialogFlow = TargetMurdererDialog();
                        break;
                    case TargetsCrime.Thief:
                        this.targetDialogFlow = TargetThiefDialog();
                        break;
                }
                DialogFlow resultFlow = DialogFlow.CreateDialogFlow("start", 600).NpcLine("Hey there!").Condition(() => this._targetHero != null && Hero.OneToOneConversationHero == this._targetHero && !base.IsFinalized).
                    PlayerLine("why did you do it.").
                    PlayerLine("complete quest").Consequence( delegate { base.CompleteQuestWithSuccess(); }).CloseDialog();

                //this.targetDialogFlow = resultFlow;
            }

            private DialogFlow TargetDeserterDialog()
            {
                DialogFlow resultFlow = DialogFlow.CreateDialogFlow("start", 600).NpcLine("Hey there!").Condition(() => this._targetHero != null && Hero.OneToOneConversationHero == this._targetHero && !base.IsFinalized).
                    PlayerLine("why did you DESERT.").
                    PlayerLine("complete quest").Consequence(delegate { base.CompleteQuestWithSuccess(); }).CloseDialog();

                return resultFlow;
            }
            private DialogFlow TargetMurdererDialog()
            {
                DialogFlow resultFlow = DialogFlow.CreateDialogFlow("start", 600).NpcLine("Hey there!").Condition(() => this._targetHero != null && Hero.OneToOneConversationHero == this._targetHero && !base.IsFinalized).
                    PlayerLine("why did you Murder.").
                    PlayerLine("complete quest").Consequence(delegate { base.CompleteQuestWithSuccess(); }).CloseDialog();

                return resultFlow;
            }
            private DialogFlow TargetThiefDialog()
            {
                DialogFlow resultFlow = DialogFlow.CreateDialogFlow("start", 600).NpcLine("Hey there!").Condition(() => this._targetHero != null && Hero.OneToOneConversationHero == this._targetHero && !base.IsFinalized).
                    PlayerLine("why did you Steal...").
                    PlayerLine("complete quest").Consequence(delegate { base.CompleteQuestWithSuccess(); }).CloseDialog();

                return resultFlow;
            }
            
            // Optional Overrides (virtual)
            protected override void RegisterEvents()
            {
                base.RegisterEvents();
                CampaignEvents.WarDeclared.AddNonSerializedListener(this, new Action<IFaction, IFaction>(this.OnWarDeclared));
                CampaignEvents.VillageBeingRaided.AddNonSerializedListener(this, new Action<Village>(this.OnVillageRaid));
                CampaignEvents.SettlementEntered.AddNonSerializedListener(this, new Action<MobileParty, Settlement, Hero>(this.OnSettlementEntered));
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
                //foreach(Location loc in settlement.LocationComplex.GetListOfLocations())
                //{
                //    InformationManager.DisplayMessage(new InformationMessage(loc.Name.ToString()));
                //}                

                InformationManager.DisplayMessage(new InformationMessage("target location: " + this._targetLocChar.Character.Name.ToString()));

                foreach (Location loc in this.TargetVillage.Settlement.LocationComplex.GetListOfLocations())
                {
                    InformationManager.DisplayMessage(new InformationMessage("Locations: "+loc.Name.ToString()));
                    
                }

                //InformationManager.DisplayMessage(new InformationMessage("Characters: "+settlement.LocationComplex.GetListOfCharacters() .ToString()));
                if (party != null && this.TargetVillage != null && party.IsMainParty && settlement.IsVillage && settlement.Village == this.TargetVillage)
                {
                    InformationManager.DisplayMessage(new InformationMessage("Hey, this seems to be the right village. TEST"));
                    if (this._targetLocChar != null)
                    {
                        this._targetLocationId.AddCharacter(this._targetLocChar);
                        InformationManager.DisplayMessage(new InformationMessage("target hidden: "+this._targetLocChar.IsHidden));
                        InformationManager.DisplayMessage(new InformationMessage("target visible: " + this._targetLocChar.IsVisualTracked));
                    }
                    //else
                    //{
                    //    this._targetLocationId = settlement.LocationComplex.GetListOfLocations().GetRandomElement<Location>();
                    //    this._targetLocationId.AddCharacter(this._targetLocChar);
                    //}
                }
            }

            private void CreateTargetCharacter()
            {                
                Random rand = new Random();
                //bool targetIsSoldier = this._questTargetCrime == TargetsCrime.Deserter;
                bool targetIsRanged = false; //need to determine any way/reason to implement this.
                int targetAge = rand.Next(targetMinAge, targetMaxAge);
                int weaponTier;
                BasicCharacterObject charOb = base.QuestGiver.Culture.Townsman;
                charOb.Name = new TextObject("TARGET");
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

                if (this._questTargetCrime == TargetsCrime.Deserter)
                {
                    this._targetHero = HeroCreator.CreateSpecialHero(CharacterObject.All.First((CharacterObject x) => x.StringId == "guard_" + base.QuestGiver.Culture.StringId));
                    this.targetForceCivilianEquip = false;
                }
                else
                {
                    List<ItemObject.ItemTypeEnum> weaponTypes = new List<ItemObject.ItemTypeEnum>()
                    { 
                        ItemObject.ItemTypeEnum.OneHandedWeapon,
                        ItemObject.ItemTypeEnum.TwoHandedWeapon,
                        ItemObject.ItemTypeEnum.Polearm
                    };

                    //Pick random weapon type
                    ItemObject.ItemTypeEnum targetWeaponType = weaponTypes.GetRandomElement<ItemObject.ItemTypeEnum>();
                    InformationManager.DisplayMessage(new InformationMessage("Weapon Type: "+targetWeaponType.ToString()));

                    //create hero template
                    this._targetHero = HeroCreator.CreateSpecialHero((from charTemp in CharacterObject.All
                                                                      where
                                               charTemp.Culture == base.QuestGiver.Culture &&
                                               charTemp.Occupation == Occupation.Wanderer &&
                                               charTemp.IsFemale == this._targetIsFemale
                                                                      select charTemp).GetRandomElement<CharacterObject>()) ;                    
                    //pick random weapon
                    ItemObject targetWeapon = new ItemObject((from temp in ItemObject.All
                                               where
                                               temp.WeaponComponent != null &&
                                               temp.ItemType == targetWeaponType &&
                                               temp.Tier == (ItemObject.ItemTiers)weaponTier
                                               //temp.Culture == base.QuestGiver.Culture
                                               select temp).GetRandomElement<ItemObject>());
                    this.targetForceCivilianEquip = false; // targetWeapon.IsCivilian;                    

                    EquipmentElement targetWeaponEquipment = new EquipmentElement(MBObjectManager.Instance.GetObject<ItemObject>(targetWeapon.StringId), null);

                    //float weaponSkillToAdd = //targetWeapon.Item.RelevantSkill
                    this._targetHero.AddSkillXp(targetWeapon.RelevantSkill, 200f * this._questDifficulty);
                    InformationManager.DisplayMessage(new InformationMessage("Target's Weapon: "+targetWeapon.ToString()));
                    //give weapon to target
                    
                    this._targetHero.CivilianEquipment.AddEquipmentToSlotWithoutAgent(EquipmentIndex.WeaponItemBeginSlot, targetWeaponEquipment);

                    MBReadOnlyList<Equipment> equipList = charOb.AllEquipments;

                    charOb.InitializeEquipmentsOnLoad(equipList);
                }

                

                this._targetHero.Name = NameGenerator.Current.GenerateHeroFirstName(this._targetHero, false);
                this._targetHero.AddEventForOccupiedHero(base.StringId);
                
                InformationManager.DisplayMessage(new InformationMessage("item in 0 slot: "+this._targetHero.CivilianEquipment.GetEquipmentFromSlot((EquipmentIndex)0).ToString()));

                InformationManager.DisplayMessage(new InformationMessage("Age: "+this._targetHero.Age.ToString()));
                InformationManager.DisplayMessage(new InformationMessage("Culture: "+this._targetHero.CharacterObject.Culture));
                InformationManager.DisplayMessage(new InformationMessage("Tier: "+this._targetHero.CharacterObject.Tier.ToString()));


                //AgentData agentData = new AgentData(new SimpleAgentOrigin(this._targetHero.CharacterObject));

                

                AgentData agentData = new AgentData(new SimpleAgentOrigin(charOb));

                this._targetLocChar = new LocationCharacter(agentData, new LocationCharacter.AddBehaviorsDelegate(SandBoxManager.Instance.AgentBehaviorManager.AddWandererBehaviors),
                "npc_common", true, LocationCharacter.CharacterRelations.Neutral, "as_human_villager_gangleader", true, false, null, false, false, true);

                this._targetLocationId = this.TargetVillage.Settlement.LocationComplex.GetListOfLocations().GetRandomElement<Location>();                
                
            }
            

            private void PrepTargetVillage()
            {
                //Random rand = new Random();
                //IEnumerable<Village> villageList = this.QuestGiver.CurrentSettlement.Village.TradeBound.BoundVillages.Where((Village settl) => settl.VillageState != Village.VillageStates.BeingRaided && settl.VillageState != Village.VillageStates.Looted);
                
                foreach(Village v in this.QuestGiver.CurrentSettlement.Village.TradeBound.BoundVillages.Where((Village settl) => settl.VillageState != Village.VillageStates.BeingRaided && settl.VillageState != Village.VillageStates.Looted))
                {
                    InformationManager.DisplayMessage(new InformationMessage(v.Name.ToString()));
                }

                //this.TargetVillage = villageList.ElementAt(rand.Next((int)villageList.Count()));
                this.TargetVillage = this.QuestGiver.CurrentSettlement.Village.TradeBound.BoundVillages.Where((Village settl) => settl.VillageState != Village.VillageStates.BeingRaided && settl.VillageState != Village.VillageStates.Looted && settl != base.QuestGiver.CurrentSettlement.Village).GetRandomElement<Village>();
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
            public Hero _targetHero;

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
        }
    }
}
