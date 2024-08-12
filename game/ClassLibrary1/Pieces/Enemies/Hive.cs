using ClassLibrary1.Pieces.Players;
using ClassLibrary1.Pieces.Terrain;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using static ClassLibrary1.Map.Map;
using AttackType = ClassLibrary1.Pieces.CombatTypes.AttackType;
using DefenseType = ClassLibrary1.Pieces.CombatTypes.DefenseType;
using Tile = ClassLibrary1.Map.Map.Tile;

namespace ClassLibrary1.Pieces.Enemies
{
    [Serializable]
    public class Hive : EnemyPiece, IDeserializationCallback
    {
        private readonly SpawnChance spawner;

        private readonly IKillable killable;
        private readonly IAttacker attacker;

        internal readonly double Cost;
        private double energy;

        public bool Dead => killable.Dead;

        private Hive(Tile tile, SpawnChance spawner, IEnumerable<IKillable.Values> killable, double resilience, IEnumerable<IAttacker.Values> attacks, double cost, double energy)
            : base(tile, AIState.Fight)
        {
            this.spawner = spawner;

            this.Cost = cost + energy;
            this.energy = energy;

            this.killable = new Killable(this, killable, resilience);
            this.attacker = new Attacker(this, attacks);
            SetBehavior(this.killable, this.attacker);

            OnDeserialization(this);
        }
        internal static Hive NewHive(Tile tile, int hiveIdx, SpawnChance spawner)
        {
            IEnumerable<IKillable.Values> killable = GenKillable(hiveIdx);
            double resilience = MechBlueprint.GenResilience(.26, .169, 1 + hiveIdx);
            IEnumerable<IAttacker.Values> attacks = GenAttacker(hiveIdx);
            double strInc = Math.Pow(1.5, hiveIdx);
            MechBlueprint.CalcCost(3.9 + strInc / 2.1, 0, killable, resilience, attacks, null, out double energy, out double mass);
            double cost = energy + mass * Consts.EnergyMassRatio;
            energy = Game.Rand.Gaussian(Consts.EnemyEnergy * (52 + 2.6 * strInc) - cost, .13);
            Debug.WriteLine($"hiveCost #{hiveIdx + 1}: {cost} ({energy})");

            Hive obj = new(tile, spawner, killable, resilience, attacks, cost, energy);
            tile.Map.Game.AddPiece(obj);

            Tile ResourceSpawn() => Game.Rand.SelectValue(tile.GetTilesInRange(obj.attacker).Where(t => t.Piece == null));
            if (Game.Rand.Bool())
            {
                Artifact.NewArtifact(ResourceSpawn());
            }
            else if (Game.Rand.Bool())
            {
                if (Game.Rand.Bool())
                    Biomass.NewBiomass(ResourceSpawn());
                else
                    Metal.NewMetal(ResourceSpawn());
            }

            return obj;
        }
        public override void OnDeserialization(object sender)
        {
            base.OnDeserialization(sender);
            if (killable != null)
            {
                ((Killable)killable).OnDeserialization(this);
                killable.Event.DamagedEvent += Killable_DamagedEvent;
            }
            if (attacker != null)
            {
                ((Attacker)attacker).OnDeserialization(this);
                attacker.Event.AttackEvent += Attacker_AttackEvent;
            }
        }

        private void Attacker_AttackEvent(object sender, Attacker.AttackEventArgs e)
        {
            Tile.Map.UpdateVision(Tile.Location, Math.Sqrt(SumRange + Attack.MIN_RANGED));
        }
        private void Killable_DamagedEvent(object sender, Killable.DamagedEventArgs e)
        {
            double cur = killable.AllDefenses.Sum(d => Consts.StatValue(d.DefenseCur));
            double max = killable.AllDefenses.Sum(d => Consts.StatValue(d.DefenseMax));
            ((Enemy)Side).HiveDamaged(this, e.DefTile, spawner, ref energy,
                killable.Hits.DefenseCur, cur / max, MaxRange / 2.1 + Attack.MELEE_RANGE);
        }
        public double SumRange => attacker.Attacks.Sum(a => a.Range);
        public double MaxRange => attacker.Attacks.Max(a => a.Range);

        internal override void StartTurn()
        {
            base.StartTurn();
            spawner.Mult(1 + 1.0 / 65);
        }

        internal override void Die()
        {
            Tile tile = this.Tile;
            base.Die();
            Game.VictoryPoint();
            tile.Map.GenResources(_ => tile, .065);
            Game.CollectResources(tile, Cost / 1.69, out _, out _);
        }

        private static IEnumerable<IKillable.Values> GenKillable(int hiveIdx)
        {
            hiveIdx += Game.Rand.Next(3);
            IKillable.Values hits = new(DefenseType.Hits, Game.Rand.GaussianOEInt(10.4 + 3.9 * hiveIdx, .13, .13, 10));

            List<IKillable.Values> defenses = new() { hits };

            double def = 9.1 + .65 * hiveIdx;
            bool armor = Game.Rand.Bool();
            if (armor)
            {
                int shield = GenShield(5.2 + .39 * hiveIdx);
                // use StatValueDiff
                def = Math.Max(1, Consts.StatValueInverse(Consts.StatValue(def * 2.1) - Consts.StatValue(shield)));
                defenses.Add(new(DefenseType.Shield, shield));
                defenses.Add(new(DefenseType.Armor, Game.Rand.GaussianOEInt(def, .13, .13, Math.Min((int)def, 5))));
            }
            else
            {
                defenses.Add(new(DefenseType.Shield, GenShield(def)));
            }
            static int GenShield(double v) => Game.Rand.GaussianOEInt(v, .13, .13, 5);

            return defenses;
        }
        private static IEnumerable<IAttacker.Values> GenAttacker(int hiveIdx)
        {
            hiveIdx += Game.Rand.Next(3);
            bool flag = Game.Rand.Bool();

            int att = Game.Rand.GaussianOEInt(6.5 + 1.3 * hiveIdx, .13, .13, 5);
            double range = Game.Rand.GaussianOE(16.9 + 2.1 * hiveIdx, .13, .13, 10);
            IAttacker.Values att1 = new(flag ? AttackType.Energy : AttackType.Kinetic, att, range);

            att = Game.Rand.GaussianOEInt(1.69 + 2.6 * hiveIdx, .13, .13, 1);
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
