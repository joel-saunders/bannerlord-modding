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

namespace SC_Teaser_Quests
{
    class SC_HeadmanNeedsGrainIssueBehavior : CampaignBehaviorBase //1-Update class name
    {
        
        internal static int AverageGrainPriceInCalradia
        {
            get
            {
                return Campaign.Current.GetCampaignBehavior<SC_HeadmanNeedsGrainIssueBehavior>()._averageGrainPriceInCalradia;
            }
        }

        public override void RegisterEvents()
        {
            CampaignEvents.OnCheckForIssueEvent.AddNonSerializedListener(this, new Action<IssueArgs>(this.OnCheckForIssue));
            CampaignEvents.RemoveListeners(Campaign.Current.GetCampaignBehavior<HeadmanNeedsGrainIssueBehavior>());
            CampaignEvents.WeeklyTickEvent.AddNonSerializedListener(this, new Action(this.WeeklyTick));
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(this.OnSessionLaunched));
        }



        public override void SyncData(IDataStore dataStore)
        {
            //do nothing..
        }

        public void OnCheckForIssue(IssueArgs issueArgs)
        {
            if (this.ConditionsHold(issueArgs.IssueOwner))
            {
                issueArgs.SetPotentialIssueData(new PotentialIssueData(new Func<PotentialIssueData, Hero, IssueBase>(this.OnSelected), typeof(SC_HeadmanNeedsGrainIssueBehavior.SC_HeadmanNeedsGrainIssue), IssueBase.IssueFrequency.VeryCommon, null));
                return;
            }
            issueArgs.SetPotentialIssueData(new PotentialIssueData(typeof(SC_HeadmanNeedsGrainIssueBehavior.SC_HeadmanNeedsGrainIssue), IssueBase.IssueFrequency.VeryCommon));
        }
        //dedicated method function for Quest availablility logic
        private bool ConditionsHold(Hero issueGiver)
        {
            if (issueGiver.CurrentSettlement == null || !issueGiver.IsNotable || !issueGiver.CurrentSettlement.IsVillage || !issueGiver.CurrentSettlement.Village.Bound.IsTown)
            {
                return false;
            }
            bool flag = false;
            foreach (ItemRosterElement itemRosterElement in issueGiver.CurrentSettlement.Village.Bound.ItemRoster)
            {
                if (itemRosterElement.EquipmentElement.Item == DefaultItems.Grain && itemRosterElement.Amount < 50)
                {
                    flag = true;
                    break;
                }
            }
            return issueGiver.IsHeadman && issueGiver.CurrentSettlement.Village.IsProducing(DefaultItems.Grain) && flag && (float)issueGiver.CurrentSettlement.Village.GetItemPrice(DefaultItems.Grain, null, false) > (float)this._averageGrainPriceInCalradia * 1.3f;
        }

        private IssueBase OnSelected(PotentialIssueData pid, Hero issueOwner)
        {
            return new SC_HeadmanNeedsGrainIssueBehavior.SC_HeadmanNeedsGrainIssue(issueOwner);
        }

        public class RQTCampaignBehviorIssueTypeDefiner : CampaignBehaviorBase.SaveableCampaignBehaviorTypeDefiner //1-Update class name
        {
            public RQTCampaignBehviorIssueTypeDefiner() : base(0983218932) //1-Update class name
            {
            }

            protected override void DefineClassTypes()
            {
                AddClassDefinition(typeof(SC_HeadmanNeedsGrainIssueBehavior.SC_HeadmanNeedsGrainIssue), 1); //1-Update class name
                AddClassDefinition(typeof(SC_HeadmanNeedsGrainIssueBehavior.SC_HeadmanNeedsGrainIssueQuest), 2); //1-Update class name
            }
        }

        private void WeeklyTick()
        {
            this.CacheGrainPrice();
        }

        private void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            this.CacheGrainPrice();
        }

        private void CacheGrainPrice()
        {
            int num = 0;
            int num2 = 0;
            foreach (Settlement settlement in Settlement.All)
            {
                if (settlement.IsTown)
                {
                    num2 += settlement.Town.GetItemPrice(DefaultItems.Grain, null, false);
                    num++;
                }
                else if (settlement.IsVillage)
                {
                    num2 += settlement.Village.GetItemPrice(DefaultItems.Grain, null, false);
                    num++;
                }
            }
            this._averageGrainPriceInCalradia = num2 / num;
        }

        private const IssueBase.IssueFrequency HeadmanNeedsGrainIssueFrequency = IssueBase.IssueFrequency.VeryCommon;

        private const int TimeLimit = 10;

        private const int NearbyTownMarketGrainLimit = 50;

        private int _averageGrainPriceInCalradia;

        internal class SC_HeadmanNeedsGrainIssue : IssueBase //1-Update class name
        {
            public SC_HeadmanNeedsGrainIssue(Hero issueOwner) : base(issueOwner, CampaignTime.DaysFromNow(20f)) //1-Update class name
            {
            }

            private int NeededGrainAmount
            {
                get
                {
                    return (int)(30f + 50f * base.IssueDifficultyMultiplier);
                }
            }

            private int AlternativeSolutionNeededGold
            {
                get
                {
                    return this.NeededGrainAmount * SC_HeadmanNeedsGrainIssueBehavior.AverageGrainPriceInCalradia;
                }
            }

            protected override int AlternativeSolutionNeededMenCount
            {
                get
                {
                    return (int)(5f + 3f * base.IssueDifficultyMultiplier);
                }
            }

            protected override int AlternativeSolutionDurationInDays
            {
                get
                {
                    return (int)(10f + 7f * base.IssueDifficultyMultiplier);
                }
            }

            protected override int RewardGold
            {
                get
                {
                    return 0;
                }
            }

            private int CompanionTradeSkillLimit
            {
                get
                {
                    return (int)(150f * base.IssueDifficultyMultiplier);
                }
            }
            // <Required overrides (abstract)
            public override TextObject Title
            {
                get
                {
                    TextObject textObject = new TextObject("{=sQBBOKDD}{ISSUE_SETTLEMENT} Needs Beskar", null);
                    textObject.SetTextVariable("ISSUE_SETTLEMENT", base.IssueSettlement.Name);
                    return textObject;
                }
            }
            public override TextObject Description
            {
                get
                {
                    TextObject textObject = new TextObject("{=OJObD61e}The headman of {ISSUE_SETTLEMENT} needs grain seeds for the coming sowing season.", null);
                    textObject.SetTextVariable("ISSUE_SETTLEMENT", base.IssueSettlement.Name);
                    return textObject;
                }
            }
            protected override TextObject IssueBriefByIssueGiver //3-Update quest acceptance text
            {
                get
                {
                    return new TextObject("{=p1buAbOQ}The harvest has been poor, and rats have eaten much of our stores. We can eat less and tighten are belts, but if we don't have seed grain left over to plant, we'll starve next year.", null);
                }
            }

            protected override TextObject IssueAcceptByPlayer //3-Update quest acceptance text
            {
                get
                {
                    return new TextObject("{=vKwndBbe}Is there a way to prevent this?");
                }
            }

            protected override TextObject IssueQuestSolutionExplanationByIssueGiver //3-Update quest acceptance text
            {
                get
                {
                    TextObject textObject = new TextObject("{=nG750jQB}Grain will solve our problems. If we had {GRAIN_AMOUNT} bushels, we could use it to sow our fields. But I doubt that {NEARBY_TOWN} has so much to sell at this time of the year. {GRAIN_AMOUNT} bushels of grain costs around 1000 denars in the markets, and we don't have that!", null);
                    textObject.SetTextVariable("NEARBY_TOWN", base.IssueSettlement.Village.Bound.Name);
                    textObject.SetTextVariable("GRAIN_AMOUNT", this.NeededGrainAmount);
                    return textObject;
                }
            }

            protected override TextObject IssueQuestSolutionAcceptByPlayer //3-Update quest acceptance text
            {
                get
                {
                    return new TextObject("{=ihfuqu2S}I will find that seed grain for you.", null);
                }
            }

            protected override bool IsThereAlternativeSolution => true;

            protected override bool IsThereLordSolution => false; //not sure what this is..

            protected override TextObject IssueAlternativeSolutionExplanationByIssueGiver
            {
                get
                {
                    TextObject textObject = new TextObject("{=5NYPqKBj}I know you're busy, but maybe you can ask some of your men to find us that grain? {MEN_COUNT} men should do the job, and I'd reckon the whole affair should take two weeks. \nI'm desperate here, {?PLAYER.GENDER}madam{?}sir{\\?}... Don't let our children starve!", null);
                    textObject.SetTextVariable("MEN_COUNT", this.AlternativeSolutionNeededMenCount);
                    return textObject;
                }
            }

            protected override TextObject IssueAlternativeSolutionAcceptByPlayer
            {
                get
                {
                    TextObject textObject = new TextObject("{=HCMsvAFv}I can order one of my companions and {MEN_COUNT} men to find grain for you.", null);
                    textObject.SetTextVariable("MEN_COUNT", this.AlternativeSolutionNeededMenCount);
                    return textObject;
                }
            }

            protected override TextObject IssueAlternativeSolutionResponseByIssueGiver
            {
                get
                {
                    return new TextObject("{=k63ZKmXX}Thank you, {?PLAYER.GENDER}milady{?}sir{\\?}! You are a saviour.", null);
                }
            }

            protected override TextObject AlternativeSolutionStartLog
            {
                get
                {
                    TextObject textObject = new TextObject("{=a0UTO8tW}{ISSUE_OWNER.LINK}, the headman of {ISSUE_SETTLEMENT}, asked you to deliver {GRAIN_AMOUNT} bushels of grain to {?QUEST_GIVER.GENDER}her{?}him{\\?} to use as seeds. Otherwise the peasants cannot sow their fields and starve in the coming season. You have agreed to send your companion {COMPANION.NAME} along with {MEN_COUNT} men to find some grain and return to the village. Your men should return in {RETURN_DAYS} days.", null);
                    StringHelpers.SetCharacterProperties("ISSUE_OWNER", base.IssueOwner.CharacterObject, null, textObject, false);
                    StringHelpers.SetCharacterProperties("COMPANION", base.AlternativeSolutionHero.CharacterObject, null, textObject, false);
                    textObject.SetTextVariable("ISSUE_SETTLEMENT", base.IssueSettlement.Name);
                    textObject.SetTextVariable("GRAIN_AMOUNT", this.NeededGrainAmount);
                    textObject.SetTextVariable("RETURN_DAYS", this.AlternativeSolutionDurationInDays);
                    textObject.SetTextVariable("MEN_COUNT", this.AlternativeSolutionSentTroops.TotalManCount - 1);
                    return textObject;
                }
            }

            protected override ValueTuple<SkillObject, int> CompanionSkillAndRewardXP
            {
                get
                {
                    return new ValueTuple<SkillObject, int>(DefaultSkills.Trade, (int)(500f + 700f * base.IssueDifficultyMultiplier));
                }
            }

            protected override Dictionary<IssueEffect, float> GetIssueEffectsAndAmountInternal()
            {
                return new Dictionary<IssueEffect, float>
                {
                    {
                        DefaultIssueEffects.SettlementProsperity,
                        -0.2f
                    },
                    {
                        DefaultIssueEffects.SettlementLoyalty,
                        -0.5f
                    }
                };
            }

            protected override bool DoTroopsSatisfyAlternativeSolution(TroopRoster troopRoster, out TextObject explanation)
            {
                explanation = TextObject.Empty;
                return QuestHelper.CheckRosterForAlternativeSolution(troopRoster, this.AlternativeSolutionNeededMenCount, ref explanation, 0, false);
            }
            protected override bool CompanionOrFamilyMemberClickableCondition(Hero companion, out TextObject explanation)
            {
                explanation = TextObject.Empty;
                return QuestHelper.CheckCompanionForAlternativeSolution(companion.CharacterObject, ref explanation, this.GetAlternativeSolutionRequiredCompanionSkills(), null);
            }
            private Dictionary<SkillObject, int> GetAlternativeSolutionRequiredCompanionSkills()
            {
                return new Dictionary<SkillObject, int>
                {
                    {
                        DefaultSkills.Trade,
                        this.CompanionTradeSkillLimit
                    }
                };
            }
            protected override bool AlternativeSolutionCondition(out TextObject explanation)
            {
                Dictionary<SkillObject, int> alternativeSolutionRequiredCompanionSkills = this.GetAlternativeSolutionRequiredCompanionSkills();
                explanation = TextObject.Empty;
                return QuestHelper.CheckAllCompanionsCondition(MobileParty.MainParty.MemberRoster, ref explanation, alternativeSolutionRequiredCompanionSkills, null) && QuestHelper.CheckRosterForAlternativeSolution(MobileParty.MainParty.MemberRoster, this.AlternativeSolutionNeededMenCount, ref explanation, 0, false) && QuestHelper.CheckGoldForAlternativeSolution(this.AlternativeSolutionNeededGold, ref explanation);
            }

            protected override void AlternativeSolutionEndConsequence()
            {
                TraitLevelingHelper.OnIssueSolvedThroughAlternativeSolution(base.IssueOwner, new Tuple<TraitObject, int>[]
                {
                    new Tuple<TraitObject, int>(DefaultTraits.Generosity, 20)
                });
                base.IssueOwner.AddPower(10f);
                base.IssueSettlement.Prosperity += 50f;
                this.RelationshipChangeWithIssueOwner = 6;
            }
            protected override void AlternativeSolutionStartConsequence()
            {
                GiveGoldAction.ApplyForCharacterToParty(Hero.MainHero, base.IssueSettlement.Party, this.AlternativeSolutionNeededGold, false);
                TextObject textObject = new TextObject("{=ex6ZhAAv}You gave {DENAR}{GOLD_ICON} to companion to buy {GRAIN_AMOUNT} units of grain for the {ISSUE_OWNER.NAME}.", null);
                textObject.SetTextVariable("GRAIN_AMOUNT", this.NeededGrainAmount);
                textObject.SetTextVariable("DENAR", this.AlternativeSolutionNeededGold);
                textObject.SetTextVariable("GOLD_ICON", "{=!}<img src=\"Icons\\Coin@2x\">");
                StringHelpers.SetCharacterProperties("ISSUE_OWNER", base.IssueOwner.CharacterObject, null, textObject, false);
                InformationManager.AddQuickInformation(textObject, 0, null, "");
            }
            public override TextObject IssueAsRumorInSettlement
            {
                get
                {
                    TextObject textObject = new TextObject("{=WVobv24n}Heaven save us if {QUEST_GIVER.NAME} can't get {?QUEST_GIVER.GENDER}her{?}his{\\?} hands on more grain.", null);
                    StringHelpers.SetCharacterProperties("QUEST_GIVER", base.IssueOwner.CharacterObject, null, textObject, false);
                    return textObject;
                }
            }
            public override IssueBase.IssueFrequency GetFrequency() //VeryCommon, Common, Rare
            {
                return IssueBase.IssueFrequency.VeryCommon;
            }

            public override bool IssueStayAliveConditions() //not sure what this is
            {
                return (float)base.IssueOwner.CurrentSettlement.Village.GetItemPrice(DefaultItems.Grain, null, false) > (float)SC_HeadmanNeedsGrainIssueBehavior.AverageGrainPriceInCalradia * 1.05f;
            }
            //Not sure what the difference is between this and the "oncheckforissues" logic. Does this allow the quest to still generate but you just can't see it?
            protected override bool CanPlayerTakeQuestConditions(Hero issueGiver, out IssueBase.PreconditionFlags flag, out Hero relationHero, out SkillObject skill)
            {
                bool flag2 = issueGiver.GetRelationWithPlayer() >= -10f;
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
                this.IssueDueTime = CampaignTime.Never;
                return new SC_HeadmanNeedsGrainIssueBehavior.SC_HeadmanNeedsGrainIssueQuest(questId, base.IssueOwner, CampaignTime.DaysFromNow(10f), base.IssueDifficultyMultiplier, this.RewardGold, this.NeededGrainAmount);
            }

            protected override void OnGameLoad()
            {

            }
            // </Required overrides (abstract)

            private const int BaseReturnDays = 10;
            private const int AlternativeSolutionBaseMenCount = 5;
            private const int AlternativeSolutionSuccessGenerosityBonus = 20;
            private const int AlternativeSolutionSuccessPowerBonus = 10;
            private const int AlternativeSolutionSuccessRelationBonus = 6;
            private const int AlternativeSolutionSuccessProsperityBonus = 50;
        }
        //Quest class. For the most part, takes over the quest process after IssueBase.GenerateIssueQuest is called
        internal class SC_HeadmanNeedsGrainIssueQuest : QuestBase //1-Update class name
        {            
            // Required overrides (abstract)
            public override TextObject Title
            {
                get
                {
                    TextObject textObject = new TextObject("{=apr2dH0n}{ISSUE_SETTLEMENT} Needs Beskar", null);
                    textObject.SetTextVariable("ISSUE_SETTLEMENT", this.QuestGiver.CurrentSettlement.Name);
                    return textObject;
                }
            }
            public override bool IsRemainingTimeHidden
            {
                get
                {
                    return false;
                }
            }
            private TextObject _playerAcceptedQuestLogText
            {
                get
                {
                    TextObject textObject = new TextObject("{=5CokRxmL}{QUEST_GIVER.LINK}, the headman of the {QUEST_SETTLEMENT} asked you to deliver {GRAIN_AMOUNT} units of grain to {?QUEST_GIVER.GENDER}her{?}him{\\?} to use as seeds. Otherwise peasants cannot sow their fields and starve in the coming season. \n \n You have agreed to bring them {GRAIN_AMOUNT} units of grain as soon as possible.", null);
                    StringHelpers.SetCharacterProperties("QUEST_GIVER", this.QuestGiver.CharacterObject, null, textObject, false);
                    textObject.SetTextVariable("QUEST_SETTLEMENT", this.QuestGiver.CurrentSettlement.Name);
                    textObject.SetTextVariable("GRAIN_AMOUNT", this._neededGrainAmount);
                    return textObject;
                }
            }

            // Token: 0x17000E97 RID: 3735
            // (get) Token: 0x06003B5E RID: 15198 RVA: 0x0010251C File Offset: 0x0010071C
            private TextObject _playerHasNeededGrainsLogText
            {
                get
                {
                    TextObject textObject = new TextObject("{=vOHc5dxC}You now have enough grain seeds to complete the quest. Return to {QUEST_SETTLEMENT} to hand them over.", null);
                    textObject.SetTextVariable("QUEST_SETTLEMENT", this.QuestGiver.CurrentSettlement.Name);
                    return textObject;
                }
            }

            // Token: 0x17000E98 RID: 3736
            // (get) Token: 0x06003B5F RID: 15199 RVA: 0x00102545 File Offset: 0x00100745
            private TextObject _questTimeoutLogText
            {
                get
                {
                    TextObject textObject = new TextObject("{=brDw7ewN}You have failed to deliver {GRAIN_AMOUNT} units of grain to the villagers. They won't be able to sow them before the coming winter. The Headman and the villagers are doomed.", null);
                    textObject.SetTextVariable("GRAIN_AMOUNT", this._neededGrainAmount);
                    return textObject;
                }
            }

            // Token: 0x17000E99 RID: 3737
            // (get) Token: 0x06003B60 RID: 15200 RVA: 0x00102564 File Offset: 0x00100764
            private TextObject _successLog
            {
                get
                {
                    TextObject textObject = new TextObject("{=GGTxzAtn}You have delivered {GRAIN_AMOUNT} units of grain to the villagers. They will be able to sow them before the coming winter. You have saved a lot of lives today. The Headman and the villagers are grateful.", null);
                    textObject.SetTextVariable("GRAIN_AMOUNT", this._neededGrainAmount);
                    return textObject;
                }
            }

            // Token: 0x17000E9A RID: 3738
            // (get) Token: 0x06003B61 RID: 15201 RVA: 0x00102584 File Offset: 0x00100784
            private TextObject _cancelLogOnWarDeclared
            {
                get
                {
                    TextObject textObject = new TextObject("{=8Z4vlcib}Your clan is now at war with the {ISSUE_GIVER.LINK}'s lord. Your agreement with {ISSUE_GIVER.LINK} was canceled.", null);
                    StringHelpers.SetCharacterProperties("ISSUE_GIVER", this.QuestGiver.CharacterObject, null, textObject, false);
                    return textObject;
                }
            }

            // Token: 0x17000E9B RID: 3739
            // (get) Token: 0x06003B62 RID: 15202 RVA: 0x001025B8 File Offset: 0x001007B8
            private TextObject _cancelLogOnVillageRaided
            {
                get
                {
                    TextObject textObject = new TextObject("{=PgFJLK85}{SETTLEMENT_NAME} is raided by someone else. Your agreement with {ISSUE_GIVER.LINK} was canceled.", null);
                    textObject.SetTextVariable("SETTLEMENT_NAME", this.QuestGiver.CurrentSettlement.Name);
                    StringHelpers.SetCharacterProperties("ISSUE_GIVER", this.QuestGiver.CharacterObject, null, textObject, false);
                    return textObject;
                }
            }

            // Token: 0x06003B63 RID: 15203 RVA: 0x00102606 File Offset: 0x00100806
            public SC_HeadmanNeedsGrainIssueQuest(string questId, Hero giverHero, CampaignTime duration, float difficultyMultiplier, int rewardGold, int neededGrainAmount) : base(questId, giverHero, duration, rewardGold)
            {
                this._neededGrainAmount = neededGrainAmount;
                this._rewardGold = rewardGold;
                this.SetDialogs();
                base.InitializeQuestOnCreation();
            }

            // Token: 0x06003B64 RID: 15204 RVA: 0x0010262F File Offset: 0x0010082F
            protected override void InitializeQuestOnGameLoad()
            {
                this.SetDialogs();
            }

            // Token: 0x06003B65 RID: 15205 RVA: 0x00102638 File Offset: 0x00100838
            protected override void RegisterEvents()
            {
                CampaignEvents.PlayerInventoryExchangeEvent.AddNonSerializedListener(this, new Action<List<ValueTuple<ItemRosterElement, int>>, List<ValueTuple<ItemRosterElement, int>>, bool>(this.OnPlayerInventoryExchange));
                CampaignEvents.OnPartyConsumedFoodEvent.AddNonSerializedListener(this, new Action<MobileParty>(this.OnPartyConsumedFood));
                CampaignEvents.OnHeroSharedFoodWithAnotherHeroEvent.AddNonSerializedListener(this, new Action<Hero, Hero, float>(this.OnHeroSharedFoodWithAnotherHero));
                CampaignEvents.WarDeclared.AddNonSerializedListener(this, new Action<IFaction, IFaction>(this.OnWarDeclared));
                CampaignEvents.RaidCompletedEvent.AddNonSerializedListener(this, new Action<BattleSideEnum, MapEvent>(this.OnRaidCompleted));
            }

            // Token: 0x06003B66 RID: 15206 RVA: 0x001026B8 File Offset: 0x001008B8
            protected override void OnTimedOut()
            {
                base.AddLog(this._questTimeoutLogText, false);
                this.Fail();
            }

            // Token: 0x06003B67 RID: 15207 RVA: 0x001026D0 File Offset: 0x001008D0
            protected override void SetDialogs()
            {
                this.OfferDialogFlow = DialogFlow.CreateDialogFlow("issue_classic_quest_start", 100).NpcLine(new TextObject("{=k63ZKmXX}Thank you, {?PLAYER.GENDER}milady{?}sir{\\?}! You are a saviour.", null), null, null).Condition(() => CharacterObject.OneToOneConversationCharacter == this.QuestGiver.CharacterObject).Consequence(new ConversationSentence.OnConsequenceDelegate(this.QuestAcceptedConsequences)).CloseDialog();
                this.DiscussDialogFlow = DialogFlow.CreateDialogFlow("quest_discuss", 100).NpcLine(new TextObject("{=Zsn6kpjt}Have you brought {GRAIN_AMOUNT} bushels of grain?", null), null, null).Condition(delegate
                {
                    MBTextManager.SetTextVariable("GRAIN_AMOUNT", this._neededGrainAmount);
                    return CharacterObject.OneToOneConversationCharacter == this.QuestGiver.CharacterObject;
                }).BeginPlayerOptions().PlayerOption(new TextObject("{=9UABeRWO}Yes. Here is your grain.", null), null).ClickableCondition(new ConversationSentence.OnClickableConditionDelegate(this.ReturnGrainsClickableConditions)).NpcLine(new TextObject("{=k63ZKmXX}Thank you {?PLAYER.GENDER}milady{?}sir{\\?}! You are a saviour.", null), null, null).Consequence(delegate
                {
                    Campaign.Current.ConversationManager.ConversationEndOneShot += this.Success;
                }).CloseDialog().PlayerOption(new TextObject("{=PI6ikMsc}I'm working on it.", null), null).NpcLine(new TextObject("{=HeIIW3EH}We await your success, {?PLAYER.GENDER}milady{?}sir{\\?}.", null), null, null).CloseDialog().EndPlayerOptions().CloseDialog();
            }

            // Token: 0x06003B68 RID: 15208 RVA: 0x001027DE File Offset: 0x001009DE
            private bool ReturnGrainsClickableConditions(out TextObject explanation)
            {
                if (this._playerAcceptedQuestLog.CurrentProgress >= this._neededGrainAmount)
                {
                    explanation = TextObject.Empty;
                    return true;
                }
                explanation = new TextObject("{=mzabdwoh}You don't have enough grain.", null);
                return false;
            }

            // Token: 0x06003B69 RID: 15209 RVA: 0x0010280C File Offset: 0x00100A0C
            private void QuestAcceptedConsequences()
            {
                base.StartQuest();
                int requiredGrainCountOnPlayer = this.GetRequiredGrainCountOnPlayer();
                this._playerAcceptedQuestLog = base.AddDiscreteLog(this._playerAcceptedQuestLogText, new TextObject("{=eEwI880g}Collect Grain", null), requiredGrainCountOnPlayer, this._neededGrainAmount, null, false);
            }

            // Token: 0x06003B6A RID: 15210 RVA: 0x0010284C File Offset: 0x00100A4C
            private int GetRequiredGrainCountOnPlayer()
            {
                int itemNumber = PartyBase.MainParty.ItemRoster.GetItemNumber(DefaultItems.Grain);
                if (itemNumber >= this._neededGrainAmount)
                {
                    TextObject textObject = new TextObject("{=Gtbfm10o}You have enough grain to complete the quest. Return to {QUEST_SETTLEMENT} to hand it over.", null);
                    textObject.SetTextVariable("QUEST_SETTLEMENT", this.QuestGiver.CurrentSettlement.Name);
                    InformationManager.AddQuickInformation(textObject, 0, null, "");
                }
                if (itemNumber <= this._neededGrainAmount)
                {
                    return itemNumber;
                }
                return this._neededGrainAmount;
            }

            // Token: 0x06003B6B RID: 15211 RVA: 0x001028BC File Offset: 0x00100ABC
            private void CheckIfPlayerReadyToReturnGrains()
            {
                if (this._playerHasNeededGrainsLog == null && this._playerAcceptedQuestLog.CurrentProgress >= this._neededGrainAmount)
                {
                    this._playerHasNeededGrainsLog = base.AddLog(this._playerHasNeededGrainsLogText, false);
                    return;
                }
                if (this._playerHasNeededGrainsLog != null && this._playerAcceptedQuestLog.CurrentProgress < this._neededGrainAmount)
                {
                    base.RemoveLog(this._playerHasNeededGrainsLog);
                    this._playerHasNeededGrainsLog = null;
                }
            }

            // Token: 0x06003B6C RID: 15212 RVA: 0x00102928 File Offset: 0x00100B28
            private void OnPlayerInventoryExchange(List<ValueTuple<ItemRosterElement, int>> purchasedItems, List<ValueTuple<ItemRosterElement, int>> soldItems, bool isTrading)
            {
                bool flag = false;
                foreach (ValueTuple<ItemRosterElement, int> valueTuple in purchasedItems)
                {
                    ItemRosterElement item = valueTuple.Item1;
                    if (item.EquipmentElement.Item == DefaultItems.Grain)
                    {
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    foreach (ValueTuple<ItemRosterElement, int> valueTuple2 in soldItems)
                    {
                        ItemRosterElement item = valueTuple2.Item1;
                        if (item.EquipmentElement.Item == DefaultItems.Grain)
                        {
                            flag = true;
                            break;
                        }
                    }
                }
                if (flag)
                {
                    this._playerAcceptedQuestLog.UpdateCurrentProgress(this.GetRequiredGrainCountOnPlayer());
                    this.CheckIfPlayerReadyToReturnGrains();
                }
            }

            // Token: 0x06003B6D RID: 15213 RVA: 0x00102A0C File Offset: 0x00100C0C
            private void OnPartyConsumedFood(MobileParty party)
            {
                if (party.IsMainParty)
                {
                    this._playerAcceptedQuestLog.UpdateCurrentProgress(this.GetRequiredGrainCountOnPlayer());
                    this.CheckIfPlayerReadyToReturnGrains();
                }
            }

            // Token: 0x06003B6E RID: 15214 RVA: 0x00102A2D File Offset: 0x00100C2D
            private void OnHeroSharedFoodWithAnotherHero(Hero supporterHero, Hero supportedHero, float influence)
            {
                if (supporterHero == Hero.MainHero || supportedHero == Hero.MainHero)
                {
                    this._playerAcceptedQuestLog.UpdateCurrentProgress(this.GetRequiredGrainCountOnPlayer());
                    this.CheckIfPlayerReadyToReturnGrains();
                }
            }

            // Token: 0x06003B6F RID: 15215 RVA: 0x00102A56 File Offset: 0x00100C56
            private void OnWarDeclared(IFaction faction1, IFaction faction2)
            {
                if (this.QuestGiver.CurrentSettlement.OwnerClan.IsAtWarWith(Clan.PlayerClan))
                {
                    base.CompleteQuestWithCancel(this._cancelLogOnWarDeclared);
                }
            }

            // Token: 0x06003B70 RID: 15216 RVA: 0x00102A80 File Offset: 0x00100C80
            private void OnRaidCompleted(BattleSideEnum battleSide, MapEvent mapEvent)
            {
                if (mapEvent.MapEventSettlement == this.QuestGiver.CurrentSettlement)
                {
                    base.CompleteQuestWithCancel(this._cancelLogOnVillageRaided);
                }
            }

            // Token: 0x06003B71 RID: 15217 RVA: 0x00102AA4 File Offset: 0x00100CA4
            private void Success()
            {
                base.CompleteQuestWithSuccess();
                base.AddLog(this._successLog, false);
                TraitLevelingHelper.OnIssueSolvedThroughQuest(this.QuestGiver, new Tuple<TraitObject, int>[]
                {
                    new Tuple<TraitObject, int>(DefaultTraits.Mercy, 50),
                    new Tuple<TraitObject, int>(DefaultTraits.Generosity, 30)
                });
                GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, this._rewardGold, false);
                GiveItemAction.ApplyForParties(PartyBase.MainParty, Settlement.CurrentSettlement.Party, DefaultItems.Grain, this._neededGrainAmount);
                this.QuestGiver.AddPower(10f);
                Settlement.CurrentSettlement.Prosperity += 50f;
                this.RelationshipChangeWithQuestGiver = 8;
                ChangeRelationAction.ApplyPlayerRelation(this.QuestGiver, this.RelationshipChangeWithQuestGiver, true, true);
            }

            // Token: 0x06003B72 RID: 15218 RVA: 0x00102B68 File Offset: 0x00100D68
            private void Fail()
            {
                this.QuestGiver.AddPower(-5f);
                this.QuestGiver.CurrentSettlement.Prosperity += -10f;
                this.RelationshipChangeWithQuestGiver = -5;
                ChangeRelationAction.ApplyPlayerRelation(this.QuestGiver, this.RelationshipChangeWithQuestGiver, true, true);
            }

            // Token: 0x040016DA RID: 5850
            private const int SuccessMercyBonus = 50;

            // Token: 0x040016DB RID: 5851
            private const int SuccessGenerosityBonus = 30;

            // Token: 0x040016DC RID: 5852
            private const int SuccessRelationBonus = 8;

            // Token: 0x040016DD RID: 5853
            private const int SuccessPowerBonus = 10;

            // Token: 0x040016DE RID: 5854
            private const int SuccessProsperityBonus = 50;

            // Token: 0x040016DF RID: 5855
            private const int FailRelationPenalty = -5;

            // Token: 0x040016E0 RID: 5856
            private const int TimeOutProsperityPenalty = -10;

            // Token: 0x040016E1 RID: 5857
            private const int TimeOutPowerPenalty = -5;

            // Token: 0x040016E2 RID: 5858
            [SaveableField(10)]
            private readonly int _neededGrainAmount;

            // Token: 0x040016E3 RID: 5859
            [SaveableField(20)]
            private int _rewardGold;

            // Token: 0x040016E4 RID: 5860
            [SaveableField(30)]
            private JournalLog _playerAcceptedQuestLog;

            // Token: 0x040016E5 RID: 5861
            [SaveableField(40)]
            private JournalLog _playerHasNeededGrainsLog;
        }
    }
}
