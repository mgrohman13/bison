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

        public static HashSet<Piece> Moved = new();

        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Game = new Game();
            Form = new Main();

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
        public static bool MoveLeft(Piece piece)
        {
            if (Moved.Contains(piece))
                return false;

            bool move = false;
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
