using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using MattUtil;
using ClassLibrary1.Pieces;

namespace ClassLibrary1.Pieces.Players
{
    [Serializable]
    public class Core : PlayerPiece, IKillable, IBuilder.IBuildConstructor, IBuilder.IBuildMech
    {
        private readonly IKillable killable;
        private readonly IBuilder.IBuildConstructor buildConstructor;
        private readonly IBuilder.IBuildMech buildMech;

        public Piece Piece => this;

        private Core(Game game)
            : base(game.Map.GetTile(0, 0), 4)
        {
            killable = new Killable(this, new(100, .25, 1, 100, 300));
            buildConstructor = new Builder.BuildConstructor(this);
            buildMech = new Builder.BuildMech(this);
            SetBehavior(this.killable, this.buildConstructor, this.buildMech);
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
            energyInc += 250;
            massInc += 100;
            researchInc += 25;
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

        #region IBuilding 

        public Constructor Build(Map.Tile tile)
        {
            return buildConstructor.Build(tile);
        }
        public Mech Build(Map.Tile tile, MechBlueprint blueprint)
        {
            return buildMech.Build(tile, blueprint);
        }

        #endregion IBuilding
    }
}
