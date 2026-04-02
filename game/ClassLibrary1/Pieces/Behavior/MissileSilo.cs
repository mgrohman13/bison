using ClassLibrary1.Pieces.Behavior.Combat;
using ClassLibrary1.Pieces.Enemies;
using ClassLibrary1.Pieces.Players;
using System;
using System.Collections.Generic;
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
        //TODO
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

        //TODO
        public double GetAttack(IKillable killable)
        {
            int att = GetValues().Attack.Attack;

            double totalAtt = 0, totalWeight = 0;

            var defenders = GetDefenders(killable);
            if (defenders.Count > 1)
                ;
            foreach (var pair in defenders)
            {
                totalAtt += att * GetAttackMult(killable) * pair.Value;
                totalWeight += pair.Value;
            }

            return totalAtt / totalWeight;
        }

        //TODO
        public bool Fire(IKillable killable)
        {
            bool fired = false;

            if (((IMissileSilo)this).Online)
            {
                //no range or CanAttack checks
                var defenders = GetDefenders(killable);
                if (defenders.Count > 0)
                {
                    killable = Game.Rand.SelectValue(defenders);
                    EnemyPiece enemy = killable.Piece as EnemyPiece;
                    if (enemy is not null)
                    {
                        double energy = enemy.Cost;
                        double hitPct = killable.CurDefenseValue / killable.MaxDefenseValue;

                        Tile[] tiles = [this.Piece.Tile, enemy.Tile];

                        Attack attack = new(Piece, Attack);
                        fired = attack.Missile(killable, GetAttackMult(killable));
                        if (fired)
                        {
                            _numMissiles--;

                            hitPct -= killable.CurDefenseValue / killable.MaxDefenseValue;
                            energy *= hitPct;
                            Piece.Game.Enemy.AddEnergy(energy); // full amt???

                            double income = Piece.Game.Enemy.IncomeReference();
                            double mult = (energy + income) / income;

                            double spawnerMult = Math.Sqrt(mult);
                            foreach (Tile tile in tiles)
                                Piece.Game.Map.GetClosestSpawner(tile.Location).Spawner.Mult(Game.Rand.Range(1, spawnerMult));

                            mult *= mult;
                            foreach (Alien a in Game.Rand.Iterate(Piece.Game.Enemy.PiecesOfType<Alien>()))
                                if (a != enemy)
                                {
                                    foreach (Tile tile in tiles)
                                        a.MissileFired(tile, Game.Rand.Range(1, mult));
                                }
                        }
                        else
                            ;
                    }
                    else
                        ;
                }
                else
                    ;
            } 

            return fired;
        }
        private Dictionary<IKillable, int> GetDefenders(IKillable killable) =>
            Combat.Attack.GetDefenders(Piece.Side, killable.Piece);

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

        [Serializable]
        [DataContract(IsReference = true)]
        private class Values : IUpgradeValues
        {
            private IAttacker.Values attack;
            private double costMult, attResearchMult;
            private int energy, mass;

            public Values()
            {
                attack = new(AttackType.Kinetic, 1, Combat.Attack.MELEE_RANGE, 1);

                UpgradeMissileAttack(1);
                UpgradeMissileRange(1);
                UpgradeMissileCost(1);
            }

            public IAttacker.Values Attack => attack;
            public int Energy => energy;
            public int Mass => mass;

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
                int att = Game.Rand.Round(attAvg);
                attack = new(attack.Type, att, attack.Range, 1);
                attResearchMult = researchMult;
                SetCost();
            }
            private void UpgradeMissileRange(double researchMult)
            {
                double range = Calc(UpgType.MissileRange, researchMult);
                attack = new(attack.Type, attack.Attack, range, 1);
                SetCost();
            }
            private void UpgradeMissileCost(double researchMult)
            {
                costMult = Calc(UpgType.MissileCost, researchMult);
                SetCost();
            }
            private void SetCost()
            {
                double cost = MechBlueprint.MissileCost(attack, attResearchMult);
                double rangeMult = Math.Sqrt(attack.Range / START_RANGE);
                cost *= rangeMult * costMult * Consts.MissileCostMult;

                double costE = cost * Consts.MissileEnergyCostRatio;
                energy = Game.Rand.GaussianCappedInt(costE, 1 / costE);

                double costM = (cost - energy) / Consts.EnergyMassRatio;
                mass = Game.Rand.GaussianCappedInt(costM, 1 / costM);
            }
        }
    }
}
