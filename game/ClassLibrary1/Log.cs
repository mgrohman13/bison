using ClassLibrary1.Pieces;
using ClassLibrary1.Pieces.Players;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClassLibrary1
{
    [Serializable]
    public class Log
    {
        public readonly Game Game;

        private readonly Dictionary<string, SortedSet<LogEntry>> _log;
        private int _logNumInc;

        internal Log(Game game)
        {
            this.Game = game;
            this._log = new();
            this._logNumInc = 0;
        }

        public SortedSet<LogEntry> Data(Piece piece)
        {
            SortedSet<LogEntry> entries;
            if (piece == null)
                entries = new(_log.SelectMany(e => e.Value));
            else
                _log.TryGetValue(piece.ToString(), out entries);
            return entries ?? new();
        }

        internal void LogAttack(Attack attack, int startAttack, IKillable target, Dictionary<Defense, int> startDefense)
        {
            LogEntry entry = new(_logNumInc++, Game.Turn, attack, startAttack, target, startDefense);
            AddLog(entry, attack.Piece, target.Piece);
        }

        private void AddLog(LogEntry entry, params Piece[] pieces)
        {
            if (pieces.Length == 0)
                pieces = new Piece[] { null };
            foreach (Piece piece in pieces)
            {
                string key = piece.ToString();
                if (!_log.TryGetValue(key, out SortedSet<LogEntry> entries))
                    _log.Add(key, entries = new());
                entries.Add(entry);
            }
        }

        [Serializable]
        public class LogEntry : IComparable
        {
            public readonly int LogNum;
            public readonly int Turn;

            public readonly Side AttackerSide;
            public readonly string AttackerName;
            public readonly string AttackerType;
            public readonly Side DefenderSide;
            public readonly string DefenderName;
            public readonly string DefenderType;

            public readonly bool Killed;

            public readonly Stat Attack;
            public readonly Stat[] Defense;

            public LogEntry(int logNum, int turn, Attack attack, int startAttack, IKillable target, Dictionary<Defense, int> startDefense)
            {
                static string GetName(Piece p) => p is Mech mech ? mech.GetName() : p.ToString();
                static string GetBlueprint(Piece p) => p is Mech mech ? mech.GetBlueprintName() : "";

                this.LogNum = logNum;
                this.Turn = turn;

                Piece attacker = attack.Piece;
                this.AttackerSide = attacker.Side;
                this.AttackerName = GetName(attacker);
                this.AttackerType = GetBlueprint(attacker);

                Piece defender = target.Piece;
                this.DefenderSide = defender.Side;
                this.DefenderName = GetName(defender);
                this.DefenderType = GetBlueprint(defender);

                this.Killed = target.Dead;

                this.Attack = new(startAttack, attack.AttackCur, attack.AttackMax);
                this.Defense = target.AllDefenses.Select(d => new Stat(startDefense[d], d.DefenseCur, d.DefenseMax)).ToArray();
                //.OrderBy(d => d.Type switch
                //{
                //    DefenseType.Hits => 1,
                //    DefenseType.Shield => 2,
                //    DefenseType.Armor => 3,
                //    _ => throw new Exception(),
                //})
            }

            public int CompareTo(object obj)
            {
                return ((LogEntry)obj).LogNum - LogNum;
            }

            [Serializable]
            public class Stat
            {
                public readonly int Prev, Cur, Max;
                public Stat(int prev, int cur, int max)
                {
                    this.Prev = prev;
                    this.Cur = cur;
                    this.Max = max;
                }
            }
        }
    }
}
