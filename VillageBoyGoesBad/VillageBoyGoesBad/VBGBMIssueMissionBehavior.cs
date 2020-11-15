using System;
using System.Collections.Generic;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace SandBox.Source.Missions.Handlers
{
	public class VBGBMissionFightHandler : MissionLogic
	{
		// Token: 0x170000B3 RID: 179
		// (get) Token: 0x06000887 RID: 2183 RVA: 0x000458ED File Offset: 0x00043AED
		public IEnumerable<Agent> PlayerSideAgents
		{
			get
			{
				return this._playerSideAgents.AsReadOnly();
			}
		}

		// Token: 0x170000B4 RID: 180
		// (get) Token: 0x06000888 RID: 2184 RVA: 0x000458FA File Offset: 0x00043AFA
		public IEnumerable<Agent> OpponentSideAgents
		{
			get
			{
				return this._opponentSideAgents.AsReadOnly();
			}
		}

		// Token: 0x170000B5 RID: 181
		// (get) Token: 0x06000889 RID: 2185 RVA: 0x00045907 File Offset: 0x00043B07
		private static VBGBMissionFightHandler _current
		{
			get
			{
				return Mission.Current.GetMissionBehaviour<VBGBMissionFightHandler>();
			}
		}

		// Token: 0x170000B6 RID: 182
		// (get) Token: 0x0600088A RID: 2186 RVA: 0x00045913 File Offset: 0x00043B13
		public bool IsPlayerSideWon
		{
			get
			{
				return this._isPlayerSideWon;
			}
		}

		// Token: 0x0600088B RID: 2187 RVA: 0x0004591B File Offset: 0x00043B1B
		public override void EarlyStart()
		{
			this._playerSideAgents = new List<Agent>();
			this._opponentSideAgents = new List<Agent>();
			this._missionAgentHandler = base.Mission.GetMissionBehaviour<MissionAgentHandler>();
		}

		// Token: 0x0600088C RID: 2188 RVA: 0x00045944 File Offset: 0x00043B44
		public override void AfterStart()
		{
			LocationEncounter locationEncounter = PlayerEncounter.LocationEncounter;
			if (locationEncounter != null && locationEncounter.DuelOpponent != null)
			{
				VBGBMissionFightHandler.StartDuel();
			}
		}

		// Token: 0x0600088D RID: 2189 RVA: 0x00045968 File Offset: 0x00043B68
		public override void OnMissionTick(float dt)
		{
			if (this._finishTimer != null && this._finishTimer.ElapsedTime > 5f)
			{
				this._finishTimer = null;
				this.EndFight();
				this._prepareTimer = new BasicTimer((MBCommon.TimeType)1);
			}
			if (this._prepareTimer != null && this._prepareTimer.ElapsedTime > 3f)
			{
				this._prepareTimer = null;
			}
		}

		// Token: 0x0600088E RID: 2190 RVA: 0x000459CC File Offset: 0x00043BCC
		public void StartCustomFight(List<Agent> playerSideAgents, List<Agent> opponentSideAgents, bool isThereSpectators, bool dropWeapons, bool isItemUseDisabled, VBGBMissionFightHandler.OnFightEndDelegate onFightEndDelegate, bool spawnReinforcements = true, List<LocationCharacter> playerSideReinforcements = null, List<LocationCharacter> opponentSideReinforcements = null, List<MatrixFrame> playerResinforcementSpawnFrames = null, List<MatrixFrame> opponentReinforcementSpawnFrames = null)
		{
			this._state = VBGBMissionFightHandler.State.Fighting;
			this._opponentSideAgents = opponentSideAgents;
			this._playerSideAgents = playerSideAgents;
			this._playerSideAgentsOldTeamData = new Dictionary<Agent, Team>();
			this._opponentSideAgentsOldTeamData = new Dictionary<Agent, Team>();
			this._playerSideInitialAgentCount = this._playerSideAgents.Count;
			this._opponentSideInitialAgentCount = this._opponentSideAgents.Count;
			this._playerSideReinforcements = (playerSideReinforcements ?? new List<LocationCharacter>());
			this._opponentSideReinforcements = (opponentSideReinforcements ?? new List<LocationCharacter>());
			this._playerReinforcementSpawnFrames = (playerResinforcementSpawnFrames ?? new List<MatrixFrame>());
			this._opponentReinforcementSpawnFrames = (opponentReinforcementSpawnFrames ?? new List<MatrixFrame>());
			VBGBMissionFightHandler._onFightEnd = onFightEndDelegate;
			this._spawnReinforcements = spawnReinforcements;
			if (this._opponentSideReinforcements.Any<LocationCharacter>() && this._opponentReinforcementSpawnFrames.IsEmpty<MatrixFrame>())
			{
				this._opponentReinforcementSpawnFrames.Add(Agent.Main.Frame);
			}
			VBGBMissionFightHandler.IsThereSpectators = isThereSpectators;
			Mission.Current.MainAgent.IsItemUseDisabled = isItemUseDisabled;
			this._oldMissionMode = Mission.Current.Mode;
			Mission.Current.SetMissionMode(MissionMode.Battle, false);
			foreach (Agent agent in this._opponentSideAgents)
			{
				if (dropWeapons)
				{
					this.DropAllWeapons(agent);
				}
				this._opponentSideAgentsOldTeamData.Add(agent, agent.Team);
				this.ForceAgentForFight(agent);
			}
			foreach (Agent agent2 in this._playerSideAgents)
			{
				if (dropWeapons)
				{
					this.DropAllWeapons(agent2);
				}
				this._playerSideAgentsOldTeamData.Add(agent2, agent2.Team);
				this.ForceAgentForFight(agent2);
			}
			this.SetTeamsForFightAndDuel();
		}

		// Token: 0x0600088F RID: 2191 RVA: 0x00045BA4 File Offset: 0x00043DA4
		public static void StartFight()
		{
			Agent agent = VBGBMissionFightHandler.DuelAgent ?? ConversationMission.OneToOneConversationAgent;
			PlayerEncounter.LocationEncounter.DuelOpponent = (((agent != null) ? agent.Character : null) as CharacterObject);
			agent.DisableScriptedMovement();
			agent.ClearTargetFrame();
			VBGBMissionFightHandler._current.StartCustomFight(new List<Agent>
			{
				Agent.Main
			}, new List<Agent>
			{
				agent
			}, true, false, false, null, true, null, null, null, null);
		}

		// Token: 0x06000890 RID: 2192 RVA: 0x00045C18 File Offset: 0x00043E18
		public static void StartBrawl()
		{
			LocationEncounter locationEncounter = PlayerEncounter.LocationEncounter;
			VBGBMissionFightHandler._current.StartCustomFight(new List<Agent>
			{
				Agent.Main
			}, new List<Agent>(), true, true, true, null, true, null, null, null, null);
		}

		// Token: 0x06000891 RID: 2193 RVA: 0x00045C54 File Offset: 0x00043E54
		public static void StartDuel()
		{
			VBGBMissionFightHandler current = VBGBMissionFightHandler._current;
			current._state = VBGBMissionFightHandler.State.Fighting;
			current._playerSideAgents.Add(Agent.Main);
			LocationEncounter encounter = PlayerEncounter.LocationEncounter;
			Agent agent2 = Mission.Current.Agents.FirstOrDefault((Agent agent) => agent.Character == encounter.DuelOpponent);
			GameEntity gameEntity = Mission.Current.Scene.FindEntityWithTag("sp_duel_npc");
			agent2.TeleportToPosition(gameEntity.GetFrame().origin);
			Vec3 f = gameEntity.GetFrame().rotation.f;
			agent2.SetMovementDirection(ref f);
			GameEntity gameEntity2 = Mission.Current.Scene.FindEntityWithTag("sp_duel_player");
			Mission.Current.MainAgent.TeleportToPosition(gameEntity2.GetFrame().origin);
			Vec3 f2 = gameEntity2.GetFrame().rotation.f;
			Mission.Current.MainAgent.SetMovementDirection(ref f2);
			VBGBMissionFightHandler._current.StartCustomFight(new List<Agent>
			{
				Agent.Main
			}, new List<Agent>
			{
				agent2
			}, true, false, false, null, true, null, null, null, null);
		}

		// Token: 0x06000892 RID: 2194 RVA: 0x00045D6C File Offset: 0x00043F6C
		private void ForceAgentForFight(Agent agent)
		{
			if (agent.GetComponent<CampaignAgentComponent>().AgentNavigator != null)
			{
				AlarmedBehaviorGroup behaviorGroup = agent.GetComponent<CampaignAgentComponent>().AgentNavigator.GetBehaviorGroup<AlarmedBehaviorGroup>();
				behaviorGroup.DisableCalmDown = true;
				behaviorGroup.AddBehavior<FightBehavior>();
				behaviorGroup.SetScriptedBehavior<FightBehavior>();
			}
		}

		// Token: 0x06000893 RID: 2195 RVA: 0x00045DA0 File Offset: 0x00043FA0
		private void SetTeamsForFightAndDuel()
		{
			Mission.Current.PlayerEnemyTeam.SetIsEnemyOf(Mission.Current.PlayerTeam, true);
			foreach (Agent agent in this._playerSideAgents)
			{
				if (agent.IsHuman)
				{
					if (agent.IsAIControlled)
					{
						AgentComponentExtensions.SetWatchState(agent, (AgentAIStateFlagComponent.WatchState)2);
					}
					agent.SetTeam(Mission.Current.PlayerTeam, true);
				}
			}
			foreach (Agent agent2 in this._opponentSideAgents)
			{
				if (agent2.IsHuman)
				{
					if (agent2.IsAIControlled)
					{
						AgentComponentExtensions.SetWatchState(agent2, (AgentAIStateFlagComponent.WatchState)2);
					}
					agent2.SetTeam(Mission.Current.PlayerEnemyTeam, true);
				}
			}
		}

		// Token: 0x06000894 RID: 2196 RVA: 0x00045E94 File Offset: 0x00044094
		private void ResetTeamsForFightAndDuel()
		{
			foreach (Agent agent in this._playerSideAgents)
			{
				if (agent.IsAIControlled)
				{
					AgentComponentExtensions.SetWatchState(agent, 0);
				}
				agent.SetTeam(new Team(this._playerSideAgentsOldTeamData[agent].MBTeam, BattleSideEnum.None, uint.MaxValue, uint.MaxValue, null), true);
			}
			foreach (Agent agent2 in this._opponentSideAgents)
			{
				if (agent2.IsAIControlled)
				{
					AgentComponentExtensions.SetWatchState(agent2, 0);
				}
				agent2.SetTeam(new Team(this._opponentSideAgentsOldTeamData[agent2].MBTeam, BattleSideEnum.None, uint.MaxValue, uint.MaxValue, null), true);
			}
		}

		// Token: 0x06000895 RID: 2197 RVA: 0x00045F7C File Offset: 0x0004417C
		public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow killingBlow)
		{
			if (this._state != VBGBMissionFightHandler.State.Fighting)
			{
				return;
			}
			//if (affectedAgent == Agent.Main)
			//{
			//	Mission.Current.NextCheckTimeEndMission += 8f;
			//}
			if (affectorAgent != null && this._playerSideAgents.Contains(affectedAgent))
			{
				this._playerSideAgents.Remove(affectedAgent);
				if (this._spawnReinforcements)
				{
					this.CheckForReinforcement(this._playerSideAgents, this._playerSideReinforcements, this._playerReinforcementSpawnFrames, this._playerSideInitialAgentCount, base.Mission.PlayerTeam);
				}
				if (this._playerSideAgents.Count == 0 && (!this._spawnReinforcements || this._playerSideReinforcements.Count == 0))
				{
					this._isPlayerSideWon = false;
					this.ResetScriptedBehaviors();
					this._finishTimer = new BasicTimer((MBCommon.TimeType)1);
					return;
				}
			}
			else if (affectorAgent != null && this._opponentSideAgents.Contains(affectedAgent))
			{
				this._opponentSideAgents.Remove(affectedAgent);
				if (this._spawnReinforcements)
				{
					this.CheckForReinforcement(this._opponentSideAgents, this._opponentSideReinforcements, this._opponentReinforcementSpawnFrames, this._opponentSideInitialAgentCount, base.Mission.PlayerEnemyTeam);
				}
				if (this._opponentSideAgents.Count == 0 && (!this._spawnReinforcements || this._opponentSideReinforcements.Count == 0))
				{
					this._isPlayerSideWon = true;
					this.ResetScriptedBehaviors();
					this._finishTimer = new BasicTimer((MBCommon.TimeType)1);
				}
			}
		}

		// Token: 0x06000896 RID: 2198 RVA: 0x000460D8 File Offset: 0x000442D8
		private void CheckForReinforcement(List<Agent> agentList, List<LocationCharacter> reinforcementList, List<MatrixFrame> spawnFrames, int initialAgentCount, Team team)
		{
			if (agentList.Count <= initialAgentCount / 2)
			{
				for (int i = Math.Min(initialAgentCount - agentList.Count, reinforcementList.Count); i > 0; i--)
				{
					LocationCharacter locationCharacter = reinforcementList.First<LocationCharacter>();
					reinforcementList.RemoveAt(0);
					Agent agent = this._missionAgentHandler.SpawnHiddenLocationCharacter(locationCharacter, spawnFrames[MBRandom.RandomInt(spawnFrames.Count)]);
					AgentComponentExtensions.SetWatchState(agent, (AgentAIStateFlagComponent.WatchState)2);
					agent.SetTeam(team, true);
					this.ForceAgentForFight(agent);
					agentList.Add(agent);
				}
			}
		}

		// Token: 0x06000897 RID: 2199 RVA: 0x0004615A File Offset: 0x0004435A
		public override bool IsAgentInteractionAllowed()
		{
			return this._state != VBGBMissionFightHandler.State.Fighting;
		}

		// Token: 0x06000898 RID: 2200 RVA: 0x00046168 File Offset: 0x00044368
		public static Agent GetAgentToSpectate()
		{
			VBGBMissionFightHandler current = VBGBMissionFightHandler._current;
			if (current._playerSideAgents.Count > 0)
			{
				return current._playerSideAgents[0];
			}
			if (current._opponentSideAgents.Count > 0)
			{
				return current._opponentSideAgents[0];
			}
			return null;
		}

		// Token: 0x06000899 RID: 2201 RVA: 0x000461B4 File Offset: 0x000443B4
		public static bool IsDuelOpponent(Agent agent)
		{
			LocationEncounter locationEncounter = PlayerEncounter.LocationEncounter;
			return agent != null && locationEncounter != null && locationEncounter.DuelOpponent != null && agent.Character == locationEncounter.DuelOpponent;
		}

		// Token: 0x0600089A RID: 2202 RVA: 0x000461E8 File Offset: 0x000443E8
		public static void EndDuelFromConversation()
		{
			LocationEncounter locationEncounter = PlayerEncounter.LocationEncounter;
			if (locationEncounter != null && locationEncounter.DuelOpponent != null)
			{
				foreach (Agent agent in Mission.Current.Agents)
				{
					if (agent.Character == locationEncounter.DuelOpponent)
					{
						agent.GetComponent<CampaignAgentComponent>().AgentNavigator.GetBehaviorGroup<AlarmedBehaviorGroup>().DisableScriptedBehavior();
						AgentComponentExtensions.SetWatchState(agent, 0);
						agent.TryToSheathWeaponInHand((Agent.HandIndex)1, (Agent.WeaponWieldActionType)1);
						agent.TryToSheathWeaponInHand(0, 0);
						agent.UpdateWeapons();
					}
				}
				VBGBMissionFightHandler._current.EndFight();
			}
		}

		// Token: 0x0600089B RID: 2203 RVA: 0x00046290 File Offset: 0x00044490
		public override InquiryData OnEndMissionRequest(out bool canPlayerLeave)
		{
			canPlayerLeave = true;
			if (this._state == VBGBMissionFightHandler.State.Fighting && (this._opponentSideAgents.Count > 0 || this._playerSideAgents.Count > 0))
			{
				InformationManager.AddQuickInformation(new TextObject("{=Fpk3BUBs}Your duel has not ended yet!", null), 0, null, "");
				canPlayerLeave = false;
			}
			return null;
		}

		// Token: 0x0600089C RID: 2204 RVA: 0x000462E0 File Offset: 0x000444E0
		private void DropAllWeapons(Agent agent)
		{
			for (EquipmentIndex equipmentIndex = EquipmentIndex.WeaponItemBeginSlot; equipmentIndex < EquipmentIndex.NumAllWeaponSlots; equipmentIndex++)
			{
				if (!agent.Equipment[equipmentIndex].IsEmpty)
				{
					agent.DropItem(equipmentIndex, WeaponClass.Undefined);
				}
			}
		}

		// Token: 0x0600089D RID: 2205 RVA: 0x00046318 File Offset: 0x00044518
		private void ResetScriptedBehaviors()
		{
			foreach (Agent agent in this._playerSideAgents)
			{
				if (agent.IsActive() && agent.GetComponent<CampaignAgentComponent>().AgentNavigator != null)
				{
					agent.GetComponent<CampaignAgentComponent>().AgentNavigator.GetBehaviorGroup<AlarmedBehaviorGroup>().DisableScriptedBehavior();
				}
			}
			foreach (Agent agent2 in this._opponentSideAgents)
			{
				if (agent2.IsActive() && agent2.GetComponent<CampaignAgentComponent>().AgentNavigator != null)
				{
					agent2.GetComponent<CampaignAgentComponent>().AgentNavigator.GetBehaviorGroup<AlarmedBehaviorGroup>().DisableScriptedBehavior();
				}
			}
		}

		// Token: 0x0600089E RID: 2206 RVA: 0x000463F4 File Offset: 0x000445F4
		public void EndFight()
		{
			this.ResetTeamsForFightAndDuel();
			this._state = VBGBMissionFightHandler.State.FightEnded;
			VBGBMissionFightHandler.DuelAgent = null;
			VBGBMissionFightHandler.IsThereSpectators = false;
			PlayerEncounter.LocationEncounter.DuelOpponent = null;
			foreach (Agent agent in this._playerSideAgents)
			{
				agent.TryToSheathWeaponInHand(0, (Agent.WeaponWieldActionType)3);
				agent.TryToSheathWeaponInHand((Agent.HandIndex)1, (Agent.WeaponWieldActionType)3);
			}
			foreach (Agent agent2 in this._opponentSideAgents)
			{
				agent2.TryToSheathWeaponInHand(0, (Agent.WeaponWieldActionType)3);
				agent2.TryToSheathWeaponInHand((Agent.HandIndex)1, (Agent.WeaponWieldActionType)3);
			}
			this._playerSideAgents.Clear();
			this._opponentSideAgents.Clear();
			if (Mission.Current.MainAgent != null)
			{
				Mission.Current.MainAgent.IsItemUseDisabled = false;
			}
			Mission.Current.SetMissionMode(this._oldMissionMode, false);
			if (VBGBMissionFightHandler._onFightEnd != null)
			{
				VBGBMissionFightHandler._onFightEnd(this._isPlayerSideWon);
				VBGBMissionFightHandler._onFightEnd = null;
			}
		}

		// Token: 0x0600089F RID: 2207 RVA: 0x0004651C File Offset: 0x0004471C
		public bool IsThereActiveFight()
		{
			return this._state == VBGBMissionFightHandler.State.Fighting;
		}

		// Token: 0x060008A0 RID: 2208 RVA: 0x00046528 File Offset: 0x00044728
		public void AddAgentToSide(Agent agent, bool isPlayerSide)
		{
			if (!this.IsThereActiveFight() || this._playerSideAgents.Contains(agent) || this._opponentSideAgents.Contains(agent))
			{
				return;
			}
			if (isPlayerSide)
			{
				agent.SetTeam(Mission.Current.PlayerTeam, true);
				this._playerSideAgents.Add(agent);
				return;
			}
			agent.SetTeam(Mission.Current.PlayerEnemyTeam, true);
			this._opponentSideAgents.Add(agent);
		}

		// Token: 0x060008A1 RID: 2209 RVA: 0x00046598 File Offset: 0x00044798
		public IEnumerable<Agent> GetDangerSources(Agent ownerAgent)
		{
			if (!(ownerAgent.Character is CharacterObject))
			{
				return new List<Agent>();
			}
			if (this.IsThereActiveFight() && !VBGBMissionFightHandler.IsAgentAggressive(ownerAgent) && Agent.Main != null)
			{
				return new List<Agent>
				{
					Agent.Main
				};
			}
			return new List<Agent>();
		}

		// Token: 0x060008A2 RID: 2210 RVA: 0x000465E8 File Offset: 0x000447E8
		public static bool IsAgentAggressive(Agent agent)
		{
			CharacterObject characterObject = agent.Character as CharacterObject;
			return agent.HasWeapon() || (characterObject != null && (characterObject.Occupation == Occupation.Mercenary || VBGBMissionFightHandler.IsAgentVillian(characterObject) || VBGBMissionFightHandler.IsAgentJusticeWarrior(characterObject)));
		}

		// Token: 0x060008A3 RID: 2211 RVA: 0x00046629 File Offset: 0x00044829
		public static bool IsAgentJusticeWarrior(CharacterObject character)
		{
			return character.Occupation == Occupation.Soldier || character.Occupation == Occupation.Guard || character.Occupation == Occupation.PrisonGuard;
		}

		// Token: 0x060008A4 RID: 2212 RVA: 0x0004664A File Offset: 0x0004484A
		public static bool IsAgentVillian(CharacterObject character)
		{
			return character.Occupation == Occupation.Gangster || character.Occupation == Occupation.GangLeader || character.Occupation == Occupation.Bandit || character.Occupation == Occupation.Outlaw;
		}

		// Token: 0x040003BF RID: 959
		public static Agent DuelAgent;

		// Token: 0x040003C0 RID: 960
		public static bool IsThereSpectators;

		// Token: 0x040003C1 RID: 961
		private static VBGBMissionFightHandler.OnFightEndDelegate _onFightEnd;

		// Token: 0x040003C2 RID: 962
		private MissionAgentHandler _missionAgentHandler;

		// Token: 0x040003C3 RID: 963
		private List<Agent> _playerSideAgents;

		// Token: 0x040003C4 RID: 964
		private List<Agent> _opponentSideAgents;

		// Token: 0x040003C5 RID: 965
		private Dictionary<Agent, Team> _playerSideAgentsOldTeamData;

		// Token: 0x040003C6 RID: 966
		private Dictionary<Agent, Team> _opponentSideAgentsOldTeamData;

		// Token: 0x040003C7 RID: 967
		private List<LocationCharacter> _playerSideReinforcements;

		// Token: 0x040003C8 RID: 968
		private List<LocationCharacter> _opponentSideReinforcements;

		// Token: 0x040003C9 RID: 969
		private int _playerSideInitialAgentCount;

		// Token: 0x040003CA RID: 970
		private int _opponentSideInitialAgentCount;

		// Token: 0x040003CB RID: 971
		private VBGBMissionFightHandler.State _state;

		// Token: 0x040003CC RID: 972
		private BasicTimer _finishTimer;

		// Token: 0x040003CD RID: 973
		private BasicTimer _prepareTimer;

		// Token: 0x040003CE RID: 974
		private bool _isPlayerSideWon;

		// Token: 0x040003CF RID: 975
		private List<MatrixFrame> _playerReinforcementSpawnFrames;

		// Token: 0x040003D0 RID: 976
		private List<MatrixFrame> _opponentReinforcementSpawnFrames;

		// Token: 0x040003D1 RID: 977
		private MissionMode _oldMissionMode;

		// Token: 0x040003D2 RID: 978
		private bool _spawnReinforcements;

		// Token: 0x02000187 RID: 391
		private enum State
		{
			// Token: 0x0400070A RID: 1802
			NoFight,
			// Token: 0x0400070B RID: 1803
			Fighting,
			// Token: 0x0400070C RID: 1804
			FightEnded
		}

		// Token: 0x02000188 RID: 392
		// (Invoke) Token: 0x06000F21 RID: 3873
		public delegate void OnFightEndDelegate(bool isPlayerSideWon);
	}
}
