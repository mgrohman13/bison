using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using MattUtil;
using ClassLibrary1.Pieces;

namespace ClassLibrary1.Pieces.Players
{
    [Serializable]
    public class Core : PlayerPiece, IKillable, IBuilding
    {
        private readonly IKillable killable;
        private readonly IBuilding building;

        public Piece Piece => this;

        private Core(Game game) : base(game, game.Map.GetTile(0, 0), 3.9)
        {
            killable = new Killable(this, new(100, .25, 1, 100, 300));
            building = new Building(this);
        }
        internal static Core NewCore(Game game)
        {
            Core obj = new(game);
            game.AddPiece(obj);
            return obj;
        }
        internal override void EndTurn()
        {
            base.EndTurn();
            killable.EndTurn();
            building.EndTurn();
        }

        #region IKillable

        public double HitsCur => killable.HitsCur;
        public double HitsMax => killable.HitsMax;
        public double Armor => killable.Armor;
        public double ShieldCur => killable.ShieldCur;
        public double ShieldInc => killable.ShieldInc;
        public double ShieldMax => killable.ShieldMax;
        public double ShieldLimit => killable.ShieldLimit;
        void IKillable.Damage(double damage, double shieldDmg)
        {
            killable.Damage(damage, shieldDmg);
        }
        void IKillable.EndTurn()
        {
            EndTurn();
        }

        #endregion IKillable

        #region IBuilding

        public void Build(ISide side, Map.Tile tile, double vision, IKillable.Values killable, List<IAttacker.Values> attacks, IMovable.Values movable)
        {
            building.Build(side, tile, vision, killable, attacks, movable);
        }

        void IBuilding.EndTurn()
        {
            EndTurn();
        }

        #endregion IBuilding
    }
}
