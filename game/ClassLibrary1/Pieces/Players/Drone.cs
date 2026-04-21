using ClassLibrary1.Pieces.Behavior;
using ClassLibrary1.Pieces.Behavior.Combat;
using ClassLibrary1.Pieces.Terrain;
using System;
using System.Runtime.Serialization;
using DefenseType = ClassLibrary1.Pieces.Behavior.Combat.CombatTypes.DefenseType;
using Tile = ClassLibrary1.Map.Map.Tile;
using UpgType = ClassLibrary1.ResearchUpgValues.UpgType;

namespace ClassLibrary1.Pieces.Players
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class Drone : PlayerPiece, IDeserializationCallback
    {
        public static double Resilience => Values.Resilience;

        private int _turns;
        private readonly int _baseTurns, _baseDef;
        private double _treasure = 1;

        public int Turns => _turns;

        private readonly IKillable killable;
        private readonly IRepair repair;

        private Drone(Tile tile, Values values, double cost)
            : base(tile, Attack.MELEE_RANGE)
        {
            double defMult = Game.Rand.GaussianCapped(1, .078, .65);
            double moveMult = Game.Rand.GaussianCapped(1, .117, .65);

            this.killable = new Killable(this, [values.GetKillable(defMult)], Values.Resilience);
            this.repair = new Repair(this, values.GetRepair(moveMult));
            SetBehavior(
                killable,
                new Movable(this, values.GetMovable(repair.RateBase), 0),
                repair
            );

            this._turns = values.GetTurns(killable.Hits.DefenseMax);
            this._baseTurns = Turns;
            this._baseDef = killable.Hits.DefenseMax;
            this._treasure = cost * Consts.DroneRefund;

            OnDeserialization(this);
        }
        internal static Drone NewDrone(Tile tile)
        {
            Cost(tile.Map.Game, out int energy, out int mass);
            double cost = energy + mass * Consts.EnergyMassRatio;

            Drone obj = new(tile, GetValues(tile.Map.Game), cost);
            tile.Map.Game.AddPiece(obj);
            return obj;
        }
        public void OnDeserialization(object sender)
        {
            //base.OnDeserialization(sender);
            if (killable != null)
            {
                ((Killable)killable).OnDeserialization(this);
                killable.Event.DamagedEvent += Killable_DamagedEvent;
            }
        }

        public static void Cost(Game game, out int energy, out int mass)
        {
            Values values = GetValues(game);
            energy = values.Energy;
            mass = values.Mass;
        }

        internal override void OnResearch(Research.Type type)
        {
        }
        private static Values GetValues(Game game)
        {
            return game.Player.GetUpgradeValues<Values>();
        }

        internal override void StartTurn()
        {
            if (repair.Repaired)
                MultTreasure(1, _baseTurns);

            this._turns--;
            if (Turns < 1)
            {
                Die();
            }
            else
            {
                double mult = Turns / (Turns + 1.0);
                int def = Math.Max(1, Game.Rand.Round(Consts.StatValueInverse(Consts.StatValue(killable.Hits.DefenseCur) * mult)));
                killable.SetHits(def, killable.Hits.DefenseMax + def - killable.Hits.DefenseCur);

                base.StartTurn();
            }
        }
        private void Killable_DamagedEvent(object sender, Killable.DamagedEventArgs e)
        {
            if (!killable.Dead)
            {
                int def = killable.Hits.DefenseCur;

                double baseDef = Consts.StatValue(_baseDef);
                MultTreasure(def + 1, baseDef);

                double mult = Consts.StatValue(def) / Consts.StatValue(def + 1);
                this._turns = Math.Max(Math.Min(2, Turns), Game.Rand.Round(Turns * mult));

                killable.SetHits(def, killable.Hits.DefenseMax);
            }
        }

        private void MultTreasure(double decrement, double max)
        {
            double mult = (max - decrement) / max;
            double diff = Math.Min(mult, 1 - mult);
            double value = _treasure * mult;
            this._treasure = Game.Rand.GaussianCapped(value, diff * .169);
        }

        internal override void Die(out Tile tile, out double treasure)
        {
            base.Die(out tile, out treasure); 
            treasure += _treasure;
        }

        internal override void GetUpkeep(ref double energyUpk, ref double massUpk)
        {
            base.GetUpkeep(ref energyUpk, ref massUpk);
            energyUpk += Consts.BaseDroneUpkeep;
        }
        internal override void EndTurn(ref double energyUpk, ref double massUpk)
        {
            base.EndTurn(ref energyUpk, ref massUpk);
            energyUpk += Consts.BaseDroneUpkeep;
        }

        public override string ToString()
        {
            return "Drone " + PieceNum;
        }

        [Serializable]
        [DataContract(IsReference = true)]
        private class Values : IUpgradeValues
        {
            public const double Resilience = .7;

            private int _energy, _mass;

            private double _turns, _hits, _repairRate, _moveInc, _moveMax, _moveLimit, _costMult;//, energyRounding, massRounding;
            public Values()
            {
                UpgradeConstructorCost(1);
                UpgradeConstructorDefense(1);
                UpgradeConstructorMove(1);
                UpgradeRepairDrone(1);
                //energyRounding = massRounding = .5;
            }
            public int Energy => _energy;
            public int Mass => _mass;

            public int GetTurns(int defense)
            {
                double mult = Math.Sqrt(Consts.StatValue(_hits) / Consts.StatValue(defense));
                return Game.Rand.Round(_turns * mult);
            }
            public IKillable.Values GetKillable(double defMult)
            {
                int def = Game.Rand.Round(_hits * defMult);
                return new(DefenseType.Hits, def);
            }
            public IMovable.Values GetMovable(int repair)
            {
                double mult = _repairRate / repair;
                mult *= mult;

                double avgMax = _moveMax * mult;
                double avgLimit = _moveLimit * mult;
                int max = Game.Rand.Round(avgMax);
                int limit = Game.Rand.Round(avgLimit);
                if (max >= limit)
                    limit = max + 1;

                bool loop;
                double inc;
                do
                {
                    double m = mult * Math.Sqrt(avgLimit / limit * avgMax / max);
                    inc = _moveInc * Math.Sqrt(m);

                    loop = inc >= max;
                    if (loop)
                    {
                        max++;
                        limit++;
                    }
                } while (loop);

                return new IMovable.Values(inc, max, limit);
            }
            public IRepair.Values GetRepair(double moveMult)
            {
                int rate = Math.Max(1, Game.Rand.Round(_repairRate / moveMult));
                return new(new(Attack.MELEE_RANGE), rate);
            }

            public void Upgrade(Research.Type type, double researchMult)
            {
                if (type == Research.Type.ConstructorCost)
                    UpgradeConstructorCost(researchMult);
                else if (type == Research.Type.ConstructorDefense)
                    UpgradeConstructorDefense(researchMult);
                else if (type == Research.Type.ConstructorMove)
                    UpgradeConstructorMove(researchMult);
                else if (type == Research.Type.RepairDrone)
                    UpgradeRepairDrone(researchMult);
            }

            private void UpgradeConstructorCost(double researchMult)
            {
                this._costMult = ResearchUpgValues.Calc(UpgType.DroneCost, researchMult);
                SetCost();
            }

            private void UpgradeConstructorDefense(double researchMult)
            {
                this._hits = ResearchUpgValues.Calc(UpgType.DroneDefense, researchMult);
                SetCost();
            }
            private void UpgradeConstructorMove(double researchMult)
            {
                double move = ResearchUpgValues.Calc(UpgType.DroneMove, researchMult);
                this._moveInc = move;
                this._moveMax = move + 1.5;
                this._moveLimit = move * 1.69;
                SetCost();
            }
            private void UpgradeRepairDrone(double researchMult)
            {
                this._turns = ResearchUpgValues.Calc(UpgType.DroneTurns, researchMult);
                this._repairRate = ResearchUpgValues.Calc(UpgType.DroneRepair, researchMult);
                SetCost();
            }

            private void SetCost()
            {
                double turn = _turns * 3.9 + Math.Sqrt(Consts.StatValue(_hits));
                double repair = _repairRate * 3.9 + Consts.MoveValue(_moveInc, _moveMax, _moveLimit);

                double costE = Consts.DroneCost * _costMult * Math.Sqrt(turn * repair);
                this._energy = Game.Rand.GaussianCappedInt(costE, 1 / costE);

                double costM = costE * Consts.DroneMassCostMult + (costE - _energy) / Consts.EnergyMassRatio;
                this._mass = Game.Rand.GaussianCappedInt(costM, 1 / costM);
            }
        }
    }
}
