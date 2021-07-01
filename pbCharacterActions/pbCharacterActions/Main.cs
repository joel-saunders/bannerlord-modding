﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.CampaignSystem;
using SandBox;

namespace pbCharacterActions
{
    public class Main : MBSubModuleBase
    {

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            if (true) //game.GameType is Campaign
            { 
                try
                {
                    CampaignGameStarter gameStarter = (CampaignGameStarter)gameStarterObject;
                    gameStarter.AddBehavior(new CharacterActionsCampaignBehavior()); //can this be simplified?
                }
                catch (Exception e)
                {
                    MessageBox.Show("Something went wrong when trying to add the Campaign behavior to the game starter: " + e.ToString());
                }
            }
        }
    }
}
