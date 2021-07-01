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
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.CampaignSystem.Conversation.Tags;

namespace pbCharacterActions
{
    class CharacterActionsCampaignBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(this.OnSessionLaunched));
        }

        public override void SyncData(IDataStore dataStore)
        {            
        }

        private void OnSessionLaunched(CampaignGameStarter campaign)
        {
            this.AddDialogs();

            InformationManager.DisplayMessage(new InformationMessage("addedDialogs"));

            this.AddGameMenus(campaign);
        }

        private void AddGameMenus(CampaignGameStarter campaign)
        {
            campaign.AddGameMenu("pb_LandOwner_HuntingGrounds_GameMenu", "Hunting Grounds", null);
            campaign.AddGameMenuOption("pb_LandOwner_HuntingGrounds_GameMenu", "pb_LandOwner_HuntingGrounds_StartHuntOption", "Start the hunt!", null, delegate { GameMenu.SwitchToMenu("pb_LandOwner_HuntingGrounds_WaitGameMenu"); });
            campaign.AddWaitGameMenu("pb_LandOwner_HuntingGrounds_WaitGameMenu", "You're hunting!", 
                new OnInitDelegate(this.hunting_wait_menu_init), 
                new OnConditionDelegate(this.hunting_wait_menu_condition), 
                new OnConsequenceDelegate(this.hunting_wait_menu_consequence),
                new OnTickDelegate(this.hunting_wait_menu_tick), GameMenu.MenuAndOptionType.WaitMenuShowOnlyProgressOption, GameOverlays.MenuOverlayType.None, 10f);
        }

        private void hunting_wait_menu_init(MenuCallbackArgs args)
        {
            //args.MenuContext.GameMenu.SetTargetedWaitingTimeAndInitialProgress(100f, 0f);
            this.huntingprogress = 0f;
        }

        private bool hunting_wait_menu_condition(MenuCallbackArgs args)
        {
            //args.MenuContext.GameMenu.SetTargetedWaitingTimeAndInitialProgress(100f, 0f);
            return true;
        }

        private void hunting_wait_menu_consequence(MenuCallbackArgs args)
        {
            //args.MenuContext.GameMenu.SetTargetedWaitingTimeAndInitialProgress(100f, 0f);            
        }
        private void hunting_wait_menu_tick(MenuCallbackArgs args, CampaignTime dt)
        {
            this.huntingprogress +=  .1f;
            args.MenuContext.GameMenu.SetProgressOfWaitingInMenu(this.huntingprogress / 10f);
        }

        private void AddDialogs()
        {
            Campaign.Current.ConversationManager.AddDialogFlow(this.AskNotableForAction());
            Campaign.Current.ConversationManager.AddDialogFlow(this.AskLandOwnerForHunting());
        }

        private DialogFlow AskNotableForAction()
        {
            DialogFlow dialog = DialogFlow.CreateDialogFlow("hero_main_options", 6000).
                PlayerLine("I was wondering if there was any way you could help me?").//Condition(()=> Hero.OneToOneConversationHero.IsNotable).
                NpcLine("Yes, I can do x y or z, what would you like?").GotoDialogState("pbNotable_options_for_player"); //lord_talk_ask_something_2

            InformationManager.DisplayMessage(new InformationMessage("Created dialog flow"));
            return dialog;
        }

        private DialogFlow AskLandOwnerForHunting()
        {
            DialogFlow dialog = DialogFlow.CreateDialogFlow("pbNotable_options_for_player", 100).
                PlayerLine("Can I hunt on your lands?").Condition(() => Hero.OneToOneConversationHero.IsRuralNotable).
                NpcLine("Sure, here you are!").Consequence(delegate { LandownerHuntingGroundsAction.ExecuteAction(Hero.OneToOneConversationHero, Hero.OneToOneConversationHero.CurrentSettlement); }).
                PlayerLine("Thanks!").CloseDialog();

            return dialog;
        }

        public float huntingprogress;
    }
}
