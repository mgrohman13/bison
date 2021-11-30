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
        private static int numInc = 0;
        private readonly int num;

        private readonly IKillable killable;
        private readonly IAttacker attacker;
        private readonly IMovable movable;

        public Piece Piece => this;

        private Alien(Map.Tile tile, IKillable.Values killable, List<IAttacker.Values> attacks, IMovable.Values movable)
            : base(tile)
        {
            this.num = numInc++;
            this.killable = new Killable(this, killable);
            this.attacker = new Attacker(this, attacks);
            this.movable = new Movable(this, movable);
            SetBehavior(this.killable, this.attacker, this.movable);
        }
        internal static Alien NewAlien(Map.Tile tile, IKillable.Values killable, List<IAttacker.Values> attacks, IMovable.Values movable)
        {
            Alien obj = new(tile, killable, attacks, movable);
            tile.Map.Game.AddPiece(obj);
            return obj;
        }

        public override string ToString()
        {
            return "Alien " + num;
        }

        #region IKillable

        public double HitsCur => killable.HitsCur;
        public double HitsMax => killable.HitsMax;
        public double Armor => killable.Armor;
        public double ShieldCur => killable.ShieldCur;
        public double ShieldInc => killable.ShieldInc;
        public double ShieldMax => killable.ShieldMax;
        public double ShieldLimit => killable.ShieldLimit;
        public bool Dead => killable.Dead;

        double IKillable.GetInc()
        {
            return killable.GetInc();
        }

        void IKillable.Damage(ref double damage, ref double shieldDmg)
        {
            killable.Damage(ref damage, ref shieldDmg);
        }

        #endregion IKillable

        #region IAttacker

        public IReadOnlyCollection<Attacker.Attack> Attacks => attacker.Attacks;
        public bool Fire(IKillable killable)
        {
            return attacker.Fire(killable);
        }
        bool IAttacker.EnemyFire(IKillable killable)
        {
            return attacker.EnemyFire(killable);
        }

        #endregion IAttacker

        #region IMovable

        public double MoveCur => movable.MoveCur;
        public double MoveInc => movable.MoveInc;
        public double MoveMax => movable.MoveMax;
        public double MoveLimit => movable.MoveLimit;

        double IMovable.GetInc()
        {
            return movable.GetInc();
        }

        public bool Move(Map.Tile to)
        {
            return movable.Move(to);
        }
        bool IMovable.EnemyMove(Map.Tile to)
        {
            return movable.EnemyMove(to);
        }

        #endregion IMovable
    }
}
