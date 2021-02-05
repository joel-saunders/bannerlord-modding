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

namespace PB_bounty_hunting
{
    class pbTraveller
    {
        List<Occupation> Occupations = new List<Occupation> { Occupation.Guard, Occupation.Mercenary, Occupation.Merchant};

        //Clue triggered
        public pbTraveller(BountyHuntingCampaignBehavior.BountyHuntingQuest quest, Clue cluetrigger, Intel intel)
        {
            this._quest = quest;
            this.clueTrigger = cluetrigger;
            this.intelKnown = intel;
            this.travellerOccupation = Occupations.GetRandomElement<Occupation>();

            this.GenerateClueAndIntelDialog();

            //if (quest._intelOnTarget.Count > 0)
            //{
            //    this.intelKnown = quest._intelOnTarget.GetRandomElement<Intel>();
            //    quest._intelOnTarget.Remove(this.intelKnown);
            //}

            this.travellerHero = HeroCreator.CreateSpecialHero(CharacterObject.All.Where((CharacterObject charO) =>
                                                                                            charO.Occupation == Occupation.Wanderer
                                                                                            ).GetRandomElement<CharacterObject>());
            this.travellerHero.Name = new TextObject(this.travellerHero.FirstName + ", traveller");

            this.cluesAskedtoTraveller = new List<Clue>();
            this.intelAskedtoTraveller = new List<Intel>();
        }

        //Intel triggered
        public pbTraveller(BountyHuntingCampaignBehavior.BountyHuntingQuest quest, Intel inteltrigger, Intel intel)
        {
            this._quest = quest;
            this.intelTrigger = inteltrigger;
            this.intelKnown = intel;
            this.travellerOccupation = Occupations.GetRandomElement<Occupation>();

            this.GenerateClueAndIntelDialog();

            //if (quest._intelOnTarget.Count > 0)
            //{
            //    this.intelKnown = quest._intelOnTarget.GetRandomElement<Intel>();
            //    quest._intelOnTarget.Remove(this.intelKnown);
            //}

            this.travellerHero = HeroCreator.CreateSpecialHero(CharacterObject.All.Where((CharacterObject charO) =>
                                                                                            charO.Occupation == Occupation.Wanderer
                                                                                            ).GetRandomElement<CharacterObject>());
            this.travellerHero.Name = new TextObject(this.travellerHero.FirstName + ", traveller");

            this.cluesAskedtoTraveller = new List<Clue>();
            this.intelAskedtoTraveller = new List<Intel>();
        }


        //They know nothing!
        public pbTraveller(BountyHuntingCampaignBehavior.BountyHuntingQuest quest)
        {
            this.travellerHero = HeroCreator.CreateSpecialHero(CharacterObject.All.Where((CharacterObject charO) =>
                                                                                            charO.Occupation == Occupation.Wanderer
                                                                                            ).GetRandomElement<CharacterObject>());

            this.travellerHero.Name = new TextObject(this.travellerHero.FirstName + ", traveller");

            this.cluesAskedtoTraveller = new List<Clue>();
            this.intelAskedtoTraveller = new List<Intel>();

        }

        private void GenerateClueAndIntelDialog()
        {
            if (this.clueTrigger != null)
            {
                switch (this.clueTrigger.Type)
                {
                    case Clue.ClueType.Culture:
                        this.clueDialog = "Oh yea, I remeber seeing them. They were " + this._quest.Target.culture.ToString() + ".";
                        break;
                    case Clue.ClueType.Location:
                        this.clueDialog = "Oh yea, I remeber seeing them. They had come from the direction of" + this._quest.QuestGiver.CurrentSettlement.ToString() + ".";
                        break;
                    case Clue.ClueType.TimeofDay:
                        this.clueDialog = "Oh yea, I remeber seeing them. It was " + this._quest.Target.timeOfDayAvailable.ToString() + ".";
                        break;
                }
            }
            else if(this.intelTrigger != null)
            {
                switch (this.intelTrigger.Type)
                {
                    case Intel.IntelType.Alibi:
                        this.clueDialog = "Oh yea, I remeber seeing them. They were " + this._quest.Target.culture.ToString() + ".";
                        break;
                    case Intel.IntelType.Background:
                        this.clueDialog = "Oh yea, I remeber seeing them. They had come from the direction of" + this._quest.QuestGiver.CurrentSettlement.ToString() + ".";
                        break;
                    case Intel.IntelType.Location :
                        this.clueDialog = "Oh yea, I remeber seeing them. It was " + this._quest.Target.timeOfDayAvailable.ToString() + ".";
                        break;
                }
            }
        }

        [SaveableField(10)]
        public Hero travellerHero;

        [SaveableField(20)]
        public Clue clueTrigger;

        [SaveableField(30)]
        public Intel intelKnown;

        [SaveableField(40)]
        public List<Clue> cluesAskedtoTraveller;

        [SaveableField(50)]
        public Occupation travellerOccupation;

        [SaveableField(60)]
        public string clueDialog;

        [SaveableField(70)]
        private BountyHuntingCampaignBehavior.BountyHuntingQuest _quest;

        [SaveableField(80)]
        public Intel intelTrigger;

        [SaveableField(90)]
        public List<Intel> intelAskedtoTraveller;

        [SaveableField(100)]
        public bool intelDiscoveredByPlayer;
    }
}
