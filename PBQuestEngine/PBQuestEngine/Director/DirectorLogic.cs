using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBQuestEngine.Director
{
    class DirectorLogic
    {

        public void AddSeigeCompletedEventLog(String eventLog)
        {
            _SiegeCompletedEventLog.Add(eventLog);
        }

        private static List<String> _SiegeCompletedEventLog;
    }
}
