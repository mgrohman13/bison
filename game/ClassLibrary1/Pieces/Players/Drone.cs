using ClassLibrary1.Pieces.Terrain;
using MattUtil;
using System;
using System.Runtime.Serialization;
using DefenseType = ClassLibrary1.Pieces.CombatTypes.DefenseType;
using Tile = ClassLibrary1.Map.Map.Tile;
using UpgType = ClassLibrary1.ResearchUpgValues.UpgType;

namespace ClassLibrary1.Pieces.Players
{
    [Serializable]
    public class Drone : PlayerPiece, IDeserializationCallback
    {
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

            this.killable = new Killable(this, new[] { values.GetKillable(defMult) }, 1);
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
            GetValues(game).GetCost(out energy, out mass);
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
                // use stat value??
                int def = Math.Max(1, Game.Rand.Round(killable.Hits.DefenseCur * mult));
                killable.SetHits(def, def);

                base.StartTurn();
            }
        }
        private void Killable_DamagedEvent(object sender, Killable.DamagedEventArgs e)
        {
            if (!killable.Dead)
            {
                int def = killable.Hits.DefenseCur;
                int max = killable.Hits.DefenseMax;

                // use StatValueDiff
                double baseDef = Consts.StatValue(_baseDef);
                MultTreasure(def + 1, baseDef / 2.0);

                double mult = Consts.StatValue(def) / Consts.StatValue(max);
                this._turns = Math.Max(Math.Min(2, Turns), Game.Rand.Round(Turns * mult));

                killable.SetHits(def, def);
            }
        }

        private void MultTreasure(double decrement, double max)
        {
            double mult = (max - decrement) / max;
            double diff = Math.Min(mult, 1 - mult);
            double value = _treasure * mult;
            this._treasure = Game.Rand.GaussianCapped(value, diff * .169);
        }

        internal override void Die()
        {
            Tile tile = this.Tile;
            base.Die();
            Treasure.NewTreasure(tile, _treasure);
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
        private class Values : IUpgradeValues
        {
            private double turns, hits, repairRate, moveInc, moveMax, moveLimit, costMult, energyRounding, massRounding;
            public Values()
            {
                UpgradeConstructorCost(1);
                UpgradeConstructorDefense(1);
                UpgradeConstructorMove(1);
                UpgradeRepairDrone(1);
                energyRounding = massRounding = .5;
            }

            public void GetCost(out int energy, out int mass)
            {
                double turn = turns * 2.1 + Math.Sqrt(Consts.StatValue(hits));
                double repair = repairRate * 3.9 + Consts.MoveValue(moveInc, moveMax, moveLimit);

                double cost = Consts.DroneCost * costMult * Math.Sqrt(turn * repair);

                energy = MTRandom.Round(cost, energyRounding);
                mass = MTRandom.Round(cost * Consts.DroneMassCostMult, massRounding);
            }

            public int GetTurns(int defense)
            {
                double mult = Math.Sqrt(Consts.StatValue(hits) / Consts.StatValue(defense));
                return Game.Rand.Round(this.turns * mult);
            }
            public IKillable.Values GetKillable(double defMult)
            {
                int def = Game.Rand.Round(hits * defMult);
                return new(DefenseType.Hits, def);
            }
            public IMovable.Values GetMovable(int repair)
            {
                double mult = repairRate / repair;
                mult *= mult;

                double avgMax = moveMax * mult;
                double avgLimit = moveLimit * mult;
                int max = Game.Rand.Round(avgMax);
                int limit = Game.Rand.Round(avgLimit);
                if (max >= limit)
                    limit = max + 1;

                bool loop;
                double inc;
                do
                {
                    double m = mult * Math.Sqrt(avgLimit / limit * avgMax / max);
                    inc = moveInc * Math.Sqrt(m);

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
                int rate = Math.Max(1, Game.Rand.Round(repairRate / moveMult));
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
                this.costMult = ResearchUpgValues.Calc(UpgType.DroneCost, researchMult);
                SetCostRounding();
            }

            private void UpgradeConstructorDefense(double researchMult)
            {
                this.hits = ResearchUpgValues.Calc(UpgType.DroneDefense, researchMult);
                SetCostRounding();
            }
            private void UpgradeConstructorMove(double researchMult)
            {
                double move = ResearchUpgValues.Calc(UpgType.DroneMove, researchMult);
                this.moveInc = move;
                this.moveMax = move + 1.5;
                this.moveLimit = move * 1.69;
                SetCostRounding();
            }
            private void UpgradeRepairDrone(double researchMult)
            {
                this.turns = ResearchUpgValues.Calc(UpgType.DroneTurns, researchMult);
                this.repairRate = ResearchUpgValues.Calc(UpgType.DroneRepair, researchMult);
                SetCostRounding();
            }

            private void SetCostRounding()
            {
                this.energyRounding = Game.Rand.NextDouble();
                this.massRounding = Game.Rand.NextDouble();
            }
        }
    }
}
