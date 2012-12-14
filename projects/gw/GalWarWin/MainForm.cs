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

namespace GalWarWin
{
    public partial class MainForm : Form, IEventHandler
    {
        #region fields and constructors

        private static string initialAutoSave;

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
                _game.AutoSavePath = initialAutoSave;
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

        private bool started = false, saved = true, ended = false, _showMoves = false, _showAtt = false;
        private Point mouse;
        private Font font = new Font("arial", 13f);

        private MainForm dialog;

        private bool emphasisEvent = true;

        private Tile selectedTile = null;
        private HashSet<Ship> hold, holdPersistent;

        private MainForm(bool isDialog)
        {
            InitializeComponent();
            this.ResizeRedraw = true;
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            this.Bounds = Screen.GetWorkingArea(this);

            this.isDialog = isDialog;
            if (isDialog)
            {
                HideButtons(this);
                StartGame();
                this.AcceptButton = btnCancel;
            }
            else
            {
                gameForm = this;
            }

            this.hold = new HashSet<Ship>();
            this.holdPersistent = new HashSet<Ship>();
        }

        private void HideButtons(Control parent)
        {
            if (parent is CheckBox)
                parent.Enabled = false;
            else if (parent is Button && parent != this.btnCancel && parent != this.btnCombat
                    && parent != this.btnInvasion && parent != this.btnShowMoves && parent != this.btnGraphs)
                parent.Hide();
            else
                foreach (Control child in parent.Controls)
                    HideButtons(child);
        }

        public MainForm()
            : this(false)
        {
            this.dialog = new MainForm(true);
            this.dialog.MouseMove += new MouseEventHandler(this.MainForm_MouseMove);

            this.pnlInfo.Hide();

            string initialDirectory = GetInitialDirectory();
            MainForm.initialAutoSave = GetInitialAutoSave(initialDirectory);

            this.openFileDialog1.InitialDirectory = initialDirectory;
            this.openFileDialog1.FileName = "g.gws";
            this.saveFileDialog1.InitialDirectory = this.openFileDialog1.InitialDirectory;
            this.saveFileDialog1.FileName = this.openFileDialog1.FileName;
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

        private void SetBtnShowMovesText()
        {
            this.btnShowMoves.Text = ( ( showMoves && !showAtt ) ? "Enemy Attacks" : "Enemy Moves" );
        }


        private int ClientHeight
        {
            get
            {
                int height = ClientSize.Height;
                if (this.tbTurns.Visible)
                    height -= this.tbTurns.Height;
                return height;
            }
        }

        #endregion //fields and constructors

        #region Drawing

        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                base.OnPaint(e);

                if (started)
                {
                    float height = this.ClientHeight;
                    float width = this.ClientSize.Width - this.pnlInfo.Width;

                    e.Graphics.FillRectangle(Brushes.Black, 0, 0, width, height);

                    float scale = GetScale();

                    float startX = GetStartX();

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

                    float newSize = font.Size * scale / e.Graphics.MeasureString("99%", font).Width;
                    if (newSize > 13f)
                        newSize = 13f;
                    else if (newSize < 1f)
                        newSize = 1f;
                    if (newSize != font.Size)
                    {
                        font.Dispose();
                        font = new Font("arial", newSize);
                    }

                    Dictionary<Tile, float> moves = null;
                    if (showMoves)
                        moves = GetMoves();

                    Tile[,] map = Game.GetMap();
                    for (int x = 0 ; x < Game.Diameter ; ++x)
                        for (int y = 0 ; y < Game.Diameter ; ++y)
                        {
                            Tile tile = map[x, y];
                            if (tile != null)
                            {
                                RectangleF rect = new RectangleF(startX + scale * x + ( y % 2 == 0 ? 0 : scale / 2f ), 2 + scale * y, scale, scale);

                                Ship ship;
                                Planet planet;
                                if (( planet = ( tile.SpaceObject as Planet ) ) != null)
                                    DrawPlanet(e, scale, rect, planet, minQuality, maxQuality, minPop, maxPop);
                                else if (( ship = ( tile.SpaceObject as Ship ) ) != null)
                                    DrawShip(e, scale, rect, ship, minStr, maxStr);

                                DrawBorder(e, tile, rect);

                                if (moves != null)
                                {
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
                                        SizeF strSize = e.Graphics.MeasureString(s, font);
                                        e.Graphics.DrawString(s, font, Brushes.White, rect.Right - strSize.Width, rect.Bottom - strSize.Height);
                                    }
                                }
                            }
                        }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                using (Font font = new Font("arial", 13f))
                    e.Graphics.DrawString(ex.ToString(), font, Brushes.White, 0, 0);
            }
        }

        private void GetVals(ref float min, ref float max, float value)
        {
            min = (float)Math.Min(min, Math.Sqrt(value));
            max = (float)Math.Max(max, Math.Sqrt(value));
        }

        private void DrawBorder(PaintEventArgs e, Tile tile, RectangleF rect)
        {
            Planet planet = null;
            Ship ship = null;
            if (tile != null)
            {
                planet = tile.SpaceObject as Planet;
                ship = tile.SpaceObject as Ship;
            }

            float size;
            if (tile == this.selectedTile)
                size = 3f;
            else if (planet != null || ( this.isDialog ? ValidDialogTile(tile) : ship != null &&
                    ship.Player.IsTurn && ( ( ship.AutoRepair == 0 && ship.HP < ship.MaxHP ) || ship.CurSpeed > 0 ) ))
                size = 2f;
            else
                size = 1f;
            if (tile == this.dialogTile)
                ++size;

            e.Graphics.DrawRectangle(new Pen(Color.White, size), rect.X, rect.Y, rect.Width, rect.Height);
        }

        private void DrawPlanet(PaintEventArgs e, float scale, RectangleF rect, Planet planet,
                float minQuality, float maxQuality, float minPop, float maxPop)
        {
            Colony colony = planet.Colony;
            Brush brush;
            RectangleF planetRect;
            if (colony == null)
            {
                planetRect = Inflate(scale, rect, planet.Quality, minQuality, maxQuality, .735f, .169f);
                brush = Brushes.White;
            }
            else
            {
                planetRect = Inflate(scale, rect, colony.Population, minPop, maxPop, .735f, .169f);
                brush = new SolidBrush(colony.Player.Color);
            }
            e.Graphics.FillEllipse(brush, planetRect);

            if (colony != null)
            {
                if (colony.HP > 0)
                    e.Graphics.DrawEllipse(Pens.White, planetRect);
                double pct = colony.Population / (double)planet.Quality;
                if (pct < 1)
                {
                    string str = FormatPctWithCheck(pct);
                    SizeF strSize = e.Graphics.MeasureString(str, font);
                    e.Graphics.DrawString(str, font, Brushes.White, rect.Right - strSize.Width, rect.Bottom - strSize.Height);
                }
            }
        }

        private void DrawShip(PaintEventArgs e, float scale, RectangleF rect, Ship ship, float minStr, float maxStr)
        {
            rect = Inflate(scale, rect, ship.GetStrength() * ship.HP / (float)ship.MaxHP, minStr, maxStr, .666f, .13f);
            e.Graphics.FillRectangle(new SolidBrush(ship.Player.Color), rect);

            if (ship.DeathStar || ship.Population > 0)
                e.Graphics.DrawRectangle(Pens.White, rect.X, rect.Y, rect.Width, rect.Height);

            double pct = ship.HP / (double)ship.MaxHP;
            if (pct < 1)
            {
                PointF endPoint;
                if (pct < .75f)
                    endPoint = new PointF(rect.Right, rect.Bottom);
                else
                    endPoint = GetMid(rect);
                e.Graphics.DrawLine(Pens.Black, rect.Location, endPoint);

                if (pct < .5f)
                {
                    if (pct < .25f)
                        endPoint = new PointF(rect.Right, rect.Top);
                    else
                        endPoint = GetMid(rect);
                    e.Graphics.DrawLine(Pens.Black, new PointF(rect.Left, rect.Bottom), endPoint);
                }
            }
        }

        private RectangleF Inflate(float scale, RectangleF rect, double value, float min, float max, float smallInvsPct, float inc)
        {
            float inflate = (float)( -scale * ( 1f - ( smallInvsPct + inc * ( ( Math.Sqrt(value) - min + 1 ) / ( max - min + 1 ) ) ) ) );
            rect.Inflate(inflate, inflate);
            return rect;
        }

        private PointF GetMid(RectangleF rect)
        {
            return new PointF(rect.X + rect.Width / 2f, rect.Y + rect.Height / 2f);
        }

        private float GetScale()
        {
            float height = this.ClientHeight - 5;
            float width = this.ClientSize.Width - this.pnlInfo.Width - 5;
            return (float)Math.Min(height / (double)Game.Diameter, width / (double)( Game.Diameter + .5 ));
        }

        private float GetStartX()
        {
            float width = this.ClientSize.Width - this.pnlInfo.Width;
            float scale = GetScale();
            return width - scale * ( Game.Diameter + .5f ) - 3;
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
            float stat = MattUtil.TBSUtil.FindValue(delegate(int test)
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
                if (showPlayer != null && ( tile.SpaceObject == null || ( ship != null && ship.Player == showPlayer ) ))
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
                if (newMove > move && ( neighbor.SpaceObject == null || ( ( ship = neighbor.SpaceObject as Ship ) != null && ship.Player == enemy ) )
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
            showAtt = ( showMoves ? !showAtt : false );
            showMoves = true;
            InvalidateMap();
        }

        private void btnGraphs_Click(object sender, EventArgs e)
        {
            GraphsForm.ShowForm(Game);
        }

        private void btnNewGame_Click(object sender, EventArgs e)
        {
            Player black = new Player("Black", Color.Blue, null);//new GalWarAI.GalWarAI());
            Player blue = new Player("Blue", Color.Cyan, null);//new GalWarAI.GalWarAI());
            Player green = new Player("Green", Color.Green, null);//new GalWarAI.GalWarAI());
            Player pink = new Player("Pink", Color.Magenta, null);//new GalWarAI.GalWarAI());
            Player red = new Player("Red", Color.Red, null);//new GalWarAI.GalWarAI());
            Player yellow = new Player("Yellow", Color.Gold, null);//new GalWarAI.GalWarAI());
            Game = new Game(new Player[] { black, blue, green, pink, red, yellow },
                    Game.Random.GaussianCappedInt(16.5f, .21f, 13) + Game.Random.OEInt(1.3),
                    Game.Random.GaussianCapped(0.006, .52, 0.0021));

            mouse = new Point(ClientSize.Width / 2, ClientHeight / 2);
            StartGame();

            Game.StartGame(this);

            saved = false;
            this.RefreshAll();
        }

        private void btnLoadGame_Click(object sender, EventArgs e)
        {
            if (this.openFileDialog1.ShowDialog() == DialogResult.OK)
                LoadGame(this.openFileDialog1.FileName);
        }

        private void LoadGame(string filePath)
        {
            this.saveFileDialog1.InitialDirectory = Path.GetDirectoryName(filePath);
            this.saveFileDialog1.FileName = Path.GetFileName(filePath);

            Game = Game.LoadGame(filePath);

            StartGame();

            saved = true;
            RefreshAll();
        }

        private void btnAutosaveView_Click(object sender, EventArgs e)
        {
            this.openFileDialog1.InitialDirectory = initialAutoSave;
            this.openFileDialog1.FileName = "1.gws";

            if (this.openFileDialog1.ShowDialog() == DialogResult.OK)
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

                this.tbTurns.Visible = true;
                this.tbTurns.Maximum = max;
                this.tbTurns.Value = max;
                this.tbTurns.Minimum = min;
                this.tbTurns.Value = min;

                this.tbTurns.TickFrequency = ( max - min ) / 39;

                tbTurns_Scroll(null, null);
            }
        }

        private void tbTurns_Scroll(object sender, EventArgs e)
        {
            int turn;
            while (( Game == null ? -1 : Game.Turn ) != ( turn = (int)this.tbTurns.Value ))
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
                        ++this.tbTurns.Value;
                    else
                        --this.tbTurns.Value;
                }
                catch
                {
                    Application.Exit();
                    return;
                }
            }
        }

        private string GetAutosaveFolder()
        {
            return Path.GetDirectoryName(this.openFileDialog1.FileName);
        }

        private void StartGame()
        {
            this.started = true;
            this.pnlInfo.Show();
            this.btnNewGame.Hide();
            this.btnLoadGame.Hide();
            this.btnAutosaveView.Hide();
        }

        private void btnSaveGame_Click(object sender, EventArgs e)
        {
            if (this.saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string filePath = this.saveFileDialog1.FileName;
                this.saveFileDialog1.InitialDirectory = Path.GetDirectoryName(filePath);
                this.saveFileDialog1.FileName = Path.GetFileName(filePath);

                Game.SaveGame(filePath);
                saved = true;
            }
        }

        private void MainForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (sender == this.pnlInfo)
                this.mouse = new Point(this.pnlInfo.Location.X + e.Location.X, this.pnlInfo.Location.Y + e.Location.Y);
            else
                this.mouse = e.Location;
        }

        private void btnProduction_Click(object sender, EventArgs e)
        {
            Colony colony = ( (Planet)this.selectedTile.SpaceObject ).Colony;
            colony.StartBuilding(this, ChangeBuild(colony, true, true));

            saved = false;
            this.RefreshAll();
        }

        private Buildable ChangeBuild(Colony colony, bool accountForIncome, bool switchLoss, params double[] additionalLosses)
        {
            this.selectedTile = colony.Tile;
            this.RefreshAll();

            return ProductionForm.ShowForm(colony, accountForIncome, switchLoss, additionalLosses);
        }

        private void btnProdRepair_Click(object sender, EventArgs eventArgs)
        {
            Colony colony = ( (Planet)this.selectedTile.SpaceObject ).Colony;
            if (colony.RepairShip == null)
            {
                Tile tile = SelectTile(this.selectedTile, false);
                if (tile != null)
                    colony.SetRepairShip(this, tile.SpaceObject as Ship);
            }
            else
            {
                colony.SetRepairShip(this, null);
            }

            saved = false;
            this.RefreshAll();
        }

        private Tile SelectTile(Tile tile, bool build)
        {
            if (build)
                dialog.pnlBuild.SetBuildable((Buildable)( (Planet)tile.SpaceObject ).Colony.Buildable);
            dialog.pnlBuild.Visible = build;
            dialog.btnCancel.Visible = true;

            dialog.isBuild = build;
            dialog.dialogTile = tile;
            dialog.selectedTile = tile;

            dialog.RefreshAll();

            dialog.Location = this.Location;
            dialog.Size = this.Size;
            if (dialog.ShowDialog() == DialogResult.Cancel)
                dialog.selectedTile = null;

            this.Location = dialog.Location;
            this.Size = dialog.Size;

            this.pnlBuild.Visible = false;
            this.btnCancel.Visible = false;

            if (dialog.selectedTile == null)
                this.selectedTile = tile;
            else
                this.selectedTile = dialog.selectedTile;

            return dialog.selectedTile;
        }

        private void btnGoldRepair_Click(object sender, EventArgs e)
        {
            Ship ship = (Ship)this.selectedTile.SpaceObject;

            if (ship.HP < ship.MaxHP && !ship.HasRepaired)
            {
                int HP = SliderForm.ShowForm(new GoldRepair(ship));

                if (HP > 0)
                    ship.GoldRepair(this, HP);

                saved = false;
                this.RefreshAll();
            }
            else if (AutoRepairForm.ShowForm(ship))
            {
                saved = false;
                this.RefreshAll();
            }
        }

        private void btnAutoRepairShips_Click(object sender, EventArgs e)
        {
            if (RepairAllForm.ShowForm())
            {
                CheckRepairedShips();

                saved = false;
                this.RefreshAll();
            }
        }

        private void btnDisband_Click(object sender, EventArgs e)
        {
            Ship ship = (Ship)this.selectedTile.SpaceObject;

            Colony colony = null;
            foreach (Tile neighbor in Tile.GetNeighbors(this.selectedTile))
            {
                Planet planet = neighbor.SpaceObject as Planet;
                if (planet != null)
                {
                    colony = planet.Colony;
                    if (colony != null)
                        break;
                }
            }

            double production = ship.DisbandValue;
            double gold = Player.RoundGold(production) + Player.RoundGold(ship.GetDestroyGold());

            Buildable buildable = null;
            if (colony != null && colony.Player.IsTurn)
                buildable = colony.Buildable;
            if (buildable is StoreProd)
                production -= production * Consts.StoreProdLossPct;

            if (buildable != null && ShowOption("Disband for " + FormatDouble(production) + " production?"))
                ship.Disband(this, colony);
            else if (ShowOption("Disband for " + FormatDouble(gold) + " gold?"))
                ship.Disband(this, null);

            saved = false;
            this.RefreshAll();
        }

        private void btnUndo_Click(object sender, EventArgs e)
        {
            this.selectedTile = Game.Undo(this);

            saved = false;
            this.RefreshAll();
        }

        private void btnEndTurn_Click(object sender, EventArgs e)
        {
            if (CheckGold() && CheckMovedShips() && CheckRepairedShips())
            {
                CombatForm.FlushLog();

                HashSet<Ship> check = new HashSet<Ship>();
                foreach (Ship ship in Game.CurrentPlayer.GetShips())
                    if (ship.GetRepairedFrom() != null)
                        check.Add(ship);

                Game.EndTurn(this);

                foreach (Ship ship in check)
                    if (ship.HP == ship.MaxHP)
                        this.holdPersistent.Remove(ship);
                this.hold.RemoveWhere(delegate(Ship ship)
                {
                    return ( !holdPersistent.Contains(ship) );
                });
                SelectNextShip();

                showMoves = false;
                saved = false;
                this.RefreshAll();

                CombatForm.FlushLog();
            }
        }

        private bool CheckGold()
        {
            bool end = true;
            if (Game.CurrentPlayer.MinGoldNegative())
                end = ShowOption("You are running out of gold.  Partial production may be sold and one or more ships disbanded.  Are you sure you want to end your turn?", true);
            return end;
        }

        private bool CheckMovedShips()
        {
            bool end = true;

            foreach (Ship ship in Game.CurrentPlayer.GetShips())
                if (ship.CurSpeed > 0 && !hold.Contains(ship))
                {
                    end = ShowOption("You have not moved all of your ships.  Are you sure you want to end your turn?");
                    break;
                }

            return end;
        }

        private bool CheckRepairedShips()
        {
            bool end = true;

            foreach (Ship ship in Game.CurrentPlayer.GetShips())
                if (!ship.HasRepaired && ship.HP < ship.MaxHP && double.IsNaN(ship.AutoRepair))
                {
                    selectedTile = ship.Tile;
                    RefreshAll();

                    int hp = SliderForm.ShowForm(new GoldRepair(ship));
                    if (hp > 0)
                        ship.GoldRepair(this, hp);

                    end = ( hp > -1 );
                    if (!end)
                        break;
                }

            return end;
        }

        private void SelectNextShip()
        {
            ReadOnlyCollection<Ship> ships = Game.CurrentPlayer.GetShips();
            if (ships.Count > 0)
            {
                int start = ships.IndexOf(this.selectedTile == null ? null : this.selectedTile.SpaceObject as Ship);
                int index = start + 1;
                if (start < 0)
                {
                    index = 0;
                    start = ships.Count;
                }
                while (true)
                {
                    if (index == ships.Count)
                    {
                        index = -1;
                    }
                    else if (ships[index].CurSpeed > 0 && !hold.Contains(ships[index]))
                    {
                        this.selectedTile = ships[index].Tile;
                        break;
                    }
                    if (++index == start)
                    {
                        start = -1;
                    }
                    else if (start == -1)
                    {
                        this.selectedTile = null;
                        break;
                    }
                }
            }
            else
            {
                this.selectedTile = null;
            }
        }

        private void chkEmphasis_CheckedChanged(object sender, EventArgs e)
        {
            if (emphasisEvent)
            {
                Game.CurrentPlayer.SetGoldEmphasis(this, chkGold.Checked);
                Game.CurrentPlayer.SetResearchEmphasis(this, chkResearch.Checked);
                Game.CurrentPlayer.SetProductionEmphasis(this, chkProduction.Checked);

                RefreshAll();
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!isDialog)
                if (!saved && !ShowOption("Are you sure you want to quit without saving?", true))
                    e.Cancel = true;
                else
                    CombatForm.FlushLog();
        }

        private Point? clicked = null;
        private void MainForm_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ClickMouse(e, true);
        }
        private void MainForm_MouseClick(object sender, MouseEventArgs e)
        {
            this.clicked = null;
            ClickMouse(e, false);
        }
        private void ClickMouse(MouseEventArgs e, bool doubleClick)
        {
            if (started)
            {
                float scale = GetScale();

                int y = (int)( e.Y / scale );
                int x = (int)( ( ( e.X - GetStartX() ) - ( y % 2 == 0 ? 0 : scale / 2f ) ) / scale );

                if (x >= 0 && y >= 0 && x < Game.Diameter && y < Game.Diameter)
                {

                    Tile clickedTile = Game.GetMap()[x, y];
                    if (clickedTile != null)
                        if (e.Button == MouseButtons.Left)
                        {
                            this.selectedTile = clickedTile;
                            Ship ship = this.selectedTile.SpaceObject as Ship;
                            if (ship != null && ship.Player.IsTurn)
                            {
                                hold.Remove(ship);
                                holdPersistent.Remove(ship);
                            }

                            if (this.isDialog && ValidDialogTile(this.selectedTile))
                            {
                                this.DialogResult = DialogResult.OK;
                                this.Close();
                                return;
                            }
                        }
                        else if (!this.isDialog && e.Button == MouseButtons.Right)
                        {
                            Ship target = clickedTile.SpaceObject as Ship;

                            Point clicked = new Point(x, y);
                            if (doubleClick && target != null && this.clicked == clicked && hold.Contains(target))
                            {
                                holdPersistent.Add(target);
                                this.clicked = null;
                                return;
                            }
                            this.clicked = clicked;

                            Colony colony = GetSelectedColony();
                            if (colony != null && target != null && colony.Player != target.Player && colony.HP > 0)
                            {
                                CombatForm.ShowForm(colony, target);
                            }
                            else
                            {
                                Ship ship = null;
                                int oldSpeed = -1;
                                if (this.selectedTile != null)
                                {
                                    ship = this.selectedTile.SpaceObject as Ship;
                                    if (ship != null)
                                        oldSpeed = ship.CurSpeed;
                                }

                                bool selectNext = true;

                                if (this.selectedTile != null && ( ship == null || ship.Player.IsTurn ))
                                    if (Tile.IsNeighbor(clickedTile, this.selectedTile))
                                        selectNext &= RightClick(clickedTile);
                                    else if (clickedTile == this.selectedTile)
                                        hold.Add(ship);

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

                                if (selectNext && ( this.selectedTile == null || ( ship == null || !ship.Player.IsTurn || ship.CurSpeed == 0 || ship.CurSpeed == oldSpeed ) ))
                                    SelectNextShip();

                                saved = false;
                            }
                        }
                }

                showMoves = false;
                RefreshAll();
            }
        }

        private bool ValidDialogTile(Tile tile)
        {
            if (!Tile.IsNeighbor(dialogTile, tile))
                return false;
            if (this.isBuild)
            {
                return tile.SpaceObject == null;
            }
            else
            {
                Ship ship = tile.SpaceObject as Ship;
                return ( ship != null && ship.HP < ship.MaxHP && ship.Player.IsTurn );
            }
        }

        private bool RightClick(Tile adjacentTile)
        {
            bool selectNext = true;

            ISpaceObject spaceObject = this.selectedTile.SpaceObject;

            bool switchTroops = false;
            Planet planet;
            if (( planet = ( spaceObject as Planet ) ) != null)
            {
                if (planet.Colony != null && adjacentTile.SpaceObject is Ship)
                {
                    spaceObject = adjacentTile.SpaceObject;
                    adjacentTile = this.selectedTile;
                    switchTroops = true;
                }
            }

            Ship ship;
            if (( ship = ( spaceObject as Ship ) ) != null)
            {
                if (ship.Player.IsTurn)
                {
                    try
                    {
                        Planet trgPlanet;
                        Ship trgShip;
                        if (adjacentTile.SpaceObject == null)
                            TargetTile(adjacentTile, ship);
                        else if (( trgShip = ( adjacentTile.SpaceObject as Ship ) ) != null)
                            selectNext &= TargetShip(trgShip, ship, switchTroops);
                        else if (( trgPlanet = ( adjacentTile.SpaceObject as Planet ) ) != null)
                            selectNext &= TargetPlanet(trgPlanet, ship, switchTroops);
                    }
                    catch (AssertException e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }

            return selectNext;
        }

        private void TargetTile(Tile targetTile, Ship ship)
        {
            ship.Move(this, targetTile);
            this.selectedTile = targetTile;
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
                    this.selectedTile = planet.Tile;
                }
            }

            if (bombard && ship.CurSpeed > 0 && ( !ship.Colony || ShowOption("Bombard planet?") ))
            {
                if (ship.CurSpeed == 1)
                    this.selectedTile = planet.Tile;
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
                    this.selectedTile = targetColony.Tile;
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
                int troops = SliderForm.ShowForm(new MoveTroops(Game, from, to));
                if (troops > 0)
                {
                    this.selectedTile = to.Tile;
                    from.MovePop(this, troops, to);
                }
                return false;
            }
            return true;
        }

        private void InvadePlanet(Ship ship, Colony colony)
        {
            Planet planet = colony.Planet;

            this.selectedTile = planet.Tile;
            this.RefreshAll();

            int gold = 0, troops = ship.AvailablePop;
            if (troops > 0)
                if (colony.Population > 0)
                    gold = SliderForm.ShowForm(new Invade(ship, colony));
                else
                    troops = SliderForm.ShowForm(new MoveTroops(Game, ship, colony));

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
                this.selectedTile = ship.Tile;
        }

        public void SetLocation(Form form)
        {
            Point mouse;
            try
            {
                mouse = this.PointToScreen(this.mouse);
            }
            catch (ObjectDisposedException e)
            {
                Console.WriteLine(e);

                return;
            }

            int x = mouse.X - form.Width / 2;
            int y = mouse.Y - form.Height / 2;

            Rectangle bounds = Screen.FromControl(this).WorkingArea;

            if (x < bounds.X)
                x = bounds.X;
            else if (x + form.Width > bounds.X + bounds.Width)
                x = bounds.X + bounds.Width - form.Width;

            if (y < bounds.Y)
                y = bounds.Y;
            else if (y + form.Height > bounds.Y + bounds.Height)
                y = bounds.Y + bounds.Height - form.Height;

            form.Location = new Point(x, y);
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
                upkeep += ship.Upkeep - ship.GetUpkeepReturn();
                ships += ship.Upkeep;
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

        private Colony GetSelectedColony()
        {
            if (this.selectedTile != null)
            {
                Planet planet = this.selectedTile.SpaceObject as Planet;
                if (planet != null)
                {
                    return planet.Colony;
                }
            }
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
            if (colony == null)
                if (this.selectedTile == null)
                    CostCalculatorForm.ShowForm();
                else
                    CostCalculatorForm.ShowForm(this.selectedTile.SpaceObject as Ship);
            else if (colony.Player.IsTurn)
                CostCalculatorForm.ShowForm(colony.Buildable as ShipDesign);
            else
                CostCalculatorForm.ShowForm();
        }

        #endregion //Events

        #region Refresh

        public void RefreshAll()
        {
            if (!ended && Game.GetPlayers().Length < 2)
            {
                ended = true;
                TextForm.ShowForm(Game.GetGameResult());
                Game.AutoSave();
            }

            RefreshCurrentPlayer();
            RefreshSelectedInfo();
            InvalidateMap();

            this.btnUndo.Enabled = Game.CanUndo();

            CombatForm.ShowLog();
        }

        private void InvalidateMap()
        {
            this.Invalidate(GetInvalidateRectangle(this.ClientRectangle, this.pnlInfo.Location.X));
        }

        public static Rectangle GetInvalidateRectangle(Rectangle client, int width)
        {
            return new Rectangle(client.X, client.Y, width, client.Height);
        }

        private void RefreshCurrentPlayer()
        {
            this.lblPlayer.BackColor = Game.CurrentPlayer.Color;
            this.lblPlayer.Text = Game.Turn.ToString() + " - " + Game.CurrentPlayer.Name;
            RefreshPlayerInfo();
        }

        private void RefreshPlayerInfo()
        {
            this.lblPopulation.Text = Game.CurrentPlayer.GetPopulation().ToString();
            this.lblGold.Text = FormatDouble(Game.CurrentPlayer.Gold);

            int research;
            double population, production, gold;
            Game.CurrentPlayer.GetTurnIncome(out population, out research, out production, out gold);

            FormatIncome(lblPopInc, population);
            FormatIncome(lblGoldInc, gold, true);
            FormatIncome(lblResearch, research);
            this.lblRsrchPct.Text = FormatPct(Game.CurrentPlayer.GetResearchChance(research));
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

            if (this.selectedTile != null)
            {
                Player player = null;

                Ship ship;
                Planet planet;
                if (( ship = ( this.selectedTile.SpaceObject as Ship ) ) != null)
                    player = ShipInfo(ship);
                else if (( planet = ( this.selectedTile.SpaceObject as Planet ) ) != null)
                    player = PlanetInfo(planet);

                if (player != null)
                    PlayerInfo(player);
            }
        }

        private void ClearSelectedInfo()
        {
            this.lblTop.BackColor = SystemColors.Control;
            this.lbl2Inf.ForeColor = Color.Black;
            this.lbl5Inf.ForeColor = Color.Black;

            this.lbl4.BorderStyle = BorderStyle.None;
            this.lbl5.BorderStyle = BorderStyle.None;

            this.lblTop.Text = string.Empty;
            this.lbl1.Text = string.Empty;
            this.lbl2.Text = string.Empty;
            this.lbl3.Text = string.Empty;
            this.lbl4.Text = string.Empty;
            this.lbl5.Text = string.Empty;
            this.lbl6.Text = string.Empty;
            this.lbl7.Text = string.Empty;
            this.lbl1Inf.Text = string.Empty;
            this.lbl2Inf.Text = string.Empty;
            this.lbl3Inf.Text = string.Empty;
            this.lbl4Inf.Text = string.Empty;
            this.lbl5Inf.Text = string.Empty;
            this.lbl6Inf.Text = string.Empty;
            this.lbl7Inf.Text = string.Empty;
            this.lblBottom.Text = string.Empty;

            this.btnProdRepair.Visible = false;
            this.btnProduction.Visible = false;
            this.btnGoldRepair.Visible = false;
            this.btnDisband.Visible = false;
        }

        private Player ShipInfo(Ship ship)
        {
            if (!this.isDialog && ship.Player.IsTurn)
            {
                this.btnDisband.Visible = true;

                this.btnGoldRepair.Visible = true;
                this.btnGoldRepair.Text = ( ship.HP < ship.MaxHP && !ship.HasRepaired ? "Repair Ship" : "Auto Repair" );
                if (ship.AutoRepair != 0)
                    this.btnGoldRepair.Text += string.Format(" ({0})", double.IsNaN(ship.AutoRepair) ? "M" : FormatDouble(ship.AutoRepair));
            }

            this.lblTop.Text = ship.ToString();

            this.lbl1.Text = "Attack";
            this.lbl1Inf.Text = ship.Att.ToString();

            this.lbl2.Text = "Defense";
            this.lbl2Inf.Text = ship.Def.ToString();

            this.lbl3.Text = "Hits";
            this.lbl3Inf.Text = ship.HP.ToString() + " / " + ship.MaxHP.ToString() + " - "
                    + FormatPct(ship.HP / (double)ship.MaxHP);

            this.lbl4.Text = "Speed";
            this.lbl4Inf.Text = ship.CurSpeed.ToString() + " / " + ship.MaxSpeed.ToString();

            if (ship.Player.IsTurn)
            {
                this.lbl5.Text = "Upkeep";
                this.lbl5Inf.Text = ship.Upkeep.ToString();
            }
            else if (ship.Repair > 0)
            {
                this.lbl5.Text = "Repair";
                this.lbl5Inf.Text = "+" + ship.Repair;
            }

            this.lbl6.Text = "Experience";
            this.lbl6Inf.Text = ship.GetTotalExp().ToString() + " (" + ship.NextExpType.ToString() + ")";

            if (ship.MaxPop > 0)
            {
                this.lbl7.Text = "Troops";
                this.lbl7Inf.Text = ship.Population.ToString() + " / " + ship.MaxPop.ToString();
                if (ship.Population > 0)
                    this.lbl7Inf.Text += " (" + FormatPct(ship.GetTotalSoldierPct()) + ")";
            }

            if (ship.Colony)
            {
                this.lblBottom.Text = "Colony Ship";
                if (ship.Player.IsTurn)
                    this.lblBottom.Text += " (" + FormatDouble(ship.ColonizationValue) + ")";
            }
            else if (ship.DeathStar)
            {
                this.lblBottom.Text = "Death Star (" + FormatDouble(ship.BombardDamage) + ")";
            }

            return ship.Player;
        }

        private Player PlanetInfo(Planet planet)
        {
            this.lbl1.Text = "Quality";
            this.lbl1Inf.Text = planet.Quality.ToString();

            Colony colony = planet.Colony;
            if (colony == null)
            {
                this.lblTop.Text = "Uncolonized";
                this.lbl2.Text = "Cost";
                this.lbl2Inf.Text = FormatDouble(planet.ColonizationCost);

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
                if (!this.isDialog)
                {
                    this.btnProduction.Visible = true;
                    this.btnProdRepair.Visible = true;
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
                    this.btnProdRepair.Enabled = enabled;
                    this.btnProdRepair.Text = "Repair Ship";
                }
                else
                {
                    this.btnProdRepair.Enabled = true;
                    this.btnProdRepair.Text = "Stop Repair";
                }
            }

            this.lbl2.Text = "Population";
            FormatIncome(this.lbl2Inf, colony.GetPopulationGrowth(), true);
            this.lbl2Inf.Text = colony.Population.ToString() + " " + lbl2Inf.Text;

            this.lbl3.Text = "Soldiers";
            this.lbl3Inf.Text = FormatPct(colony.Player.IsTurn ? colony.GetSoldierPct() : colony.GetTotalSoldierPct());

            this.lbl4.Text = "Defense";
            if (!colony.MinDefenses)
                this.lbl4Inf.Text = string.Format("{0} : {1}   ({2})", colony.Att, colony.Def, colony.HP);

            if (!colony.Player.IsTurn)
            {
                string soldierChange = FormatPct(colony.SoldierChange, true);
                if (soldierChange != "0.0%")
                    this.lbl3Inf.Text += string.Format(" ({1}{0})", soldierChange, colony.SoldierChange > 0 ? "+" : string.Empty);

                int attChange = colony.DefenseAttChange, defChange = colony.DefenseDefChange;
                if (attChange == colony.Att)
                    --attChange;
                if (defChange == colony.Def)
                    --defChange;
                double pdChange = colony.HP - ( colony.HP - colony.DefenseHPChange ) / colony.PlanetDefenseStrength
                        * ShipDesign.GetPlanetDefenseStrength(colony.Att - attChange, colony.Def - defChange);
                string strChange = FormatUsuallyInt(pdChange);
                if (strChange != "0")
                    this.lbl4Inf.Text += string.Format(" ({1}{0})", strChange, pdChange > 0 ? "+" : string.Empty);

                if (colony.Repair > 0)
                {
                    this.lbl6.Text = "Repair";
                    this.lbl6Inf.Text = "+" + colony.Repair;
                }
            }

            this.lbl4.BorderStyle = BorderStyle.FixedSingle;

            this.lbl5.Text = "Income";
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
            FormatIncome(this.lbl5Inf, income, true);

            if (colony.Player.IsTurn)
            {
                this.lbl5Inf.Text += " (" + colony.GetProductionIncome() + ")";

                this.lbl5.BorderStyle = BorderStyle.FixedSingle;

                this.lbl6.Text = "Building";
                Ship repairShip = colony.RepairShip;
                if (repairShip != null)
                {
                    double hp = repairShip.GetHPForProd(colony.GetProductionIncome());
                    if (hp > repairShip.MaxHP - repairShip.HP)
                        hp = repairShip.MaxHP - repairShip.HP;
                    this.lbl6Inf.Text = "Repair +" + FormatDouble(hp);
                }
                else if (colony.Buildable != null)
                {
                    this.lbl6Inf.Text = colony.Buildable.ToString();
                }
                else
                {
                    this.lbl6Inf.Text = "Gold";
                }

                this.lbl7Inf.Text = GetProdText(colony);
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
            else if (build is Soldiering)
            {
                prodInc += production;
                prodInc /= Consts.ProductionForSoldiers;
                prodInc += colony.Soldiers;
                prodInc /= colony.Population + colony.GetPopulationGrowth();
                prodInc -= colony.GetSoldierPct();

                inc = FormatPct(prodInc, true);
            }
            else if (build is PlanetDefense)
            {
                prodInc += production;
                inc = GetBuildingDefense(colony, prodInc);
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

        public static string GetBuildingDefense(Colony colony, double production)
        {
            double newAtt, newDef, newHP;
            colony.GetPlanetDefenseInc(production, out newAtt, out newDef, out newHP);
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

        private void PlayerInfo(Player player)
        {
            if (this.lblTop.Text == string.Empty)
                this.lblTop.Text = player.Name;
            this.lblTop.BackColor = player.Color;
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
            //never display 100% if pct is less than 1
            if (pct < 1 && retVal == "100%")
                retVal = "99%";
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
            this.RefreshAll();

            return SelectTile(colony.Tile, true);
        }

        Buildable IEventHandler.getNewBuild(Colony colony, bool accountForIncome, bool switchLoss, params double[] additionalLosses)
        {
            this.RefreshAll();

            return ChangeBuild(colony, accountForIncome, switchLoss, additionalLosses);
        }

        int IEventHandler.MoveTroops(Colony fromColony, int max, int free, int totalPop, double soldiers)
        {
            if (fromColony != null)
                this.selectedTile = fromColony.Tile;
            this.RefreshAll();

            return SliderForm.ShowForm(new MoveTroops(Game, fromColony, max, free, totalPop, soldiers));
        }

        bool IEventHandler.ConfirmCombat(Combatant attacker, Combatant defender, int freeDmg)
        {
            if (attacker is Ship)
                this.selectedTile = attacker.Tile;
            else
                this.selectedTile = defender.Tile;
            this.RefreshAll();

            return CombatForm.ShowForm(attacker, defender, true, freeDmg);
        }

        void IEventHandler.OnResearch(ShipDesign newDesign, HashSet<ShipDesign> obsolete, PlanetDefense oldDefense, PlanetDefense newDefense)
        {
            this.RefreshAll();

            ResearchForm.ShowForm(newDesign, obsolete, oldDefense, newDefense);
        }

        void IEventHandler.OnCombat(Combatant attacker, Combatant defender, int attack, int defense, int startHP, int popLoss)
        {
            CombatForm.OnCombat(attacker, defender, attack, defense, startHP, popLoss);
        }

        void IEventHandler.OnLevel(Ship ship, Ship.ExpType expType, double pct, int needExp, int lastExp)
        {
            CombatForm.OnLevel(ship, expType, pct, needExp, lastExp);
        }

        void IEventHandler.OnBombard(Ship ship, Planet planet, Colony colony, int freeDmg, int colonyDamage, int planetDamage, int startExp)
        {
            CombatForm.OnBombard(ship, planet, colony, freeDmg, colonyDamage, planetDamage, startExp);
        }

        void IEventHandler.Event()
        {
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
            string msg = string.Format(format, args);
            Console.WriteLine(msg);
            log += msg + "\r\n";
        }

        public void LogMsg()
        {
            Console.WriteLine();
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
            return this.saveFileDialog1.InitialDirectory + "\\gw.log";
        }

        #endregion //Log
    }
}
