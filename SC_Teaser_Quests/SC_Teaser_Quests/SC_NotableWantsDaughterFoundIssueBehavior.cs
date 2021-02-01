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
		public class SC_NotableWantsDaughterFoundIssueBehavior : CampaignBehaviorBase
		{			
			public override void RegisterEvents()
			{
				CampaignEvents.OnCheckForIssueEvent.AddNonSerializedListener(this, new Action<IssueArgs>(this.OnCheckForIssue));
			}

			public void OnCheckForIssue(IssueArgs issueArgs)
			{
				if (this.ConditionsHold(issueArgs.IssueOwner))
				{
					issueArgs.SetPotentialIssueData(new PotentialIssueData(new Func<PotentialIssueData, Hero, IssueBase>(this.OnStartIssue), typeof(SC_NotableWantsDaughterFoundIssueBehavior.SC_NotableWantsDaughterFoundIssue), IssueBase.IssueFrequency.Rare, null));
					return;
				}
				issueArgs.SetPotentialIssueData(new PotentialIssueData(typeof(SC_NotableWantsDaughterFoundIssueBehavior.SC_NotableWantsDaughterFoundIssue), IssueBase.IssueFrequency.Rare));
			}

			private bool ConditionsHold(Hero issueGiver)
			{
				return issueGiver.IsRuralNotable && issueGiver.CurrentSettlement.IsVillage && issueGiver.CurrentSettlement.Village.TradeBound.BoundVillages.Count > 2 && !issueGiver.IsOccupiedByAnEvent() && issueGiver.Age > (float)(Campaign.Current.Models.AgeModel.HeroComesOfAge * 2) && CharacterObject.Templates.Any((CharacterObject x) => issueGiver.CurrentSettlement.Culture == x.Culture && x.Occupation == Occupation.Wanderer && x.IsFemale) && CharacterObject.Templates.Any((CharacterObject x) => issueGiver.CurrentSettlement.Culture == x.Culture && x.Occupation == Occupation.GangLeader && !x.IsFemale) && issueGiver.GetTraitLevel(DefaultTraits.Mercy) <= 0 && issueGiver.GetTraitLevel(DefaultTraits.Generosity) <= 0;
			}

			private IssueBase OnStartIssue(PotentialIssueData pid, Hero issueOwner)
			{
				return new SC_NotableWantsDaughterFoundIssueBehavior.SC_NotableWantsDaughterFoundIssue(issueOwner);
			}

			public override void SyncData(IDataStore dataStore)
			{
			}

			private const IssueBase.IssueFrequency SC_NotableWantsDaughterFoundIssueFrequency = IssueBase.IssueFrequency.Rare;

			private const int IssueDuration = 20;

			private const int BaseRewardGold = 500;

			private const int AlternativeSolutionBaseMenCount = 16;

			public class SC_NotableWantsDaughterFoundIssueTypeDefiner : CampaignBehaviorBase.SaveableCampaignBehaviorTypeDefiner
			{
				public SC_NotableWantsDaughterFoundIssueTypeDefiner() : base(0983219288)
				{
				}

				protected override void DefineClassTypes()
				{
					base.AddClassDefinition(typeof(SC_NotableWantsDaughterFoundIssueBehavior.SC_NotableWantsDaughterFoundIssue), 1);
					base.AddClassDefinition(typeof(SC_NotableWantsDaughterFoundIssueBehavior.SC_NotableWantsDaughterFoundIssueQuest), 2);
				}
			}

			internal class SC_NotableWantsDaughterFoundIssue : IssueBase
			{
				protected override bool IsThereAlternativeSolution
				{
					get
					{
						return true;
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
						return 500 + MathF.Round(1200f * base.IssueDifficultyMultiplier);
					}
				}

				private int RequiredScoutSkillLevelForSendingComp
				{
					get
					{
						return MathF.Round(150f * base.IssueDifficultyMultiplier);
					}
				}

				protected override int AlternativeSolutionNeededMenCount
				{
					get
					{
						return 4 + MathF.Round(16f * base.IssueDifficultyMultiplier);
					}
				}

				protected override int AlternativeSolutionDurationInDays
				{
					get
					{
						return 10 + MathF.Round(15f * base.IssueDifficultyMultiplier);
					}
				}

				protected override ValueTuple<SkillObject, int> CompanionSkillAndRewardXP
				{
					get
					{
						return new ValueTuple<SkillObject, int>(DefaultSkills.Scouting, (int)(500f + 1000f * base.IssueDifficultyMultiplier));
					}
				}

				public SC_NotableWantsDaughterFoundIssue(Hero issueOwner) : base(issueOwner, CampaignTime.DaysFromNow(20f))
				{
				}

				protected override Dictionary<IssueEffect, float> GetIssueEffectsAndAmountInternal()
				{
					return new Dictionary<IssueEffect, float>
				{
					{
						DefaultIssueEffects.IssueOwnerPower,
						-0.1f
					}
				};
				}

				protected override TextObject IssueBriefByIssueGiver
				{
					get
					{
						TextObject textObject = new TextObject("{=x9VgLEzi}Yes... I've suffered a great misfortune. My daughter, a headstrong girl, has been bewitched by this never-do-well. I told her to stop seeing him but she wouldn't listen! Now she's missing - I'm sure she's been abducted by him! I'm offering a bounty of {BASE_REWARD_GOLD}{GOLD_ICON} to anyone who brings her back. Please {?PLAYER.GENDER}ma'am{?}sir{\\?}! Don't let a father's heart be broken.", null);
						StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, null, textObject, false);
						textObject.SetTextVariable("GOLD_ICON", "{=!}<img src=\"Icons\\Coin@2x\">");
						textObject.SetTextVariable("BASE_REWARD_GOLD", this.RewardGold);
						return textObject;
					}
				}

				protected override TextObject IssueAcceptByPlayer
				{
					get
					{
						return new TextObject("{=35w6g8gM}Tell me more. What's wrong with the man? ", null);
					}
				}

				protected override TextObject IssueQuestSolutionExplanationByIssueGiver
				{
					get
					{
						return new TextObject("{=IY5b9vZV}Everything is wrong. He is from a low family, the kind who is always involved in some land fraud scheme, or seen dealing with known bandits. Every village has a black sheep like that but I never imagined he would get his hooks into my daughter!", null);
					}
				}

				protected override TextObject IssueAlternativeSolutionExplanationByIssueGiver
				{
					get
					{
						TextObject textObject = new TextObject("{=v0XsM7Zz}If you send your best tracker with a few men, I am sure they will find my girl and be back to you in no more than {ALTERNATIVE_SOLUTION_WAIT_DAYS} days.", null);
						textObject.SetTextVariable("ALTERNATIVE_SOLUTION_WAIT_DAYS", MathF.Round((float)this.AlternativeSolutionDurationInDays));
						return textObject;
					}
				}

				protected override TextObject IssuePlayerResponseAfterAlternativeExplanation
				{
					get
					{
						return new TextObject("{=Ldp6ckgj}Don't worry, either I or one of my companions should be able to find her and see what's going on.", null);
					}
				}

				protected override TextObject IssueQuestSolutionAcceptByPlayer
				{
					get
					{
						return new TextObject("{=uYrxCtDa}I should be able to find her and see what's going on.", null);
					}
				}

				protected override TextObject IssueAlternativeSolutionAcceptByPlayer
				{
					get
					{
						TextObject textObject = new TextObject("{=WSrGHkal}I will have one of my trackers and {REQUIRED_TROOP_AMOUNT} of my men to find your daughter.", null);
						textObject.SetTextVariable("REQUIRED_TROOP_AMOUNT", MathF.Ceiling((float)this.AlternativeSolutionNeededMenCount));
						return textObject;
					}
				}

				protected override TextObject IssueAlternativeSolutionResponseByIssueGiver
				{
					get
					{
						TextObject textObject = new TextObject("{=Hhd3KaKu}Thank you, my {?PLAYER.GENDER}lady{?}lord{\\?}. If your men can find my girl and bring her back to me, I will be so grateful. I will pay you {BASE_REWARD_GOLD}{GOLD_ICON} for your trouble.", null);
						textObject.SetTextVariable("GOLD_ICON", "{=!}<img src=\"Icons\\Coin@2x\">");
						textObject.SetTextVariable("BASE_REWARD_GOLD", this.RewardGold);
						StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, null, textObject, false);
						return textObject;
					}
				}

				protected override TextObject AlternativeSolutionStartLog
				{
					get
					{
						TextObject textObject = new TextObject("{=6OmbzoBs}{ISSUE_GIVER.LINK}, a merchant from {ISSUE_GIVER_SETTLEMENT}, has told you that {?ISSUE_GIVER.GENDER}her{?}his{\\?} daughter has gone missing. {?ISSUE_GIVER.GENDER}She{?}He{\\?} offers a bounty of {BASE_REWARD_GOLD}{GOLD_ICON} to anyone who finds her and brings her back. You choose {COMPANION.LINK} and {REQUIRED_TROOP_AMOUNT} men to search for her and bring her back. You expect them to complete this task and return in {ALTERNATIVE_SOLUTION_DAYS} days.", null);
						textObject.SetTextVariable("GOLD_ICON", "{=!}<img src=\"Icons\\Coin@2x\">");
						textObject.SetTextVariable("BASE_REWARD_GOLD", this.RewardGold);
						textObject.SetTextVariable("ISSUE_GIVER_SETTLEMENT", base.IssueOwner.CurrentSettlement.Name);
						textObject.SetTextVariable("REQUIRED_TROOP_AMOUNT", this.AlternativeSolutionSentTroops.TotalManCount - 1);
						textObject.SetTextVariable("ALTERNATIVE_SOLUTION_DAYS", MathF.Round((float)this.AlternativeSolutionDurationInDays));
						StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, null, textObject, false);
						StringHelpers.SetCharacterProperties("ISSUE_GIVER", base.IssueOwner.CharacterObject, null, textObject, false);
						StringHelpers.SetCharacterProperties("COMPANION", base.AlternativeSolutionHero.CharacterObject, null, textObject, false);
						return textObject;
					}
				}

				protected override void AlternativeSolutionEndConsequence()
				{
					this.ApplySuccessRewards();
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
					base.AlternativeSolutionHero.AddSkillXp(skill, (float)((int)(500f + 1000f * base.IssueDifficultyMultiplier)));
				}

				protected override TextObject AlternativeSolutionEndLog
				{
					get
					{
						TextObject textObject = new TextObject("{=MaXA5HJi}Your companions report that the {ISSUE_GIVER.LINK}'s daughter returns to {?ISSUE_GIVER.GENDER}her{?}him{\\?} safe and sound. {?ISSUE_GIVER.GENDER}She{?}He{\\?} is happy and sends {?ISSUE_GIVER.GENDER}her{?}his{\\?} regards with a large pouch of {BASE_REWARD_GOLD}{GOLD_ICON}.", null);
						StringHelpers.SetCharacterProperties("ISSUE_GIVER", base.IssueOwner.CharacterObject, null, textObject, false);
						textObject.SetTextVariable("GOLD_ICON", "{=!}<img src=\"Icons\\Coin@2x\">");
						textObject.SetTextVariable("BASE_REWARD_GOLD", this.RewardGold);
						return textObject;
					}
				}

				private void ApplySuccessRewards()
				{
					GainRenownAction.Apply(Hero.MainHero, 2f, false);
					base.IssueOwner.AddPower(10f);
					this.RelationshipChangeWithIssueOwner = 10;
					base.IssueOwner.CurrentSettlement.Village.TradeBound.Town.Security += 10f;
				}

				public override TextObject Title
				{
					get
					{
						TextObject textObject = new TextObject("SW: {=kr68V5pm}{ISSUE_GIVER.NAME} wants {?ISSUE_GIVER.GENDER}her{?}his{\\?} daughter found", null);
						StringHelpers.SetCharacterProperties("ISSUE_GIVER", base.IssueOwner.CharacterObject, null, textObject, false);
						return textObject;
					}
				}

				public override TextObject Description
				{
					get
					{
						TextObject textObject = new TextObject("{=SkzM5eSv}{ISSUE_GIVER.LINK}'s daughter is missing. {?ISSUE_GIVER.GENDER}She{?}He{\\?} is offering a substantial reward to find the young woman and bring her back safely.", null);
						StringHelpers.SetCharacterProperties("ISSUE_GIVER", base.IssueOwner.CharacterObject, null, textObject, false);
						return textObject;
					}
				}

				public override TextObject IssueAsRumorInSettlement
				{
					get
					{
						TextObject textObject = new TextObject("{=7RyXSkEE}Wouldn't want to be the poor lovesick sap who ran off with {QUEST_GIVER.NAME}'s daughter.", null);
						StringHelpers.SetCharacterProperties("QUEST_GIVER", base.IssueOwner.CharacterObject, null, textObject, false);
						return textObject;
					}
				}

				protected override void OnGameLoad()
				{
				}

				protected override QuestBase GenerateIssueQuest(string questId)
				{
					return new SC_NotableWantsDaughterFoundIssueBehavior.SC_NotableWantsDaughterFoundIssueQuest(questId, base.IssueOwner, CampaignTime.DaysFromNow(20f), this.RewardGold, base.IssueDifficultyMultiplier);
				}

				public override IssueBase.IssueFrequency GetFrequency()
				{
					return IssueBase.IssueFrequency.Rare;
				}

				private Dictionary<SkillObject, int> GetAlternativeSolutionRequiredCompanionSkills()
				{
					return new Dictionary<SkillObject, int>
				{
					{
						DefaultSkills.Scouting,
						this.RequiredScoutSkillLevelForSendingComp
					}
				};
				}

				protected override bool AlternativeSolutionCondition(out TextObject explanation)
				{
					Dictionary<SkillObject, int> alternativeSolutionRequiredCompanionSkills = this.GetAlternativeSolutionRequiredCompanionSkills();
					explanation = TextObject.Empty;
					return QuestHelper.CheckAllCompanionsCondition(MobileParty.MainParty.MemberRoster, ref explanation, alternativeSolutionRequiredCompanionSkills, null) && QuestHelper.CheckRosterForAlternativeSolution(MobileParty.MainParty.MemberRoster, this.AlternativeSolutionNeededMenCount, ref explanation, 2, false);
				}

				protected override bool DoTroopsSatisfyAlternativeSolution(TroopRoster troopRoster, out TextObject explanation)
				{
					explanation = TextObject.Empty;
					return QuestHelper.CheckRosterForAlternativeSolution(troopRoster, this.AlternativeSolutionNeededMenCount, ref explanation, 2, false);
				}

				protected override bool CompanionOrFamilyMemberClickableCondition(Hero companion, out TextObject explanation)
				{
					explanation = TextObject.Empty;
					return QuestHelper.CheckCompanionForAlternativeSolution(companion.CharacterObject, ref explanation, this.GetAlternativeSolutionRequiredCompanionSkills(), null);
				}

				protected override bool IsTroopTypeNeededByAlternativeSolution(CharacterObject character)
				{
					return character.Tier >= 2;
				}

				protected override bool CanPlayerTakeQuestConditions(Hero issueGiver, out IssueBase.PreconditionFlags flag, out Hero relationHero, out SkillObject skill)
				{
					bool flag2 = issueGiver.GetRelationWithPlayer() >= -10f && !issueGiver.CurrentSettlement.MapFaction.IsAtWarWith(Hero.MainHero.MapFaction);
					flag = (flag2 ? IssueBase.PreconditionFlags.None : ((!issueGiver.CurrentSettlement.MapFaction.IsAtWarWith(Hero.MainHero.MapFaction)) ? IssueBase.PreconditionFlags.Relation : IssueBase.PreconditionFlags.AtWar));
					relationHero = issueGiver;
					skill = null;
					return flag2;
				}

				public override bool IssueStayAliveConditions()
				{
					return true;
				}

				protected override void CompleteIssueWithTimedOutConsequences()
				{
				}

				private const int TroopTierForAlternativeSolution = 2;
			}

			internal class SC_NotableWantsDaughterFoundIssueQuest : QuestBase
			{
				public override TextObject Title
				{
					get
					{
						TextObject textObject = new TextObject("SW {=PDhmSieV}{QUEST_GIVER.NAME}'s Kidnapped Daughter at {SETTLEMENT}", null);
						textObject.SetTextVariable("SETTLEMENT", this.QuestGiver.CurrentSettlement.Name);
						StringHelpers.SetCharacterProperties("QUEST_GIVER", this.QuestGiver.CharacterObject, null, textObject, false);
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

				private bool DoesMainPartyHasEnoughScoutingSkill
				{
					get
					{
						return (float)MobileParty.GetMainPartySkillCounsellor(DefaultSkills.Scouting).GetSkillValue(DefaultSkills.Scouting) >= 150f * this._questDifficultyMultiplier;
					}
				}

				private TextObject _playerStartsQuestLogText
				{
					get
					{
						TextObject textObject = new TextObject("{=1jExD58d}{QUEST_GIVER.LINK}, a merchant from {SETTLEMENT_NAME}, told you that {?QUEST_GIVER.GENDER}her{?}his{\\?} daughter {TARGET_HERO.LINK} has either been abducted or run off with a local rogue. {?QUEST_GIVER.GENDER}She{?}He{\\?} is offering a bounty of {BASE_REWARD_GOLD}{GOLD_ICON} to anyone who finds the young woman and brings her back. You have agreed to search for her and bring her back to {SETTLEMENT_NAME}. If you cannot find their tracks when you exit settlement, you should visit the nearby villages of {SETTLEMENT_NAME} to look for clues and tracks of the kidnapper.", null);
						StringHelpers.SetCharacterProperties("QUEST_GIVER", this.QuestGiver.CharacterObject, null, textObject, false);
						StringHelpers.SetCharacterProperties("TARGET_HERO", this._daughterHero.CharacterObject, null, textObject, false);
						textObject.SetTextVariable("SETTLEMENT_NAME", this.QuestGiver.CurrentSettlement.EncyclopediaLinkWithName);
						textObject.SetTextVariable("BASE_REWARD_GOLD", this.RewardGold);
						textObject.SetTextVariable("GOLD_ICON", "{=!}<img src=\"Icons\\Coin@2x\">");
						return textObject;
					}
				}

				private TextObject _successQuestLogText
				{
					get
					{
						TextObject textObject = new TextObject("{=asVE53ac}Daughter returns to {QUEST_GIVER.LINK}. {?QUEST_GIVER.GENDER}She{?}He{\\?} is happy. Sends {?QUEST_GIVER.GENDER}her{?}his{\\?} regards with a large pouch of {BASE_REWARD}{GOLD_ICON}.", null);
						StringHelpers.SetCharacterProperties("QUEST_GIVER", this.QuestGiver.CharacterObject, null, textObject, false);
						textObject.SetTextVariable("GOLD_ICON", "{=!}<img src=\"Icons\\Coin@2x\">");
						textObject.SetTextVariable("BASE_REWARD", this.RewardGold);
						return textObject;
					}
				}

				private TextObject _failQuestLogText
				{
					get
					{
						TextObject textObject = new TextObject("{=ak2EMWWR}You failed to bring the daughter back to her {?QUEST_GIVER.GENDER}mother{?}father{\\?} as promised to {QUEST_GIVER.LINK}. {QUEST_GIVER.LINK} is furious", null);
						StringHelpers.SetCharacterProperties("QUEST_GIVER", this.QuestGiver.CharacterObject, null, textObject, false);
						return textObject;
					}
				}

				private TextObject _questCanceledWarDeclaredLog
				{
					get
					{
						TextObject textObject = new TextObject("{=vW6kBki9}Your clan is now at war with {QUEST_GIVER.LINK}'s realm. Your agreement with {QUEST_GIVER.LINK} is canceled.", null);
						StringHelpers.SetCharacterProperties("QUEST_GIVER", this.QuestGiver.CharacterObject, null, textObject, false);
						return textObject;
					}
				}

				public SC_NotableWantsDaughterFoundIssueQuest(string questId, Hero questGiver, CampaignTime duration, int baseReward, float issueDifficultyMultiplier) : base(questId, questGiver, duration, baseReward)
				{
					this._questDifficultyMultiplier = issueDifficultyMultiplier;
					this._targetVillage = TaleWorlds.Core.Extensions.GetRandomElement<Village>(from x in questGiver.CurrentSettlement.Village.TradeBound.BoundVillages
																			   where x != questGiver.CurrentSettlement.Village
																			   select x);
					Dictionary<string, CharacterObject> rogueCharacterBasedOnCulture = this._rogueCharacterBasedOnCulture;
					string key = "khuzait";
					Clan clan = Clan.BanditFactions.FirstOrDefault((Clan x) => x.StringId == "steppe_bandits");
					rogueCharacterBasedOnCulture.Add(key, (clan != null) ? clan.Culture.BanditBoss : null);
					Dictionary<string, CharacterObject> rogueCharacterBasedOnCulture2 = this._rogueCharacterBasedOnCulture;
					string key2 = "vlandia";
					Clan clan2 = Clan.BanditFactions.FirstOrDefault((Clan x) => x.StringId == "mountain_bandits");
					rogueCharacterBasedOnCulture2.Add(key2, (clan2 != null) ? clan2.Culture.BanditBoss : null);
					Dictionary<string, CharacterObject> rogueCharacterBasedOnCulture3 = this._rogueCharacterBasedOnCulture;
					string key3 = "aserai";
					Clan clan3 = Clan.BanditFactions.FirstOrDefault((Clan x) => x.StringId == "desert_bandits");
					rogueCharacterBasedOnCulture3.Add(key3, (clan3 != null) ? clan3.Culture.BanditBoss : null);
					Dictionary<string, CharacterObject> rogueCharacterBasedOnCulture4 = this._rogueCharacterBasedOnCulture;
					string key4 = "battania";
					Clan clan4 = Clan.BanditFactions.FirstOrDefault((Clan x) => x.StringId == "forest_bandits");
					rogueCharacterBasedOnCulture4.Add(key4, (clan4 != null) ? clan4.Culture.BanditBoss : null);
					Dictionary<string, CharacterObject> rogueCharacterBasedOnCulture5 = this._rogueCharacterBasedOnCulture;
					string key5 = "sturgia";
					Clan clan5 = Clan.BanditFactions.FirstOrDefault((Clan x) => x.StringId == "sea_raiders");
					rogueCharacterBasedOnCulture5.Add(key5, (clan5 != null) ? clan5.Culture.BanditBoss : null);
					Dictionary<string, CharacterObject> rogueCharacterBasedOnCulture6 = this._rogueCharacterBasedOnCulture;
					string key6 = "empire_w";
					Clan clan6 = Clan.BanditFactions.FirstOrDefault((Clan x) => x.StringId == "mountain_bandits");
					rogueCharacterBasedOnCulture6.Add(key6, (clan6 != null) ? clan6.Culture.BanditBoss : null);
					Dictionary<string, CharacterObject> rogueCharacterBasedOnCulture7 = this._rogueCharacterBasedOnCulture;
					string key7 = "empire_s";
					Clan clan7 = Clan.BanditFactions.FirstOrDefault((Clan x) => x.StringId == "mountain_bandits");
					rogueCharacterBasedOnCulture7.Add(key7, (clan7 != null) ? clan7.Culture.BanditBoss : null);
					Dictionary<string, CharacterObject> rogueCharacterBasedOnCulture8 = this._rogueCharacterBasedOnCulture;
					string key8 = "empire";
					Clan clan8 = Clan.BanditFactions.FirstOrDefault((Clan x) => x.StringId == "mountain_bandits");
					rogueCharacterBasedOnCulture8.Add(key8, (clan8 != null) ? clan8.Culture.BanditBoss : null);
					int heroComesOfAge = Campaign.Current.Models.AgeModel.HeroComesOfAge;
					int num = MBRandom.RandomInt(heroComesOfAge, (int)MathF.Clamp(questGiver.Age - (float)heroComesOfAge, (float)heroComesOfAge, (float)(heroComesOfAge * 2)));
					this._daughterHero = HeroCreator.CreateSpecialHero(TaleWorlds.Core.Extensions.GetRandomElement<CharacterObject>(from x in CharacterObject.Templates
																													where questGiver.CurrentSettlement.Culture == x.Culture && x.Occupation == Occupation.Wanderer && x.IsFemale
																													select x), questGiver.HomeSettlement, questGiver.Clan, null, num);
					this._rogueHero = HeroCreator.CreateSpecialHero(this.GetRogueCharacterBasedOnCulture(questGiver.Culture), questGiver.HomeSettlement, questGiver.Clan, null, MBRandom.RandomInt(num, heroComesOfAge * 2));
					questGiver.Children.Add(this._daughterHero);
					this.SetDialogs();
					base.InitializeQuestOnCreation();
					this.HandleTheRogueAndDaughter();
				}

				private CharacterObject GetRogueCharacterBasedOnCulture(CultureObject culture)
				{
					CharacterObject characterObject;
					if (this._rogueCharacterBasedOnCulture.ContainsKey(culture.StringId))
					{
						characterObject = this._rogueCharacterBasedOnCulture[culture.StringId];
					}
					else
					{
						characterObject = TaleWorlds.Core.Extensions.GetRandomElement<CharacterObject>(from x in CharacterObject.Templates
																					   where this.QuestGiver.CurrentSettlement.Culture == x.Culture && x.Occupation == Occupation.GangLeader && !x.IsFemale
																					   select x);
					}
					characterObject.Culture = this.QuestGiver.Culture;
					return characterObject;
				}

				protected override void SetDialogs()
				{
					TextObject textObject = new TextObject("{=PZq1EMcx}Thank you for your help. I am still very worried about my girl {TARGET_HERO.LINK}. Please find her and bring her back to me as soon as you can.", null);
					StringHelpers.SetCharacterProperties("TARGET_HERO", this._daughterHero.CharacterObject, null, textObject, false);
					TextObject npcText = new TextObject("{=sglD6abb}Please! Bring my daughter back.", null);
					TextObject npcText2 = new TextObject("{=ddEu5IFQ}I hope so.", null);
					TextObject npcText3 = new TextObject("{=IdKG3IaS}Good to hear that.", null);
					TextObject text = new TextObject("{=0hXofVLx}Don't worry I'll bring her.", null);
					TextObject text2 = new TextObject("{=zpqP5LsC}I'll go right away.", null);
					this.OfferDialogFlow = DialogFlow.CreateDialogFlow("issue_classic_quest_start", 100).NpcLine(textObject, null, null).Condition(() => Hero.OneToOneConversationHero == this.QuestGiver && !this._didPlayerBeatRouge).Consequence(new ConversationSentence.OnConsequenceDelegate(this.QuestAcceptedConsequences)).CloseDialog();
					this.DiscussDialogFlow = DialogFlow.CreateDialogFlow("quest_discuss", 100).NpcLine(npcText, null, null).Condition(() => Hero.OneToOneConversationHero == this.QuestGiver && !this._didPlayerBeatRouge).BeginPlayerOptions().PlayerOption(text, null).NpcLine(npcText2, null, null).CloseDialog().PlayerOption(text2, null).NpcLine(npcText3, null, null).CloseDialog();
					Campaign.Current.ConversationManager.AddDialogFlow(this.GetRougeDialogFlow(), this);
					Campaign.Current.ConversationManager.AddDialogFlow(this.GetDaughterAfterFightDialog(), this);
					Campaign.Current.ConversationManager.AddDialogFlow(this.GetDaughterAfterAcceptDialog(), this);
					Campaign.Current.ConversationManager.AddDialogFlow(this.GetDaughterAfterPersuadedDialog(), this);
					Campaign.Current.ConversationManager.AddDialogFlow(this.GetDaughterDialogWhenVillageRaid(), this);
					Campaign.Current.ConversationManager.AddDialogFlow(this.GetRougeAfterAcceptDialog(), this);
					Campaign.Current.ConversationManager.AddDialogFlow(this.GetRogueAfterPersuadedDialog(), this);
				}

				protected override void InitializeQuestOnGameLoad()
				{
					this.SetDialogs();
					if (this._villagesAndAlreadyVisitedBooleans == null)
					{
						this._villagesAndAlreadyVisitedBooleans = new Dictionary<Village, bool>();
						foreach (Village village in this.QuestGiver.CurrentSettlement.Village.TradeBound.BoundVillages)
						{
							if (village != this.QuestGiver.CurrentSettlement.Village)
							{
								this._villagesAndAlreadyVisitedBooleans.Add(village, false);
							}
						}
					}
				}

				private bool IsRougeHero(IAgent agent)
				{
					return agent.Character == this._rogueHero.CharacterObject;
				}

				private bool IsDaughterHero(IAgent agent)
				{
					return agent.Character == this._daughterHero.CharacterObject;
				}

				private bool IsMainHero(IAgent agent)
				{
					return agent.Character == CharacterObject.PlayerCharacter;
				}

				private bool multi_character_conversation_on_condition()
				{
					if (!this._villageIsRaidedTalkWithDaughter && !this._isDaughterPersuaded && !this._didPlayerBeatRouge && !this._acceptedDaughtersEscape && this._isQuestTargetMission && (CharacterObject.OneToOneConversationCharacter == this._daughterHero.CharacterObject || CharacterObject.OneToOneConversationCharacter == this._rogueHero.CharacterObject))
					{
						foreach (Agent agent in Mission.Current.GetNearbyAgents(Agent.Main.Position.AsVec2, 100f))
						{
							if (agent.Character == this._daughterHero.CharacterObject)
							{
								this._daughterAgent = agent;
								if (Mission.Current.GetMissionBehaviour<MissionConversationHandler>() != null && Hero.OneToOneConversationHero != this._daughterHero)
								{
									Campaign.Current.ConversationManager.AddConversationAgents(new List<Agent>
								{
									this._daughterAgent
								}, true);
								}
							}
							else if (agent.Character == this._rogueHero.CharacterObject)
							{
								this._rogueAgent = agent;
								if (Mission.Current.GetMissionBehaviour<MissionConversationHandler>() != null && Hero.OneToOneConversationHero != this._rogueHero)
								{
									Campaign.Current.ConversationManager.AddConversationAgents(new List<Agent>
								{
									this._rogueAgent
								}, true);
								}
							}
						}
						return this._daughterAgent != null && this._rogueAgent != null && this._daughterAgent.Health > 10f && this._rogueAgent.Health > 10f;
					}
					return false;
				}

				private bool daughter_conversation_after_fight_on_condition()
				{
					return CharacterObject.OneToOneConversationCharacter == this._daughterHero.CharacterObject && this._didPlayerBeatRouge;
				}

				private void multi_agent_conversation_on_consequence()
				{
					this._task = this.GetPersuasionTask();
				}

				private DialogFlow GetRougeDialogFlow()
				{
					TextObject textObject = new TextObject("{=ovFbMMTJ}Who are you? Are you one of the bounty hunters sent by {QUEST_GIVER.LINK} to track us? Like we're animals or something? Look friend, we have done nothing wrong. As you may figured out already, this woman and I, we love each other. I didn't force her to do anything.[ib:closed]", null);
					StringHelpers.SetCharacterProperties("QUEST_GIVER", this.QuestGiver.CharacterObject, null, textObject, false);
					TextObject textObject2 = new TextObject("{=D25oY3j1}Thank you {?PLAYER.GENDER}lady{?}sir{\\?}. For your kindness and understanding. We won't forget this.", null);
					StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, null, textObject2, false);
					TextObject textObject3 = new TextObject("{=oL3amiu1}Come {DAUGHTER_NAME.NAME}, let's go before other hounds sniff our trail... I mean... No offense {?PLAYER.GENDER}madam{?}sir{\\?}.", null);
					StringHelpers.SetCharacterProperties("DAUGHTER_NAME", this._daughterHero.CharacterObject, null, textObject3, false);
					StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, null, textObject3, false);
					TextObject textObject4 = new TextObject("{=92sbq1YY}I'm no child, {?PLAYER.GENDER}lady{?}sir{\\?}! Draw your weapon! I challenge you to a duel!", null);
					StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, null, textObject4, false);
					TextObject textObject5 = new TextObject("{=jfzErupx}He is right! I ran away with him willingly. I love my {?QUEST_GIVER.GENDER}mother{?}father{\\?} but {?QUEST_GIVER.GENDER}she{?}he{\\?} can be such a tyrant. Please {?PLAYER.GENDER}lady{?}sir{\\?} if you believe in freedom and love, please leave us be.", null);
					StringHelpers.SetCharacterProperties("QUEST_GIVER", this.QuestGiver.CharacterObject, null, textObject5, false);
					StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, null, textObject5, false);
					TextObject textObject6 = new TextObject("{=5NljlbLA}Thank you kind {?PLAYER.GENDER}lady{?}sir{\\?}, thank you.", null);
					StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, null, textObject6, false);
					TextObject textObject7 = new TextObject("{=i5fNZrhh}Please, {?PLAYER.GENDER}lady{?}sir{\\?}. I love him truly and I wish to spend rest of my life with him. I beg of you, please don't stand in our way.", null);
					StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, null, textObject7, false);
					TextObject textObject8 = new TextObject("{=0RCdPKj2}Yes {?QUEST_GIVER.GENDER}she{?}he{\\?} would probably be sad. But not because of what you think. See, {QUEST_GIVER.LINK} promised me to one of {?QUEST_GIVER.GENDER}her{?}his{\\?} allies' son and this will devastate {?QUEST_GIVER.GENDER}her{?}his{\\?} plans. That is true.", null);
					StringHelpers.SetCharacterProperties("QUEST_GIVER", this.QuestGiver.CharacterObject, null, textObject8, false);
					TextObject text = new TextObject("{=5W7Kxfq9}I understand. If that is the case, I will let you go.", null);
					TextObject text2 = new TextObject("{=3XimdHOn}How do I know he's not forcing you to say that?", null);
					TextObject textObject9 = new TextObject("{=zNqDEuAw}But I've promised to find you and return you to your {?QUEST_GIVER.GENDER}mother{?}father{\\?}. {?QUEST_GIVER.GENDER}She{?}He{\\?} would be devastated.", null);
					StringHelpers.SetCharacterProperties("QUEST_GIVER", this.QuestGiver.CharacterObject, null, textObject9, false);
					TextObject textObject10 = new TextObject("{=tuaQ5uU3}I guess the only way to free you from this pretty boy's spell is to kill him.", null);
					TextObject textObject11 = new TextObject("{=HDCmeGhG}I'm sorry but I gave a promise. I don't break my promises.", null);
					TextObject text3 = new TextObject("{=VGrHWxzf}This will be a massacre, not a duel, but I'm fine with that.", null);
					TextObject text4 = new TextObject("{=sytYViXb}I accept your duel.", null);
					DialogFlow dialogFlow = DialogFlow.CreateDialogFlow("start", 125).NpcLine(textObject, new ConversationSentence.OnMultipleConversationConsequenceDelegate(this.IsRougeHero), new ConversationSentence.OnMultipleConversationConsequenceDelegate(this.IsMainHero)).Condition(new ConversationSentence.OnConditionDelegate(this.multi_character_conversation_on_condition)).Consequence(new ConversationSentence.OnConsequenceDelegate(this.multi_agent_conversation_on_consequence)).NpcLine(textObject5, new ConversationSentence.OnMultipleConversationConsequenceDelegate(this.IsDaughterHero), new ConversationSentence.OnMultipleConversationConsequenceDelegate(this.IsMainHero)).BeginPlayerOptions().PlayerOption(text, new ConversationSentence.OnMultipleConversationConsequenceDelegate(this.IsDaughterHero)).NpcLine(textObject2, new ConversationSentence.OnMultipleConversationConsequenceDelegate(this.IsRougeHero), new ConversationSentence.OnMultipleConversationConsequenceDelegate(this.IsMainHero)).NpcLine(textObject3, new ConversationSentence.OnMultipleConversationConsequenceDelegate(this.IsRougeHero), new ConversationSentence.OnMultipleConversationConsequenceDelegate(this.IsDaughterHero)).NpcLine(textObject6, new ConversationSentence.OnMultipleConversationConsequenceDelegate(this.IsDaughterHero), new ConversationSentence.OnMultipleConversationConsequenceDelegate(this.IsMainHero)).Consequence(delegate
					{
						Campaign.Current.ConversationManager.ConversationEndOneShot += this.PlayerAcceptedDaughtersEscape;
					}).CloseDialog().PlayerOption(text2, new ConversationSentence.OnMultipleConversationConsequenceDelegate(this.IsDaughterHero)).NpcLine(textObject7, new ConversationSentence.OnMultipleConversationConsequenceDelegate(this.IsDaughterHero), new ConversationSentence.OnMultipleConversationConsequenceDelegate(this.IsMainHero)).PlayerLine(textObject9, new ConversationSentence.OnMultipleConversationConsequenceDelegate(this.IsDaughterHero)).NpcLine(textObject8, new ConversationSentence.OnMultipleConversationConsequenceDelegate(this.IsDaughterHero), new ConversationSentence.OnMultipleConversationConsequenceDelegate(this.IsMainHero)).Consequence(new ConversationSentence.OnConsequenceDelegate(this.ChangeConversationCharacterForPersuasion)).GotoDialogState("start_daughter_persuade_to_come_persuasion").GoBackToDialogState("daughter_persuade_to_come_persuasion_finished").PlayerLine((Hero.MainHero.GetTraitLevel(DefaultTraits.Mercy) < 0) ? textObject10 : textObject11, new ConversationSentence.OnMultipleConversationConsequenceDelegate(this.IsDaughterHero)).NpcLine(textObject4, new ConversationSentence.OnMultipleConversationConsequenceDelegate(this.IsRougeHero), new ConversationSentence.OnMultipleConversationConsequenceDelegate(this.IsMainHero)).BeginPlayerOptions().PlayerOption(text3, new ConversationSentence.OnMultipleConversationConsequenceDelegate(this.IsRougeHero)).NpcLine(new TextObject("{=XWVW0oTB}You bastard![ib:aggressive][rb:very_negative]", null), new ConversationSentence.OnMultipleConversationConsequenceDelegate(this.IsRougeHero), null).Consequence(delegate
					{
						Campaign.Current.ConversationManager.ConversationEndOneShot += this.PlayerRejectsDuelFight;
					}).CloseDialog().PlayerOption(text4, new ConversationSentence.OnMultipleConversationConsequenceDelegate(this.IsRougeHero)).NpcLine(new TextObject("{=jqahxjWD}Heaven protect me![ib:aggressive][rb:very_negative]", null), new ConversationSentence.OnMultipleConversationConsequenceDelegate(this.IsRougeHero), null).Consequence(delegate
					{
						Campaign.Current.ConversationManager.ConversationEndOneShot += this.PlayerAcceptsDuelFight;
					}).CloseDialog().EndPlayerOptions().EndPlayerOptions().CloseDialog();
					this.AddPersuasionDialogs(dialogFlow);
					return dialogFlow;
				}

				private DialogFlow GetDaughterAfterFightDialog()
				{
					TextObject npcText = new TextObject("{=MN2v1AZQ}I hate you! You killed him! I can't believe it!. I will hate you with all my heart till my dying days. ", null);
					TextObject npcText2 = new TextObject("{=TTkVcObg}What choice do I have you heartless bastard![rb:very_negative]", null);
					TextObject textObject = new TextObject("{=XqsrsjiL}I did what I had to do. Pack up, you need to go.", null);
					TextObject textObject2 = new TextObject("{=KQ3aYvp3}Some day you'll see I did you a favor. Pack up, you need to go.", null);
					return DialogFlow.CreateDialogFlow("start", 125).NpcLine(npcText, null, null).Condition(new ConversationSentence.OnConditionDelegate(this.daughter_conversation_after_fight_on_condition)).PlayerLine((Hero.MainHero.GetTraitLevel(DefaultTraits.Mercy) < 0) ? textObject : textObject2, null).NpcLine(npcText2, null, null).Consequence(delegate
					{
						Campaign.Current.ConversationManager.ConversationEndOneShot += this.PlayerWonTheFight;
					}).CloseDialog();
				}

				private DialogFlow GetDaughterAfterAcceptDialog()
				{
					TextObject textObject = new TextObject("{=0Wg00sfN}Thank you, {?PLAYER.GENDER}madam{?}sir{\\?}. We will be moving immediately.", null);
					StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, null, textObject, false);
					TextObject playerText = new TextObject("{=kUReBc04}Good.", null);
					return DialogFlow.CreateDialogFlow("start", 125).NpcLine(textObject, null, null).Condition(new ConversationSentence.OnConditionDelegate(this.daughter_conversation_after_accept_on_condition)).PlayerLine(playerText, null).CloseDialog();
				}

				private bool daughter_conversation_after_accept_on_condition()
				{
					return CharacterObject.OneToOneConversationCharacter == this._daughterHero.CharacterObject && this._acceptedDaughtersEscape;
				}

				private DialogFlow GetDaughterAfterPersuadedDialog()
				{
					TextObject textObject = new TextObject("{=B8bHpJRP}You are right, {?PLAYER.GENDER}my lady{?}sir{\\?}. I should be moving immediately.", null);
					StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, null, textObject, false);
					TextObject playerText = new TextObject("{=kUReBc04}Good.", null);
					return DialogFlow.CreateDialogFlow("start", 125).NpcLine(textObject, null, null).Condition(new ConversationSentence.OnConditionDelegate(this.daughter_conversation_after_persuaded_on_condition)).PlayerLine(playerText, null).CloseDialog();
				}

				private DialogFlow GetDaughterDialogWhenVillageRaid()
				{
					return DialogFlow.CreateDialogFlow("start", 125).NpcLine(new TextObject("{=w0HPC53e}Who are you? What do you want from me?", null), null, null).Condition(() => this._villageIsRaidedTalkWithDaughter).PlayerLine(new TextObject("{=iRupMGI0}Calm down! Your father has sent me to find you.", null), null).NpcLine(new TextObject("{=dwNquUNr}My father? Oh, thank god! I saw terrible things. They took my beloved one and slew many innocents without hesitation.", null), null, null).PlayerLine("{=HtAr22re}Try to forget all about these and return to your father's house.", null).NpcLine("{=FgSIsasF}Yes, you are right. I shall be on my way...", null, null).Consequence(delegate
					{
						Campaign.Current.ConversationManager.ConversationEndOneShot += delegate ()
						{
							this.ApplyDeliverySuccessConsequences();
							base.CompleteQuestWithSuccess();
							base.AddLog(this._successQuestLogText, false);
							this._villageIsRaidedTalkWithDaughter = false;
						};
					}).CloseDialog();
				}

				private bool daughter_conversation_after_persuaded_on_condition()
				{
					return CharacterObject.OneToOneConversationCharacter == this._daughterHero.CharacterObject && this._isDaughterPersuaded;
				}

				private DialogFlow GetRougeAfterAcceptDialog()
				{
					TextObject textObject = new TextObject("{=wlKtDR2z}Thank you, {?PLAYER.GENDER}my lady{?}sir{\\?}.", null);
					StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, null, textObject, false);
					return DialogFlow.CreateDialogFlow("start", 125).NpcLine(textObject, null, null).Condition(new ConversationSentence.OnConditionDelegate(this.rogue_conversation_after_accept_on_condition)).PlayerLine(new TextObject("{=0YJGvJ7o}You should leave now.", null), null).NpcLine(new TextObject("{=6Q4cPOSG}Yes, we will.", null), null, null).CloseDialog();
				}

				private bool rogue_conversation_after_accept_on_condition()
				{
					return CharacterObject.OneToOneConversationCharacter == this._rogueHero.CharacterObject && this._acceptedDaughtersEscape;
				}

				private DialogFlow GetRogueAfterPersuadedDialog()
				{
					TextObject textObject = new TextObject("{=GFt9KiHP}You are right. Maybe we need to persuade {QUEST_GIVER.NAME}", null);
					StringHelpers.SetCharacterProperties("QUEST_GIVER", this.QuestGiver.CharacterObject, null, textObject, false);
					TextObject playerText = new TextObject("{=btJkBTSF}I am sure you can solve it.", null);
					return DialogFlow.CreateDialogFlow("start", 125).NpcLine(textObject, null, null).Condition(new ConversationSentence.OnConditionDelegate(this.rogue_conversation_after_persuaded_on_condition)).PlayerLine(playerText, null).CloseDialog();
				}

				private bool rogue_conversation_after_persuaded_on_condition()
				{
					return CharacterObject.OneToOneConversationCharacter == this._rogueHero.CharacterObject && this._isDaughterPersuaded;
				}

				protected override void OnTimedOut()
				{
					this.ApplyDeliveryRejectedFailConsequences();
					TextObject textObject = new TextObject("{=KAvwytDK}You didn't bring {DAUGHTER.LINK} to {QUEST_GIVER.LINK}. {?QUEST_GIVER.GENDER}she{?}he{\\?} must be furious.", null);
					StringHelpers.SetCharacterProperties("QUEST_GIVER", this.QuestGiver.CharacterObject, null, textObject, false);
					StringHelpers.SetCharacterProperties("DAUGHTER", this._daughterHero.CharacterObject, null, textObject, false);
					base.AddLog(textObject, false);
				}

				private void PlayerAcceptedDaughtersEscape()
				{
					this._acceptedDaughtersEscape = true;
				}

				private void PlayerWonTheFight()
				{
					this._isDaughterCaptured = true;
					Mission.Current.SetMissionMode(0, false);
				}

				private void ChangeConversationCharacterForPersuasion()
				{
					if (!Campaign.Current.ConversationManager.ConversationAgents[0].Character.IsFemale)
					{
						Campaign.Current.ConversationManager.ConversationAgents.Clear();
						Campaign.Current.ConversationManager.ConversationAgents.Add(this._daughterAgent);
						Campaign.Current.ConversationManager.ConversationAgents.Add(this._rogueAgent);
					}
				}

				private void ApplyDaughtersEscapeAcceptedFailConsequences()
				{
					this.RelationshipChangeWithQuestGiver = -10;
					ChangeRelationAction.ApplyPlayerRelation(this._daughterHero, 5, true, true);
					this.QuestGiver.CurrentSettlement.Village.TradeBound.Town.Security -= 5f;
					this.QuestGiver.CurrentSettlement.Village.TradeBound.Prosperity -= 5f;
				}

				private void ApplyDeliveryRejectedFailConsequences()
				{
					this.RelationshipChangeWithQuestGiver = -10;
					this.QuestGiver.CurrentSettlement.Village.TradeBound.Town.Security -= 5f;
					this.QuestGiver.CurrentSettlement.Village.TradeBound.Prosperity -= 5f;
				}

				private void ApplyDeliverySuccessConsequences()
				{
					GainRenownAction.Apply(Hero.MainHero, 2f, false);
					this.QuestGiver.AddPower(10f);
					this.RelationshipChangeWithQuestGiver = 10;
					this.QuestGiver.CurrentSettlement.Village.TradeBound.Town.Security += 10f;
					GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, this.RewardGold, false);
				}

				private void PlayerRejectsDuelFight()
				{
					this._rogueAgent = (Agent)MissionConversationHandler.Current.ConversationManager.ConversationAgents.First((IAgent x) => !x.Character.IsFemale);
					List<Agent> list = new List<Agent>
				{
					Agent.Main
				};
					List<Agent> opponentSideAgents = new List<Agent>
				{
					this._rogueAgent
				};
					foreach (Agent agent in Mission.Current.GetNearbyAgents(Agent.Main.Position.AsVec2, 30f))
					{
						foreach (Hero hero in Hero.MainHero.CompanionsInParty)
						{
							if (agent.Character == hero.CharacterObject)
							{
								list.Add(agent);
								break;
							}
						}
					}
					this._rogueAgent.Health = 200f * this._questDifficultyMultiplier * (float)list.Count;
					this._rogueAgent.Defensiveness = 1f;
					Mission.Current.GetMissionBehaviour<MissionFightHandler>().StartCustomFight(list, opponentSideAgents, true, false, false, new MissionFightHandler.OnFightEndDelegate(this.StartConversationAfterFight), true, null, null, null, null);
				}

				private void PlayerAcceptsDuelFight()
				{
					this._rogueAgent = (Agent)MissionConversationHandler.Current.ConversationManager.ConversationAgents.First((IAgent x) => !x.Character.IsFemale);
					List<Agent> playerSideAgents = new List<Agent>
				{
					Agent.Main
				};
					List<Agent> opponentSideAgents = new List<Agent>
				{
					this._rogueAgent
				};
					foreach (Agent agent in Mission.Current.GetNearbyAgents(Agent.Main.Position.AsVec2, 30f))
					{
						foreach (Hero hero in Hero.MainHero.CompanionsInParty)
						{
							if (agent.Character == hero.CharacterObject)
							{
								agent.SetTeam(Mission.Current.AttackerAllyTeam, false);
								DailyBehaviorGroup behaviorGroup = agent.GetComponent<CampaignAgentComponent>().AgentNavigator.GetBehaviorGroup<DailyBehaviorGroup>();
								if (behaviorGroup.GetActiveBehavior() is FollowAgentBehavior)
								{
									behaviorGroup.GetBehavior<FollowAgentBehavior>().SetTargetAgent(null);
									break;
								}
								break;
							}
						}
					}
					this._rogueAgent.Health = 200f;
					Mission.Current.GetMissionBehaviour<MissionFightHandler>().StartCustomFight(playerSideAgents, opponentSideAgents, true, false, false, new MissionFightHandler.OnFightEndDelegate(this.StartConversationAfterFight), true, null, null, null, null);
				}

				private void StartConversationAfterFight(bool isPlayerSideWon)
				{
					if (isPlayerSideWon)
					{
						this._didPlayerBeatRouge = true;
						Campaign.Current.ConversationManager.SetupAndStartMissionConversation(this._daughterAgent, Mission.Current.MainAgent, false);
						TraitLevelingHelper.OnHostileAction(50);
						return;
					}
					TextObject textObject = new TextObject("{=i1sth9Ls}You were defeated by the rogue. He and {TARGET_HERO.LINK} ran off while you were unconscious. You failed to bring the daughter back to her {?QUEST_GIVER.GENDER}mother{?}father{\\?} as promised to {QUEST_GIVER.LINK}. {?QUEST_GIVER.GENDER}She{?}He{\\?} is furious.", null);
					StringHelpers.SetCharacterProperties("QUEST_GIVER", this.QuestGiver.CharacterObject, null, textObject, false);
					StringHelpers.SetCharacterProperties("TARGET_HERO", this._daughterHero.CharacterObject, null, textObject, false);
					base.CompleteQuestWithFail(textObject);
					this._isQuestTargetMission = false;
				}

				private void AddPersuasionDialogs(DialogFlow dialog)
				{
					TextObject textObject = new TextObject("{=ob5SejgJ}I will not abandon my love, {?PLAYER.GENDER}lady{?}sir{\\?}!", null);
					StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, null, textObject, false);
					TextObject textObject2 = new TextObject("{=cqe8FU8M}{?QUEST_GIVER.GENDER}She{?}He{\\?} cares nothing about me! Only about {?QUEST_GIVER.GENDER}her{?}his{\\?} reputation in our district.", null);
					StringHelpers.SetCharacterProperties("QUEST_GIVER", this.QuestGiver.CharacterObject, null, textObject2, false);
					dialog.AddDialogLine("daughter_persuade_to_come_introduction", "start_daughter_persuade_to_come_persuasion", "daughter_persuade_to_come_start_reservation", textObject2.ToString(), null, new ConversationSentence.OnConsequenceDelegate(this.persuasion_start_with_daughter_on_consequence), this, 100, null);
					dialog.AddDialogLine("daughter_persuade_to_come_rejected", "daughter_persuade_to_come_start_reservation", "daughter_persuade_to_come_persuasion_failed", "{=!}{FAILED_PERSUASION_LINE}", new ConversationSentence.OnConditionDelegate(this.daughter_persuade_to_come_persuasion_failed_on_condition), new ConversationSentence.OnConsequenceDelegate(this.daughter_persuade_to_come_persuasion_failed_on_consequence), this, 100, null);
					dialog.AddDialogLine("daughter_persuade_to_come_failed", "daughter_persuade_to_come_persuasion_failed", "daughter_persuade_to_come_persuasion_finished", textObject.ToString(), null, null, this, 100, null);
					dialog.AddDialogLine("daughter_persuade_to_come_start", "daughter_persuade_to_come_start_reservation", "daughter_persuade_to_come_persuasion_select_option", "{=9b2BETct}I have already decided. Don't expect me to change my mind.", () => !this.daughter_persuade_to_come_persuasion_failed_on_condition(), null, this, 100, null);
					dialog.AddDialogLine("daughter_persuade_to_come_success", "daughter_persuade_to_come_start_reservation", "close_window", "{=3tmXBpRH}You're right. I cannot do this. I will return to my family. ", new ConversationSentence.OnConditionDelegate(ConversationManager.GetPersuasionProgressSatisfied), new ConversationSentence.OnConsequenceDelegate(this.daughter_persuade_to_come_persuasion_success_on_consequence), this, int.MaxValue, null);
					string id = "daughter_persuade_to_come_select_option_1";
					string inputToken = "daughter_persuade_to_come_persuasion_select_option";
					string outputToken = "daughter_persuade_to_come_persuasion_selected_option_response";
					string text = "{=!}{DAUGHTER_PERSUADE_TO_COME_PERSUADE_ATTEMPT_1}";
					ConversationSentence.OnConditionDelegate conditionDelegate = new ConversationSentence.OnConditionDelegate(this.persuasion_select_option_1_on_condition);
					ConversationSentence.OnConsequenceDelegate consequenceDelegate = new ConversationSentence.OnConsequenceDelegate(this.persuasion_select_option_1_on_consequence);
					ConversationSentence.OnPersuasionOptionDelegate persuasionOptionDelegate = new ConversationSentence.OnPersuasionOptionDelegate(this.persuasion_setup_option_1);
					ConversationSentence.OnClickableConditionDelegate clickableConditionDelegate = new ConversationSentence.OnClickableConditionDelegate(this.persuasion_clickable_option_1_on_condition);
					dialog.AddPlayerLine(id, inputToken, outputToken, text, conditionDelegate, consequenceDelegate, this, 100, clickableConditionDelegate, persuasionOptionDelegate);
					string id2 = "daughter_persuade_to_come_select_option_2";
					string inputToken2 = "daughter_persuade_to_come_persuasion_select_option";
					string outputToken2 = "daughter_persuade_to_come_persuasion_selected_option_response";
					string text2 = "{=!}{DAUGHTER_PERSUADE_TO_COME_PERSUADE_ATTEMPT_2}";
					ConversationSentence.OnConditionDelegate conditionDelegate2 = new ConversationSentence.OnConditionDelegate(this.persuasion_select_option_2_on_condition);
					ConversationSentence.OnConsequenceDelegate consequenceDelegate2 = new ConversationSentence.OnConsequenceDelegate(this.persuasion_select_option_2_on_consequence);
					persuasionOptionDelegate = new ConversationSentence.OnPersuasionOptionDelegate(this.persuasion_setup_option_2);
					clickableConditionDelegate = new ConversationSentence.OnClickableConditionDelegate(this.persuasion_clickable_option_2_on_condition);
					dialog.AddPlayerLine(id2, inputToken2, outputToken2, text2, conditionDelegate2, consequenceDelegate2, this, 100, clickableConditionDelegate, persuasionOptionDelegate);
					string id3 = "daughter_persuade_to_come_select_option_3";
					string inputToken3 = "daughter_persuade_to_come_persuasion_select_option";
					string outputToken3 = "daughter_persuade_to_come_persuasion_selected_option_response";
					string text3 = "{=!}{DAUGHTER_PERSUADE_TO_COME_PERSUADE_ATTEMPT_3}";
					ConversationSentence.OnConditionDelegate conditionDelegate3 = new ConversationSentence.OnConditionDelegate(this.persuasion_select_option_3_on_condition);
					ConversationSentence.OnConsequenceDelegate consequenceDelegate3 = new ConversationSentence.OnConsequenceDelegate(this.persuasion_select_option_3_on_consequence);
					persuasionOptionDelegate = new ConversationSentence.OnPersuasionOptionDelegate(this.persuasion_setup_option_3);
					clickableConditionDelegate = new ConversationSentence.OnClickableConditionDelegate(this.persuasion_clickable_option_3_on_condition);
					dialog.AddPlayerLine(id3, inputToken3, outputToken3, text3, conditionDelegate3, consequenceDelegate3, this, 100, clickableConditionDelegate, persuasionOptionDelegate);
					string id4 = "daughter_persuade_to_come_select_option_4";
					string inputToken4 = "daughter_persuade_to_come_persuasion_select_option";
					string outputToken4 = "daughter_persuade_to_come_persuasion_selected_option_response";
					string text4 = "{=!}{DAUGHTER_PERSUADE_TO_COME_PERSUADE_ATTEMPT_4}";
					ConversationSentence.OnConditionDelegate conditionDelegate4 = new ConversationSentence.OnConditionDelegate(this.persuasion_select_option_4_on_condition);
					ConversationSentence.OnConsequenceDelegate consequenceDelegate4 = new ConversationSentence.OnConsequenceDelegate(this.persuasion_select_option_4_on_consequence);
					persuasionOptionDelegate = new ConversationSentence.OnPersuasionOptionDelegate(this.persuasion_setup_option_4);
					clickableConditionDelegate = new ConversationSentence.OnClickableConditionDelegate(this.persuasion_clickable_option_4_on_condition);
					dialog.AddPlayerLine(id4, inputToken4, outputToken4, text4, conditionDelegate4, consequenceDelegate4, this, 100, clickableConditionDelegate, persuasionOptionDelegate);
					dialog.AddDialogLine("daughter_persuade_to_come_select_option_reaction", "daughter_persuade_to_come_persuasion_selected_option_response", "daughter_persuade_to_come_start_reservation", "{=D0xDRqvm}{PERSUASION_REACTION}", new ConversationSentence.OnConditionDelegate(this.persuasion_selected_option_response_on_condition), new ConversationSentence.OnConsequenceDelegate(this.persuasion_selected_option_response_on_consequence), this, 100, null);
				}

				private void persuasion_selected_option_response_on_consequence()
				{
					Tuple<PersuasionOptionArgs, PersuasionOptionResult> tuple = ConversationManager.GetPersuasionChosenOptions().Last<Tuple<PersuasionOptionArgs, PersuasionOptionResult>>();
					float difficulty = Campaign.Current.Models.PersuasionModel.GetDifficulty(PersuasionDifficulty.Hard);
					float moveToNextStageChance;
					float blockRandomOptionChance;
					Campaign.Current.Models.PersuasionModel.GetEffectChances(tuple.Item1, out moveToNextStageChance, out blockRandomOptionChance, difficulty);
					this._task.ApplyEffects(moveToNextStageChance, blockRandomOptionChance);
				}

				private bool persuasion_selected_option_response_on_condition()
				{
					PersuasionOptionResult item = ConversationManager.GetPersuasionChosenOptions().Last<Tuple<PersuasionOptionArgs, PersuasionOptionResult>>().Item2;
					MBTextManager.SetTextVariable("PERSUASION_REACTION", PersuasionHelper.GetDefaultPersuasionOptionReaction(item), false);
					return true;
				}

				private bool persuasion_select_option_1_on_condition()
				{
					if (this._task.Options.Count<PersuasionOptionArgs>() > 0)
					{
						TextObject textObject = new TextObject("{=bSo9hKwr}{PERSUASION_OPTION_LINE} {SUCCESS_CHANCE}", null);
						textObject.SetTextVariable("SUCCESS_CHANCE", PersuasionHelper.ShowSuccess(this._task.Options.ElementAt(0), false));
						textObject.SetTextVariable("PERSUASION_OPTION_LINE", this._task.Options.ElementAt(0).Line);
						MBTextManager.SetTextVariable("DAUGHTER_PERSUADE_TO_COME_PERSUADE_ATTEMPT_1", textObject, false);
						return true;
					}
					return false;
				}

				private bool persuasion_select_option_2_on_condition()
				{
					if (this._task.Options.Count<PersuasionOptionArgs>() > 1)
					{
						TextObject textObject = new TextObject("{=bSo9hKwr}{PERSUASION_OPTION_LINE} {SUCCESS_CHANCE}", null);
						textObject.SetTextVariable("SUCCESS_CHANCE", PersuasionHelper.ShowSuccess(this._task.Options.ElementAt(1), false));
						textObject.SetTextVariable("PERSUASION_OPTION_LINE", this._task.Options.ElementAt(1).Line);
						MBTextManager.SetTextVariable("DAUGHTER_PERSUADE_TO_COME_PERSUADE_ATTEMPT_2", textObject, false);
						return true;
					}
					return false;
				}

				private bool persuasion_select_option_3_on_condition()
				{
					if (this._task.Options.Count<PersuasionOptionArgs>() > 2)
					{
						TextObject textObject = new TextObject("{=bSo9hKwr}{PERSUASION_OPTION_LINE} {SUCCESS_CHANCE}", null);
						textObject.SetTextVariable("SUCCESS_CHANCE", PersuasionHelper.ShowSuccess(this._task.Options.ElementAt(2), false));
						textObject.SetTextVariable("PERSUASION_OPTION_LINE", this._task.Options.ElementAt(2).Line);
						MBTextManager.SetTextVariable("DAUGHTER_PERSUADE_TO_COME_PERSUADE_ATTEMPT_3", textObject, false);
						return true;
					}
					return false;
				}

				private bool persuasion_select_option_4_on_condition()
				{
					if (this._task.Options.Count<PersuasionOptionArgs>() > 3)
					{
						TextObject textObject = new TextObject("{=bSo9hKwr}{PERSUASION_OPTION_LINE} {SUCCESS_CHANCE}", null);
						textObject.SetTextVariable("SUCCESS_CHANCE", PersuasionHelper.ShowSuccess(this._task.Options.ElementAt(3), false));
						textObject.SetTextVariable("PERSUASION_OPTION_LINE", this._task.Options.ElementAt(3).Line);
						MBTextManager.SetTextVariable("DAUGHTER_PERSUADE_TO_COME_PERSUADE_ATTEMPT_4", textObject, false);
						return true;
					}
					return false;
				}

				private void persuasion_select_option_1_on_consequence()
				{
					if (this._task.Options.Count > 0)
					{
						this._task.Options[0].BlockTheOption(true);
					}
				}

				private void persuasion_select_option_2_on_consequence()
				{
					if (this._task.Options.Count > 1)
					{
						this._task.Options[1].BlockTheOption(true);
					}
				}

				private void persuasion_select_option_3_on_consequence()
				{
					if (this._task.Options.Count > 2)
					{
						this._task.Options[2].BlockTheOption(true);
					}
				}

				private void persuasion_select_option_4_on_consequence()
				{
					if (this._task.Options.Count > 3)
					{
						this._task.Options[3].BlockTheOption(true);
					}
				}

				private PersuasionOptionArgs persuasion_setup_option_1()
				{
					return this._task.Options.ElementAt(0);
				}

				private PersuasionOptionArgs persuasion_setup_option_2()
				{
					return this._task.Options.ElementAt(1);
				}

				private PersuasionOptionArgs persuasion_setup_option_3()
				{
					return this._task.Options.ElementAt(2);
				}

				private PersuasionOptionArgs persuasion_setup_option_4()
				{
					return this._task.Options.ElementAt(3);
				}

				private bool persuasion_clickable_option_1_on_condition(out TextObject hintText)
				{
					hintText = new TextObject("{=9ACJsI6S}Blocked", null);
					if (this._task.Options.Any<PersuasionOptionArgs>())
					{
						hintText = (this._task.Options.ElementAt(0).IsBlocked ? hintText : TextObject.Empty);
						return !this._task.Options.ElementAt(0).IsBlocked;
					}
					return false;
				}

				private bool persuasion_clickable_option_2_on_condition(out TextObject hintText)
				{
					hintText = new TextObject("{=9ACJsI6S}Blocked", null);
					if (this._task.Options.Count > 1)
					{
						hintText = (this._task.Options.ElementAt(1).IsBlocked ? hintText : TextObject.Empty);
						return !this._task.Options.ElementAt(1).IsBlocked;
					}
					return false;
				}

				private bool persuasion_clickable_option_3_on_condition(out TextObject hintText)
				{
					hintText = new TextObject("{=9ACJsI6S}Blocked", null);
					if (this._task.Options.Count > 2)
					{
						hintText = (this._task.Options.ElementAt(2).IsBlocked ? hintText : TextObject.Empty);
						return !this._task.Options.ElementAt(2).IsBlocked;
					}
					return false;
				}

				private bool persuasion_clickable_option_4_on_condition(out TextObject hintText)
				{
					hintText = new TextObject("{=9ACJsI6S}Blocked", null);
					if (this._task.Options.Count > 3)
					{
						hintText = (this._task.Options.ElementAt(3).IsBlocked ? hintText : TextObject.Empty);
						return !this._task.Options.ElementAt(3).IsBlocked;
					}
					return false;
				}

				private PersuasionTask GetPersuasionTask()
				{
					PersuasionTask persuasionTask = new PersuasionTask(0);
					persuasionTask.FinalFailLine = new TextObject("{=5aDlmdmb}No... No. It does not make sense.", null);
					persuasionTask.TryLaterLine = TextObject.Empty;
					persuasionTask.SpokenLine = new TextObject("{=Lkf3sB9I}Maybe...", null);
					PersuasionOptionArgs option = new PersuasionOptionArgs(DefaultSkills.Leadership, DefaultTraits.Honor, TraitEffect.Positive, PersuasionArgumentStrength.Hard, true, new TextObject("{=Nhfl6tcM}Maybe, but that is your duty to your family.", null), null, false, false, false);
					persuasionTask.AddOptionToTask(option);
					TextObject textObject = new TextObject("{=lustkZ7s}Perhaps {?QUEST_GIVER.GENDER}she{?}he{\\?} made those plans because {?QUEST_GIVER.GENDER}she{?}he{\\?} loves you.", null);
					StringHelpers.SetCharacterProperties("QUEST_GIVER", this.QuestGiver.CharacterObject, null, textObject, false);
					PersuasionOptionArgs option2 = new PersuasionOptionArgs(DefaultSkills.Charm, DefaultTraits.Mercy, TraitEffect.Positive, PersuasionArgumentStrength.VeryEasy, false, textObject, null, false, false, false);
					persuasionTask.AddOptionToTask(option2);
					PersuasionOptionArgs option3 = new PersuasionOptionArgs(DefaultSkills.Roguery, DefaultTraits.Calculating, TraitEffect.Positive, PersuasionArgumentStrength.VeryHard, false, new TextObject("{=Ns6Svjsn}Do you think this one will be faithful to you over many years? I know a rogue when I see one.", null), null, false, false, false);
					persuasionTask.AddOptionToTask(option3);
					PersuasionOptionArgs option4 = new PersuasionOptionArgs(DefaultSkills.Roguery, DefaultTraits.Mercy, TraitEffect.Negative, PersuasionArgumentStrength.ExtremelyHard, true, new TextObject("{=2dL6j8Hp}You want to marry a corpse? Because I'm going to kill your lover if you don't listen.", null), null, true, false, false);
					persuasionTask.AddOptionToTask(option4);
					return persuasionTask;
				}

				private void persuasion_start_with_daughter_on_consequence()
				{
					ConversationManager.StartPersuasion(2f, 1f, 0f, 2f, 2f, 0f, PersuasionDifficulty.Hard);
				}

				private void daughter_persuade_to_come_persuasion_success_on_consequence()
				{
					ConversationManager.EndPersuasion();
					this._isDaughterPersuaded = true;
				}

				private bool daughter_persuade_to_come_persuasion_failed_on_condition()
				{
					if (this._task.Options.All((PersuasionOptionArgs x) => x.IsBlocked) && !ConversationManager.GetPersuasionProgressSatisfied())
					{
						MBTextManager.SetTextVariable("FAILED_PERSUASION_LINE", this._task.FinalFailLine, false);
						return true;
					}
					return false;
				}

				private void daughter_persuade_to_come_persuasion_failed_on_consequence()
				{
					ConversationManager.EndPersuasion();
				}

				private void OnSettlementLeft(MobileParty party, Settlement settlement)
				{
					if (party.IsMainParty && settlement == this.QuestGiver.CurrentSettlement && this._exitedQuestSettlementForTheFirstTime)
					{
						if (this.DoesMainPartyHasEnoughScoutingSkill)
						{
							QuestHelper.AddMapArrowFromPointToTarget(new TextObject("{=YdwLnWa1}Direction of daughter and rogue", null), settlement.Position2D, this._targetVillage.Settlement.Position2D, 5f, 0.1f, 5858);
							InformationManager.AddQuickInformation(new TextObject("{=O15PyNUK}With the help of your scouting skill, you were able to trace their tracks.", null), 0, null, "");
							InformationManager.AddQuickInformation(new TextObject("{=gOWebWiK}Their direction is marked with an arrow in the campaign map.", null), 0, null, "");
							base.AddTrackedObject(this._targetVillage.Settlement);
						}
						else
						{
							foreach (Village village in this.QuestGiver.CurrentSettlement.Village.TradeBound.BoundVillages)
							{
								if (village != this.QuestGiver.CurrentSettlement.Village)
								{
									this._villagesAndAlreadyVisitedBooleans.Add(village, false);
									base.AddTrackedObject(village.Settlement);
								}
							}
						}
						TextObject textObject = new TextObject("{=FvtAJE2Q}In order to find {QUEST_GIVER.LINK}'s daughter, you have decided to visit nearby villages.", null);
						StringHelpers.SetCharacterProperties("QUEST_GIVER", this.QuestGiver.CharacterObject, null, textObject, false);
						base.AddLog(textObject, this.DoesMainPartyHasEnoughScoutingSkill);
						this._exitedQuestSettlementForTheFirstTime = false;
					}
					if (party.IsMainParty && settlement == this._targetVillage.Settlement)
					{
						this._isQuestTargetMission = false;
					}
				}

				private void HandleTheRogueAndDaughter()
				{
					this._daughterHero.AddEventForOccupiedHero(base.StringId);
					this._rogueHero.AddEventForOccupiedHero(base.StringId);
				}

				public void OnBeforeMissionOpened()
				{
					if (this._isQuestTargetMission)
					{
						Location locationWithId = Settlement.CurrentSettlement.LocationComplex.GetLocationWithId("village_center");
						if (locationWithId != null)
						{
							ItemObject @object = MBObjectManager.Instance.GetObject<ItemObject>("short_sword_t3");
							this._rogueHero.CivilianEquipment.AddEquipmentToSlotWithoutAgent(0, new EquipmentElement(@object, null));
							locationWithId.AddCharacter(this.CreateQuestLocationCharacter(this._daughterHero.CharacterObject, LocationCharacter.CharacterRelations.Neutral));
							locationWithId.AddCharacter(this.CreateQuestLocationCharacter(this._rogueHero.CharacterObject, LocationCharacter.CharacterRelations.Neutral));
						}
					}
				}

				private void OnMissionEnded(IMission mission)
				{
					if (this._isQuestTargetMission)
					{
						this._daughterAgent = null;
						this._rogueAgent = null;
						if (this._isDaughterPersuaded)
						{
							this.ApplyDeliverySuccessConsequences();
							base.CompleteQuestWithSuccess();
							base.AddLog(this._successQuestLogText, false);
							return;
						}
						if (this._acceptedDaughtersEscape)
						{
							this.ApplyDaughtersEscapeAcceptedFailConsequences();
							base.CompleteQuestWithFail(this._failQuestLogText);
							return;
						}
						if (this._isDaughterCaptured)
						{
							MakeHeroFugitiveAction.Apply(this._daughterHero);
							this.ApplyDeliverySuccessConsequences();
							base.CompleteQuestWithSuccess();
							base.AddLog(this._successQuestLogText, false);
						}
					}
				}

				private LocationCharacter CreateQuestLocationCharacter(CharacterObject character, LocationCharacter.CharacterRelations relation)
				{
					Tuple<string, Monster> tuple = new Tuple<string, Monster>("as_human_villager", Campaign.Current.HumanMonsterSettlement);
					return new LocationCharacter(new AgentData(new SimpleAgentOrigin(character, -1, null, default(UniqueTroopDescriptor))).Monster(tuple.Item2), new LocationCharacter.AddBehaviorsDelegate(SandBoxManager.Instance.AgentBehaviorManager.AddCompanionBehaviors), "common_area_2", true, relation, tuple.Item1, false, false, null, false, true, true);
				}

				private void OnSettlementEntered(MobileParty party, Settlement settlement, Hero hero)
				{
					if (party != null && party.IsMainParty && settlement.IsVillage)
					{
						if (this._villagesAndAlreadyVisitedBooleans.ContainsKey(settlement.Village) && !this._villagesAndAlreadyVisitedBooleans[settlement.Village])
						{
							if (settlement.Village != this._targetVillage)
							{
								TextObject textObject = settlement.IsRaided ? new TextObject("{=YTaM6G1E}It seems the village has been raided a short while ago. You found nothing but smoke, fire and crying people.", null) : new TextObject("{=2P3UJ8be}You ask around the village if anyone saw {TARGET_HERO.LINK} or some suspicious characters with a young woman.\n\nVillagers say that they saw a young man and woman ride in early in the morning. They bought some supplies and trotted off towards {TARGET_VILLAGE}.", null);
								textObject.SetTextVariable("TARGET_VILLAGE", this._targetVillage.Name);
								StringHelpers.SetCharacterProperties("TARGET_HERO", this._daughterHero.CharacterObject, null, textObject, false);
								InformationManager.ShowInquiry(new InquiryData(this.Title.ToString(), textObject.ToString(), true, false, new TextObject("{=yS7PvrTD}OK", null).ToString(), "", null, null, ""), false);
								if (!this._isTrackerLogAdded)
								{
									TextObject textObject2 = new TextObject("{=WGi3Zuv7}You asked the villagers around {CURRENT_SETTLEMENT} if they saw a young woman matching the description of {QUEST_GIVER.LINK}'s daughter, {TARGET_HERO.LINK}.\n\nThey said a young woman and a young man dropped by early in the morning to buy some supplies and then rode off towards {TARGET_VILLAGE}.", null);
									textObject2.SetTextVariable("CURRENT_SETTLEMENT", Hero.MainHero.CurrentSettlement.Name);
									textObject2.SetTextVariable("TARGET_VILLAGE", this._targetVillage.Settlement.EncyclopediaLinkWithName);
									StringHelpers.SetCharacterProperties("TARGET_HERO", this._daughterHero.CharacterObject, null, textObject2, false);
									StringHelpers.SetCharacterProperties("QUEST_GIVER", this.QuestGiver.CharacterObject, null, textObject2, false);
									base.AddLog(textObject2, false);
									this._isTrackerLogAdded = true;
								}
							}
							else
							{
								InquiryData inquiryData;
								if (settlement.IsRaided)
								{
									TextObject textObject3 = new TextObject("{=edoXFdmg}You have found {QUEST_GIVER.NAME}'s daughter.", null);
									StringHelpers.SetCharacterProperties("QUEST_GIVER", this.QuestGiver.CharacterObject, null, textObject3, false);
									TextObject textObject4 = new TextObject("{=aYMW8bWi}Talk to her", null);
									inquiryData = new InquiryData(this.Title.ToString(), textObject3.ToString(), true, false, textObject4.ToString(), null, new Action(this.TalkWithDaughterAfterRaid), null, "");
								}
								else
								{
									TextObject textObject5 = new TextObject("{=bbwNIIKI}You ask around the village if anyone saw {TARGET_HERO.LINK} or some suspicious characters with a young woman.\n\nVillagers say that there was a young man and woman who arrived here exhausted. The villagers allowed them to stay for a while.\nYou can check the area, and see if they are still hiding here.", null);
									StringHelpers.SetCharacterProperties("TARGET_HERO", this._daughterHero.CharacterObject, null, textObject5, false);
									inquiryData = new InquiryData(this.Title.ToString(), textObject5.ToString(), true, true, new TextObject("{=bb6e8DoM}Search the village", null).ToString(), new TextObject("{=3CpNUnVl}Cancel", null).ToString(), new Action(this.SearchTheVillage), null, "");
								}
								InformationManager.ShowInquiry(inquiryData, false);
							}
							this._villagesAndAlreadyVisitedBooleans[settlement.Village] = true;
						}
						if (settlement == this._targetVillage.Settlement)
						{
							base.AddTrackedObject(this._daughterHero);
							base.AddTrackedObject(this._rogueHero);
							this._isQuestTargetMission = true;
						}
					}
				}

				private void SearchTheVillage()
				{
					VillageEncouter villageEncouter = PlayerEncounter.LocationEncounter as VillageEncouter;
					if (villageEncouter == null)
					{
						return;
					}
					villageEncouter.CreateAndOpenMissionController(LocationComplex.Current.GetLocationWithId("village_center"), null, null, null);
				}

				private void TalkWithDaughterAfterRaid()
				{
					this._villageIsRaidedTalkWithDaughter = true;
					CampaignMapConversation.OpenConversation(new ConversationCharacterData(CharacterObject.PlayerCharacter, null, false, false, false, false), new ConversationCharacterData(this._daughterHero.CharacterObject, null, false, false, false, false));
				}

				private void QuestAcceptedConsequences()
				{
					base.StartQuest();
					base.AddLog(this._playerStartsQuestLogText, false);
				}

				protected override void RegisterEvents()
				{
					CampaignEvents.OnSettlementLeftEvent.AddNonSerializedListener(this, new Action<MobileParty, Settlement>(this.OnSettlementLeft));
					CampaignEvents.SettlementEntered.AddNonSerializedListener(this, new Action<MobileParty, Settlement, Hero>(this.OnSettlementEntered));
					CampaignEvents.BeforeMissionOpenedEvent.AddNonSerializedListener(this, new Action(this.OnBeforeMissionOpened));
					CampaignEvents.OnMissionEndedEvent.AddNonSerializedListener(this, new Action<IMission>(this.OnMissionEnded));
					CampaignEvents.WarDeclared.AddNonSerializedListener(this, new Action<IFaction, IFaction>(this.WarDeclared));
				}

				private void WarDeclared(IFaction faction1, IFaction faction2)
				{
					if (Hero.MainHero.MapFaction.IsAtWarWith(this.QuestGiver.CurrentSettlement.MapFaction))
					{
						base.AddLog(this._questCanceledWarDeclaredLog, false);
						base.CompleteQuestWithCancel(null);
					}
				}

				protected override void OnFinalize()
				{
					Campaign.Current.VisualTrackerManager.RemoveTrackedObject(this._targetVillage.Settlement);
					if (!this.DoesMainPartyHasEnoughScoutingSkill)
					{
						foreach (Village village in this.QuestGiver.CurrentSettlement.BoundVillages)
						{
							base.RemoveTrackedObject(village.Settlement);
						}
					}
					Hero daughterHero = this._daughterHero;
					if (daughterHero != null)
					{
						daughterHero.RemoveEventFromOccupiedHero(base.StringId);
					}
					Hero rogueHero = this._rogueHero;
					if (rogueHero != null)
					{
						rogueHero.RemoveEventFromOccupiedHero(base.StringId);
					}
					if (this._rogueHero != null && this._rogueHero.IsActive && this._rogueHero.IsAlive)
					{
						KillCharacterAction.ApplyByMurder(this._rogueHero, null, false);
					}
				}

				[SaveableField(10)]
				private readonly Hero _daughterHero;

				[SaveableField(20)]
				private readonly Hero _rogueHero;

				private Agent _daughterAgent;

				private Agent _rogueAgent;

				[SaveableField(50)]
				private bool _isQuestTargetMission;

				[SaveableField(60)]
				private bool _didPlayerBeatRouge;

				[SaveableField(70)]
				private bool _exitedQuestSettlementForTheFirstTime = true;

				[SaveableField(80)]
				private bool _isTrackerLogAdded;

				[SaveableField(90)]
				private bool _isDaughterPersuaded;

				[SaveableField(91)]
				private bool _isDaughterCaptured;

				[SaveableField(100)]
				private bool _acceptedDaughtersEscape;

				[SaveableField(110)]
				private readonly Village _targetVillage;

				[SaveableField(120)]
				private bool _villageIsRaidedTalkWithDaughter;

				[SaveableField(140)]
				private Dictionary<Village, bool> _villagesAndAlreadyVisitedBooleans = new Dictionary<Village, bool>();

				private Dictionary<string, CharacterObject> _rogueCharacterBasedOnCulture = new Dictionary<string, CharacterObject>();

				private PersuasionTask _task;

				private const PersuasionDifficulty Difficulty = PersuasionDifficulty.Hard;

				[SaveableField(130)]
				private readonly float _questDifficultyMultiplier;
			}
		}
		
}
