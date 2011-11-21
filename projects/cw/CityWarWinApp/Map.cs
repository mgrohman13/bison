using System;
using System.Collections.Generic;
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

        //constants 
        public const int panelWidth = 255;
        public const int startZoom = 150;

        //miscellaneous
        static List<Piece> okToMove = new List<Piece>();
        Point selected = new Point(-1, -1);
        Rectangle oldBounds, invalidateRectangle;
        UnitInfo InfoForm;
        bool shift = false, ctrl = false, alt = false, storeOld = true, saved = true, canClose = false;
        int lastGroup = -1;

        //scrolling
        bool xAxis, yAxis, up = false, down = false, left = false, right = false;
        float offX = 0, offY = 0, scrollSpeed;

        //zooming - set the starting zoom
        float _zoom = Game.Random.GaussianCapped(startZoom, .01f, 30f), topX, topY, side, middle;
        Font tileInfoFont;
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
            this.panelPieces.Initialize(GetPiecesForPiecesPanel, GetDrawFlagsForPiece, GetTextForPiece,
                delegate()
                {
                    return Brushes.DarkMagenta;
                });
            this.MouseWheel += new MouseEventHandler(this.panelPieces.PiecesPanel_MouseWheel);
            this.ResizeRedraw = false;

            //reduce flickering
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);

            //the rectangle to draw the map
            invalidateRectangle = new Rectangle(0, 0, this.ClientSize.Width - panelWidth, this.ClientSize.Height);
        }

        private void MainMap_Load(object sender, EventArgs e)
        {
            //set to full screen
            setOldBounds();
            storeOld = false;
            this.Bounds = Screen.PrimaryScreen.Bounds;
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
        public static bool CheckAircraft(Piece piece, int moveMod)
        {
            return CheckAircraft(piece, moveMod, piece.Tile);
        }
        static bool CheckAircraft(Piece piece, int moveMod, Tile toTile)
        {
            //not enough movement, so this is immaterial
            if (piece.Movement < 1)
                return true;

            return CheckAircraft(piece, moveMod, toTile, null);
        }
        static bool CheckAircraft(Piece piece, int moveMod, Tile toTile, Dictionary<Tile, Tile> CanGetCarrier)
        {
            //cant move if it isn't your turn
            if (piece.Owner != Game.CurrentPlayer)
                return true;

            Unit unit = piece as Unit;
            if (piece.Abilty == Abilities.AircraftCarrier)
            {
                //store variables for the Tile.CheckAircraft instance method
                Tile.MovedCarrier = piece;
                Tile.MovedToTile = toTile;
                Tile.MoveModifier = moveMod;

                //if this piece is a carrier, check each aircraft the player owns
                foreach (Piece aircraft in piece.Owner.GetPieces())
                    if (aircraft.Abilty == Abilities.Aircraft)
                        //call recursively for the aircraft
                        if (!CheckAircraft(aircraft, 0, aircraft.Tile, CanGetCarrier))
                        {
                            Tile.MovedCarrier = null;
                            Tile.MovedToTile = null;
                            Tile.MoveModifier = 0;

                            //make sure that the carrier could have reached the aircraft were it not to move
                            //do this by simply checking again with the static variables cleared
                            if (CheckAircraft(aircraft, 0, toTile, CanGetCarrier))
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
            else if (piece.Abilty == Abilities.Aircraft && !( unit != null && !unit.IsRanded )
                && !okToMove.Contains(piece) && toTile.CheckAircraft(moveMod, piece, ref CanGetCarrier))
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

        //this method speaks for itself...
        private void RefreshResources()
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
            this.lblRelic.Text = currentPlayer.Relic.ToString();
            this.lblPpl.Text = currentPlayer.Population.ToString();
        }

        private void RefreshButtons()
        {
            if (selected.X == -1 && selected.Y == -1)
            {
                btnBuild.Enabled = false;
                btnBuildCity.Enabled = false;
                return;
            }

            //get the selected tile and its pieces
            Tile selTile = Game.GetTile(selected.X, selected.Y);
            Piece[] Selected = selTile.GetSelectedPieces();

            //check if the only selected piece is a wizard, in which case you can change the terrain
            if (Selected.Length == 1 && Selected[0] is Wizard && Selected[0].Movement == Selected[0].MaxMove)
                btnHeal.Text = "Map";
            else
                btnHeal.Text = "Rest";

            //if any of the pieces are a Capturable, show the build button
            bool any = false;
            foreach (Piece p in Selected)
            {
                if (p is Capturable)
                {
                    any = true;
                    break;
                }
            }
            btnBuild.Enabled = any;

            //check if the tile can build a city and if any pieces have full move
            btnBuildCity.Enabled = false;
            if (selTile.CityTime != -1 && !selTile.MadeCity)
            {
                foreach (Piece p in Selected)
                    if (p.MaxMove != 0 && p.Movement == p.MaxMove)
                    {
                        //if so, show the build button
                        btnBuildCity.Enabled = true;
                        break;
                    }
            }

            btnUndo.Enabled = Game.CanUndoCommand();
        }

        private void RefreshZoom()
        {
            //the ratio of a hexagon's side length to its cross length, divided by four
            const float mult = 0.288675f;

            //change the scroll speed based on the zoom
            scrollSpeed = (float)( Math.Sqrt(Zoom * startZoom) / 2.6f );

            //calculate side and cross length of the hexes
            side = Zoom * mult;
            middle = Zoom / 4f;

            //see if there is enough room to scroll each axis
            xAxis = ( ( Zoom * Game.Width ) > this.ClientSize.Width - panelWidth );
            yAxis = ( ( side * 3 * Game.Height ) > this.ClientSize.Height );

            //get the maximum values for offX and offY
            if (xAxis)
                topX = ( (float)Game.Width + .5f ) * (float)Zoom - ( (float)ClientSize.Width - panelWidth - 24f );
            if (yAxis)
                topY = ( (float)Game.Height + 1f / 3f ) * ( (float)Zoom * 3f * mult ) - (float)ClientSize.Height + 26f;

            //get the font for city and wizard numbers on a tile
            tileInfoFont = new Font(new FontFamily("Arial"), Zoom / 9f, FontStyle.Bold);

            //reset the pics so they can be the proper size
            Player.ResetPics(_zoom);

            //center on the selected tile
            CenterOnSelected();
        }

        private void CenterOnSelected()
        {
            if (selected.X > -1 && selected.X < Game.Width && selected.Y > -1 && selected.Y < Game.Height)
                CenterOn(Game.GetTile(selected.X, selected.Y));
            else
                CenterOn(Game.CurrentPlayer.GetCenter());
        }

        private void CenterOn(Tile tile)
        {
            //the x and y to center on
            selected = new Point(tile.x, tile.y);

            //get the x and y distances to use
            float y = (float)ClientSize.Height / 2f, xx = ( (float)( ClientSize.Width - panelWidth ) ) / 2f;

            //set the offY
            if (yAxis)
                offY = ( side * ( 3f * (float)tile.y + 2f ) - y + 13f );

            //set the offX
            if (xAxis)
                if (tile.y % 2 == 0)
                    offX = (float)tile.x * Zoom + Zoom - xx + 13f;
                else
                    offX = ( 2f * (float)tile.x * Zoom + Zoom - 2f * ( xx + 13f ) ) / 2f;

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
            Tile selectedTile = Game.GetTile(selected.X, selected.Y);

            //select any pieces with movement left
            selectedTile.Select();

            panelPieces.ScrollToSelected();

            //int index = selectedTile.GetIndexOfSelected();
            //if (index != -1)
            //{
            //    //if any units are selected, scroll appropriately
            //    int val = (126 * index + 139 - this.sbUnits.Height) / unitScroll + 1;
            //    if (val < sbUnits.Minimum)
            //        val = sbUnits.Minimum;
            //    if (val > sbUnits.Maximum)
            //        val = sbUnits.Maximum;
            //    this.sbUnits.Value = val;
            //}

            RefreshButtons();
            //panelPieces.Invalidate();
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
                        selectedTile.ClickOn(clicked, shift, ctrl, alt);
                        RefreshButtons();
                        panelPieces.Invalidate();
                    }
                }
            }
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

        private string GetTextForPiece(Piece piece)
        {
            return string.Format("{0} / {1}", piece.Movement, piece.MaxMove);
        }

        #endregion

        #region Draw Map

        //double total = 0, amt = 0;

        protected override void OnPaint(PaintEventArgs e)
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
                side2_3 = side * 2.3f,
                zoom_9 = _zoom / 9f,
                zoom_3 = _zoom / 3f,
                zoom_4_5mid_2 = _zoom / 4.5f + mid_2,
                zoom_9zoom_6_m = zoom_9 + _zoom / 6f - .0666f * _zoom,
                _zoom_9side2_3 = -zoom_9 + side2_3,
                _zoom_9side2_3_p = _zoom_9side2_3 + 3f,
                zoom_4_5mid_2zoom_3 = zoom_4_5mid_2 + zoom_3,
                zoom_9mid_2 = zoom_9 + mid_2,
                zoom_9mid_2zoom_3 = zoom_9mid_2 + zoom_3;

            //get the range of hexes that need to be drawn
            int minX = (int)( offX / _zoom ) - 2, maxX = (int)( ( offX + (float)( ClientSize.Width - panelWidth ) ) / _zoom ) + 1;
            int minY = (int)( offY / ( side3 ) ) - 2, maxY = (int)( ( offY + ClientSize.Height ) / ( side3 ) ) + 1;
            if (minX < -1)
                minX = -1;
            if (minY < -1)
                minY = -1;
            if (maxX > Game.Width)
                maxX = Game.Width;
            if (maxY > Game.Height)
                maxY = Game.Height;

            //draw the hexes
            for (int X = minX ; ++X < maxX ; )
                for (int Y = minY ; ++Y < maxY ; )
                {
                    //get the current tile being drawn
                    Tile thisTile = Game.GetTile(X, Y);

                    //get the proper color
                    Brush theBrush;
                    switch (thisTile.Terrain)
                    {
                    case Terrain.Forest:
                        theBrush = Brushes.Green;
                        break;
                    case Terrain.Mountain:
                        theBrush = Brushes.Gold;
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
                    float xVal = (float)X * mid4 - OffX + ( Y % 2 == 0 ? mid2 : 0f );
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
                    int wp = thisTile.WizardPoints;
                    if (wp > 0)
                    {
                        e.Graphics.FillEllipse(Brushes.DeepPink, xVal + zoom_9mid_2,
                            yVal + _zoom_9side2_3_p, zoom_9, zoom_9);
                        e.Graphics.DrawString(wp.ToString(), tileInfoFont, Brushes.Black,
                            xVal + zoom_4_5mid_2, yVal + _zoom_9side2_3);
                    }
                    int cp = thisTile.CityTime;
                    if (cp > -1)
                    {
                        e.Graphics.FillEllipse(Brushes.DarkRed, xVal + zoom_9mid_2zoom_3,
                            yVal + _zoom_9side2_3_p, zoom_9, zoom_9);
                        e.Graphics.DrawString(cp.ToString(), tileInfoFont, Brushes.Black,
                            xVal + zoom_4_5mid_2zoom_3, yVal + _zoom_9side2_3);
                    }

                    //draw the unit picture
                    Image pic = thisTile.GetPieceImage();
                    if (pic != null)
                        e.Graphics.DrawImage(pic, xVal + zoom_9, yVal - zoom_9zoom_6_m);
                }

            //show the selected tile
            if (selected.X > minX && selected.X < maxX && selected.Y > minY && selected.Y < maxY)
            {
                float XVal = (float)selected.X * mid4 - OffX + ( selected.Y % 2 == 0 ? mid2 : 0f );
                float YVal = (float)selected.Y * side3 - OffY;
                PointF[] points = new PointF[6];
                points[0] = new PointF(XVal, YVal);
                points[1] = new PointF(XVal + mid2, YVal - side);
                points[2] = new PointF(XVal + mid4, YVal);
                points[3] = new PointF(XVal + mid4, YVal + side2);
                points[4] = new PointF(XVal + mid2, YVal + side3);
                points[5] = new PointF(XVal, YVal + side2);
                e.Graphics.DrawPolygon(new Pen(Color.Black, 3f), points);
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
            timerGraphics.Enabled = ( up || down || left || right );

            //get the x and y coordinates of the hex over which the mouse resides
            int y = (int)Math.Round(( (float)e.Y + offY - 13f + side ) / side / 3f) - 1;
            int x = ( y % 2 == 0 ? (int)Math.Round(( (float)e.X + offX - 13f ) / Zoom) - 1
                : (int)Math.Round(( (float)e.X + offX - 13f + Zoom / 2f ) / Zoom) - 1 );

            //show the x and y of the mouse, if it's within range
            if (x >= 0 && x < Game.Width && y >= 0 && y < Game.Height)
                this.lblMouse.Text = string.Format("({0},{1})", x, y);
            else
                this.lblMouse.Text = "";
        }

        private void MainMap_MouseLeave(object sender, EventArgs e)
        {
            //stop scrolling if the mouse leaves the window
            timerGraphics.Enabled = false;
        }

        private void MainMap_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.X < ClientSize.Width - panelWidth)
            {
                int y = (int)Math.Round(( (float)e.Y + offY - 13f + side ) / side / 3f) - 1;
                int x = ( y % 2 == 0 ? (int)Math.Round(( (float)e.X + offX - 13f ) / Zoom) - 1
                    : (int)Math.Round(( (float)e.X + offX - 13f + Zoom / 2f ) / Zoom) - 1 );

                if (x >= 0 && y >= 0 && x < Game.Width && y < Game.Height)
                {
                    //select the tile
                    if (e.Button == MouseButtons.Left)
                    {
                        selected = new Point(x, y);
                    }
                    //right mouse button to move units
                    else if (e.Button == MouseButtons.Right)
                    {
                        //get the selected tile (if it exists) and the clicked tile
                        Tile selectedTile = null;
                        if (selected.X != -1 && selected.Y != -1)
                            selectedTile = Game.GetTile(selected.X, selected.Y);
                        Tile clicked = Game.GetTile(x, y);

                        //check that the selected tile exists
                        if (selected.X != -1 && selected.Y != -1)
                        {
                            Player occ, cur;
                            if (!selectedTile.Occupied(out cur) || cur == Game.CurrentPlayer)
                            {
                                //check that the tiles are neighbors
                                if (selectedTile.IsNeighbor(x, y))
                                {
                                    //check that the destination is not occupied by an enemy
                                    if (!( clicked.OccupiedByUnit(out occ) && cur != occ ) || !selectedTile.OccupiedByUnit())
                                    {
                                        //check for potential aircraft death
                                        foreach (Piece p in selectedTile.GetSelectedPieces())
                                            if (p.CanMove(clicked) && !CheckAircraft(p, 1, clicked))
                                                //if any aircraft shouldn't be moved, don't move anything
                                                return;

                                        //check that any units were selected
                                        if (selectedTile.GetSelectedPieces().Length > 0)
                                        {
                                            saved = false;

                                            //try to move the units
                                            if (Game.MovePieces(selectedTile, x, y, this.chbGroup.Checked))
                                            {
                                                //if any were moved, select the destination tile
                                                selected = new Point(x, y);
                                                selectedTile = Game.GetTile(selected.X, selected.Y);
                                            }

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
                                    else if (selectedTile.GetSelectedUnits().Length > 0)
                                    {
                                        //if there is an enemy at the destination, fight it
                                        new Battle(Game.StartBattle(selectedTile, clicked)).ShowDialog();
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

        private void lblResource_Click(object sender, EventArgs e)
        {
            new Trade(Game.CurrentPlayer).ShowDialog();
            RefreshResources();
        }

        #endregion

        #region Key Events

        private void MainMap_KeyDown(object sender, KeyEventArgs e)
        {
            //always keep track of shift, alt, and control
            shift = e.Shift;
            ctrl = e.Control;
            alt = e.Alt;
        }

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
                    this.Bounds = Screen.PrimaryScreen.Bounds;
                    storeOld = true;
                }
            }

            //always keep track of shift, alt, and control
            shift = e.Shift;
            ctrl = e.Control;
            alt = e.Alt;
        }

        private void MainMap_KeyPress(object sender, KeyPressEventArgs e)
        {
            //z zooms in, x zooms out
            if (e.KeyChar == 'z' || e.KeyChar == 'Z')
                new Zoom(this, Zoom, true).Show();
            else if (e.KeyChar == 'x' || e.KeyChar == 'X')
                new Zoom(this, Zoom, false).Show();
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
            this.timerGraphics.Enabled = ( left || right || up || down );

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
            this.timerGraphics.Enabled = ( left || right || down );
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
            new Build(capts, PointToScreen(this.ClientRectangle.Location), this.ClientSize).ShowDialog();

            RefreshButtons();
            RefreshResources();
            Invalidate(invalidateRectangle);
            panelPieces.Invalidate();
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            this.timerGraphics.Enabled = false;

            ////get currently selected pieces
            //Piece[] selPieces;
            //if (selected.X != -1 && selected.Y != -1)
            //    selPieces = Game.getTile(selected.X, selected.Y).GetSelectedPieces();
            //else
            //    selPieces = new Piece[0];

            //get the next piece based on the currently selected ones
            Piece u = Game.CurrentPlayer.NextPiece(Game.GetTile(selected.X, selected.Y).CurrentGroup);
            if (u == null)
            {
                //if no pieces have move left
                Game.GetTile(selected.X, selected.Y).CurrentGroup = int.MinValue;
                CenterOnSelected();
            }
            else
            {
                //center on the next unit
                u.Tile.CurrentGroup = u.Group;
                CenterOn(u.Tile);
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
                new ChangeTerrain((Wizard)selPieces[0], this.PointToScreen(btnHeal.Location)).ShowDialog();
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
                if (!CheckAircraft(p, 1))
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

            //get currently selected pieces
            Tile tile = Game.GetTile(selected.X, selected.Y);
            int curGroup = tile.CurrentGroup;
            Piece[] selPieces = tile.FindAllPieces(delegate(Piece p)
            {
                return ( p.Group == curGroup && p.MaxMove != 0 && p.Movement == p.MaxMove );
            });

            //find the best piece to use
            Piece piece = null;
            int minWork = int.MaxValue;
            foreach (Piece p in selPieces)
            {
                const int captWork = 1000000000;
                const int injdWork = 100000000;
                Unit unit = p as Unit;
                if (unit == null)
                {
                    if (captWork < minWork)
                    {
                        piece = p;
                        minWork = captWork;
                    }
                }
                else
                {
                    int work = unit.Regen * unit.MaxMove;
                    if (unit.Hits < unit.maxHits)
                    {
                        if (injdWork < minWork)
                            work += injdWork + (int)Math.Round(injdWork * ( 1 - unit.Hits / (double)unit.maxHits ));
                        else
                            continue;
                    }

                    if (work < minWork)
                    {
                        piece = p;
                        minWork = work;
                    }
                }
            }

            //check for a piece with move left
            if (piece.MaxMove != 0 && piece.Movement == piece.MaxMove)
            {
                //build the city
                Game.ConvertCity(piece);

                //check if any selected units have move left
                bool nomoves = true;
                foreach (Piece pp in selPieces)
                    if (pp.Movement > 0)
                    {
                        nomoves = false;
                        break;
                    }
                if (nomoves)
                {
                    //if no one has movement left, go to the next unit
                    btnNext_Click(this, e);
                }
                else
                {
                    this.panelPieces.Invalidate();
                    this.Invalidate(invalidateRectangle, false);
                }

                RefreshResources();
                RefreshButtons();

                return;
            }

            //button shouldn't have been visible in the first place...
            throw new Exception();
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
                if (!CheckAircraft(p, int.MaxValue))
                    return;

            saved = false;

            //end the tuen
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

            //center on the next player's center
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
            double deathValue = 0;
            foreach (Unit unit in units)
                deathValue += (int)Math.Ceiling(unit.GetDisbandAmount());
            if (MessageBox.Show(string.Format("Are you sure you want to disband the {0} selected units for {1} death?", units.Length,
                deathValue.ToString("0")), "Disband", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
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

        //private void SaveGame(string file)
        //{
        //    //open a new file stream and binary writer
        //    FileStream fs = File.Open(file, FileMode.OpenOrCreate);
        //    MyWriter bw = new MyWriter(fs);

        //    //save game data
        //    Game.SaveGame(bw);

        //    //float
        //    bw.Write(offX);
        //    bw.Write(offY);
        //    bw.Write(scrollSpeed);
        //    bw.Write(Zoom);
        //    bw.Write(topX);
        //    bw.Write(topY);
        //    bw.Write(side);
        //    bw.Write(middle);

        //    //point
        //    bw.Write(selected.X);
        //    bw.Write(selected.Y);

        //    //rectangle
        //    bw.Write(oldBounds.X);
        //    bw.Write(oldBounds.Y);
        //    bw.Write(oldBounds.Width);
        //    bw.Write(oldBounds.Height);

        //    bw.Close();
        //    fs.Close();
        //}

        //  void LoadGame(string file)
        //{
        //    FileStream fs = File.OpenRead(file);
        //    MyReader br = new MyReader(fs);

        //    Game.LoadGame(br);

        //    offX = br.ReadSingle();
        //    offY = br.ReadSingle();
        //    scrollSpeed = br.ReadSingle();
        //    Zoom = br.ReadSingle();
        //    topX = br.ReadSingle();
        //    topY = br.ReadSingle();
        //    side = br.ReadSingle();
        //    middle = br.ReadSingle();

        //    selected = new Point(br.ReadInt32(), br.ReadInt32());

        //    oldBounds = new Rectangle(br.ReadInt32(), br.ReadInt32(), br.ReadInt32(), br.ReadInt32());

        //    //clear the OKed aircraft
        //    okToMove = new List<Piece>();

        //    saved = true;

        //    br.Close();
        //    fs.Close();

        //    //refresh stuff after loading
        //    this.lblTurn.Text = Game.GetTurn().ToString();
        //    this.lblPlayer.Text = Game.getCurrentPlayer().Name;
        //    this.lblPlayer.BackColor = Game.getCurrentPlayer().color;
        //    RefreshResources();
        //    RefreshButtons();
        //    RefreshZoom();

        //    Player.CreatePics(true);

        //    //center on the selected tile, if it exists
        //    if (selected.X == -1 && selected.Y == -1)
        //    {
        //        this.Invalidate(invalidateRectangle, false);
        //        this.panelUnits.Invalidate();
        //    }
        //    else
        //        CenterOn(Game.getTile(selected.X, selected.Y));
        //}

        private void SaveGame(string file)
        {
            //open a new file stream and binary writer
            FileStream fs = File.Open(file, FileMode.OpenOrCreate);
            BinaryWriter bw = new BinaryWriter(fs);

            //save game data
            Game.SaveGame(bw);

            //float
            bw.Write(offX);
            bw.Write(offY);
            bw.Write(scrollSpeed);
            bw.Write(Zoom);
            bw.Write(topX);
            bw.Write(topY);
            bw.Write(side);
            bw.Write(middle);

            //point
            bw.Write(selected.X);
            bw.Write(selected.Y);

            //rectangle
            bw.Write(oldBounds.X);
            bw.Write(oldBounds.Y);
            bw.Write(oldBounds.Width);
            bw.Write(oldBounds.Height);

            bw.Close();
            fs.Close();
        }

        public void LoadGame(string file)
        {
            FileStream fs = File.OpenRead(file);
            BinaryReader br = new BinaryReader(fs);

            Game.LoadGame(br);

            offX = br.ReadSingle();
            offY = br.ReadSingle();
            scrollSpeed = br.ReadSingle();
            Zoom = br.ReadSingle();
            topX = br.ReadSingle();
            topY = br.ReadSingle();
            side = br.ReadSingle();
            middle = br.ReadSingle();

            selected = new Point(br.ReadInt32(), br.ReadInt32());

            oldBounds = new Rectangle(br.ReadInt32(), br.ReadInt32(), br.ReadInt32(), br.ReadInt32());

            //clear the OKed aircraft
            okToMove.Clear();

            saved = true;

            br.Close();
            fs.Close();

            //refresh stuff after loading
            RefreshCurrentPlayer();
            RefreshResources();
            RefreshButtons();
            RefreshZoom();

            //center on the selected tile, if it exists
            if (selected.X == -1 && selected.Y == -1)
            {
                this.Invalidate(invalidateRectangle, false);
                this.panelPieces.Invalidate();
            }
            else
                CenterOnSelected();
        }

        #endregion
    }
}
