using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using SandBox.Source.Missions.Handlers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment.Managers;
using TaleWorlds.CampaignSystem.Conversation.Persuasion;
using TaleWorlds.CampaignSystem.SandBox;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;
using SandBox;

namespace SC_Teaser_Quests
{
	public class SC_NearbyBanditBaseIssueBehavior : CampaignBehaviorBase
	{
		private Settlement FindSuitableHideout(Hero issueOwner)
		{
			Settlement result = null;
			float num = float.MaxValue;
			foreach (Settlement settlement in from t in Settlement.All
											  where t.IsHideout() && t.Hideout.IsInfested
											  select t)
			{
				float num2 = settlement.GatePosition.DistanceSquared(issueOwner.GetMapPoint().Position2D);
				if (num2 <= 1225f && num2 < num)
				{
					num = num2;
					result = settlement;
				}
			}
			return result;
		}

		private void OnCheckForIssue(IssueArgs issueArgs)
		{
			if (issueArgs.IssueOwner.IsNotable)
			{
				Settlement settlement = this.FindSuitableHideout(issueArgs.IssueOwner);
				if (this.ConditionsHold(issueArgs.IssueOwner) && settlement != null)
				{
					issueArgs.SetPotentialIssueData(new PotentialIssueData(new Func<PotentialIssueData, Hero, IssueBase>(this.OnIssueSelected), typeof(SC_NearbyBanditBaseIssueBehavior.SC_NearbyBanditBaseIssue), IssueBase.IssueFrequency.VeryCommon, settlement));
					return;
				}
				issueArgs.SetPotentialIssueData(new PotentialIssueData(typeof(SC_NearbyBanditBaseIssueBehavior.SC_NearbyBanditBaseIssue), IssueBase.IssueFrequency.VeryCommon));
			}
		}

		private IssueBase OnIssueSelected(PotentialIssueData pid, Hero issueOwner)
		{
			//InformationManager.DisplayMessage(new InformationMessage(GameTexts.FindText("sc_quest_dialog_test").ToString()));
			return new SC_NearbyBanditBaseIssueBehavior.SC_NearbyBanditBaseIssue(issueOwner, pid.RelatedObject as Settlement);
		}

		private bool ConditionsHold(Hero issueGiver)
		{
			return issueGiver.IsHeadman && issueGiver.CurrentSettlement != null && issueGiver.CurrentSettlement.Village.Bound.Town.Security <= 50f;
		}

		private void OnIssueUpdated(IssueBase issue, IssueBase.IssueUpdateDetails details, Hero issueSolver = null)
		{
			if (issue is SC_NearbyBanditBaseIssueBehavior.SC_NearbyBanditBaseIssue && details == IssueBase.IssueUpdateDetails.IssueFinishedByAILord)
			{
				foreach (MobileParty mobileParty in ((SC_NearbyBanditBaseIssueBehavior.SC_NearbyBanditBaseIssue)issue).TargetHideout.Parties)
				{
					mobileParty.SetMovePatrolAroundSettlement(((SC_NearbyBanditBaseIssueBehavior.SC_NearbyBanditBaseIssue)issue).TargetHideout);
				}
			}
		}

		public override void RegisterEvents()
		{
			CampaignEvents.OnCheckForIssueEvent.AddNonSerializedListener(this, new Action<IssueArgs>(this.OnCheckForIssue));
			CampaignEvents.OnIssueUpdatedEvent.AddNonSerializedListener(this, new Action<IssueBase, IssueBase.IssueUpdateDetails, Hero>(this.OnIssueUpdated));
		}

		public override void SyncData(IDataStore dataStore)
		{
		}

		private const int NearbyHideoutMaxRange = 35;

		private const IssueBase.IssueFrequency NearbyHideoutIssueFrequency = IssueBase.IssueFrequency.VeryCommon;

		public class SC_NearbyBanditBaseIssueTypeDefiner : SaveableTypeDefiner
		{
			public SC_NearbyBanditBaseIssueTypeDefiner() : base(0983216728)
			{
			}

			protected override void DefineClassTypes()
			{
				base.AddClassDefinition(typeof(SC_NearbyBanditBaseIssueBehavior.SC_NearbyBanditBaseIssue), 1);
				base.AddClassDefinition(typeof(SC_NearbyBanditBaseIssueBehavior.SC_NearbyBanditBaseIssueQuest), 2);
			}
		}

		internal class SC_NearbyBanditBaseIssue : IssueBase
		{
			protected override int AlternativeSolutionNeededMenCount
			{
				get
				{
					return 10;
				}
			}

			protected override int AlternativeSolutionDurationInDays
			{
				get
				{
					return 20;
				}
			}

			protected override int RewardGold
			{
				get
				{
					return 2000;
				}
			}

			internal Settlement TargetHideout
			{
				get
				{
					return this._targetHideout;
				}
			}

			protected override TextObject IssueBriefByIssueGiver
			{
				get
				{
					return new TextObject("{=vw2Q9jJH}There's this old ruin, a place that offers a good view of the roads, and is yet hard to reach. Needless to say, it attracts bandits. A new gang has moved in and they have been giving hell to the caravans and travellers passing by.", null);
				}
			}

			protected override TextObject IssueAcceptByPlayer
			{
				get
				{
					return new TextObject("{=IqH0jFdK}So you need someone to deal with these bastards?", null);
				}
			}

			protected override TextObject IssueQuestSolutionExplanationByIssueGiver
			{
				get
				{
					return new TextObject("{=zstiYI49}Any bandits there can easily spot and evade a large army moving against them, but if you can enter the hideout with a small group of determined warriors you can catch them unawares.", null);
				}
			}

			protected override TextObject IssueQuestSolutionAcceptByPlayer
			{
				get
				{
					return new TextObject("{=uhYprSnG}I will go to the hideout myself and ambush the bandits.", null);
				}
			}

			public override bool CanBeCompletedByAI()
			{
				return Hero.MainHero.PartyBelongedToAsPrisoner != this._targetHideout.Party;
			}

			protected override TextObject IssueAlternativeSolutionAcceptByPlayer
			{
				get
				{
					TextObject textObject = new TextObject("{=IFasMslv}I will assign a companion with {TROOP_COUNT} good men for {RETURN_DAYS} days.", null);
					textObject.SetTextVariable("TROOP_COUNT", this.AlternativeSolutionNeededMenCount);
					textObject.SetTextVariable("RETURN_DAYS", this.AlternativeSolutionDurationInDays);
					return textObject;
				}
			}

			protected override TextObject IssueAlternativeSolutionResponseByIssueGiver
			{
				get
				{
					TextObject textObject = new TextObject("{=aXOgAKfj}Thank you, {?PLAYER.GENDER}madam{?}sir{\\?}. I hope your people will be successful.", null);
					StringHelpers.SetCharacterProperties("PLAYER", Hero.MainHero.CharacterObject, null, textObject, false);
					return textObject;
				}
			}

			protected override TextObject IssueAlternativeSolutionExplanationByIssueGiver
			{
				get
				{
					TextObject textObject = new TextObject("{=VNXgZ8mt}Alternatively, if you can assign a companion with {TROOP_COUNT} or so men to this task, they can do the job.", null);
					textObject.SetTextVariable("TROOP_COUNT", this.AlternativeSolutionNeededMenCount);
					return textObject;
				}
			}

			public override TextObject IssueAsRumorInSettlement
			{
				get
				{
					TextObject textObject = new TextObject("{=ctgihUte}I hope {QUEST_GIVER.NAME} has a plan to get rid of those bandits.", null);
					StringHelpers.SetCharacterProperties("QUEST_GIVER", base.IssueOwner.CharacterObject, null, textObject, false);
					return textObject;
				}
			}

			protected override bool IsThereAlternativeSolution
			{
				get
				{
					return true;
				}
			}

			protected override TextObject AlternativeSolutionStartLog
			{
				get
				{
					TextObject textObject = new TextObject("{=G4kpabSf}{ISSUE_GIVER.LINK}, a merchant from {ISSUE_SETTLEMENT}, has told you about recent bandit attacks on local caravans and villagers and asked you to clear out the outlaws' hideout. You asked {COMPANION.LINK} to take {TROOP_COUNT} of your best men to go and take care of it. They should report back to you in {RETURN_DAYS} days.", null);
					StringHelpers.SetCharacterProperties("PLAYER", Hero.MainHero.CharacterObject, null, textObject, false);
					StringHelpers.SetCharacterProperties("ISSUE_GIVER", base.IssueOwner.CharacterObject, null, textObject, false);
					StringHelpers.SetCharacterProperties("COMPANION", base.IssueOwner.CharacterObject, null, textObject, false);
					textObject.SetTextVariable("ISSUE_SETTLEMENT", this._issueSettlement.EncyclopediaLinkWithName);
					textObject.SetTextVariable("TROOP_COUNT", this.AlternativeSolutionSentTroops.TotalManCount - 1);
					textObject.SetTextVariable("RETURN_DAYS", this.AlternativeSolutionDurationInDays);
					return textObject;
				}
			}

			protected override bool IsThereLordSolution
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
					TextObject textObject = new TextObject("SW: {=ENYbLO8r}Bandit Base Near {SETTLEMENT}", null);
					textObject.SetTextVariable("SETTLEMENT", this._issueSettlement.Name);
					return textObject;
				}
			}

			public override TextObject Description
			{
				get
				{
					TextObject textObject = new TextObject("{=vZ01a4cG}{QUEST_GIVER.LINK} wants you to clear the hideout that attracts more bandits to {?QUEST_GIVER.GENDER}her{?}his{\\?} region.", null);
					StringHelpers.SetCharacterProperties("QUEST_GIVER", base.IssueOwner.CharacterObject, null, textObject, false);
					return textObject;
				}
			}

			protected override bool IssueQuestCanBeDuplicated
			{
				get
				{
					return false;
				}
			}

			public SC_NearbyBanditBaseIssue(Hero issueOwner, Settlement targetHideout) : base(issueOwner, CampaignTime.DaysFromNow(60f))
			{
				this._targetHideout = targetHideout;
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
						DefaultIssueEffects.SettlementSecurity,
						-1f
					}
				};
			}

			protected override void AfterIssueCreation()
			{
				this._issueSettlement = base.IssueOwner.CurrentSettlement;
			}

			protected override bool DoTroopsSatisfyAlternativeSolution(TroopRoster troopRoster, out TextObject explanation)
			{
				explanation = TextObject.Empty;
				return QuestHelper.CheckRosterForAlternativeSolution(troopRoster, this.AlternativeSolutionNeededMenCount, ref explanation, 2, false);
			}

			protected override bool IsTroopTypeNeededByAlternativeSolution(CharacterObject character)
			{
				return character.Tier >= 2;
			}

			private void GetAlternativeSolutionRequiredCompanionSkills(out Dictionary<SkillObject, int> shouldHaveAll, out Dictionary<SkillObject, int> shouldHaveOneOfThem)
			{
				shouldHaveAll = new Dictionary<SkillObject, int>();
				shouldHaveAll.Add(DefaultSkills.Tactics, 30);
				shouldHaveOneOfThem = new Dictionary<SkillObject, int>();
				shouldHaveOneOfThem.Add(DefaultSkills.OneHanded, 50);
				shouldHaveOneOfThem.Add(DefaultSkills.TwoHanded, 50);
				shouldHaveOneOfThem.Add(DefaultSkills.Polearm, 50);
			}

			protected override bool AlternativeSolutionCondition(out TextObject explanation)
			{
				Dictionary<SkillObject, int> shouldHaveAll;
				Dictionary<SkillObject, int> shouldHaveOneOfThem;
				this.GetAlternativeSolutionRequiredCompanionSkills(out shouldHaveAll, out shouldHaveOneOfThem);
				explanation = TextObject.Empty;
				return QuestHelper.CheckAllCompanionsCondition(MobileParty.MainParty.MemberRoster, ref explanation, shouldHaveAll, shouldHaveOneOfThem) && QuestHelper.CheckRosterForAlternativeSolution(MobileParty.MainParty.MemberRoster, this.AlternativeSolutionNeededMenCount, ref explanation, 2, false);
			}

			protected override bool CompanionOrFamilyMemberClickableCondition(Hero companion, out TextObject explanation)
			{
				Dictionary<SkillObject, int> shouldHaveAll;
				Dictionary<SkillObject, int> shouldHaveOneOfThem;
				this.GetAlternativeSolutionRequiredCompanionSkills(out shouldHaveAll, out shouldHaveOneOfThem);
				explanation = TextObject.Empty;
				return QuestHelper.CheckCompanionForAlternativeSolution(companion.CharacterObject, ref explanation, shouldHaveAll, shouldHaveOneOfThem);
			}

			protected override void AlternativeSolutionEndConsequence()
			{
				this.RelationshipChangeWithIssueOwner = 5;
				base.IssueOwner.AddPower(5f);
				this._issueSettlement.Prosperity += 10f;
				float randomFloat = MBRandom.RandomFloat;
				SkillObject skill;
				if (randomFloat <= 0.33f)
				{
					skill = DefaultSkills.OneHanded;
				}
				else if (randomFloat <= 0.66f)
				{
					skill = DefaultSkills.TwoHanded;
				}
				else
				{
					skill = DefaultSkills.Polearm;
				}
				base.AlternativeSolutionHero.AddSkillXp(skill, (float)((int)(1000f + 1250f * base.IssueDifficultyMultiplier)));
				foreach (TroopRosterElement troopRosterElement in this.AlternativeSolutionSentTroops)
				{
					MobileParty.MainParty.MemberRoster.AddXpToTroop(5, troopRosterElement.Character);
				}
			}

			protected override void OnGameLoad()
			{
			}

			protected override QuestBase GenerateIssueQuest(string questId)
			{
				return new SC_NearbyBanditBaseIssueBehavior.SC_NearbyBanditBaseIssueQuest(questId, base.IssueOwner, this._targetHideout, this._issueSettlement, this.RewardGold, CampaignTime.DaysFromNow(30f));
			}

			public override IssueBase.IssueFrequency GetFrequency()
			{
				return IssueBase.IssueFrequency.VeryCommon;
			}

			protected override bool CanPlayerTakeQuestConditions(Hero issueGiver, out IssueBase.PreconditionFlags flags, out Hero relationHero, out SkillObject skill)
			{
				flags = IssueBase.PreconditionFlags.None;
				relationHero = null;
				skill = null;
				if (issueGiver.GetRelationWithPlayer() < -10f)
				{
					flags |= IssueBase.PreconditionFlags.Relation;
					relationHero = issueGiver;
				}
				if (FactionManager.IsAtWarAgainstFaction(issueGiver.MapFaction, Hero.MainHero.MapFaction))
				{
					flags |= IssueBase.PreconditionFlags.AtWar;
				}
				return flags == IssueBase.PreconditionFlags.None;
			}

			public override bool IssueStayAliveConditions()
			{
				return this._targetHideout.Hideout.IsInfested && base.IssueOwner.CurrentSettlement.IsVillage && base.IssueOwner.CurrentSettlement.Village.Bound.Town.Security <= 80f;
			}

			protected override void CompleteIssueWithTimedOutConsequences()
			{
			}

			private const int AlternativeSolutionFinalMenCount = 10;

			private const int AlternativeSolutionMinimumTroopTier = 2;

			private const int AlternativeSolutionCompanionMeleeSkillThreshold = 50;

			private const int AlternativeSolutionCompanionTacticsSkillThreshold = 30;

			private const int AlternativeSolutionReturnTimeInDays = 20;

			private const int AlternativeSolutionRelationRewardOnSuccess = 5;

			private const int IssueOwnerPowerBonusOnSuccess = 5;

			private const int SettlementProsperityBonusOnSuccess = 10;

			private const int IssueDurationInDays = 60;

			private const int IssueQuestDuration = 30;

			[SaveableField(100)]
			private readonly Settlement _targetHideout;

			[SaveableField(101)]
			private Settlement _issueSettlement;
		}

		internal class SC_NearbyBanditBaseIssueQuest : QuestBase
		{
			public override TextObject Title
			{
				get
				{
					TextObject textObject = new TextObject("SW: {=ENYbLO8r}Bandit Base Near {SETTLEMENT}", null);
					textObject.SetTextVariable("SETTLEMENT", this._questSettlement.Name);
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

			private TextObject _onQuestStartedLogText
			{
				get
				{
					TextObject textObject = new TextObject("{=ogsh3V6G}{QUEST_GIVER.LINK}, a merchant from {QUEST_SETTLEMENT}, has told you about the hideout of some bandits who have recently been attacking local caravans and villagers. You told {?QUEST_GIVER.GENDER}her{?}him{\\?} that you will take care of the situation yourself. {QUEST_GIVER.LINK} also marked the location of the hideout on your map.", null);
					StringHelpers.SetCharacterProperties("QUEST_GIVER", this.QuestGiver.CharacterObject, null, textObject, false);
					textObject.SetTextVariable("QUEST_SETTLEMENT", this._questSettlement.EncyclopediaLinkWithName);
					return textObject;
				}
			}

			private TextObject _onQuestSucceededLogText
			{
				get
				{
					TextObject textObject = new TextObject("{=SN3pjZiK}You received a message from {QUEST_GIVER.LINK}.\n\"Thank you for clearing out that bandits' nest. Please accept thes {REWARD} denars with our gratitude.\"", null);
					StringHelpers.SetCharacterProperties("QUEST_GIVER", this.QuestGiver.CharacterObject, null, textObject, false);
					textObject.SetTextVariable("REWARD", this.RewardGold);
					return textObject;
				}
			}

			private TextObject _onQuestFailedLogText
			{
				get
				{
					TextObject textObject = new TextObject("{=qsMnnfQ3}You failed to clear the hideout in time to prevent further attacks. {QUEST_GIVER.LINK} is disappointed.", null);
					StringHelpers.SetCharacterProperties("QUEST_GIVER", this.QuestGiver.CharacterObject, null, textObject, false);
					return textObject;
				}
			}

			private TextObject _onQuestCanceledLogText
			{
				get
				{
					TextObject textObject = new TextObject("{=4Bub0GY6}Hideout was cleared by someone else. Your agreement with {QUEST_GIVER.LINK} is canceled.", null);
					StringHelpers.SetCharacterProperties("QUEST_GIVER", this.QuestGiver.CharacterObject, null, textObject, false);
					return textObject;
				}
			}

			public SC_NearbyBanditBaseIssueQuest(string questId, Hero questGiver, Settlement targetHideout, Settlement questSettlement, int rewardGold, CampaignTime duration) : base(questId, questGiver, duration, rewardGold)
			{
				this._targetHideout = targetHideout;
				this._questSettlement = questSettlement;
				this.SetDialogs();
				base.InitializeQuestOnCreation();
			}

			protected override void InitializeQuestOnGameLoad()
			{
				this.SetDialogs();
			}

			protected override void SetDialogs()
			{
				this.OfferDialogFlow = DialogFlow.CreateDialogFlow("issue_classic_quest_start", 100).NpcLine("{=spj8bYVo}Good! I'll mark the hideout for you on a map.", null, null).Condition(() => Hero.OneToOneConversationHero == this.QuestGiver).Consequence(new ConversationSentence.OnConsequenceDelegate(this.OnQuestAccepted)).CloseDialog();
				this.DiscussDialogFlow = DialogFlow.CreateDialogFlow("quest_discuss", 100).NpcLine("{=l9wYpIuV}Any news? Have you managed to clear out the hideout yet?", null, null).Condition(() => Hero.OneToOneConversationHero == this.QuestGiver).BeginPlayerOptions().PlayerOption("{=wErSpkjy}I'm still working on it.", null).NpcLine("{=XTt6gZ7h}Do make haste, if you can. As long as those bandits are up there, no traveller is safe!", null, null).CloseDialog().PlayerOption("{=I8raOMRH}Sorry. No progress yet.", null).NpcLine("{=kWruAXaF}Well... You know as long as those bandits remain there, no traveller is safe.", null, null).CloseDialog().EndPlayerOptions().CloseDialog();
			}

			private void OnQuestAccepted()
			{
				base.StartQuest();
				this._targetHideout.Hideout.IsSpotted = true;
				this._targetHideout.IsVisible = true;
				base.AddTrackedObject(this._targetHideout);
				QuestHelper.AddMapArrowFromPointToTarget(new TextObject("{=xpsQyPaV}Direction to Bandits", null), this._questSettlement.Position2D, this._targetHideout.Position2D, 5f, 0.1f, 1056731);
				TextObject textObject = new TextObject("{=XGa8MkbJ}{QUEST_GIVER.NAME} has marked the hideout on your map", null);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", this.QuestGiver.CharacterObject, null, textObject, false);
				InformationManager.AddQuickInformation(textObject, 0, null, "");
				base.AddLog(this._onQuestStartedLogText, false);
			}

			private void OnQuestSucceeded()
			{
				base.AddLog(this._onQuestSucceededLogText, false);
				GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, this.RewardGold, false);
				GainRenownAction.Apply(Hero.MainHero, 1f, false);
				TraitLevelingHelper.OnIssueSolvedThroughQuest(this.QuestGiver, new Tuple<TraitObject, int>[]
				{
					new Tuple<TraitObject, int>(DefaultTraits.Honor, 50)
				});
				this.QuestGiver.AddPower(5f);
				this.RelationshipChangeWithQuestGiver = 5;
				this._questSettlement.Prosperity += 10f;
				base.CompleteQuestWithSuccess();
			}

			private void OnQuestFailed(bool isTimedOut)
			{
				base.AddLog(this._onQuestFailedLogText, false);
				this.RelationshipChangeWithQuestGiver = -5;
				this.QuestGiver.AddPower(-5f);
				this._questSettlement.Prosperity += -10f;
				if (!isTimedOut)
				{
					base.CompleteQuestWithFail(null);
				}
			}

			private void OnQuestCanceled()
			{
				base.AddLog(this._onQuestCanceledLogText, false);
				base.CompleteQuestWithFail(null);
			}

			protected override void OnTimedOut()
			{
				this.OnQuestFailed(true);
			}

			protected override void RegisterEvents()
			{
				CampaignEvents.MapEventEnded.AddNonSerializedListener(this, new Action<MapEvent>(this.OnMapEventEnded));
				CampaignEvents.OnHideoutClearedEvent.AddNonSerializedListener(this, new Action<Settlement>(this.OnHideoutCleared));
			}

			private void OnHideoutCleared(Settlement hideout)
			{
				if (this._targetHideout == hideout)
				{
					base.CompleteQuestWithCancel(null);
				}
			}

			private void OnMapEventEnded(MapEvent mapEvent)
			{
				if (mapEvent.IsHideoutBattle && mapEvent.MapEventSettlement == this._targetHideout)
				{
					if (mapEvent.InvolvedParties.Contains(PartyBase.MainParty))
					{
						if (mapEvent.BattleState == BattleState.DefenderVictory)
						{
							this.OnQuestFailed(false);
							return;
						}
						if (mapEvent.BattleState == BattleState.AttackerVictory)
						{
							this.OnQuestSucceeded();
							return;
						}
					}
					else if (mapEvent.BattleState == BattleState.AttackerVictory)
					{
						this.OnQuestCanceled();
					}
				}
			}

			private const int QuestGiverRelationBonus = 5;

			private const int QuestGiverRelationPenalty = -5;

			private const int QuestGiverPowerBonus = 5;

			private const int QuestGiverPowerPenalty = -5;

			private const int TownProsperityBonus = 10;

			private const int TownProsperityPenalty = -10;

			private const int TownSecurityPenalty = -10;

			[SaveableField(100)]
			private readonly Settlement _targetHideout;

			[SaveableField(101)]
			private readonly Settlement _questSettlement;

			private const int questGuid = 1056731;
		}
	}
}
