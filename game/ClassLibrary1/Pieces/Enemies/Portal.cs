﻿using ClassLibrary1.Pieces.Terrain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using DefenseType = ClassLibrary1.Pieces.CombatTypes.DefenseType;
using Tile = ClassLibrary1.Map.Map.Tile;

namespace ClassLibrary1.Pieces.Enemies
{
    [Serializable]
    public class Portal : EnemyPiece, IDeserializationCallback
    {
        internal const double AvgRange = 6.5, MIN_RANGE = Attack.MELEE_RANGE;// Attack.MIN_RANGED;
        //private readonly SpawnChance spawner;

        private readonly IKillable killable;
        private readonly PieceSpawn spawn;
        //private readonly IAttacker attacker;

        private readonly bool _exit;
        private int _decay;
        private double _range, _collect;
        private readonly double _total;

        //internal readonly double Cost;
        //private double energy;

        public bool Exit => _exit;
        public bool Dead => killable.Dead;

        private Portal(Tile tile, bool exit, IEnumerable<IKillable.Values> killable,
            double resilience, double range, double collect, PieceSpawn spawn)
            : base(tile, AIState.Fight, spawn)
        {
            this._exit = exit;
            this._decay = 0;
            this._range = range;
            this._collect = collect;
            this._total = collect;

            //this.Cost = cost + energy;
            //this.energy = energy;

            this.killable = new Killable(this, killable, resilience);
            this.spawn = spawn;
            //this.attacker = new Attacker(this, attacks);
            SetBehavior(this.killable);

            OnDeserialization(this);
        }
        public override void OnDeserialization(object sender)
        {
            base.OnDeserialization(sender);
            spawn?.OnDeserialization(GetOutTile);
        }
        internal static Portal NewPortal(Tile tile, double difficulty, bool exit, out double cost)
        {
            IEnumerable<IKillable.Values> killable = GenKillable(difficulty, exit);
            //double resilience = MechBlueprint.GenResilience(.26, .169, 1 + hiveIdx);
            //IEnumerable<IAttacker.Values> attacks = GenAttacker(hiveIdx);
            //double strInc = Math.Pow(1.5, hiveIdx);
            //MechBlueprint.CalcCost(3.9 + strInc / 2.1, 0, killable, resilience, attacks, null, out double energy, out double mass);
            //double cost = energy + mass * Consts.MechMassDiv;
            //energy = Game.Rand.Gaussian(Consts.EnemyEnergy * (52 + 2.6 * strInc) - cost, .13);
            //Debug.WriteLine($"hiveCost #{hiveIdx + 1}: {cost} ({energy})");

            double range = Game.Rand.GaussianCapped(AvgRange, .13, MIN_RANGE);
            cost = Consts.PortalCost * (Consts.StatValue(killable.First().Defense) + Consts.PortalDecayRate * range);

            PieceSpawn spawn = exit ? new PieceSpawn() : null;
            spawn?.Spawner?.Mult(3.9);
            Portal obj = new(tile, exit, killable, 1, range, cost * Consts.PortalRewardPct, spawn);
            tile.Map.Game.AddPiece(obj);

            //if (exit)
            //    cost *= Consts.PortalExitCost;
            return obj;
        }

        internal override void StartTurn()
        {
            base.StartTurn();
            if (Side.PiecesOfType<Portal>().Any(p => p.Exit != Exit))
                Decay();
            else
                base._spawn = null;
        }

        private void Decay()
        {
            base._spawn = spawn;

            IKillable killable1 = GetBehavior<IKillable>();
            Defense hits = killable1.Hits;
            int def = hits.DefenseCur;

            double pct = Math.Max(0, 1 - Game.Rand.DoubleFull(Consts.PortalDecayRate) / Consts.StatValue(def));
            _range = MIN_RANGE + pct * (_range - MIN_RANGE);
            int decay = Game.Rand.OEInt(Consts.PortalDecayRate);
            _collect *= 1 - decay / (decay + Consts.PortalExitDef);

            pct = 2 / (1 + pct);
            spawn?.Spawner.Mult(pct * pct);

            this._decay += decay;
            while (_decay >= def && def > 0)
            {
                _decay -= def;
                def--;
            }

            if (def > 0)
                killable1.SetHits(def, hits.DefenseMax);
            else
                Die();
        }

        internal override void Die()
        {
            Tile tile = this.Tile;
            base.Die();
            Treasure.NewTreasure(tile, _collect);
            Game.Enemy.Income(_total - _collect); 
        }

        private static IEnumerable<IKillable.Values> GenKillable(double difficulty, bool exit)
        {
            double avg = GetDefAvg(difficulty, exit);
            IKillable.Values hits = new(DefenseType.Hits, Game.Rand.GaussianOEInt(avg, .13, .13, 10));

            List<IKillable.Values> defenses = new() { hits };
            return defenses;
        }
        internal static double GetDefAvg(double difficulty, bool exit)
        {
            double avg = exit ? Consts.PortalExitDef : Consts.PortalEntranceDef;
            avg = Consts.StatValueInverse(avg * difficulty);
            return avg;
        }

        public bool CanPort(IMovable movable, out Portal exit, out double dist)
        {
            Piece piece = movable.Piece;
            exit = null;
            dist = this.Tile.GetDistance(piece.Tile);
            if (this.Side == piece.Side && !this.Exit && movable.CanMove && dist <= movable.MoveCur)
            {
                var exits = this.Side.PiecesOfType<Portal>().Where(p => p.Exit);
                if (exits.Any())
                    exit = Game.Rand.SelectValue(exits);
            }
            return exit is not null;
        }
        internal Tile GetOutTile()
        {
            if (Exit)
            {
                Tile tile;
                double range = _range;
                do
                {
                    tile = Game.Map.GetTile(Tile.X + Game.Rand.GaussianInt(range), Tile.Y + Game.Rand.GaussianInt(range));
                    range += Game.Rand.DoubleFull();
                }
                while (tile is null || tile.Piece is not null);
                return tile;
            }
            return null;
        }

        public override string ToString()
        {
            return "Wormhole " + (_exit ? "Exit" : "Entrance");
        }
    }
}
