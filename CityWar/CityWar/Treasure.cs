using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace CityWar
{
    [Serializable]
    public class Treasure
    {
        private const int CityCountdown = 210, UnitCountdown = 260;

        public readonly TreasureType Type;

        public bool Collected { get; private set; }
        public int Value { get; private set; }

        public bool UnitCollect => Type != TreasureType.Wizard;

        private readonly static Dictionary<TreasureType, Bitmap> pics = new();

        public Treasure(Tile tile, TreasureType type)
        {
            if (!CanCreate(tile, type))
                throw new Exception();

            this.Type = type;
            this.Collected = false;
            this.Value = GetValue(tile, type);
        }
        internal static bool CanCreate(Tile tile, TreasureType type)
        {
            return type switch
            {
                TreasureType.Wizard => GetValue(tile, TreasureType.Wizard) > 0,
                TreasureType.City => !tile.GetNeighbors(true, true).Any(t => t == null || t.HasCity()),
                _ => true,
            };
        }
        private static int GetValue(Tile tile, TreasureType type)
        {
            return type switch
            {
                TreasureType.Wizard => tile.FindDistance(t => t.HasWizard()) - 1,
                TreasureType.City => Game.Random.GaussianOEInt(7.8, .39, .169, 1),
                TreasureType.Magic => Game.Random.GaussianOEInt(6.5, .21, .065, 1),
                TreasureType.Relic => Game.Random.GaussianOEInt(5.2, .169, .091, 1),
                TreasureType.Unit => Game.Random.GaussianOEInt(3.9, .26, .13, 1),
                _ => throw new Exception(),
            };
        }
        internal void AddTo()
        {
            if (Type == TreasureType.Wizard)
                Value += Game.Random.RangeInt(0, 2);
            else
                Value += Game.Random.GaussianOEInt(3.9, .52, .21, 0);
        }

        internal bool MoveTo(Tile tile, Piece piece)
        {
            if (!Collected && !UnitCollect)
            {
                if (Type == TreasureType.Wizard && piece is Wizard)
                {
                    Collected = true;
                    piece.Owner.CollectResources(true, Value, tile.Terrain);
                    tile.Game.CreateWizardPts();
                    return true;
                }
                else throw new Exception();
            }
            return false;
        }

        internal bool Collect(Tile tile, Unit unit, out bool canUndo)
        {
            canUndo = false;
            if (!Collected && UnitCollect)
            {
                Collected = true;
                --Value;

                Player player = unit.Owner;
                //get a little bit of work as if the unit partially rested
                player.AddWork(GetUnitWork(unit));

                canUndo = Type switch
                {
                    TreasureType.City => CollectCity(tile, player),
                    TreasureType.Magic => CollectMagic(player),
                    TreasureType.Relic => CollectRelic(player),
                    TreasureType.Unit => CollectUnit(tile, player),
                    _ => throw new Exception(),
                };
            }
            return (Value == 0);
        }
        internal bool UndoCollect(Unit unit)
        {
            Player player = unit.Owner;
            bool retVal = Type switch
            {
                TreasureType.City => UndoCollectCity(player),
                TreasureType.Magic => UndoCollectMagic(player),
                TreasureType.Relic => UndoCollectRelic(player),
                TreasureType.Unit => UndoCollectUnit(player),
                _ => throw new Exception(),
            };

            player.AddWork(-GetUnitWork(unit));
            ++Value;
            Collected = false;

            return retVal || player.Work < 0;
        }
        private static double GetUnitWork(Unit u)
        {
            return u.WorkRegen * u.MaxMove * .39;
        }
        private bool CollectCity(Tile tile, Player player)
        {
            //get population based on the number of turns remaining
            player.Spend(0, CostType.Production, GetCountdownValue(CityCountdown));
            if (Value == 0)
            {
                //get the city
                new City(player, tile);
                player.AddUpkeep(390, .039);
                return false;
            }
            return true;
        }
        private bool UndoCollectCity(Player player)
        {
            if (Value < 1)
                throw new Exception();
            player.Spend(0, CostType.Production, -GetCountdownValue(CityCountdown));
            return player.Population < 0;
        }
        private bool CollectMagic(Player player)
        {
            player.SpendMagic(GetCollectValue());
            return true;
        }
        private bool UndoCollectMagic(Player player)
        {
            player.SpendMagic(-GetCollectValue());
            return player.Magic < 0;
        }
        private bool CollectRelic(Player player)
        {
            int CountRelics() => player.GetPieces().OfType<Relic>().Count();
            int relics = CountRelics();
            player.AddRelic(-GetCollectValue());
            return relics == CountRelics();
        }
        private bool UndoCollectRelic(Player player)
        {
            player.AddRelic(GetCollectValue());
            return player.Relic < 0;
        }
        private bool CollectUnit(Tile tile, Player player)
        {
            //get production based on the number of turns remaining
            player.Spend(GetCountdownValue(UnitCountdown), CostType.Production, 0);
            if (Value == 0)
            {
                //get a random unit 
                Unit unit = Unit.NewUnit(Game.Random.SelectValue(Game.Races[player.Race]), tile, player);
                player.BalanceForUnit(Game.UnitTypes.GetSchema().Unit.Average(u => u.Cost + u.People), unit.RandedCost);
                return false;
            }
            return true;
        }
        private bool UndoCollectUnit(Player player)
        {
            if (Value < 1)
                throw new Exception();
            player.Spend(-GetCountdownValue(UnitCountdown), CostType.Production, 0);
            return player.Production < 0;
        }
        private static int GetCollectValue()
        {
            return -Game.Random.GaussianCappedInt(Game.Random.Range(6.5, 13), .052);
        }
        private int GetCountdownValue(double mult)
        {
            return -Game.Random.GaussianCappedInt(Math.Sqrt(Value * mult), .065);
        }

        internal void Reset()
        {
            Collected = false;
        }

        public Bitmap GetPic()
        {
            if (!pics.TryGetValue(Type, out Bitmap retVal))
                pics[Type] = retVal = ImageUtil.LoadPicFromFile(Type);
            return retVal;
        }
        internal static void ResetPics()
        {
            foreach (Bitmap image in pics.Values)
                image.Dispose();
            pics.Clear();
        }

        public enum TreasureType
        {
            //collected by wizard moving to:
            Wizard,
            //collected by unit pausing whole turn:
            City,
            Relic,
            Magic,
            Unit,
        }
    }
}
