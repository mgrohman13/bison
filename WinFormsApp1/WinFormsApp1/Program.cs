using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
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

        public static HashSet<Piece> Moved = new();

        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Game = new Game();
            Form = new Main();
            DgvForm = new DgvForm();

            Application.Run(Form);
        }

        internal static void EndTurn()
        {
            bool end = true;
            if (Game.Player.Pieces.Any(MoveLeft))
                end = MessageBox.Show("Move remaining.  End Turn?", string.Empty, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK;
            if (end)
            {
                Game.EndTurn();
                Moved.Clear();
                Form.Refresh();
            }
        }

        internal static void Next(bool dir)
        {
            var moveLeft = Program.Game.Player.Pieces.Where(Program.MoveLeft).ToList();
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
            if (Moved.Contains(piece))
                return false;

            bool move = false;
            if (!move && piece is IBuilder.IBuildMech)
            {
                Mech.Cost(out double e, out double m, Game.Blueprint1, Game.Player.GetResearchMult());
                move = e < Game.Player.Energy && m < Game.Player.Mass;
                if (!move)
                {
                    Mech.Cost(out e, out m, Game.Blueprint2, Game.Player.GetResearchMult());
                    move = e < Game.Player.Energy && m < Game.Player.Mass;
                }
            }
            if (!move && piece is IMovable movable)
                move |= movable.MoveCur + movable.MoveInc > movable.MoveMax;
            if (!move && piece is IAttacker attacker)
            {
                double range = attacker.Attacks.Max(a => a.Attacked ? 0 : a.Range);
                move |= range > 0 && piece.Tile.GetTilesInRange(range).Any(t => t.Piece is IKillable && t.Piece.IsEnemy);
            }
            return move;
        }
    }
}
