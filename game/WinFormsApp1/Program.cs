using ClassLibrary1;
using ClassLibrary1.Pieces;
using ClassLibrary1.Pieces.Behavior;
using ClassLibrary1.Pieces.Behavior.Combat;
using ClassLibrary1.Pieces.Players;
using ClassLibrary1.Pieces.Terrain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Forms;
using Point = MattUtil.Point;
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
        private readonly static byte order = (byte)Game.Rand.Next(8);
        public static bool ViewedResearch
        {
            get { return data.ViewedResearch; }
            set { data.ViewedResearch = value; }
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
            lock (Game)
                if (File.Exists(Game.SavePath))
                {
                    string path = Game.SavePath.Replace("\\", "/");
                    path = path[..path.LastIndexOf('/')] + "/" + "auto_" + Game.Turn + "_" + suffix + ".sav";
                    if (File.Exists(path))
                        File.Delete(path);
                    File.Copy(Game.SavePath, path);
                }
        }

        public static void SaveGame()
        {
            lock (Game)
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
            if (!savePath.EndsWith('/') && !savePath.EndsWith('\\') && !savePath.EndsWith(Path.PathSeparator))
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
            lock (Game)
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

                    static IEnumerable<PlayerPiece> GetRepairs() => data.Sleep.Where(p => p.Tile != null && p.IsRepairing());
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
                    data.Moved.Clear();
                    data.Sleep.RemoveWhere(p => !p.IsPlayer || p.Tile == null || (p.HasBehavior(out IKillable k) && k.Dead));
                    data.Sleep.ExceptWith(repairs.Except(GetRepairs()));

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
        }
        private static bool LikelyResearch()
        {
            Player player = Game.Player;
            Research research = player.Research;
            Type researching = research.Researching;
            player.GetIncome(out _, out _, out int researchInc);
            //double deviation = Consts.IncomeDev(researchInc);
            //int add = (int)Math.Ceiling(researchInc + deviation * Math.PI);
            int progress = research.GetProgress(researching);
            int cost = research.GetCost(researching);
            return progress + researchInc >= cost;
        }

        public static void Moved(IBehavior behavior)
        {
            Wake(behavior);
            RefreshChanged();
            SaveGame();
        }

        public static void Hold() => data.Sleep.Remove(Toggle(data.Moved));
        public static void Sleep()
        {
            PlayerPiece piece = Toggle(data.Sleep);
            data.Moved.Remove(piece);
            if (MoveLeft(piece))
                data.Moved.Add(piece);
        }
        public static void Wake(IBehavior behavior)
        {
            if (behavior?.Piece is PlayerPiece playerPiece)
            {
                data.Sleep.Remove(playerPiece);
                data.Moved.Remove(playerPiece);
            }
        }
        private static PlayerPiece Toggle(HashSet<PlayerPiece> set)
        {
            PlayerPiece playerPiece = Form.MapMain.SelTile?.Piece as PlayerPiece;
            if (playerPiece != null)
                if (set.Remove(playerPiece))
                {
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
            //Rectangle gameRect = Game.Map.GameRect();

            var tiles = Game.Player.Pieces.Where(MoveLeft).Select(p => p.Tile);
            if (tiles.Any() && Form.MapMain.SelTile != null)
                tiles = tiles.Concat([Form.MapMain.SelTile]);

            var ordered = tiles.Distinct().OrderByDescending(t => t.GetDistance(Game.Player.Core.Tile));

            void f1() => ordered = ordered.ThenBy(t => t.X * ((order & 1) == 0 ? 1 : -1));
            void f2() => ordered = ordered.ThenBy(t => t.Y * ((order & 2) == 0 ? 1 : -1));
            if ((order & 4) == 0)
            {
                f1();
                f2();
            }
            else
            {
                f2();
                f1();
            }

            //{
            //Point p = new(t.X - Game.Player.Core.Tile.X, t.Y - Game.Player.Core.Tile.Y);
            //int main, secondary;
            //if (p.X > p.Y && p.X < -p.Y)
            //{
            //    main = 1;
            //    secondary = p.Y * gameRect.Width + p.X;
            //}
            //else if (p.X < p.Y && p.X < -p.Y)
            //{
            //    main = 2;
            //    secondary = p.X * gameRect.Height + p.Y;
            //}
            //else if (p.X > p.Y && p.X > -p.Y)
            //{
            //    main = 4;
            //    secondary = -p.X * gameRect.Height + p.Y;
            //}
            //else if (p.X < p.Y && p.X > -p.Y)
            //{
            //    main = 5;
            //    secondary = -p.Y * gameRect.Width + p.X;
            //}
            //else
            //{
            //    main = 3;
            //    secondary = p.Y * gameRect.Width + p.X;
            //}
            //main *= 2 * gameRect.Width * gameRect.Height;
            //return main + secondary;
            //}

            var moveLeft = ordered.ToList();
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

            if (data.Moved.Contains(piece))
                return false;

            piece.HasBehavior(out IKillable killable);
            // optimize?
            var friendly = piece.Tile.GetVisibleAdjacentTiles().Select(t => t.Piece).Where(p => p != null && p.IsPlayer);
            var attacks = Game.Enemy.VisiblePieces
                .Select(p => p.GetBehavior<IAttacker>())
                .Where(a => a != null)
                .SelectMany(a => a.Attacks)
                .SelectMany(a => friendly.Select(f => Tuple.Create(a, GetDefenders(a, f))))
                .Where(t => t.Item2.ContainsKey(killable));
            Dictionary<IKillable, int> GetDefenders(Attack attack, Piece friendly)
            {
                Piece attacker = attack.Piece;
                Tile attackFrom = attacker.Tile;
                //not quite right
                if (attack.Range == Attack.MELEE_RANGE && attacker.HasBehavior(out IMovable movable))
                    attackFrom = friendly.Tile.GetVisibleAdjacentTiles().Where(t => t.Piece == null || t.Piece.HasBehavior<IMovable>())
                        .FirstOrDefault(t => attackFrom.MoveDistTo(t) <= movable.MoveCur);
                return attack.GetDefenders(friendly, attackFrom);
            }

            if (data.Sleep.Contains(piece))
            {
                move |= attacks.Any();
            }
            else if (!move)
            {
                IBuilder builder = piece.GetBehavior<IBuilder>();
                if (!move && piece.HasBehavior<IBuilder.IBuildMech>())
                    move |= Game.Player.Research.Blueprints.Any(b => Game.Player.Has(b.Energy, b.Mass) && GetNotify(b));
                if (!move && piece.HasBehavior(out IBuilder.IBuildConstructor constructorB))
                {
                    Constructor.Cost(Game, out int e, out int m);
                    move |= Game.Player.Has(e, m) && Notify(constructorB);
                }
                if (!move && piece.HasBehavior(out IBuilder.IBuildDrone droneB))
                {
                    Drone.Cost(Game, out int e, out int m);
                    move |= Game.Player.Has(e, m) && Notify(droneB);
                }
                if (!move && piece.HasBehavior<IBuilder.IBuildExtractor>())
                    move |= piece.Tile.GetVisibleTilesInRange(builder).Select(t => t.Piece as Resource).Where(r => r != null).Any(r =>
                    {
                        Extractor.Cost(r, out int e, out int m);
                        return Game.Player.Has(e, m);
                    });
                if (!move)
                    if (builder != null && piece.Tile.GetVisibleTilesInRange(builder).Select(t => t.Piece as Foundation).Any(f => f != null))
                    {
                        if (piece.HasBehavior(out IBuilder.IBuildOutpost outpostB))
                        {
                            Outpost.Cost(Game, out int e, out int m);
                            move |= Game.Player.Has(e, m) && Notify(outpostB);
                        }
                        if (!move && piece.HasBehavior(out IBuilder.IBuildFactory factoryB))
                        {
                            Factory.Cost(Game, out int e, out int m);
                            move |= Game.Player.Has(e, m) && Notify(factoryB);
                        }
                        if (!move && piece.HasBehavior(out IBuilder.IBuildTurret turretB))
                        {
                            Turret.Cost(Game, out int e, out int m);
                            move |= Game.Player.Has(e, m) && Notify(turretB);
                        }
                        if (!move && piece.HasBehavior(out IBuilder.IBuildGenerator generatorB))
                        {
                            Generator.Cost(Game, out int e, out int m);
                            move |= Game.Player.Has(e, m) && Notify(generatorB);
                        }
                    }

                if (!move && piece is FoundationPiece foundationPiece)
                {
                    bool replaceable;
                    if (!move)
                    {
                        foundationPiece.ReplaceOutpost(false, out int e, out int m, out replaceable);
                        move |= replaceable && Game.Player.Has(e, m) && Notify(foundationPiece.GetBehavior<IBuilder.IBuildOutpost>(), true);
                    }
                    if (!move)
                    {
                        foundationPiece.ReplaceFactory(false, out int e, out int m, out replaceable);
                        move |= replaceable && Game.Player.Has(e, m) && Notify(foundationPiece.GetBehavior<IBuilder.IBuildFactory>(), true);
                    }
                    if (!move)
                    {
                        foundationPiece.ReplaceTurret(false, out int e, out int m, out replaceable);
                        move |= replaceable && Game.Player.Has(e, m) && Notify(foundationPiece.GetBehavior<IBuilder.IBuildTurret>(), true);
                    }
                    if (!move)
                    {
                        foundationPiece.ReplaceGenerator(false, out int e, out int m, out replaceable);
                        move |= replaceable && Game.Player.Has(e, m) && Notify(foundationPiece.GetBehavior<IBuilder.IBuildGenerator>(), true);
                    }
                }

                if (!move && piece.HasBehavior(out IMovable movable))
                {
                    //need to support rallying long distances to uncomment this enhancement
                    move |= movable.CanMove && movable.MoveCur > 1 && movable.MoveCur + movable.MoveInc > movable.MoveMax + (movable.MoveLimit - movable.MoveMax > 1 ? 1 : 0);
                    if (!move && killable != null)
                    {
                        var flattenedDef = attacks.Select(t => t.Item2.Keys
                                .Select(k => Tuple.Create(t.Item1, k, k.CurDefenseValue))
                                .OrderByDescending(t => t.Item3).ThenBy(t => t.Item2.Piece.PieceNum).ThenBy(t => t.Item2.Piece.GetType().ToString()).First());
                        var attDefPairs = flattenedDef.GroupBy(t => t.Item2)
                            .Select(g => Tuple.Create(g.Select(t => t.Item1).Distinct().Sum(a => Consts.StatValue(a.AttackCur)), g.Max(t => t.Item3)));
                        move |= attDefPairs.Any(t => t.Item1 >= t.Item2);
                    }
                }

                if (!move && piece.HasBehavior(out IMissileSilo silo))
                {
                    //move |= !silo.Producing;
                    if (!move)
                        move |= silo.Online && Game.Enemy.VisiblePieces.Any(e =>
                            e.HasBehavior<IKillable>() && piece.Tile.GetDistance(e.Tile) <= silo.SampleAttack.Range);
                }

                if (piece is Mech mech)
                {
                    move |= mech.CanUpgrade(out _, out _, out _);
                    if (!move)
                        move |= mech.CanCombineNow();
                }
            }

            if (piece.HasBehavior(out IAttacker attacker))
            {
                static double GetRange(Attack a) => a.CanAttack() ? a.Range : 0;
                double maxRange = attacker.Attacks.Max(GetRange);
                Attack max = Game.Rand.SelectValue(attacker.Attacks.Where(a => GetRange(a) == maxRange));
                canAttack = maxRange > 0 && piece.Tile.GetVisibleTilesInRange(max).Any(t => t.Piece != null && t.Piece.HasBehavior<IKillable>() && t.Piece.IsEnemy);
                if (!canAttack && piece.HasBehavior(out IMovable movable) && attacker.Attacks.Any(a => a.CanAttack() && a.Range == Attack.MELEE_RANGE))
                {
                    double meleeRange = movable.MoveCur + Attack.MELEE_RANGE;
                    var meleeTiles = Game.Enemy.VisiblePieces
                        .Where(e => e.HasBehavior<IKillable>() && piece.Tile.GetDistance(e.Tile) <= meleeRange)
                        .SelectMany(e => e.Tile.GetVisibleAdjacentTiles())
                        .Where(t => t.Piece == null || (t.Piece.IsPlayer && t.Piece.HasBehavior<IMovable>())).ToHashSet();
                    canAttack = meleeTiles.Any(t => piece.Tile.MoveDistTo(t) <= movable.MoveCur);
                    //if (!canAttack)
                    //    canAttack = TurnPath(movable).Select(Game.Map.GetVisibleTile).Any(meleeTiles.Contains);
                }
                move |= canAttack;
            }

            if (move)
                Wake(piece);

            return move;
        }
        public static IEnumerable<Point> TurnPath(IMovable movable, HashSet<Point> initial = null)
        {
            return initial;
            //Piece piece = movable.Piece;
            //Side side = piece.Side;
            //Tile start = piece.Tile;
            //double moveCur = movable.MoveCur;

            //Tile GetTile(Point point)
            //{
            //    Tile t = Game.Map.GetVisibleTile(point);
            //    Piece p = t?.Piece;
            //    if (p != null && (p.Side != side || !p.HasBehavior<IMovable>()))
            //        t = null;
            //    return t;
            //}

            //initial ??= [.. start.GetPointsInRange(movable)];
            //Dictionary<Tile, double> moves = initial.Select(GetTile).Where(t => t != null)
            //    .Where(t => start.GetDistance(t) == start.MoveDistTo(t))
            //    .ToDictionary(t => t, t => start.GetDistance(t));

            //var extended = start.GetPointsInRange(moveCur).Select(GetTile).Where(t => t != null).Except(moves.Keys).ToHashSet();

            //foreach (var pair in moves.ToArray())
            //    foreach (Tile to in extended.ToArray())
            //    {
            //        double dist = pair.Key.GetDistance(to);
            //        if (dist == pair.Key.MoveDistTo(to))
            //        {
            //            dist += pair.Value;
            //            if (dist <= moveCur)
            //            {
            //                moves[to] = Math.Min(moves.GetValueOrDefault(to, moveCur), dist);
            //                //extended.Remove(to);
            //                //found = true;
            //            }
            //        }
            //    }

            ////bool found = false;
            ////while (extended.Count > 0 && !found)
            ////{
            ////    found = false;
            ////    foreach (Tile to in extended.ToArray())
            ////    {
            ////        double min = moveCur + 1;
            ////        foreach (var pair in moves.ToArray())
            ////            if (pair.Key.GetDistance(to) == pair.Key.MoveDistTo(to))
            ////                min = Math.Min(min, pair.Value + pair.Key.GetDistance(to));
            ////        if (min <= moveCur)
            ////        {
            ////            moves.Add(to, min);
            ////            extended.Remove(to);
            ////            found = true;
            ////        }
            ////    }
            ////}

            //return moves.Keys.Select(t => t.Location);
            ////foreach (Point p1 in initial)
            ////    foreach (Point p2 in TurnPath(side, start, p1, extended, moveCur))
            ////        yield return p2;
        }
        //private static IEnumerable<Point> TurnPath(Side side, Tile start, Point point, HashSet<Point> extended, double moveCur)
        //{
        //    Tile tile = Game.Map.GetVisibleTile(point);
        //    //need to ensure min path....
        //    double remaining = moveCur - start.MoveDistTo(tile);
        //    if (remaining >= 0)
        //    {
        //        yield return point;
        //        extended.Remove(point);

        //        if (remaining >= 1)
        //            foreach (Point p1 in extended.ToHashSet())
        //                foreach (Point p2 in TurnPath(side, tile, p1, extended, remaining))
        //                    yield return p2;
        //    }
        //}
        //public static bool TurnPath(IMovable movable, Point point)
        //{
        //    //Tile from = movable.Piece.Tile, to = Game.Map.GetVisibleTile(point), t = null;
        //    //return from.GetPointsInRange(movable).Any(p =>
        //    //       from.MoveDistTo(t = Game.Map.GetVisibleTile(p)) + t.MoveDistTo(to) <= movable.MoveCur);
        //    return false;
        //    //TODO: performance? - need to walk outward 
        //    //Tile from = movable.Piece.Tile;
        //    //Tile to = Game.Map.GetVisibleTile(point);
        //    //double moveCur = movable.MoveCur;
        //    //var path = Game.Map.PathFind(from, to, moveCur, moveCur, p => from.GetDistance(p) > moveCur);
        //    //if (path == null)
        //    //    return false;
        //    //double dist = 0;
        //    //foreach (var p in path.Skip(1))
        //    //{
        //    //    Tile tile = Game.Map.GetVisibleTile(p);
        //    //    dist += from.MoveDistTo(tile);
        //    //    if (dist > moveCur)
        //    //        return false;
        //    //    from = tile;
        //    //}
        //    //return true;
        //}

        internal static void SetNotify(MechBlueprint blueprint, bool value)
        {
            if (value)
                data.NotifyOff.Remove(blueprint);
            else
                data.NotifyOff.Add(blueprint);
        }
        internal static bool GetNotify(MechBlueprint blueprint)
        {
            return !data.NotifyOff.Contains(blueprint);
        }

        public static bool Notify(IBuilder builder, bool replace = false)
        {
            var n = replace ? data.NotfiyReplace : data.Notfiy;
            return n.GetValueOrDefault(builder.GetType().Name, true);
        }
        public static void Notify(IBuilder builder, bool value, bool replace = false)
        {
            var n = replace ? data.NotfiyReplace : data.Notfiy;
            n[builder.GetType().Name] = value;
        }

        [Serializable]
        [DataContract(IsReference = true)]
        private class UIData // : ISerializable
        {
            public readonly HashSet<PlayerPiece> Moved = [], Sleep = [];

            public Dictionary<string, bool> Notfiy = new();
            public Dictionary<string, bool> NotfiyReplace = new();
            public readonly HashSet<MechBlueprint> NotifyOff = [];

            public bool AlertResearch = false;
            [NonSerialized]
            public bool ViewedResearch = false;

            //public void GetObjectData(SerializationInfo info, StreamingContext context)
            //{
            //}
        }
    }
}
