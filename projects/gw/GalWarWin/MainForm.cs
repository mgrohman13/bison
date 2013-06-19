using System;
using System.Collections.Generic;
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

        private static bool emphasisEvent = true, anomExp = false;

        private static Point? _selected = null, panning = null;
        private static HashSet<SpaceObject> hold, holdPersistent;

        private static HashSet<Tuple<PopCarrier, PopCarrier>> movedTroops;

        private static float panX, panY, scale;

        private MainForm(bool dialog)
        {
            InitializeComponent();
            MouseWheel += new MouseEventHandler(MainForm_MouseWheel);
            ResizeRedraw = true;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            Bounds = Screen.GetWorkingArea(this);

            isDialog = dialog;
            if (isDialog)
            {
                HideButtons(this);
                AcceptButton = btnCancel;
            }

            hold = new HashSet<SpaceObject>();
            holdPersistent = new HashSet<SpaceObject>();
            movedTroops = new HashSet<Tuple<PopCarrier, PopCarrier>>();
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
            string savePath;
            try
            {
                using (StreamReader reader = new StreamReader("savepath.txt"))
                    savePath = reader.ReadLine();
                if (!Directory.Exists(savePath))
                    savePath = null;
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

        private Point? selected
        {
            get
            {
                return _selected;
            }
            set
            {
                _selected = value;
                Center(false);
            }
        }
        private Tile GetSelectedTile()
        {
            if (selected == null)
                return null;
            return Game.GetTile(selected.Value);
        }
        private void SelectTile(Tile tile)
        {
            if (tile == null)
                selected = null;
            else
                selected = tile.Point;
        }

        private void SetBtnShowMovesText()
        {
            btnShowMoves.Text = ( ( showMoves && !showAtt ) ? "Enemy Attacks" : "Enemy Moves" );
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

        private void Center()
        {
            Center(true);
        }
        private void Center(bool always)
        {
            if (selected != null && ( always || !SelectedVisible() ))
                Center(selected.Value);
            else
                VerifyScalePan();
        }
        private void Center(Point point)
        {
            Point center = GetGamePoint(new PointForm(( this.Width - pnlHUD.Width ) / 2, this.Height / 2));
            panX += ( center.X - point.X ) * scale;
            panY += ( center.Y - point.Y ) * scale;
            VerifyScalePan();
        }
        private bool SelectedVisible()
        {
            Rectangle gameBounds = GetGameBounds();
            int x = selected.Value.X, y = selected.Value.Y;
            return ( x > gameBounds.Left + 1 && x < gameBounds.Right - 1 && y > gameBounds.Top + 1 && y < gameBounds.Bottom - 1 );
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

                    g.FillRectangle(Brushes.Black, 0, 0, width, height);

                    float minQuality = int.MaxValue, maxQuality = int.MinValue,
                           minPop = int.MaxValue, maxPop = int.MinValue, minStr = int.MaxValue, maxStr = int.MinValue;
                    foreach (Planet planet in Game.GetPlanets())
                    {
                        GetVals(ref minQuality, ref maxQuality, planet.Quality);
                        if (planet.Colony != null)
                            GetVals(ref minPop, ref maxPop, planet.Colony.Population);
                    }
                    foreach (Player player in Game.GetPlayers())
                        foreach (Ship ship in player.GetShips())
                            GetVals(ref minStr, ref maxStr, (float)ship.GetStrength() * ship.HP / (float)ship.MaxHP);

                    if (scale > TextScale)
                    {
                        float newSize = font.Size * scale / g.MeasureString("99%", font).Width;
                        if (newSize > 13f)
                            newSize = 13f;
                        else if (newSize < 1f)
                            newSize = 1f;
                        if (newSize != font.Size)
                        {
                            font.Dispose();
                            font = new Font("arial", newSize);
                        }
                    }

                    Rectangle gameBounds = GetGameBounds();

                    if (scale > GridScale)
                    {
                        RectangleF[] rects = new RectangleF[( gameBounds.Width + 3 ) * ( gameBounds.Height + 3 )];
                        int a = -1;
                        for (int x = gameBounds.Left - 1 ; x <= gameBounds.Right + 1 ; ++x)
                            for (int y = gameBounds.Top - 1 ; y <= gameBounds.Bottom + 1 ; ++y)
                                rects[++a] = GetDrawRect(x, y);
                        using (Pen pen = new Pen(Color.White, 1f))
                            g.DrawRectangles(pen, rects);
                    }

                    //if (testDistance != null)
                    //{
                    //    RectangleF[] rects = new RectangleF[( gameBounds.Width + 3 ) * ( gameBounds.Height + 3 )];
                    //    int a = -1;
                    //    foreach (Tile t in testDistance)
                    //        rects[++a] = GetDrawRect(t.X, t.Y);
                    //    using (Brush b = new SolidBrush(Game.CurrentPlayer.Color))
                    //        g.FillRectangles(b, rects);
                    //}

                    if (selected != null)
                        DrawObject(g, gameBounds, selected.Value, null);
                    foreach (Tuple<Point, Point> teleporter in Game.GetTeleporters())
                    {
                        DrawObject(g, gameBounds, teleporter.Item1, null);
                        DrawObject(g, gameBounds, teleporter.Item2, null);
                    }
                    if (isDialog && dialogTile != null)
                        foreach (Tile neighbor in Tile.GetNeighbors(dialogTile))
                            DrawObject(g, gameBounds, neighbor.Point, null);

                    Dictionary<Tile, float> moves = null;
                    if (showMoves && scale > TextScale)
                    {
                        moves = GetMoves();
                        foreach (Tile tile in moves.Keys)
                            DrawObject(g, gameBounds, tile.Point, moves);
                    }

                    foreach (SpaceObject spaceObject in Game.GetSpaceObjects())
                        DrawObject(g, gameBounds, spaceObject.Tile.Point, moves, minQuality, maxQuality, minPop, maxPop, minStr, maxStr);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);

                    using (Font font = new Font("arial", 13f))
                        g.DrawString(e.ToString(), font, Brushes.White, 0, 0);
                }
            }
        }
        private void DrawObject(Graphics g, Rectangle gameBounds, Point point, Dictionary<Tile, float> moves)
        {
            DrawObject(g, gameBounds, point, moves, float.NaN, float.NaN, float.NaN, float.NaN, float.NaN, float.NaN);
        }
        private void DrawObject(Graphics g, Rectangle gameBounds, Point point, Dictionary<Tile, float> moves, float minQuality, float maxQuality, float minPop, float maxPop, float minStr, float maxStr)
        {
            int x = point.X, y = point.Y;
            if (x > gameBounds.Left - 2 && x < gameBounds.Right + 2 && y > gameBounds.Top - 2 && y < gameBounds.Bottom + 2)
            {
                SpaceObject spaceObject = Game.GetSpaceObject(point);
                RectangleF rect = GetDrawRect(x, y);

                int telNum;
                if (Game.GetTeleporter(point, out telNum) != null)
                {
                    Brush brush = Brushes.DarkGray;
                    if (Game.GetTeleporters().Count > 1)
                    {
                        float telCount = Game.GetTeleporters().Count + 1;
                        float min = 520 / telCount / telCount;
                        int color = Game.Random.Round(min + telNum * ( 255 - min ) / telCount);
                        brush = new SolidBrush(Color.FromArgb(color, color, color));
                    }
                    g.FillRectangle(brush, RectangleF.Inflate(rect, .5f, .5f));
                }

                if (spaceObject is Anomaly)
                    g.FillRectangle(Brushes.White, Inflate(scale, rect, 1, 1, 1, .6f, .13f));

                Ship ship;
                Planet planet;
                if (( planet = ( spaceObject as Planet ) ) != null)
                    DrawPlanet(g, scale, rect, planet, minQuality, maxQuality, minPop, maxPop);
                else if (( ship = ( spaceObject as Ship ) ) != null)
                    DrawShip(g, scale, rect, ship, minStr, maxStr);

                if (scale > GridScale || point == selected || ( isDialog && ValidDialogTile(point, spaceObject) ))
                    DrawBorder(g, point, spaceObject, rect, scale);

                if (moves != null && scale > TextScale)
                {
                    Tile tile = Game.GetTile(point);
                    float move;
                    if (moves.TryGetValue(tile, out move))
                    {
                        string s = ( showAtt ? FormatDouble(move) : FormatInt(move) );
                        if (showAtt)
                            foreach (Tile neighbor in Tile.GetNeighbors(tile))
                            {
                                planet = neighbor.SpaceObject as Planet;
                                if (planet != null && planet.Colony != null && !planet.Colony.Player.IsTurn)
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
            return new RectangleF(panX + scale * x + ( y % 2 == 0 ? 0 : scale / 2f ), panY + scale * y, scale, scale);
        }

        private Rectangle GetGameBounds()
        {
            Point min = GetGamePoint(new PointForm(0, 0));
            Point max = GetGamePoint(new PointForm(ClientSize.Width - pnlHUD.Width, ClientSize.Height));
            return new Rectangle(min.X, min.Y, max.X - min.X, max.Y - min.Y);
        }

        private void GetVals(ref float min, ref float max, float value)
        {
            min = (float)Math.Min(min, Math.Sqrt(value));
            max = (float)Math.Max(max, Math.Sqrt(value));
        }

        private void DrawBorder(Graphics g, Point point, SpaceObject spaceObject, RectangleF rect, float scale)
        {
            Planet planet = spaceObject as Planet;
            Ship ship = spaceObject as Ship;

            float size;
            if (scale <= GridScale)
                size = 1f;
            else if (point == selected)
                size = 3f;
            else if (planet != null || ( isDialog ? ValidDialogTile(point, spaceObject) : ship != null &&
                    ship.Player.IsTurn && ship.CurSpeed > 0 ))
                size = 2f;
            else
                size = 1f;
            if (dialogTile != null && point == new Point(dialogTile.X, dialogTile.Y))
                ++size;

            if (scale <= GridScale || size != 1f)
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
            rect = Inflate(scale, rect, ship.GetStrength() * ship.HP / (float)ship.MaxHP, minStr, maxStr, .666f, .13f);
            using (Brush brush = new SolidBrush(ship.Player.Color))
                g.FillRectangle(brush, rect);

            if (scale > ShipDetailScale)
            {
                if (ship.DeathStar || ship.Population > 0)
                    g.DrawRectangle(Pens.White, rect.X, rect.Y, rect.Width, rect.Height);

                double pct = ship.HP / (double)ship.MaxHP;
                if (pct < 1)
                {
                    Pen pen = Pens.Black;
                    if (ship.Player.IsTurn && !ship.HasRepaired && ship.AutoRepair == 0)
                        pen = new Pen(Color.White, 2);

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

        private RectangleF Inflate(float scale, RectangleF rect, double value, float min, float max, float smallInvsPct, float inc)
        {
            if (scale <= GridScale)
            {
                float big = 1 - ( 1 - ( smallInvsPct + inc ) ) / 3f;
                if (big > ( scale - 1f ) / scale)
                    big = ( scale - 1f ) / scale;
                smallInvsPct = .5f + 2f / scale + ( smallInvsPct - .5f ) / 3f;
                if (smallInvsPct > big)
                    smallInvsPct = big;
                inc = big - smallInvsPct;
                if (inc < 0)
                    inc = 0;
            }
            float inflate = (float)( -scale * ( 1f - ( smallInvsPct + inc * ( ( Math.Sqrt(value) - min + 1 ) / ( max - min + 1 ) ) ) ) );
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
                int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
                foreach (SpaceObject spaceObject in Game.GetSpaceObjects())
                    FindBounds(ref minX, ref minY, ref maxX, ref maxY, spaceObject.Tile.Point);
                foreach (Tuple<Point, Point> teleporter in Game.GetTeleporters())
                {
                    FindBounds(ref minX, ref minY, ref maxX, ref maxY, teleporter.Item1);
                    FindBounds(ref minX, ref minY, ref maxX, ref maxY, teleporter.Item2);
                }
                --minX;
                --minY;
                ++maxX;
                ++maxY;

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
                    float minPanX = width - scale * ( xDiameter + minX ) - padding;
                    float maxPanX = padding - scale * minX;
                    //check min last so the map is anchored at the top
                    if (panX > maxPanX)
                        panX = maxPanX;
                    if (panX < minPanX)
                        panX = minPanX;

                    float minPanY = height - scale * ( yDiameter + minY ) - padding;
                    float maxPanY = padding - scale * minY;
                    //check max last so the map is anchored at the right
                    if (panY < minPanY)
                        panY = minPanY;
                    if (panY > maxPanY)
                        panY = maxPanY;
                }
            }
        }
        private static void FindBounds(ref int minX, ref int minY, ref int maxX, ref int maxY, Point point)
        {
            minX = Math.Min(minX, point.X);
            minY = Math.Min(minY, point.Y);
            maxX = Math.Max(maxX, point.X);
            maxY = Math.Max(maxY, point.Y);
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
            return Math.Min(( height - padding * 2 ) / yDiameter, ( width - padding * 2 ) / xDiameter);
        }

        private Dictionary<Tile, float> GetMoves()
        {
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

            return totals;
        }

        private static float GetStatFromValue(float value)
        {
            if (value < 1.5f)
                return value;

            const float digit = .1f, div = 1f / digit;
            int min = (int)Math.Floor(Math.Pow(value, 1f / 3f) * div) + 3, max = (int)Math.Ceiling(Math.Sqrt(value) * div);
            float stat = TBSUtil.FindValue(delegate(int test)
            {
                return ( ShipDesign.GetStatValue(test / div) > value );
            }, min, max, true) / div;
            if (( ShipDesign.GetStatValue(stat) - value ) > ( value - ShipDesign.GetStatValue(stat - digit) ))
                stat -= digit;

            const float adj = digit / 10f, half = .5f;
            if (Math.Abs(stat % 1f - half) < Consts.FLOAT_ERROR)
                if (( ShipDesign.GetStatValue(stat + half) - value ) > ( value - ShipDesign.GetStatValue(stat - half) ))
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
            if (showAtt && ( ( colony = combatant as Colony ) == null || !colony.MinDefenses ))
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

                Ship ship = tile.SpaceObject as Ship;
                if (showPlayer != null && ( tile.SpaceObject == null ||
                        ( !showAtt && tile.SpaceObject is Anomaly ) || ( ship != null && ship.Player == showPlayer ) ))
                {
                    float val;
                    totals.TryGetValue(tile, out val);
                    totals[tile] = val + add;
                }
            }
        }

        private void AddTiles(Dictionary<Tile, Point> retVal, Player enemy, Tile tile, int speed, bool ignoreZoc)
        {
            foreach (Tile neighbor in Tile.GetNeighbors(tile))
            {
                Point v1;
                retVal.TryGetValue(neighbor, out v1);
                int damage = v1.X;
                int move = v1.Y;
                int newDamage = Math.Max(speed, damage);
                int newMove = speed - ( showAtt ? 1 : 0 );

                Ship ship;
                if (newMove > move && ( neighbor.SpaceObject == null || neighbor.SpaceObject is Anomaly ||
                        ( ( ship = neighbor.SpaceObject as Ship ) != null && ship.Player == enemy ) )
                        && ( ignoreZoc || Ship.CheckZOC(enemy, tile, neighbor) ))
                {
                    retVal[neighbor] = new Point(newDamage, newMove);
                    AddTiles(retVal, enemy, neighbor, newMove - ( showAtt ? 0 : 1 ), ignoreZoc);
                }
                else if (newDamage > damage)
                {
                    retVal[neighbor] = new Point(newDamage, move);
                }
            }
        }

        #endregion //Drawing

        #region Events

        private void btnShowMoves_Click(object sender, EventArgs e)
        {
            if (scale < TextScale)
                SetScale(TextScale);

            showAtt = ( showMoves ? !showAtt : false );
            showMoves = true;
            InvalidateMap();
        }
        private void SetScale(float value)
        {
            scale = (float)( value * ( 1 + Consts.FLOAT_ERROR ) );
            VerifyScale();
        }

        private void btnGraphs_Click(object sender, EventArgs e)
        {
            GraphsForm.ShowForm();
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
                    Game.Random.GaussianOE(13, .13, .13, 5.2), Game.Random.GaussianCapped(.169, .26, .039));

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

            Game = Game.LoadGame(filePath);

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
                    int turn;
                    if (int.TryParse(Path.GetFileNameWithoutExtension(file), out turn))
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

                tbTurns.TickFrequency = ( max - min ) / 39;

                tbTurns_Scroll(null, null);
                tbTurns_MouseLeave(null, null);
            }
        }

        private void tbTurns_Scroll(object sender, EventArgs e)
        {
            int turn;
            while (( Game == null ? -1 : Game.Turn ) != ( turn = (int)tbTurns.Value ))
            {
                string filePath = GetAutosaveFolder() + "\\" + turn + ".gws";
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
            panning = GetGamePoint(e.Location);
        }
        private void MainForm_MouseUp(object sender, MouseEventArgs e)
        {
            panning = null;
        }
        private void MainForm_MouseLeave(object sender, EventArgs e)
        {
            panning = null;
        }

        private void MainForm_MouseMove(object sender, MouseEventArgs e)
        {
            Point gamePoint = GetGamePoint(e.Location);
            if (panning != null && sender != pnlHUD && gamePoint != panning)
            {
                //Ship ship = GetSelectedShip();
                //if (panning == selected && ship != null && ship.Player.IsTurn)
                //    VectorShip(ship, gamePoint);
                //else
                DragPan(e.Location);
            }

            if (sender == pnlHUD)
                mouse = new PointForm(pnlHUD.Location.X + e.Location.X, pnlHUD.Location.Y + e.Location.Y);
            else
                mouse = e.Location;

            if (started)
            {
                Point raw = GetGamePoint(mouse);
                lblLoc.Text = Tile.GetDistance(Game.GetTile(Game.Center), Game.GetTile(raw)) + "  :  "
                        + new Point(raw.X - Game.Center.X, raw.Y - Game.Center.Y);
            }
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

        //private void VectorShip(Ship ship, Point gamePoint)
        //{
        //    Tile vector = Game.GetTile(gamePoint);
        //    if (vector != ship.Vector)
        //    {
        //        ship.Vector = vector;

        //        InvalidateMap();
        //    }
        //}

        private void MainForm_MouseWheel(object sender, MouseEventArgs e)
        {
            float oldScale = scale;
            Point gamePoint = GetGamePoint(mouse);

            if (e.Delta < 0)
                scale /= RandScale(-e.Delta);
            else
                scale *= RandScale(e.Delta);

            VerifyScale();

            panY -= ( scale - oldScale ) * gamePoint.Y;
            panX -= ( scale - oldScale ) * ( gamePoint.X + ( gamePoint.Y % 2 != 0 ? 1 : 0 ) );

            VerifyPan();
            InvalidateMap();
        }
        private static float RandScale(int delta)
        {
            return 1 + Game.Random.GaussianCapped(delta / 910f, 0.13f);
        }

        private Point GetGamePoint(PointForm point)
        {
            int y = (int)Math.Floor(( point.Y - panY ) / scale);
            int x = (int)Math.Floor(( ( point.X - panX ) - ( y % 2 == 0 ? 0 : scale / 2f ) ) / scale);
            return new Point(x, y);
        }

        private void btnProduction_Click(object sender, EventArgs e)
        {
            Colony colony = GetSelectedColony();
            colony.StartBuilding(this, ChangeBuild(colony));

            saved = false;
            RefreshAll();
        }

        private Buildable ChangeBuild(Colony colony)
        {
            SelectTile(colony.Tile);
            RefreshAll();

            return ProductionForm.ShowForm(colony);
        }

        private void btnProdRepair_Click(object sender, EventArgs eventArgs)
        {
            Colony colony = GetSelectedColony();
            if (colony.RepairShip == null)
            {
                SelectTileDialog(selected.Value, false);
                if (selected != null)
                    colony.RepairShip = ( GetSelectedTile().SpaceObject as Ship );
            }
            else
            {
                colony.RepairShip = null;
            }

            saved = false;
            RefreshAll();
        }

        private void SelectTileDialog(Point point, bool build)
        {
            Tile tile = Game.GetTile(point);
            if (build)
                dialog.pnlBuild.SetColony(( (Planet)tile.SpaceObject ).Colony);
            dialog.pnlBuild.Visible = build;
            dialog.btnCancel.Visible = true;

            dialog.isBuild = build;
            dialog.dialogTile = tile;
            selected = point;

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

            Ship ship = GetSelectedShip();

            Colony colony = null;
            foreach (Tile neighbor in Tile.GetNeighbors(GetSelectedTile()))
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
                buildable = colony.Buildable;
                production = colony.GetAddProduction(ship.DisbandValue);
            }

            if (buildable != null && ShowOption("Disband for " + production + " production?"))
                ship.Disband(this, colony);
            else if (ShowOption("Disband for " + FormatDouble(gold) + " gold?"))
                ship.Disband(this, null);

            saved = false;
            RefreshAll();
        }

        private void btnUndo_Click(object sender, EventArgs e)
        {
            showMoves = false;

            SelectTile(Game.Undo(this));

            Ship selectedShip = GetSelectedShip();
            UnHold(selectedShip);
            movedTroops.RemoveWhere(delegate(Tuple<PopCarrier, PopCarrier> moved)
            {
                return ( selectedShip == moved.Item1 || selectedShip == moved.Item2 );
            });

            saved = false;
            RefreshAll();
        }

        private void btnEndTurn_Click(object sender, EventArgs e)
        {
            showMoves = false;

            if (CheckGold() && CheckMovedShips() && CheckRepairedShips())
            {
                CombatForm.FlushLog();

                HashSet<Ship> check = new HashSet<Ship>();
                foreach (Ship ship in Game.CurrentPlayer.GetShips())
                    if (ship.GetRepairedFrom() != null)
                        check.Add(ship);

                List<Anomaly> anomalies = Game.EndTurn(this);

                foreach (Ship ship in check)
                    if (ship.HP == ship.MaxHP)
                        holdPersistent.Remove(ship);
                hold.IntersectWith(holdPersistent);

                movedTroops.Clear();

                if (anomalies.Count > 0)
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

        private bool CheckGold()
        {
            bool end = true;
            if (Game.CurrentPlayer.Gold < 0)
                end = ShowOption("You are out of gold.  Partial production may be sold and one or more ships disbanded.  Are you sure you want to end your turn?", true);
            return end;
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
                            if (planet.Colony != null && planet.Colony.Player.IsTurn && planet.Colony.RepairShip == null && planet.Colony.GetProductionIncome() > 0)
                                return true;
                            if (!( ship.MaxPop > 0 ))
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
                            if (popCarrier != null && ( CanTransfer(ship, popCarrier) || ( CanTransfer(popCarrier, ship) ) )
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
            return ( from.AvailablePop > 0 && to.FreeSpace > 0 );
        }
        private bool CanBeInvaded(Colony colony)
        {
            if (colony.Player.IsTurn && !hold.Contains(colony.Planet))
            {
                bool showAtt = this.showAtt;
                this.showAtt = false;

                foreach (Tile neighbor in Tile.GetNeighbors(colony.Tile))
                {
                    Ship ship = ( neighbor.SpaceObject as Ship );
                    if (ship != null && !ship.Player.IsTurn && ( ship.Population > 0 || ship.DeathStar ))
                        return true;
                }

                foreach (SpaceObject spaceObject in Game.GetSpaceObjects())
                {
                    Ship ship = ( spaceObject as Ship );
                    if (ship != null && !ship.Player.IsTurn && ( ship.Population > 0 || ship.DeathStar ))
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
            Ship ship = ( spaceObject as Ship );
            bool retVal = ( ship == null );
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

        private void ShowAnomalies(List<Anomaly> anomalies)
        {
            if (anomalies.Count > 1)
            {
                int minX = int.MaxValue, maxX = int.MinValue, minY = int.MaxValue, maxY = int.MinValue;
                foreach (Anomaly anomaly in anomalies)
                    FindBounds(ref minX, ref minY, ref maxX, ref maxY, anomaly.Tile.Point);
                minX -= 2;
                minY -= 2;
                maxX += 2;
                maxY += 2;

                scale = GetScale(minX, minY, maxX, maxY);
                VerifyScale();
                selected = new Point(Game.Random.Round(( minX + maxX ) / 2f), Game.Random.Round(( minY + maxY ) / 2f));
                Center();
                double t1 = panX, t2 = panY;
                SelectTile(anomalies[Game.Random.Next(anomalies.Count)].Tile);
                if (t1 != panX || t2 != panY)
                    throw new Exception();
            }
            else
            {
                SelectTile(anomalies[0].Tile);
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
        private void MainForm_MouseClick(object sender, MouseEventArgs e)
        {
            firstClick = null;
            ClickMouse(e, false);
        }

        //int testDist;
        //IEnumerable<Tile> testDistance;

        private void ClickMouse(MouseEventArgs e, bool doubleClick)
        {
            if (started)
            {
                Point gamePoint = GetGamePoint(e.Location);

                Tile clickedTile = Game.GetTile(gamePoint);
                if (e.Button == MouseButtons.Left)
                {

                    //if (selected == gamePoint)
                    //    ++testDist;
                    //else
                    //    testDist = 0;

                    selected = gamePoint;
                    UnHold(GetSelectedSpaceObject());

                    //testDistance = Game.TestDistance(selected.Value, testDist);

                    if (isDialog && ValidDialogTile(selected.Value, clickedTile.SpaceObject))
                    {
                        DialogResult = DialogResult.OK;
                        Close();
                        return;
                    }
                }
                else if (!isDialog && e.Button == MouseButtons.Right)
                {

                    showMoves = false;

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

                        if (selectedTile != null && ( ship == null || ship.Player.IsTurn ))
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
                                    !( ship.Player.IsTurn && defender is Colony && ship.AvailablePop > 0 && Tile.IsNeighbor(defender.Tile, ship.Tile) ))
                            {
                                Colony defCol = defender as Colony;
                                if (defCol != null && ship.Population > 0)
                                    InvadeCalculatorForm.ShowForm(ship, defCol);
                                else if (defender.HP > 0)
                                    selectNext &= Attack(ship, defender);
                            }
                        }

                        if (selectNext && ( selectedTile == null || ship == null || !ship.Player.IsTurn || !HasMoveLeft(ship) || ship.CurSpeed == oldSpeed ))
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

        private bool ValidDialogTile(Point point, SpaceObject spaceObject)
        {
            if (dialogTile != null)
                foreach (Tile neighbor in Tile.GetNeighbors(dialogTile))
                {
                    if (neighbor.Point == point)
                    {
                        if (isBuild)
                        {
                            return ( spaceObject == null );
                        }
                        else
                        {
                            Ship ship = ( spaceObject as Ship );
                            return ( ship != null && ship.HP < ship.MaxHP && ship.Player.IsTurn );
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
            Planet planet = ( spaceObject as Planet );
            if (planet != null && planet.Colony != null && adjacentTile.SpaceObject is Ship)
            {
                spaceObject = adjacentTile.SpaceObject;
                adjacentTile = planet.Tile;
                switchTroops = true;
            }

            Ship ship;
            if (( ship = ( spaceObject as Ship ) ) != null)
            {
                if (ship.Player.IsTurn)
                {
                    //try
                    //{
                    Planet trgPlanet = null;
                    Ship trgShip = null;
                    Anomaly trgAnomaly = null;
                    if (adjacentTile.SpaceObject == null)
                        TargetTile(adjacentTile, ship);
                    else if (( trgShip = ( adjacentTile.SpaceObject as Ship ) ) != null)
                        selectNext &= TargetShip(trgShip, ship, switchTroops);
                    else if (( trgPlanet = ( adjacentTile.SpaceObject as Planet ) ) != null)
                        selectNext &= TargetPlanet(trgPlanet, ship, switchTroops);
                    else if (( trgAnomaly = ( adjacentTile.SpaceObject as Anomaly ) ) != null)
                        selectNext &= targetAnomaly(selectNext, ship, trgAnomaly);

                    if (trgShip == null && !selectNext && !( trgPlanet != null && trgPlanet.Colony != null && trgPlanet.Colony.HP > 0 ))
                        refShip = null;
                    //}
                    //catch (AssertException e)
                    //{
                    //    Console.WriteLine(e);
                    //}
                }
            }

            return selectNext;
        }

        private bool targetAnomaly(bool selectNext, Ship ship, Anomaly trgAnomaly)
        {
            ship.Explore(this, trgAnomaly);
            return false;
        }

        private void TargetTile(Tile targetTile, Ship ship)
        {
            if (ship.CurSpeed > 0 && Ship.CheckZOC(Game.CurrentPlayer, ship.Tile, targetTile))
            {
                ship.Move(this, targetTile);
                SelectTile(targetTile);
            }
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
                if (goldCost > ship.Player.Gold)
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

            if (bombard && ship.CurSpeed > 0 && ( !ship.Colony || ShowOption("Bombard planet?") ))
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
            if (ShowOption("Bombard planet?"))
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
                int troops = SliderForm.ShowForm(new MoveTroops(from, to));
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
                    troops = SliderForm.ShowForm(new MoveTroops(ship, colony));

            bool selectShip = true;
            if (troops > 0 && gold > -1)
            {
                ship.Invade(this, colony, troops, gold);

                if (!planet.Dead)
                    selectShip = false;
            }
            else if (ship.CurSpeed > 0 && ( colony.HP > 0 || ship.AvailablePop == 0 || ShowOption("Bombard planet?") ))
            {
                if (colony.HP > 0 && ( !ship.DeathStar || !ShowOption("Bombard planet?") ))
                    CombatForm.ShowForm(ship, colony);
                else
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
            int i1, i2, ships = 0;
            double d1, d2, income = 0, upkeep = 0, total;
            foreach (Colony colony in Game.CurrentPlayer.GetColonies())
            {
                double gold;
                colony.GetTurnValues(out i1, out gold, out i2);
                income += gold;
                upkeep += colony.Upkeep;
            }
            foreach (Ship ship in Game.CurrentPlayer.GetShips())
            {
                upkeep += ship.Upkeep;
                ships += ship.BaseUpkeep;
            }
            Game.CurrentPlayer.GetTurnIncome(out d1, out i1, out d2, out total);

            income = Player.RoundGold(income);
            upkeep = Player.RoundGold(upkeep);
            total = Player.RoundGold(total);

            LabelsForm.ShowForm("Num Ships", Game.CurrentPlayer.GetShips().Count.ToString(), "Ship Upk", ships.ToString(),
                    "Repairs", FormatIncome(-Game.CurrentPlayer.GetAutoRepairCost()), string.Empty, string.Empty,
                    "Income", FormatIncome(income), "Upkeep", FormatIncome(-upkeep),
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
            LabelsForm.ShowForm("Galaxy Size", FormatDouble(Game.MapDeviation),
                    "Anomalies", FormatPct(Game.AnomalyPct, true), "Planets", FormatPct(Game.PlanetPct / Game.AnomalyPct, true),
                    string.Empty, string.Empty, "Prod/Upk", FormatDouble(1 / Consts.GetProductionUpkeepMult(Game.MapSize)));
        }

        private void lblTop_Click(object sender, EventArgs e)
        {
            Tile selected = GetSelectedTile();
            if (selected != null)
            {
                Tile teleporter = selected.Teleporter;
                if (teleporter != null)
                {
                    Point point = teleporter.Point;
                    Point center = GetGamePoint(new PointForm(( this.Width - pnlHUD.Width ) / 2, this.Height / 2));
                    if (Math.Abs(point.X - center.X) + Math.Abs(point.Y - center.Y) < 3 || !SelectedVisible())
                        point = selected.Point;

                    Center(point);
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
            {
                double population = 0, production = 0, gold = 0, origGold;
                int research = 0, origProd;
                colony.GetTurnIncome(ref population, ref production, ref gold, ref research, false);
                colony.GetTurnValues(out origProd, out origGold, out research);
                gold = Player.RoundGold(gold);
                production = Player.RoundGold(production);

                LabelsForm.ShowForm("Income", ShowOrig(colony.GetTotalIncome(), production + gold + research), "Upkeep", FormatDouble(-colony.Upkeep), string.Empty, string.Empty,
                        "Gold", ShowOrig(origGold, gold), "Research", FormatDouble(research), "Production", ShowOrig(origProd, production));
            }
        }

        private string ShowOrig(double orig, double mod)
        {
            orig = Player.RoundGold(orig);
            string retVal = FormatDouble(mod);
            if (orig != mod)
                retVal = string.Format("({0}) {1}", FormatUsuallyInt(orig), retVal);
            return retVal;
        }

        private SpaceObject GetSelectedSpaceObject()
        {
            if (selected == null)
                return null;
            return Game.GetSpaceObject(selected.Value);
        }

        private Ship GetSelectedShip()
        {
            return ( GetSelectedSpaceObject() as Ship );
        }

        private Planet GetSelectedPlanet()
        {
            return ( GetSelectedSpaceObject() as Planet );
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
                CostCalculatorForm.ShowForm(colony.Buildable as ShipDesign);
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
            if (!suppressLog && anomExp)
                OnRefresh();

            if (!ended && Game.GetPlayers().Count < 2)
            {
                ended = true;
                TextForm.ShowForm(Game.GetGameResult());
                Game.AutoSave();
            }

            RefreshCurrentPlayer();
            RefreshSelectedInfo();
            InvalidateMap();

            btnUndo.Enabled = Game.CanUndo();

            if (!suppressLog)
                OnRefresh();
        }

        private void OnRefresh()
        {
            if (!CombatForm.OnRefresh(anomExp) && anomExp)
                ShowExploreMessage(Anomaly.AnomalyType.Experience);
            anomExp = false;
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
            lblRsrchPct.Text = FormatPctWithCheck(Game.CurrentPlayer.GetResearchChance(research));
            FormatIncome(lblProduction, production);

            emphasisEvent = false;
            chkGold.Checked = Game.CurrentPlayer.GoldEmphasis;
            chkResearch.Checked = Game.CurrentPlayer.ResearchEmphasis;
            chkProduction.Checked = Game.CurrentPlayer.ProductionEmphasis;
            emphasisEvent = true;
        }

        public static void FormatIncome(Label label, double income)
        {
            FormatIncome(label, income, false);
        }

        public static void FormatIncome(Label label, double income, bool forceDouble)
        {
            ColorForIncome(label, FormatIncome(income, forceDouble));

            if (label.TextAlign == ContentAlignment.MiddleRight)
                label.Width = ( label.Text.Contains(".") ? 46 : 35 );
        }

        public static void ColorForIncome(Label label, String text)
        {
            label.ForeColor = ( text.StartsWith("-") ? Color.DarkRed : Color.Black );
            label.Text = text;
        }

        public static string FormatIncome(double income)
        {
            return FormatIncome(income, true);
        }

        public static string FormatIncome(double income, bool forceDouble)
        {
            return ( income > 0 ? "+" : string.Empty ) + ( forceDouble ? FormatDouble(income) : FormatUsuallyInt(income) );
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

                int telNum;
                Game.GetTeleporter(selected.Value, out telNum);
                if (telNum > -1)
                    lblTop.Text = "Wormhole " + telNum + ( lblTop.Text.Length > 0 ? " - " + lblTop.Text : "" );

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
                btnDisband.Visible = true;

                if (ship.HP < ship.MaxHP)
                {
                    btnGoldRepair.Visible = true;
                    btnGoldRepair.Text = ( ship.HasRepaired ? "Auto Repair" : "Repair Ship" );
                    double autoRepair = ship.AutoRepair;
                    if (autoRepair != 0)
                        btnGoldRepair.Text += string.Format(" ({0})", double.IsNaN(autoRepair) ? "M" : FormatDouble(autoRepair));
                }
            }

            lblTop.Text = ship.ToString();

            lbl1.Text = "Attack";
            lbl1Inf.Text = ship.Att.ToString();

            lbl2.Text = "Defense";
            lbl2Inf.Text = ship.Def.ToString();

            lbl3.Text = "Hits";
            lbl3Inf.Text = ship.HP.ToString() + " / " + ship.MaxHP.ToString() + " - "
                   + FormatPctWithCheck(ship.HP / (double)ship.MaxHP);

            lbl4.Text = "Speed";
            lbl4Inf.Text = ship.CurSpeed.ToString() + " / " + ship.MaxSpeed.ToString();

            if (ship.Player.IsTurn)
            {
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
                lblBottom.Text = "Colony Ship";
                if (ship.Player.IsTurn)
                {
                    double colonizationValue = ship.ColonizationValue;
                    string repair = string.Empty;
                    Colony repairedFrom = ship.GetRepairedFrom();
                    if (repairedFrom != null)
                        repair = " +" + FormatDouble(ship.GetColonizationValue(ship.GetHPForProd(repairedFrom.GetProductionIncome())) - colonizationValue);

                    lblBottom.Text += " (" + FormatDouble(colonizationValue) + repair + ")";

                }
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

                    pnlBuild.Visible = true;
                    pnlBuild.SetColony(colony);
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
                double pdChange = colony.HP - ( colony.HP - colony.DefenseHPChange ) / colony.PlanetDefenseStrengthPerHP
                        * ShipDesign.GetPlanetDefenseStrength(colony.Att - attChange, colony.Def - defChange);
                string strChange = FormatUsuallyInt(pdChange);
                if (strChange != "0")
                    lbl4Inf.Text += string.Format(" ({1}{0})", strChange, pdChange > 0 ? "+" : string.Empty);

                if (colony.Repair > 0)
                {
                    lbl6.Text = "Repair";
                    lbl6Inf.Text = "+" + colony.Repair;
                }
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
                else if (colony.Buildable != null)
                    lbl6Inf.Text = colony.Buildable.ToString();
                else
                    lbl6Inf.Text = "Gold";

                lbl7Inf.Text = GetProdText(colony);
            }

            return colony.Player;
        }

        private static string GetProdText(Colony colony)
        {
            return GetProdText(colony, colony.Buildable, colony.Production);
        }

        public static string GetProdText(Colony colony, Buildable build, double production)
        {
            string retVal = string.Empty;
            if (build != null)
                retVal = build.GetProdText(FormatUsuallyInt(production));

            double prodInc = colony.GetAfterRepairProdInc();

            string inc;
            if (build == null)
            {
                prodInc /= Consts.GoldProductionForGold;

                inc = FormatDouble(prodInc);
            }
            else if (build is PlanetDefense)
            {
                prodInc += production;
                inc = GetBuildingDefense(colony, build, prodInc);
                if (prodInc > 0)
                    prodInc = -1;
            }
            else
            {
                if (build is StoreProd)
                    prodInc -= prodInc * Consts.StoreProdLossPct;

                inc = FormatUsuallyInt(prodInc);
            }

            double incDbl;
            if (prodInc != 0 && ( !double.TryParse(inc.TrimEnd('%'), out incDbl) || incDbl != 0 ))
            {
                if (retVal.Length > 0)
                    retVal += " ";
                if (prodInc > 0)
                    retVal += "+";
                retVal += inc;
            }
            return retVal;
        }

        public static string GetBuildingDefense(Colony colony, Buildable buildable, double production)
        {
            double newAtt, newDef, newHP, soldiers;
            colony.GetPlanetDefenseInc(buildable, production, out newAtt, out newDef, out newHP, out soldiers);
            return GetBuildingDefense(colony, newAtt - colony.Att, newDef - colony.Def, newHP - colony.HP);
        }
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

        private void AnomalyInfo(Anomaly anomaly)
        {
            lblTop.Text = "Anomaly";
            lbl1.Text = "Planet";
            lbl1Inf.Text = FormatPct(Game.GetAnomalyPlanetChance(anomaly.Tile), true);
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
            return ( MessageBox.Show(message, string.Empty, MessageBoxButtons.OKCancel, alert ? MessageBoxIcon.Warning : MessageBoxIcon.None) == DialogResult.OK );
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
            return ( place ? FormatDouble(pct) : FormatInt(pct) ) + "%";
        }

        #endregion //Refresh

        #region IEventHandler

        Tile IEventHandler.getBuildTile(Colony colony)
        {
            showMoves = false;

            RefreshAll();

            SelectTileDialog(colony.Tile.Point, true);
            return GetSelectedTile();
        }

        Buildable IEventHandler.getNewBuild(Colony colony)
        {
            showMoves = false;

            RefreshAll();

            return ChangeBuild(colony);
        }

        int IEventHandler.MoveTroops(Colony fromColony, int max, int totalPop, double soldiers)
        {
            showMoves = false;

            if (fromColony != null)
                SelectTile(fromColony.Tile);
            RefreshAll();

            return SliderForm.ShowForm(new MoveTroops(fromColony, max, totalPop, soldiers));
        }

        bool IEventHandler.Continue()
        {
            showMoves = false;

            return ShowOption("Planet population has been killed off.  Continue attacking?");
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

            switch (anomalyType)
            {

            case Anomaly.AnomalyType.AskProductionOrDefense:
                SelectTile(( (Colony)info[0] ).Tile);
                RefreshAll();
                return ShowOption("Take +" + FormatDouble((double)info[1]) + " producton or build defenses?");

            case Anomaly.AnomalyType.AskResearchOrGold:
                return ShowOption("Take research or +" + info[0] + " gold?");

            case Anomaly.AnomalyType.AskTerraform:
                SelectTile(( (Colony)info[0] ).Tile);
                RefreshAll();
                string inf = "Terraform planet?\r\n+" + info[1] + " Quality\r\n" +
                        FormatIncome((double)info[2]) + " Gold (" + FormatIncome((double)info[3]) + ")\r\nChances: ";
                double[] chances = (double[])info[4];
                Array.Sort(chances);
                for (int a = chances.Length ; --a >= 0 ; )
                {
                    inf += FormatDouble(chances[a]);
                    if (a > 0)
                        inf += ", ";
                }
                if ((bool)info[5])
                {
                    return ShowOption(inf);
                }
                else
                {
                    inf += "\r\n\r\nToo Expensive!";
                    MessageBox.Show(inf);
                    return true;
                }


            case Anomaly.AnomalyType.Death:
            case Anomaly.AnomalyType.Heal:
                MessageBox.Show(FormatIncome((int)info[0]) + " HP!");
                return true;

            case Anomaly.AnomalyType.Experience:
                anomExp = true;
                return true;

            case Anomaly.AnomalyType.Gold:
                MessageBox.Show("+" + FormatDouble((double)info[0]) + " Gold!");
                return true;

            case Anomaly.AnomalyType.LostColony:
                string str = "Hostile";
                if (( (Player)info[0] ).IsTurn)
                    str = "Friendly";
                MessageBox.Show(str + " Colony!");
                return true;

            case Anomaly.AnomalyType.PickupPopulation:
                MessageBox.Show("Picked up " + info[0] + " population!");
                return true;

            case Anomaly.AnomalyType.PickupSoldiers:
                MessageBox.Show("+" + FormatPct((double)info[0] / GetSelectedShip().Population) + " soldiers!");
                return true;

            case Anomaly.AnomalyType.SalvageShip:
                string player = "Hostile";
                if (( (Player)info[0] ).IsTurn)
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
            case Anomaly.AnomalyType.SoldiersAndDefense:
                string msg = "";
                if (info.Length > 0)
                {
                    SelectTile(( (Colony)info[0] ).Tile);
                    RefreshAll();
                    msg = "+" + FormatDouble((double)info[1]) + " ";
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
            MessageBox.Show(msg + Game.CamelToSpaces(anomalyType.ToString()) + "!");
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
            log += string.Format(format, args) + "\r\n";
        }

        public void LogMsg()
        {
            log += "\r\n";

            try
            {
                using (StreamWriter streamWriter = new StreamWriter(GetLogPath(), true))
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
            return log;
        }

        private string GetLogPath()
        {
            return saveFileDialog1.InitialDirectory + "\\gw.log";
        }

        #endregion //Log
    }
}
