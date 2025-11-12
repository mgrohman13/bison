using game2.game;
using game2.map;

namespace game2.pieces.behavior
{
    public class Combatant(Piece piece, bool passive, int attCur, int attBase, int defCur, int defBase, int hpCur, int hpMax) : IBehavior
    {
        private readonly Piece _piece = piece;
        public Piece Piece => _piece;
        public Game Game => Piece.Game;

        public readonly bool Passive = passive;

        private int _attCur = attCur, _defCur = defCur, _hpCur = hpCur;
        private readonly int _attMax = attBase, _defMax = defBase, _hpMax = hpMax;
        private bool _attacked = true;

        public int AttCur => _attCur + Piece.Tile.Terrain.AttMod();
        public int AttBase => _attMax;
        public int DefCur => _defCur + Piece.Tile.Terrain.DefMod();
        public int DefBase => _defMax;
        public int HPCur => _hpCur;
        public int HPMax => _hpMax;

        public bool Dead => _hpCur <= 0;

        Piece IBehavior.Piece => throw new NotImplementedException();

        Game IBehavior.Game => throw new NotImplementedException();

        public Combatant(Piece piece, int def, int hp) : this(piece, true, 0, 0, def, def, hp, hp) { }
        public Combatant(Piece piece, int att, int def, int hp) : this(piece, false, att, att, def, def, hp, hp) { }

        private bool Attack(Combatant target)
        {
            bool DoAtt() => !_attacked && AttCur > 0 && !Dead && !target.Dead;
            if (DoAtt())
            {
                //Piece.Game.Map.UpdateVision(new[] { Piece, target.Piece }.Select(p => p.Tile));

                //target.OnAttacked();
                //int startAttack = AttackCur;
                //Dictionary<Defense, int> startDefense = target.AllDefenses.ToDictionary(d => d, d => d.DefenseCur);

                int attDmg = 0, defDmg = 0;

                int rounds = AttCur;
                for (int a = 0; a < rounds && DoAtt() && attDmg < target.HPCur && defDmg < this.HPCur; a++)
                //if (a == 0 || Game.Rand.Bool())
                {
                    ////Defense defense = Game.Rand.Iterate(target.TotalDefenses.Where(d => !d.Dead)).OrderBy(CombatTypes.CompareDef).First();
                    //Defense defense = Game.Rand.SelectValue(target.AllDefenses, CombatTypes.GetDefenceChance);
                    //bool activeDefense = target.HasBehavior<IAttacker>();

                    if (Game.Rand.Next(AttCur + target.DefCur) < AttCur)
                        attDmg++;
                    else
                        defDmg++;
                }

                this.Damage(attDmg);//, target);
                target.Damage(defDmg);//, this);

                _attacked = true;
                if (Piece.HasBehavior(out Movable? movable))
                    movable.RestrictMove = true;

                //if (Piece.HasBehavior(out IAttacker attacker))
                //    attacker.RaiseAttackEvent(this, target, targetTile);
                //Piece.Game.Log.LogAttack(this, startAttack, target, startDefense);
                return true;
            }
            return false;
        }

        private void Damage(int damage)//, Combatant? attacker)
        {
            if (damage > 0)
            {
                _hpCur -= damage;
                if (Dead)
                    Piece.Die();
                else
                    Piece.Wound(damage / (float)(HPCur + damage));
            }
        }

        void IBehavior.Wound(float woundPct)
        {
            _attCur = Game.Consts.Wound(woundPct, _attCur, AttBase);
            _defCur = Game.Consts.Wound(woundPct, _defCur, DefBase);
        }

        Resources IBehavior.GetTurnEnd()
        {
            return new Resources();
        }
        void IBehavior.EndTurn(ref Resources resources)
        {
        }

        void IBehavior.StartTurn()
        {
        }
    }
}
