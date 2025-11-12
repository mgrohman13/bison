using game2.game;
using game2.map;
using game2.pieces.player;
using static game2.pieces.behavior.Movable;

namespace game2.pieces.behavior
{
    [Serializable]
    public class Movable(Piece piece, MoveType type, int moveCur, int moveInc, int moveMax) : IBehavior
    {
        private readonly Piece _piece = piece;
        public Piece Piece => _piece;
        public Game Game => Piece.Game;

        public readonly MoveType Type = type;

        private int _moveCur = moveCur, _moveInc = moveInc;
        private readonly int _moveMax = moveMax, _moveBase = moveInc;
        private bool _moved = true, _restrictMove;

        public int MoveCur => _moveCur;
        public int MoveInc => _moveInc;
        public int MoveMax => _moveMax;
        public int MoveBase => _moveBase;
        public bool Moved => _moved;
        public bool RestrictMove
        {
            get => _restrictMove;
            internal set
            {
                if (Moved || !value)
                    _restrictMove = value;
            }
        }

        public Movable(Piece piece, MoveType type, int moveInc, int moveMax) : this(piece, type, 0, moveInc, moveMax) { }

        void IBehavior.Wound(float woundPct) =>
            _moveInc = Game.Consts.Wound(woundPct, _moveInc, MoveBase);

        //bool IMovable.Move(Tile to)
        //{
        //    Tile from = Piece.Tile;
        //    //bool move = Piece.IsPlayer && to != null && to.Piece == null && Piece.Tile != to && CanMove;

        //    if (CanMoveTo(to) && to.Visible && Piece is PlayerPiece piece)
        //    {
        //        const double lineDist = .5;
        //        bool isX = from.X == to.X;
        //        double a = isX ? 0 : (to.Y - from.Y) / (double)(to.X - from.X);
        //        double c = to.Y - a * to.X;
        //        double b = -1;

        //        IOrderedEnumerable<MattUtil.Point> ps =
        //            Game.Rand.Iterate(Math.Min(from.X, to.X), Math.Max(from.X, to.X), Math.Min(from.Y, to.Y), Math.Max(from.Y, to.Y))
        //            .Where(p => isX || Map.Map.PointLineDistanceAbs(a, b, c, p) < lineDist)
        //            .OrderBy(from.GetDistance);

        //        bool stop = false;
        //        foreach (var p in  Game.Rand.Iterate(ps))
        //        {
        //            stop |= from.Map.UpdateVision(p, piece.Vision);
        //            Tile tile = from.Map.GetTile(p);
        //            if (stop && tile != null && tile.Piece == null)
        //            {
        //                to = tile;
        //                break;
        //            }
        //        }

        //        if (!Move(to))
        //            throw new Exception();
        //        return true;
        //    }
        //    return false;
        //}
        public bool Move(Tile to)
        {
            if (Piece is PlayerPiece && CanMoveTo(to))
            {
                int dist = MoveCost(to, out _);
                //if (valid)
                //{
                _moved = true;
                _moveCur -= dist;
                Piece.SetTile(to);
                return true;
                //}
            }
            return false;
        }
        public bool CanMoveTo(Tile to) =>
            CanMove && to.Piece == null && to.Visible && Piece.Tile.GetNeighbors().Contains(to)
                && MoveCost(to, out bool valid) <= MoveCur && valid;
        public bool CanMove => MoveCur > 0 && !RestrictMove;
        private int MoveCost(Tile to, out bool valid)
        {
            valid = to.Terrain != Terrain.Glacier;
            int cost = to.Terrain.MoveCost();
            if (valid)
                switch (Type)
                {
                    case MoveType.Ground:
                        valid = to.Terrain != Terrain.Sea && to.Terrain != Terrain.Kelp;
                        break;
                    case MoveType.Sea:
                        valid = to.Terrain == Terrain.Sea;
                        break;
                    case MoveType.Amphibious:
                        break;
                    case MoveType.Air:
                        cost = Math.Max(1, cost - 1);
                        break;
                    default:
                        throw new Exception();
                }
            return valid ? cost : int.MaxValue;
        }

        //bool IMovable.Port(Portal portal)
        //{
        //    if (portal.CanPort(this, out Portal exit, out double dist))
        //    {
        //        _moveCur -= dist;
        //        _moved = true;
        //        if (Piece.HasBehavior(out IAttacker attacker))
        //            attacker.Attacked = true;
        //        // Piece.DrainMove();

        //        Piece.SetTile(exit.GetOutTile());
        //        return true;
        //    }
        //    return false;
        //}

        //public double GetInc()
        //{
        //    return IncMove(false);
        //}
        //private double IncMove(bool doEndTurn)
        //{
        //    double moveInc = Consts.IncValueWithMaxLimit(MoveCur, MoveInc, Consts.MoveDev, MoveMax, MoveLimit, Consts.MoveLimitPow, doEndTurn);
        //    if (doEndTurn)
        //    {
        //        //this._moved = false;
        //        _moveCur += moveInc;
        //    }
        //    return moveInc;
        //}
        //void IBehavior.GetUpkeep(ref double energyUpk, ref double massUpk)
        //{
        //    EndTurn(false, ref energyUpk);
        //}
        //void IBehavior.StartTurn()
        //{
        //    _moved = false;
        //}
        //void IBehavior.EndTurn(ref double energyUpk, ref double massUpk)
        //{
        //    EndTurn(true, ref energyUpk);
        //}
        //private void EndTurn(bool doEndTurn, ref double energyUpk)
        //{
        //    energyUpk += IncMove(doEndTurn) * Consts.EnergyPerMove;
        //}

        Resources IBehavior.GetTurnEnd()
        {
            return new Resources();
        }
        void IBehavior.EndTurn(ref Resources resources)
        {
            _moveCur = Math.Min(MoveMax, _moveCur + MoveInc);
            _moved = false;
            _restrictMove = false;
        }

        void IBehavior.StartTurn()
        {
        }

        public enum MoveType
        {
            Ground,
            Sea,
            Amphibious,
            Air,
        }
    }
}
