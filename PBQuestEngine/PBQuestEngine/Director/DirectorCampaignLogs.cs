using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.Core;

using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Overlay;
using TaleWorlds.CampaignSystem.SandBox;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;

namespace PBQuestEngine.Director
{
    class DirectorCampaignLogs
    {
        public struct SiegeCompletedEvent
        {
            public SiegeCompletedEvent(Settlement seigedSettlement, MobileParty seigingParty, bool bool1, bool bool2)
            {
                this._seigedSettlement = seigedSettlement;
                this._seigingParty = seigingParty;
                this._bool1 = bool1;
                this._bool2 = bool2;
            }
            public Settlement _seigedSettlement;
            public MobileParty _seigingParty;
            public bool _bool1;
            public bool _bool2;
        }
        public static void AddSeigeCompletedEventLog(SiegeCompletedEvent eventLog)
        {
            _SiegeCompletedEventLog = new List<SiegeCompletedEvent>();
            _SiegeCompletedEventLog.Add(eventLog);
            //new DirectorQuest("3721089", )
            DirectorQuest quest = new DirectorQuest("74832910", eventLog._seigingParty.LeaderHero, CampaignTime.DaysFromNow(20f), 1000, null, null);
            quest.StartQuest();
        }

        private static List<SiegeCompletedEvent> _SiegeCompletedEventLog;
    }
}
