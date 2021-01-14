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

        internal struct TravellerDialog
        {           
            

            public TextObject dialogBackground;
            public TextObject dialogPlayerDescription;
            public TextObject dialogHeadmanAffirmation;
            public TextObject dialogHeadmanUnknown;
        }

        List<TravellerDialog> wandererDialogs = new List<TravellerDialog>()
        {
            new TravellerDialog{ dialogBackground = new TextObject("Oh, I've been wandering around these parts for years. Just doing the odd job here or there."),
                        dialogPlayerDescription = new TextObject("They said they've been around here for a while, do you know them?"),
                        dialogHeadmanAffirmation = new TextObject("Oh yea, I've known them for years. You can trust them. "),
                        dialogHeadmanUnknown = new TextObject("hmmm, I don't even recall ever seeing the. I wouldn't trust them.")},
            new TravellerDialog{ dialogBackground = new TextObject("Just visiting. I grew up in this side of the country and somehow I never left. There's always work that needs doing from somebody."),
                        dialogPlayerDescription = new TextObject("They said they grew up around this area and has been in and out all their life."),
                        dialogHeadmanAffirmation = new TextObject("Well that's because they have. you can trust them."),
                        dialogHeadmanUnknown = new TextObject("Well... I think they may be lying. I've never seen them until recently.")}            
        };

        List<TravellerDialog> traderDialogs = new List<TravellerDialog>()
        {
            new TravellerDialog{ dialogBackground = new TextObject("Well I'm here for the to inspect the materials being produced here. I'm a trader, you see."),
                        dialogPlayerDescription = new TextObject("They said they're a trader, here to inspect materials"),
                        dialogHeadmanAffirmation = new TextObject("Oh yes, blank has been giving our people a hard time for their product. You can trust them."),
                        dialogHeadmanUnknown = new TextObject("That would be news to me, I don't know them.")},
            new TravellerDialog{ dialogBackground = new TextObject("Looking for a good deal. That's about it. I'm just a merchant."),
                        dialogPlayerDescription = new TextObject("They claim to be a merchant"),
                        dialogHeadmanAffirmation = new TextObject("Well that's because they are. You can believe them."),
                        dialogHeadmanUnknown = new TextObject("I couldn't tell you. I don't know them.")}
        };

        List<TravellerDialog> mercDialogs = new List<TravellerDialog>()
        {
            new TravellerDialog{ dialogBackground = new TextObject("I'm just a sellsword, friend. Nothing more, but certainly not anything less."),
                        dialogPlayerDescription = new TextObject("They mentioned they were a mercanary. Does that sound right to you."),
                        dialogHeadmanAffirmation = new TextObject("Based off of some conversations I've heard, yes. They are indeed a sellsword."),
                        dialogHeadmanUnknown = new TextObject("I couldn't tell you. All I know is that they are armed.")},
            new TravellerDialog{ dialogBackground = new TextObject("Let's just say I'm looking for a job. Specifically protection. Or, some muscle, for the right price."),
                        dialogPlayerDescription = new TextObject("They claim to be a mercanary. Can you validate that?"),
                        dialogHeadmanAffirmation = new TextObject("Yes, infact I've used their services before. They're telling the truth."),
                        dialogHeadmanUnknown = new TextObject("I'm not sure. I don't know them.")}
        };
        
        internal struct pbSuspectProperties
        {
            public pbSuspectProperties(LordBountyHuntingBehavior.LordBountyHuntingQuest quest, pbSuspect targetSuspect = null)
            {
                if (targetSuspect == null)
                {
                    if(MBRandom.Random.Next(0, 4) == 1)
                    {
                        this.culture = Campaign.Current.Kingdoms.GetRandomElement().Culture;
                    }
                    else
                    {
                        this.culture = quest.QuestGiver.Culture;
                    }
                    this.culture = Campaign.Current.Kingdoms.GetRandomElement().Culture;
                    this.background = new List<Background>() { Background.Wanderer, Background.Trader, Background.Mercanary }.GetRandomElement();
                    this.HasFacialHair = MBRandom.Random.Next(0, 2) == 1;
                    this.HasFacialHair = MBRandom.Random.Next(0, 2) == 1;

                    this.HasScar = MBRandom.Random.Next(0, 2) == 1;

                    this.isFemale = quest._questTargetCrime != LordBountyHuntingBehavior.TargetsCrime.Deserter && MBRandom.Random.Next(0, 3) == 1;

                    //this.isBald = MBRandom.Random.Next(0, 2) == 1;
                    this.personality = new List<PersonalityType>() { PersonalityType.Aggresive, PersonalityType.Dismissive, PersonalityType.Scared }.GetRandomElement();                    
                }
                else
                {
                    do
                    {
                        if (MBRandom.Random.Next(0, 4) == 1)
                        {
                            this.culture = Campaign.Current.Kingdoms.GetRandomElement().Culture;
                        }
                        else
                        {
                            this.culture = quest.QuestGiver.Culture;
                        }
                        this.HasFacialHair = MBRandom.Random.Next(0, 2) == 1;
                        this.HasFacialHair = MBRandom.Random.Next(0, 2) == 1;

                        this.HasScar = MBRandom.Random.Next(0, 2) == 1;

                        this.isFemale = quest._questTargetCrime != LordBountyHuntingBehavior.TargetsCrime.Deserter && MBRandom.Random.Next(0, 3) == 1;

                        //this.isBald = MBRandom.Random.Next(0, 2) == 1;
                        this.personality = new List<PersonalityType>() { PersonalityType.Aggresive, PersonalityType.Dismissive, PersonalityType.Scared }.GetRandomElement();
                        this.background = new List<Background>() { Background.Wanderer, Background.Trader, Background.Mercanary }.GetRandomElement();
                    } while (targetSuspect.Properties.HasFacialHair == this.HasFacialHair &&
                            targetSuspect.Properties.HasScar == this.HasScar &&
                            targetSuspect.Properties.isFemale == this.isFemale &&
                            //targetSuspect.Properties.isBald == this.isBald && 
                            targetSuspect.Properties.culture == this.culture &&
                            targetSuspect.Properties.personality == this.personality &&
                            targetSuspect.Properties.background == this.background);
                }
            }

            [SaveableField(10)]
            public bool HasFacialHair;

            public readonly bool HasScar;

            public readonly bool isFemale;

            public readonly CultureObject culture;

            //public readonly bool isBald;

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

        public pbSuspect(LordBountyHuntingBehavior.LordBountyHuntingQuest quest, pbSuspect targetSuspect = null)
        {
            this.CurrentQuest = quest;

            this.Properties = new pbSuspectProperties(quest, targetSuspect);            
            //The target
            if(targetSuspect == null)
            {
                switch (this.Properties.background)
                {
                    case Background.Mercanary:
                        this.CharObject = HeroCreator.CreateSpecialHero( CharacterObject.All.Where((CharacterObject charO) =>
                                                            !charO.IsHero &&
                                                            charO.Culture == this.Properties.culture &&
                                                            (charO.Occupation == Occupation.Mercenary || charO.IsBasicTroop)
                                                            ).GetRandomElement<CharacterObject>());
                        this.dialogs = mercDialogs.GetRandomElement();
                        break;
                    case Background.Trader:
                        this.CharObject = HeroCreator.CreateSpecialHero(CharacterObject.All.Where((CharacterObject charO) =>
                                                            !charO.IsHero &&
                                                            charO.Culture == this.Properties.culture &&
                                                            (charO.Occupation == Occupation.Merchant || charO.Occupation == Occupation.ShopKeeper)
                                                            ).GetRandomElement<CharacterObject>());
                        this.dialogs = traderDialogs.GetRandomElement();
                        break;
                    case Background.Wanderer:
                        this.CharObject = HeroCreator.CreateSpecialHero(CharacterObject.All.Where((CharacterObject charO) =>
                                                            !charO.IsHero &&
                                                            charO.TattooTags != null &&
                                                            charO.Culture == this.Properties.culture &&
                                                            (charO.Occupation == Occupation.Wanderer || charO.Occupation == Occupation.Gangster)
                                                            ).GetRandomElement<CharacterObject>());
                        this.dialogs = wandererDialogs.GetRandomElement();
                        break;
                }

                //this.HeroObject = HeroCreator.CreateSpecialHero((from charO in CharacterObject.All
                //                                                 where
                //           charO.Culture == this.CurrentQuest.QuestGiver.Culture &&
                //           charO.IsFemale == this.Properties.isFemale 
                //           //charO.Occupation == Occupation.
                //           //charO.IsBasicTroop &&
                //           //charO.Tier == 5

                //                                                 select charO).GetRandomElement<CharacterObject>());
            }
            //Other travellers
            else
            {
                switch(this.Properties.background)
                {
                    case Background.Mercanary:
                        this.CharObject = HeroCreator.CreateSpecialHero(CharacterObject.All.Where((CharacterObject charO) =>
                                                            !charO.IsHero &&
                                                            charO.Culture == this.Properties.culture &&
                                                            (charO.Occupation == Occupation.Mercenary || charO.IsBasicTroop)).GetRandomElement<CharacterObject>());
                        this.dialogs = mercDialogs.GetRandomElement();
                        break;
                    case Background.Trader:
                        this.CharObject = HeroCreator.CreateSpecialHero(CharacterObject.All.Where((CharacterObject charO) =>
                                                            !charO.IsHero &&
                                                            charO.Culture == this.Properties.culture &&
                                                            charO.Occupation == Occupation.Merchant).GetRandomElement<CharacterObject>());
                        this.dialogs = traderDialogs.GetRandomElement();
                        break;
                    case Background.Wanderer:
                        this.CharObject = HeroCreator.CreateSpecialHero(CharacterObject.All.Where((CharacterObject charO) =>
                                                            !charO.IsHero &&
                                                            charO.Culture == this.Properties.culture &&
                                                            charO.Occupation == Occupation.Wanderer).GetRandomElement<CharacterObject>());
                        this.dialogs = wandererDialogs.GetRandomElement();
                        break;
                }
                
            }
            Hero tempHero = HeroCreator.CreateSpecialHero((from charO in CharacterObject.All
                                                                 where
                                                            charO.IsHero &&
                                                            charO.Culture == this.Properties.culture select charO).GetRandomElement<CharacterObject>());

            

            this.Name = NameGenerator.Current.GenerateHeroFirstName( tempHero, false);            
            //this.background = new List<Background>() { Background.Wanderer, Background.Trader, Background.Mercanary }.GetRandomElement();
            this.introductionDone = false;            
        }

        public void CompleteIntroduction()
        {
            this.CharObject.Name = this.Name;
            this.introductionDone = true;

        }

        [SaveableField(10)]
        public readonly LordBountyHuntingBehavior.LordBountyHuntingQuest CurrentQuest;

        [SaveableField(30)]
        public readonly PersonalityType personality;
        
        [SaveableField(60)]
        public bool introductionDone;

        [SaveableField(70)]
        public TextObject Name;

        [SaveableField(80)]
        public LordBountyHuntingBehavior.TargetsCrime Crime;

        [SaveableField(90)]
        public pbSuspectProperties Properties;

        [SaveableField(100)]
        public Hero CharObject;

        [SaveableField(110)]
        public bool knownToHeadman;

        [SaveableField(120)]
        public TravellerDialog dialogs;

        //[SaveableField(130)]
        //public bool knowsOtherTraveller;

        [SaveableField(140)]
        public String dialogId;

        [SaveableField(150)]
        public pbSuspect knowsThisTraveller;

        [SaveableField(160)]
        public bool knownToTraveller;
    }
}