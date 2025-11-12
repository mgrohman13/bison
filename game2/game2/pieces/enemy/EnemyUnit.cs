using game2.map;
using game2.pieces.behavior;

namespace game2.pieces.enemy
{
    internal class EnemyUnit : EnemyPiece
    {
        private EnemyUnit(Tile tile, int moveInc, int moveMax, int att, int def, int hp) : base(tile)
        {
            SetBehavior(
                new Movable(this, Movable.MoveType.Ground, moveInc, moveMax),
                new Combatant(this, att, def, hp));
        }
        internal static EnemyUnit NewEnemyUnit(Tile tile, int moveInc, int moveMax, int att, int def, int hp)
        {
            EnemyUnit enemyUnit = new(tile, moveInc, moveMax, att, def, hp);
            tile.Map.Game.AddPiece(enemyUnit);
            return enemyUnit;
        }
    }
}
