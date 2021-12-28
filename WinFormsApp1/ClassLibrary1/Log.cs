using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MattUtil;
using ClassLibrary1;
using ClassLibrary1.Pieces;
using ClassLibrary1.Pieces.Enemies;
using ClassLibrary1.Pieces.Players;
using Values = ClassLibrary1.Pieces.IAttacker.Values;

namespace ClassLibrary1
{
    [Serializable]
    public class Log
    {
        public readonly Game Game;

        private readonly Dictionary<Piece, SortedSet<LogEntry>> _log;
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
                _log.TryGetValue(piece, out entries);
            return entries ?? new();
        }

        internal void LogAttack(IAttacker attacker, IKillable defender, double baseDamage, int randDmg, int hitsDmg, double shieldDmg)
        {
            LogEntry entry = new(_logNumInc++, Game.Turn, attacker, defender, baseDamage, randDmg, hitsDmg, shieldDmg);
            //Debug.WriteLine(statement);
            AddLog(entry, attacker.Piece, defender.Piece);
        }

        private void AddLog(LogEntry entry, params Piece[] pieces)
        {
            if (pieces.Length == 0)
                pieces = new Piece[] { null };
            foreach (Piece piece in pieces)
            {
                if (!_log.TryGetValue(piece, out SortedSet<LogEntry> entries))
                    _log.Add(piece, entries = new());
                entries.Add(entry);
            }
        }

        [Serializable]
        public class LogEntry : IComparable
        {
            public readonly int LogNum;
            public readonly int Turn;

            public Side AttackerSide;
            public string AttackerName;
            public string AttackerType;
            public Side DefenderSide;
            public string DefenderName;
            public string DefenderType;
            public readonly double BaseDamage;
            public readonly int RandDmg;
            public readonly int HitsDmg;
            public readonly double ShieldDmg;
            public readonly int HitsCur;
            public readonly double ShieldCur;

            public LogEntry(int logNum, int turn, IAttacker attacker, IKillable defender, double baseDamage, int randDmg, int hitsDmg, double shieldDmg)
            {
                static string GetName(IBehavior b) => b.Piece is Mech mech ? mech.GetName() : b.Piece.ToString();
                static string GetBlueprint(IBehavior b) => b.Piece is Mech mech ? mech.GetBlueprintName() : "";
                this.LogNum = logNum;
                this.Turn = turn;
                this.AttackerSide = attacker.Piece.Side;
                this.AttackerName = GetName(attacker);
                this.AttackerType = GetBlueprint(attacker);
                this.DefenderSide = defender.Piece.Side;
                this.DefenderName = GetName(defender);
                this.DefenderType = GetBlueprint(defender);
                this.BaseDamage = baseDamage;
                this.RandDmg = randDmg;
                this.HitsDmg = hitsDmg;
                this.ShieldDmg = shieldDmg;
                this.HitsCur = defender.HitsCur;
                this.ShieldCur = defender.ShieldCur;
            }

            public int CompareTo(object obj)
            {
                return ((LogEntry)obj).LogNum - LogNum;
            }
        }
    }
}
