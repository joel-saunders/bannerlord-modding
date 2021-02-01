using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.SaveSystem;

namespace PB_bounty_hunting
{
    class pbTraveller
    {
        List<Occupation> Occupations = new List<Occupation> { Occupation.Guard, Occupation.Mercenary, Occupation.Merchant};

        public pbTraveller(BountyHuntingCampaignBehavior.BountyHuntingQuest quest, BountyHuntingCampaignBehavior.BountyHuntingQuest.Clue clue)
        {
            this.clueTrigger = clue;
            if (quest._intelOnTarget.Count > 0)
            {
                this.intelKnown = quest._intelOnTarget.GetRandomElement<BountyHuntingCampaignBehavior.BountyHuntingQuest.Intel>();
                quest._intelOnTarget.Remove(this.intelKnown);
            }

            this.travellerHero = HeroCreator.CreateSpecialHero(CharacterObject.All.Where((CharacterObject charO) =>
                                                                                            charO.Occupation == Occupation.Wanderer
                                                                                            ).GetRandomElement<CharacterObject>());

            this.cluesAskedtoTraveller = new List<BountyHuntingCampaignBehavior.BountyHuntingQuest.Clue>();
        }

        public pbTraveller(BountyHuntingCampaignBehavior.BountyHuntingQuest quest)
        {
            this.travellerHero = HeroCreator.CreateSpecialHero(CharacterObject.All.Where((CharacterObject charO) =>
                                                                                            charO.Occupation == Occupation.Wanderer
                                                                                            ).GetRandomElement<CharacterObject>());

            this.cluesAskedtoTraveller = new List<BountyHuntingCampaignBehavior.BountyHuntingQuest.Clue>();
            
        }

        [SaveableField(10)]
        public Hero travellerHero;

        [SaveableField(20)]
        public BountyHuntingCampaignBehavior.BountyHuntingQuest.Clue clueTrigger;

        [SaveableField(30)]
        public BountyHuntingCampaignBehavior.BountyHuntingQuest.Intel intelKnown;

        [SaveableField(40)]
        public List<BountyHuntingCampaignBehavior.BountyHuntingQuest.Clue> cluesAskedtoTraveller;
    }
}
