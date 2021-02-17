using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GalWar;

namespace GalWarWin
{
	public partial class CombatForm : Form
	{

		//backgroundworker?

		private GameForm gameForm;
		private Ship attacker, defender;

		//private int acceptMillis = 666;
		//private float acceptComplexity = float.MaxValue;
		//private float acceptRiseMult = 1.3;

		public CombatForm(GameForm gameForm)
		{
			InitializeComponent();

			this.gameForm = gameForm;
		}

		public void SetShips(Ship attacker, Ship defender)
		{
			this.attacker = attacker;
			this.defender = defender;

			RefreshShips();
		}

		private void RefreshShips()
		{
			this.btnAttack.Visible = Tile.IsNeighbor(attacker.Tile, defender.Tile)
					&& (attacker.CurSpeed > 0 && attacker.Player.IsTurn);

			this.lblAttack.Text = attacker.Att.ToString();
			this.lblAttHP.Text = attacker.CurHP.ToString() + " / " + attacker.MaxHP.ToString();
			this.lblDefense.Text = defender.Def.ToString();
			this.lblDefHP.Text = defender.CurHP.ToString() + " / " + defender.MaxHP.ToString();

			this.lblAttPlayer.BackColor = attacker.Player.Color;
			this.lblAttPlayer.Text = attacker.Player.Name;
			this.lblDefPlayer.BackColor = defender.Player.Color;
			this.lblDefPlayer.Text = defender.Player.Name;

			CalculateOdds();
		}

		private void CalculateOdds()
		//{
		//    CalculateOdds(false);
		//}

		//private void CalculateOdds(bool forceSimulate)
		{
			//this.btnExact.Visible = false;

			int att = attacker.Att, def = defender.Def;

			Dictionary<int, float> damageTable = new Dictionary<int, float>();
			float avgAtt = 0, avgDef = 0, total = 0;
			for (int a = 0; a <= att; ++a)
				for (int d = 0; d <= def; ++d)
				{
					int damage = a - d;
					if (damage > 0)
						avgAtt += damage;
					else
						avgDef -= damage;
					++total;
					AddChance<int>(damageTable, damage, 1);
				}

			float attDead = 0, defDead = 0;
			float attDmg = avgDef / total * att;
			float defDmg = avgAtt / total * att;

			if (att * att >= defender.CurHP || att * def >= attacker.CurHP)
			{
				//Approximate(out attDead, out defDead, damageTable);
				//if (attDead > .005 || defDead > .005)
				Simulate(out attDmg, out defDmg, out attDead, out defDead, damageTable);
			}

			this.lblAttKill.Text = FormatPct(attDead);
			this.lblDefKill.Text = FormatPct(defDead);

			this.lblAttDmg.Text = FormatDmg(attDmg);
			this.lblDefDmg.Text = FormatDmg(defDmg);
		}

		//private void DoCalc(bool forceSimulate, ref float attDmg, ref float defDmg, out float attDead, out float defDead, Dictionary<int, float> damageTable)
		//{
		//    attDead = 0;
		//    defDead = 0;

		//    bool simulate = forceSimulate;
		//    if (!simulate)
		//    {
		//        int att = attacker.Att, def = defender.Def, attHP = attacker.CurHP, defHP = defender.CurHP;
		//        if (!(att * att < defHP && def * att < attHP))
		//        {
		//            Approximate(out attDmg, out defDmg, out attDead, out defDead, damageTable);
		//            simulate = (attDead > .005 || defDead > .005);
		//            if (GetComplexity(att, def, attHP, defHP) > acceptComplexity)
		//            {
		//                this.btnExact.Visible = true;
		//                simulate = false;
		//            }
		//        }
		//    }
		//    if (simulate)
		//        Simulate(out attDmg, out defDmg, out attDead, out defDead, damageTable);
		//}

		private void Approximate(out float attDead, out float defDead, Dictionary<int, float> damageTable)
		{
			int rounds = attacker.Att;
			int attHP = attacker.CurHP;
			int defHP = defender.CurHP;

			float[] attArr = new float[attacker.Att + 1];
			float[] defArr = new float[defender.Def + 1];
			foreach (int damage in damageTable.Keys)
			{
				int att = damage;
				int def = damage;
				if (att < 0)
					att = 0;
				if (def > 0)
					def = 0;
				def *= -1;

				float chance = damageTable[damage];
				attArr[att] += chance;
				defArr[def] += chance;
			}


			attArr = GetArr(attArr, rounds);
			defDead = 0;
			float attChances = 0;
			int attLen = attArr.Length;
			for (int a = 0; a < attLen; ++a)
			{
				attChances += attArr[a];
				if (a >= defHP)
					defDead += attArr[a];
			}
			defDead /= attChances;

			defArr = GetArr(defArr, rounds);
			attDead = 0;
			float defChances = 0;
			int defLen = defArr.Length;
			for (int d = 0; d < defLen; ++d)
			{
				defChances += defArr[d];
				if (d >= attHP)
					attDead += defArr[d];
			}
			attDead /= defChances;
		}

		private float[] GetArr(float[] cur, int amt)
		{
			int count = cur.Length;
			int lower = count - 1;
			for (int b = 2; b <= amt; ++b)
			{
				int len = lower * b + 1;
				float[] prev = new float[len];
				prev[0] = cur[0];
				for (int c = 1; c < len; ++c)
					prev[c] = prev[c - 1] + (c < cur.Length ? cur[c] : 0);
				cur = new float[len];
				for (int d = 0; d < len; ++d)
				{
					cur[d] = prev[d];
					if (d > lower)
						cur[d] -= prev[d - count];
				}
			}
			return cur;
		}

		//private float[] GetArr(float[] cur, int amt)
		//{
		//    int count = cur.Length;
		//    int lower = count - 1;
		//    for (int b = 2; b <= amt; ++b)
		//    {
		//        int len = lower * b + 1;
		//        int div = (int)Math.Floor((len + 1) / 2.0);
		//        float[] prev = new float[div];
		//        prev[0] = cur[0];
		//        for (int c = 1; c < div; ++c)
		//            prev[c] = prev[c - 1] + cur[c];
		//        cur = new float[len];
		//        for (int d = 0; d < div; ++d)
		//        {
		//            cur[d] = prev[d];
		//            if (d > lower)
		//                cur[d] -= prev[d - count];
		//        }
		//        for (int e = len; e > div; --e)
		//            cur[e - 1] = cur[len - e];
		//    }
		//    return cur;
		//}

		private void Simulate(out float attDmg, out float defDmg, out float attDead, out float defDead,
				Dictionary<int, float> damageTable)
		{
			//long startTime = Environment.TickCount;

			int att = attacker.Att, def = defender.Def;
			float possibleDamages = (att + 1) * (def + 1);

			Dictionary<Point, float> chances = new Dictionary<Point, float>();
			chances.Add(new Point(attacker.CurHP, defender.CurHP), 1);

			for (int round = -1; ++round < att; )
			{
				Dictionary<Point, float> newChances = new Dictionary<Point, float>();

				foreach (Point point in chances.Keys)
				{
					float chance = chances[point];
					if (point.Y > 0 && point.X > 0)
						foreach (int damage in damageTable.Keys)
						{
							Point p;
							if (damage > 0)
								p = new Point(point.X, point.Y - damage);
							else if (damage < 0)
								p = new Point(point.X + damage, point.Y);
							else
								p = point;
							AddChance<Point>(newChances, p, chance * damageTable[damage]);
						}
					else
						AddChance<Point>(newChances, point, chance * possibleDamages);
				}

				chances = newChances;
			}

			float total = 0;
			attDead = 0;
			defDead = 0;
			attDmg = 0;
			defDmg = 0;
			foreach (Point p in chances.Keys)
			{
				float chance = chances[p];
				total += chance;
				int attHP = p.X;
				int defHP = p.Y;
				if (attHP <= 0)
				{
					attDead += chance;
					attHP = 0;
				}
				if (defHP <= 0)
				{
					defDead += chance;
					defHP = 0;
				}
				attDmg += (attacker.CurHP - attHP) * chance;
				defDmg += (defender.CurHP - defHP) * chance;
			}
			attDead /= total;
			defDead /= total;
			attDmg /= total;
			defDmg /= total;

			//float complexity = GetComplexity(att, def, attacker.CurHP, defender.CurHP);
			//if (Environment.TickCount - startTime > acceptMillis)
			//    acceptComplexity = Math.Min(complexity, acceptComplexity);
			//else if (complexity > acceptComplexity)
			//    acceptComplexity = complexity;
			//acceptComplexity *= acceptRiseMult;
		}

		//private float GetComplexity(int att, int def, int attHP, int defHP)
		//{
		//    float rounds = att;
		//    float damageTableLength = att + def + 1;
		//    float possibleResults = (attHP + 1) * (defHP + 1) - 1;
		//    return Math.Min(Math.Pow(damageTableLength, rounds), rounds * possibleResults * damageTableLength);
		//}

		private void AddChance<Key>(Dictionary<Key, float> dictionary, Key key, float chance)
		{
			if (dictionary.ContainsKey(key))
				dictionary[key] += chance;
			else
				dictionary.Add(key, chance);
		}

		private string FormatPct(float pct)
		{
			pct *= 100;
			return pct.ToString("0") + "%";
		}

		private string FormatDmg(float dmg)
		{
			return "-" + dmg.ToString("0.0");
		}

		private void btnAttack_Click(object sender, EventArgs e)
		{
			attacker.Attack(defender);
			if (attacker.CurSpeed < 1 || attacker.CurHP < 1 || defender.CurHP < 1)
			{
				this.DialogResult = DialogResult.OK;
			}
			else
			{
				gameForm.RefreshAll();
				RefreshShips();
			}
		}

		private void btnSwap_Click(object sender, EventArgs e)
		{
			Ship temp = attacker;
			attacker = defender;
			defender = temp;

			RefreshShips();
		}

	}
}
