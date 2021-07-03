using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.CampaignSystem;
using PBQuestEngine.Director;

namespace PBQuestEngine
{
    public class Main : MBSubModuleBase
    {

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            if (game.GameType is Campaign)
            {
                try
                {
                    CampaignGameStarter gameStarter = (CampaignGameStarter)gameStarterObject;
                    gameStarter.AddBehavior(new DirectorCampaignBehavior()); //can this be simplified?
                    InitLogs();
                }
                catch (Exception e)
                {
                    MessageBox.Show("Something went wrong when trying to add the Campaign behavior to the game starter: " + e.ToString());
                }
            }
        }

        private void InitLogs()
        {
            if (DirectorCampaignLogs._SiegeCompletedEventLog == null) { DirectorCampaignLogs._SiegeCompletedEventLog = new List<DirectorCampaignLogs.SiegeCompletedEvent>(); };
            if (DirectorCampaignLogs._SiegeCompletedEventLog == null) { DirectorCampaignLogs.VillageLootedEventLogs = new List<DirectorCampaignLogs.VillageLootedEventLog>(); };
        }
    }
}
