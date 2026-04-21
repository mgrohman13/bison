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
        public static double START_RANGE = 26;

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

        private readonly Piece _piece = piece;
        private bool _producing = false;
        private int _numMissiles = 0;

        public T GetBehavior<T>() where T : class, IBehavior
        {
            return _piece.GetBehavior<T>();
        }
        private Values GetValues()
        {
            return Piece.Game.Player.GetUpgradeValues<Values>();
        }

        public double GetAttack(IKillable killable) => Attack.Attack * GetAttackMult(killable);

        public bool Fire(IKillable killable)
        {
            bool fired = false;

            if (((IMissileSilo)this).Online)
            {
                EnemyPiece enemy = killable.Piece as EnemyPiece;
                if (enemy is not null)
                {
                    double energy = enemy.Cost * Consts.MissileHitRefundPct;
                    double hitPct = killable.CurDefenseValue / killable.MaxDefenseValue;

                    Tile[] tiles = [this.Piece.Tile, enemy.Tile];

                    double attMult = GetAttackMult(killable);
                    double a = Attack.Attack * attMult;
                    int att = Game.Rand.GaussianCappedInt(a, 1 / a, 1);
                    Attack attack = new(Piece, new(Attack.Type, att, Attack.Range));
                    fired = attack.Missile(killable);
                    if (fired)
                    {
                        _numMissiles--;

                        hitPct -= killable.CurDefenseValue / killable.MaxDefenseValue;
                        energy *= hitPct;

                        Values values = GetValues();
                        double attLoss = Consts.StatValue(attack.AttackMax);
                        attLoss = (attLoss - Consts.StatValue(attack.AttackCur)) / attLoss;
                        attLoss *= values.Energy + values.Mass * Consts.EnergyMassRatio;
                        energy += attLoss * attMult;

                        killable.Piece.Side.AddResources(energy);

                        double income = Piece.Game.Enemy.IncomeReference();
                        double mult = (energy + income) / income;

                        double spawnerMult = Math.Sqrt(mult);
                        foreach (Tile tile in Game.Rand.Iterate(tiles))
                            Piece.Game.Map.GetClosestSpawner(tile.Location).Spawner.Mult(Game.Rand.Range(1, spawnerMult));

                        mult *= mult;
                        foreach (Alien alien in Game.Rand.Iterate(Piece.Game.Enemy.PiecesOfType<Alien>()))
                            if (alien != enemy)
                            {
                                foreach (Tile tile in Game.Rand.Iterate(tiles))
                                    alien.MissileFired(tile, Game.Rand.Range(1, mult));
                            }
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

            double distance = Piece.Tile.GetDistance(killable.Piece.Tile) / GetValues().Attack.Range;
            if (distance < 1)
                mult = 1 + (1 - distance) * (Math.Sqrt(2) - 1);
            else
                mult = 1 / distance;

            if (!killable.Piece.HasBehavior<IMovable>())
                mult *= Consts.MissileAttImmobileMult;

            return mult;
        }
        //private Attack TempAttack() => new(Piece, Attack);

        void IBehavior.StartTurn()
        {
            //this._attacked = false; 
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
            double value = _numMissiles * Consts.MissileScrapRefund;
            return (values.Energy + values.Mass * Consts.EnergyMassRatio) * value;
        }

        [Serializable]
        [DataContract(IsReference = true)]
        private class Values : IUpgradeValues
        {
            private double _costMult, _range = START_RANGE, _rangeMult = 1;
            private int _energy, _mass, _att;

            public Values()
            {
                UpgradeMissileAttack(1);
                UpgradeMissileRange(1);
                UpgradeMissileCost(1);
            }

            public IAttacker.Values Attack => new(AttackType.Kinetic, _att, _range, 1);
            public int Energy => _energy;
            public int Mass => _mass;

            public void Upgrade(Research.Type type, double researchMult)
            {
                if (type == Research.Type.Missile)
                    UpgradeMissileAttack(researchMult);
                else if (type == Research.Type.MissileRange)
                    UpgradeMissileRange(researchMult);
                else if (type == Research.Type.MissileCost)
                    UpgradeMissileCost(researchMult);
            }
            private void UpgradeMissileAttack(double researchMult)
            {
                double attAvg = Calc(UpgType.MissileAttack, researchMult);
                this._att = Game.Rand.Round(attAvg);

                this._range /= _rangeMult;
                this._rangeMult = Math.Sqrt(Consts.StatValue(attAvg) / Consts.StatValue(_att));
                this._range *= _rangeMult;

                SetCost(researchMult);
            }
            private void UpgradeMissileRange(double researchMult)
            {
                this._range = Calc(UpgType.MissileRange, researchMult) * _rangeMult;
                SetCost(researchMult);
            }
            private void UpgradeMissileCost(double researchMult)
            {
                this._costMult = Calc(UpgType.MissileCost, researchMult);
                SetCost(researchMult);
            }
            private void SetCost(double researchMult)
            {
                double cost = MechBlueprint.MissileCost(Attack, researchMult);
                double rangeMult = Math.Sqrt(_range / START_RANGE);
                cost *= rangeMult * _costMult * Consts.MissileCostMult;

                double costE = cost * Consts.MissileEnergyCostRatio;
                this._energy = Game.Rand.GaussianCappedInt(costE, 1 / costE);

                double costM = (cost - _energy) / Consts.EnergyMassRatio;
                this._mass = Game.Rand.GaussianCappedInt(costM, 1 / costM);
            }
        }
    }
}
