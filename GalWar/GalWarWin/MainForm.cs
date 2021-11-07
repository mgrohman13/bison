using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using GalWar;
using GalWarWin.Sliders;
using MattUtil;
using Point = MattUtil.Point;
using PointForm = System.Drawing.Point;

namespace GalWarWin
{
    public partial class MainForm : Form, IEventHandler
    {
        #region fields and constructors

        private static Game _game;
        public static Game Game
        {
            get
            {
                return _game;
            }
            private set
            {
                _game = value;
            }
        }

        private static MainForm gameForm;
        public static MainForm GameForm
        {
            get
            {
                return gameForm;
            }
        }

        private bool isDialog;
        private bool isBuild;
        private Tile dialogTile = null;

        private static bool started = false, saved = true, ended = false, _showMoves = false, _showAtt = false;
        private static PointForm mouse;
        private static Font font = new Font("arial", 13f);

        private MainForm dialog;

        private static bool emphasisEvent = true, showDist = false;

        private static Ship anomExp = null, vector = null;

        private static Tile _selected = null, panning = null;
        private static HashSet<SpaceObject> hold = new HashSet<SpaceObject>(), holdPersistent = new HashSet<SpaceObject>();

        private static HashSet<Tuple<PopCarrier, PopCarrier>> movedTroops = new HashSet<Tuple<PopCarrier, PopCarrier>>();

        private static float panX, panY, scale;

        private MainForm(bool dialog)
        {
            InitializeComponent();
            lblResearch.Parent = lblRsrchPct;
            lblResearch.Location = new PointForm(56, 0);
            lblRsrchTot.Parent = lblRsrchPct;
            lblRsrchTot.Location = new PointForm(0, 0);
            MouseWheel += new MouseEventHandler(MainForm_MouseWheel);
            ResizeRedraw = true;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            //Bounds = Screen.GetWorkingArea(this);

            isDialog = dialog;
            if (isDialog)
            {
                HideButtons(this);
                AcceptButton = btnCancel;
            }
        }

        private void HideButtons(Control control)
        {
            if (control is CheckBox)
                control.Enabled = false;
            else if (control is Button && control != btnCancel && control != btnCombat
                    && control != btnInvasion && control != btnShowMoves
                    && control != btnGraphs && control != btnCostCalc)
                control.Hide();
            else
                foreach (Control child in control.Controls)
                    HideButtons(child);
        }

        public MainForm()
            : this(false)
        {
            gameForm = this;

            dialog = new MainForm(true);

            pnlHUD.Hide();

            string initialDirectory = GetInitialDirectory();
            Game.AutoSavePath = GetInitialAutoSave(initialDirectory);

            openFileDialog1.InitialDirectory = initialDirectory;
            openFileDialog1.FileName = "g.gws";
            saveFileDialog1.InitialDirectory = openFileDialog1.InitialDirectory;
            saveFileDialog1.FileName = openFileDialog1.FileName;
        }

        private static string GetInitialDirectory()
        {
            string savePath = null;
            try
            {
                if (File.Exists("savepath.txt"))
                {
                    using (StreamReader reader = new StreamReader("savepath.txt"))
                        savePath = reader.ReadLine();
                    if (!Directory.Exists(savePath))
                        savePath = null;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                savePath = null;
            }

            if (savePath == null)
                savePath = Directory.GetParent(Directory.GetParent(Directory.GetParent(Environment.CurrentDirectory).FullName).FullName).FullName;

            return savePath;
        }

        private static string GetInitialAutoSave(string initialDirectory)
        {
            return initialDirectory + "\\auto";
        }

        public Tile selected
        {
            get
            {
                return _selected;
            }
            private set
            {
                _selected = value;
                Center(false);
            }
        }

        private bool showMoves
        {
            get
            {
                return _showMoves;
            }
            set
            {
                _showMoves = value;
                SetBtnShowMovesText();
            }
        }
        private bool showAtt
        {
            get
            {
                return _showAtt;
            }
            set
            {
                _showAtt = value;
                SetBtnShowMovesText();
            }
        }

        private Tile GetSelectedTile()
        {
            return selected;
        }
        internal void SelectTile(Tile tile)
        {
            selected = tile;
        }

        private void SetBtnShowMovesText()
        {
            btnShowMoves.Text = ((showMoves && !showAtt) ? "Enemy Attacks" : "Enemy Moves");
        }

        private int ClientHeight
        {
            get
            {
                int height = ClientSize.Height;
                if (tbTurns.Visible)
                    height -= tbTurns.Height;
                return height;
            }
        }

        #endregion //fields and constructors

        #region Drawing

        public void Center()
        {
            Center(true);
        }
        private void Center(bool always)
        {
            if (selected != null && (always || !SelectedVisible()))
                Center(selected);
            else
                VerifyScalePan();
        }
        private void Center(Tile tile)
        {
            Tile center = GetTile(new PointForm((this.Width - pnlHUD.Width) / 2, this.Height / 2));
            panX += (center.X - tile.X) * scale;
            panY += (center.Y - tile.Y) * scale;
            VerifyScalePan();
        }
        private bool SelectedVisible()
        {
            Rectangle gameBounds = GetGameBounds();
            int x = selected.X, y = selected.Y;
            return (x > gameBounds.Left + 1 && x < gameBounds.Right - 1 && y > gameBounds.Top + 1 && y < gameBounds.Bottom - 1);
        }

        private const float ShipDetailScale = 9.1f;
        private const float GridScale = 16.9f;
        private const float TextScale = 21f;

        protected override void OnPaint(PaintEventArgs paintEventArgs)
        {
            base.OnPaint(paintEventArgs);

            if (started)
            {
                Graphics g = paintEventArgs.Graphics;

                try
                {
                    float height = ClientHeight;
                    float width = ClientSize.Width - pnlHUD.Width;

                    g.Clear(Color.Black);

                    float minQuality = int.MaxValue, maxQuality = int.MinValue,
                            minPop = int.MaxValue, maxPop = int.MinValue, minStr = int.MaxValue, maxStr = int.MinValue;
                    foreach (SpaceObject obj in Game.GetSpaceObjects())
                    {
                        Ship ship;
                        Planet planet;
                        if ((ship = (obj as Ship)) != null)
                        {
                            GetVals(ref minStr, ref maxStr, (float)ship.GetStrength() * ship.HP / (float)ship.MaxHP);
                        }
                        else if ((planet = (obj as Planet)) != null)
                        {
                            GetVals(ref minQuality, ref maxQuality, planet.Quality);
                            Colony colony = planet.Colony;
                            if (colony != null)
                                GetVals(ref minPop, ref maxPop, colony.Population);
                        }
                    }

                    if (scale > TextScale)
                    {
                        float newSize = font.Size * scale / g.MeasureString("99%", font).Width;
                        if (newSize > 13f)
                            newSize = 13f;
                        else if (newSize < 1f)
                            newSize = 1f;
                        if (Math.Abs(newSize - font.Size) > Consts.FLOAT_ERROR_ZERO * 13)
                        {
                            font.Dispose();
                            font = new Font("arial", newSize);
                        }
                    }

                    Rectangle gameBounds = GetGameBounds();

                    if (scale > GridScale && !showDist)
                    {
                        int capacity = ((gameBounds.Width + 3) * (gameBounds.Height + 1) * 2) / 3;
                        List<RectangleF> rects = new List<RectangleF>(capacity);
                        for (int y = gameBounds.Top; y <= gameBounds.Bottom; ++y)
                        {
                            //we can skip drawing every 3rd rectangle as a decent optimization
                            int skip = (y % 2 == 0 ? 1 : 0);
                            for (int x = gameBounds.Left; x <= gameBounds.Right; ++x)
                                if (++skip == 3 && x != gameBounds.Right)
                                    skip = 0;
                                else
                                    rects.Add(GetDrawRect(x, y));
                        }
                        //Console.WriteLine(rects.Count + "\t" + capacity);
                        if (capacity < rects.Count)
                            ;
                        using (Pen pen = new Pen(Color.White, 1f))
                            g.DrawRectangles(pen, rects.ToArray());
                    }

                    HashSet<Tile> drawTiles = new HashSet<Tile>();
                    if (selected != null)
                        drawTiles.Add(selected);
                    drawTiles.UnionWith(Game.GetWormholes().SelectMany(wormhole => wormhole.Tiles));
                    if (isDialog && dialogTile != null)
                    {
                        drawTiles.Add(dialogTile);
                        drawTiles.UnionWith(Tile.GetNeighbors(dialogTile));
                    }
                    drawTiles.UnionWith(Game.GetSpaceObjects().Select(spaceObject => spaceObject.Tile));

                    Dictionary<Tile, float> moves = null;
                    if (showMoves && scale > TextScale)
                    {
                        moves = GetMoves();
                        drawTiles.UnionWith(moves.Keys);
                    }

                    gameBounds = new Rectangle(gameBounds.X - (gameBounds.Top % 2 == 0 ? 1 : 0), gameBounds.Y,
                            gameBounds.Width + (gameBounds.Top % 2 == 0 ? 3 : 2), gameBounds.Height + 1);

                    //foreach (Tile tile in Game.GetSpaceObjects().OfType<Ship>().SelectMany(ship => Tile.GetNeighbors(ship.Tile)))
                    //{
                    //    int x = tile.X, y = tile.Y;
                    //    if (gameBounds.Contains(x, y))
                    //    {
                    //        Player zoc = tile.GetZOC();
                    //        if (zoc != null && ( tile.SpaceObject is Ship ? zoc != tile.SpaceObject.Player : tile.SpaceObject == null ))
                    //        {
                    //            RectangleF rect = GetDrawRect(x, y);
                    //            const float dim = 3f;
                    //            using (Brush brush = new SolidBrush(Color.FromArgb(Game.Random.Round(zoc.Color.R / dim), Game.Random.Round(zoc.Color.G / dim), Game.Random.Round(zoc.Color.B / dim))))
                    //                g.FillRectangle(brush, RectangleF.Inflate(rect, -1, -1));
                    //        }
                    //    }
                    //}

                    if (showDist)
                    {
                        IEnumerable<Point> allTiles = Game.Random.Iterate(gameBounds.Left, gameBounds.Right, gameBounds.Top, gameBounds.Bottom);
                        double max = allTiles.Max(p => Tile.GetDistance(Game.GetTile(p), Game.Center));
                        foreach (Point p in allTiles)
                        {
                            int val = Game.Random.Round(255 * Tile.GetDistance(Game.Center, Game.GetTile(p)) / max);
                            using (Brush brush = new SolidBrush(Color.FromArgb(val, val, val)))
                                g.FillRectangle(brush, GetDrawRect(p.X, p.Y));
                        }
                    }

                    foreach (Tile tile in drawTiles)
                        DrawObject(g, gameBounds, tile, moves, minQuality, maxQuality, minPop, maxPop, minStr, maxStr);

                    Ship selectedShip = GetSelectedShip();
                    if (selectedShip != null)
                    {
                        IEnumerable<Tile> path;
                        if (!selectedShip.Player.IsTurn)
                            path = selectedShip.Moved.Concat(new[] { selectedShip.Tile });
                        else if (selectedShip.Vector != null)
                            path = Tile.PathFind(selectedShip);
                        else
                            path = null;
                        if (path != null && path.Skip(1).Any())
                        {
                            using (Pen pen = new Pen(selectedShip.Player.Color))
                                g.DrawLines(pen, path.Select(tile => GetDrawRect(tile.X, tile.Y)).Select(
                                        rect => new PointF(rect.X + rect.Width / 2f, rect.Y + rect.Height / 2f)).ToArray());
                            if (selectedShip.Player.IsTurn && scale > TextScale)
                                using (Brush brush = new SolidBrush(selectedShip.Player.Color))
                                {
                                    Tile point = path.Last();
                                    RectangleF rect = GetDrawRect(point.X, point.Y);
                                    float x = rect.Left + rect.Width / 2f, y = rect.Top + rect.Height / 2f;
                                    int dist = path.Count() - 1;
                                    g.DrawString(dist.ToString(), font, brush, x, y);
                                    g.DrawString(Math.Ceiling((dist - selectedShip.CurSpeed) / (double)selectedShip.MaxSpeed).ToString(),
                                            font, brush, x, y + g.MeasureString(dist.ToString(), font).Height + 3f);
                                }
                        }
                    }

                    //Tile t = selected;
                    //if (selected == null)
                    //    t = Game.Center;
                    //if (last != t)
                    //    meh = 0;
                    //last = t;
                    //List<RectangleF> r = new List<RectangleF>();
                    //List<Tile> mehs = Game.GetDistanceTiles(t, ++meh).ToList();
                    //Console.WriteLine(mehs.Count);
                    //foreach (Tile tt in mehs)
                    //    r.Add(GetDrawRect(tt.X, tt.Y));
                    //using (Brush b = new SolidBrush(Game.CurrentPlayer.Color))
                    //    g.FillRectangles(b, r.ToArray());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);

                    using (Font font = new Font("arial", 13f))
                        g.DrawString(e.ToString(), font, Brushes.White, 0f, 0f);
                }
            }
        }
        //int meh = 0;
        //Tile last = null;
        private void DrawObject(Graphics g, Rectangle gameBounds, Tile tile, Dictionary<Tile, float> moves)
        {
            DrawObject(g, gameBounds, tile, moves, float.NaN, float.NaN, float.NaN, float.NaN, float.NaN, float.NaN);
        }
        private void DrawObject(Graphics g, Rectangle gameBounds, Tile tile, Dictionary<Tile, float> moves, float minQuality, float maxQuality, float minPop, float maxPop, float minStr, float maxStr)
        {
            int x = tile.X, y = tile.Y;
            if (gameBounds.Contains(x, y))
            {
                SpaceObject spaceObject = tile.SpaceObject;
                RectangleF rect = GetDrawRect(x, y);

                int telNum = tile.WormholeNumber;
                if (telNum > 0)
                {
                    Brush brush = Brushes.DarkGray;
                    if (Game.GetWormholes().Count > 1)
                    {
                        float telCount = Game.GetWormholes().Count + 1f;
                        float min = 520f / telCount / telCount;
                        int color = (int)(.5f + min + telNum * (255f - min) / telCount);
                        brush = new SolidBrush(Color.FromArgb(color, color, color));
                    }
                    g.FillRectangle(brush, RectangleF.Inflate(rect, .5f, .5f));
                    if (showDist)
                        using (Pen pen = new Pen(Color.White, 2f))
                            g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
                }

                if (spaceObject is Anomaly)
                {
                    RectangleF anomaly = Inflate(scale, rect, 1f, 1f, 1f, .6f, .13f);
                    if (scale <= GridScale)
                    {
                        anomaly = RectangleF.Inflate(anomaly, -1f, -1f);
                        if (anomaly.Width < (rect.Width > 3.5f ? 2f : 1f))
                            anomaly.Width = (rect.Width > 3.5f ? 2f : 1f);
                        if (anomaly.Height < (rect.Height > 3.5f ? 2f : 1f))
                            anomaly.Height = (rect.Height > 3.5f ? 2f : 1f);
                    }
                    g.FillRectangle(Brushes.White, anomaly);
                }
                else
                {
                    Ship ship;
                    Planet planet;
                    if ((ship = (spaceObject as Ship)) != null)
                        DrawShip(g, scale, rect, ship, minStr, maxStr);
                    else if ((planet = (spaceObject as Planet)) != null)
                        DrawPlanet(g, scale, rect, planet, minQuality, maxQuality, minPop, maxPop);
                }

                DrawBorder(g, tile, spaceObject, rect, scale);

                if (moves != null && scale > TextScale)
                {
                    float move;
                    if (moves.TryGetValue(tile, out move))
                    {
                        string s = (showAtt ? FormatDouble(move) : FormatInt(move));
                        if (showAtt)
                            foreach (Tile neighbor in Tile.GetNeighbors(tile))
                            {
                                Planet planet = neighbor.SpaceObject as Planet;
                                if (planet != null && planet.Player != null && !planet.Player.IsTurn)
                                {
                                    s = FormatInt(move) + "+";
                                    break;
                                }
                            }
                        SizeF strSize = g.MeasureString(s, font);
                        g.DrawString(s, font, Brushes.White, rect.Right - strSize.Width, rect.Bottom - strSize.Height);
                    }
                }
            }
        }
        private static RectangleF GetDrawRect(int x, int y)
        {
            return new RectangleF(panX + scale * x + (y % 2 == 0 ? 0f : scale / 2f), panY + scale * y, scale, scale);
        }

        private Rectangle GetGameBounds()
        {
            Tile min = GetTile(new PointForm(0, 0));
            Tile max = GetTile(new PointForm(ClientSize.Width - pnlHUD.Width, ClientSize.Height));
            return new Rectangle(min.X, min.Y, max.X - min.X, max.Y - min.Y);
        }

        private void GetVals(ref float min, ref float max, float value)
        {
            min = (float)Math.Min(min, Math.Sqrt(value));
            max = (float)Math.Max(max, Math.Sqrt(value));
        }

        private void DrawBorder(Graphics g, Tile tile, SpaceObject spaceObject, RectangleF rect, float scale)
        {
            float size = float.NaN;
            Ship ship;
            if (tile == selected || tile == dialogTile)
                size = 3f;
            else if (spaceObject is Planet || (scale > GridScale && (ship = (spaceObject as Ship)) != null && ship.Player.IsTurn && ship.CurSpeed > 0)
                    || (isDialog && ValidDialogTile(tile, spaceObject)))
                size = 2f;
            if (scale <= GridScale)
                size -= 1f;

            if (!float.IsNaN(size))
                using (Pen pen = new Pen(Color.White, size))
                    g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
        }

        private void DrawPlanet(Graphics g, float scale, RectangleF rect, Planet planet,
                float minQuality, float maxQuality, float minPop, float maxPop)
        {
            Colony colony = planet.Colony;
            RectangleF planetRect;
            if (colony == null)
            {
                planetRect = Inflate(scale, rect, planet.Quality, minQuality, maxQuality, .735f, .169f);
                g.FillEllipse(Brushes.White, planetRect);
            }
            else
            {
                planetRect = Inflate(scale, rect, colony.Population, minPop, maxPop, .735f, .169f);
                using (Brush brush = new SolidBrush(colony.Player.Color))
                    g.FillEllipse(brush, planetRect);
            }

            if (colony != null)
                if (scale > ShipDetailScale)
                {
                    if (colony.HP > 0)
                        g.DrawEllipse(Pens.White, planetRect);
                    double pct = colony.Population / (double)planet.Quality;
                    if (pct < 1 && scale > TextScale)
                    {
                        string str = FormatPctWithCheck(pct);
                        SizeF strSize = g.MeasureString(str, font);
                        g.DrawString(str, font, Brushes.White, rect.Right - strSize.Width, rect.Bottom - strSize.Height);
                    }
                }
                else
                {
                    using (Brush brush = new SolidBrush(colony.Player.Color))
                        g.FillRectangle(brush, rect);
                    g.DrawEllipse(Pens.White, RectangleF.Inflate(planetRect, .5f, .5f));
                }
        }

        private void DrawShip(Graphics g, float scale, RectangleF rect, Ship ship, float minStr, float maxStr)
        {
            rect = Inflate(scale, rect, (float)ship.GetStrength() * ship.HP / (float)ship.MaxHP, minStr, maxStr, .666f, .13f);
            using (Brush brush = new SolidBrush(ship.Player.Color))
                g.FillRectangle(brush, rect);

            if (scale > ShipDetailScale)
            {
                if (ship.DeathStar || ship.Population > 0)
                    g.DrawRectangle(Pens.White, rect.X, rect.Y, rect.Width, rect.Height);

                double pct = ship.HP / (double)ship.MaxHP;
                if (pct < 1)
                {
                    Pen pen = new Pen(Color.Black, 2f);
                    if (ship.Player.IsTurn && !ship.HasRepaired && ship.AutoRepair == 0)
                        pen = new Pen(Color.White, 2f);

                    PointF endPoint;
                    if (pct < .75f)
                        endPoint = new PointF(rect.Right, rect.Bottom);
                    else
                        endPoint = GetMid(rect);
                    g.DrawLine(pen, rect.Location, endPoint);

                    if (pct < .5f)
                    {
                        if (pct < .25f)
                            endPoint = new PointF(rect.Right, rect.Top);
                        else
                            endPoint = GetMid(rect);
                        g.DrawLine(pen, new PointF(rect.Left, rect.Bottom), endPoint);
                    }
                }
            }
        }

        private RectangleF Inflate(float scale, RectangleF rect, float value, float min, float max, float smallInvsPct, float inc)
        {
            if (scale <= GridScale)
            {
                float big = 1f - (1f - (smallInvsPct + inc)) / 3f;
                if (big > (scale - 1f) / scale)
                    big = (scale - 1f) / scale;
                smallInvsPct = .5f + 2f / scale + (smallInvsPct - .5f) / 3f;
                if (smallInvsPct > big)
                    smallInvsPct = big;
                inc = big - smallInvsPct;
                if (inc < 0f)
                    inc = 0f;
            }
            float inflate = (-scale * (1f - (smallInvsPct + inc * (((float)Math.Sqrt(value) - min + 1f) / (max - min + 1f)))));
            rect.Inflate(inflate, inflate);
            if (rect.Width < 1f)
                rect.Width = 1f;
            if (rect.Height < 1f)
                rect.Height = 1f;
            return rect;
        }

        private PointF GetMid(RectangleF rect)
        {
            return new PointF(rect.X + rect.Width / 2f, rect.Y + rect.Height / 2f);
        }

        private void VerifyScale()
        {
            VerifyScalePan(true, false);
        }
        private void VerifyPan()
        {
            VerifyScalePan(false, true);
        }
        private void VerifyScalePan()
        {
            VerifyScalePan(true, true);
        }
        private void VerifyScalePan(bool scaling, bool panning)
        {
            if (started)
            {
                Rectangle bounds = Game.GetGameBounds();
                int inflate = (int)(Game.GetSpaceObjects().OfType<Ship>().Select(ship => ship.MaxSpeed).DefaultIfEmpty(1).Max() * 1.3f + 2.1f);
                bounds.Inflate(inflate, inflate);
                int minX = bounds.Left, minY = bounds.Top, maxX = bounds.Right, maxY = bounds.Bottom;

                float xDiameter, yDiameter, width, height, padding;
                float minScale = GetScale(minX, minY, maxX, maxY, out xDiameter, out yDiameter, out width, out height, out padding);

                if (scaling)
                {
                    float maxScale = 91f;
                    if (scale < minScale)
                        scale = minScale;
                    if (scale > maxScale)
                        scale = maxScale;
                }

                if (panning)
                {
                    float minPanX = width - scale * (xDiameter + minX) - padding;
                    float maxPanX = padding - scale * minX;
                    //check min last so the map is anchored at the top
                    if (panX > maxPanX)
                        panX = maxPanX;
                    if (panX < minPanX)
                        panX = minPanX;

                    float minPanY = height - scale * (yDiameter + minY) - padding;
                    float maxPanY = padding - scale * minY;
                    //check max last so the map is anchored at the right
                    if (panY < minPanY)
                        panY = minPanY;
                    if (panY > maxPanY)
                        panY = maxPanY;
                }
            }
        }
        private static void FindBounds(ref int minX, ref int minY, ref int maxX, ref int maxY, Tile tile)
        {
            minX = Math.Min(minX, tile.X);
            minY = Math.Min(minY, tile.Y);
            maxX = Math.Max(maxX, tile.X);
            maxY = Math.Max(maxY, tile.Y);
        }
        private float GetScale(int minX, int minY, int maxX, int maxY)
        {
            float xDiameter, yDiameter, width, height, padding;
            return GetScale(minX, minY, maxX, maxY, out xDiameter, out yDiameter, out width, out height, out padding);
        }
        private float GetScale(int minX, int minY, int maxX, int maxY,
                out float xDiameter, out float yDiameter, out float width, out float height, out float padding)
        {
            xDiameter = maxX - minX + 1.5f;
            yDiameter = maxY - minY + 1;
            width = ClientSize.Width - pnlHUD.Width;
            height = ClientHeight;
            padding = 3f;
            return Math.Min((height - padding * 2) / yDiameter, (width - padding * 2) / xDiameter);
        }

        private Dictionary<Tile, float> GetMoves()
        {
            gm1 = gm2 = gm3 = 0;

            Dictionary<Tile, Point> temp = new Dictionary<Tile, Point>();
            Dictionary<Tile, float> totals = new Dictionary<Tile, float>();
            foreach (Player enemy in Game.GetPlayers())
                if (enemy != Game.CurrentPlayer)
                {
                    foreach (Ship ship in enemy.GetShips())
                        AddShip(totals, temp, enemy, ship, ship.MaxSpeed);
                    foreach (Colony colony in enemy.GetColonies())
                        AddShip(totals, temp, enemy, colony, 1);
                }

            if (showAtt)
            {
                Dictionary<int, float> statFromValue = new Dictionary<int, float>(totals.Count);
                Dictionary<Tile, float> retVal = new Dictionary<Tile, float>(totals.Count);
                foreach (KeyValuePair<Tile, float> pair in totals)
                {
                    Tile tile = pair.Key;
                    float statValue = pair.Value;
                    float value;
                    int key = (int)Math.Round(statValue * 3f);
                    if (!statFromValue.TryGetValue(key, out value))
                    {
                        value = GetStatFromValue(statValue);
                        statFromValue.Add(key, value);
                    }
                    retVal.Add(tile, value);
                }
                totals = retVal;
            }

            Console.WriteLine(gm1);
            Console.WriteLine(gm2);
            Console.WriteLine(gm3);

            return totals;
        }

        private static int gm1, gm2, gm3;
        private static float GetStatFromValue(float value)
        {
            if (value < Consts.FLOAT_ERROR_ONE)
                return value;

            const float digit = .1f, div = 1f / digit;
            int min = (int)Math.Floor(Math.Pow(value, 1f / 3f) * div + 2), max = (int)Math.Ceiling(Math.Sqrt(value) * div + 1);
            float stat = TBSUtil.FindValue(delegate (int test)
            {
                ++gm1;
                return (ShipDesign.GetStatValue(test / div) >= value);
            }, min, max, true) / div;

            if ((stat - min / 10.0) < Consts.FLOAT_ERROR_ZERO || (max / 10.0 - stat) < Consts.FLOAT_ERROR_ZERO)
                throw new Exception();

            if ((ShipDesign.GetStatValue(stat) - value) > (value - ShipDesign.GetStatValue(stat - digit)))
                stat -= digit;

            const float adj = digit / 10f, half = .5f;
            if (Math.Abs(stat % 1f - half) < half * Consts.FLOAT_ERROR_ZERO)
                if ((ShipDesign.GetStatValue(stat + half) - value) > (value - ShipDesign.GetStatValue(stat - half)))
                    stat -= adj;
                else
                    stat += adj;

            return stat;
        }

        private void AddShip(Dictionary<Tile, float> totals, Dictionary<Tile, Point> temp, Player enemy, Combatant combatant, int speed)
        {
            temp.Clear();
            AddTiles(temp, enemy, combatant.Tile, speed, combatant is Colony);

            Colony colony;
            float statValue = 0;
            if (showAtt && ((colony = combatant as Colony) == null || colony.HP > 0))
                statValue = (float)ShipDesign.GetStatValue(combatant.Att);

            foreach (KeyValuePair<Tile, Point> pair in temp)
            {
                Tile tile = pair.Key;
                Point point = pair.Value;

                float add = float.NaN;
                Player showPlayer = null;
                if (showAtt)
                {
                    add = point.X * statValue;
                    showPlayer = Game.CurrentPlayer;
                }
                else if (point.Y > 0)
                {
                    add = 1;
                    showPlayer = combatant.Player;
                }
                else
                {
                    ++gm3;
                }

                Ship ship = tile.SpaceObject as Ship;
                if (showPlayer != null && (tile.SpaceObject == null ||
                        (!showAtt && tile.SpaceObject is Anomaly) || (ship != null && ship.Player == showPlayer)))
                {
                    float val;
                    totals.TryGetValue(tile, out val);
                    totals[tile] = val + add;
                }
            }
        }

        private void AddTiles(Dictionary<Tile, Point> retVal, Player enemy, Tile tile, int speed, bool ignoreZoc)
        {
            if (speed > 0)
                foreach (Tile neighbor in Tile.GetNeighbors(tile))
                {
                    ++gm2;
                    Point v1;
                    retVal.TryGetValue(neighbor, out v1);
                    int damage = v1.X;
                    int move = v1.Y;
                    int newDamage = Math.Max(speed, damage);
                    int newMove = speed - (showAtt ? 1 : 0);

                    Ship ship;
                    if (newMove > move && (neighbor.SpaceObject == null || neighbor.SpaceObject is Anomaly ||
                            ((ship = neighbor.SpaceObject as Ship) != null && ship.Player == enemy))
                            && (ignoreZoc || Ship.CheckZOC(enemy, tile, neighbor)))
                    {
                        retVal[neighbor] = new Point(newDamage, newMove);
                        AddTiles(retVal, enemy, neighbor, newMove - (showAtt ? 0 : 1), ignoreZoc);
                    }
                    else if (newDamage > damage)
                    {
                        retVal[neighbor] = new Point(newDamage, move);
                    }
                    else
                    {
                        gm3++;
                    }
                }
        }

        #endregion //Drawing

        #region Events

        private void btnShowMoves_Click(object sender, EventArgs e)
        {
            if (scale < TextScale)
                SetScale(TextScale);

            showAtt = (showMoves ? !showAtt : false);
            showMoves = true;
            showDist = false;
            InvalidateMap();
        }
        private void SetScale(float value)
        {
            scale = (float)(value * Consts.FLOAT_ERROR_ONE);
            VerifyScale();
        }

        private void btnGraphs_Click(object sender, EventArgs e)
        {
            GraphsForm.ShowForm();
        }

        private void btnColonies_Click(object sender, EventArgs e)
        {
            ColoniesForm.ShowForm();
        }

        private void btnShips_Click(object sender, EventArgs e)
        {
            ShipsForm.ShowForm();
        }

        private void btnNewGame_Click(object sender, EventArgs e)
        {
            Player.StartingPlayer black = new Player.StartingPlayer("Black", Color.Blue, null);//new GalWarAI.GalWarAI());
            Player.StartingPlayer blue = new Player.StartingPlayer("Blue", Color.Cyan, null);//new GalWarAI.GalWarAI());
            Player.StartingPlayer green = new Player.StartingPlayer("Green", Color.Green, null);//new GalWarAI.GalWarAI());
            Player.StartingPlayer pink = new Player.StartingPlayer("Pink", Color.Magenta, null);//new GalWarAI.GalWarAI());
            Player.StartingPlayer red = new Player.StartingPlayer("Red", Color.Red, null);//new GalWarAI.GalWarAI());
            Player.StartingPlayer yellow = new Player.StartingPlayer("Yellow", Color.Gold, null);//new GalWarAI.GalWarAI());
            Game = new Game(new Player.StartingPlayer[] { black, blue, green, pink, red, yellow },
                    Game.Random.GaussianOE(13, .13, .13, 5.2), Game.Random.GaussianCapped(.39, .21, .13));

            mouse = new PointForm(ClientSize.Width / 2, ClientHeight / 2);
            StartGame();

            Game.StartGame(this);

            saved = false;
            RefreshAll();
        }

        private void btnLoadGame_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                LoadGame(openFileDialog1.FileName);
                SelectNewTurn();
            }
        }

        private void LoadGame(string filePath)
        {
            saveFileDialog1.InitialDirectory = Path.GetDirectoryName(filePath);
            saveFileDialog1.FileName = Path.GetFileName(filePath);

            Game oldGame = Game;
            Game = Game.LoadGame(filePath);
            LoadLog(oldGame);

            dialogTile = _selected = panning = null;
            anomExp = null;
            hold = new HashSet<SpaceObject>();
            holdPersistent = new HashSet<SpaceObject>();
            movedTroops = new HashSet<Tuple<PopCarrier, PopCarrier>>();

            StartGame();

            saved = true;
            RefreshAll();
        }

        private void btnAutosaveView_Click(object sender, EventArgs e)
        {
            Game.AutoSavePath = null;

            openFileDialog1.InitialDirectory = Game.AutoSavePath;
            openFileDialog1.FileName = "1.gws";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string[] files = Directory.GetFiles(GetAutosaveFolder(), "*.gws", SearchOption.AllDirectories);

                int min = int.MaxValue, max = int.MinValue;
                foreach (string file in files)
                {
                    string fileName = Path.GetFileNameWithoutExtension(file).Split('_')[0];
                    int turn;
                    if (int.TryParse(fileName, out turn))
                    {
                        min = Math.Min(min, turn);
                        max = Math.Max(max, turn);
                    }
                }

                tbTurns.Visible = true;
                tbTurns.Maximum = max;
                tbTurns.Value = max;
                tbTurns.Minimum = min;
                tbTurns.Value = min;

                tbTurns.TickFrequency = (max - min) / 39;

                tbTurns_Scroll(null, null);
                tbTurns_MouseLeave(null, null);
            }
        }

        private void tbTurns_Scroll(object sender, EventArgs e)
        {
            int turn;
            while ((Game == null ? -1 : Game.Turn) != (turn = (int)tbTurns.Value))
            {
                string filePath = GetAutosaveFolder() + "\\" + turn + "_1.gws";
                if (File.Exists(filePath))
                    try
                    {
                        LoadGame(filePath);
                        return;
                    }
                    catch
                    {
                    }
                try
                {
                    if (Game == null || Game.Turn < turn)
                        ++tbTurns.Value;
                    else
                        --tbTurns.Value;
                }
                catch
                {
                    Application.Exit();
                    return;
                }
            }
        }

        private void tbTurns_MouseEnter(object sender, EventArgs e)
        {
            this.tbTurns.Focus();
        }
        private void tbTurns_MouseLeave(object sender, EventArgs e)
        {
            this.pnlHUD.Focus();
        }

        private string GetAutosaveFolder()
        {
            return Path.GetDirectoryName(openFileDialog1.FileName);
        }

        private void StartGame()
        {
            if (!started)
            {
                started = true;
                pnlHUD.Show();
                btnNewGame.Hide();
                btnLoadGame.Hide();
                btnAutosaveView.Hide();

                if (!isDialog)
                    dialog.StartGame();

                SetScale(TextScale);
                panX = float.MinValue;
                panY = float.MaxValue;
                VerifyScalePan();
            }
        }

        private void btnSaveGame_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string filePath = saveFileDialog1.FileName;
                saveFileDialog1.InitialDirectory = Path.GetDirectoryName(filePath);
                saveFileDialog1.FileName = Path.GetFileName(filePath);

                Game.SaveGame(filePath);
                saved = true;
            }
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            VerifyScalePan();
        }

        private void MainForm_MouseDown(object sender, MouseEventArgs e)
        {
            Tile clicked = GetTile(e.Location);
            if (clicked != GetSelectedTile())
            {
                Ship selected = GetSelectedShip();
                if (selected != null && selected.Player.IsTurn && selected.Vector != null && Tile.PathFind(selected).Contains(clicked))
                    vector = selected;
            }
            if (vector == null)
                panning = clicked;
        }
        private void MainForm_MouseUp(object sender, MouseEventArgs e)
        {
            panning = null;
            vector = null;
        }
        private void MainForm_MouseLeave(object sender, EventArgs e)
        {
            panning = null;
            vector = null;
        }

        private void MainForm_MouseMove(object sender, MouseEventArgs e)
        {
            Tile tile = GetTile(e.Location);
            if (panning != null && sender != pnlHUD && tile != panning)
            {
                // TODO: vector - left click drag to vector should not select target tile
                Ship ship = GetSelectedShip();
                if (panning == selected && ship != null && ship.Player.IsTurn)
                    VectorShip(ship, tile);
                else
                    DragPan(e.Location);
            }

            if (sender == pnlHUD)
                mouse = new PointForm(pnlHUD.Location.X + e.Location.X, pnlHUD.Location.Y + e.Location.Y);
            else
                mouse = e.Location;

            if (started)
            {
                Tile raw = GetTile(mouse);
                lblLoc.Text = GetLoction(raw);
            }
        }
        private static string GetLoction(Tile tile)
        {
            return Tile.GetDistance(Game.Center, tile) + "  :  " + tile.GetLoction();
        }
        private void DragPan(PointForm point)
        {
            int diff = point.X - mouse.X;
            panX += diff;

            diff = point.Y - mouse.Y;
            panY += diff;

            VerifyPan();
            InvalidateMap();
        }

        private void VectorShip(Ship ship, Tile vector)
        {
            if (vector != ship.Vector)
            {
                ship.Vector = vector;
                InvalidateMap();
            }
        }

        private void MainForm_MouseWheel(object sender, MouseEventArgs e)
        {
            float oldScale = scale;
            Tile tile = GetTile(mouse);

            if (e.Delta < 0)
                scale /= RandScale(-e.Delta);
            else
                scale *= RandScale(e.Delta);

            VerifyScale();

            panY -= (scale - oldScale) * tile.Y;
            panX -= (scale - oldScale) * (tile.X + (tile.Y % 2 != 0 ? 1 : 0));

            VerifyPan();
            InvalidateMap();
        }
        private static float RandScale(int delta)
        {
            return 1 + Game.Random.GaussianCapped(delta / 910f, 0.13f);
        }

        private Tile GetTile(PointForm point)
        {
            if (Game == null)
                return null;
            int y = (int)Math.Floor((point.Y - panY) / scale);
            int x = (int)Math.Floor(((point.X - panX) - (y % 2 == 0 ? 0 : scale / 2f)) / scale);
            return Game.GetTile(x, y);
        }

        private void btnProduction_Click(object sender, EventArgs e)
        {
            ChangeBuild(GetSelectedColony());
        }

        private Buildable ChangeBuild(Colony colony, double production, bool floor, out bool pause)
        {
            return ChangeBuild(colony, true, production, floor, out pause);
        }
        public void ChangeBuild(Colony colony)
        {
            bool pause;
            ChangeBuild(colony, false, 0, false, out pause);
        }
        private Buildable ChangeBuild(Colony colony, bool callback, double production, bool floor, out bool pause)
        {
            SelectTile(colony.Tile);
            RefreshAll();

            ShipDesign obsolete;
            Buildable buildable = ProductionForm.ShowForm(colony, callback, production, floor, out pause, out obsolete);
            if (!callback)
            {
                colony.StartBuilding(this, buildable, pause);
                if (obsolete != null)
                    MainForm.Game.CurrentPlayer.MarkObsolete(MainForm.GameForm, obsolete);
            }

            saved = false;
            RefreshAll();
            return buildable;
        }

        private void btnProdRepair_Click(object sender, EventArgs eventArgs)
        {
            Colony colony = GetSelectedColony();
            if (colony.RepairShip == null)
            {
                SelectTileDialog(selected, false);
                if (selected != null)
                    colony.RepairShip = (GetSelectedTile().SpaceObject as Ship);
            }
            else
            {
                colony.RepairShip = null;
            }

            saved = false;
            RefreshAll();
        }

        private void SelectTileDialog(Tile tile, bool build)
        {
            if (build)
                dialog.pnlBuild.SetColony(((Planet)tile.SpaceObject).Colony);
            dialog.pnlBuild.Visible = build;
            dialog.btnCancel.Visible = true;

            dialog.isBuild = build;
            dialog.dialogTile = tile;
            selected = tile;

            dialog.RefreshAll();

            dialog.Location = Location;
            dialog.Size = Size;

            if (dialog.ShowDialog() == DialogResult.Cancel)
                selected = null;

            Location = dialog.Location;
            Size = dialog.Size;

            pnlBuild.Visible = false;
            btnCancel.Visible = false;
        }

        private void btnGoldRepair_Click(object sender, EventArgs e)
        {
            Ship ship = GetSelectedShip();

            if (ship.HP < ship.MaxHP && !ship.HasRepaired)
            {
                int HP = SliderForm.ShowForm(new GoldRepair(ship));

                if (HP > 0)
                    ship.GoldRepair(this, HP);
            }
            else
            {
                AutoRepairForm.ShowForm(ship);
            }

            saved = false;
            RefreshAll();
        }

        private void btnAutoRepairShips_Click(object sender, EventArgs e)
        {
            RepairAllForm.ShowForm();

            saved = false;
            RefreshAll();
        }

        private void btnDisband_Click(object sender, EventArgs e)
        {
            showMoves = false;
            InvalidateMap();

            DisbandShip(GetSelectedShip());

            saved = false;
            RefreshAll();
        }
        public bool DisbandShip(Ship ship)
        {
            Colony colony = null;
            foreach (Tile neighbor in Tile.GetNeighbors(ship.Tile))
            {
                Planet planet = neighbor.SpaceObject as Planet;
                if (planet != null)
                {
                    colony = planet.Colony;
                    if (colony != null)
                        break;
                }
            }

            int production = -1;
            double gold = Player.RoundGold(ship.DisbandValue) + Player.RoundGold(ship.GetDestroyGold());

            Buildable buildable = null;
            if (colony != null && colony.Player.IsTurn)
            {
                buildable = colony.CurBuild;
                production = colony.GetAddProduction(ship.DisbandValue);
            }

            if (buildable != null && ShowOption("Disband for " + production + " production?"))
            {
                ship.Disband(this, colony);
                return true;
            }
            if (ShowOption("Disband for " + FormatDouble(gold) + " gold?"))
            {
                ship.Disband(this, null);
                return true;
            }
            return false;
        }

        private void btnUndo_Click(object sender, EventArgs e)
        {
            showMoves = false;

            SelectTile(Game.Undo(this));

            PopCarrier selected = GetSelectedPopCarrier();
            UnHold(selected);
            movedTroops.RemoveWhere(delegate (Tuple<PopCarrier, PopCarrier> moved)
            {
                return (selected == moved.Item1 || selected == moved.Item2);
            });

            saved = false;
            RefreshAll();
        }

        private void btnEndTurn_Click(object sender, EventArgs e)
        {
            showMoves = false;
            InvalidateMap();

            if (CheckMovedShips() && CheckRepairedShips())
            {
                CombatForm.FlushLog();

                HashSet<Ship> check = new HashSet<Ship>();
                foreach (Ship ship in Game.CurrentPlayer.GetShips())
                    if (ship.GetRepairedFrom() != null)
                        check.Add(ship);

                IEnumerable<Tile> anomalies = Game.EndTurn(this);

                foreach (Ship ship in check)
                    if (ship.HP == ship.MaxHP)
                        holdPersistent.Remove(ship);
                hold.IntersectWith(holdPersistent);

                movedTroops.Clear();

                if (anomalies.Any())
                    ShowAnomalies(anomalies);
                else
                    SelectNewTurn();

                saved = false;
                RefreshAll();
            }
        }

        private void SelectNewTurn()
        {
            SelectNext();
            if (selected == null)
            {
                int max = int.MinValue;
                Colony select = null;
                foreach (Colony colony in Game.Random.Iterate(Game.CurrentPlayer.GetColonies()))
                    if (colony.Population > max)
                    {
                        max = colony.Population;
                        select = colony;
                    }
                SelectTile(select.Tile);
            }
            Center();
        }

        private bool CheckMovedShips()
        {
            bool end = true;

            foreach (Ship ship in Game.CurrentPlayer.GetShips())
                if (HasMoveLeft(ship))
                {
                    end = ShowOption("You have not moved all of your ships.  Are you sure you want to end your turn?");
                    break;
                }
            if (end)
                foreach (Colony colony in Game.CurrentPlayer.GetColonies())
                    if (CanBeInvaded(colony))
                    {
                        end = ShowOption("You have colonies that can be attacked.  End turn?");
                        break;
                    }

            return end;
        }
        private static bool HasMoveLeft(Ship ship)
        {
            if (ship.Player.IsTurn && !hold.Contains(ship))
            {
                if (ship.CurSpeed > 0)
                    return true;

                //determine if this ship can still do something with population, or be production repaired
                if (ship.MaxPop > 0 || ship.HP < ship.MaxHP)
                    foreach (Tile neighbor in Tile.GetNeighbors(ship.Tile))
                    {
                        Planet planet = neighbor.SpaceObject as Planet;
                        if (ship.HP < ship.MaxHP && planet != null)
                        {
                            //can be production repaired
                            if (planet.Colony != null && planet.Player.IsTurn && planet.Colony.RepairShip == null && planet.Colony.GetProductionIncome() > 0)
                                return true;
                            if (!(ship.MaxPop > 0))
                                break;
                        }
                        if (ship.MaxPop > 0)
                        {
                            PopCarrier popCarrier = null;

                            Ship neighborShip = neighbor.SpaceObject as Ship;
                            if (neighborShip != null)
                            {
                                //might can transfer population
                                if (neighborShip.Player == ship.Player)
                                    popCarrier = neighborShip;
                            }
                            else if (planet != null)
                            {
                                if (planet.Player == ship.Player)
                                {
                                    //might can transfer population
                                    popCarrier = planet.Colony;
                                }
                                else if (planet.Colony == null)
                                {
                                    //can colonize a planet
                                    if (ship.Colony && ship.AvailablePop == ship.Population && ship.Population > 0)
                                        return true;
                                }
                                else if (ship.AvailablePop > 0)
                                {
                                    //can invade a colony
                                    return true;
                                }
                            }

                            //can transfer population
                            if (popCarrier != null && (CanTransfer(ship, popCarrier) || (CanTransfer(popCarrier, ship)))
                                    && !AlreadyTransfered(ship, popCarrier) && !AlreadyTransfered(popCarrier, ship))
                                return true;
                        }
                    }
            }

            return false;
        }
        private static bool AlreadyTransfered(PopCarrier from, PopCarrier to)
        {
            return movedTroops.Contains(new Tuple<PopCarrier, PopCarrier>(from, to));
        }
        private static bool CanTransfer(PopCarrier from, PopCarrier to)
        {
            return (from.AvailablePop > 0 && to.FreeSpace > 0);
        }
        private bool CanBeInvaded(Colony colony)
        {
            if (colony.Player.IsTurn && !hold.Contains(colony.Planet))
            {
                bool showAtt = this.showAtt;
                this.showAtt = false;

                foreach (Tile neighbor in Tile.GetNeighbors(colony.Tile))
                {
                    Ship ship = (neighbor.SpaceObject as Ship);
                    if (ship != null && !ship.Player.IsTurn && (ship.Population > 0 || ship.DeathStar))
                        return true;
                }

                foreach (SpaceObject spaceObject in Game.GetSpaceObjects())
                {
                    Ship ship = (spaceObject as Ship);
                    if (ship != null && !ship.Player.IsTurn && (ship.Population > 0 || ship.DeathStar))
                    {
                        var totals = new Dictionary<Tile, float>();
                        AddShip(totals, new Dictionary<Tile, Point>(), ship.Player, ship, ship.MaxSpeed);
                        foreach (Tile neighbor in Tile.GetNeighbors(colony.Tile))
                            if (totals.ContainsKey(neighbor))
                                return true;
                    }
                }

                this.showAtt = showAtt;
            }

            return false;
        }

        private bool CheckRepairedShips()
        {
            foreach (Ship ship in Game.CurrentPlayer.GetShips())
                if (!ship.HasRepaired && ship.HP < ship.MaxHP && double.IsNaN(ship.AutoRepair))
                {
                    SelectTile(ship.Tile);
                    RefreshAll();

                    int hp = SliderForm.ShowForm(new GoldRepair(ship));
                    if (hp > 0)
                    {
                        ship.GoldRepair(this, hp);

                        saved = false;
                        RefreshAll();
                    }
                    else if (hp == -1 && double.IsNaN(ship.AutoRepair))
                    {
                        return false;
                    }
                }

            return true;
        }

        private void SelectNext()
        {
            List<SpaceObject> loop = new List<SpaceObject>();
            loop.AddRange(Game.CurrentPlayer.GetColonies());
            loop.AddRange(Game.CurrentPlayer.GetShips());

            SpaceObject spaceObject = GetSelectedColony();
            if (spaceObject == null)
                spaceObject = GetSelectedSpaceObject();

            int start = loop.IndexOf(spaceObject);
            int index = start + 1;
            if (start < 0)
            {
                start = loop.Count;
                index = 0;
            }

            while (true)
            {
                if (index == loop.Count)
                {
                    index = -1;
                }
                else if (DoSelect(loop[index]))
                {
                    SelectTile(loop[index].Tile);
                    break;
                }

                if (++index == start)
                {
                    start = -1;
                }
                else if (start == -1)
                {
                    selected = null;
                    break;
                }
            }
        }
        private bool DoSelect(SpaceObject spaceObject)
        {
            Ship ship = (spaceObject as Ship);
            bool retVal = (ship == null);
            if (retVal)
            {
                Colony colony = (Colony)spaceObject;

                retVal = !hold.Remove(colony.Planet);
                bool invaded = CanBeInvaded(colony);
                if (!retVal)
                    hold.Add(colony.Planet);
                retVal &= invaded;

                if (!invaded)
                    holdPersistent.Remove(colony.Planet);

            }
            else
            {
                retVal = HasMoveLeft(ship);
            }

            return retVal;
        }

        private void ShowAnomalies(IEnumerable<Tile> anomalies)
        {
            if (anomalies.Skip(1).Any())
            {
                int minX = int.MaxValue, maxX = int.MinValue, minY = int.MaxValue, maxY = int.MinValue;
                foreach (Tile anomaly in anomalies)
                    FindBounds(ref minX, ref minY, ref maxX, ref maxY, anomaly);
                minX -= 2;
                minY -= 2;
                maxX += 2;
                maxY += 2;

                scale = GetScale(minX, minY, maxX, maxY);
                VerifyScale();
                selected = Game.GetTile(Game.Random.Round((minX + maxX) / 2f), Game.Random.Round((minY + maxY) / 2f));
                Center();
                float t1 = panX, t2 = panY;
                SelectTile(Game.Random.SelectValue(anomalies));
                if (t1 != panX || t2 != panY)
                    ShowAnomalies(anomalies);
            }
            else
            {
                SelectTile(anomalies.Single());
                Center();
            }
        }

        private void chkEmphasis_CheckedChanged(object sender, EventArgs e)
        {
            if (emphasisEvent)
            {
                Game.CurrentPlayer.GoldEmphasis = chkGold.Checked;
                Game.CurrentPlayer.ResearchEmphasis = chkResearch.Checked;
                Game.CurrentPlayer.ProductionEmphasis = chkProduction.Checked;

                RefreshAll();
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!isDialog && Game != null)
                if (!saved && !ShowOption("Are you sure you want to quit without saving?", true))
                    e.Cancel = true;
                else
                    CombatForm.FlushLog();
        }

        private SpaceObject firstClick = null;
        private void MainForm_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ClickMouse(e, true);
        }
        //int meh = 0;
        private void MainForm_MouseClick(object sender, MouseEventArgs e)
        {
            firstClick = null;
            ClickMouse(e, false);

            //if (e.Button == System.Windows.Forms.MouseButtons.Right)
            //    meh = 0;
            //else
            //    ++meh;
        }

        private void ClickMouse(MouseEventArgs e, bool doubleClick)
        {
            if (started)
            {
                Tile clickedTile = GetTile(e.Location);
                if (e.Button == MouseButtons.Left)
                {
                    selected = clickedTile;
                    UnHold(GetSelectedSpaceObject());

                    if (isDialog && ValidDialogTile(selected, clickedTile.SpaceObject))
                    {
                        DialogResult = DialogResult.OK;
                        Close();
                        return;
                    }
                }
                else if (!isDialog && e.Button == MouseButtons.Right)
                {

                    showMoves = false;
                    InvalidateMap();

                    if (doubleClick && firstClick != null && hold.Contains(firstClick))
                    {
                        holdPersistent.Add(firstClick);
                        firstClick = null;
                        return;
                    }
                    firstClick = clickedTile.SpaceObject;

                    Colony colony = GetSelectedColony();
                    Ship target = clickedTile.SpaceObject as Ship;
                    if (colony != null && target != null && colony.Player != target.Player && colony.HP > 0)
                    {
                        CombatForm.ShowForm(colony, target);
                    }
                    else
                    {
                        Tile selectedTile = GetSelectedTile();

                        Ship ship = GetSelectedShip();
                        int oldSpeed = -1;
                        if (ship != null)
                            oldSpeed = ship.CurSpeed;

                        bool selectNext = true;

                        if (selectedTile != null && (ship == null || ship.Player.IsTurn))
                            if (Tile.IsNeighbor(clickedTile, selectedTile))
                                selectNext &= RightClick(clickedTile, ref ship);
                            else if (clickedTile == selectedTile)
                                hold.Add(GetSelectedSpaceObject());

                        if (ship != null)
                        {
                            Combatant defender = null;
                            Planet planet = clickedTile.SpaceObject as Planet;
                            if (planet == null)
                                defender = clickedTile.SpaceObject as Combatant;
                            else if (oldSpeed == 0 || !ship.Player.IsTurn || !Tile.IsNeighbor(ship.Tile, planet.Tile))
                                defender = planet.Colony;
                            if (defender != null && ship.Player != defender.Player &&
                                    !(ship.Player.IsTurn && defender is Colony && ship.AvailablePop > 0 && Tile.IsNeighbor(defender.Tile, ship.Tile)))
                            {
                                Colony defCol = defender as Colony;
                                if (defCol != null && ship.Population > 0)
                                    InvadeCalculatorForm.ShowForm(ship, defCol);
                                else if (defender.HP > 0)
                                    selectNext &= Attack(ship, defender);
                            }
                        }

                        if (ship != null && ship.Player.IsTurn && ship.Vector != null)
                        {
                            if (ship.Vector == clickedTile)
                            {
                                if (ship.Tile != panning)
                                    ship.VectorZOC = !ship.VectorZOC;
                                selectNext = false;
                            }
                            else if (ship.Tile == clickedTile)
                            {
                                while (ship.CurSpeed > 0)
                                {
                                    List<Tile> path = Tile.PathFind(ship);
                                    if (path.Count > 1 && TargetTile(path[1], ship))
                                        selectNext = false;
                                    else
                                        break;

                                    if (ship.Tile == ship.Vector)
                                    {
                                        ship.Vector = null;
                                        break;
                                    }
                                }
                            }
                        }

                        if (selectNext && (selectedTile == null || ship == null || !ship.Player.IsTurn || !HasMoveLeft(ship) || ship.CurSpeed == oldSpeed))
                            SelectNext();

                        saved = false;
                    }
                }

                RefreshAll();
            }
        }

        private void UnHold(SpaceObject spaceObject)
        {
            if (spaceObject != null && spaceObject.Player != null && spaceObject.Player.IsTurn)
            {
                hold.Remove(spaceObject);
                holdPersistent.Remove(spaceObject);
            }
        }

        private bool ValidDialogTile(Tile tile, SpaceObject spaceObject)
        {
            if (dialogTile != null)
                foreach (Tile neighbor in Tile.GetNeighbors(dialogTile))
                {
                    if (neighbor == tile)
                    {
                        if (isBuild)
                        {
                            return (spaceObject == null);
                        }
                        else
                        {
                            Ship ship = (spaceObject as Ship);
                            return (ship != null && ship.HP < ship.MaxHP && ship.Player.IsTurn);
                        }
                    }
                }
            return false;
        }

        private bool RightClick(Tile adjacentTile, ref Ship refShip)
        {
            bool selectNext = true;

            SpaceObject spaceObject = GetSelectedSpaceObject();

            bool switchTroops = false;
            Planet planet = (spaceObject as Planet);
            if (planet != null && planet.Colony != null && adjacentTile.SpaceObject is Ship)
            {
                spaceObject = adjacentTile.SpaceObject;
                adjacentTile = planet.Tile;
                switchTroops = true;
            }

            Ship ship;
            if ((ship = (spaceObject as Ship)) != null)
            {
                if (ship.Player.IsTurn)
                {
                    ship.Vector = null;

                    Planet trgPlanet = null;
                    Ship trgShip = null;
                    Anomaly trgAnomaly = null;
                    if (adjacentTile.SpaceObject == null)
                        TargetTile(adjacentTile, ship);
                    else if ((trgShip = (adjacentTile.SpaceObject as Ship)) != null)
                        selectNext &= TargetShip(trgShip, ship, switchTroops);
                    else if ((trgPlanet = (adjacentTile.SpaceObject as Planet)) != null)
                        selectNext &= TargetPlanet(trgPlanet, ship, switchTroops);
                    else if ((trgAnomaly = (adjacentTile.SpaceObject as Anomaly)) != null)
                        selectNext &= targetAnomaly(selectNext, ship, trgAnomaly);

                    if (trgShip == null && !selectNext && !(trgPlanet != null && trgPlanet.Colony != null && trgPlanet.Colony.HP > 0))
                        refShip = null;
                }
            }

            return selectNext;
        }

        private bool targetAnomaly(bool selectNext, Ship ship, Anomaly trgAnomaly)
        {
            Tile tile = trgAnomaly.Tile;
            ship.Explore(this, trgAnomaly);
            if (holdPersistent.Remove(tile.SpaceObject))
            {
            }
            else
            {
            }
            return false;
        }

        private bool TargetTile(Tile targetTile, Ship ship)
        {
            if (ship.CurSpeed > 0 && targetTile.SpaceObject == null && Ship.CheckZOC(Game.CurrentPlayer, ship.Tile, targetTile))
            {
                ship.Move(this, targetTile);
                SelectTile(targetTile);
                return true;
            }
            return false;
        }

        private bool TargetShip(Ship targetShip, Ship ship, bool switchTroops)
        {
            bool selectNext = true;

            if (targetShip.Player.IsTurn)
                selectNext &= MoveTroops(ship, targetShip, switchTroops);

            return selectNext;
        }

        private bool Attack(Ship attacker, Combatant defender)
        {
            return CombatForm.ShowForm(attacker, defender);
        }

        public static void Attacked(Combatant defender)
        {
            holdPersistent.Remove(defender as Ship);
        }

        private bool TargetPlanet(Planet targetPlanet, Ship ship, bool switchTroops)
        {
            bool selectNext = true;

            Colony targetColony = targetPlanet.Colony;
            if (targetColony == null)
            {
                selectNext = ColonizePlanet(ship, targetPlanet);
            }
            else if (targetColony.Player.IsTurn)
            {
                if (ship.DeathStar && !switchTroops)
                    selectNext = BombardFriendly(ship, targetColony);
                else
                    selectNext = MoveTroops(ship, targetColony, switchTroops);
            }
            else
            {
                InvadePlanet(ship, targetColony);

                selectNext = false;
            }

            return selectNext;
        }

        private bool ColonizePlanet(Ship ship, Planet planet)
        {
            bool selectNext = true;

            bool bombard = true;
            if (ship.Colony && ship.AvailablePop == ship.Population && ship.AvailablePop > 0)
            {
                double goldCost = planet.ColonizationCost;
                string cost = FormatDouble(goldCost);
                if (!ship.Player.HasGold(goldCost))
                {
                    MessageBox.Show("You need " + cost + " gold to colonize this planet.");
                }
                else if (ShowOption("Colonize planet for " + cost + " gold?"))
                {
                    bombard = false;
                    ship.Colonize(this, planet);
                    SelectTile(planet.Tile);
                }
            }

            if (bombard && ship.CurSpeed > 0 && ship.DeathStar && (!ship.Colony || ShowOption("Bombard planet?")))
            {
                if (ship.CurSpeed == 1)
                    SelectTile(planet.Tile);
                selectNext = false;

                ship.Bombard(this, planet);
            }

            return selectNext;
        }

        private bool BombardFriendly(Ship ship, Colony targetColony)
        {
            bool selectNext = true;
            if (ship.DeathStar && ShowOption("Bombard planet?"))
            {
                if (ship.CurSpeed == 1)
                    SelectTile(targetColony.Tile);
                selectNext = false;

                ship.Bombard(this, targetColony.Planet);
            }
            return selectNext;
        }

        private bool MoveTroops(PopCarrier from, PopCarrier to, bool switchTroops)
        {
            if (switchTroops)
            {
                PopCarrier temp = from;
                from = to;
                to = temp;
            }
            return MoveTroops(from, to);
        }

        private bool MoveTroops(PopCarrier from, PopCarrier to)
        {
            if (from.MaxPop > 0 && to.MaxPop > 0)
            {
                int troops = SliderForm.ShowForm(new MoveTroops(from, to, false));
                if (troops > 0)
                {
                    SelectTile(to.Tile);
                    from.MovePop(this, troops, to);

                    movedTroops.Add(new Tuple<PopCarrier, PopCarrier>(from, to));
                    movedTroops.Add(new Tuple<PopCarrier, PopCarrier>(to, from));
                }
                return false;
            }
            return true;
        }

        private void InvadePlanet(Ship ship, Colony colony)
        {
            Planet planet = colony.Planet;

            SelectTile(planet.Tile);
            RefreshAll();

            int gold = 0, troops = ship.AvailablePop;
            if (troops > 0)
                if (colony.Population > 0)
                    gold = SliderForm.ShowForm(new Invade(ship, colony));
                else
                    troops = SliderForm.ShowForm(new MoveTroops(ship, colony, false));

            bool selectShip = true;
            if (troops > 0 && gold > 0)
            {
                ship.Invade(this, colony, troops, gold);

                if (!planet.Dead)
                    selectShip = false;
            }
            else if (ship.CurSpeed > 0 && (colony.HP > 0 || ship.AvailablePop == 0 || ShowOption("Bombard planet?")))
            {
                if (colony.HP > 0 && (!ship.DeathStar || !ShowOption("Bombard planet?")))
                    CombatForm.ShowForm(ship, colony);
                else if (ship.DeathStar || colony.Population > 0)
                    ship.Bombard(this, colony.Planet);

                if (ship.CurSpeed == 0 && !planet.Dead)
                    selectShip = false;
            }
            if (selectShip)
                SelectTile(ship.Tile);
        }

        public void SetLocation(Form form)
        {
            PointForm loc;
            try
            {
                loc = PointToScreen(mouse);
            }
            catch (ObjectDisposedException e)
            {
                Console.WriteLine(e);

                return;
            }

            int x = loc.X - form.Width / 2;
            int y = loc.Y - form.Height / 2;

            Rectangle bounds = Screen.FromControl(this).WorkingArea;

            if (x < bounds.X)
                x = bounds.X;
            else if (x + form.Width > bounds.X + bounds.Width - pnlHUD.Width)
                x = bounds.X + bounds.Width - pnlHUD.Width - form.Width;

            if (y < bounds.Y)
                y = bounds.Y;
            else if (y + form.Height > bounds.Y + bounds.Height)
                y = bounds.Y + bounds.Height - form.Height;

            form.Location = new PointForm(x, y);
        }

        private void lblGold_Click(object sender, EventArgs e)
        {
            int i1, i2, i3, i4, ships = 0;
            double d1, d2, d3, d4, income = 0, upkeep = 0, basic, total;
            foreach (Colony colony in Game.CurrentPlayer.GetColonies())
            {
                double gold;
                colony.GetTurnValues(out i1, out gold, out i2, out i4);
                income += gold;
                upkeep += colony.Upkeep;
            }
            foreach (Ship ship in Game.CurrentPlayer.GetShips())
            {
                upkeep += ship.Upkeep;
                ships += ship.BaseUpkeep;
            }
            Game.CurrentPlayer.GetTurnIncome(out d1, out i1, out d2, out total);
            MainForm.Game.CurrentPlayer.GetTurnIncome(out d3, out i3, out d4, out basic, false);

            income = Player.RoundGold(income);
            upkeep = Player.RoundGold(upkeep);
            total = Player.RoundGold(total);

            LabelsForm.ShowForm("Num Ships", Game.CurrentPlayer.GetShips().Count.ToString(), "Ship Upk", ships.ToString(),
                    "Repairs", FormatDouble(Game.CurrentPlayer.GetAutoRepairCost()), string.Empty, string.Empty,
                    "Income", FormatIncome(income), "Upkeep", FormatIncome(-upkeep), //"Base", FormatIncome(basic),
                    "Other", FormatIncome(total - income + upkeep), "Total", FormatIncome(total),
                    "Minimum", FormatIncome(Game.CurrentPlayer.GetMinGold()));
        }

        private void lblResearch_Click(object sender, EventArgs e)
        {
            ShowResearchFocus();
        }

        private void ShowResearchFocus()
        {
            ResearchFocusForm.ShowForm();
            RefreshAll();
        }

        private void lblLoc_Click(object sender, EventArgs e)
        {
            MouseButtons button = ((MouseEventArgs)e).Button;
            if (button == MouseButtons.Left)
            {
                Point start = new Point(-Game.Center.X, -Game.Center.Y);
                LabelsForm.ShowForm("Start", start.ToString(), string.Empty, string.Empty, "Galaxy Size", FormatDouble(Game.MapDeviation),
                        "Anomalies", FormatPct(Game.AnomalyPct, true), "Planets", FormatPct(Game.PlanetPct / Game.AnomalyPct, true),
                        string.Empty, string.Empty, "Prod/Upk", FormatDouble(1 / Consts.GetProductionUpkeepMult(Game.MapSize)));
            }
            else if (button == MouseButtons.Right)
            {
                showDist = !showDist;
                showMoves = false;
                InvalidateMap();
            }
        }

        private void lblTop_Click(object sender, EventArgs e)
        {
            Tile selected = GetSelectedTile();
            if (selected != null)
            {
                Wormhole wormhole = selected.Wormhole;
                if (wormhole != null)
                {
                    Tile next = null;
                    foreach (Tile t in wormhole.Tiles)
                        if (next == null)
                        {
                            if (t == selected)
                                next = selected;
                        }
                        else
                        {
                            next = t;
                            break;
                        }
                    if (next == null || next == selected)
                        next = wormhole.Tiles.First(t => t != selected);

                    Tile center = GetTile(new PointForm((this.Width - pnlHUD.Width) / 2, this.Height / 2));
                    if (Math.Abs(next.X - center.X) + Math.Abs(next.Y - center.Y) < 3 || !SelectedVisible())
                        next = selected;

                    SelectTile(next);
                    Center(next);
                    RefreshSelectedInfo();
                    InvalidateMap();
                }
            }
        }

        private void lbl4_Click(object sender, EventArgs e)
        {
            Colony colony = GetSelectedColony();
            if (colony != null)
                PlanetDefenseForm.ShowForm(colony);
        }

        private void lbl5_Click(object sender, EventArgs e)
        {
            Colony colony = GetSelectedColony();
            if (colony != null && colony.Player.IsTurn)
                LabelsForm.ShowColonyIncome(colony);
        }

        private SpaceObject GetSelectedSpaceObject()
        {
            if (selected == null)
                return null;
            return selected.SpaceObject;
        }

        private PopCarrier GetSelectedPopCarrier()
        {
            return (GetSelectedSpaceObject() as PopCarrier);
        }

        private Ship GetSelectedShip()
        {
            return (GetSelectedSpaceObject() as Ship);
        }

        private Planet GetSelectedPlanet()
        {
            return (GetSelectedSpaceObject() as Planet);
        }

        private Colony GetSelectedColony()
        {
            Planet planet = GetSelectedPlanet();
            if (planet != null)
                return planet.Colony;
            return null;
        }

        private void btnCombat_Click(object sender, EventArgs e)
        {
            CombatForm.ShowForm();
        }

        private void btnInvasion_Click(object sender, EventArgs e)
        {
            InvadeCalculatorForm.ShowForm();
        }

        private void btnCostCalc_Click(object sender, EventArgs e)
        {
            Colony colony = GetSelectedColony();
            if (colony != null && colony.Player.IsTurn)
                CostCalculatorForm.ShowForm(colony.CurBuild is BuildShip ? ((BuildShip)colony.CurBuild).ShipDesign : null);
            else
                CostCalculatorForm.ShowForm(GetSelectedShip());
        }

        private void pnlInfo_MouseClick(object sender, MouseEventArgs e)
        {
            if (GetSelectedShip() != null && e.Button == MouseButtons.Right)
                CostCalculatorForm.ShowForm(GetSelectedShip());
        }

        #endregion //Events

        #region Refresh

        public void RefreshAll()
        {
            RefreshAll(false);
        }
        public void RefreshAll(bool suppressLog)
        {
            if (!suppressLog && anomExp != null)
                OnRefresh();

            if (!ended && Game.GetPlayers().Count < 2)
            {
                ended = true;
                Game.AutoSave();
                TextForm.ShowForm(Game.GetGameResult());
            }

            RefreshCurrentPlayer();
            RefreshSelectedInfo();
            InvalidateMap();

            btnGraphs.Text = "View Empires";
            Player winner;
            double winChance = Game.GetResearchVictoryChance(out winner);
            if (winChance > 0)
            {
                btnGraphs.Text += " (" + FormatPctWithCheck(winChance) + ")";
                btnGraphs.BackColor = winner.Color;
                btnGraphs.UseVisualStyleBackColor = false;
            }
            else
            {
                btnGraphs.BackColor = SystemColors.Control;
                btnGraphs.UseVisualStyleBackColor = true;
            }

            btnUndo.Enabled = Game.CanUndo();

            if (!suppressLog)
                OnRefresh();
        }

        private void OnRefresh()
        {
            if (!CombatForm.OnRefresh(anomExp) && anomExp != null)
                ShowExploreMessage(Anomaly.AnomalyType.Experience);
            anomExp = null;
        }

        private void InvalidateMap()
        {
            Invalidate(GetInvalidateRectangle(ClientRectangle, pnlHUD.Location.X));
        }

        public static Rectangle GetInvalidateRectangle(Rectangle client, int width)
        {
            return new Rectangle(client.X, client.Y, width, client.Height);
        }

        private void RefreshCurrentPlayer()
        {
            int playerIndex = Game.currentPlayer;
            IEnumerable<Player> players = Game.GetPlayers();
            lblPrev.BackColor = (playerIndex > 0 ? players.ElementAt(playerIndex - 1).Color : Color.Black);
            lblNext.BackColor = (playerIndex + 1 < players.Count() ? players.ElementAt(playerIndex + 1).Color : Color.Black);

            lblPlayer.BackColor = Game.CurrentPlayer.Color;
            lblPlayer.Text = Game.Turn.ToString() + " - " + Game.CurrentPlayer.Name;
            RefreshPlayerInfo();
        }

        private void RefreshPlayerInfo()
        {
            lblPopulation.Text = Game.CurrentPlayer.GetPopulation().ToString();
            ColorForIncome(lblGold, FormatDouble(Game.CurrentPlayer.Gold));

            int research;
            double population, production, gold;
            Game.CurrentPlayer.GetTurnIncome(out population, out research, out production, out gold);

            FormatIncome(lblPopInc, population);
            FormatIncome(lblGoldInc, gold, true);
            FormatIncome(lblResearch, research);
            lblRsrchTot.Text = Game.CurrentPlayer.GetCurrentResearch().ToString();
            lblRsrchPct.Text = FormatPctWithCheck(Game.CurrentPlayer.GetResearchChance(research)) + new string(' ', lblResearch.Text.Length + 6);
            FormatIncome(lblProduction, production);
            lblProdTot.Text = Game.CurrentPlayer.GetColonies().Sum(colony => colony.Production2).ToString();

            emphasisEvent = false;
            chkGold.Checked = Game.CurrentPlayer.GoldEmphasis;
            chkResearch.Checked = Game.CurrentPlayer.ResearchEmphasis;
            chkProduction.Checked = Game.CurrentPlayer.ProductionEmphasis;
            emphasisEvent = true;

            bool enabled = !Game.CurrentPlayer.NegativeGold();
            chkGold.Enabled = enabled;
            chkResearch.Enabled = enabled;
            chkProduction.Enabled = enabled;
        }

        public static void FormatIncome(Label label, double income)
        {
            FormatIncome(label, income, false);
        }

        public static void FormatIncome(Label label, double income, bool forceDouble)
        {
            ColorForIncome(label, FormatIncome(income, forceDouble));

            if (label.TextAlign == ContentAlignment.MiddleRight)
                label.Width = (label.Text.Contains(".") ? 46 : 35);
        }

        public static void ColorForIncome(Label label, String text)
        {
            label.ForeColor = (text.StartsWith("-") ? Color.DarkRed : Color.Black);
            label.Text = text;
        }

        public static string FormatIncome(double income)
        {
            return FormatIncome(income, true);
        }

        public static string FormatIncome(double income, bool forceDouble)
        {
            return (income > 0 ? "+" : string.Empty) + (forceDouble ? FormatDouble(income) : FormatUsuallyInt(income));
        }

        private void RefreshSelectedInfo()
        {
            ClearSelectedInfo();

            if (selected != null)
            {
                Player player = null;

                Ship ship = GetSelectedShip();
                Planet planet = GetSelectedPlanet();
                Anomaly anomaly = GetSelectedSpaceObject() as Anomaly;
                if (ship != null)
                    player = ShipInfo(ship);
                else if (planet != null)
                    player = PlanetInfo(planet);
                else if (anomaly != null)
                    AnomalyInfo(anomaly);
                else if (selected.Wormhole != null)
                {
                    lbl1.Text = "Exits";
                    lbl1Inf.Text = selected.Wormhole.Tiles.Count().ToString();
                    lbl2.Text = "Age";
                    lbl2Inf.Text = (Game.Turn - selected.Wormhole.CreatedTurn).ToString();
                }

                int telNum = selected.WormholeNumber;
                if (telNum > 0)
                    lblTop.Text = "Wormhole " + telNum + (lblTop.Text.Length > 0 ? " - " + lblTop.Text : string.Empty);

                if (player != null)
                    PlayerInfo(player);
            }
        }

        private void ClearSelectedInfo()
        {
            if (!isDialog)
                pnlBuild.Visible = false;

            lblTop.BackColor = SystemColors.Control;
            lbl2Inf.ForeColor = Color.Black;
            lbl5Inf.ForeColor = Color.Black;

            lbl4.BorderStyle = BorderStyle.None;
            lbl5.BorderStyle = BorderStyle.None;

            lblTop.Text = string.Empty;
            lbl1.Text = string.Empty;
            lbl2.Text = string.Empty;
            lbl3.Text = string.Empty;
            lbl4.Text = string.Empty;
            lbl5.Text = string.Empty;
            lbl6.Text = string.Empty;
            lbl7.Text = string.Empty;
            lbl1Inf.Text = string.Empty;
            lbl2Inf.Text = string.Empty;
            lbl3Inf.Text = string.Empty;
            lbl4Inf.Text = string.Empty;
            lbl5Inf.Text = string.Empty;
            lbl6Inf.Text = string.Empty;
            lbl7Inf.Text = string.Empty;
            lblBottom.Text = string.Empty;

            btnProdRepair.Visible = false;
            btnProduction.Visible = false;
            btnGoldRepair.Visible = false;
            btnDisband.Visible = false;
        }

        private Player ShipInfo(Ship ship)
        {
            if (!isDialog && ship.Player.IsTurn)
            {
                btnGoldRepair.Visible = true;
                btnDisband.Visible = true;

                bool repair = (ship.HP < ship.MaxHP);
                btnGoldRepair.Enabled = repair;

                if (repair)
                {
                    btnGoldRepair.Text = (ship.HasRepaired ? "Auto Repair" : "Repair Ship");
                    double autoRepair = ship.AutoRepair;
                    if (autoRepair != 0)
                        btnGoldRepair.Text += string.Format(" ({0})", double.IsNaN(autoRepair) ? "M" : autoRepair.ToString("0.00"));
                }
            }

            lblTop.Text = ship.ToString();

            lbl1.Text = "Attack";
            lbl1Inf.Text = ship.Att.ToString();

            lbl2.Text = "Defense";
            lbl2Inf.Text = ship.Def.ToString();

            lbl3.Text = "HP";
            lbl3Inf.Text = ship.HP.ToString() + " / " + ship.MaxHP.ToString() + " - "
                   + FormatPctWithCheck(ship.HP / (double)ship.MaxHP);

            lbl4.Text = "Speed";
            lbl4Inf.Text = ship.CurSpeed.ToString();

            if (ship.Player.IsTurn)
            {
                lbl4Inf.Text += " / " + ship.MaxSpeed.ToString();

                lbl5.Text = "Upkeep";
                lbl5Inf.Text = ship.BaseUpkeep.ToString();
            }
            else if (ship.Repair > 0)
            {
                lbl5.Text = "Repair";
                lbl5Inf.Text = "+" + ship.Repair;
            }

            lbl6.Text = "Experience";
            lbl6Inf.Text = ship.GetTotalExp().ToString() + " (" + ship.NextExpType.ToString() + ")";

            if (ship.MaxPop > 0)
            {
                lbl7.Text = "Troops";
                lbl7Inf.Text = ship.Population.ToString() + " / " + ship.MaxPop.ToString();
                if (ship.Population > 0)
                    lbl7Inf.Text += " (" + FormatPct(ship.GetSoldierPct()) + ")";
            }

            if (ship.Colony)
            {
                double colonizationValue;
                string repair = string.Empty;
                if (ship.Player.IsTurn)
                {
                    colonizationValue = ship.ColonizationValue;
                    Colony repairedFrom = ship.GetRepairedFrom();
                    if (repairedFrom != null)
                        repair = " +" + FormatDouble(ship.GetColonizationValue(ship.GetHPForProd(repairedFrom.GetProductionIncome())) - colonizationValue);
                }
                else
                {
                    colonizationValue = CostCalculatorForm.CalcColonizationValue(ship);
                }

                lblBottom.Text += "Colony Ship (" + FormatDouble(colonizationValue) + repair + ")";
            }
            else if (ship.DeathStar)
            {
                lblBottom.Text = "Death Star (" + FormatInt(ship.BombardDamage) + ")";
            }

            return ship.Player;
        }

        private Player PlanetInfo(Planet planet)
        {
            lbl1.Text = "Quality";
            lbl1Inf.Text = planet.Quality.ToString();

            Colony colony = planet.Colony;
            if (colony == null)
            {
                lblTop.Text = "Uncolonized";
                lbl2.Text = "Cost";
                lbl2Inf.Text = FormatDouble(planet.ColonizationCost);

                return null;
            }
            else
            {
                return ColonyInfo(colony);
            }
        }

        private Player ColonyInfo(Colony colony)
        {
            if (colony.Player.IsTurn)
            {
                if (!isDialog)
                {
                    btnProduction.Visible = true;
                    btnProdRepair.Visible = true;

                    bool visible = pnlBuild.SetColony(colony);
                    pnlBuild.Visible = visible;
                }

                if (colony.RepairShip == null)
                {
                    bool enabled = false;
                    foreach (Tile tile in Tile.GetNeighbors(colony.Tile))
                    {
                        Ship ship = tile.SpaceObject as Ship;
                        if (ship != null && ship.HP < ship.MaxHP && ship.Player.IsTurn)
                        {
                            enabled = true;
                            break;
                        }
                    }
                    btnProdRepair.Enabled = enabled;
                    btnProdRepair.Text = "Repair Ship";
                }
                else
                {
                    btnProdRepair.Enabled = true;
                    btnProdRepair.Text = "Stop Repair";
                }
            }

            lbl2.Text = "Population";
            FormatIncome(lbl2Inf, colony.GetPopulationGrowth(), true);
            if (lbl2Inf.Text == "0.0")
                lbl2Inf.Text = string.Empty;
            lbl2Inf.Text = colony.Population.ToString() + " " + lbl2Inf.Text;

            lbl3.Text = "Soldiers";
            lbl3Inf.Text = FormatPct(colony.Player.IsTurn ? colony.GetSoldierPct() : colony.GetSoldierPct());

            lbl4.Text = "Defense";
            if (!colony.MinDefenses)
                lbl4Inf.Text = string.Format("{0} : {1}   ({2})", colony.Att, colony.Def, colony.HP);

            if (!colony.Player.IsTurn)
            {
                string soldierChange = FormatPct(colony.SoldierChange, true);
                if (soldierChange != "0.0%")
                    lbl3Inf.Text += string.Format(" ({1}{0})", soldierChange, colony.SoldierChange > 0 ? "+" : string.Empty);

                int attChange = colony.DefenseAttChange, defChange = colony.DefenseDefChange;
                if (attChange == colony.Att)
                    --attChange;
                if (defChange == colony.Def)
                    --defChange;
                double pdChange = colony.DefenseHPChange;
                if (colony.HP > 0)
                    pdChange = colony.HP - (colony.HP - colony.DefenseHPChange) / (colony.PDStrength / colony.HP)
                        * ShipDesign.GetPlanetDefenseStrength(colony.Att - attChange, colony.Def - defChange);
                else
                    ;
                string strChange = FormatUsuallyInt(pdChange);
                if (strChange != "0")
                    lbl4Inf.Text += string.Format(" ({1}{0})", strChange, pdChange > 0 ? "+" : string.Empty);

                lbl7.Text = "Production";
                lbl7Inf.Text = FormatDouble(colony.ProdGuess);
            }

            lbl4.BorderStyle = BorderStyle.FixedSingle;

            lbl5.Text = "Income";
            double income;
            if (colony.Player.IsTurn)
            {
                double population = 0, production = 0, gold = 0;
                int research = 0;
                colony.GetTurnIncome(ref population, ref production, ref gold, ref research, false);
                income = production + gold + research;
            }
            else
            {
                income = colony.GetTotalIncome();
            }
            FormatIncome(lbl5Inf, income, true);

            if (colony.Player.IsTurn)
            {
                lbl5Inf.Text += " (" + colony.GetProductionIncome() + ")";

                lbl5.BorderStyle = BorderStyle.FixedSingle;

                lbl6.Text = "Building";
                Ship repairShip = colony.RepairShip;
                if (repairShip != null)
                    lbl6Inf.Text = "Repair +" + FormatDouble(repairShip.GetHPForProd(colony.GetProductionIncome()));
                else if (colony.CurBuild is BuildGold)
                    lbl6Inf.Text = "Gold";
                else
                    lbl6Inf.Text = colony.CurBuild.ToString();

                lbl7Inf.Text = GetProdText(colony);
            }

            return colony.Player;
        }

        internal static string GetProdText(Colony colony)
        {
            return GetProdText(colony, colony.CurBuild, colony.CurBuild.Production, colony.PauseBuild);
        }

        internal static string GetProdText(Colony colony, Buildable build, double production, bool paused)
        {
            string retVal = string.Empty;
            retVal = FormatUsuallyInt(production);
            if (!paused)
                retVal = build.GetProdText(retVal);

            double prodInc = colony.GetAfterRepairProdInc();

            string inc;
            if (build is BuildGold)
            {
                retVal = string.Empty;
                prodInc /= Consts.GoldProductionForGold;
                inc = FormatDouble(prodInc);
            }
            else
            {
                if (build is StoreProd)
                    prodInc *= Consts.StoreProdRatio;
                else if (build is BuildInfrastructure)
                    prodInc += colony.GetInfrastructureIncome();
                inc = FormatUsuallyInt(prodInc);
            }

            double incDbl;
            if (prodInc != 0 && (!double.TryParse(inc.TrimEnd('%'), out incDbl) || incDbl != 0))
            {
                if (retVal.Length > 0)
                    retVal += " ";
                if (prodInc > 0)
                    retVal += "+";
                retVal += inc;
            }
            return retVal;
        }

        //public static string GetBuildingDefense(Colony colony, Buildable buildable, double production)
        //{
        //    double newAtt, newDef, newHP, d1, d2, d3;
        //    colony.GetPlanetDefenseInc(buildable, production, MainForm.Game.CurrentPlayer.GetCurrentResearch(), out newAtt, out newDef, out newHP, out d1, out d2, out d3, false, false);
        //    return GetBuildingDefense(colony, newAtt - colony.Att, newDef - colony.Def, newHP - colony.HP);
        //}
        public static string GetBuildingDefense(Colony colony, double newAtt, double newDef, double newHP)
        {
            if (colony.MinDefenses)
            {
                ++newAtt;
                ++newDef;
            }
            return string.Format("{3}{0}:{4}{1} ({5}{2})", FormatUsuallyInt(newAtt), FormatUsuallyInt(newDef), FormatUsuallyInt(newHP),
                    newAtt > 0 ? "+" : string.Empty, newDef > 0 ? "+" : string.Empty, newHP > 0 ? "+" : string.Empty);
        }

        //public static string GetBuildingSoldiers(double curPop, double popInc, double curSoldiers, double soldierInc)
        //{
        //    double soldiers = (curSoldiers + soldierInc) / (curPop + popInc) - curSoldiers / curPop;
        //    string soldierChange = FormatPct(soldiers, true);
        //    if (soldierChange != "0.0%" && soldiers > 0)
        //        soldierChange = "+" + soldierChange;
        //    return soldierChange;
        //}

        private void AnomalyInfo(Anomaly anomaly)
        {
            lblTop.Text = "Anomaly";
            lbl1.Text = "Planet";
            lbl1Inf.Text = FormatPct(Game.GetPlanetChance(anomaly), true);
        }

        private void PlayerInfo(Player player)
        {
            if (lblTop.Text == string.Empty)
                lblTop.Text = player.Name;
            lblTop.BackColor = player.Color;
        }

        public static bool ShowOption(string message)
        {
            return ShowOption(message, false);
        }

        public static bool ShowOption(string message, bool alert)
        {
            return (MessageBox.Show(message, string.Empty, MessageBoxButtons.OKCancel, alert ? MessageBoxIcon.Warning : MessageBoxIcon.None) == DialogResult.OK);
        }

        public static string FormatInt(double value)
        {
            return value.ToString("0");
        }

        public static string FormatUsuallyInt(double value)
        {
            return FormatDouble(value).TrimEnd('0').TrimEnd('.');
        }

        public static string FormatDouble(double value)
        {
            return Player.RoundGold(value).ToString("0.0");
        }

        public static string FormatPctWithCheck(double pct)
        {
            string retVal = FormatPct(pct);
            if (pct < 1 && retVal == "100%")
                retVal = ">99%";
            else if (pct > 0 && retVal == "0%")
                retVal = "< 1%";
            return retVal;
        }

        public static string FormatPct(double pct)
        {
            return FormatPct(pct, false);
        }

        public static string FormatPct(double pct, bool place)
        {
            pct *= 100;
            return (place ? FormatDouble(pct) : FormatInt(pct)) + "%";
        }

        #endregion //Refresh

        #region IEventHandler

        Tile IEventHandler.getBuildTile(Colony colony)
        {
            showMoves = false;
            InvalidateMap();

            SelectTileDialog(colony.Tile, true);
            return GetSelectedTile();
        }

        Buildable IEventHandler.getNewBuild(Colony colony, double production, bool floor, out bool pause)
        {
            showMoves = false;
            InvalidateMap();

            return ChangeBuild(colony, production, floor, out pause);
        }

        int IEventHandler.MoveTroops(Colony fromColony, int max, int totalPop, double soldiers, bool extraCost)
        {
            showMoves = false;

            if (fromColony != null)
                SelectTile(fromColony.Tile);
            RefreshAll();

            return SliderForm.ShowForm(new MoveTroops(fromColony, max, totalPop, soldiers, extraCost));
        }

        bool IEventHandler.Continue(Planet planet, int initPop, int initQuality, int stopPop, int stopQuality, int finalPop, int finalQuality)
        {
            bool showPop = true; // ( planet.Player != null && !planet.Player.IsTurn );
            string format = "{0}Quality: {2}" + (showPop ? ",    Population: {1}" : string.Empty) + "{0}";
            Func<int, int, string> GetString = (pop, quality) => string.Format(format, Environment.NewLine, Math.Max(pop, 0).ToString().PadLeft(4), Math.Max(quality, -1).ToString().PadLeft(4));
            return ShowOption(String.Format("Planet with{1}has been reduced to{2}{0}Continue attacking to {3}?",
                    Environment.NewLine, GetString(initPop, initQuality), GetString(stopPop, stopQuality),
                    finalQuality >= 0 ? "reduce Quality down to " + finalQuality : "destroy planet?"));
        }

        bool IEventHandler.ConfirmCombat(Combatant attacker, Combatant defender)
        {
            showMoves = false;

            if (attacker is Ship)
                SelectTile(attacker.Tile);
            else
                SelectTile(defender.Tile);
            RefreshAll(true);

            return CombatForm.ShowForm(attacker, defender, true);
        }

        bool IEventHandler.Explore(Anomaly.AnomalyType anomalyType, params object[] info)
        {
            showMoves = false;
            InvalidateMap();

            switch (anomalyType)
            {

                case Anomaly.AnomalyType.AskProductionOrDefense:
                    SelectTile(((Colony)info[0]).Tile);
                    RefreshAll();
                    return ShowOption("Take +" + ((int)info[1]) + " Producton or build Defenses?");

                case Anomaly.AnomalyType.AskResearchOrGold:
                    return ShowOption("Take Research or +" + FormatDouble((double)info[0]) + " Gold?");

                case Anomaly.AnomalyType.AskTerraform:
                    SelectTile(((Colony)info[0]).Tile);
                    RefreshAll();
                    return ShowOption("Terraform Planet?" + Environment.NewLine + "+" + ((int)info[1]) + " Quality" + Environment.NewLine + FormatIncome(-(double)info[2]) + " Gold");

                case Anomaly.AnomalyType.Terraform:
                    SelectTile(((Planet)info[0]).Tile);
                    RefreshAll();
                    MessageBox.Show("+" + ((int)info[1]) + " Quality!");
                    return true;

                case Anomaly.AnomalyType.Death:
                case Anomaly.AnomalyType.Heal:
                    MessageBox.Show(FormatIncome((int)info[0]) + " HP!");
                    return true;

                case Anomaly.AnomalyType.Experience:
                    anomExp = (Ship)info[0];
                    return true;

                case Anomaly.AnomalyType.Gold:
                    MessageBox.Show("+" + FormatDouble((double)info[0]) + " Gold!");
                    return true;

                case Anomaly.AnomalyType.LostColony:
                    string str = "Hostile";
                    if (((Player)info[0]).IsTurn)
                        str = "Friendly";
                    MessageBox.Show(str + " Colony!");
                    return true;

                case Anomaly.AnomalyType.PickupPopulation:
                    MessageBox.Show("+" + info[0] + " Population!");
                    return true;

                case Anomaly.AnomalyType.PickupSoldiers:
                    MessageBox.Show("+" + FormatPct((double)info[0] / GetSelectedShip().Population) + " Soldiers!");
                    return true;

                case Anomaly.AnomalyType.SalvageShip:
                    string player = "Hostile";
                    if (((Player)info[0]).IsTurn)
                        player = "Friendly";
                    MessageBox.Show(player + " Ship!");
                    return true;

                case Anomaly.AnomalyType.Apocalypse:
                case Anomaly.AnomalyType.NewPlanet:
                case Anomaly.AnomalyType.PopulationGrowth:
                case Anomaly.AnomalyType.Wormhole:
                    ShowExploreMessage(anomalyType);
                    return true;

                case Anomaly.AnomalyType.PlanetDefenses:
                case Anomaly.AnomalyType.Production:
                case Anomaly.AnomalyType.Soldiers:
                case Anomaly.AnomalyType.SoldiersAndDefenses:
                    string msg = string.Empty;
                    if (info.Length > 0)
                    {
                        SelectTile(((Colony)info[0]).Tile);
                        RefreshAll();
                        msg = "+" + FormatUsuallyInt((double)info[1]) + " ";
                    }
                    ShowExploreMessage(anomalyType, msg);
                    return true;

                default:
                    throw new Exception();
            }
        }
        private static void ShowExploreMessage(Anomaly.AnomalyType anomalyType)
        {
            ShowExploreMessage(anomalyType, string.Empty);
        }
        private static void ShowExploreMessage(Anomaly.AnomalyType anomalyType, string msg)
        {
            MessageBox.Show(msg + Game.CamelToSpaces(anomalyType.ToString()).Replace(" And ", " and ") + "!");
        }

        void IEventHandler.OnResearch(ShipDesign newDesign, HashSet<ShipDesign> obsolete)
        {
            showMoves = false;
            RefreshAll();

            ResearchForm.ShowForm(newDesign, obsolete);

            ShowResearchFocus();
        }

        void IEventHandler.OnCombat(Combatant attacker, Combatant defender, int attack, int defense)
        {
            showMoves = false;

            CombatForm.OnCombat(attacker, defender, attack, defense);
        }

        void IEventHandler.OnLevel(Ship ship, double pct, int last, int needed)
        {
            showMoves = false;

            CombatForm.OnLevel(ship, pct, last, needed);
        }

        void IEventHandler.OnBombard(Ship ship, Planet planet, int freeDmg, int colonyDamage, int planetDamage)
        {
            showMoves = false;

            CombatForm.OnBombard(ship, planet, freeDmg, colonyDamage, planetDamage);
        }

        void IEventHandler.OnInvade(Ship ship, Colony colony, int attackers, double attSoldiers, double gold, double attack, double defense)
        {
            showMoves = false;

            CombatForm.OnInvade(ship, colony, attackers, attSoldiers, gold, attack, defense);
        }

        void IEventHandler.Event()
        {
            showMoves = false;

            //if (Game.CurrentPlayer.Name == "Pink")
            //{
            //    Refresh();
            //    MessageBox.Show(Environment.StackTrace);
            //    System.Threading.Thread.Sleep(1000);
            //}
        }

        #endregion

        #region Log

        private int flushed = 0;
        private string log = string.Empty;

        public void LogMsg(string format, params object[] args)
        {
            log += string.Format(format, args) + Environment.NewLine;
        }

        public void LogMsg()
        {
            LogMsg(Game);
        }
        private void LogMsg(Game game)
        {
            log += Environment.NewLine;

            try
            {
                using (StreamWriter streamWriter = new StreamWriter(GetLogPath(game), true))
                    streamWriter.Write(log.Substring(flushed));
                flushed = log.Length;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public string GetLog()
        {
            string retVal = log;
            const int max = 65536;
            if (retVal.Length > max)
                retVal = retVal.Substring(retVal.IndexOf(Environment.NewLine + Environment.NewLine, retVal.Length - max)).TrimStart();
            return retVal.TrimStart();
        }

        private string GetLogPath(Game game)
        {
            return saveFileDialog1.InitialDirectory + "\\" + game.ID + "_gw.log";
        }

        private void LoadLog(Game oldGame)
        {
            if (oldGame != null && log.Trim().Length > 0)
                LogMsg(oldGame);

            log = string.Empty;
            string logPath = GetLogPath(Game);
            if (File.Exists(logPath))
                using (StreamReader reader = new StreamReader(logPath))
                    log += reader.ReadToEnd();
            flushed = log.Length;

            log += "****** " + Game.ID + " " + Game.Turn + "_" + (Game.currentPlayer + 1) + " - " + DateTime.Now.ToString("MM/dd/yyyy h:mm tt") + " ***"
                    + Environment.NewLine + Environment.NewLine;
        }
    }

    #endregion //Log

}
