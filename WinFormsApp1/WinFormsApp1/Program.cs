using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using ClassLibrary1;
using ClassLibrary1.Pieces;
using ClassLibrary1.Pieces.Enemies;
using ClassLibrary1.Pieces.Players;

namespace WinFormsApp1
{
    static class Program
    {
        public static Game Game;

        public static Main Form;
        public static DgvForm DgvForm;

        private readonly static HashSet<PlayerPiece> moved = new();

        public static string savePath;

        [STAThread]
        static void Main()
        {
            LoadGame();

            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Form = new Main();
            DgvForm = new DgvForm();

            Application.Run(Form);
        }

        public static void AutoSave()
        {
            if (File.Exists(Game.SavePath))
            {
                string path = Game.SavePath.Replace("\\", "/");
                path = path.Substring(0, path.LastIndexOf("/")) + "/" + "prev_" + Game.Turn + ".sav";
                if (File.Exists(path))
                    File.Delete(path);
                File.Copy(Game.SavePath, path);
            }
            Game.SaveGame();
        }
        public static void LoadGame()
        {
            if (File.Exists("savepath.txt"))
            {
                using (StreamReader reader = new("savepath.txt"))
                    savePath = reader.ReadLine();
                if (!Directory.Exists(savePath))
                    savePath = null;
            }
            if (savePath == null)
                savePath = ".";
            if (!savePath.EndsWith("/") && !savePath.EndsWith("\\") && !savePath.EndsWith(Path.PathSeparator))
                savePath += Path.DirectorySeparatorChar;
            savePath += "game.sav";

            if (File.Exists(savePath))
                Game = Game.LoadGame(savePath);
            else
                Game = new Game(savePath);
        }

        public static void EndTurn()
        {
            bool end = true;
            if (Game.Player.Pieces.Any(MoveLeft))
                end = MessageBox.Show("Move remaining.  End Turn?", string.Empty, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK;
            if (end)
            {
                Game.EndTurn();
                if (Game.GameOver)
                {
                    MessageBox.Show("Game over!  " + Game.Turn + " turns.");
                    //Game = new Game(savePath);
                }
                else
                {
                    AutoSave();
                }
                moved.Clear();
                Form.Refresh();
            }
        }

        public static void Hold()
        {
            if (Form.MapMain.SelTile != null && Form.MapMain.SelTile.Piece is PlayerPiece playerPiece)
                if (moved.Contains(playerPiece))
                    moved.Remove(playerPiece);
                else
                {
                    Next(true);
                    moved.Add(playerPiece);
                }
        }
        public static void Next(bool dir)
        {
            System.Drawing.Rectangle gameRect = Game.Map.GameRect();
            var moveLeft = Program.Game.Player.Pieces.Where(Program.MoveLeft).OrderBy(p =>
            {
                int main, secondary;
                if (p.Tile.Y < Game.Map.down)
                {
                    main = 1;
                    secondary = p.Tile.Y * gameRect.Width + p.Tile.X;
                }
                else if (p.Tile.X < Game.Map.left)
                {
                    main = 2;
                    secondary = p.Tile.X * gameRect.Height + p.Tile.Y;
                }
                else if (p.Tile.X > Game.Map.right)
                {
                    main = 4;
                    secondary = -p.Tile.X * gameRect.Height + p.Tile.Y;
                }
                else if (p.Tile.Y > Game.Map.up)
                {
                    main = 5;
                    secondary = -p.Tile.Y * gameRect.Width + p.Tile.X;
                }
                else
                {
                    main = 3;
                    secondary = p.Tile.Y * gameRect.Width + p.Tile.X;
                }
                main *= 2 * gameRect.Width * gameRect.Height;
                return main + secondary;
            }).ToList();

            if (moveLeft.Count > 0)
            {
                int cur = Form.MapMain.SelTile == null ? -1 : moveLeft.IndexOf(Form.MapMain.SelTile.Piece);
                if (dir)
                {
                    if (++cur >= moveLeft.Count)
                        cur = 0;
                }
                else if (--cur < 0)
                    cur = moveLeft.Count - 1;
                Form.MapMain.SelTile = moveLeft[cur].Tile;
            }
            else
            {
                Form.MapMain.SelTile = null;
            }
        }
        public static bool MoveLeft(Piece piece)
        {
            if (moved.Contains(piece))
                return false;

            bool move = false;
            if (!move && piece.HasBehavior<IBuilder.IBuildMech>(out _))
            {
                Mech.Cost(Game, out double e, out double m, Game.Blueprint1);
                move = e < Game.Player.Energy && m < Game.Player.Mass;
                if (!move)
                {
                    Mech.Cost(Game, out e, out m, Game.Blueprint2);
                    move = e < Game.Player.Energy && m < Game.Player.Mass;
                }
            }
            if (!move && piece.HasBehavior<IMovable>(out IMovable movable))
                move |= movable.MoveCur > 1 && movable.MoveCur + movable.MoveInc > movable.MoveMax;
            if (!move && piece.HasBehavior<IAttacker>(out IAttacker attacker))
            {
                double range = attacker.Attacks.Max(a => a.Attacked ? 0 : a.Range);
                move |= range > 0 && piece.Tile.GetVisibleTilesInRange(range).Any(t => t.Piece.HasBehavior<IKillable>(out _) && t.Piece.IsEnemy);
            }
            return move;
        }
    }
}
