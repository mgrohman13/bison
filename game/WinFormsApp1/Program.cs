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
using Tile = ClassLibrary1.Map.Map.Tile;
using Type = ClassLibrary1.Research.Type;

namespace WinFormsApp1
{
    static class Program
    {
        public static Game Game;

        public readonly static Main Form;
        public readonly static BuildForm BuildForm;

        private static UIData data = new();

        public static bool ViewedResearch
        {
            get { return data.ViewedResearch; }
            set { data.ViewedResearch = value; }
        }
        public static bool NotifyConstructor
        {
            get { return data.NotifyConstructor; }
            set { data.NotifyConstructor = value; }
        }
        public static bool NotifyDrone
        {
            get { return data.NotifyDrone; }
            set { data.NotifyDrone = value; }
        }

        public static string savePath;

        static Program()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Form = new Main();
            BuildForm = new BuildForm();
        }

        [STAThread]
        static void Main()
        {
            //if (MTRandom.GetOEIntMax(Consts.PortalDecayRate) > Consts.PortalEntranceDef)
            //    throw new Exception();

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

            Treasure.CollectEvent += Treasure_CollectEvent;
            LoadGame();
            Application.Run(Form);
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

        public static void CopyAutoSave(string suffix)
        {
            if (File.Exists(Game.SavePath))
            {
                string path = Game.SavePath.Replace("\\", "/");
                path = path[..path.LastIndexOf("/")] + "/" + "auto_" + Game.Turn + "_" + suffix + ".sav";
                if (File.Exists(path))
                    File.Delete(path);
                File.Copy(Game.SavePath, path);
            }
        }

        public static void SaveGame()
        {
            Game.SaveGame(data);
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
            {
                Game = Game.LoadGame(savePath, out data);
            }
            else
            {
                Game = new Game(savePath);
                data = new();
                SaveGame();
            }
        }

        private static void Treasure_CollectEvent(object sender, Treasure.CollectEventArgs e)
        {
            if (e.Tile != null)
                Form.MapMain.Center(e.Tile);
            Form.Refresh();
            MessageBox.Show(e.Info);
            if (e.Research)
                ResearchForm.ShowForm();
        }

        public static void EndTurn()
        {
            bool end = true;
            if (Game.Player.Pieces.Any(MoveLeft))
                end = MessageBox.Show("Move remaining.  End Turn?", "", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK;
            if (end)
            {
                if (!ViewedResearch && (data.AlertResearch || LikelyResearch()))
                    if (ResearchForm.ShowForm())
                        RefreshChanged();
                    else
                        return;

                SaveGame();
                CopyAutoSave("e");

                IEnumerable<PlayerPiece> GetRepairs() => data.sleep.Where(p => p.IsRepairing());
                var repairs = GetRepairs().ToHashSet();

                Form.UpdateProgress(null, 0);
                Type? researched = Game.EndTurn(Form.UpdateProgress);
                Form.UpdateProgress(null, 2);

                if (Game.GameOver)
                {
                    MessageBox.Show((Game.Win ? "VICTORY!!!  :)" : "DEFEAT!  :(") + $"{Environment.NewLine}Hives Destroyed: {Game.Victory}/{Game.POINTS_TO_WIN}{Environment.NewLine}Game over...  {Game.Turn} turns.");
                    CopyAutoSave(Game.Win ? "win" : "loss");
                }
                else
                {
                    CopyAutoSave("s");
                }
                data.moved.Clear();
                data.sleep.RemoveWhere(p => !p.IsPlayer || p.Tile == null || (p.HasBehavior(out IKillable k) && k.Dead));
                data.sleep.ExceptWith(repairs.Except(GetRepairs()));

                data.AlertResearch = researched.HasValue;
                if (researched.HasValue && (researched.Value == Type.Mech || Research.IsMech(researched.Value)))
                {
                    RefreshChanged();
                    BuildForm.BuilderDialogMech();
                }
                ViewedResearch = false;

                RefreshChanged();
            }
        }
        private static bool LikelyResearch()
        {
            Player player = Game.Player;
            Research research = player.Research;
            Type researching = research.Researching;
            player.GetIncome(out _, out _, out double researchInc);
            double deviation = Consts.IncomeDev(researchInc);
            int add = (int)Math.Ceiling(researchInc + deviation * Math.PI);
            int progress = research.GetProgress(researching);
            int cost = research.GetCost(researching);
            return progress + add >= cost;
        }

        public static void Moved(IBehavior behavior)
        {
            Wake(behavior);
            RefreshChanged();
            //Program.SaveGame();
        }

        public static void Hold() => data.sleep.Remove(Toggle(data.moved));
        public static void Sleep() => data.moved.Remove(Toggle(data.sleep));
        public static void Wake(IBehavior behavior)
        {
            if (behavior?.Piece is PlayerPiece playerPiece)
            {
                data.sleep.Remove(playerPiece);
                data.moved.Remove(playerPiece);
            }
        }
        private static PlayerPiece Toggle(HashSet<PlayerPiece> set)
        {
            PlayerPiece playerPiece = Form.MapMain.SelTile?.Piece as PlayerPiece;
            if (playerPiece is not null)
                if (set.Contains(playerPiece))
                {
                    set.Remove(playerPiece);
                    RefreshSelected();
                }
                else
                {
                    set.Add(playerPiece);
                    Next(true);
                }
            return playerPiece;
        }
        public static void Next(bool dir)
        {
            Rectangle gameRect = Game.Map.GameRect();
            var tiles = Game.Player.Pieces.Where(MoveLeft).Select(p => p.Tile);
            if (tiles.Any() && Form.MapMain.SelTile != null)
                tiles = tiles.Concat(new Tile[] { Form.MapMain.SelTile });

            var moveLeft = tiles.Distinct().OrderBy(t =>
            {
                var tiles = Game.Player.Pieces.Where(MoveLeft).Select(p => p.Tile);
                Point p = new(t.X - Game.Player.Core.Tile.X, t.Y - Game.Player.Core.Tile.Y);
                int main, secondary;
                if (p.X > p.Y && p.X < -p.Y)
                {
                    main = 1;
                    secondary = p.Y * gameRect.Width + p.X;
                }
                else if (p.X < p.Y && p.X < -p.Y)
                {
                    main = 2;
                    secondary = p.X * gameRect.Height + p.Y;
                }
                else if (p.X > p.Y && p.X > -p.Y)
                {
                    main = 4;
                    secondary = -p.X * gameRect.Height + p.Y;
                }
                else if (p.X < p.Y && p.X > -p.Y)
                {
                    main = 5;
                    secondary = -p.Y * gameRect.Width + p.X;
                }
                else
                {
                    main = 3;
                    secondary = p.Y * gameRect.Width + p.X;
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
        public static bool MoveLeft(Piece piece) => MoveLeft(piece, out _);
        public static bool MoveLeft(Piece piece, out bool canAttack)
        {
            bool move = false;
            canAttack = false;

            if (piece.HasBehavior(out IAttacker attacker))
            {
                static double GetRange(Attack a) => a.CanAttack() ? a.Range : 0;
                double maxRange = attacker.Attacks.Max(GetRange);
                Attack max = Game.Rand.SelectValue(attacker.Attacks.Where(a => GetRange(a) == maxRange));
                canAttack = maxRange > 0 && piece.Tile.GetVisibleTilesInRange(max).Any(t => t.Piece != null && t.Piece.HasBehavior<IKillable>() && t.Piece.IsEnemy);
                if (!canAttack && piece.HasBehavior(out IMovable movable) && attacker.Attacks.Any(a => a.CanAttack() && a.Range == Attack.MELEE_RANGE))
                    canAttack |= piece.Tile.GetPointsInRange(movable).Select(Game.Map.GetVisibleTile).Where(t => t is not null).SelectMany(t => t.GetVisibleAdjacentTiles())
                        .Any(t => t.Piece is not null && t.Piece.IsEnemy && t.Piece.HasBehavior<IKillable>());
                move |= canAttack;
            }

            if (data.moved.Contains(piece))
                return false;

            piece.HasBehavior(out IKillable killable);
            // optimize?
            var friendly = piece.Tile.GetVisibleAdjacentTiles().Select(t => t.Piece).Where(p => p is not null && p.IsPlayer);
            var attacks = Game.Enemy.VisiblePieces
                .Select(p => p.GetBehavior<IAttacker>())
                .Where(a => a is not null)
                .SelectMany(a => a.Attacks)
                .SelectMany(a => friendly.Select(f => Tuple.Create(a, GetDefenders(a, f))))
                .Where(t => t.Item2.ContainsKey(killable));
            Dictionary<IKillable, int> GetDefenders(Attack attack, Piece friendly)
            {
                Piece attacker = attack.Piece;
                Tile attackFrom = attacker.Tile;
                if (attack.Range == Attack.MELEE_RANGE && attacker.HasBehavior(out IMovable movable))
                    attackFrom = friendly.Tile.GetVisibleAdjacentTiles().Where(t => t.Piece is null || t.Piece.HasBehavior<IMovable>())
                        .FirstOrDefault(t => t.GetDistance(attackFrom) <= movable.MoveCur);
                return attack.GetDefenders(friendly, attackFrom);
            }

            if (data.sleep.Contains(piece))
            {
                move |= attacks.Any();
            }
            else if (!move)
            {
                IBuilder builder = piece.GetBehavior<IBuilder>();
                if (!move && piece.HasBehavior<IBuilder.IBuildMech>())
                    move = Game.Player.Research.Blueprints.Any(b => Game.Player.Has(b.Energy, b.Mass) && GetNotify(b));
                if (!move && piece.HasBehavior<IBuilder.IBuildConstructor>())
                {
                    Constructor.Cost(Game, out int e, out int m);
                    move = Game.Player.Has(e, m) && NotifyConstructor;
                }
                if (!move && piece.HasBehavior<IBuilder.IBuildDrone>())
                {
                    Drone.Cost(Game, out int e, out int m);
                    move = Game.Player.Has(e, m) && NotifyDrone;
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
                        if (!move && piece.HasBehavior<IBuilder.IBuildGenerator>())
                        {
                            Generator.Cost(Game, out int e, out int m);
                            move = Game.Player.Has(e, m);
                        }
                    }

                if (!move && piece.HasBehavior(out IMovable movable))
                {
                    //need to support rallying long distances to uncomment this enhancement
                    move |= movable.CanMove && movable.MoveCur > 1 && movable.MoveCur + movable.MoveInc > movable.MoveMax;// + (movable.MoveLimit - movable.MoveMax > 1 ? 1 : 0);
                    if (!move && killable != null)
                    {
                        var flattenedDef = attacks.Select(t => t.Item2.Keys
                                .Select(k => Tuple.Create(t.Item1, k, k.AllDefenses.Sum(d => Consts.StatValue(d.DefenseCur))))
                                .OrderByDescending(t => t.Item3).ThenBy(t => t.Item2.Piece.PieceNum).ThenBy(t => t.Item2.Piece.GetType().ToString()).First());
                        var attDefPairs = flattenedDef.GroupBy(t => t.Item2)
                            .Select(g => Tuple.Create(g.Select(t => t.Item1).Distinct().Sum(a => Consts.StatValue(a.AttackCur)), g.Max(t => t.Item3)));
                        move |= attDefPairs.Any(t => t.Item1 >= t.Item2);
                    }
                }

                if (piece is Mech mech)
                    move |= mech.CanUpgrade(out _, out _, out _);
            }

            if (move)
                Wake(piece);

            return move;
        }

        internal static void SetNotify(MechBlueprint blueprint, bool value)
        {
            if (value)
                data.notifyOff.Remove(blueprint);
            else
                data.notifyOff.Add(blueprint);
        }
        internal static bool GetNotify(MechBlueprint blueprint)
        {
            return !data.notifyOff.Contains(blueprint);
        }

        [Serializable]
        private class UIData // : ISerializable
        {
            public readonly HashSet<PlayerPiece> moved = new(), sleep = new();

            public readonly HashSet<MechBlueprint> notifyOff = new();
            public bool NotifyConstructor = true, NotifyDrone = true;

            public bool AlertResearch = false;
            [NonSerialized]
            public bool ViewedResearch = false;

            //public void GetObjectData(SerializationInfo info, StreamingContext context)
            //{
            //}
        }
    }
}
