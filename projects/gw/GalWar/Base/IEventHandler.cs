using System;
using System.Collections.Generic;
using System.Text;

namespace GalWar
{
    public interface IEventHandler
    {
        Tile getBuildTile(Colony colony);

        Buildable getNewBuild(Colony colony, bool accountForIncome, bool switchLoss, params double[] additionalLosses);

        int MoveTroops(Colony fromColony, int total, int free, int totalPop, double soldiers);

        bool ConfirmCombat(Combatant attacker, Combatant defender);

        void OnResearch(ShipDesign newDesign, HashSet<ShipDesign> obsolete, PlanetDefense oldDefense, PlanetDefense newDefense);

        void OnCombat(Combatant attacker, Combatant defender, int attack, int defense, int popLoss);

        void OnLevel(Ship ship, Ship.ExpType expType, double pct, int needExp, int lastExp);
    }
}
