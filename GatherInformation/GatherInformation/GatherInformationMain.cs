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

namespace GatherInformation
{
    public class GatherInformationMain : MBSubModuleBase
    {
        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {

            //need to pass gamestarter object to 
            if (game.GameType is Campaign)
            {
                try
                {
                    CampaignGameStarter gameStarter = (CampaignGameStarter)gameStarterObject;
                    gameStarter.AddBehavior(new GatherInformationBehavior());
                }
                catch (Exception e)
                {
                    MessageBox.Show("uh oh.." + e.ToString());
                }
            }
        }
    }
}
