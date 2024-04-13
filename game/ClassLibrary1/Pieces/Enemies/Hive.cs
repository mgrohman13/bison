using ClassLibrary1.Pieces.Players;
using ClassLibrary1.Pieces.Terrain;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using AttackType = ClassLibrary1.Pieces.CombatTypes.AttackType;
using DefenseType = ClassLibrary1.Pieces.CombatTypes.DefenseType;
using Tile = ClassLibrary1.Map.Map.Tile;

namespace ClassLibrary1.Pieces.Enemies
{
    [Serializable]
    public class Hive : EnemyPiece, IDeserializationCallback
    {
        private readonly IKillable killable;
        private readonly IAttacker attacker;

        private readonly double cost;
        private double energy;

        public Piece Piece => this;
        public bool Dead => killable.Dead;

        private Hive(Tile tile, IEnumerable<IKillable.Values> killable, double resilience, IEnumerable<IAttacker.Values> attacks, double cost, double energy)
            : base(tile)
        {
            this.cost = cost + energy;
            this.energy = energy;

            this.killable = new Killable(this, killable, resilience);
            this.attacker = new Attacker(this, attacks);
            SetBehavior(this.killable, this.attacker);

            OnDeserialization(this);
        }
        internal static Hive NewHive(Tile tile, int hiveIdx)
        {
            IEnumerable<IKillable.Values> killable = GenKillable(hiveIdx);
            double resilience = Consts.GetPct(Game.Rand.GaussianCapped(.26, .26, .013), 1 + hiveIdx);
            IEnumerable<IAttacker.Values> attacks = GenAttacker(hiveIdx);
            double strInc = Math.Pow(1.5, hiveIdx);
            MechBlueprint.CalcCost(3.9 + strInc / 1.69, 0, killable, resilience, attacks, null, out double energy, out double mass);
            double cost = energy + mass * Consts.MechMassDiv;
            energy = Game.Rand.Gaussian(Consts.EnemyEnergy * (39 + 2.1 * strInc) - cost, .13);
            Debug.WriteLine($"hiveCost #{hiveIdx + 1}: {cost} ({energy})");

            Hive obj = new(tile, killable, resilience, attacks, cost, energy);
            tile.Map.Game.AddPiece(obj);

            Artifact.NewArtifact(Game.Rand.SelectValue(tile.GetTilesInRange(obj.attacker).Where(t => t.Piece == null)));

            return obj;
        }

        public void OnDeserialization(object sender)
        {
            ((Killable)this.killable).OnDeserialization(this);
            ((Attacker)this.attacker).OnDeserialization(this);
            this.attacker.Event.AttackEvent += Attacker_AttackEvent;
            this.killable.Event.DamagedEvent += Killable_DamagedEvent;
        }

        private void Attacker_AttackEvent(object sender, Attacker.AttackEventArgs e)
        {
            Tile.Map.UpdateVision(this.Tile, Math.Sqrt(attacker.Attacks.Sum(a => a.Range) + Attack.MELEE_RANGE));
        }
        private void Killable_DamagedEvent(object sender, Killable.DamagedEventArgs e)
        {
            double cur = killable.TotalDefenses.Sum(d => Consts.StatValue(d.DefenseCur));
            double max = killable.TotalDefenses.Sum(d => Consts.StatValue(d.DefenseMax));
            ((Enemy)Side).HiveDamaged(this, ref energy, killable.Hits.DefenseCur, cur / max, MaxRange);
        }
        public double MaxRange => attacker.Attacks.Max(a => a.Range);

        internal override void Die()
        {
            Tile tile = this.Tile;
            base.Die();
            tile.Map.GenResources(new[] { tile }, true);
            Game.CollectHive(cost);
        }

        private static IEnumerable<IKillable.Values> GenKillable(int hiveIdx)
        {
            hiveIdx += Game.Rand.Next(3);
            IKillable.Values hits = new(DefenseType.Hits, Game.Rand.GaussianOEInt(13 + 3.9 * hiveIdx, .13, .13, 10));

            DefenseType type = Game.Rand.Bool() ? DefenseType.Shield : DefenseType.Armor;
            double def = 6.5 + .26 * hiveIdx;
            if (type == DefenseType.Armor)
                def *= 2.1;
            IKillable.Values extra = new(type, Game.Rand.GaussianOEInt(def, .13, .13, 5));

            return new[] { hits, extra };
        }
        private static IEnumerable<IAttacker.Values> GenAttacker(int hiveIdx)
        {
            hiveIdx += Game.Rand.Next(3);
            bool flag = Game.Rand.Bool();

            int att = Game.Rand.GaussianOEInt(6.5 + 1.3 * hiveIdx, .13, .13, 5);
            double range = Game.Rand.GaussianOE(16.9 + 2.1 * hiveIdx, .13, .13, 10);
            IAttacker.Values att1 = new(flag ? AttackType.Energy : AttackType.Kinetic, att, range);

            att = Game.Rand.GaussianOEInt(1.69 + 2.1 * hiveIdx, .13, .13, 1);
            range = Game.Rand.GaussianOE(13 + 1.69 * hiveIdx, .13, .13, 10);
            IAttacker.Values att2 = new(flag ? AttackType.Kinetic : AttackType.Energy, att, range);

            return new[] { att1, att2 };
        }

        public override string ToString()
        {
            return "Hive " + PieceNum;
        }
    }
}
