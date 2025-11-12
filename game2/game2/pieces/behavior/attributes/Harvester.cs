using game2.game;

namespace game2.pieces.behavior.attributes
{
    public class Harvester(Piece piece, int bonus) : IBehavior
    {
        private readonly Piece _piece = piece;
        public Piece Piece => _piece;

        //private int _bonus = bonus;
        private readonly int _bonusBase = bonus;
        //public int Bonus => _bonus;
        public int BonusBase => _bonusBase;

        Piece IBehavior.Piece => throw new NotImplementedException();

        void IBehavior.Wound(float woundPct)
        { }
        //=> _bonus = Piece.Tile.Map.Game.Consts.Wound(woundPct, _bonus, BonusBase);

        Resources IBehavior.GetTurnEnd()
        {
            //multiple harvesters??
            Resources r = new();
            foreach (var tile in Game.Rand.Iterate(Piece.Tile.GetNeighbors()))
                if (tile.Piece is Resource resource)
                {
                    r += resource.GetGenerate();// Bonus);
                    return r;
                }
            return r;
        }
        void IBehavior.EndTurn(ref Resources resources)
        {
            foreach (var tile in Game.Rand.Iterate(Piece.Tile.GetNeighbors()))
                if (tile.Piece is Resource resource)
                    resource.DoGenerate(ref resources);//, Bonus);
        }

        void IBehavior.StartTurn()
        {
        }
    }
}
