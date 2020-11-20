using System;
using System.Collections.Generic;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace VillageBoyGoesBad
{
	public class VBGBMissionFightHandler : MissionLogic
	{
		public VBGBMissionFightHandler(Action<Agent, int> agentHitAction)
		{
			this.OnAgentHitAction = agentHitAction;
		}

		public override void OnAgentHit(Agent affectedAgent, Agent affectorAgent, int damage, in MissionWeapon affectorWeapon)
		{
			Action<Agent, int> onAgentHitAction = this.OnAgentHitAction;
			if(OnAgentHitAction == null)
			{
				return;
			}
			OnAgentHitAction(affectedAgent, damage);
		}

		private Action<Agent, int> OnAgentHitAction;
	}
}
