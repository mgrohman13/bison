using ClassLibrary1;
using ClassLibrary1.Pieces;
using ClassLibrary1.Pieces.Players;
using ClassLibrary1.Pieces.Terrain;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Tile = ClassLibrary1.Map.Tile;
using Type = ClassLibrary1.Research.Type;

namespace WinFormsApp1
{
    static class Program
    {
        public static Game Game;

        public static Main Form;
        public static DgvForm DgvForm;

        private readonly static HashSet<PlayerPiece> moved = new();
        private readonly static HashSet<MechBlueprint> notifyOff = new();

        public static bool NotifyConstructor { get; set; } = true;

        public static string savePath;

        [STAThread]
        static void Main()
        {
            //void bp(int blueprintNum)
            //{
            //    Debug.Write(blueprintNum + "\t");
            //    string BlueprintNum = "";
            //    while (blueprintNum > 0)
            //    {
            //        blueprintNum--;
            //        BlueprintNum += (char)(blueprintNum % 26 + 65);
            //        blueprintNum /= 26;
            //    }
            //    Debug.WriteLine(BlueprintNum);
            //};
            //for (int a = 1; a < 780; a++)
            //    bp(a);
            LoadGame();

            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Form = new Main();
            DgvForm = new DgvForm();

            Application.Run(Form);

            Game.SaveGame();
        }

        public static void RefreshSelected()
        {
            Form.MapMain.Invalidate();
            Form.Info.Refresh();
            Form.Refresh();
        }
        public static void RefreshChanged()
        {
            Form.MapMain.RefreshRanges();
            RefreshSelected();
        }

        public static void AutoSave()
        {
            if (File.Exists(Game.SavePath))
            {
                string path = Game.SavePath.Replace("\\", "/");
                path = path[..path.LastIndexOf("/")] + "/" + "auto_" + (Game.Turn - 1) + ".sav";
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
            savePath ??= ".";
            if (!savePath.EndsWith("/") && !savePath.EndsWith("\\") && !savePath.EndsWith(Path.PathSeparator))
                savePath += Path.DirectorySeparatorChar;
            savePath += "game.sav";

            if (File.Exists(savePath) && !Game.TEST_MAP_GEN.HasValue)
                Game = Game.LoadGame(savePath);
            else
                Game = new Game(savePath);
        }

        public static void EndTurn()
        {
            bool end = true;
            if (Game.Player.Pieces.Any(MoveLeft))
                end = MessageBox.Show("Move remaining.  End Turn?", "", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK;
            if (end)
            {
                Type? researched = Game.EndTurn();
                if (Game.GameOver)
                {
                    MessageBox.Show((Game.Win ? "VICTORY!!!  :)" : "DEFEAT!  :(") + $"{Environment.NewLine}Game over...  {Game.Turn} turns.");
                    //Game = new Game(savePath);
                }
                else
                {
                    AutoSave();
                }
                moved.Clear();
                Program.RefreshChanged();

                if (researched.HasValue)
                    ResearchForm.ShowForm();
            }
        }

        public static void Hold()
        {
            if (Form.MapMain.SelTile != null && Form.MapMain.SelTile.Piece is PlayerPiece playerPiece)
                if (moved.Contains(playerPiece))
                    moved.Remove(playerPiece);
                else
                {
                    moved.Add(playerPiece);
                    Next(true);
                }
        }
        public static void Next(bool dir)
        {
            Rectangle gameRect = Game.Map.GameRect();
            var tiles = Program.Game.Player.Pieces.Where(Program.MoveLeft).Select(p => p.Tile);
            if (tiles.Any() && Form.MapMain.SelTile != null)
                tiles = tiles.Concat(new Tile[] { Form.MapMain.SelTile });
            var moveLeft = tiles.Distinct().OrderBy(t =>
            {
                int main, secondary;
                if (t.X > t.Y && t.X < -t.Y)
                {
                    main = 1;
                    secondary = t.Y * gameRect.Width + t.X;
                }
                else if (t.X < t.Y && t.X < -t.Y)
                {
                    main = 2;
                    secondary = t.X * gameRect.Height + t.Y;
                }
                else if (t.X > t.Y && t.X > -t.Y)
                {
                    main = 4;
                    secondary = -t.X * gameRect.Height + t.Y;
                }
                else if (t.X < t.Y && t.X > -t.Y)
                {
                    main = 5;
                    secondary = -t.Y * gameRect.Width + t.X;
                }
                else
                {
                    main = 3;
                    secondary = t.Y * gameRect.Width + t.X;
                }
                main *= 2 * gameRect.Width * gameRect.Height;
                return main + secondary;
            }).ToList();

            if (moveLeft.Count > 0)
            {
                int cur = Form.MapMain.SelTile == null ? -1 : moveLeft.IndexOf(Form.MapMain.SelTile);
                if (dir)
                {
                    if (++cur >= moveLeft.Count)
                        cur = 0;
                }
                else if (--cur < 0)
                    cur = moveLeft.Count - 1;
                Form.MapMain.SelTile = moveLeft[cur];
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
            IBuilder builder = piece.GetBehavior<IBuilder>();
            if (!move && piece.HasBehavior<IBuilder.IBuildMech>())
                move = Game.Player.Research.Blueprints.Any(b => Game.Player.Has(b.Energy, b.Mass) && GetNotify(b));
            if (!move && piece.HasBehavior<IBuilder.IBuildConstructor>())
            {
                Constructor.Cost(Game, out int e, out int m);
                move = Game.Player.Has(e, m) && NotifyConstructor;
            }
            if (!move && piece.HasBehavior<IBuilder.IBuildExtractor>())
                move = piece.Tile.GetVisibleTilesInRange(builder).Select(t => t.Piece as Resource).Where(r => r != null).Any(r =>
                {
                    Extractor.Cost(out int e, out int m, r);
                    return Game.Player.Has(e, m);
                });
            if (!move)
                if (builder != null && piece.Tile.GetVisibleTilesInRange(builder).Select(t => t.Piece as Foundation).Any(f => f != null))
                {
                    if (piece.HasBehavior<IBuilder.IBuildFactory>())
                    {
                        Factory.Cost(Game, out int e, out int m);
                        move = Game.Player.Has(e, m);
                    }
                    if (!move && piece.HasBehavior<IBuilder.IBuildTurret>())
                    {
                        Turret.Cost(Game, out int e, out int m);
                        move = Game.Player.Has(e, m);
                    }
                }

            if (!move && piece.HasBehavior(out IMovable movable))
                //need to support rallying long distances to uncomment this enhancement
                move |= movable.MoveCur > 1 && movable.MoveCur + movable.MoveInc > movable.MoveMax;// + (movable.MoveLimit - movable.MoveMax > 1 ? 1 : 0);
            if (!move && piece.HasBehavior(out IAttacker attacker))
            {
                static double GetRange(Attack a) => a.Attacked ? 0 : a.Range;
                double maxRange = attacker.Attacks.Max(GetRange);
                Attack max = Game.Rand.SelectValue(attacker.Attacks.Where(a => GetRange(a) == maxRange));
                move |= maxRange > 0 && piece.Tile.GetVisibleTilesInRange(max).Any(t => t.Piece != null && t.Piece.HasBehavior<IKillable>() && t.Piece.IsEnemy);
            }

            return move;
        }

        internal static void SetNotify(MechBlueprint blueprint, bool value)
        {
            if (value)
                notifyOff.Remove(blueprint);
            else
                notifyOff.Add(blueprint);
        }
        internal static bool GetNotify(MechBlueprint blueprint)
        {
            return !notifyOff.Contains(blueprint);
        }
    }
}
