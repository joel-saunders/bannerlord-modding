using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Overlay;
using TaleWorlds.CampaignSystem.SandBox;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;
using System.Runtime.Remoting.Messaging;

namespace PBQuestEngine.Director
{
    class DirectorQuest : QuestBase
    {
        public DirectorQuest(String questId, Hero questGiver, CampaignTime duration, int baseReward, IPresentationBlock presentationBlock, IQuestElement[] questElements) : base(questId, questGiver, duration, baseReward)
        {
        }

        public override TextObject Title => new TextObject("Director Test Quest");

        public override bool IsRemainingTimeHidden => false;

        protected override void InitializeQuestOnGameLoad()
        {
            this.SetDialogs();
        }

        protected override void SetDialogs()
        {
            
        }
    }
}
