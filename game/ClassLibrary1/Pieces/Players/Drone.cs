using ClassLibrary1.Pieces.Behavior;
using ClassLibrary1.Pieces.Behavior.Combat;
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

        private readonly int _baseDef, _baseTurns;
        private int _turns;
        private double _treasure = 1;

        public int Turns => _turns;

        private Drone(Tile tile, Values values, double cost)
            : base(tile, Attack.MELEE_RANGE)
        {
            double defMult = Game.Rand.GaussianCapped(1, .078, .78);
            double repairMult = Game.Rand.GaussianCapped(1, .117, .65);

            Killable killable = new(this, [values.GetKillable(defMult)], Values.Resilience);
            Repair repair = new(this, values.GetRepair(repairMult));
            Movable movable = new(this, values.GetMovable(repair.RateBase), 0);
            SetBehavior(killable, repair, movable);

            this._baseDef = killable.Hits.DefenseMax;
            this._turns = values.GetTurns(_baseDef);
            this._baseTurns = _turns;
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
            if (HasBehavior(out IKillable killable))
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
        internal override void Cost(out int energy, out int mass) =>
            Cost(Game, out energy, out mass);

        internal override void OnResearch(Research.Type type)
        {
        }
        private static Values GetValues(Game game) => game.Player.GetUpgradeValues<Values>();

        internal override void StartTurn()
        {
            base.StartTurn();

            IRepair repair = GetBehavior<IRepair>();
            if (repair.Repaired)
                MultTreasure(1, _baseTurns);

            this._turns--;
            if (Turns < 1)
            {
                Die();
            }
            else
            {
                IKillable killable = GetBehavior<IKillable>();
                double mult = Turns / (Turns + 1.0);
                int def = Math.Max(1, Game.Rand.Round(Consts.StatValueInverse(Consts.StatValue(killable.Hits.DefenseCur) * mult)));
                killable.SetHits(def, killable.Hits.DefenseMax + def - killable.Hits.DefenseCur);

                base.StartTurn();
            }
        }
        private void Killable_DamagedEvent(object sender, Killable.DamagedEventArgs e)
        {
            IKillable killable = GetBehavior<IKillable>();
            if (!killable.Dead)
            {
                int def = killable.Hits.DefenseCur;

                double baseDef = Consts.StatValue(_baseDef);
                MultTreasure(def + 1, baseDef);

                double mult = Consts.StatValue(def) / Consts.StatValue(def + 1);
                this._turns = Math.Max(Math.Min(2, Turns), Game.Rand.Round(Turns * mult));
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

            private int energy, mass;
            private double turns, hits, repairRate, moveInc, moveMax, moveLimit, costMult;

            public Values()
            {
                UpgradeConstructorCost(1);
                UpgradeConstructorDefense(1);
                UpgradeConstructorMove(1);
                UpgradeRepairDrone(1);
            }
            public int Energy => energy;
            public int Mass => mass;

            public IKillable.Values GetKillable(double defMult)
            {
                int def = Game.Rand.Round(this.hits * defMult);
                def = Math.Max(def, 1);
                return new(DefenseType.Hits, def);
            }
            public IRepair.Values GetRepair(double repairMult)
            {
                int rate = Game.Rand.Round(this.repairRate * repairMult);
                rate = Math.Max(rate, 1);
                return new(new(Attack.MELEE_RANGE), rate);
            }
            public IMovable.Values GetMovable(int repair)
            {
                return Constructor.GetMove(this.repairRate / repair, this.moveInc, this.moveMax, this.moveLimit,
                    Game.Rand.NextDouble(), Game.Rand.NextDouble());
            }
            public int GetTurns(int defense)
            {
                double mult = Math.Sqrt(Consts.StatValue(this.hits) / Consts.StatValue(defense));
                return Game.Rand.Round(turns * Math.Sqrt(mult));
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
                this.costMult = ResearchUpgValues.Calc(UpgType.DroneCost, researchMult);
                SetCost();
            }
            private void UpgradeConstructorDefense(double researchMult)
            {
                this.hits = ResearchUpgValues.Calc(UpgType.DroneDefense, researchMult);
                SetCost();
            }
            private void UpgradeConstructorMove(double researchMult)
            {
                double move = ResearchUpgValues.Calc(UpgType.DroneMove, researchMult);
                this.moveInc = move;
                this.moveMax = move + 1.5;
                this.moveLimit = move * 1.69;
                SetCost();
            }
            private void UpgradeRepairDrone(double researchMult)
            {
                this.turns = ResearchUpgValues.Calc(UpgType.DroneTurns, researchMult);
                this.repairRate = ResearchUpgValues.Calc(UpgType.DroneRepair, researchMult);
                SetCost();
            }
            private void SetCost()
            {
                double turn = turns * 3.9 + Math.Sqrt(Consts.StatValue(hits));
                double repair = repairRate * 3.9 + Consts.MoveValue(moveInc, moveMax, moveLimit);

                double costE = Consts.DroneCost * costMult * Math.Sqrt(turn * repair);
                this.energy = Game.Rand.GaussianCappedInt(costE + 1, 1 / costE, 1);

                double costM = costE * Consts.DroneMassCostMult + (costE - energy) / Consts.EnergyMassRatio;
                this.mass = Game.Rand.GaussianInt(costM, 1 / costM);
                if (this.mass < 0)
                {
                    this.energy += Game.Rand.Round(this.mass * Consts.EnergyMassRatio);
                    this.mass = 0;
                }
            }
        }
    }
}
