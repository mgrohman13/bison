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
        private static Game game;
        public static Game Game
        {
            get
            {
                return game;
            }
        }

        #region fields and constructors

        private bool isDialog;
        private bool isBuild;
        private Tile dialogTile = null;

        private bool started = false, saved = true, ended = false, showMoves = false;
        private Point mouse;
        private Font font = new Font("arial", 13f);

        private MainForm dialog;

        private bool emphasisEvent = true;

        private Tile selectedTile = null;
        private HashSet<Ship> hold;

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

            this.hold = new HashSet<Ship>();
        }

        private void HideButtons(Control parent)
        {
            if (parent is CheckBox)
                parent.Enabled = false;
            else if (parent is Button && parent != this.btnCancel && parent != this.btnCombat && parent != this.btnInvasion && parent != this.btnShowMoves)
                parent.Hide();
            else
                foreach (Control child in parent.Controls)
                    HideButtons(child);
        }

        public MainForm()
            : this(false)
        {
            this.dialog = new MainForm(true);
            this.dialog.MouseMove += new MouseEventHandler(this.GameForm_MouseMove);

            this.pnlInfo.Hide();

            this.openFileDialog1.InitialDirectory = Directory.GetParent(Directory.GetParent(Directory.GetParent(Environment.CurrentDirectory).FullName).FullName).FullName;
            this.openFileDialog1.FileName = "g.gws";
            this.saveFileDialog1.InitialDirectory = this.openFileDialog1.InitialDirectory;
            this.saveFileDialog1.FileName = this.openFileDialog1.FileName;
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
                    float height = this.ClientSize.Height;
                    float width = this.ClientSize.Width - this.pnlInfo.Width;

                    e.Graphics.FillRectangle(Brushes.Black, 0, 0, width, height);

                    float scale = GetScale();

                    float startX = GetStartX();

                    float minQuality = int.MaxValue, maxQuality = int.MinValue,
                           minPop = int.MaxValue, maxPop = int.MinValue, minStr = int.MaxValue, maxStr = int.MinValue;
                    foreach (Planet planet in game.GetPlanets())
                    {
                        GetVals(ref minQuality, ref maxQuality, planet.Quality);
                        if (planet.Colony != null)
                            GetVals(ref minPop, ref maxPop, planet.Colony.Population);
                    }
                    foreach (Player player in game.GetPlayers())
                        foreach (Ship ship in player.GetShips())
                            GetVals(ref minStr, ref maxStr, (float)ship.GetStrength() * ship.HP / (float)ship.MaxHP);

                    float newSize = font.Size * scale / e.Graphics.MeasureString("99%", font).Width;
                    if (newSize != font.Size)
                    {
                        if (newSize > 13f)
                            newSize = 13f;
                        font.Dispose();
                        font = new Font("arial", newSize);
                    }

                    Dictionary<Tile, float> moves = null;
                    if (showMoves)
                        moves = GetMoves(game.CurrentPlayer);

                    Tile[,] map = game.GetMap();
                    for (int x = 0 ; x < game.Diameter ; ++x)
                        for (int y = 0 ; y < game.Diameter ; ++y)
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
                                        string s = FormatDouble(move);
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
            else if (planet != null || ( this.isDialog ? ValidDialogTile(tile) : ship != null && ship.CurSpeed > 0 && ship.Player.IsTurn ))
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
            float height = this.ClientSize.Height - 5;
            float width = this.ClientSize.Width - this.pnlInfo.Width - 5;
            return (float)Math.Min(height / (double)game.Diameter, width / (double)( game.Diameter + .5 ));
        }

        private float GetStartX()
        {
            float width = this.ClientSize.Width - this.pnlInfo.Width;
            float scale = GetScale();
            return width - scale * ( game.Diameter + .5f ) - 3;
        }

        private Dictionary<Tile, float> GetMoves(Player player)
        {
            Dictionary<Tile, Point> temp = new Dictionary<Tile, Point>();
            Dictionary<Tile, float> totals = new Dictionary<Tile, float>();
            foreach (Player enemy in game.GetPlayers())
                if (enemy != player)
                {
                    foreach (Ship ship in enemy.GetShips())
                        AddShip(totals, temp, enemy, ship, ship.MaxSpeed);
                    foreach (Colony colony in enemy.GetColonies())
                        if (colony.HP > 0)
                            AddShip(totals, temp, enemy, colony, 1);
                }

            Dictionary<float, float> statFromValue = new Dictionary<float, float>(totals.Count);
            Dictionary<Tile, float> retVal = new Dictionary<Tile, float>(totals.Count);
            foreach (KeyValuePair<Tile, float> pair in totals)
            {
                Tile tile = pair.Key;
                Ship ship;
                if (tile.SpaceObject == null || ( ( ship = tile.SpaceObject as Ship ) != null && ship.Player == player ))
                {
                    float key = pair.Value;
                    float value;
                    if (!statFromValue.TryGetValue(key, out value))
                    {
                        value = GetStatFromValue(pair.Value);
                        statFromValue.Add(key, value);
                    }
                    retVal.Add(tile, value);
                }
            }
            return retVal;
        }

        private static float GetStatFromValue(float value)
        {
            float min = 1, max = (float)Math.Round(Math.Sqrt(value) + .1, 1);
            while (true)
            {
                float diff = max - min;
                float mid = min + diff / 2.0f;
                if (diff < .15f)
                    if (ShipDesign.GetStatValue(mid) < value)
                        return max;
                    else
                        return min;
                mid = (float)Math.Round(mid, 1, MidpointRounding.AwayFromZero);
                if (ShipDesign.GetStatValue(mid) < value)
                    min = mid;
                else
                    max = mid;
            }
        }

        private void AddShip(Dictionary<Tile, float> totals, Dictionary<Tile, Point> temp, Player enemy, Combatant combatant, int speed)
        {
            temp.Clear();
            AddTiles(temp, enemy, combatant.Tile, speed);

            float statValue = (float)ShipDesign.GetStatValue(combatant.Att);
            foreach (KeyValuePair<Tile, Point> pair in temp)
            {
                float val;
                totals.TryGetValue(pair.Key, out val);
                totals[pair.Key] = val + pair.Value.X * statValue;
            }
        }

        private void AddTiles(Dictionary<Tile, Point> retVal, Player enemy, Tile tile, int speed)
        {
            foreach (Tile neighbor in Tile.GetNeighbors(tile))
            {
                Point v1;
                retVal.TryGetValue(neighbor, out v1);
                int damage = v1.X;
                int move = v1.Y;
                int newDamage = Math.Max(speed, damage);
                int newMove = speed - 1;

                bool added = false;
                if (newMove > move)
                {
                    Ship ship;
                    if (( neighbor.SpaceObject == null || ( ( ship = neighbor.SpaceObject as Ship ) != null && ship.Player == enemy ) )
                            && Ship.CheckZOC(enemy, tile, neighbor))
                    {
                        added = true;
                        retVal[neighbor] = new Point(newDamage, newMove);
                        AddTiles(retVal, enemy, neighbor, newMove);
                    }
                }

                if (!added && newDamage > damage)
                    retVal[neighbor] = new Point(newDamage, move);
            }
        }

        #endregion //Drawing

        #region Events

        private void btnShowMoves_Click(object sender, EventArgs e)
        {
            showMoves = true;
            InvalidateMap();
        }

        private void btnGraphs_Click(object sender, EventArgs e)
        {
            GraphsForm.ShowDialog(this, game);
        }

        private void btnNewGame_Click(object sender, EventArgs e)
        {
            Player black = new Player("Black", Color.Blue);
            Player blue = new Player("Blue", Color.Cyan);
            Player green = new Player("Green", Color.Green);
            Player pink = new Player("Pink", Color.Magenta);
            Player red = new Player("Red", Color.Red);
            Player yellow = new Player("Yellow", Color.Gold);
            game = new Game(new Player[] { black, blue, green, pink, red, yellow },
                    Game.Random.GaussianCappedInt(16.5f, .21f, 13) + Game.Random.OEInt(1.3),
                    Game.Random.GaussianCapped(0.006, .5, 0.0021));

            mouse = new Point(ClientSize.Width / 2, ClientSize.Height / 2);
            StartGame();

            game.StartGame(this);

            saved = false;
            this.RefreshAll();
        }

        private void btnLoadGame_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog1.FileName;
                this.saveFileDialog1.InitialDirectory = Path.GetDirectoryName(filePath);
                this.saveFileDialog1.FileName = Path.GetFileName(filePath);

                game = Game.LoadGame(filePath);

                StartGame();

                saved = true;
                RefreshAll();
            }
        }

        private void StartGame()
        {
            this.started = true;
            this.pnlInfo.Show();
            this.btnNewGame.Hide();
            this.btnLoadGame.Hide();
        }

        private void btnSaveGame_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string filePath = saveFileDialog1.FileName;
                this.saveFileDialog1.InitialDirectory = Path.GetDirectoryName(filePath);
                this.saveFileDialog1.FileName = Path.GetFileName(filePath);

                game.SaveGame(filePath);
                saved = true;
            }
        }

        private void GameForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (sender == this.pnlInfo)
                this.mouse = new Point(this.pnlInfo.Location.X + e.Location.X, this.pnlInfo.Location.Y + e.Location.Y);
            else
                this.mouse = e.Location;
        }

        private void btnProduction_Click(object sender, EventArgs e)
        {
            Colony colony = ( (Planet)this.selectedTile.SpaceObject ).Colony;
            colony.StartBuilding(ChangeBuild(colony, true, true));

            saved = false;
            this.RefreshAll();
        }

        private Buildable ChangeBuild(Colony colony, bool accountForIncome, bool switchLoss, params double[] additionalLosses)
        {
            this.selectedTile = colony.Planet.Tile;
            this.RefreshAll();

            return ProductionForm.ShowDialog(this, colony, accountForIncome, switchLoss, additionalLosses);
        }

        private void btnProdRepair_Click(object sender, EventArgs eventArgs)
        {
            Colony colony = ( (Planet)this.selectedTile.SpaceObject ).Colony;
            if (colony.RepairShip == null)
            {
                Tile tile = SelectTile(this.selectedTile, false);
                if (tile != null)
                    try
                    {
                        colony.RepairShip = tile.SpaceObject as Ship;
                    }
                    catch (AssertException e)
                    {
                        Console.Write(e.ToString());
                    }
            }
            else
            {
                colony.RepairShip = null;
            }

            saved = false;
            this.RefreshAll();
        }

        private Tile SelectTile(Tile tile, bool build)
        {
            if (build)
                dialog.pnlBuild.SetBuildable((Buildable)( (Planet)tile.SpaceObject ).Colony.Buildable);
            dialog.pnlBuild.Visible = build;
            dialog.btnCancel.Visible = build;

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

            int HP = SliderForm.ShowDialog(this, new GoldRepair(ship));

            if (HP > 0)
            {
                ship.GoldRepair(HP);

                saved = false;
                this.RefreshAll();
            }
        }

        private void btnDisband_Click(object sender, EventArgs e)
        {
            Ship ship = (Ship)this.selectedTile.SpaceObject;
            double disbandValue = ship.DisbandValue;
            double goldValue = ship.GetDestroyGold();
            double total = disbandValue + goldValue;

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

            Buildable buildable = null;
            if (colony != null && colony.Player.IsTurn)
                buildable = colony.Buildable;

            if (buildable is StoreProd)
            {
                double loseProduction = disbandValue * Consts.StoreProdLossPct;
                goldValue += loseProduction / Consts.ProductionForGold;
                disbandValue -= loseProduction;
            }

            if (buildable != null && ShowOption("Disband for " + FormatDouble(disbandValue) + " production and " + FormatDouble(goldValue) + " gold?"))
                ship.Disband(colony);
            else if (ShowOption("Disband for " + FormatDouble(total) + " gold?"))
                ship.Disband(null);

            saved = false;
            this.RefreshAll();
        }

        private void btnEndTurn_Click(object sender, EventArgs e)
        {
            if (CheckGold() && CheckShips())
            {
                showMoves = false;
                game.EndTurn(this);

                this.hold.Clear();
                SelectNextShip();

                saved = false;
                this.RefreshAll();

                CombatForm.FlushLog(this);
            }
        }

        private bool CheckGold()
        {
            bool end = true;

            double gold = game.CurrentPlayer.GetMinGold();
            if (game.CurrentPlayer.Gold < -gold)
                end = ShowOption("You are running out of gold.  Partial production may be sold and one or more ships disbanded.  Are you sure you want to end your turn?", true);

            return end;
        }

        private bool CheckShips()
        {
            bool end = true;

            foreach (Ship ship in game.CurrentPlayer.GetShips())
                if (ship.CurSpeed > 0 && !hold.Contains(ship))
                {
                    end = ShowOption("You have not moved all of your ships.  Are you sure you want to end your turn?");
                    break;
                }

            return end;
        }

        private void HoldPosistion(Ship ship)
        {
            hold.Add(ship);
        }

        private void SelectNextShip()
        {
            ReadOnlyCollection<Ship> ships = game.CurrentPlayer.GetShips();
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
                game.CurrentPlayer.GoldEmphasis = chkGold.Checked;
                game.CurrentPlayer.ResearchEmphasis = chkResearch.Checked;
                game.CurrentPlayer.ProductionEmphasis = chkProduction.Checked;

                RefreshAll();
            }
        }

        private void GameForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!saved && !ShowOption("Are you sure you want to quit without saving?", true))
                e.Cancel = true;
            else
                Game.Random.Dispose();
        }

        private void GameForm_MouseClick(object sender, MouseEventArgs e)
        {
            if (started)
            {
                float scale = GetScale();

                int y = (int)( e.Y / scale );
                int x = (int)( ( ( e.X - GetStartX() ) - ( y % 2 == 0 ? 0 : scale / 2f ) ) / scale );

                if (x >= 0 && y >= 0 && x < game.Diameter && y < game.Diameter)
                {
                    Tile clickedTile = game.GetMap()[x, y];
                    if (clickedTile != null)
                        if (e.Button == MouseButtons.Left)
                        {
                            this.selectedTile = clickedTile;
                            Ship ship = this.selectedTile.SpaceObject as Ship;
                            if (ship != null)
                                hold.Remove(ship);

                            if (this.isDialog && ValidDialogTile(this.selectedTile))
                            {
                                this.DialogResult = DialogResult.OK;
                                this.Close();
                                return;
                            }
                        }
                        else if (!this.isDialog && e.Button == MouseButtons.Right)
                        {
                            Colony colony = GetSelectedColony();
                            Ship target = clickedTile.SpaceObject as Ship;
                            if (colony != null && target != null && colony.Player != target.Player && colony.HP > 0)
                            {
                                CombatForm.ShowDialog(this, colony, target);
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
                                        HoldPosistion(ship);

                                if (ship != null)
                                {
                                    Combatant defender = null;
                                    Planet planet = clickedTile.SpaceObject as Planet;
                                    if (planet == null)
                                        defender = clickedTile.SpaceObject as Combatant;
                                    else if (oldSpeed == 0 || !ship.Player.IsTurn || !Tile.IsNeighbor(ship.Tile, planet.Tile))
                                        defender = planet.Colony;
                                    if (defender != null && ship.Player != defender.Player && defender.HP > 0)
                                        selectNext &= Attack(ship, defender);
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
                        Console.Write(e.ToString());
                    }
                }
            }

            return selectNext;
        }

        private void TargetTile(Tile targetTile, Ship ship)
        {
            bool move = ( ship.CurSpeed > 0 );
            if (move && ship.Colony && ship.Population == 0)
                foreach (Tile neighbor in Tile.GetNeighbors(ship.Tile))
                {
                    Planet planet = neighbor.SpaceObject as Planet;
                    if (planet != null)
                    {
                        if (planet.Colony != null && planet.Colony.Player.IsTurn && !Tile.IsNeighbor(planet.Tile, targetTile))
                            move = ShowOption("Are you sure you want to move away with no troops?");
                        break;
                    }
                }

            if (move)
            {
                ship.Move(targetTile);
                this.selectedTile = targetTile;
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
            return CombatForm.ShowDialog(this, attacker, defender);
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
                double goldCost = ship.Population * Consts.MovePopulationGoldCost + planet.ColonizationCost;
                string cost = FormatDouble(goldCost);
                if (goldCost > ship.Player.Gold)
                {
                    MessageBox.Show("You need " + cost + " gold to colonize this planet.");
                }
                else if (ShowOption("Colonize planet for " + cost + " gold?"))
                {
                    bombard = false;
                    ship.Colonize(planet, this);
                    this.selectedTile = planet.Tile;
                }
            }

            if (bombard && ship.CurSpeed > 0 && ( !ship.Colony || ShowOption("Bombard planet?") ))
            {
                if (ship.CurSpeed == 1)
                    this.selectedTile = planet.Tile;
                selectNext = false;

                ship.Bombard(planet, this);
                CombatForm.FlushLog(this);
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

                ship.Bombard(targetColony.Planet, this);
                CombatForm.FlushLog(this);
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
                int troops = SliderForm.ShowDialog(this, new MoveTroops(game, from, to));
                if (troops > 0)
                {
                    this.selectedTile = to.Tile;
                    from.MovePop(troops, to);
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

            int gold = -1, troops = -1;
            if (ship.AvailablePop > 0)
                if (colony.Population > 0)
                {
                    Invade invadeSlider = new Invade(ship, colony);
                    gold = SliderForm.ShowDialog(this, invadeSlider);
                    troops = invadeSlider.GetTroops(gold);
                }
                else
                {
                    gold = int.MaxValue;
                    troops = SliderForm.ShowDialog(this, new MoveTroops(game, ship, colony));
                }

            bool selectShip = true;
            if (troops > 0)
            {
                int extraPop = ship.AvailablePop - troops;
                ship.Invade(colony, gold, troops, ref extraPop, this);

                if (extraPop > 0)
                    ship.MovePop(extraPop, planet.Colony);

                if (!planet.Dead)
                    selectShip = false;
            }
            else if (ship.CurSpeed > 0 && ( colony.HP > 0 || ship.AvailablePop == 0 || ShowOption("Bombard planet?") ))
            {
                if (colony.HP > 0 && ( !ship.DeathStar || !ShowOption("Bombard planet?") ))
                {
                    CombatForm.ShowDialog(this, ship, colony);
                }
                else
                {
                    ship.AttackColony(colony, this);
                    CombatForm.FlushLog(this);
                }

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
                Console.Write(e.ToString());

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
            MessageBox.Show("Minimum gold: " + FormatDouble(game.CurrentPlayer.GetMinGold()));
        }

        private void lbl4_Click(object sender, EventArgs e)
        {
            Colony colony = GetSelectedColony();
            if (colony != null)
                PlanetDefenseForm.ShowDialog(this, colony);
        }

        private void lbl5_Click(object sender, EventArgs e)
        {
            Colony colony = GetSelectedColony();
            if (colony != null && colony.Player.IsTurn)
            {
                double population = 0, production = 0, gold = 0, gold2;
                int research = 0, production2, research2;
                colony.GetTurnIncome(ref population, ref production, ref gold, ref research, false);
                colony.GetTurnValues(out production2, out gold2, out research2);
                string modInc = FormatDouble(production + gold + research);
                string modGold = FormatDouble(gold);
                string modProd = FormatUsuallyInt(production);
                MessageBox.Show(string.Format("Income: {0}{7}\r\nUpkeep: {1}\r\n\r\nGold: {2}{5}\r\nResearch: {3}\r\nProduction: {4}{6}",
                        modInc, FormatDouble(colony.Upkeep), modGold, research, modProd,
                        ShowOrig(FormatDouble(gold2), modGold), ShowOrig(production2.ToString(), modProd),
                        ShowOrig(FormatDouble(colony.GetTotalIncome()), modInc)));
            }
        }

        private string ShowOrig(string orig, string mod)
        {
            if (orig == mod)
                return string.Empty;
            return string.Format(" ({0})", orig);
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
            CombatForm.ShowDialog(this);
        }

        private void btnInvasion_Click(object sender, EventArgs e)
        {
            InvadeCalculatorForm.ShowDialog(this);
        }

        #endregion //Events

        #region Refresh

        public void RefreshAll()
        {
            if (!ended && game.GetPlayers().Length < 2)
            {
                ended = true;
                TextForm.ShowDialog(this, game.GetGameResult());
                game.AutoSave();
            }

            RefreshCurrentPlayer();
            RefreshSelectedInfo();
            InvalidateMap();
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
            this.lblPlayer.BackColor = game.CurrentPlayer.Color;
            this.lblPlayer.Text = game.Turn.ToString() + " - " + game.CurrentPlayer.Name;
            RefreshPlayerInfo();
        }

        private void RefreshPlayerInfo()
        {
            this.lblPopulation.Text = game.CurrentPlayer.GetPopulation().ToString();
            this.lblGold.Text = FormatDouble(game.CurrentPlayer.Gold);

            int research;
            double population, production, gold;
            game.CurrentPlayer.GetTurnIncome(out population, out research, out production, out gold);

            FormatIncome(lblPopInc, population);
            FormatIncome(lblGoldInc, gold, true);
            FormatIncome(lblResearch, research);
            this.lblRsrchPct.Text = FormatPct(game.CurrentPlayer.GetResearchChance(research));
            FormatIncome(lblProduction, production);

            emphasisEvent = false;
            chkGold.Checked = game.CurrentPlayer.GoldEmphasis;
            chkResearch.Checked = game.CurrentPlayer.ResearchEmphasis;
            chkProduction.Checked = game.CurrentPlayer.ProductionEmphasis;
            emphasisEvent = true;
        }

        private void FormatIncome(Label label, double income)
        {
            FormatIncome(label, income, false);
        }

        private void FormatIncome(Label label, double income, bool forceDouble)
        {
            string sign = string.Empty;

            if (income < 0)
            {
                label.ForeColor = Color.DarkRed;
            }
            else
            {
                label.ForeColor = Color.Black;

                if (income > 0)
                    sign = "+";
            }

            label.Text = sign + ( forceDouble ? FormatDouble(income) : FormatUsuallyInt(income) );

            if (label.TextAlign == ContentAlignment.MiddleRight)
                label.Width = ( label.Text.Contains(".") ? 46 : 35 );
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
                this.btnGoldRepair.Visible = ship.HP < ship.MaxHP && !ship.HasRepaired;
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

            this.lbl6.Text = "Experience";
            this.lbl6Inf.Text = ship.GetTotalExp().ToString() + " (" + Enum.GetName(typeof(Ship.ExpType), ship.NextExpType) + ")";

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
                    foreach (Tile tile in Tile.GetNeighbors(colony.Planet.Tile))
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
                    double HP = repairShip.GetHPForProd(colony.GetProductionIncome());
                    if (HP > repairShip.MaxHP - repairShip.HP)
                        HP = repairShip.MaxHP - repairShip.HP;
                    this.lbl6Inf.Text = "Repair +" + FormatDouble(HP);
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

            double prodInc = colony.GetProductionIncome();
            Ship repairShip = colony.RepairShip;
            if (repairShip != null)
            {
                prodInc -= repairShip.GetProdForHP(repairShip.MaxHP - repairShip.HP);
                if (prodInc < 0)
                    prodInc = 0;
            }

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

            if (prodInc != 0)
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

        public static string FormatUsuallyInt(double value)
        {
            return FormatDouble(value).TrimEnd('0').TrimEnd('.');
        }

        public static string FormatDouble(double value)
        {
            return value.ToString("0.0");
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
            return ( place ? FormatDouble(pct) : pct.ToString("0") ) + "%";
        }

        #endregion //Refresh

        #region IEventHandler Members

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

        int IEventHandler.MoveTroops(Colony fromColony, int total, int free, int totalPop, double soldiers)
        {
            if (fromColony != null)
                this.selectedTile = fromColony.Planet.Tile;
            this.RefreshAll();

            return SliderForm.ShowDialog(this, new MoveTroops(game, fromColony, total, free, totalPop, soldiers));
        }

        bool IEventHandler.ConfirmCombat(Combatant attacker, Combatant defender)
        {
            if (attacker is Ship)
                this.selectedTile = attacker.Tile;
            else
                this.selectedTile = defender.Tile;
            this.RefreshAll();

            return CombatForm.ShowDialog(this, attacker, defender, true);
        }

        void IEventHandler.OnResearch(ShipDesign newDesign, HashSet<ShipDesign> obsolete, PlanetDefense oldDefense, PlanetDefense newDefense)
        {
            this.RefreshAll();

            ResearchForm.ShowDialog(this, newDesign, obsolete, oldDefense, newDefense);
        }

        void IEventHandler.OnCombat(Combatant attacker, Combatant defender, int attack, int defense, int popLoss)
        {
            CombatForm.Combat(attacker, defender, attack, defense, popLoss);
        }

        void IEventHandler.OnLevel(Ship ship, Ship.ExpType expType, double pct, int needExp, int lastExp)
        {
            CombatForm.LevelUp(ship, expType, pct, needExp, lastExp);
        }

        #endregion

        private string log = string.Empty;

        public void LogMsg(string format, params object[] args)
        {
            string msg = string.Format(format, args);
            Console.Write(msg);
            log += msg;
            LogMsg();
        }

        public void LogMsg()
        {
            Console.WriteLine();
            log += "\r\n";
        }

        public string GetLog()
        {
            return log;
        }
    }
}
