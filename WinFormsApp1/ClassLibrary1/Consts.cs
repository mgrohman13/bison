﻿using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using MattUtil;
using ClassLibrary1.Pieces;
using ClassLibrary1.Pieces.Enemies;
using ClassLibrary1.Pieces.Players;

namespace ClassLibrary1
{
    public static class Consts
    {
        public const double MapCoordSize = 16.9;
        public const double MapDev = .13;
        public const int MinMapCoord = 9;
        public const double ResourceAvgDist = 26;

        public const double ResearchFactor = 6500;

        public const double MoveDev = .013;
        public const double MoveLimitPow = 1.3;
        public const double ShielDev = .065;
        public const double ShieldLimitPow = 1.69;

        public const double ResourceDistAdd = 26;
        public const double ResourceDistDiv = 78;
        public const double ResourceDistPow = .39;
        public const double ResourceSustainValuePow = .169;
        public const double ExtractTurns = 65;
        public const double ExtractPower = .65 / (1 - .65); // x/(1-x) where x is desired power when sustain=1
        public const double ExtractSustainPow = .13;
        public const double ResourceDev = .21;
        public const double ResourceOE = .26;

        public const double BiomassEnergyInc = 100;
        public const double BiomassSustain = .75;
        public const double BiomassResearchIncDiv = 50;
        public const double MetalMassInc = 50;
        public const double MetalSustain = 1.25;
        public const double MetalEnergyUpkDiv = 10;
        public const double ArtifactResearchInc = 10;
        public const double ArtifactSustain = 1.75;
        public const double ArtifactMassIncDiv = 3;
        public const double ArtifactEnergyUpkDiv = 1;

        public const double BiomassExtractorEnergyCost = 250;
        public const double BiomassExtractorMassCost = 250;
        public const double MetalExtractorEnergyCost = 500;
        public const double MetalExtractorMassCost = 100;
        public const double ArtifactExtractorEnergyCost = 1000;
        public const double ArtifactExtractorMassCost = 750;

        public const double EnergyPerMass = 20;
        public const double MassPerEnergy = 3;
        public const double ResearchPerMass = 1;

        public const double BaseConstructorUpkeep = 5;
        public const double BaseMechUpkeep = 1;
        public const double WeaponRechargeUpkeep = 3;
        public const double UpkeepPerShield = 2;
        public const double UpkeepPerMove = .5;

        public static double IncValueWithMaxLimit(double cur, double inc, double dev, double max, double limit, double pow, bool rand)
        {
            double start = cur;
            if (inc > 0)
            {
                double startMax = Math.Max(cur, max);

                if (rand)
                    inc = Game.Rand.GaussianCapped(inc, dev, dev);
                cur += inc;

                double extra = cur - startMax;
                if (extra > 0)
                {
                    limit -= startMax;
                    double mult = limit / (limit + max);
                    extra *= Math.Pow(mult, pow);
                    extra += startMax;

                    cur = extra;
                }

                //Debug.WriteLine(cur);
            }
            return cur - start;
        }
    }
}
