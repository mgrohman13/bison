using game2.game;
using game2.sides;

namespace game2.runes.pattern
{
    internal class Trade : IRunePattern<Trade>
    {
        public readonly Player Player;

        public readonly Resources main;
        public readonly List<Resources> choices;

        public readonly (int min, int max) Charges;

        public Trade(Player player, (int min, int max) charges, Resources main, List<Resources> choices)
        {
            this.Player = player;
            this.Charges = charges;
            this.main = main;
            this.choices = choices;
        }

        public static Trade NewPattern(Player player, int researchLevel, float runeValue, int? forceCharges = null)
        {
            const float baseRate = 3.5f, amtMult = .5f, amtDeviation = .078f; //TODO: randomize 
            Consts consts = player.Game.Consts;

            (int resourceIdx, bool direction) = SelectTrade(player);

            float researchMult = consts.GetResearchMult(researchLevel);
            float efficiency = (researchMult + baseRate) / researchMult;

            int min = Game.Rand.Round(Math.Sqrt(consts.ResourceValue.Max()));
            int mainAmt = Game.Rand.GaussianCappedInt(Math.Sqrt(min * min
                + amtMult * GetValue() / consts.ResourceValue[resourceIdx]), amtDeviation, min);
            if (!direction)
            {
                mainAmt *= -1;
                efficiency = 1f / efficiency;
            }
            float tradeValue = (mainAmt * consts.ResourceValue[resourceIdx] - runeValue) * -efficiency;

            Resources main = new();
            main[resourceIdx] = mainAmt;

            List<Resources> choices = [];
            for (int a = 0; a < Resources.NumMapResources; a++)
                if (a != resourceIdx)
                {
                    Resources choice = new();
                    choice[a] = Game.Rand.Round(tradeValue / consts.ResourceValue[a]);
                    choices.Add(choice);
                }

            return new Trade(player, (1, 1), main, choices); //TODO: charges

            float GetValue()
            {
                float[] values = consts.ResourceValue;
                float resourceValue = values[resourceIdx];
                values[resourceIdx] = 0;
                resourceValue *= values.Max() * baseRate;
                return resourceValue + runeValue;
            }
        }
        private static (int resourceIdx, bool direction) SelectTrade(Player player)
        {
            Consts consts = player.Game.Consts;

            var allTrades = player.Deck.Select(r => r.Pattern as Trade).Where(t => t != null)
                .SelectMany(t => t!.choices.Select(c => t.main + c));

            Dictionary<(int resourceIdx, bool direction), int> chances = new();

            const float baseChanceOffset = 1.5f, positiveMult = 5f, negativeMult = 3f; //TODO: randomize 
            float[] positiveExponents = [3f, 1.5f, 1f, 2.5f]; //TODO: randomize 
            float[] negativeExponents = [2f, 2f, 3f, 1f]; //TODO: randomize 
            for (int b = 0; b < Resources.NumMapResources; b++)
            {
                var filtered = allTrades.Select(r => r[b]).Where(v => v != 0).Select(v =>
                {
                    return (float?)(v * consts.ResourceValue[b]);
                });
                float positive = filtered.Where(v => v > 0).Sum() ?? 0;
                float negative = -filtered.Where(v => v < 0).Sum() ?? 0;
                chances.Add((b, true), GetChance(positive, positiveMult, positiveExponents[b]));
                chances.Add((b, false), GetChance(negative, negativeMult, negativeExponents[b]));
            }

            (int resourceIdx, bool direction) r = Game.Rand.SelectValue(chances);

            int GetChance(float amt, float mult, float exp) =>
                Game.Rand.Round(BaseChance(amt) * (amt == 0f ? Math.Pow(mult, exp) : 1f));
            float BaseChance(float amt) => 1f + ushort.MaxValue / (amt + baseChanceOffset * consts.ResourceValue.Max());
            return r;
        }

        public RuneShape NewShape() => new(Player, this, main, Charges);

        public bool CanPlay(Rune rune) => choices.Any(Player.HasResources);
        public void Play(Rune rune, object? target)
        {
            throw new NotImplementedException();
        }
        public (bool play, object target) HandleChoice(IChoiceHandler handler)
        {
            (bool play, int choiceIdx) = handler.SelectResource(choices);
            Resources choice = choices[choiceIdx];
            if (!Player.HasResources(choice))
                play = false;
            return (play, choice);
        }

        public IEnumerable<IRuneEffect>? GetEffects() => [];
        //{
        //    throw new NotImplementedException();
        //}
    }
}
