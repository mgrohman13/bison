namespace ClassLibrary1.Map
{
    public partial class Map
    {
        public interface IEnemySpawn
        {
            public SpawnChance Spawner { get; }
            //public void Turn(int turn);
            int SpawnChance(int turn, double? enemyMove);
            Tile SpawnTile(Map map);
        }
    }
}
