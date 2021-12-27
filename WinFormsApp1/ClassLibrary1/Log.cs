﻿using System;
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

        public string Data(Piece piece)
        {
            SortedSet<LogEntry> entries;
            if (piece == null)
                entries = new SortedSet<LogEntry>(_log.SelectMany(e => e.Value));
            else
                _log.TryGetValue(piece, out entries);
            return entries == null ? "" : String.Join(Environment.NewLine, entries.Select(e => e.Entry));
        }

        internal void LogAttack(IAttacker attacker, IKillable defender, double baseDamage, int randDmg, int hitsDmg, double shieldDmg)
        {
            string statement = string.Format("{0}. {1} -> {2} ({3})" + Environment.NewLine + "      {4}{5} ({6})", Game.Turn, attacker.Piece, defender.Piece, Format(baseDamage),
                hitsDmg > 0 ? string.Format("{0} - {1} = {2}", defender.HitsCur + hitsDmg, hitsDmg, defender.HitsCur) : defender.HitsCur,
                shieldDmg > 0 ? string.Format(" ; {0:0.0} - {1:0.0} = {2:0.0}", defender.ShieldCur + shieldDmg, shieldDmg, defender.ShieldCur) : "", randDmg);
            Debug.WriteLine(statement);
            AddLog(statement, attacker.Piece, defender.Piece);
        }
        private static string Format(double v)
        {
            string result = v.ToString("0.0");
            if (result.EndsWith(".0"))
                result = v.ToString("0");
            return result;
        }

        private void AddLog(string statement, params Piece[] pieces)
        {
            LogEntry entry = new(_logNumInc++, statement);
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
        private class LogEntry : IComparable
        {
            public readonly int LogNum;
            public readonly string Entry;
            public LogEntry(int logNum, string entry)
            {
                this.LogNum = logNum;
                this.Entry = entry;
            }
            public int CompareTo(object obj)
            {
                return ((LogEntry)obj).LogNum - LogNum;
            }
        }
    }
}
