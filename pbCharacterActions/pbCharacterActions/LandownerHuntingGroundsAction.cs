using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace pbCharacterActions
{
    class LandownerHuntingGroundsAction : INotableAction
    {
        public static void ExecuteAction(Hero notable, Settlement village)
        {
            Campaign.Current.GameMenuManager.SetNextMenu("pb_LandOwner_HuntingGrounds_GameMenu");
            InformationManager.DisplayMessage(new InformationMessage("Landowner - Hunting Grounds: Action fired."));
        }
    }
}
