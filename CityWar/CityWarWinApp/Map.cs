using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using CityWar;
using System.IO;

namespace CityWarWinApp
{
    partial class Map : Form
    {
        #region Fields and Loading

        public static Game game;
        public static Game Game
        {
            get
            {
                return game;
            }
            set
            {
                game = value;
                Game.AutoSavePath = MainMenu.SavePath;
            }
        }

        //public static Screen GetScreen()
        //{
        //}

        //constants 
        public const int panelWidth = 255;
        public const int startZoom = 100;

        //miscellaneous
        static List<Piece> okToMove = new List<Piece>();
        Point _selected = new Point(-1, -1);
        Point selected
        {
            get
            {
                return _selected;
            }
            set
            {
                if (_selected != value)
                {
                    _selected = value;

                    if (Game.GetTile(_selected.X, _selected.Y) == null)
                        _selected = new Point(-1, -1);

                    CheckPath();
                }
            }
        }
        Rectangle oldBounds, invalidateRectangle;
        UnitInfo InfoForm;
        bool storeOld = true, saved = true, canClose = false;
        int lastGroup = -1;

        //scrolling
        bool xAxis, yAxis, up = false, down = false, left = false, right = false;
        float offX = 0, offY = 0, scrollSpeed;

        //zooming - set the starting zoom
        //float _zoom = Game.Random.GaussianCapped(startZoom, .091f, 30f);
        float _zoom = 106;
        float topX, topY, side, middle;
        Font tileInfoFont;

        ChangeTracker changeTracker = new();
        public float Zoom
        {
            get
            {
                return _zoom;
            }
            set
            {
                _zoom = value;
                //refresh all the zoom values
                RefreshZoom();
            }
        }

        public Map()
        {
            InitializeComponent();
            this.panelPieces.Initialize(GetPiecesForPiecesPanel, GetDrawFlagsForPiece, GetTextForPiece, () => Brushes.DarkMagenta);
            //this.MouseWheel += new MouseEventHandler(this.panelPieces.PiecesPanel_MouseWheel);
            this.MouseWheel += new MouseEventHandler(MainMap_MouseWheel);
            this.ResizeRedraw = false;

            //reduce flickering
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);

            //the rectangle to draw the map
            invalidateRectangle = new Rectangle(0, 0, this.ClientSize.Width - panelWidth, this.ClientSize.Height);
        }

        private void MainMap_Load(object sender, EventArgs e)
        {
            //set to full screen
            this.FormBorderStyle = FormBorderStyle.None;
            setOldBounds();
            storeOld = false;
            this.Bounds = GetScreenBounds();
            storeOld = true;

            //refresh information
            RefreshCurrentPlayer();
            RefreshResources();
            RefreshButtons();
            RefreshZoom();

            CenterOn(Game.CurrentPlayer.GetCenter());
        }

        #endregion

        #region Check Aircraft

        //check if moving an aircraft might end up destroying it
        public static bool CheckAircraft(Piece piece, int moveMod, int fuelMod)
        {
            return CheckAircraft(piece, moveMod, fuelMod, piece.Tile);
        }
        static bool CheckAircraft(Piece piece, int moveMod, int fuelMod, Tile toTile)
        {
            //not enough movement, so this is immaterial
            if (piece.Movement < 1 || (piece.IsAir() && ((Unit)piece).Fuel < 1))
                return true;

            return CheckAircraft(piece, moveMod, fuelMod, toTile, null);
        }
        static bool CheckAircraft(Piece piece, int moveMod, int fuelMod, Tile toTile, Dictionary<Tile, Tile> CanGetCarrier)
        {
            //cant move if it isn't your turn
            if (piece.Owner != Game.CurrentPlayer)
                return true;

            Unit unit = piece as Unit;
            if (piece.IsAbility(Ability.AircraftCarrier))
            {
                //store variables for the Tile.CheckAircraft instance method
                Tile.MovedCarrier = piece;
                Tile.MovedToTile = toTile;
                Tile.MoveModifier = moveMod;

                //if this piece is a carrier, check each aircraft the player owns
                foreach (Piece aircraft in piece.Owner.GetPieces())
                    if (aircraft.IsAir())
                        //call recursively for the aircraft
                        if (!CheckAircraft(aircraft, 0, 0, aircraft.Tile, CanGetCarrier))
                        {
                            Tile.MovedCarrier = null;
                            Tile.MovedToTile = null;
                            Tile.MoveModifier = 0;

                            //make sure that the carrier could have reached the aircraft were it not to move
                            //do this by simply checking again with the static variables cleared
                            if (CheckAircraft(aircraft, 0, 0, toTile, CanGetCarrier))
                            {
                                //the carrier can save the aircraft by not moving
                                return false;
                            }
                            else
                            {
                                //the aircraft is doomed either way, so store the variables again and keep going
                                Tile.MovedCarrier = piece;
                                Tile.MovedToTile = toTile;
                                Tile.MoveModifier = moveMod;
                            }
                        }

                Tile.MovedCarrier = null;
                Tile.MovedToTile = null;
                Tile.MoveModifier = 0;

                //all aircraft passed, so go ahead and move
                return true;
            }
            //if the piece is an aircraft, and has not already been OKed, call the Tile.CheckAircraft instance method
            else if (piece.IsAir() && !(unit != null && !unit.IsRanded)
               && !okToMove.Contains(piece) && toTile.CheckAircraft(moveMod, fuelMod, piece, ref CanGetCarrier))
            {
                //if moving may cause it to die, show a dialog asking confirmation
                if (new Aircraft(piece).ShowDialog() == DialogResult.OK)
                {
                    //store that the aircraft has been OKed to die (kinda sadistic)
                    okToMove.Add(piece);
                    return true;
                }
                else
                    return false;
            }

            //the piece is not either an aircraft or an aircraft carrier, so go ahead and move
            return true;
        }

        #endregion

        #region Refreshing and Centering

        private void RefreshCurrentPlayer()
        {
            Player currentPlayer = Game.CurrentPlayer;
            this.lblTurn.Text = Game.Turn.ToString();
            this.lblPlayer.Text = currentPlayer.Name;
            this.lblPlayer.BackColor = currentPlayer.Color;
            this.lblPlayer.ForeColor = currentPlayer.InverseColor;
        }

        internal void RefreshResources()
        {
            Player currentPlayer = Game.CurrentPlayer;
            this.lblAir.Text = currentPlayer.Air.ToString();
            this.lblDeath.Text = currentPlayer.Death.ToString();
            this.lblEarth.Text = currentPlayer.Earth.ToString();
            this.lblWizard.Text = currentPlayer.Magic.ToString();
            this.lblNature.Text = currentPlayer.Nature.ToString();
            this.lblWork.Text = currentPlayer.Work.ToString();
            this.lblProd.Text = currentPlayer.Production.ToString();
            this.lblWater.Text = currentPlayer.Water.ToString();
            this.lblRelic.Text = currentPlayer.RelicProgress.ToString();
            this.lblPpl.Text = currentPlayer.Population.ToString();

            //bold work when you are broke and may lose some resources
            bool small = (currentPlayer.GetTurnUpkeep() < currentPlayer.Work);
            this.lblWork.Font = new System.Drawing.Font("Arial", small ? 9.75F : 11.25F,
                    small ? FontStyle.Regular : FontStyle.Bold);

            //bold magic when you can afford a wizard
            small = (currentPlayer.Magic < Player.WizardCost);
            this.lblWizard.Font = new System.Drawing.Font("Arial", small ? 9.75F : 11.25F,
                    small ? FontStyle.Regular : FontStyle.Bold);
        }

        private void RefreshButtons()
        {
            this.btnBuildPiece.Visible = false;
            this.btnCaptureCity.Visible = false;
            this.btnDisbandUnits.Visible = false;
            this.btnRest.Visible = false;
            this.btnGroup.Visible = false;
            this.btnUngroup.Visible = false;

            //unrelated to current selection
            this.btnUndo.Visible = Game.CanUndoCommand();

            if (selected.X != -1 && selected.Y != -1)
            {
                Tile selectedTile = Game.GetTile(selected.X, selected.Y);
                Piece[] selectedPieces = selectedTile.GetSelectedPieces();
                Player owner;
                selectedTile.Occupied(out owner);

                foreach (Piece p in selectedPieces)
                {
                    //if any of the pieces are a Capturable, show the build piece button
                    if (p is Capturable)
                        this.btnBuildPiece.Visible = true;

                    //if the tile has a neutral city and any pieces have full move, show the capture city button
                    if (selectedTile.Treasure != null && selectedTile.Treasure.UnitCollect && !selectedTile.Treasure.Collected
                            && p is Unit && p.MaxMove != 0 && p.Movement == p.MaxMove && !p.IsAir())
                        this.btnCaptureCity.Visible = true;

                    //if any selected pieces are units, show the disband units button
                    if (p is Unit)
                        this.btnDisbandUnits.Visible = true;

                    //if any selected pieces have movement left, show the rest button
                    if (p.Movement > 0)
                        this.btnRest.Visible = true;
                }

                //check if the only selected piece is a wizard with full move, in which case you can change the map terrain
                if (selectedPieces.Length == 1 && selectedPieces[0] is Wizard && selectedPieces[0].Movement == selectedPieces[0].MaxMove)
                    this.btnRest.Text = "Map";
                else
                    this.btnRest.Text = "Rest";

                //if the current player occupies the selected tile, show the grouping buttons
                bool group = (owner == Game.CurrentPlayer);
                this.btnGroup.Visible = group;
                this.btnUngroup.Visible = group;
            }
        }

        private void RefreshZoom()
        {
            //the ratio of a hexagon's side length to its cross length, divided by four - sqrt(1/12)
            const float mult = 0.288675f;

            //change the scroll speed based on the zoom
            scrollSpeed = (float)(Math.Sqrt(Zoom * startZoom) / 2.6f);

            //calculate side and cross length of the hexes
            side = Zoom * mult;
            middle = Zoom / 4f;

            //see if there is enough room to scroll each axis
            xAxis = ((Zoom * Game.Diameter) > this.ClientSize.Width - panelWidth);
            yAxis = ((side * 3 * Game.Diameter) > this.ClientSize.Height);

            //get the maximum values for offX and offY
            if (xAxis)
                topX = ((float)Game.Diameter + .5f) * (float)Zoom - ((float)ClientSize.Width - panelWidth - 24f);
            if (yAxis)
                topY = ((float)Game.Diameter + 1f / 3f) * ((float)Zoom * 3f * mult) - (float)ClientSize.Height + 26f;

            //get the font for city and wizard numbers on a tile
            if (tileInfoFont != null)
                tileInfoFont.Dispose();
            tileInfoFont = new Font("Arial", Zoom / 9f);

            //reset the pics so they can be the proper size
            Game.ResetPics(this._zoom);

            //center on the selected tile
            CenterOnSelected();
        }

        private void CenterOnSelected()
        {
            if (selected.X == -1 && selected.Y == -1)
                CenterOn(Game.CurrentPlayer.GetCenter());
            else
                CenterOn(Game.GetTile(selected.X, selected.Y));
        }

        private void CenterOn(Tile tile)
        {
            //the x and y to center on
            selected = new Point(tile.X, tile.Y);

            //get the x and y distances to use
            float y = (float)ClientSize.Height / 2f, xx = ((float)(ClientSize.Width - panelWidth)) / 2f;

            //set the offY
            if (yAxis)
                offY = (side * (3f * (float)tile.Y + 2f) - y + 13f);

            //set the offX
            if (xAxis)
                if (tile.Y % 2 == 0)
                    offX = (float)tile.X * Zoom + Zoom - xx + 13f;
                else
                    offX = (2f * (float)tile.X * Zoom + Zoom - 2f * (xx + 13f)) / 2f;

            //check maximums/minimums
            if (xAxis)
            {
                if (offX < 0)
                    offX = 0;
                else if (offX > topX)
                    offX = topX;
            }
            else
                offX = 0;
            if (yAxis)
            {
                if (offY < 0)
                    offY = 0;
                else if (offY > topY)
                    offY = topY;
            }
            else
                offY = 0;

            //repaint the map
            this.Invalidate(invalidateRectangle, false);

            //center the selected unit in the units panel
            CenterUnit();
        }

        private void CenterUnit()
        {
            if (selected != null && selected.X != -1)
            {
                Game.GetTile(selected.X, selected.Y).Select();
                panelPieces.ScrollToSelected();
                RefreshButtons();
            }
        }

        private void setOldBounds()
        {
            //store the current bounds as the old bounds, if the event is set to fire
            if (storeOld)
                oldBounds = new Rectangle(this.Bounds.X, this.Bounds.Y, this.Bounds.Width, this.Bounds.Height);
        }

        #endregion

        #region Units Panel

        void panelPieces_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                //try to close the info form, if it's up
                InfoForm.Close();
        }

        private void panelPieces_MouseClick(object sender, MouseEventArgs e)
        {
            //check if a tile is selected
            if (selected.X != -1 && selected.Y != -1)
            {
                Piece clicked = panelPieces.GetClickedPiece(e);
                if (clicked != null)
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        Tile selectedTile = Game.GetTile(selected.X, selected.Y);
                        lastGroup = selectedTile.CurrentGroup;
                        //click on the piece
                        selectedTile.ClickOn(clicked, CheckModifier(Keys.Shift), CheckModifier(Keys.Control), CheckModifier(Keys.Alt));
                        RefreshButtons();
                        panelPieces.Invalidate();

                        CheckPath();
                    }
                }
            }
        }

        private bool CheckModifier(Keys key)
        {
            return ((Control.ModifierKeys & key) != Keys.None);
        }

        private void panelPieces_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            //check if a tile is selected
            if (selected.X != -1 && selected.Y != -1)
            {
                Piece clicked = panelPieces.GetClickedPiece(e);
                if (clicked != null)
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        Tile selectedTile = Game.GetTile(selected.X, selected.Y);
                        selectedTile.CurrentGroup = lastGroup;
                        //click on the piece
                        selectedTile.ClickOn(clicked, false, true, false);
                        RefreshButtons();
                        panelPieces.Invalidate();

                        CheckPath();
                    }
                }
            }
        }

        private void panelPieces_MouseDown(object sender, MouseEventArgs e)
        {
            //check if a tile is selected
            if (selected.X != -1 && selected.Y != -1 && e.Button == MouseButtons.Right)
            {
                Piece piece = panelPieces.GetClickedPiece(e);

                //if a piece was clicked on, show the info form
                if (piece != null)
                {
                    try
                    {
                        if (InfoForm != null)
                            InfoForm.Close();
                    }
                    catch
                    {
                    }
                    InfoForm = new UnitInfo(piece, panelPieces.PointToScreen(e.Location), -1);
                    InfoForm.Show();
                }
            }
        }

        private Piece[] GetPiecesForPiecesPanel()
        {
            if (selected.X > -1 && selected.Y > -1)
            {
                return Game.GetTile(selected.X, selected.Y).GetAllPieces();
            }
            return new Piece[0];
        }

        private MattUtil.EnumFlags<PiecesPanel.DrawFlags> GetDrawFlagsForPiece(Piece piece)
        {
            MattUtil.EnumFlags<PiecesPanel.DrawFlags> result = new MattUtil.EnumFlags<PiecesPanel.DrawFlags>();

            if (piece.Owner == Game.CurrentPlayer)
            {
                if (piece.MaxMove > 0)
                    result.Add(PiecesPanel.DrawFlags.Text);
                if (piece.Group == piece.Tile.CurrentGroup)
                    result.Add(PiecesPanel.DrawFlags.Frame);
            }

            return result;
        }

        private Tuple<string, string> GetTextForPiece(Piece piece)
        {
            string fuel = piece.IsAir() ? string.Format(" ({0})", ((Unit)piece).Fuel) : "";
            return new Tuple<string, string>(string.Format("{0} / {1}{2}", piece.Movement, piece.MaxMove, fuel), null);
        }

        #endregion

        #region Draw Map

        //double total = 0, amt = 0;

        private HashSet<Tile> changedTiles = new();
        internal void ClearChanges()
        {
            changedTiles = new();
        }
        private bool ShowChanges(IEnumerable<Tile> tiles)
        {
            changedTiles = tiles.ToHashSet();
            // center on something that changed if anything
            bool any = changedTiles.Any();
            if (any)
                CenterOn(Game.Random.SelectValue(changedTiles));
            return any;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                //System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
                //watch.Reset();
                //watch.Start();

                float OffX = this.offX - 13f;
                float OffY = this.offY - 13f - side;

                //get a bunch of arithmetic out of the way to speed up the loop
                float
                    mid4 = middle * 4f,
                    mid2 = middle * 2f,
                    mid_2 = middle / 2f,
                    side2 = side * 2f,
                    side3 = side * 3f,
                    zoom_9 = _zoom / 9f,
                    zoom_3 = _zoom / 3f,
                    zoom_4_5mid_2 = _zoom / 4.5f + mid_2,
                    zoom_9zoom_6_m = zoom_9 + _zoom / 6f - .0666f * _zoom,
                    _zoom_9side2_3 = -zoom_9 + side * 2.3f,
                    _zoom_9side2_3_p = _zoom_9side2_3 + 3f,
                    zoom_4_5mid_2zoom_3 = zoom_4_5mid_2 + zoom_3,
                    zoom_9mid_2 = zoom_9 + mid_2,
                    zoom_9mid_2zoom_3 = zoom_9mid_2 + zoom_3;

                //get the range of hexes that need to be drawn
                int minX = (int)(offX / _zoom) - 2, maxX = (int)((offX + (float)(ClientSize.Width - panelWidth)) / _zoom) + 1;
                int minY = (int)(offY / (side3)) - 2, maxY = (int)((offY + ClientSize.Height) / (side3)) + 1;
                if (minX < -1)
                    minX = -1;
                if (minY < -1)
                    minY = -1;
                if (maxX > Game.Diameter)
                    maxX = Game.Diameter;
                if (maxY > Game.Diameter)
                    maxY = Game.Diameter;

                //draw the hexes
                foreach (var p in Game.Random.Iterate(minX + 1, maxX - 1, minY + 1, maxY - 1))
                {
                    int X = p.X;
                    int Y = p.Y;
                    //get the current tile being drawn
                    Tile thisTile = Game.GetTile(X, Y);
                    if (thisTile != null)
                    {
                        //Brush textBrush = Brushes.Black;
                        //if (thisTile.Terrain == Terrain.Forest || thisTile.Terrain == Terrain.Water)
                        //    textBrush = Brushes.White;

                        //get the proper color
                        Brush theBrush;
                        switch (thisTile.Terrain)
                        {
                            case Terrain.Forest:
                                theBrush = Brushes.Green;
                                break;
                            case Terrain.Mountain:
                                theBrush = Brushes.DarkOrange;
                                break;
                            case Terrain.Plains:
                                theBrush = Brushes.Gray;
                                break;
                            case Terrain.Water:
                                theBrush = Brushes.Blue;
                                break;
                            default:
                                throw new Exception();
                        }

                        //caulculate the upper left hand corner of the hex
                        float xVal = (float)X * mid4 - OffX + (Y % 2 == 0 ? mid2 : 0f);
                        float yVal = (float)Y * side3 - OffY;

                        //draw the terrain hexegon
                        PointF[] points = new PointF[6];
                        points[0] = new PointF(xVal, yVal);
                        points[1] = new PointF(xVal + mid2, yVal - side);
                        points[2] = new PointF(xVal + mid4, yVal);
                        points[3] = new PointF(xVal + mid4, yVal + side2);
                        points[4] = new PointF(xVal + mid2, yVal + side3);
                        points[5] = new PointF(xVal, yVal + side2);
                        e.Graphics.FillPolygon(theBrush, points);
                        e.Graphics.DrawPolygon(Pens.White, points);

                        //draw tile information
                        if (thisTile.Treasure != null)
                        {
                            e.Graphics.DrawImage(thisTile.Treasure.GetPic(),
                                xVal + _zoom / 9f + middle / 2f, yVal + _zoom / 3f + side / 3f);
                            e.Graphics.DrawString(thisTile.Treasure.Value.ToString(), tileInfoFont, Brushes.White,
                                xVal + _zoom / 3.3f + middle, yVal - _zoom / 9f + side * 2.2f);
                        }
                    }
                }

                using (Pen pen3 = new Pen(Color.Black, 3f))
                {
                    //show the selected tile
                    if (selected.X > minX && selected.X < maxX && selected.Y > minY && selected.Y < maxY)
                    {
                        float XVal = (float)selected.X * mid4 - OffX + (selected.Y % 2 == 0 ? mid2 : 0f);
                        float YVal = (float)selected.Y * side3 - OffY;
                        PointF[] points = new PointF[6];
                        points[0] = new PointF(XVal, YVal);
                        points[1] = new PointF(XVal + mid2, YVal - side);
                        points[2] = new PointF(XVal + mid4, YVal);
                        points[3] = new PointF(XVal + mid4, YVal + side2);
                        points[4] = new PointF(XVal + mid2, YVal + side3);
                        points[5] = new PointF(XVal, YVal + side2);
                        e.Graphics.DrawPolygon(pen3, points);
                    }

                    //draw the units
                    for (int X = minX; ++X < maxX;)
                        for (int Y = minY; ++Y < maxY;)
                        {
                            Tile thisTile = Game.GetTile(X, Y);
                            if (thisTile != null)
                            {
                                ////Brush textBrush = Brushes.Black;
                                ////if (thisTile.Terrain == Terrain.Forest || thisTile.Terrain == Terrain.Water)
                                //Brush textBrush = Brushes.White;

                                Image pic = thisTile.GetPieceImage();
                                if (pic != null)
                                {
                                    float xVal = (float)X * mid4 - OffX + (Y % 2 == 0 ? mid2 : 0f);
                                    float yVal = (float)Y * side3 - OffY;
                                    e.Graphics.DrawImage(pic, xVal + zoom_9, yVal - zoom_9zoom_6_m);

                                    double str = thisTile.GetAllUnits().Sum(u => u.RandedCost * u.GetHealthPct());
                                    if (str > 0)
                                    {
                                        string strDisp = str.ToString("0");
                                        float strWidth = e.Graphics.MeasureString(strDisp, tileInfoFont).Width;
                                        e.Graphics.DrawString(strDisp, tileInfoFont, Brushes.White,
                                            xVal + Zoom - strWidth, yVal);
                                    }
                                }
                            }
                        }

                    if (selected.X != -1 && selected.Y != -1)
                    {
                        var path = Game.GetTile(selected.X, selected.Y).GetSelectedPieces().Where(piece => piece.Path != null).FirstOrDefault();
                        if (path != null && path.Path != null && path.Path.Count > 1)
                        {
                            List<PointF> points = new List<PointF>();
                            foreach (Tile t in path.Path)
                            {
                                float xVal = (float)t.X * mid4 - OffX + (t.Y % 2 == 0 ? mid2 : 0f);
                                float yVal = (float)t.Y * side3 - OffY;
                                points.Add(new PointF(xVal + mid2, yVal + side2 / 2f));
                            }
                            e.Graphics.DrawLines(pen3, points.ToArray());
                        }
                    }
                }

                using (Pen pen4 = new Pen(Color.Black, 2f))
                {
                    foreach (Tile t in changedTiles)
                    {
                        if (t.X > minX && t.X < maxX && t.Y > minY && t.Y < maxY)
                        {
                            float XVal = (float)t.X * mid4 - OffX + (t.Y % 2 == 0 ? mid2 : 0f);
                            float YVal = (float)t.Y * side3 - OffY;
                            PointF[] points = new PointF[6];
                            points[0] = new PointF(XVal, YVal);
                            points[1] = new PointF(XVal + mid2, YVal - side);
                            points[2] = new PointF(XVal + mid4, YVal);
                            points[3] = new PointF(XVal + mid4, YVal + side2);
                            points[4] = new PointF(XVal + mid2, YVal + side3);
                            points[5] = new PointF(XVal, YVal + side2);
                            e.Graphics.DrawPolygon(pen4, points);
                        }
                    }
                }

                //clip it
                e.Graphics.FillRectangle(Brushes.Black, new Rectangle(this.ClientSize.Width - panelWidth, 0, panelWidth, this.ClientSize.Height));

                base.OnPaint(e);

                //watch.Stop();
                //total += watch.ElapsedTicks;
                //++amt;

                //if (amt > 100)
                //{
                //    double avg = total / amt;

                //    total = 0;
                //    amt = 0;
                //}

                //101343.44554455446
                // 97529.0099009901
                // 98505.316831683172
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        #endregion

        #region Mouse Events

        private void MainMap_MouseMove(object sender, MouseEventArgs e)
        {
            //how close to the edge the mouse has to be to scroll
            const int scrollRange = 21;

            //check for x-axis scrolling
            if (e.X < scrollRange)
                left = true;
            else
            {
                left = false;
                if (e.X > ClientSize.Width - scrollRange)
                    right = true;
                else
                    right = false;
            }

            //check for y-axis scrolling
            if (e.Y < scrollRange)
                up = true;
            else
            {
                up = false;
                if (e.Y > ClientSize.Height - scrollRange)
                    down = true;
                else
                    down = false;
            }

            //enable the timer if scrolling
            timerGraphics.Enabled = (up || down || left || right);
            //if (!timerGraphics.Enabled)
            //    Console.WriteLine("MainMap_MouseMove");

            //get the x and y coordinates of the hex over which the mouse resides
            int y = (int)Math.Round(((float)e.Y + offY - 13f + side) / side / 3f) - 1;
            int x = (y % 2 == 0 ? (int)Math.Round(((float)e.X + offX - 13f) / Zoom) - 1
                : (int)Math.Round(((float)e.X + offX - 13f + Zoom / 2f) / Zoom) - 1);

            //show the x and y of the mouse, if it's within range
            if (x >= 0 && x < Game.Diameter && y >= 0 && y < Game.Diameter)
                this.lblMouse.Text = string.Format("({0},{1})", x, y);
            else
                this.lblMouse.Text = "";
        }

        private void MainMap_MouseLeave(object sender, EventArgs e)
        {
            if (this.FormBorderStyle != FormBorderStyle.None)
            {
                //stop scrolling if the mouse leaves the window
                timerGraphics.Enabled = false;
                //Console.WriteLine("MainMap_MouseLeave");
            }
        }

        private void MainMap_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.X < ClientSize.Width - panelWidth)
            {
                int y = (int)Math.Round(((float)e.Y + offY - 13f + side) / side / 3f) - 1;
                int x = (y % 2 == 0 ? (int)Math.Round(((float)e.X + offX - 13f) / Zoom) - 1
                    : (int)Math.Round(((float)e.X + offX - 13f + Zoom / 2f) / Zoom) - 1);

                if (x >= 0 && y >= 0 && x < Game.Diameter && y < Game.Diameter)
                {
                    //select the tile
                    if (e.Button == MouseButtons.Left)
                        selected = new Point(x, y);
                    //right mouse button to move units
                    else if (e.Button == MouseButtons.Right)
                    {
                        //get the selected tile (if it exists) and the clicked tile
                        Tile selectedTile = null;
                        if (selected.X != -1 && selected.Y != -1)
                            selectedTile = Game.GetTile(selected.X, selected.Y);
                        Tile clicked = Game.GetTile(x, y);
                        if (clicked != null)
                        {
                            ClearChanges();

                            //if there is an enemy at the destination, fight it
                            Player occ, cur;
                            if (clicked.OccupiedByUnit(out occ) && occ != Game.CurrentPlayer)
                            {
                                Unit[] selectedUnits = null;
                                if (Control.ModifierKeys != Keys.None && selectedTile != null && selectedTile.IsNeighbor(x, y))
                                {
                                    selectedUnits = selectedTile.FindAllUnits(unit =>
                                            (unit.Group == selectedTile.CurrentGroup && unit.Owner == Game.CurrentPlayer && unit.Movement > 0));
                                    if (selectedUnits.Length == 0)
                                        selectedUnits = null;
                                }
                                CityWar.Battle b = Game.StartBattle(clicked, selectedUnits);
                                if (b != null)
                                {
                                    changeTracker.StartBattle();
                                    Battle.StartBattle(this, b);
                                    ShowChanges(changeTracker.EndBattle());
                                }
                                else
                                    return;
                            }
                            //check that the selected tile exists
                            else if (selectedTile != null)
                            {
                                if (!selectedTile.Occupied(out cur) || cur == Game.CurrentPlayer)
                                {
                                    //check that the tiles are neighbors
                                    if (selectedTile.IsNeighbor(x, y))
                                    {
                                        //check that the destination is not occupied by an enemy
                                        if (!(clicked.OccupiedByUnit(out occ) && cur != occ) || !selectedTile.OccupiedByUnit())
                                        {
                                            //check for potential aircraft death
                                            foreach (Piece p in selectedTile.GetSelectedPieces())
                                                if (p.CanMove(clicked) && !CheckAircraft(p, 1, 1, clicked))
                                                    //if any aircraft shouldn't be moved, don't move anything
                                                    return;

                                            //check that any units were selected
                                            if (selectedTile.GetSelectedPieces().Length > 0)
                                            {
                                                saved = false;

                                                changeTracker.StartMove();
                                                //try to move the units
                                                if (Game.MovePieces(selectedTile, x, y, this.chbGroup.Checked, !this.chbGamble.Checked))
                                                {
                                                    //if any were moved, select the destination tile
                                                    selected = new Point(x, y);
                                                    selectedTile = Game.GetTile(selected.X, selected.Y);
                                                }
                                                ShowChanges(changeTracker.EndMove());

                                                //check if any selected units have move remaining
                                                bool nomves = true;
                                                foreach (Piece p in selectedTile.GetSelectedPieces())
                                                    if (p.Movement > 0)
                                                    {
                                                        nomves = false;
                                                        break;
                                                    }

                                                //if not, go to the next unit
                                                if (nomves)
                                                    btnNext_Click(null, null);
                                            }
                                            else
                                                return;
                                        }
                                        else
                                            return;
                                    }
                                    else
                                    {
                                        var selectedPieces = selectedTile.GetSelectedPieces();
                                        if (selectedPieces.Any())
                                            Unit.PathFind(selectedPieces, clicked);
                                        else
                                            return;
                                    }
                                }
                                else
                                    return;
                            }
                            else
                                return;
                        }
                        else
                            return;
                    }
                    else
                        return;

                    //if anything happened, refresh stuff
                    RefreshResources();
                    RefreshButtons();
                    RefreshCurrentPlayer();
                    CenterUnit();
                    this.Invalidate(invalidateRectangle, false);
                }
            }
        }

        private void CheckPath()
        {
            this.Invalidate();
            if (selected.X != -1 && selected.Y != -1)
            {
                Tile selectedTile = Game.GetTile(selected.X, selected.Y);
                var paths = selectedTile.GetSelectedPieces().Where(piece => piece.Path != null);
                if (paths.Any())
                {
                    foreach (var group in paths.GroupBy(piece => piece.Path[piece.Path.Count - 1]))
                        Unit.PathFind(group, group.Key);
                }
            }
        }

        private void lblResource_Click(object sender, EventArgs e)
        {
            new Trade(Game.CurrentPlayer).ShowDialog();
            RefreshResources();
        }

        #endregion

        #region Key Events

        private void MainMap_KeyUp(object sender, KeyEventArgs e)
        {
            //F11 to toggle fullscreen
            if (e.KeyData == Keys.F11)
            {
                //use the border style to determine current state
                if (this.FormBorderStyle == FormBorderStyle.None)
                {
                    //set to windowed
                    storeOld = false;
                    this.FormBorderStyle = FormBorderStyle.Sizable;
                    this.Bounds = oldBounds;
                    storeOld = true;
                }
                else
                {
                    //set to full screen
                    storeOld = false;
                    this.FormBorderStyle = FormBorderStyle.None;
                    this.Bounds = Screen.FromControl(this).Bounds;
                    storeOld = true;
                }
                screenBounds = Screen.FromControl(this);
            }
        }

        private static Screen screenBounds = null;
        public static Rectangle GetScreenBounds()
        {
            if (screenBounds == null)
                screenBounds = Screen.PrimaryScreen;
            return screenBounds.Bounds;
        }

        private void MainMap_KeyPress(object sender, KeyPressEventArgs e)
        {
            //z zooms in, x zooms out
            if (e.KeyChar == 'z' || e.KeyChar == 'Z')
                new Zoom(this, Zoom, true, 0f).Show();
            else if (e.KeyChar == 'x' || e.KeyChar == 'X')
                new Zoom(this, Zoom, false, 0f).Show();
        }

        private void MainMap_MouseWheel(object sender, MouseEventArgs e)
        {
            if (ModifierKeys == Keys.Shift)
                new Zoom(this, Zoom, null, e.Delta).Show();
            else
                this.panelPieces.PiecesPanel_MouseWheel(sender, e);
        }

        #endregion

        #region Scrolling

        private void timerGraphics_Tick(object sender, EventArgs e)
        {
            //check if the map is big enough to scroll at all on the x-axis
            if (xAxis)
            {
                //check if scrolling left or right
                if (left)
                {
                    offX -= scrollSpeed;
                    //check limit
                    if (offX < 0)
                    {
                        left = false;
                        offX = 0;
                    }
                }
                else if (right)
                {
                    offX += scrollSpeed;
                    //check limit
                    if (offX > topX)
                    {
                        right = false;
                        offX = topX;
                    }
                }
            }
            else
            {
                left = false;
                right = false;
                offX = 0;
            }

            //check if the map is big enough to scroll at all on the y-axis
            if (yAxis)
            {
                //check if scrolling up or down
                if (up)
                {
                    offY -= scrollSpeed;
                    //check limit
                    if (offY < 0)
                    {
                        up = false;
                        offY = 0;
                    }
                }
                else if (down)
                {
                    offY += scrollSpeed;
                    //check limit
                    if (offY > topY)
                    {
                        down = false;
                        offY = topY;
                    }
                }
            }
            else
            {
                up = false;
                down = false;
                offY = 0;
            }

            //check if the timer should still be enabled
            this.timerGraphics.Enabled = (left || right || up || down);

            //repaint the map
            this.Invalidate(invalidateRectangle, false);
        }

        #endregion

        #region Other Events

        private void MainMap_LocationChanged(object sender, EventArgs e)
        {
            setOldBounds();
        }

        private void MainMap_SizeChanged(object sender, EventArgs e)
        {
            setOldBounds();
            //get the portion of the form that is the map rectangle
            invalidateRectangle = new Rectangle(0, 0, this.ClientSize.Width - panelWidth, this.ClientSize.Height);
            RefreshZoom();
        }

        //scroll up when the mouse is over resource labels
        private void lblResource_MouseEnter(object sender, EventArgs e)
        {
            up = true;
            this.timerGraphics.Enabled = true;
        }

        private void lblResource_MouseLeave(object sender, EventArgs e)
        {
            up = false;
            this.timerGraphics.Enabled = (left || right || down);
        }

        private void MainMap_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!canClose)
            {
                e.Cancel = true;
                btnQuit_Click(sender, e);
            }
        }

        #endregion

        #region Button Events

        private void btnBuild_Click(object sender, EventArgs e)
        {
            this.timerGraphics.Enabled = false;

            //get all pieces that can build
            List<Capturable> capts = new List<Capturable>();
            foreach (Piece p in Game.GetTile(selected.X, selected.Y).GetSelectedPieces())
                if (p.Owner != Game.CurrentPlayer)
                    return;
                else if (p is Capturable)
                    capts.Add((Capturable)p);

            saved = false;

            if (capts.Count < 1)
            {
                //the button shouldnt be visible...
                throw new Exception();
            }

            //show the build form
            new Build(game, capts, PointToScreen(this.ClientRectangle.Location), this.ClientSize).ShowDialog();

            RefreshButtons();
            RefreshResources();
            Invalidate(invalidateRectangle);
            panelPieces.Invalidate();
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            this.timerGraphics.Enabled = false;

            Tile selectedTile = Game.GetTile(selected.X, selected.Y);

            ////get currently selected pieces
            //Piece[] selPieces;
            //if (selected.X != -1 && selected.Y != -1)
            //    selPieces = Game.getTile(selected.X, selected.Y).GetSelectedPieces();
            //else
            //    selPieces = new Piece[0];

            if (selectedTile != null)
            {
                //get the next piece based on the currently selected ones
                Piece u = Game.CurrentPlayer.NextPiece(selectedTile.CurrentGroup);
                if (u == null)
                {
                    //if no pieces have move left
                    selectedTile.CurrentGroup = int.MinValue;
                    CenterOnSelected();
                }
                else
                {
                    //center on the next unit
                    u.Tile.CurrentGroup = u.Group;
                    CenterOn(u.Tile);
                }
            }
        }

        private void btnHeal_Click(object sender, EventArgs e)
        {
            this.timerGraphics.Enabled = false;

            saved = false;

            //get currently selected pieces
            Piece[] selPieces;
            if (selected.X != -1 && selected.Y != -1)
                selPieces = Game.GetTile(selected.X, selected.Y).GetSelectedPieces();
            else
                selPieces = new Piece[0];

            if (selPieces.Length == 1 && selPieces[0] is Wizard
                && selPieces[0].Movement == selPieces[0].MaxMove)
            {
                //show the change terrain dialog
                new ChangeTerrain((Wizard)selPieces[0], this.PointToScreen(btnRest.Location)).ShowDialog();
                RefreshResources();
                RefreshButtons();

                if (selPieces[0].Movement == 0)
                    btnNext_Click(null, null);
                else
                {
                    this.Invalidate(invalidateRectangle, false);
                    this.panelPieces.Invalidate();
                }

                return;
            }

            //check each piece for aircraft
            bool can = true;
            foreach (Piece p in selPieces)
                if (!CheckAircraft(p, 1, 0))
                {
                    can = false;
                    break;
                }

            if (can)
            {
                Game.HealPieces(selPieces);

                bool nomoves = true;
                foreach (Piece p in selPieces)
                    if (p.Movement > 0)
                    {
                        nomoves = false;
                        break;
                    }

                RefreshButtons();
                RefreshResources();

                //if no pieces have move left, go to the next unit
                if (nomoves)
                    btnNext_Click(this, e);
                else
                    this.panelPieces.Invalidate();
            }
        }

        private void btnBuildCity_Click(object sender, EventArgs e)
        {
            this.timerGraphics.Enabled = false;

            saved = false;

            //get currently selected units with move left
            Tile tile = Game.GetTile(selected.X, selected.Y);
            int curGroup = tile.CurrentGroup;
            Unit[] availUnits = tile.FindAllUnits(unit => unit.Group == curGroup && unit.MaxMove != 0
                    && unit.Movement == unit.MaxMove && !unit.IsAir());

            //find the best unit to use
            Unit capture = availUnits[0];
            if (availUnits.Length > 1)
            {
                IEnumerable<Unit> bestUnits = availUnits.Where(unit => unit.Hits == unit.MaxHits || unit.Regen == 0);
                if (bestUnits.Any())
                {
                    bestUnits = OrderByRegen(bestUnits);
                }
                else
                {
                    Func<Unit, double>[] funcs = {
                        unit => unit.RandedCost * Math.Min(unit.MaxHits - unit.Hits, unit.MaxMove * unit.Regen) / (double)unit.MaxHits,
                        unit => unit.Regen / (double)unit.MaxRegen,
                        unit =>
                        {
                            double turns = 0, hp = unit.Hits, regenPct = unit.Regen / (double)unit.MaxRegen;
                            while (hp < unit.MaxHits)
                            {
                                double regen = regenPct * unit.MaxMove * unit.MaxRegen;
                                hp += regen;
                                turns += 1 - Math.Max(0, ( hp - unit.MaxHits ) / regen);
                                regenPct = Math.Pow(regenPct, Unit.RegenRecoverPower);
                            }
                            return turns;
                        },
                    };
                    var rankings = funcs.Select(func => OrderByRegen(availUnits, func).ToList());
                    bestUnits = OrderByRegen(availUnits, unit => rankings.Sum(list => -Math.Pow(.5, list.IndexOf(unit))));
                }
                capture = bestUnits.First();
            }

            Game.CollectTreasure(capture);

            //if no selected pieces have movement left, go to the next unit
            if (tile.GetSelectedPieces().Any(unit => unit.Movement > 0))
            {
                this.panelPieces.Invalidate();
                this.Invalidate(invalidateRectangle, false);
            }
            else
            {
                btnNext_Click(this, e);
            }

            RefreshResources();
            RefreshButtons();
        }
        private IEnumerable<Unit> OrderByRegen(IEnumerable<Unit> units, Func<Unit, double> keySelector = null)
        {
            units = Game.Random.Iterate(units);
            Func<Unit, double> secondary = (unit => unit.MaxMove * unit.WorkRegen);
            if (keySelector == null)
                return units.OrderBy(secondary);
            else
                return units.OrderBy(keySelector).ThenBy(secondary);
        }

        private void btnEndTurn_Click(object sender, EventArgs e)
        {
            this.timerGraphics.Enabled = false;

            if (Game.CurrentPlayer.HasMovesLeft())
                if (MessageBox.Show("You have not moved all of your units.\nDo you still wish to end your turn?",
                    "End Turn", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
                    return;

            //check for any dying aircraft
            foreach (Piece p in Game.CurrentPlayer.GetPieces())
                if (!CheckAircraft(p, p.Movement, 0))
                    return;

            this.chbGamble.Checked = true;
            this.chbGroup.Checked = true;

            saved = false;

            changeTracker.StartNextTurn();
            //end the turn
            Game.EndTurn();

            //check if the game is over
            if (Game.GetPlayers().Length < 1)
            {
                if (new GameOver().ShowDialog() == DialogResult.Yes)
                    this.btnQuit_Click(null, null);
                else
                    Application.Exit();
                return;
            }

            //refresh stuff
            RefreshCurrentPlayer();
            RefreshResources();

            //clear OKed aircraft
            okToMove.Clear();

            if (!ShowChanges(changeTracker.EndNextTurn()))
                //if nothing changes, center on the next player
                CenterOn(Game.CurrentPlayer.GetCenter());
        }

        private void btnInfo_Click(object sender, EventArgs e)
        {
            this.timerGraphics.Enabled = false;
            Info.showDialog();
        }

        private void btnQuit_Click(object sender, EventArgs e)
        {
            Info.clearDialog();

            this.timerGraphics.Enabled = false;

            DialogResult res = DialogResult.No;

            //check if the current game is saved
            if (!saved)
            {
                res = MessageBox.Show("Would you like to save your game?", "Quit",
                        MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                if (res == DialogResult.Yes)
                    if (!AskSave())
                        //if they said yes, but click cancel on the save dialog, don't quit
                        res = DialogResult.Cancel;
            }

            //if not cancelled, quit
            if (res == DialogResult.Yes || res == DialogResult.No)
            {
                new MainMenu().Show();
                canClose = true;
                this.Close();
            }
        }

        private void btnDisband_Click(object sender, EventArgs e)
        {
            Unit[] units = Game.GetTile(selected.X, selected.Y).GetSelectedUnits();
            double disbandAmount = 0, deathValue = 0;
            foreach (Unit unit in units)
            {
                disbandAmount += unit.GetDisbandAmount();
                deathValue += unit.InverseCost / Attack.DeathDivide;
            }
            if (MessageBox.Show(string.Format("Are you sure you want to disband the {0} selected units for {1} death? ({2})", units.Length,
                    disbandAmount.ToString("0.0"), deathValue.ToString("0.0")), "Disband", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                saved = false;

                Game.DisbandUnits(units);

                RefreshResources();
                RefreshButtons();
                RefreshCurrentPlayer();
                panelPieces.Invalidate();
                Invalidate(invalidateRectangle);
            }
        }

        private void btnGroup_Click(object sender, EventArgs e)
        {
            Game.GetTile(selected.X, selected.Y).Group();
            this.panelPieces.Invalidate();
            RefreshButtons();
        }

        private void btnUngroup_Click(object sender, EventArgs e)
        {
            Game.GetTile(selected.X, selected.Y).Ungroup();
            this.panelPieces.Invalidate();
            RefreshButtons();
        }

        private void btnUndo_Click(object sender, EventArgs e)
        {
            Tile newCenter = Game.UndoCommand();
            RefreshButtons();
            RefreshResources();

            if (newCenter == null)
            {
                Invalidate(invalidateRectangle);
                panelPieces.Invalidate();
            }
            else
            {
                CenterOn(newCenter);
            }
        }

        private void btnLog_Click(object sender, EventArgs e)
        {
            new Log().ShowDialog();
        }

        private void btnCalc_Click(object sender, EventArgs e)
        {
            Unit[] units = null;
            if (selected.X != -1 && selected.Y != -1)
                units = Game.GetTile(selected.X, selected.Y).GetAllUnits();
            Calculator.ShowForm(units);
        }

        #endregion

        #region Load and Save

        private void btnLoad_Click(object sender, EventArgs e)
        {
            Info.clearDialog();

            this.timerGraphics.Enabled = false;

            if (loadGame.ShowDialog() == DialogResult.OK)
            {
                //load the game
                LoadGame(loadGame.FileName);

                MessageBox.Show("Game Loaded.", "Load", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            this.timerGraphics.Enabled = false;

            AskSave();
        }

        private bool AskSave()
        {
            if (saveGame.ShowDialog() == DialogResult.OK)
            {
                //save the game
                SaveGame(saveGame.FileName);

                MessageBox.Show("Game Saved!", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
                saved = true;

                return true;
            }

            return false;
        }

        private void SaveGame(string file)
        {
            Game.SaveGame(file);
        }

        public void LoadGame(string file)
        {
            Game = Game.LoadGame(file);
            Game.ResetPics(this._zoom);
        }

        #endregion
    }
}
