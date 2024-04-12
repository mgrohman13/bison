namespace ClassLibrary1.Map
{
    public partial class Map 
    {
        private interface IEnemySpawn
        {
            public void Turn(int turn);
            int SpawnChance(int turn, double? enemyMove = null);
            Tile SpawnTile(Map map, bool isEnemy, double deviationMult = 1);
        }
    }
}
