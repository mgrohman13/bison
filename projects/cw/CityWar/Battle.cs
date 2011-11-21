using System;
using System.Collections.Generic;
using System.Text;

namespace CityWar
{
	public class Battle
	{
		internal List<Unit> attackers = new List<Unit>(), defenders = new List<Unit>(), extraDefenders = new List<Unit>();
		private Dictionary<Attack, List<Unit>> attackTargets = new Dictionary<Attack, List<Unit>>();
		private Dictionary<Unit, List<Attack>> targetAttackers = new Dictionary<Unit, List<Attack>>();

		public Unit[] GetAttackers()
		{
			return attackers.ToArray();
		}
		public Unit[] GetDefenders()
		{
			return defenders.ToArray();
		}

		internal bool canRetalliate = true;
		public bool CanRetalliate
		{
			get
			{
				return canRetalliate;
			}
		}

		private Battle() { }

		internal Battle(Unit[] attackers, Unit[] defenders)
		{
			//Tile.SortPieces(attackers);
			//Tile.SortPieces(defenders);
			this.attackers.AddRange(attackers);
			this.defenders.AddRange(defenders);
		}

		internal void StartRetalliation()
		{
			canRetalliate = false;
			List<Unit> temp = new List<Unit>(attackers);
			attackers = new List<Unit>(defenders);
			defenders = new List<Unit>(temp);
			attackers.AddRange(extraDefenders);
			extraDefenders.Clear();
		}

		//  bool AIattacker(out Attack attack, out Unit unit, out double result)
		//{
		//    BeginAI(true);
		//    return AI(out attack, out unit, out result);
		//}

		//  bool AIdefender(out Attack attack, out Unit unit, out double result)
		//{
		//    BeginAI(false);
		//    return AI(out attack, out unit, out result);
		//}

		//private void BeginAI(bool attacker)
		//{
		//    attackTargets = new Dictionary<Attack, List<Unit>>();
		//    targetAttackers = new Dictionary<Unit, List<Attack>>();

		//    List<Unit> units, enemies;
		//    if (attacker)
		//    {
		//        units = this.attackers;
		//        enemies = this.defenders;
		//    }
		//    else
		//    {
		//        units = this.defenders;
		//        enemies = this.attackers;
		//    }

		//    foreach (Unit u in units)
		//        foreach (Attack attack in u.Attacks)
		//        {
		//            List<Unit> targets = new List<Unit>();
		//            foreach (Unit unit in enemies)
		//                if (attack.CanAttack(unit))
		//                    targets.Add(unit);

		//            if (targets.Count > 0)
		//                attackTargets.Add(attack, targets);
		//        }

		//    foreach (Unit unit in enemies)
		//    {
		//        List<Attack> attackers = new List<Attack>();
		//        foreach (Unit u in units)
		//            foreach (Attack a in u.Attacks)
		//                if (a.CanAttack(unit))
		//                    attackers.Add(a);

		//        if (attackers.Count > 0)
		//            targetAttackers.Add(unit, attackers);
		//    }
		//}

		//private bool AI(out Attack attackUsed, out Unit unitUsed, out double result)
		//{
		//    attackUsed = null;
		//    unitUsed = null;
		//    result = 0;
		//    if (attackTargets.Count == 0)
		//        return false;

		//    double min = double.MaxValue;
		//    foreach (Attack attack in attackTargets.Keys)
		//    {
		//        foreach (Unit u in attackTargets[attack])
		//        {
		//            double diff = Math.Abs((double)u.Hits - attack.getAverageDamage(u));
		//            min = Math.Min(min, diff);
		//        }
		//        foreach (Unit u in attackTargets[attack])
		//        {
		//            double diff = Math.Abs((double)u.Hits - attack.getAverageDamage(u));
		//            if (Math.Abs(diff - min) < .0000001)
		//            {
		//                if (min <= u.Hits)
		//                    foreach (Attack a in targetAttackers[u])
		//                        if (Math.Abs(a.getAverageDamage(u) - min) < .0000001)
		//                            return AIAttack(a, u, out attackUsed, out unitUsed, out result);
		//            }
		//        }
		//    }

		//    min = 1;
		//    foreach (Unit u in targetAttackers.Keys)
		//        min = Math.Min(min, u.getHealthPctFloat());
		//    foreach (Unit u in targetAttackers.Keys)
		//        if (u.getHealthPctFloat() - min < .0000001)
		//        {
		//            int val = 2;
		//            int armorClass = (u.Armor - 1) / 4 + 1;
		//            if (armorClass < 1)
		//                armorClass = 1;
		//            else if (armorClass > 3)
		//                armorClass = 3;
		//            foreach (Attack a in attackTargets.Keys)
		//                val = Math.Min(val, Math.Abs(armorClass - a.AP));
		//            if (armorClass == 1)
		//            {
		//                min = int.MaxValue;
		//                foreach (Attack a in attackTargets.Keys)
		//                    if (Math.Abs(a.AP - armorClass) == val)
		//                        min = Math.Min(min, a.Average);
		//            }
		//            else if (armorClass == 3)
		//            {
		//                min = 0;
		//                foreach (Attack a in attackTargets.Keys)
		//                    if (Math.Abs(a.AP - armorClass) == val)
		//                        min = Math.Max(min, a.Average);
		//            }
		//            if (armorClass != 2)
		//                foreach (Attack attack in attackTargets.Keys)
		//                    if ((attackTargets[attack]).Contains(u)
		//                        && (Math.Abs(attack.Average - min) < .0000001))
		//                        return AIAttack(attack, u, out attackUsed, out unitUsed, out result);
		//            foreach (Attack attack in attackTargets.Keys)
		//                if ((attackTargets[attack]).Contains(u))
		//                    return AIAttack(attack, u, out attackUsed, out unitUsed, out result);
		//        }

		//    double max = 0;
		//    foreach (Attack attack in attackTargets.Keys)
		//        foreach (Unit u in attackTargets[attack])
		//            max = Math.Max(max, attack.getAverageDamage(u));

		//    foreach (Attack attack in attackTargets.Keys)
		//        foreach (Unit u in attackTargets[attack])
		//            if (Math.Abs(max - attack.getAverageDamage(u)) < .0000001)
		//                return AIAttack(attack, u, out attackUsed, out unitUsed, out result);

		//    foreach (Attack attack in attackTargets.Keys)
		//        foreach (Unit u in attackTargets[attack])
		//            return AIAttack(attack, u, out attackUsed, out unitUsed, out result);

		//    return false;
		//}

		//bool AIAttack(Attack a, Unit u, out Attack attackUsed, out Unit unitUsed, out double result)
		//{
		//    if (!a.CanAttack(u))
		//        throw new Exception("shit");

		//    attackUsed = a;
		//    unitUsed = u;
		//    result = a.AttackUnit(u);

		//    if (u.Dead)
		//    {
		//        attackers.Remove(u);
		//        defenders.Remove(u);
		//    }

		//    return true;
		//}

		public void MoveToExtraDefenders(Unit def)
		{
			defenders.Remove(def);
			extraDefenders.Add(def);
		}
	}
}
