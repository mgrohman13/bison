using game2.game;
using game2.sides;

namespace game2.runes.pattern
{
    public class BuildUnit : IRunePattern<BuildUnit>
    {
        public readonly Player Player;

        //public readonly string BlueprintNum;
        //public readonly BuildUnit UpgradeFrom;

        public readonly Resources Cost;

        public readonly int ResearchLevel;
        public readonly (int min, int max) Charges;

        public readonly int Attack, Defense, HP, MoveInc, MoveMax, Vision;

        private BuildUnit(Player player, int researchLevel, float runeValue, int att, int def, int hp, int moveInc, int moveMax, int vision) //int blueprintNum, BuildUnit upgrade,
        {
            //this.BlueprintNum = "";
            //if (blueprintNum > 0)
            //{
            //    blueprintNum--;
            //    int num = 1 + blueprintNum / 26;
            //    BlueprintNum = (char)(blueprintNum % 26 + 65) + (num > 1 ? num.ToString() : "");
            //}

            //this.UpgradeFrom = upgrade;

            this.Player = player;
            this.ResearchLevel = researchLevel;
            this.Attack = att;
            this.Defense = def;
            this.HP = hp;
            this.MoveInc = moveInc;
            this.MoveMax = moveMax;
            this.Vision = vision;

            this.Cost = CalcCost(player.Game, researchLevel);

            this.Charges = (1, 1);
            //this.Energy = Game.Rand.Round(energy / 10.0) * 10;
            //this.Mass = Game.Rand.Round((mass + (energy - this.Energy) / Consts.EnergyMassRatio) / 5.0) * 5;
        }
        public static BuildUnit NewPattern(Player player, int researchLevel, float runeValue, int? forceCharges)
        {
            return Generate.NewUnit(player, researchLevel, runeValue, forceCharges);
        }
        private Resources CalcCost(Game game, int researchLevel)
        {
            return Generate.CalcCost(game, researchLevel, Attack, Defense, HP, MoveInc, MoveMax, Vision);
        }
        RuneShape IRunePattern.NewShape() => new(Player, this, Cost, Charges);

        public bool CanPlay(Rune rune)
        {
            throw new NotImplementedException();
        }
        public void Play(Rune rune, object? target)
        {
            throw new NotImplementedException();
        }
        public (bool play, object target) HandleChoice(IChoiceHandler handler)
        {
            throw new NotImplementedException();
        }
        public IEnumerable<IRuneEffect>? GetEffects()
        {
            throw new NotImplementedException();
        }

        public int TotalCost()
        {
            return Cost.Sum();
        }
        public float TotalCostValue()
        {
            return Enumerable.Range(0, Resources.NumResources).Select(a => Cost[a] * Player.Game.Consts.ResourceValue[a]).Sum();
        }

        //public override string ToString()
        //{
        //    return "Type " + BlueprintNum;
        //}

        private class Generate
        {
            //private static void GetPcts(ICollection<ShipDesign> designs, Game game, int research,
            //        out float upkeep, out float speed, out float att, out float colony, out float trans, out float ds)
            //{
            //    if (designs != null && designs.Count > 0)
            //    {
            //        upkeep = speed = att = colony = trans = ds = 0;
            //        float speedStr = GetSpeedStr(research), transStr = GetTransStr(research), dsStr = GetDsResearchStr(research);
            //        float avgCost = designs.Average(sd => sd.GetTotCost());
            //        float baseMultTot = 0, strMultTot = 0;
            //        foreach (ShipDesign design in designs)
            //        {
            //            float totalCost = design.GetTotCost();
            //            float speedMult = design.Speed / speedStr;

            //            float baseMult = Math.Pow(research, 0.26) * 52;
            //            baseMult = (design.Research + 2 * baseMult) / (research + baseMult);
            //            baseMult = Math.Pow(baseMult, baseMult < 1 ? 3.9 : 1.3);
            //            baseMultTot += baseMult;

            //            upkeep += design.Upkeep / (totalCost / design.GetUpkeepPayoff(game) * Consts.CostUpkeepPct) * baseMult;
            //            speed += (design.Speed + SpeedAvg) / (speedStr + SpeedAvg) * baseMult;

            //            float costMult = Math.Sqrt(avgCost / totalCost);
            //            float nonDSPct = 1 - Consts.LimitPct(design.Speed / GetSpeedStr(design.Research) * design.BombardDamage / GetDsResearchStr(design.Research) * costMult);
            //            float strMult = design.GetNonColonyPct(game) * design.GetNonTransPct(game) * nonDSPct * baseMult;
            //            strMultTot += strMult;

            //            const float strAdd = 1.3;
            //            att += (design.Att + strAdd) / ((float)design.Def + strAdd) * strMult;

            //            costMult *= baseMult;

            //            if (design.Colony)
            //                colony += costMult;
            //            trans += speedMult * design.Trans / transStr * costMult;
            //            ds += speedMult * design.BombardDamage / dsStr * costMult;
            //        }

            //        upkeep /= baseMultTot;
            //        speed /= baseMultTot;

            //        att /= strMultTot;

            //        colony /= baseMultTot;
            //        trans /= baseMultTot;
            //        ds /= baseMultTot;
            //    }
            //    else
            //    {
            //        upkeep = speed = att = 1;
            //        colony = trans = ds = float.NaN;
            //    }
            //}

            public static Resources CalcCost(Game game, int researchLevel, int Attack, int Defense, int HP, int MoveInc, int MoveMax, int Vision)
            {
                float str = (Attack + Defense) * HP;
                float vision = Vision;
                float move = (MoveInc + MoveMax);

                float cost = (str + vision) * move;

                return new(Game.Rand.Round(cost));

                //float baseMove = Math.Pow(Consts.MoveValue(movable), 1.69);
                //float r = Math.Pow(Math.Pow(resilience, Math.Log(3) / Math.Log(2)) * 1.5 + 0.5, .26);

                //float AttCost(IAttacker.Values a)
                //{
                //    float rangeMult = 1;
                //    if (a.Range > Attack.MELEE_RANGE)
                //        rangeMult = (a.Range + Attack.MELEE_RANGE) / (Math.PI * Attack.MIN_RANGED);
                //    rangeMult = Math.Pow(rangeMult, 1.17);
                //    return BaseAttCost(a)
                //        * Math.Sqrt(a.Reload / CombatTypes.ReloadAvg(a.Attack))
                //        * rangeMult;
                //}
                //;
                //float DefCost(IKillable.Values d) => Consts.StatValue(d.Defense) * CombatTypes.Cost(d.Type)
                //    * (d.Type == DefenseType.Hits ? Math.Pow(r, 1.56) * .78 : 1.04);

                //float attPow = Math.Pow(1 + (MoveCostMult + baseMove) / 3.9 / MoveCostMult, .21);
                //float att = MultAttCost(Math.Pow(attacker.Sum(AttCost), attPow), researchMult);
                //float def = killable.Sum(DefCost) / researchMult * StatsCostMult;

                //float mult = Math.Sqrt(researchMult);
                //float move = (baseMove + MoveCostAdd) * MoveCostMult / mult;
                //float v = vision;
                //v = (v + 6.5) * 3.9 / mult;

                //float total = (att + v) * (def + move) * r * Consts.MechCostMult;

                ////Debug.WriteLine($"total: {total}");

                //float energyPct = Math.Sqrt(att / (att + def + v));
                //energyPct *= move / (move + def + v);
                //energyPct = Math.Sqrt(energyPct);

                //float attEnergy = attacker.Sum(a => AttCost(a) * CombatTypes.EnergyCostRatio(a.Type));
                //float defEnergy = killable.Sum(d => DefCost(d) * CombatTypes.EnergyCostRatio(d.Type));
                //float attMass = attacker.Sum(a => AttCost(a) * (1 - CombatTypes.EnergyCostRatio(a.Type)));
                //float defMass = killable.Sum(d => DefCost(d) * (1 - CombatTypes.EnergyCostRatio(d.Type)));

                //energyPct *= (attEnergy + defEnergy) / (attEnergy + defEnergy + attMass + defMass);
                //energyPct = Math.Sqrt(energyPct);

                //energy = total * energyPct;
                //mass = (total - energy) / Consts.EnergyMassRatio;
            }
            //private const float MoveCostAdd = 3.9;
            //private const float MoveCostMult = 6.5;
            //private const float StatsCostMult = 2.1;
            //private static float BaseAttCost(IAttacker.Values a) =>
            //    Consts.StatValue(a.Attack) * CombatTypes.Cost(a.Type);
            //private static float MultAttCost(float cost, float researchMult) =>
            //    cost / researchMult * StatsCostMult;
            //internal static float MissileCost(IAttacker.Values missile, float researchMult) =>
            //     (MultAttCost(BaseAttCost(missile), researchMult) + 0)
            //        * (StatsCostMult + MoveCostAdd * MoveCostMult) * 1 * Consts.MechCostMult;

            internal static BuildUnit NewUnit(Player player, int researchLevel, float runeValue, int? forceCharges)//IResearch research, int blueprintNum, int researchLevel, bool alien)
            {
                Game game = player.Game;

                (int att, int def, int hp) combatant = GenCombatant(game, researchLevel);
                int vision = GenVision(game, researchLevel);
                (int inc, int max) move = GenMovable(game, researchLevel, vision);

                return CheckCost(new(player, researchLevel, runeValue, combatant.att, combatant.def, combatant.hp, move.inc, move.max, vision));
            }

            private static (int att, int def, int hp) GenCombatant(Game game, int researchLevel)
            {
                int s1 = GenAttDef(game, researchLevel);
                int s2 = GenAttDef(game, researchLevel, s1);
                if (Game.Rand.Bool())
                    (s1, s2) = (s2, s1);

                return (s1, s2, GenHP(game, s1, s2, researchLevel));
            }
            private static (int inc, int max) GenMovable(Game game, int researchLevel, int vision)
            {
                int inc = GenMoveInc(game, researchLevel, vision);
                int max = GenMoveMax(game, researchLevel, vision, inc);
                return new(inc, max);
            }
            private static int GenAttDef(Game game, int researchLevel, int? previous = null)
            {
                throw new NotImplementedException();
            }
            private static int GenHP(Game game, int researchLevel, int att, int def)
            {
                throw new NotImplementedException();
            }
            private static int GenMoveInc(Game game, int researchLevel, int vision)
            {
                throw new NotImplementedException();
            }
            private static int GenMoveMax(Game game, int researchLevel, int vision, int inc)
            {
                throw new NotImplementedException();
            }
            private static int GenVision(Game game, int researchLevel)//IResearch research)
            {
                const float avgVision = 5.2f;
                float avg = avgVision, dev = .39f, oe = .169f;
                //bool isVision = research.GetType() == Type.MechVision;
                //ModValues(isVision, 1.7, ref avg, ref dev, ref oe);
                //avg *= research.GetMult(Type.MechVision, Blueprint_Vision_Pow);
                //if (isVision)
                //    avg += 1.3;
                return Game.Rand.GaussianOEInt(avg, dev, oe, 1);//, isVision ? Game.Rand.Round(Math.Sqrt(avg) + avgVision) : 1);
            }
            private static float GetAttDefStr(Game game, int researchLevel)//, float strMult, FocusStat focus)
            {
                //if (IsFocusing(focus, FocusStat.Cost))
                //    strMult *= FocusCostMult;
                return MakeStatStr(game, researchLevel);//, 2.1f, .585f);// randomize
            }
            private static float MakeStatStr(Game game, int researchLevel)//, float mult, float power)
            {
                return 1;
                ////all stats based initially on research
                //float str = game.Consts.GetResearchMult(researchLevel);// (Consts.ResearchFactor + research) / Consts.ResearchFactor;
                //str = (float)(Math.Pow(str, power) * mult);
                ////always add 1
                //return ++str;
            }

            private static BuildUnit CheckCost(BuildUnit buildUnit)//MechBlueprint blueprint, MechBlueprint upgrade, IResearch research, int blueprintNum, int researchLevel, bool alien)
            {
                return buildUnit;

                //if (float.IsNaN(minCost))
                //{
                //    minCost = GetMinCost(player.Game);
                //    maxCost = GetMaxCost(research, minCost);
                //}
                //else
                //{
                //    minCost = Math.Max(minCost, GetMinCost(player.Game));
                //}

                ////  ------  Cost/Upkeep       ------
                //float cost = -1, upkRnd = float.NaN;
                //int upkeep = -1;
                //GetCost(player.Game, upkeepPct, out cost, out upkeep, ref upkRnd, focus);
                //while (cost > maxCost)
                //{
                //    ModifyStat ms = GetReduce(cost, hpMult, forceColony, forceTrans);
                //    //Console.WriteLine(string.Format("Reduce: {0} {1} {2}", ms, cost, maxCost));
                //    switch (ms)
                //    {
                //        case ModifyStat.Att:
                //            --this._att;
                //            break;
                //        case ModifyStat.Def:
                //            --this._def;
                //            break;
                //        case ModifyStat.HP:
                //            --this._hp;
                //            break;
                //        case ModifyStat.Speed:
                //            --this._speed;
                //            break;
                //        case ModifyStat.Trans:
                //            --this._trans;
                //            break;
                //        case ModifyStat.DS:
                //            this._bombardDamage = (ushort)SetBombardDamage(this.BombardDamage - 1, this.Att, false);
                //            break;
                //        case ModifyStat.Colony:
                //            this._colony = false;
                //            break;
                //        case ModifyStat.None:
                //            maxCost = cost;
                //            break;
                //        default:
                //            throw new Exception();
                //    }
                //    GetCost(player.Game, upkeepPct, out cost, out upkeep, ref upkRnd, focus);
                //}
                //while (cost < minCost)
                //{
                //    ModifyStat ms = GetIncrease(hpMult);
                //    //Console.WriteLine(string.Format("Increase: {0} {1} {2}", ms, cost, minCost));
                //    switch (ms)
                //    {
                //        case ModifyStat.Att:
                //            ++this._att;
                //            this._bombardDamage = (ushort)SetBombardDamage(this._bombardDamage, this.Att, this.DeathStar);
                //            break;
                //        case ModifyStat.Def:
                //            ++this._def;
                //            break;
                //        case ModifyStat.HP:
                //            ++this._hp;
                //            break;
                //        case ModifyStat.None:
                //            break;
                //        default:
                //            throw new Exception();
                //    }
                //    GetCost(player.Game, upkeepPct, out cost, out upkeep, ref upkRnd, focus);
                //}
                //this._cost = (ushort)Game.Random.Round(cost);
                //this._upkeep = (byte)upkeep;
                //this._costNotInit = false;

                //Type researching = research.GetType();
                //int minTotal, maxTotal;
                //{
                //    minTotal = research.GetMinCost();
                //    maxTotal = research.GetMaxCost();
                //    if (upgrade != null)
                //    {
                //        //avg??
                //        minTotal = Math.Max(minTotal, Game.Rand.Round(upgrade.TotalCost() * 0.65));
                //        maxTotal = Math.Min(maxTotal, Game.Rand.Round(upgrade.TotalCost() * 1.69));
                //    }
                //    if (researching != Type.Mech)
                //    {
                //        const float minDev = .13;
                //        minTotal = Game.Rand.GaussianCappedInt(minTotal, minDev);
                //        const float maxDev = .21, maxOE = .169;
                //        if (maxTotal > minTotal)
                //            maxTotal = Game.Rand.GaussianOEInt(maxTotal, maxDev, maxOE, minTotal);
                //        else
                //            maxTotal = Game.Rand.Round(minTotal * Game.Rand.Range(1, 1 + Game.Rand.Weighted(.13)));
                //    }
                //}

                //int oldCost = blueprint.TotalCost();
                //bool canKeep = true;
                //while (blueprint.TotalCost() < minTotal && (canKeep &= ModStat(true))) ;
                //while (blueprint.TotalCost() > maxTotal && (canKeep &= ModStat(false))) ;

                //int newCost = blueprint.TotalCost();
                //if (oldCost != newCost)
                //    Debug.WriteLine($"blueprint ({(blueprint.BlueprintNum == "" ? "Alien" : blueprint.BlueprintNum)}) {oldCost} -> {newCost}");

                //if ((!canKeep || research.GetType() == Type.Mech || Game.Rand.Bool()) && (newCost < minTotal || newCost > maxTotal))
                //    blueprint = GenBlueprint(upgrade, research, blueprintNum, researchLevel, alien);

                //return blueprint;

                //bool ModStat(bool increase)
                //{
                //    Debug.WriteLine($"ModStat: {blueprint.TotalCost()} ({minTotal}-{maxTotal})");

                //    int inc = 1;

                //    //variables for relative chances
                //    float vision = blueprint.Vision;
                //    float resilience = blueprint.Resilience;
                //    float moveInc = blueprint.Movable.MoveInc;
                //    float moveMax = blueprint.Movable.MoveMax;
                //    float moveLimit = blueprint.Movable.MoveLimit;
                //    float[] def = blueprint.Killable.Select(k => (float)k.Defense).ToArray();
                //    float[] att = blueprint.Attacker.Select(a => (float)a.Attack).ToArray();
                //    float[] reload = blueprint.Attacker.Select(a => (float)a.Reload).ToArray();
                //    float[] range = blueprint.Attacker.Select(a => (float)a.Range).ToArray();

                //    if (increase)
                //    {
                //        //boundary conditions
                //        if (moveInc + inc >= moveMax)
                //            moveInc = 0;
                //        if (moveMax + inc >= moveLimit)
                //            moveMax = 0;
                //        for (int a = 0; a < att.Length; a++)
                //        {
                //            if (reload[a] + inc >= att[a])
                //                reload[a] = 0;
                //            if (range[a] == Attack.MELEE_RANGE)
                //                range[a] = 0;
                //        }
                //    }
                //    else
                //    {
                //        //boundary conditions
                //        if (vision - inc <= 1)
                //            vision = 0;
                //        if (moveInc - inc <= 1)
                //            moveInc = 0;
                //        if (moveMax - inc <= moveInc)
                //            moveMax = 0;
                //        if (moveLimit - inc <= moveMax)
                //            moveLimit = 0;
                //        for (int b = 0; b < def.Length; b++)
                //            if (def[b] - inc <= 1)
                //                def[b] = 0;
                //        for (int c = 0; c < att.Length; c++)
                //        {
                //            if (att[c] - inc < reload[c]) //allow dropping to 1
                //                att[c] = 0;
                //            if (reload[c] - inc < 1) //allow dropping to 1
                //                reload[c] = 0;
                //            if (range[c] - inc <= Attack.MIN_RANGED * 2) //buffer to allow inc by up to MIN_RANGED
                //                range[c] = 0;
                //        }

                //        //offsets
                //        vision -= 1;
                //        moveInc -= 1;
                //        moveMax -= 2;
                //        moveLimit -= 3;
                //        def = def.Select(d => d - 2).ToArray();
                //        att = att.Select(a => a - 1).ToArray();
                //        reload = reload.Select(r => r - 1).ToArray();
                //        range = range.Select(r => r - Attack.MIN_RANGED).ToArray();
                //    }

                //    //weight multipliers 
                //    const float resilienceMult = 16.9, rangeMult = .39;
                //    resilience *= resilienceMult; //resilienceMult used in inc
                //    reload = reload.Select(r => r * (increase ? 3.9 : 6.5)).ToArray(); //inc is unaffected
                //    range = range.Select(r => r * rangeMult).ToArray(); //inc is also higher 
                //    moveMax /= 2; //inc is unaffected
                //    moveLimit /= 3; //inc is unaffected

                //    int GetChance(float value)
                //    {
                //        if (value < 0)
                //            value = 0;
                //        else if (!increase)
                //            value *= value; //if decreasing, favor extreme values
                //        return Game.Rand.Round(value);
                //    }

                //    float newVision = blueprint.Vision, newResilience = blueprint.Resilience;
                //    var newKillable = blueprint.Killable.ToArray();
                //    var newAttacker = blueprint.Attacker.ToArray();
                //    var newMovable = blueprint.Movable;

                //    if (!increase)
                //        inc *= -1;

                //    Action IncVision = () => newVision += inc;
                //    Action IncResilience = () =>
                //    {
                //        if (increase)
                //            newResilience = 1 - newResilience;
                //        newResilience -= newResilience / resilienceMult;
                //        if (increase)
                //            newResilience = 1 - newResilience;
                //    };
                //    Action IncMoveInc = () => IncMovable(inc, 0, 0);
                //    Action IncMoveMax = () => IncMovable(0, inc, 0);
                //    Action IncMoveLimit = () => IncMovable(0, 0, inc);
                //    void IncMovable(int moveInc, int moveMax, int moveLimit) =>
                //        newMovable = new(blueprint.Movable.MoveInc + moveInc, blueprint.Movable.MoveMax + moveMax, blueprint.Movable.MoveLimit + moveLimit);

                //    Dictionary<Action, int> chances = new() {
                //        { IncVision, GetChance(vision) },
                //        { IncResilience, GetChance(resilience) },
                //        { IncMoveInc, GetChance(moveInc) },
                //        { IncMoveMax, GetChance(moveMax) },
                //        { IncMoveLimit, GetChance(moveLimit) },
                //    };

                //    for (int d = 0; d < blueprint.Killable.Count; d++)
                //    {
                //        int e = d; //capture loop variable
                //        chances.Add(() =>
                //            newKillable[e] = new(newKillable[e].Type, newKillable[e].Defense + inc),
                //            GetChance(def[e]));
                //    }
                //    for (int f = 0; f < blueprint.Attacker.Count; f++)
                //    {
                //        int g = f; //capture loop variable
                //        chances.Add(() =>
                //            IncAttacker(inc, 0, 0),
                //            GetChance(att[g]));
                //        chances.Add(() =>
                //            IncAttacker(0, inc * Game.Rand.Range(1, Attack.MIN_RANGED), 0), //equivalent to 0.350116223 rangeMult
                //            GetChance(range[g]));
                //        chances.Add(() =>
                //            IncAttacker(0, 0, inc),
                //            GetChance(reload[g]));
                //        void IncAttacker(int incAtt, float incRange, int incReload) =>
                //            newAttacker[g] = new(newAttacker[g].Type, newAttacker[g].Attack + incAtt, newAttacker[g].Range + incRange, newAttacker[g].Reload + incReload);
                //    }

                //    Action Inc = Game.Rand.SelectValue(chances);
                //    Inc();

                //    blueprint = new(blueprintNum, blueprint.UpgradeFrom, blueprint.ResearchLevel, newVision, newKillable, newResilience, newAttacker, newMovable);
                //    return true;
                //}
            }
            //private static IAttacker.Values UpgAttack(IAttacker.Values oldAttack, AttackType type, int att, float range) =>
            //    new(type, att, range, oldAttack.Attack == att ? oldAttack.Reload : null);

            //private static void ModValues(bool match, float mult, ref float avg, ref float dev, ref float oe)
            //{
            //    if (match)
            //    {
            //        avg *= mult;
            //        mult = Math.Sqrt(mult);
            //        dev /= mult;
            //        oe *= mult;
            //    }
            //}
        }
    }
}
