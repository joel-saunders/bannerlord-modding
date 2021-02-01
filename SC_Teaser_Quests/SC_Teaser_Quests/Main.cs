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
using HarmonyLib;
using TaleWorlds.Library;

namespace SC_Teaser_Quests
{
    public class Main : MBSubModuleBase
    {

        protected override void OnSubModuleLoad()
        {
            try
            {
                new Harmony("sc_teaser_harmony_instance").PatchAll();
            }
            catch (Exception)
            { }
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            if (game.GameType is Campaign)
            {
                try
                {
                    CampaignGameStarter gameStarter = (CampaignGameStarter)gameStarterObject;
                    gameStarter.AddBehavior(new SC_HeadmanNeedsGrainIssueBehavior());
                    gameStarter.AddBehavior(new SC_NotableWantsDaughterFoundIssueBehavior());
                    gameStarter.AddBehavior(new SC_NearbyBanditBaseIssueBehavior());

                    gameStarter.LoadGameTexts(BasePath.Name+"Modules/SC_Teaser_Quests/ModuleData/quest_dialog_strings.xml");
                    gameStarter.LoadGameTexts(BasePath.Name + "Modules/SC_Teaser_Quests/ModuleData/quest_dialog_strings_HeadmanNeedsGrain.xml");
                }
                catch (Exception e)
                {
                    MessageBox.Show("Something went wrong when trying to add the Campaign behavior to the game starter: " + e.ToString());
                }
            }
        }
    }
}
