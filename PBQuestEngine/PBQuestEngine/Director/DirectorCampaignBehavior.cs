using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.Core;

namespace PBQuestEngine.Director
{
    class DirectorCampaignBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            //Tracked information
            CampaignEvents.SiegeCompletedEvent.AddNonSerializedListener(this, new Action<Settlement, MobileParty, bool, bool>(this.OnSiegeCompletedEvent));
            CampaignEvents.OnClanDestroyedEvent.AddNonSerializedListener(this, new Action<Clan>(this.OnClanDestroyed));
            CampaignEvents.OnPrisonerTakenEvent.AddNonSerializedListener(this, new Action<FlattenedTroopRoster>(this.OnPrisonerTakenEvent));
            CampaignEvents.VillageLooted.AddNonSerializedListener(this, new Action<Village>(this.OnVillageLooted));
            CampaignEvents.ArmyCreated.AddNonSerializedListener(this, new Action<Army>(this.OnArmyCreated));
            CampaignEvents.KingdomDecisionConcluded.AddNonSerializedListener(this, new Action<KingdomDecision, DecisionOutcome, bool>(this.OnKingdomDecisionConcluded));
            CampaignEvents.MercenaryClanChangedKingdom.AddNonSerializedListener(this, new Action<Clan, Kingdom, Kingdom>(this.OnMercenaryClanChangedKingdom));
            CampaignEvents.HeroRelationChanged.AddNonSerializedListener(this, new Action<Hero, Hero, int, bool>(this.OnHeroRelationChanged));
            CampaignEvents.OnSiegeAftermathAppliedEvent.AddNonSerializedListener(this, new Action<MobileParty, Settlement, SiegeAftermathCampaignBehavior.SiegeAftermath, Clan, Dictionary<MobileParty, float>>(this.OnSigeAftermathApplied));

            //Rolls for Quest formation

            //On hero killed
        }

        private void OnSiegeCompletedEvent(Settlement seigedSettlement, MobileParty seigingParty, bool bool1, bool bool2)
        { 
            //if(seigedSettlement.Culture.Name.ToString() ==  CampaignData.CultureEmpire)// not implemented
            //{
                InformationManager.DisplayMessage(new InformationMessage("DIRECTORTEST Settlement has been successfully sieged."));

                DirectorCampaignLogs.AddSeigeCompletedEventLog(new DirectorCampaignLogs.SiegeCompletedEvent(seigedSettlement, seigingParty, bool1, bool2));
            //}
        }

        private void OnClanDestroyed(Clan clan)
        { }

        private void OnPrisonerTakenEvent(FlattenedTroopRoster troop)
        { }
        private void OnVillageLooted(Village village)
        {
            InformationManager.DisplayMessage(new InformationMessage("DIRECTORTEST Village has been looted."));
            new DirectorCampaignLogs.VillageLootedEventLog(village, village.Settlement.LastAttackerParty.LeaderHero, CampaignTime.Now);                        
        }
        private void OnArmyCreated(Army army)
        { }
        private void OnKingdomDecisionConcluded(KingdomDecision decision, DecisionOutcome outcome, bool bool1)
        { }
        private void OnMercenaryClanChangedKingdom(Clan mercenaryClan, Kingdom kingdom1, Kingdom kingdom2)
        { }
        private void OnHeroRelationChanged(Hero hero1, Hero hero2, int amount, bool bool1)
        { }
        private void OnSigeAftermathApplied(MobileParty party, Settlement settlment, SiegeAftermathCampaignBehavior.SiegeAftermath aftermath, Clan clan, Dictionary<MobileParty, float> idkitssomething)
        { }

        public override void SyncData(IDataStore dataStore)
        {
            
        }
    }
}
