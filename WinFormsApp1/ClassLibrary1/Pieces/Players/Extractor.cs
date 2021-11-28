using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using MattUtil;
using ClassLibrary1.Pieces;
using ClassLibrary1.Pieces.Terrain;

namespace ClassLibrary1.Pieces.Players
{
    public class Extractor : PlayerPiece, IKillable
    {
        private readonly IKillable killable;

        public readonly Resource Resource;
        public Piece Piece => this;

        private Extractor(Resource Resource)
            : base(Resource.Game, Resource.Tile, 5)
        {
            this.killable = new Killable(this, new(50));
            SetBehavior(this.killable);

            this.Resource = Resource;
        }
        internal static Extractor NewExtractor(Resource Resource)
        {
            Resource.Game.RemovePiece(Resource);

            Extractor obj = new(Resource);
            Resource.Game.AddPiece(obj);
            return obj;
        }

        public override void GenerateResources(ref double energyInc, ref double energyUpk, ref double massInc, ref double massUpk, ref double researchInc, ref double researchUpk)
        {
            base.GenerateResources(ref energyInc, ref energyUpk, ref massInc, ref massUpk, ref researchInc, ref researchUpk);
            Resource.GenerateResources(ref energyInc, ref energyUpk, ref massInc, ref massUpk, ref researchInc, ref researchUpk);
        }         

        public override string ToString()
        {
            string type;
            if (Resource is Biomass)
                type = "Energy";
            else if (Resource is Metal)
                type = "Mass";
            else if (Resource is Artifacts)
                type = "Research";
            else
                throw new Exception();
            return type + " Extractor";
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
    }
}
