using game2.map;
using game2.pieces;
using game2.pieces.enemy;
using game2.pieces.player;
using game2.sides;
using MattUtil;

namespace game2.game
{
    public class Game
    {
        public static readonly MTRandom Rand;
        static Game()
        {
            Rand = new();
            Rand.StartTick();
        }

        public readonly Consts Consts;
        public readonly Map Map;
        public readonly Player Player;
        public readonly Enemy Enemy;

        private int _turn;

        public Game()
        {
            Consts = new();
            Map = new Map(this);
            Player = new(this);
            Enemy = new(this);

            _turn = 1;

            StartGame();
        }
        private void StartGame()
        {
            Player.StartGame();
            Map.StartGame();
        }

        internal void AddPiece(Piece piece)
        {
            Map.AddPiece(piece);

            if (piece is PlayerPiece playerPiece)
                Player.AddPiece(playerPiece);
            else if (piece is EnemyPiece enemyPiece)
                Enemy.AddPiece(enemyPiece);
        }
        internal void RemovePiece(Piece piece)
        {
            Map.RemovePiece(piece);
            piece.Side?.RemovePiece(piece);
        }

        public void EndTurn()//Action<Tile, double> UpdateProgress)
        {
            //if (this.GameOver)
            //    return null;

            //Player.GenerateResources(out double energyInc, out double massInc, out double researchInc);

            //Research.Type? researched =
            Player.EndTurn();
            _turn++;
            //Map.PlayTurn(Turn);
            //Enemy.PlayTurn(UpdateProgress, Enemy.EnergyEquivalent(energyInc, massInc, researchInc));

            foreach(var piece in Rand.Iterate(Map.Pieces.Where(p=>p.Side == null)))
                piece.StartTurn();
            Player.StartTurn();

            //SaveGame();
            //return researched;
        }
    }
}
