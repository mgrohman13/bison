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

        internal void LogAttack(IAttacker attacker, IKillable defender, int attCur, int attMax, int defCur, int defMax, int dmgPos, int dmgNeg)// double baseDamage, int randDmg, int hitsDmg, double shieldDmg)
        {
            LogEntry entry = new(_logNumInc++, Game.Turn, attacker, defender, attCur, attMax, defCur, defMax, dmgPos, dmgNeg);// baseDamage, randDmg, hitsDmg, shieldDmg);
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

            public readonly int attCur, attMax, defCur, defMax, dmgPos, dmgNeg;

            //public readonly double BaseDamage;
            //public readonly int RandDmg;
            //public readonly int HitsDmg;
            //public readonly double ShieldDmg;
            //public readonly int HitsCur;
            //public readonly double ShieldCur;

            public LogEntry(int logNum, int turn, IAttacker attacker, IKillable defender, int attCur, int attMax, int defCur, int defMax, int dmgPos, int dmgNeg)// double baseDamage, int randDmg, int hitsDmg, double shieldDmg)
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

                this.attCur = attCur;
                this.attMax = attMax;
                this.defCur = defCur;
                this.defMax = defMax;
                this.dmgPos = dmgPos;
                this.dmgNeg = dmgNeg;

                //this.BaseDamage = baseDamage;
                //this.RandDmg = randDmg;
                //this.HitsDmg = hitsDmg;
                //this.ShieldDmg = shieldDmg;
                //this.HitsCur = defender.HitsCur;
                //this.ShieldCur = defender.ShieldCur;
            }

            public int CompareTo(object obj)
            {
                return ((LogEntry)obj).LogNum - LogNum;
            }
        }
    }
}
