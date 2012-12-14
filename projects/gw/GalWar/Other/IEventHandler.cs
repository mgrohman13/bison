using System;
using System.Collections.Generic;

namespace GalWar
{
    public interface IEventHandler
    {
        Tile getBuildTile(Colony colony);

        Buildable getNewBuild(Colony colony, bool accountForIncome, bool switchLoss, params double[] additionalLosses);

        int MoveTroops(Colony fromColony, int max, int free, int totalPop, double soldiers);

        bool ConfirmCombat(Combatant attacker, Combatant defender);

        void OnResearch(ShipDesign newDesign, HashSet<ShipDesign> obsolete, PlanetDefense oldDefense, PlanetDefense newDefense);

        void OnCombat(Combatant attacker, Combatant defender, int attack, int defense);

        void OnLevel(Ship ship, double pct, int last, int needed);

        void OnBombard(Ship ship, Planet planet, int freeDmg, int colonyDamage, int planetDamage);

        void OnInvade(Ship ship, Colony colony);

        void Event();
    }
}
