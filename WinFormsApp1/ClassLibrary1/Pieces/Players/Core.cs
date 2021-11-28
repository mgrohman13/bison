﻿using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using MattUtil;
using ClassLibrary1.Pieces;

namespace ClassLibrary1.Pieces.Players
{
    [Serializable]
    public class Core : PlayerPiece, IKillable, IBuilder
    {
        private readonly IKillable killable;
        private readonly IBuilder builder;

        public Piece Piece => this;

        private Core(Game game)
            : base(game, game.Map.GetTile(0, 0), 2.5)
        {
            killable = new Killable(this, new(100, .25, 1, 100, 300));
            builder = new Builder(this);
            SetBehavior(this.killable, this.builder);
        }
        internal static Core NewCore(Game game)
        {
            Core obj = new(game);
            game.AddPiece(obj);
            return obj;
        }

        public override void GenerateResources(ref double energyInc, ref double energyUpk, ref double massInc, ref double massUpk, ref double researchInc, ref double researchUpk)
        {
            base.GenerateResources(ref energyInc, ref energyUpk, ref massInc, ref massUpk, ref researchInc, ref researchUpk);
            energyInc += 1000;
            massInc += 100;
            researchInc += 50;
        }

        public override string ToString()
        {
            return "Core";
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

        #region IBuilding

        public void Build(ISide side, Map.Tile tile, double vision, IKillable.Values killable, List<IAttacker.Values> attacks, IMovable.Values movable)
        {
            builder.Build(side, tile, vision, killable, attacks, movable);
        } 

        #endregion IBuilding
    }
}
