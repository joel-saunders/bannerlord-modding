using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using SandBox.Source.Missions.Handlers;
using TaleWorlds.MountAndBlade;
using TaleWorlds.CampaignSystem;
using SandBox;
//using SandBox.Quests;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.CharacterDevelopment.Managers;
using TaleWorlds.CampaignSystem.Conversation.Persuasion;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.CampaignSystem.Actions;
using Helpers;
using TaleWorlds.CampaignSystem.Overlay;
//using System.Windows.Forms;
using TaleWorlds.TwoDimension;
using TaleWorlds.CampaignSystem.SandBox;
using TaleWorlds.Diamond.AccessProvider.Test;
using TaleWorlds.SaveSystem;
using TaleWorlds.CampaignSystem.SandBox.Issues;
using NetworkMessages.FromServer;

namespace radiantQuestTemplate
{
     class RQTQuest : QuestBase //1-Update class name
    {
        public RQTQuest(string questId, Hero questGiver, CampaignTime duration, int rewardGold) : base(questId, questGiver, duration, rewardGold) //1-Update class name
        {
            //init Quest vars, such as 'PlayerhastalkedwithX', 'DidPlayerFindY'
            this.SetDialogs();
            this.InitializeQuestOnCreation();
            base.AddLog(new TextObject("The quest has begun!!! woooo!")); //4-Update Quest naming
        }

        // Required overrides (abstract)
        public override TextObject Title => new TextObject("Quest Title"); //4-Update Quest naming

        public override bool IsRemainingTimeHidden => false;

        protected override void InitializeQuestOnGameLoad()
        {
            this.SetDialogs();
        }
        //there are a couple DialogFlows QuestBase has that you'll want to set here. In addition, whatever other dialog flows you have should also
        //be called here. Have them in separate methods for simplicity.
        protected override void SetDialogs()
        {
            this.OfferDialogFlow = DialogFlow.CreateDialogFlow("issue_classic_quest_start", 100). //3-Update quest acceptance text
                NpcLine("TEMPLATE: Good, I'm glad you've agreed to the quest. Good luck!").
                    Condition(() => Hero.OneToOneConversationHero == this.QuestGiver).
                    Consequence(QuestAcceptedConsequences).CloseDialog();
            this.DiscussDialogFlow = DialogFlow.CreateDialogFlow("quest_discuss", 100). //3-Update quest acceptance text
                NpcLine("TEMPLATE: Why are you here? Shouldn't you be questing?").
                    Condition(() => Hero.OneToOneConversationHero == this.QuestGiver);
            //Campaign.Current.ConversationManager.AddDialogFlow(dialogflowmethod);
        }
        // </Required overrides

        // Optional Overrides (virtual)
        protected override void RegisterEvents()
        {
            base.RegisterEvents();
            CampaignEvents.WarDeclared.AddNonSerializedListener(this, new Action<IFaction, IFaction>(this.OnWarDeclared));
            CampaignEvents.VillageBeingRaided.AddNonSerializedListener(this, new Action<Village>(this.OnVillageRaid));
        }

        private void OnWarDeclared(IFaction faction1, IFaction faction2)
        {
            if (Hero.MainHero.MapFaction.IsAtWarWith(base.QuestGiver.CurrentSettlement.MapFaction))
            {
                base.AddLog(new TextObject("Due to the war, this quest can no longer be completed."));
                base.CompleteQuestWithCancel(null);
            }
        }

        private void OnVillageRaid(Village village)
        {
            if (base.QuestGiver.CurrentSettlement == village.Settlement)
            {
                base.AddLog(new TextObject(village.Name.ToString() + " is being raided. The quest can no longer be completed."));
                base.CompleteQuestWithCancel(null);
            }
        }
        public override bool IsQuestGiverHidden => false;
        public override bool IsSpecialQuest => false; //who knows :shrug emoji
        public override int GetCurrentProgress()
        {
            return base.GetCurrentProgress();
        }
        public override int GetMaxProgress()
        {
            return base.GetMaxProgress();
        }
        public override string GetPrefabName()
        {
            return base.GetPrefabName();
        }
        public override void OnCanceled()
        {
            base.OnCanceled();
        }
        public override void OnFailed()
        {
            base.OnFailed();
        }
        public override bool QuestPreconditions()
        {
            return base.QuestPreconditions();
        }
        protected override void OnBeforeTimedOut(ref bool completeWithSuccess)
        {
            base.OnBeforeTimedOut(ref completeWithSuccess);
        }
        protected override void OnBetrayal()
        {
            base.OnBetrayal();
        }
        protected override void OnCompleteWithSuccess()
        {
            //What all should happen when the quest is complete
            base.OnCompleteWithSuccess();
        }
        protected override void OnFinalize()
        {
            base.OnFinalize();
        }
        protected override void OnStartQuest()
        {
            base.OnStartQuest();
        }
        protected override void OnTimedOut()
        {
            base.OnTimedOut();
        }
        // </Optional Overrides

        // <Delegates
        private void QuestAcceptedConsequences()
        {
            base.StartQuest();
        }

        private void SuccessComplete()
        {
            base.CompleteQuestWithSuccess();
        }

        private void FailureComplete()
        {
            base.CompleteQuestWithFail();
        }
    }
}
