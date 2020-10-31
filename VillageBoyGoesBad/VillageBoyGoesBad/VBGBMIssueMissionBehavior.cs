using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.MountAndBlade;

namespace VillageBoyGoesBad
{
    class VBGBMIssueMissionBehavior : MissionLogic
    {
        public VBGBMIssueMissionBehavior(Agent sonAgent, List<Agent> gangAgents)
        {

        }

        public override void OnAgentHit(Agent affectedAgent, Agent affectorAgent, int damage, in MissionWeapon affectorWeapon)
        {
            base.OnAgentHit(affectedAgent, affectorAgent, damage, affectorWeapon);
        }


    }
}
