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

        public readonly Color Color;
        public readonly string Name;
        public readonly bool Independent = false;

        [NonSerialized]
        private Dictionary<UnitType, Bitmap> _pics;

        private readonly List<Unit> units;

        private double score;
        private int souls;
        private int arrows;

        public Player(Color color, string Name)
            : this(null, color, Name, false, 0)
        {
        }
        public Player(Game game, Color color, string Name, int souls)
            : this(game, color, Name, false, souls)
        {
        }
        public Player(Game game, Color color, string Name, bool independent)
            : this(game, color, Name, independent, 0)
        {
        }
        public Player(Game game, Color color, string Name, bool independent, int souls)
        {
            this.Game = game;

            this.Color = color;
            this.Name = Name;
            this.Independent = independent;

            this.units = new List<Unit>();

            this.souls = souls;
            this.arrows = 0;

            LoadImages();
        }

        private Dictionary<UnitType, Bitmap> pics
        {
            get
            {
                if (_pics == null)
                    LoadImages();
                return _pics;
            }
        }

        private void LoadImages()
        {
            this._pics = new Dictionary<UnitType, Bitmap>();
            if (this.Independent)
                this.pics.Add(UnitType.Indy, new System.Drawing.Bitmap(@"pics\Indy.bmp"));
            this.pics.Add(UnitType.Archer, new System.Drawing.Bitmap(@"pics\Archer.bmp"));
            this.pics.Add(UnitType.Daemon, new System.Drawing.Bitmap(@"pics\Daemon.bmp"));
            this.pics.Add(UnitType.Infantry, new System.Drawing.Bitmap(@"pics\Infantry.bmp"));
            this.pics.Add(UnitType.Knight, new System.Drawing.Bitmap(@"pics\Knight.bmp"));

            TunePics();
        }

        private void TunePics()
        {
            foreach (KeyValuePair<UnitType, Bitmap> pair in new List<KeyValuePair<UnitType, Bitmap>>(pics))
            {
                Bitmap basePic = pair.Value;
                //white is transparent
                basePic.MakeTransparent(Color.White);

                //map gray to the player color
                ColorMap colorMap = new ColorMap();
                colorMap.OldColor = Color.FromArgb(100, 100, 100);
                colorMap.NewColor = Color;
                ImageAttributes colorRemapping = new ImageAttributes();
                colorRemapping.SetRemapTable(new ColorMap[] { colorMap });

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
                return Color.FromArgb(255 - Color.R, 255 - Color.G, 255 - Color.B);
            }
        }

        public List<Unit> Units
        {
            get
            {
                return units;
            }
        }

        public double Score
        {
            get
            {
                return score;
            }
        }

        public int Souls
        {
            get
            {
                return souls;
            }
        }

        public int Arrows
        {
            get
            {
                return arrows;
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
            double total = 0;
            for (int x = 0 ; x < Game.GetWidth() ; ++x)
                for (int y = 0 ; y < Game.GetHeight() ; ++y)
                    total += Tile.GetArmyStr(Game.GetTile(x, y).GetUnits(this));
            return total;
        }

        internal void Won(Player independent)
        {
            independent.AddSouls(this.souls);
            independent.MakeArrow(this.arrows);
        }

        public Tile NextUnit(Tile selectedTile)
        {
            var all = this.Units.GroupBy((u) => u.Tile).ToList();
            var active = GetActive(all);
            if (active.Any())
            {
                var next = all.Where((g, i) => active.Contains(i));
                int start = -1;
                if (selectedTile != null)
                    start = selectedTile.Y * Game.GetWidth() + selectedTile.X;
                return next.OrderBy((g) =>
                {
                    int cur = g.Key.Y * Game.GetWidth() + g.Key.X;
                    cur -= start;
                    if (cur <= 0)
                        cur += Game.GetHeight() * Game.GetWidth();
                    return cur;
                }).First().Key;
            }
            return null;
        }
        public IEnumerable<int> GetActive(IEnumerable<IEnumerable<Unit>> units)
        {
            Func<Unit, bool>[] funcs = { 
                (u) => u.Healed,
                (u) => u.Movement > 0 && ( u.Movement - 1 ) * u.Regen + u.Hits >= u.MaxHits,
                (u) => u.Movement > 0,
                (u) => u.ReserveMove > 0,
            };

            foreach (var func in funcs)
                if (this.Units.Any(func))
                    return units.Select((l, i) => l.Any(func) ? i : -1).Where((i) => i > -1);

            return new int[0];
        }

        internal void AddSouls(float value)
        {
            AddSouls(value, false);
        }

        internal void AddSouls(float value, bool addScore)
        {
            if (addScore)
                score += value;

            if (value > 0)
                souls += Game.Random.GaussianCappedInt(value, .078f);
            else
                souls += Game.Random.Round(value);

            SummonDaemon();
        }

        private void SummonDaemon()
        {
            while (!Independent && souls >= 666)
            {
                souls -= 666;
                Tile tile = Game.GetRandomTile();
                new Unit(UnitType.Daemon, tile, this);
            }
        }

        internal void MakeArrow(float arrows)
        {
            this.arrows += Game.Random.GaussianCappedInt(arrows, .091f);
        }
        internal void UseArrows(int needed)
        {
            this.arrows -= needed;
        }
        internal void IndyArrows(bool make)
        {
            const float rate = 6.5f;
            if (make)
            {
                this.arrows += Game.Random.Round(this.souls / rate);
                this.souls = 0;
            }
            else
            {
                AddSouls(this.arrows * rate);
                this.arrows = 0;
            }
        }

        internal void Add(Unit unit)
        {
            units.Add(unit);
        }

        internal void Remove(Unit unit)
        {
            units.Remove(unit);
            if (!Independent && units.Count == 0)
            {
                AddSouls(arrows * 3.9f);
                arrows = 0;
                souls = RoundSouls() * 666;
                SummonDaemon();

                if (units.Count == 0)
                    Game.RemovePlayer(this, false);
            }
        }

        internal void ResetMoves()
        {
            foreach (Unit unit in units)
                unit.ResetMove();
        }

        internal int RoundSouls()
        {
            return Game.Random.Round(souls / 666f);
        }

        public bool IsTurn()
        {
            return ( this == Game.GetCurrentPlayer() );
        }
    }
}
