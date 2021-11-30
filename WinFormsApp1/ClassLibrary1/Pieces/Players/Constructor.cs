using ClassLibrary1.Pieces.Terrain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1.Pieces.Players
{
    public class Constructor : PlayerPiece, IKillable, IMovable, IBuilder.IBuildExtractor
    {
        private static int numInc = 0;
        private readonly int num;

        private readonly IKillable killable;
        private readonly IMovable movable;
        private readonly IBuilder.IBuildExtractor buildExtractor;

        public Piece Piece => this;

        private Constructor(Map.Tile tile, double vision, IKillable.Values killable, IMovable.Values movable)
            : base(tile, vision)
        {
            this.num = numInc++;
            this.killable = new Killable(this, killable);
            this.movable = new Movable(this, movable);
            this.buildExtractor = new Builder.BuildExtractor(this);
            SetBehavior(this.killable, this.movable, this.buildExtractor);
        }
        internal static Constructor NewConstructor(Map.Tile tile)
        {
            double researchMult = tile.Map.Game.Player.GetResearchMult();

            double hits = 30;
            double moveInc = 3, moveMax = 7, moveLimit = 12;
            double armor = 0, shieldInc = 0, shieldMax = 0, shieldLimit = 0;
            bool defType = Game.Rand.Bool();
            if (defType)
                armor = .35;
            else
            {
                shieldInc = .5;
                shieldMax = 15;
                shieldLimit = 30;
            }

            researchMult = Math.Pow(researchMult, .3);
            hits *= researchMult;
            if (armor > 0)
                armor = 1 - (1 - armor) / researchMult;
            shieldInc *= researchMult;
            shieldMax *= researchMult;
            shieldLimit *= researchMult;
            moveInc *= researchMult;
            moveMax *= researchMult;
            moveLimit *= researchMult;

            Constructor obj = new(tile, 4.5, new(hits, armor, shieldInc, shieldMax, shieldLimit), new(moveInc, moveMax, moveLimit));
            tile.Map.Game.AddPiece(obj);
            return obj;
        }
        public static void Cost(Game game, out double energy, out double mass)
        {
            double researchMult = Math.Pow(game.Player.GetResearchMult(), .4);
            energy = 750 / researchMult;
            mass = 750 / researchMult;
        }

        public override void GenerateResources(ref double energyInc, ref double energyUpk, ref double massInc, ref double massUpk, ref double researchInc, ref double researchUpk)
        {
            base.GenerateResources(ref energyInc, ref energyUpk, ref massInc, ref massUpk, ref researchInc, ref researchUpk);
            energyUpk += Consts.BaseConstructorUpkeep;
        }

        public override string ToString()
        {
            return "Constructor " + num;
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

        #region IBuilding 

        public Extractor Build(Resource resource)
        {
            return buildExtractor.Build(resource);
        }

        #endregion IBuilding

    }
}
