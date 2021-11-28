using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MattUtil;
using ClassLibrary1.Pieces;

namespace ClassLibrary1.Pieces.Players
{
    [Serializable]
    public class Mech : PlayerPiece, IKillable, IAttacker, IMovable
    {
        private readonly IKillable killable;
        private readonly IAttacker attacker;
        private readonly IMovable movable;

        public Piece Piece => this;

        private Mech(Game game, Map.Tile tile, double vision, IKillable.Values killable, List<IAttacker.Values> attacks, IMovable.Values movable)
            : base(game, tile, vision)
        {
            this.killable = new Killable(this, killable);
            this.attacker = new Attacker(this, attacks);
            this.movable = new Movable(this, movable);
            SetBehavior(this.killable, this.attacker, this.movable);
        }
        internal static Mech NewMech(Game game, Map.Tile tile, double vision, IKillable.Values killable, List<IAttacker.Values> attacks, IMovable.Values movable)
        {
            Mech obj = new(game, tile, vision, killable, attacks, movable);
            game.AddPiece(obj);
            return obj;
        }

        public override void GenerateResources(ref double energyInc, ref double energyUpk, ref double massInc, ref double massUpk, ref double researchInc, ref double researchUpk)
        {
            base.GenerateResources(ref energyInc, ref energyUpk, ref massInc, ref massUpk, ref researchInc, ref researchUpk); 
            energyUpk += Consts.BaseMechUpkeep;
        } 

        public override string ToString()
        {
            return "Mech";
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

        #endregion IKillable

        #region IAttacker

        public ReadOnlyCollection<Attacker.Attack> Attacks => attacker.Attacks;
        public bool Fire(IKillable killable)
        {
            return attacker.Fire(killable);
        }
        bool IAttacker.EnemyFire(IKillable killable)
        {
            return false;
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
            return false;
        }

        #endregion IMovable
    }
}
