using System;
using System.Collections.Generic;
using System.Linq;

namespace GalWar
{
    public interface IEventHandler
    {
        Tile GetBuildTile(Colony colony, ShipDesign design);

        Buildable GetNewBuild(Colony colony, double production, bool floor);

        int MoveTroops(Colony fromColony, int max, int totalPop, double soldiers, bool extraCost);

        bool Continue(Planet planet, int initPop, int initQuality, int stopPop, int stopQuality, int finalPop, int finalQuality);

        bool ConfirmCombat(Combatant attacker, Combatant defender);

        bool Explore(Anomaly.AnomalyType anomalyType, params object[] info);

        void OnResearch(ShipDesign newDesign, HashSet<ShipDesign> obsolete);

        void OnCombat(Combatant attacker, Combatant defender, int attack, int defense);

        void OnLevel(Ship ship, double pct, int last, int needed);

        void OnBombard(Ship ship, Planet planet, int freeDmg, int colonyDamage, int planetDamage);

        void OnInvade(Ship ship, Colony colony, int attackers, double attSoldiers, double gold, double attack, double defense);

        void Event();
    }
}
