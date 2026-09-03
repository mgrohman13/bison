using ClassLibrary1.Pieces.Behavior.Combat;
using ClassLibrary1.Pieces.Enemies;
using ClassLibrary1.Pieces.Players;
using System;
using System.Runtime.Serialization;
using static ClassLibrary1.Pieces.Behavior.Combat.CombatTypes;
using static ClassLibrary1.ResearchUpgValues;
using Tile = ClassLibrary1.Map.Map.Tile;

namespace ClassLibrary1.Pieces.Behavior
{
    [Serializable]
    [DataContract(IsReference = true)]
    internal class MissileSilo(Piece piece) : IMissileSilo
    {
        public const double START_RANGE = 26;

        private readonly Piece _piece = piece;
        private bool _producing = false, _attacked = true;
        private int _numMissiles = 0;

        public Piece Piece => _piece;

        public IAttacker.Values Attack => GetValues().Attack;
        public bool Producing
        {
            get => _producing;
            set
            {
                if (Piece.IsPlayer)
                {
                    if (Piece.Side.Energy < 0 || Piece.Side.Mass < 0)
                        _producing = false;
                    _producing = value;
                }
                else throw new Exception();
            }
        }
        public int NumMissiles => _numMissiles;
        public bool Attacked => _attacked;

        public T GetBehavior<T>() where T : class, IBehavior
        {
            return _piece.GetBehavior<T>();
        }
        private Values GetValues()
        {
            return Piece.Game.Player.GetUpgradeValues<Values>();
        }

        public double GetAttack(IKillable killable) => Attack.Attack * GetAttackMult(killable) + TerrainAttMod(killable);

        public bool Fire(IKillable killable)
        {
            bool fired = false;

            if (((IMissileSilo)this).Online)
            {
                if (killable.Piece is EnemyPiece enemy)
                {
                    double energy = enemy.Cost * Piece.Game.Consts.MissileHitRefundPct;
                    double hitPct = killable.CurDefenseValue / killable.MaxDefenseValue;

                    Tile[] tiles = [this.Piece.Tile, enemy.Tile];

                    double attMult = GetAttackMult(killable);
                    double a = Attack.Attack * attMult + TerrainAttMod(killable);
                    int att = Game.Rand.GaussianCappedInt(a, 1 / a, 1);
                    Attack attack = new(Piece, new(Piece.Game.CombatTypes, Attack.Type, att, Attack.Range));
                    fired = attack.Missile(killable);
                    if (fired)
                    {
                        _numMissiles--;

                        hitPct -= killable.CurDefenseValue / killable.MaxDefenseValue;
                        energy *= hitPct;

                        Values values = GetValues();
                        double attLoss = Consts.StatValue(attack.AttackMax);
                        attLoss = (attLoss - Consts.StatValue(attack.AttackCur)) / attLoss;
                        attLoss *= values.Energy + values.Mass * Piece.Game.Consts.EnergyMassRatio;
                        energy += attLoss * attMult;

                        killable.Piece.Side.AddResources(energy);

                        double income = Piece.Game.Enemy.IncomeReference();
                        double mult = (energy + income) / income;

                        double spawnerMult = Math.Sqrt(mult);
                        foreach (Tile tile in Game.Rand.Iterate(tiles))
                            Piece.Game.Map.GetClosestSpawner(tile.Location).Spawner.Mult(Game.Rand.Range(1, spawnerMult));

                        mult *= mult;
                        Alien.IncMorale(enemy, mult, true, .52, tiles);
                    }
                    else
                        ;
                }
                else
                    ;
            }

            return fired;
        }
        private double GetAttackMult(IKillable killable)
        {
            double mult;

            double distance = Piece.Tile.MoveDistTo(killable.Piece.Tile) / GetValues().Attack.Range;
            if (distance < 1)
                mult = 1 + (1 - distance) * (Piece.Game.Consts.MissileAttMaxMult - 1);
            else
                mult = 1 / distance;

            if (!killable.Piece.HasBehavior<IMovable>())
                mult *= Piece.Game.Consts.MissileAttImmobileMult;

            return mult;
        }
        private double TerrainAttMod(IKillable killable) =>
            Combat.Attack.TerrainAttMod(Piece.Tile, killable.Piece.Tile);

        void IBehavior.StartTurn()
        {
            //base.StartTurn();
            this._attacked = false;
        }

        void IBehavior.GetUpkeep(ref double energyUpk, ref double massUpk) =>
            EndTurn(false, ref energyUpk, ref massUpk);

        void IBehavior.EndTurn(ref double energyUpk, ref double massUpk) =>
            EndTurn(true, ref energyUpk, ref massUpk);
        private void EndTurn(bool doEndTurn, ref double energyUpk, ref double massUpk)
        {
            if (Producing)
            {
                Values values = GetValues();
                energyUpk += values.Energy;
                massUpk += values.Mass;
                if (doEndTurn)
                    _numMissiles++;
            }
        }

        double IBehavior.Die()
        {
            Values values = GetValues();
            double value = NumMissiles * Piece.Game.Consts.MissileScrapRefund;
            return (values.Energy + values.Mass * Piece.Game.Consts.EnergyMassRatio) * value;
        }

        [Serializable]
        [DataContract(IsReference = true)]
        private class Values : IUpgradeValues
        {
            private double costMult, range = START_RANGE;
            private double energy, mass;
            private int att;

            public IAttacker.Values Attack => new(AttackType.Kinetic, att, range, 1);
            public double Energy => energy;
            public double Mass => mass;

            public void Init(Game game)
            {
                UpgradeMissileAttack(game, 1);
                UpgradeMissileRange(game, 1);
                UpgradeMissileCost(game, 1);
            }
            public void Upgrade(Game game, Research.Type type, double researchMult)
            {
                if (type == Research.Type.Missile)
                    UpgradeMissileAttack(game, researchMult);
                else if (type == Research.Type.MissileRange)
                    UpgradeMissileRange(game, researchMult);
                else if (type == Research.Type.MissileCost)
                    UpgradeMissileCost(game, researchMult);
            }
            private void UpgradeMissileAttack(Game game, double researchMult)
            {
                this.att = Game.Rand.Round(game.ResearchUpgValues.Calc(UpgType.MissileAttack, researchMult));
                SetCost(game, researchMult);
            }
            private void UpgradeMissileRange(Game game, double researchMult)
            {
                this.range = game.ResearchUpgValues.Calc(UpgType.MissileRange, researchMult);
                SetCost(game, researchMult);
            }
            private void UpgradeMissileCost(Game game, double researchMult)
            {
                this.costMult = game.ResearchUpgValues.Calc(UpgType.MissileCost, researchMult);
                SetCost(game, researchMult);
            }
            private void SetCost(Game game, double researchMult)
            {
                double cost = MechBlueprint.MissileCost(game, this.Attack, researchMult);
                double rangeMult = this.range / START_RANGE;
                cost *= rangeMult * this.costMult * game.Consts.MissileCostMult;

                double costE = cost * game.Consts.MissileEnergyCostRatio;
                this.energy = Game.Rand.GaussianCapped(costE + 1, 1 / costE, 1);
                if (Game.Rand.Bool())
                    this.energy = Math.Round(this.energy);

                double costM = (cost - energy) / game.Consts.EnergyMassRatio;
                this.mass = Game.Rand.Gaussian(costM, 1 / costM);
                if (Game.Rand.Bool())
                    this.mass = Math.Round(this.mass);
            }
        }
    }
}
