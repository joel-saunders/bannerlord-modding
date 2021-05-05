using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem.LogEntries;
using TaleWorlds.CampaignSystem.SandBox;
using PBQuestEngine;

namespace PBQuestEngine.Blocks
{
    class CampaignMapTravelBlock : IGameplayBlock, IQuestElement
    {
        public CampaignMapTravelBlock()
        {

        }

        public JournalLog startLog;
        public JournalLog endLog;
    }
}
