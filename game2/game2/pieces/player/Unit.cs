using game2.map;
using game2.pieces.behavior;

namespace game2.pieces.player
{
    internal class Unit : PlayerPiece
    {
        private Unit(Tile tile, int vision, int moveInc, int moveMax, int att, int def, int hp)
            : base(tile, vision)
        {
            SetBehavior(
                new Movable(this, Movable.MoveType.Ground, moveInc, moveMax),
                new Combatant(this, att, def, hp));
        }
        internal static Unit NewUnit(Tile tile, int vision, int moveInc, int moveMax, int att, int def, int hp)
        {
            Unit unit = new(tile, vision, moveInc, moveMax, att, def, hp);
            tile.Map.Game.AddPiece(unit);
            return unit;
        }
    }
}
