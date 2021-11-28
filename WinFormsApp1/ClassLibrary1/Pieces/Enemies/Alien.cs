using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using MattUtil;
using ClassLibrary1.Pieces;
using System.Collections.ObjectModel;

namespace ClassLibrary1.Pieces.Enemies
{
    public class Alien : EnemyPiece, IKillable, IAttacker, IMovable
    {
        private readonly IKillable killable;
        private readonly IAttacker attacker;
        private readonly IMovable movable;

        public Piece Piece => this;

        private Alien(Game game, Map.Tile tile, IKillable.Values killable, List<IAttacker.Values> attacks, IMovable.Values movable) : base(game, tile)
        {
            this.killable = new Killable(this, killable);
            this.attacker = new Attacker(this, attacks);
            this.movable = new Movable(this, movable);
        }
        internal static Alien NewAlien(Game game, Map.Tile tile, IKillable.Values killable, List<IAttacker.Values> attacks, IMovable.Values movable)
        {
            Alien obj = new(game, tile, killable, attacks, movable);
            game.AddPiece(obj);
            return obj;
        }

        internal override void EndTurn()
        {
            base.EndTurn();
            killable.EndTurn();
            attacker.EndTurn();
            movable.EndTurn();
        }

        public override string ToString()
        {
            return "Alien";
        }

        #region IKillable

        public double HitsCur => killable.HitsCur;
        public double HitsMax => killable.HitsMax;
        public double Armor => killable.Armor;
        public double ShieldCur => killable.ShieldCur;
        public double ShieldInc => killable.ShieldInc;
        public double ShieldMax => killable.ShieldMax;
        public double ShieldLimit => killable.ShieldLimit;
        void IKillable.Damage(ref double damage, ref double shieldDmg)
        {
            killable.Damage(ref damage, ref shieldDmg);
        }
        void IKillable.EndTurn()
        {
            EndTurn();
        }

        #endregion IKillable

        #region IAttacker

        public ReadOnlyCollection<Attacker.Attack> Attacks => attacker.Attacks;
        public bool Fire(IKillable killable)
        {
            return attacker.Fire(killable);
        }
        bool IAttacker.EnemyFire(IKillable killable)
        {
            return attacker.EnemyFire(killable);
        }
        void IAttacker.EndTurn()
        {
            EndTurn();
        }

        #endregion IAttacker

        #region IMovable

        public double MoveCur => movable.MoveCur;
        public double MoveInc => movable.MoveInc;
        public double MoveMax => movable.MoveMax;
        public double MoveLimit => movable.MoveLimit;

        public bool Move(Map.Tile to)
        {
            return movable.Move(to);
        }
        bool IMovable.EnemyMove(Map.Tile to)
        {
            return movable.EnemyMove(to);
        }
        void IMovable.EndTurn()
        {
            EndTurn();
        }

        #endregion IMovable
    }
}
