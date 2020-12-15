using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SandBox.Source.Missions;
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
using TaleWorlds.ObjectSystem;
using TaleWorlds.Library;

namespace LordBountyHunting
{
    class pbSuspect
    {
        public enum PersonalityType
        {
            Dismissive = 0,
            Scared = 1,
            Aggresive = 2
        }
        public enum Background
        {
            Wanderer = 0,
            Trader = 1,
            Mercanary = 2
        }

        internal struct pbSuspectProperties
        {
            public pbSuspectProperties(pbSuspect targetSuspect = null)
            {
                if (targetSuspect == null)
                {
                    this.HasFacialHair = MBRandom.Random.Next(0, 2) == 1;
                    this.HasFacialHair = MBRandom.Random.Next(0, 2) == 1;

                    this.HasScar = MBRandom.Random.Next(0, 2) == 1;

                    this.isFemale = MBRandom.Random.Next(0, 3) == 1;

                    this.isBald = MBRandom.Random.Next(0, 2) == 1;
                    this.personality = new List<PersonalityType>() { PersonalityType.Aggresive, PersonalityType.Dismissive, PersonalityType.Scared }.GetRandomElement();
                    this.background = new List<Background>() { Background.Wanderer, Background.Trader, Background.Mercanary }.GetRandomElement();
                }
                else
                {
                    do
                    {
                        this.HasFacialHair = MBRandom.Random.Next(0, 2) == 1;
                        this.HasFacialHair = MBRandom.Random.Next(0, 2) == 1;

                        this.HasScar = MBRandom.Random.Next(0, 2) == 1;

                        this.isFemale = MBRandom.Random.Next(0, 2) == 1;

                        this.isBald = MBRandom.Random.Next(0, 2) == 1;
                        this.personality = new List<PersonalityType>() { PersonalityType.Aggresive, PersonalityType.Dismissive, PersonalityType.Scared }.GetRandomElement();
                        this.background = new List<Background>() { Background.Wanderer, Background.Trader, Background.Mercanary }.GetRandomElement();
                    } while (targetSuspect.Properties.HasFacialHair == this.HasFacialHair &&
                            targetSuspect.Properties.HasScar == this.HasScar &&
                            targetSuspect.Properties.isFemale == this.isFemale &&
                            targetSuspect.Properties.isBald == this.isBald && 
                            targetSuspect.Properties.personality == this.personality &&
                            targetSuspect.Properties.background == this.background);
                }


            }

            [SaveableField(10)]
            public bool HasFacialHair;

            public readonly bool HasScar;

            public readonly bool isFemale;

            public readonly bool isBald;

            [SaveableField(120)]
            public readonly PersonalityType personality;

            [SaveableField(130)]
            public readonly Background background;
        }

        List<String> introduction = new List<string>()
        {
            "Hello stranger"
        };

        List<String> greeting = new List<string>()
        {
            "Hello again"
        };

        List<TextObject> wandererBackgrounds = new List<TextObject>()
        {
            new TextObject("I'm a Wanderer"),
            new TextObject("Oh, I be wandering around!")
        };

        List<TextObject> traderBackgrounds = new List<TextObject>()
        {
            new TextObject("I'm a Trader"),
            new TextObject("I like to exchange goods, if ya know what I mean")
        };

        List<TextObject> mercBackgrounds = new List<TextObject>()
        {
            new TextObject("I'm a mercanary"),
            new TextObject("I'm what you may call a sellsword.")
        };

        public pbSuspect(LordBountyHuntingBehavior.LordBountyHuntingQuest quest, pbSuspect targetSuspect = null)
        {
            this.CurrentQuest = quest;

            this.Properties = new pbSuspectProperties(targetSuspect);            
            if(targetSuspect == null)
            {
                this.HeroObject = HeroCreator.CreateSpecialHero((from charO in CharacterObject.All
                                                                 where
                           charO.Culture == this.CurrentQuest.QuestGiver.Culture &&
                           charO.IsFemale == this.Properties.isFemale 
                           //charO.Occupation == Occupation.
                           //charO.IsBasicTroop &&
                           //charO.Tier == 5
                           
                                                                 select charO).GetRandomElement<CharacterObject>());
            }
            else
            {
                this.HeroObject = HeroCreator.CreateSpecialHero((from charO in CharacterObject.All
                                                                 where
                           charO.Culture == this.CurrentQuest.QuestGiver.Culture &&
                           charO.IsFemale == this.Properties.isFemale                           
                                                                 select charO).GetRandomElement<CharacterObject>());
            }
            
            this.Name = NameGenerator.Current.GenerateHeroFirstName(this.HeroObject, false);
            this.personality = new List<PersonalityType>() {PersonalityType.Aggresive, PersonalityType.Dismissive, PersonalityType.Scared }.GetRandomElement(); 
            this.background = new List<Background>() { Background.Wanderer, Background.Trader, Background.Mercanary }.GetRandomElement();
            this.introductionDone = false;
            this.backgroundDialog = SetBackGroundDialog(this.background);
        }

        public void CompleteIntroduction()
        {
            this.HeroObject.Name = this.Name;
            this.introductionDone = true;

        }

        private TextObject SetBackGroundDialog(Background background)
        {
            TextObject resultText = new TextObject("test test, background dialog.");

            switch (background) 
            {
                case Background.Mercanary:
                    resultText = mercBackgrounds.GetRandomElement();
                    break;
                case Background.Trader:
                    resultText = traderBackgrounds.GetRandomElement();
                    break;
                case Background.Wanderer:
                    resultText = wandererBackgrounds.GetRandomElement();
                    break;
            }


            return resultText;
        }

        [SaveableField(10)]
        public readonly LordBountyHuntingBehavior.LordBountyHuntingQuest CurrentQuest;

        [SaveableField(20)]
        public readonly Hero HeroObject;

        [SaveableField(30)]
        public readonly PersonalityType personality;

        [SaveableField(40)]
        public readonly Background background;

        [SaveableField(50)]
        public readonly TextObject backgroundDialog;

        [SaveableField(60)]
        public bool introductionDone;

        [SaveableField(70)]
        public TextObject Name;

        [SaveableField(80)]
        public LordBountyHuntingBehavior.TargetsCrime Crime;

        [SaveableField(90)]
        public pbSuspectProperties Properties;
    }
}