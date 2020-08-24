using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace GatherInformation
{
    class GatherInformationBarterable : Barterable
    {
        public GatherInformationBarterable(Hero originalOwner, PartyBase ownerParty, GatherInformationBehavior.GatherInformationIssueQuest quest) : base(originalOwner, ownerParty)
        {
            this._quest = quest;
        }

        public override string StringID
        {
            get
            {
                return "gather_information";
            }
        }

        public override TextObject Name
        {
            get
            {
                TextObject result = new TextObject("Test Barterable");
                return result;
            }
        }

        public override void Apply()
        {
            _quest.BarterSuccess();
        }

        public override int GetUnitValueForFaction(IFaction faction)
        {
            int result = 1000;
            return result;
        }

        public override ImageIdentifier GetVisualIdentifier()
        {
            return null;
        }

        private readonly GatherInformationBehavior.GatherInformationIssueQuest _quest;
    }
}
