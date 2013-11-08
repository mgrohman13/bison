using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace Daemons
{
    [Serializable]
    public class Player
    {
        public readonly Game Game;

        public readonly string Name;
        public readonly Color Color;
        public readonly bool Independent;

        private readonly List<Unit> units;

        private double arrows, score;
        private int souls;

        [NonSerialized]
        private Dictionary<UnitType, Bitmap> _pics;

        public Player(Color color, string name)
            : this(null, color, name, false, 0)
        {
        }
        public Player(Game game, Color color, string name, int startSouls)
            : this(game, color, name, false, startSouls)
        {
        }
        public Player(Game game, Color color, string name, bool independent)
            : this(game, color, name, independent, 0)
        {
        }
        public Player(Game game, Color color, string name, bool independent, int startSouls)
        {
            this.Game = game;

            this.Color = color;
            this.Name = name;
            this.Independent = independent;

            this.units = new List<Unit>();

            this.souls = startSouls;
            this.arrows = 0;
            this.score = 0;
        }

        private Dictionary<UnitType, Bitmap> pics
        {
            get
            {
                if (this._pics == null)
                    this._pics = LoadImages(this.Independent, this.Color);
                return this._pics;
            }
        }
        private static Dictionary<UnitType, Bitmap> LoadImages(bool indy, Color color)
        {
            Dictionary<UnitType, Bitmap> pics = new Dictionary<UnitType, Bitmap>();
            if (indy)
                pics.Add(UnitType.Indy, new System.Drawing.Bitmap(@"pics\Indy.bmp"));
            pics.Add(UnitType.Archer, new System.Drawing.Bitmap(@"pics\Archer.bmp"));
            pics.Add(UnitType.Daemon, new System.Drawing.Bitmap(@"pics\Daemon.bmp"));
            pics.Add(UnitType.Infantry, new System.Drawing.Bitmap(@"pics\Infantry.bmp"));
            pics.Add(UnitType.Knight, new System.Drawing.Bitmap(@"pics\Knight.bmp"));

            TunePics(pics, color);
            return pics;
        }
        private static void TunePics(Dictionary<UnitType, Bitmap> pics, Color color)
        {
            foreach (KeyValuePair<UnitType, Bitmap> pair in pics.ToList())
            {
                Bitmap basePic = pair.Value;
                //white is transparent
                basePic.MakeTransparent(Color.White);

                //map gray to the player color
                ColorMap colorMap = new ColorMap();
                colorMap.OldColor = Color.FromArgb(100, 100, 100);
                colorMap.NewColor = color;
                ImageAttributes colorRemapping = new ImageAttributes();
                colorRemapping.SetRemapTable(new[] { colorMap });

                //draw it to a new image to remap the colors
                Bitmap newPic = new Bitmap(basePic.Width, basePic.Height);
                Graphics graphics = Graphics.FromImage(newPic);
                graphics.DrawImage(basePic, new Rectangle(new Point(0, 0), newPic.Size), 0, 0,
                        basePic.Width, basePic.Height, GraphicsUnit.Pixel, colorRemapping);

                //store the new image
                pics[pair.Key] = newPic;

                //clean up
                graphics.Dispose();
                basePic.Dispose();
                colorRemapping.Dispose();
            }
        }

        public Color InverseColor
        {
            get
            {
                return Color.FromArgb(255 - this.Color.R, 255 - this.Color.G, 255 - this.Color.B);
            }
        }

        public IEnumerable<Unit> GetUnits()
        {
            return this.units.AsReadOnly();
        }

        public double Score
        {
            get
            {
                return this.score;
            }
        }

        public int Souls
        {
            get
            {
                return this.souls;
            }
        }
        public int Arrows
        {
            get
            {
                return (int)Math.Floor(this.arrows);
            }
        }

        public Bitmap GetPic(UnitType type)
        {
            return pics[type];
        }

        public override string ToString()
        {
            return this.Name;
        }

        public double GetStrength()
        {
            return Tile.GetArmyStr(this.units);
        }

        internal void Won(Player independent)
        {
            independent.AddSouls(this.souls);
            independent.MakeArrow(this.arrows);
        }

        public Tile NextUnit(Tile selectedTile)
        {
            var all = this.units.GroupBy(unit => unit.Tile).ToList();
            var active = GetActive(all);
            if (active.Any())
            {
                var next = all.Where((group, index) => active.Contains(index));
                int start = -1;
                if (selectedTile != null)
                    start = selectedTile.Y * this.Game.Width + selectedTile.X;
                return next.OrderBy(group =>
                {
                    int cur = group.Key.Y * this.Game.Width + group.Key.X;
                    if (cur <= start)
                        cur += this.Game.Height * this.Game.Width;
                    return cur;
                }).First().Key;
            }
            return null;
        }
        public IEnumerable<int> GetActive(IEnumerable<IEnumerable<Unit>> options)
        {
            Func<Unit, bool>[] funcs = {
                unit => unit.Healed,
                Consts.MoveLeft,
                unit => unit.Movement > 0,
                unit => unit.ReserveMovement > 0,
            };

            foreach (var func in funcs)
                if (this.units.Any(func))
                    return options.Select((list, index) => ( list.Any(func) ? index : -1 )).Where(index => index > -1);

            return new int[0];
        }

        internal void AddSouls(double value)
        {
            AddSouls(value, false);
        }

        internal void AddSouls(double value, bool addScore)
        {
            if (addScore)
                this.score += value;

            if (value > 0)
                this.souls += Game.Random.GaussianCappedInt(value, Consts.SoulRand);
            else
                this.souls += Game.Random.Round(value);

            SummonDaemon();
        }

        private void SummonDaemon()
        {
            while (!this.Independent && this.souls >= Consts.DaemonSouls)
            {
                this.souls -= Consts.DaemonSouls;
                Tile tile = this.Game.GetRandomTile();
                new Unit(UnitType.Daemon, tile, this);
            }
        }

        internal void MakeArrow(double amount)
        {
            this.arrows += Game.Random.GaussianCapped(amount, .091);
        }
        internal void UseArrows(int needed)
        {
            this.arrows -= needed;
        }
        internal void IndyArrows(bool make)
        {
            const double rate = 6.5;
            if (make)
            {
                this.arrows += ( this.souls / rate );
                this.souls = 0;
            }
            else
            {
                this.souls += Game.Random.Round(this.arrows * rate);
                this.arrows = 0;
            }
        }

        internal void Add(Unit unit)
        {
            this.units.Add(unit);
        }

        internal void Remove(Unit unit)
        {
            this.units.Remove(unit);
            if (!this.Independent && this.units.Count == 0)
            {
                AddSouls(this.arrows * 3.9);
                this.arrows = 0;
                this.souls = RoundSouls() * Consts.DaemonSouls;
                SummonDaemon();

                if (this.units.Count == 0)
                    this.Game.RemovePlayer(this, false);
            }
        }

        internal void ResetMoves()
        {
            foreach (Unit unit in this.units)
                unit.ResetMove();
        }

        internal int RoundSouls()
        {
            return Game.Random.Round(this.souls / (double)Consts.DaemonSouls);
        }

        public bool IsTurn()
        {
            return ( this == this.Game.GetCurrentPlayer() );
        }
    }
}