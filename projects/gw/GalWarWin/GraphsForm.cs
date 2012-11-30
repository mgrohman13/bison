using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GalWar;

namespace GalWarWin
{
    public partial class GraphsForm : Form
    {
        private static GraphsForm form = new GraphsForm();

        private static readonly String[] abbr = new string[] { string.Empty, "k", "M", "G" };

        private Game game;

        private Label[,] labels;

        private bool checkEvents = false;
        private Dictionary<Graphs.GraphType, bool[]> checks;

        int y;

        private GraphsForm()
        {
            InitializeComponent();

            cbxType.Items.Add(Graphs.GraphType.Population);
            cbxType.Items.Add(Graphs.GraphType.Quality);
            cbxType.Items.Add(Graphs.GraphType.Armada);
            cbxType.Items.Add(Graphs.GraphType.Research);

            checks = new Dictionary<Graphs.GraphType, bool[]>();
            checks.Add(Graphs.GraphType.Population, new bool[] { true, true });
            checks.Add(Graphs.GraphType.PopulationTrans, new bool[] { false, true });
            checks.Add(Graphs.GraphType.Quality, new bool[] { true, false });
            checks.Add(Graphs.GraphType.Armada, new bool[] { true, false });
            checks.Add(Graphs.GraphType.Research, new bool[] { false, true });

            cbxType.SelectedItem = Graphs.GraphType.Population;
        }

        private void LoadData(Game game)
        {
            this.game = game;

            if (labels != null)
                for (int a = 0 ; a < labels.GetLength(0) ; ++a)
                    for (int b = 0 ; b < labels.GetLength(1) ; ++b)
                    {
                        Controls.Remove(labels[a, b]);
                        labels[a, b] = null;
                    }

            Player[] players = game.GetPlayers();

            double maxArmada = double.MinValue;
            foreach (Player player in players)
                maxArmada = Math.Max(maxArmada, player.GetArmadaStrength());
            int place = GetPlace(maxArmada);
            long div = GetDiv(place);

            Dictionary<Player, double> research = game.GetResearch();
            labels = new Label[8, players.Length];
            y = 32;
            for (int i = 0 ; i < players.Length ; ++i)
            {
                int x = 12;
                labels[0, i] = NewLabel(x, y, players[i].Name, players[i].Color);
                x += 106;
                labels[1, i] = NewLabel(x, y, players[i].GetColonies().Count.ToString(), players[i].Color);
                x += 106;
                labels[2, i] = NewLabel(x, y, players[i].GetTotalQuality().ToString(), players[i].Color);
                x += 106;
                labels[3, i] = NewLabel(x, y, players[i].GetPopulation().ToString(), players[i].Color);
                x += 106;
                labels[4, i] = NewLabel(x, y, MainForm.FormatDouble(players[i].GetPopulationGrowth()), players[i].Color);
                x += 106;
                labels[5, i] = NewLabel(x, y, MainForm.FormatDouble(players[i].GetTotalIncome()), players[i].Color);
                x += 106;
                labels[6, i] = NewLabel(x, y, GetString(GetValue(players[i].GetArmadaStrength(), false, div), div, place), players[i].Color);
                x += 106;
                labels[7, i] = NewLabel(x, y, GetResearch(research[players[i]]), players[i].Color);
                y += 26;
            }

            this.GraphsForm_SizeChanged(null, null);
        }

        internal static string GetArmadaString(double value)
        {
            double maxArmada = double.MinValue;
            foreach (Player player in MainForm.Game.GetPlayers())
                maxArmada = Math.Max(maxArmada, player.GetArmadaStrength());
            int place = GetPlace(maxArmada);
            long div = GetDiv(place);
            return GetString(GetValue(value, false, div), div, place);
        }

        private static int GetPlace(double max)
        {
            return ( MainForm.FormatInt(max).Length - 2 ) / 3;
        }

        private static long GetDiv(int place)
        {
            long retVal = 1;
            while (--place > -1)
                retVal *= 1000;
            return retVal;
        }

        private static long GetValue(double value, bool ceil, long div)
        {
            value /= div;
            return (long)( ceil ? Math.Ceiling(value) : Math.Round(value) ) * div;
        }

        private static string GetString(long value, long div, int place)
        {
            if (value == 0)
                return "0";
            value /= div;
            return value.ToString() + abbr[place];
        }

        private string GetResearch(double research)
        {
            return MainForm.FormatPctWithCheck(research / 100.0);
        }

        private Label NewLabel(int x, int y, string text, Color? backColor)
        {
            Label label = new Label();
            label.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            label.Location = new System.Drawing.Point(x, y);
            label.Size = new System.Drawing.Size(100, 23);
            label.TabIndex = 40;
            label.Text = text;
            label.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            if (backColor.HasValue)
                label.BackColor = backColor.Value;
            Controls.Add(label);
            return label;
        }

        private void groupBox1_Paint(object sender, PaintEventArgs e)
        {
            //try
            //{
            Graphs.GraphType type = (Graphs.GraphType)cbxType.SelectedItem;
            if (checkBox1.Checked)
                if (type == Graphs.GraphType.Armada)
                {
                    type = Graphs.GraphType.ArmadaDamaged;
                }
                else if (type == Graphs.GraphType.Population)
                {
                    type = Graphs.GraphType.PopulationTrans;
                    checks[type][0] = checkBox1.Checked;
                    checks[type][1] = chkSmooth.Checked;
                }
                else if (type == Graphs.GraphType.Research)
                {
                    type = Graphs.GraphType.TotalIncome;
                }

            Graphs.GraphType popType = Graphs.GraphType.Population;
            if (checks[popType][0])
                popType = Graphs.GraphType.PopulationTrans;

            float[, ,] d1 = null;
            float[, ,] d2 = null;
            Dictionary<int, Player> p1 = null;
            Dictionary<int, Player> p2 = null;
            float? maxY = null;
            if (checkBox1.Checked && type == Graphs.GraphType.Quality)
            {
                d1 = game.Graphs.Get(type, out p1);
                d2 = game.Graphs.Get(popType, out p2);
                maxY = (float)Math.Ceiling(Math.Max(GetMaxY(d1), GetMaxY(d2)));
            }

            DrawGraph(e, type, d1, p1, maxY);

            if (checkBox1.Checked && type == Graphs.GraphType.Quality)
                DrawGraph(e, popType, d2, p2, maxY);
            //}
            //catch (Exception exception)
            //{
            //}
        }

        private void DrawGraph(PaintEventArgs e, Graphs.GraphType type, float[, ,] data, Dictionary<int, Player> playerIndexes, float? maxY)
        {
            float width = cbxType.Location.X - 3 * padding;
            float height = groupBox1.ClientSize.Height - 2 * padding;

            if (data == null)
                data = game.Graphs.Get(type, out playerIndexes);
            int xLen = data.GetLength(0);
            if (xLen > 1)
            {
                Dictionary<int, Pen> pens = new Dictionary<int, Pen>();
                foreach (KeyValuePair<int, Player> player in playerIndexes)
                    pens.Add(player.Key, new Pen(player.Value.Color, 3));

                float maxX = data[xLen - 1, 0, 0];
                if (!maxY.HasValue)
                    maxY = GetMaxY(data);

                float xScale = width / (float)( Math.Ceiling(maxX) );
                float yScale = height / (float)Math.Ceiling(maxY.Value);

                if (xScale > 0 && yScale > 0)
                {
                    DrawGrid(e, height, maxX, maxY.Value, xScale, ref yScale);

                    Dictionary<int, List<PointF>> points = new Dictionary<int, List<PointF>>();
                    int yLen = data.GetLength(1);
                    for (int y = 0 ; y < yLen ; ++y)
                        points.Add(y, new List<PointF>());

                    bool smooth = false;//( checks.ContainsKey(type) ? checks[type][1] : chbSmooth.Checked );
                    for (int x = 0 ; x < xLen ; ++x)
                        for (int y = 0 ; y < yLen ; ++y)
                            AddPoint(points, y, GetPoint(data[x, y, 0], data[x, y, 1], xScale, yScale), smooth, xScale);
                    if (smooth)
                        for (int y = 0 ; y < yLen ; ++y)
                        {
                            PointF point = GetPoint(data[xLen - 1, y, 0], data[xLen - 1, y, 1], xScale, yScale);
                            SmoothPoints(points, y, GetTurn(point.X, xScale), xScale);
                            if (points[y][points[y].Count - 1].X < point.X)
                                points[y].Add(point);
                        }

                    foreach (int y in Game.Random.Iterate(yLen))
                        e.Graphics.DrawLines(pens[y], points[y].ToArray());
                }
            }
        }

        private void AddPoint(Dictionary<int, List<PointF>> points, int player, PointF point, bool smooth, float xScale)
        {
            List<PointF> List = points[player];

            if (smooth)
            {
                int lastTurn = GetTurn(point.X, xScale) - 1;
                SmoothPoints(points, player, lastTurn, xScale);
            }
            else if (List.Count > 1)
            {
                PointF l1 = List[List.Count - 1];
                PointF l2 = List[List.Count - 2];
                float slope = ( l1.Y - l2.Y ) / ( l1.X - l2.X );
                if (Math.Abs(( ( point.Y - l1.Y ) / ( point.X - l1.X ) ) - slope) < 0.001)
                    List.RemoveAt(List.Count - 1);
            }

            points[player].Add(point);
        }

        private void SmoothPoints(Dictionary<int, List<PointF>> points, int player, int turn, float xScale)
        {
            List<PointF> List = points[player];

            int count = 0;
            float sumX = 0, sumY = 0;
            for (int i = List.Count ; --i >= 0 ; )
            {
                if (GetTurn(List[i].X, xScale) == turn)
                {
                    ++count;
                    sumX += List[i].X;
                    sumY += List[i].Y;
                }
                else
                {
                    break;
                }
            }

            if (count > 0)
            {
                for (int i = 0 ; i < count && List.Count > 1 ; ++i)
                    List.RemoveAt(List.Count - 1);
                AddPoint(points, player, new PointF(sumX / count, sumY / count), false, xScale);
            }
        }

        const float padding = 21;
        private int GetTurn(float x, float xScale)
        {
            return (int)( ( x - 2 * padding ) / xScale );
        }

        private float GetMaxY(float[, ,] data)
        {
            float maxY = 0;
            for (int x = 0 ; x < data.GetLength(0) ; ++x)
                for (int y = 0 ; y < data.GetLength(1) ; ++y)
                    maxY = Math.Max(maxY, data[x, y, 1]);
            return (float)Math.Floor(maxY) + 1f;
        }

        private void DrawGrid(PaintEventArgs e, float height, float maxX, float maxY, float xScale, ref float yScale)
        {
            int place = GetPlace(maxY);
            long value;

            DrawYLine(e, height, maxX, xScale, ref yScale, ref maxY, place, true, out value);

            DrawYLine(e, maxX, xScale, yScale, 0, place);
            DrawYLine(e, maxX, xScale, yScale, maxY * 1 / 4f, place);
            DrawYLine(e, maxX, xScale, yScale, maxY * 2 / 4f, place);
            DrawYLine(e, maxX, xScale, yScale, maxY * 3 / 4f, place);

            DrawXLine(e, value, xScale, yScale, 0);
            DrawXLine(e, value, xScale, yScale, (int)Math.Round(maxX * 1 / 4f));
            DrawXLine(e, value, xScale, yScale, (int)Math.Round(maxX * 2 / 4f));
            DrawXLine(e, value, xScale, yScale, (int)Math.Round(maxX * 3 / 4f));
            DrawXLine(e, value, xScale, yScale, (int)Math.Ceiling(maxX));
        }

        private void DrawXLine(PaintEventArgs e, long maxY, float xScale, float yScale, int x)
        {
            PointF startPoint = GetPoint(x, 0, xScale, yScale);
            DrawString(e, x.ToString(), startPoint, true);
            e.Graphics.DrawLine(Pens.Black, startPoint, GetPoint(x, maxY, xScale, yScale));
        }

        private void DrawYLine(PaintEventArgs e, float maxX, float xScale, float yScale, float y, int place)
        {
            long value;
            DrawYLine(e, -1, maxX, xScale, ref yScale, ref y, place, false, out value);
        }

        private void DrawYLine(PaintEventArgs e, float height, float maxX, float xScale, ref float yScale, ref float y, int place, bool ceil, out long value)
        {
            long div = GetDiv(place);
            value = GetValue(y, ceil, div);
            if (height > 0)
            {
                y = (float)( value - Consts.FLOAT_ERROR );
                yScale = height / (float)Math.Ceiling(y);
            }

            PointF startPoint = GetPoint(0, value, xScale, yScale);
            DrawString(e, GetString(value, div, place), startPoint, false);
            e.Graphics.DrawLine(Pens.Black, startPoint, GetPoint((float)Math.Ceiling(maxX), value, xScale, yScale));
        }

        private void DrawString(PaintEventArgs e, string text, PointF point, bool xLine)
        {
            const float padding = 6;
            Font font = checkBox1.Font;
            SizeF textSize = e.Graphics.MeasureString(text, font);
            float x = point.X;
            float y = point.Y;
            if (xLine)
            {
                x -= textSize.Width / 2f;
                y += padding;
            }
            else
            {
                x -= textSize.Width + padding;
                y -= textSize.Height / 2f;
            }
            e.Graphics.DrawString(text, font, Brushes.Black, x, y);
        }

        private PointF GetPoint(float x, float y, float xScale, float yScale)
        {
            return new PointF(2 * padding + x * xScale, -padding + groupBox1.ClientSize.Height - y * yScale);
        }

        private void cbxType_SelectedIndexChanged(object sender, EventArgs e)
        {
            checkEvents = false;

            Graphs.GraphType type = (Graphs.GraphType)cbxType.SelectedItem;
            switch (type)
            {
            case Graphs.GraphType.Armada:
                checkBox1.Text = "Account For Damage";
                break;
            case Graphs.GraphType.Population:
                checkBox1.Text = "Include Transports";
                break;
            case Graphs.GraphType.Quality:
                checkBox1.Text = "Overlay Population";
                break;
            case Graphs.GraphType.Research:
                checkBox1.Text = "Total Income";
                break;
            }

            checkBox1.Checked = checks[type][0];
            chkSmooth.Checked = checks[type][1];

            RefreshGraph();
            checkEvents = true;
        }

        private void checkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (checkEvents)
            {
                Graphs.GraphType type = (Graphs.GraphType)cbxType.SelectedItem;

                checks[type] = new bool[] { checkBox1.Checked, chkSmooth.Checked };

                RefreshGraph();
            }
        }

        public static void ShowDialog(MainForm gameForm, Game game)
        {
            form.Location = gameForm.Location;
            form.Size = gameForm.Size;

            form.LoadData(game);
            form.ShowDialog();

            gameForm.Location = form.Location;
            gameForm.Size = form.Size;
        }

        private void GraphsForm_SizeChanged(object sender, EventArgs e)
        {
            groupBox1.Height = this.ClientSize.Height - y - 13;
            groupBox1.Location = new Point(groupBox1.Location.X, y + 13);
            RefreshGraph();
        }

        private void RefreshGraph()
        {
            groupBox1.Invalidate(MainForm.GetInvalidateRectangle(groupBox1.ClientRectangle, cbxType.Location.X));
        }
    }
}
