using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MattUtil;
using Ideas;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Drawing;
using Point = MattUtil.Point;

namespace randTest
{
    class Program
    {
        static MTRandom rand = new MTRandom();

        static float _prodMult, _soldierInc;

        static void dot(double variation)
        {
            double prodMult = rand.Gaussian(1, 0.169 * Math.Pow((1 - variation) / variation, .5));
            double soldierInc = rand.Weighted(.091, .13);
            checked
            {
                _prodMult = (float)(_prodMult * (1 - variation) + prodMult * variation);
                _soldierInc = (float)(_soldierInc * (1 - variation) + soldierInc * variation);
            }
        }

        [STAThread]
        static void Main(string[] args)
        {
            rand.StartTick();


            //while (true)
            //{
            //    int style = rand.Next(6);

            //    int width = 4096 / 2;
            //    int height = 2160 / 2;

            //    int channels = style > 2 ? 3 : 1;
            //    Noise[] ns = new Noise[channels];
            //    double[] thresholds = new double[channels];
            //    bool[] flags = new bool[channels];
            //    for (int c = 0; c < channels; c++)
            //    {
            //        double max = rand.GaussianOE(width, .13, .13, height);

            //        double min = rand.GaussianCapped(26, .039, 13);
            //        int steps = rand.GaussianOEInt(13, .13, .13, 8);
            //        double weightScale = rand.Weighted(.91);
            //        double weightPower = rand.GaussianOE(6.5, .13, .13, 3.9);
            //        ns[c] = new Noise(rand, min, max, steps, .052, weightScale, weightPower);
            //        thresholds[c] = rand.GaussianCapped(.5, .091);
            //        flags[c] = rand.Bool();
            //    }
            //    if (channels == 3 && (rand.Bool() || rand.Bool()))
            //    {
            //        if (rand.Bool()) flags[2] = false;
            //        flags[0] = flags[1] = flags[2];
            //    }

            //    void eval(int idx, int y, int x, ref int r)
            //    {
            //        double e = ns[idx].Evaluate(x, y);
            //        double t = thresholds[idx];
            //        if (style == 0 || style == 3)
            //        {
            //            r = e > t ? 255 : 0;
            //        }
            //        else
            //        {
            //            if (style == 1 || style == 4)
            //                e = e > t ? (e - t) / (1 - t) : 0;
            //            r = (int)(e * 256);
            //        }
            //        if (flags[idx]) r = 255 - r;
            //    }

            //    Bitmap image = new Bitmap(width, height);
            //    for (int y = 0; y < height; y++)
            //        for (int x = 0; x < width; x++)
            //        {
            //            int r = -1, g = -1, b = -1;
            //            eval(0, y, x, ref r);
            //            if (channels == 3)
            //            {
            //                eval(1, y, x, ref g);
            //                eval(2, y, x, ref b);
            //            }
            //            if (channels == 3)
            //                image.SetPixel(x, y, Color.FromArgb(r, g, b));
            //            else
            //                image.SetPixel(x, y, Color.FromArgb(r, r, r));
            //        }
            //    string filename = rand.NextUInt().ToString() + ".bmp";
            //    image.Save(filename);
            //    Console.WriteLine(filename);
            //Console.ReadKey(true);

            ////if (channels == 1)
            ////{
            //string path = "2273235993.txt";
            ////TBSUtil.SaveGame(ns[0], ".", path);
            //ns[0] = TBSUtil.LoadGame<Noise>(path);

            //Bitmap image = new Bitmap(width, height);
            //for (int y = 0; y < height; y++)
            //    for (int x = 0; x < width; x++)
            //    {
            //        int r = -1, g = -1, b = -1;
            //        eval(0, y, x, ref r);
            //        if (channels == 3)
            //        {
            //            eval(1, y, x, ref g);
            //            eval(2, y, x, ref b);
            //        }
            //        if (channels == 3)
            //            image.SetPixel(x, y, Color.FromArgb(r, g, b));
            //        else
            //            image.SetPixel(x, y, Color.FromArgb(r, r, r));
            //    }
            //string filename = rand.NextUInt().ToString() + ".bmp";
            //image.Save(filename);
            //Console.WriteLine(filename);
            ////}

            //}
            //return;


            //Console.ReadKey(true);

            //int iter = rand.GaussianOEInt(Console.BufferHeight, .13, .13);

            //double min = rand.GaussianCapped(13, .13, 10.4);
            //double max = iter / 1.3;
            //max = rand.GaussianOE(max, .13, .13, min * 5.2);
            //int steps = rand.GaussianOEInt(5.2, .13, .13, 3);
            //Noise n = new Noise(rand, min, max, steps, .13);

            //for (int a = 0; a < n.Steps; a++)
            //    Console.WriteLine(n.divs[a]);
            //Console.WriteLine();
            //Console.ReadKey(true);

            //int[] vals = new int[101];

            //bool color = false;
            //for (int b = 0; b < iter; b++)
            //    for (int a = 0; a < Console.BufferWidth; a++)
            //    {
            //        double sum = n.Evaluate(a, b);

            //        //sum = Math.Sign(sum) * Math.Pow(Math.Abs(sum), 2 / (steps + 1.0));
            //        //vals[50 + rand.Round(sum * 50)]++;
            //        //Console.WriteLine(sum);

            //        bool on = sum > .5;
            //        if (on != color)
            //        {
            //            color = on;
            //            if (color)
            //                Console.BackgroundColor = ConsoleColor.White;
            //            else
            //                Console.BackgroundColor = ConsoleColor.Black;
            //        }
            //        Console.Write(' ');
            //    }

            ////for (int d = 0; d < 101; d++)
            ////{
            ////    Console.Write((d - 50) / 50.0);
            ////    Console.Write('\t');
            ////    Console.WriteLine(vals[d]);
            ////}
            //Console.ReadKey(true);
            //return;


            //int max = 10;
            //int[] res = new int[max + 1];
            //double iter = 10000000, weight = .51, sum = 0;
            //for (int a = 0; a < iter; a++)
            //{
            //    int v = rand.WeightedInt(max, weight);
            //    res[v]++;
            //    sum += v;
            //}
            //Console.WriteLine(sum / iter);
            //Console.WriteLine();
            //foreach (int v in res)
            //    Console.WriteLine(v);
            //Console.ReadKey();
            //return;


            //rand.NextUInt();
            //rand.StartTick();
            //rand.NextUInt();
            //Console.ReadKey(true);

            ////int b = 0;
            ////while (++b > 0)
            ////{
            ////    Console.WriteLine();
            ////    Write(rand.NextUInt());
            ////}

            ////for (ushort a = 0 ; a <= MTRandom.MAX_SEED_SIZE ; ++a)
            ////    try
            ////    {
            ////        rand.StoreSeed = true;
            ////        rand.SetSeed(MTRandom.GenerateSeed(a));
            ////        Write(rand.Seed[0]);
            ////        Console.WriteLine();
            ////        Write(rand.NextUInt());
            ////        Console.WriteLine();
            ////        Console.WriteLine();
            ////    }
            ////    catch { }

            ////while (true)
            ////{
            ////    try
            ////    {
            ////        rand.GaussianOE(rand.OE(), rand.Weighted(1 / 3.0), rand.Weighted(1 / 3.0), rand.OE() / 3.0);
            ////    }
            ////    catch
            ////    {
            ////    }
            ////}


            //bool ver = rand.Bool();

            //SeedTest(true, ver);

            //int tot = 0, c = 0;
            //while (rand.Next(100) != 0)
            //{
            //    c++;
            //    tot += SeedTest(false, ver);
            //}

            //Console.WriteLine();
            //Console.WriteLine();
            //Console.WriteLine(( tot / (double)c ) / ( 16.0 * ( MTRandom.LENGTH + 4 ) ));

            //if (!ver)
            //{
            //    tot = c = 0;
            //    while (rand.Next(10000) != 0)
            //    {
            //        c++;
            //        tot += rand.GaussianInt() == 0 ? 0 : 1;
            //    }
            //    Console.WriteLine(tot / (double)c);
            //}
            //else
            //{
            //    Console.WriteLine((double)1);
            //}

            //SeedTest(true, ver);


            //for (int f = 0 ; f < 2 ; ++f)
            //{
            //    double pd = .05;
            //    double sd = .005;

            //    var pms = new SortedDictionary<int, int>();
            //    var sis = new SortedDictionary<int, int>();

            //    for (int b = 0 ; b < 10000 ; ++b)
            //    {
            //        dot(1);
            //        int c = 1;
            //        if (f == 1)
            //            c = 100;// rand.GaussianOEInt(39, 1, .52, 2);
            //        for (int a = 0 ; a < c ; ++a)
            //            dot(1.0 / c);

            //        int v1 = rand.Round(_prodMult / pd);
            //        int v2 = rand.Round(_soldierInc / sd);
            //        int t;
            //        pms.TryGetValue(v1, out t);
            //        pms[v1] = t + 1;
            //        sis.TryGetValue(v2, out t);
            //        sis[v2] = t + 1;
            //    }

            //    foreach (var dict in new SortedDictionary<int, int>[] { pms })//, sis })
            //    {
            //        double s = 0;
            //        foreach (var pair in dict)
            //            s += Math.Pow(Math.Abs(pair.Key * pd - 1), 2) * pair.Value;
            //        Console.WriteLine(Math.Sqrt(s) / 100);

            //        //int e = dict.Keys.Max();
            //        //for (int d = 0 ; d < e ; ++d)
            //        //{
            //        //    int t;
            //        //    dict.TryGetValue(d, out t);
            //        //    Console.Write(d * pd);
            //        //    Console.Write(" ");
            //        //    Console.WriteLine(t);
            //        //}
            //        //Console.WriteLine();
            //    }

            //    //Console.WriteLine();
            //}

            //Console.ReadKey();
            //return;


            //rand.StartTick();

            //const float gameSize = 1500f, Difficulty = 1f;
            //foreach (float d in new float[] { 0f, .1f, .25f, .5f, .75f, 1f, 1.25f, (float)Math.Sqrt(2), 1.5f, 2f, 3f, 4f, 5f, 10f })
            //{
            //    float dist = d * gameSize / 2f;
            //    const float gameSizeSqr = gameSize * gameSize;
            //    float val = (float)Math.Pow(((dist * dist) * 4f + gameSizeSqr) / (gameSizeSqr * 3f), .25f) - 1f;
            //    float cur = (float)Math.Sqrt(2.5f + 1f);
            //    val = ((float)(.005f * Math.Log(Math.Abs(val) + 1f) * (val < 0 ? Math.Sqrt(Difficulty) / cur : -cur / 25f)));
            //    Console.WriteLine(dist + "\t" + val.ToString("+0.00000000000;-0.00000000000"));
            //}
            //Console.ReadKey();
            //return;


            //const float AlienShipFriendlyBulletDamageMult = 13f;
            //const float BulletDamage = 3.9f;
            //const float amt = AlienShipFriendlyBulletDamageMult * BulletDamage;
            //const float AlienShipLife = 260f;
            //const float AlienDamageRandomness = .078f;
            //const float AlienDamageOEPct = .26f;

            //Console.WriteLine(amt.ToString().PadLeft(4) + " (" + ( AlienShipLife / amt ).ToString("0.0") + ") ");
            //Console.WriteLine();

            //SortedDictionary<int, int> cnt = new SortedDictionary<int, int>();
            //const int tot = 1000000;
            //for (int a = 0 ; a < tot ; ++a)
            //{
            //    float dmg = rand.GaussianOE(amt, AlienDamageRandomness, AlienDamageOEPct);
            //    int rnded = rand.Round(dmg);
            //    int val;
            //    cnt.TryGetValue(rnded, out val);
            //    cnt[rnded] = val + 1;
            //}
            //int culm = tot;
            //foreach (KeyValuePair<int, int> p in cnt)
            //{
            //    //if (p.Value >= tot / 10000)
            //    Console.WriteLine(p.Key.ToString().PadLeft(3) + "  (" + ( AlienShipLife / p.Key ).ToString("0.0") + ") "
            //            + p.Value.ToString().PadLeft(7) + " " + culm.ToString().PadLeft(7));
            //    culm -= p.Value;
            //}


            //for (int testiter = 0 ; testiter < 1 ; ++testiter)
            //{
            //    Console.WriteLine(testiter);
            //    const int g = 100;
            //    int[] c = new int[g];
            //    for (int a = 0 ; a < 1000000 ; ++a)
            //    {
            //        //double weight = rand.DoubleHalf(1);
            //        //for (int b = 0 ; b < testiter ; ++b)
            //        //    weight = rand.Weighted(weight);
            //        //c[rand.WeightedInt(g - 1, weight)]++;

            //        c[rand.WeightedInt(g - 1, rand.Weighted(rand.DoubleHalf(1)))]++;

            //    }
            //    int runtot = 0;
            //    for (int b = 0 ; b < g ; ++b)
            //        Console.WriteLine(runtot += c[b]);
            //    Console.WriteLine();
            //}
            //Console.ReadKey();
            //return;


            ////new RandBooleans(rand, .5);

            //Console.WriteLine("{0}{1}", ( 0.5 / NormSDist(-1 / RandBooleans.GaussianDeviation) ).ToString().PadRight(21), RandBooleans.GaussianDeviation);
            //float lifeDustBond = (float)( Math.E / 13.0 );
            //Console.WriteLine("{0}{1}", ( 0.5 / NormSDist(-1 / lifeDustBond) ).ToString().PadRight(21), lifeDustBond);
            //Console.WriteLine();

            //double change = rand.Weighted(1.0 / 13);
            //Console.WriteLine(change);
            //Console.WriteLine();
            //bool dir = rand.Bool();
            //int f1 = 0, fm1 = 0, f2 = 0, fm2 = 0, t1 = 0, tm1 = 0, t2 = 0, tm2 = 0;
            //double trg = rand.DoubleHalf();
            //Console.WriteLine(trg);
            //RandBooleans rb = new RandBooleans(rand, trg);

            //int t = 0, f = 0;

            //while (true)
            //{
            //    if (rand.Bool(change))
            //    {
            //        rb.Chance = trg = rand.DoubleHalf();


            //        Console.WriteLine(t / (double)( t + f ));
            //        Console.WriteLine(t + f);
            //        Console.WriteLine();
            //        t = 0;
            //        f = 0;
            //        Thread.Sleep(1300);

            //        Console.WriteLine(trg);

            //    }
            //    bool b = ( rb.GetResult() );
            //    bool r = ( rand.Bool(trg) );
            //    if (b)
            //    {
            //        ++t;
            //        f1 = 0;
            //        if (++t1 > tm1)
            //        {
            //            tm1 = t1;
            //            //Console.WriteLine("bt: " + t1);
            //        }
            //    }
            //    else
            //    {
            //        ++f;
            //        t1 = 0;
            //        if (++f1 > fm1)
            //        {
            //            fm1 = f1;
            //            //Console.WriteLine("bf: " + f1);
            //        }
            //    }
            //    if (r)
            //    {
            //        f2 = 0;
            //        if (++t2 > tm2)
            //        {
            //            tm2 = t2;
            //            //Console.WriteLine("rt: " + t2);
            //        }
            //    }
            //    else
            //    {
            //        t2 = 0;
            //        if (++f2 > fm2)
            //        {
            //            fm2 = f2;
            //            //Console.WriteLine("rf: " + f2);
            //        }
            //    }

            //    Console.WriteLine("{0}    {1}", ( dir ? b : r ).ToString().PadRight(5), dir ? r : b);
            //    Thread.Sleep(1300);
            //}


            //float f1 = rand.OEFloat();
            //float f2 = rand.OEFloat();
            //Console.WriteLine(f1);
            //Console.WriteLine(f2);
            //Console.WriteLine(f1 * f2);
            //Console.ReadKey(true);
            //while (true)
            //{
            //    float mult = 1f + ( ( rand.FloatFull() - 1f ) / ( 1 << 23 ) );
            //    float f3 = f1 * mult;
            //    mult = 1f + ( ( rand.FloatFull() - 1f ) / ( 1 << 23 ) );
            //    float f4 = f2 * mult;
            //    Console.WriteLine(f3 * f4);
            //    Console.ReadKey(true);
            //}

            //for (int a = 1; a < 9; a++)
            //{
            //    //Console.WriteLine(a);
            //    //Console.WriteLine(NormSDist(a));
            //    Console.WriteLine(1 - (1 - NormSDist(a)) * 2);
            //    //Console.WriteLine();
            //}
            //Console.WriteLine(NormSDist(3));
            //Console.ReadKey(true);
            //NormSDist();

            //NCWDist();

            //XComShipSim();

            //DaeReserveMorale();

            //TickTest();

            //SeedTest();

            //CWPortalStart();

            //IterateTest();

            //BinarySearchSqrt();

            //CWMapGen();

            //GenerateTerrain();
            //Console.ReadKey(true);

            //rand = null;
            //while (true)
            //{
            //    Thread.Sleep(1);
            //    GC.Collect();
            //}
            //for (int a = 0 ; a < 13 ; ++a)
            //    new MTRandom().StartTick(0);

            //do
            //{
            //    Console.WriteLine();
            //    FactRand(new float[] { 1 / 3f, 3f, 1f }, 3f / 12f, 4f / 12f);
            //} while (Console.ReadKey(true).KeyChar != 'q');


            lotr();


            Console.ReadKey(true);
            rand.StopTick();
        }

        private static void lotr()
        {
            //double r = 0, c = 1000000;
            //Dictionary<int, int> res = new Dictionary<int, int>();
            //Dictionary<int, int> r2 = new Dictionary<int, int>();
            //for (int a = 0; a < c; a++)
            //{
            //    Dictionary<int, int> v;
            //    int e = lotr(1, 0, 0);
            //    int o1 = lotr(1, 0, -1);
            //    int o2 = lotr(1, 0, -2);
            //    int remain, max = 6;
            //    if (e > o)
            //    {
            //        v = res;
            //        r2.TryGetValue(-1, out int d);
            //        r2[-1] = d + 1;
            //        if (o < 0)
            //        {
            //            int chance = lotr(1, 0, 0);
            //            o = (chance > -o + 1) ? 1 : 0;
            //        }
            //        remain = e - o;
            //    }
            //    else if (o > e)
            //    {
            //        max = 18;
            //        v = r2;
            //        res.TryGetValue(-1, out int d);
            //        res[-1] = d + 1;
            //        if (e < 0)
            //        {
            //            int chance = lotr(1, 0, 0);
            //            e = (chance > -e + 1) ? 1 : 0;
            //        }
            //        remain = o - e;
            //    }
            //    else
            //    {
            //        res.TryGetValue(-1, out int d);
            //        res[-1] = d + 1;
            //        r2.TryGetValue(-1, out int f);
            //        r2[-1] = f + 1;
            //        continue;
            //    }
            //    if (remain > max)
            //        remain = max;
            //    double r3 = max - remain;
            //    int r4 = rand.Round(r3 / 3);
            //    v.TryGetValue(r4, out int b);
            //    v[r4] = b + 1;

            //    //int e = lotr(1, 1, 0);
            //    //int o = lotr(1, 0, -2);
            //    //r += v;
            //    //res.TryGetValue(v, out int b);
            //    //res[v] = b + 1;
            //}
            ////Console.WriteLine(r / c);
            ////Console.WriteLine();
            //for (int d = res.Keys.Min(); d <= res.Keys.Max(); d++)
            //{
            //    res.TryGetValue(d, out int e);
            //    Console.WriteLine(d + "\t" + e / c);
            //}
            //Console.WriteLine();
            //for (int d = r2.Keys.Min(); d <= r2.Keys.Max(); d++)
            //{
            //    r2.TryGetValue(d, out int e);
            //    //if (d < 0) d--;
            //    //d++;
            //    Console.WriteLine((d < 0 ? d : d + 1) + "\t" + e / c);
            //}
        }
        private static void lotr(IEnumerable<LotrPiece> s1, IEnumerable<LotrPiece> s2)
        {

        }
        private static int lotr(int dice, int bonus, int offset)
        {
            int count = 0, value = 0, ones = 0, sixes = 0;
            while (dice > 0)
            {
                for (int a = 0; a < dice; a++)
                {
                    int res = rand.Next(6) + 1;
                    if (res == 6)
                        sixes++;
                    else if (res == 1)
                        ones++;
                    else
                    {
                        count++;
                        value += res;
                    }
                }
                dice = (sixes - ones) * 2;
                int pairs = Math.Min(ones, sixes);
                count += pairs * 2;
                value += pairs * 7;
                ones = sixes = 0;
            }
            if (count < 1)
                count = 1;
            else if (count != 1 && count != 3)
                ;
            return value + count * bonus + offset;
        }
        private class LotrPiece
        {
            public int dice, bonus, offset, damage;
            public bool alive;
            public LotrPiece(int dice, int bonus, int offset)
            {
                this.dice = dice;
                this.bonus = bonus;
                this.offset = offset;
            }
        }

        #region FactRand
        private static void FactRand(float[] scenario, float minDiff, float maxDiff)
        {
            float[] vals = new float[] { 1 / 6f, 1 / 4f, 1 / 3f, 1 / 2f, 3 / 4f, 1f, 4 / 3f, 3 / 2f, 2f, 3f, 4f, 6f };
            float tot = 1;
            float scenAvg = scenario.Aggregate((a, b) => a * b);
            string[] resources = new string[] { "iron", "copper", "stone", "coal", "uranium", "oil" };
            for (int a = 0; a < 6; ++a)
            {
                if (a == 0)
                    Console.WriteLine("res\tfreq\tsize\trich\ttot");
                Console.Write(resources[a] + '\t');
                float row = 1;
                bool match = true;
                for (int b = 0; b < 3; ++b)
                {
                    float mult;
                    do
                    {
                        float avg = scenario[b];
                        mult = rand.GaussianCapped(avg, .39f, Math.Min(vals[0], avg * avg / vals[vals.Length - 1]));
                        if (rand.Bool())
                            mult = avg * avg / mult;
                    } while (mult < vals[0] || mult > vals[vals.Length - 1]);
                    for (int c = 1; true; c++)
                    {
                        float max = vals[c];
                        if (max > mult)
                        {
                            float min = vals[c - 1];
                            if (min > mult)
                                throw new Exception();
                            float chance = (mult - min) / (max - min);
                            float val = (rand.Bool(chance) ? max : min);
                            row *= val;
                            Console.Write(string.Format("{0:P0}", val).PadLeft(4));
                            match &= (val == scenario[b]);
                            break;
                        }
                    }
                    Console.Write('\t');
                }
                Console.Write(row);
                Console.WriteLine();
                row /= scenAvg;
                tot *= row;
                double maxMult = Math.Sqrt(3);
                if (match || row < minDiff / maxMult || row > maxDiff * maxMult)
                {
                    tot = 1;
                    a = -1;
                    Console.WriteLine();
                }
                else if (a == 5)
                {
                    tot = (float)Math.Pow(tot, 1 / 6.0);
                    Console.WriteLine("tot\t\t\t\t" + tot);
                    if (tot > maxDiff || tot < minDiff)
                    {
                        tot = 1;
                        a = -1;
                        Console.WriteLine();
                    }
                }
            }
        }
        #endregion

        #region NCWDist
        static void NCWDist()
        {
            const int n = 4073;
            //1-CDF for a single Geometric Distribution
            double[] a1 = new double[n];
            a1[0] = 1;
            a1[1] = 1;
            for (int a = 2; a < n; ++a)
                a1[a] = a1[a - 1] * (5.0 / 6.0);
            //PMF for one
            double[] a2 = new double[n];
            for (int a = 0; a + 1 < n; ++a)
                a2[a] = a1[a] - a1[a + 1];
            //PMF for sum of two
            a1 = new double[n];
            for (int a = n - 1; a >= 0; --a)
                for (int b = n - 1 - a; b >= 0; --b)
                    a1[a + b] += a2[a] * a2[b];
            //account for final offset
            a2 = new double[n];
            for (int a = 1; a + 1 < n; ++a)
                a2[a] = (a1[a - 1] + a1[a + 1]) / 2.0;

            double s1 = 0, s2 = 0;
            for (int a = 0; a < n; a++)
            {
                s1 += a2[a];
                s2 += a * a2[a];
                if (a < Console.BufferHeight - 3)
                    Console.WriteLine("{0}\t{1}", a, a2[a]);//.ToString("E16"));
            }
            Console.WriteLine(-Math.Log(Math.Abs(s1 - 1)) / Math.Log(2));
            Console.WriteLine(-Math.Log(Math.Abs(s2 - 12) / 12.0) / Math.Log(2));
            Console.Write(MTRandom.DOUBLE_BITS);
        }
        #endregion //NCWDist

        #region NormSDist
        static double NormDense(double x, double mean, double stdDev)
        {
            return Math.Exp(Math.Pow(x - mean, 2.0) / (-2.0 * Math.Pow(stdDev, 2.0))) / (stdDev * Math.Sqrt(2.0 * Math.PI));
        }
        static double NormDenseInv(double y, double mean, double stdDev)
        {
            double retVal = stdDev * Math.Sqrt(2.0 * Math.Log(1.0 / (y * stdDev)) - Math.Log(2.0 * Math.PI));
            return mean + retVal; //alternatively: mean - retVal
        }
        static void NormSDist()
        {
            Console.WriteLine(NormDense(0, 0, 1));
            Console.WriteLine(0.5 / NormSDist(-2.1));
            Console.WriteLine();

            Console.WriteLine(1.0 / MTRandom.GAUSSIAN_MAX);
            Console.WriteLine(0.117 * MTRandom.GAUSSIAN_MAX);
            Console.WriteLine();

            for (int n = 2; n < 101; ++n)
                Console.WriteLine(TBSUtil.FindValue((v) => (0.5 / NormSDist(-1 / v)), n, 2, 0).ToString().PadRight(21) + n);
            Console.WriteLine();

            Action<double> Print = v => Console.WriteLine("{0}{1}", (0.5 / NormSDist(-1 / v)).ToString().PadRight(21), v);
            Print(3.0);
            Print(2.6);
            Print(2.1);
            Print(1.69);
            Print(1.3);
            Print(1.04);
            Print(1.0);
            Print(0.91);
            Print(0.78);
            Print(0.65);
            Print(0.52);
            Print(4.0 / 9.0);
            Print(0.39);
            Print(1.0 / 3.0);
            Print(0.3);
            Print(0.26);
            Print(0.21);
            Print(0.169);
            Print(0.13);
            Print(0.117);
            Print(0.104);
            Console.WriteLine();

            ////requires modification of FindValue conditional from (result <= target) to (result < target)
            ////8.2923610758167
            //Console.WriteLine(TBSUtil.FindValue(z => NormSDist(z), 1, 3.65, 10.9));
            //Console.WriteLine();

            const double excelMax = 8.29230302795755;
            Console.WriteLine(excelMax);
            Console.WriteLine(NormSDist(-excelMax));
            Console.WriteLine((-Math.Log(NormDense(excelMax, 0, 1), 2)) + "   Excel");
            Console.WriteLine(MTRandom.GAUSSIAN_MAX);
            Console.WriteLine(NormSDist(-MTRandom.GAUSSIAN_MAX));
            Console.WriteLine((-Math.Log(NormDense(MTRandom.GAUSSIAN_MAX, 0, 1), 2)) + "   MTRandom");
            const double normSDistMax = 8.2923610758167;
            Console.WriteLine(normSDistMax);
            Console.WriteLine(NormSDist(-normSDistMax));
            Console.WriteLine((-Math.Log(NormDense(normSDistMax, 0, 1), 2)) + "   NormSDist");
            Console.WriteLine();

            double val = double.NaN;
            for (int a = -3; a < 4; ++a)
            {
                double trg = 1.0 / Math.Pow(2.0, MTRandom.DOUBLE_BITS + a);
                Console.WriteLine(NormDenseInv(trg, 0, 1).ToString("0.00000000000000") + "   " + (MTRandom.DOUBLE_BITS + a));
                if (double.IsNaN(val))
                    val = NormDenseInv(trg, 0, 1);
            }
            Console.WriteLine();

            double min = 8, max = 10;
            double mid;
            while ((mid = max - min) != min && mid != max)
            {
                mid = min + mid / 2;
                if (mid == min || mid == max)
                    break;
                if (!double.IsNaN(AsymptoticSeries(-mid)))
                    max = mid;
                else if (true)
                    min = mid;
            }
            Console.WriteLine(max);//8.68271440764326
            Console.WriteLine(NormSDist(-max));
            Console.WriteLine(AsymptoticSeries(-max));
            Console.WriteLine((-Math.Log(NormDense(max, 0, 1), 2)) + "   AsymptoticSeries");
            Console.WriteLine();

            while (true)
            {
                double v3 = rand.OE(MTRandom.GAUSSIAN_MAX);

                double d1 = NormSDist(-v3), d2 = NormSDist(v3);

                Console.WriteLine(v3);
                Console.WriteLine(AsymptoticSeries(-v3));
                Console.WriteLine(d1);
                Console.WriteLine(d2);
                Console.WriteLine(d1 + d2);
                Console.WriteLine();

                Thread.Sleep(rand.OEInt(3900));
            }

            Console.ReadKey(true);
        }

        //static double ContinuedFraction(double z)
        //{
        //    bool sign = ( z > 0 );
        //    if (!sign)
        //        z = -z;
        //    const int cnt = 100000000;
        //    double f = 1;//0
        //    for (int n = cnt ; n > 0 ; --n)
        //    {
        //        f = n / ( ( ( n % 2 ) + 1.0 ) * z + f );
        //    }
        //    //Math.Sqrt(Math.PI) / 2.0 -
        //    double aln = 0.5 * Math.Exp(-z * z) / f;
        //    return ( sign ? 1.0 - aln : aln );
        //}

        //z>~8.7
        static double AsymptoticSeries(double z)
        {
            bool sign = (z > 0);
            if (!sign)
                z = -z;
            double coefficient = 1;
            double prev = double.NaN;
            double erf = 1.0 / z;
            for (int n = 1; prev != erf && !double.IsNaN(erf); ++n)
            {
                coefficient *= -(2.0 * n - 1.0);
                prev = erf;
                erf += coefficient / Math.Pow(z, 2.0 * n + 1);
            }
            double aln = erf * Math.Exp(-(z * z) / 2.0) / Math.Sqrt(2.0 * Math.PI);
            return (sign ? 1.0 - aln : aln);
        }

        //static double NormDist(double x, double mean, double stdDev)
        //{
        //    return NormSDist(x - mean) / stdDev;
        //}
        //ALGORITHM AS 66 APPL. STATIST. (1973) VOL.22, NO.3:
        //Hill,  I.D.  (1973).  Algorithm AS 66.  The normal  integral.
        //                      Appl. Statist.,22,424-427.
        //Compute values of the cumulative normal probability function
        //From G. Dallal's STAT-SAK (Fortran code)
        //Additional precision using asymptotic expression added 7/8/92
        //2/9/16: use Black-Scholes model for z<3.65
        //  add additional power series expression term and use for for z>10.9 (instead of 15)
        static double NormSDist(double z)
        {
            const double FORMULA_BREAK = 3.65; //Changes to cont. fraction
            const double UPPER_TAIL_IS_ZERO = 10.9; //Changes to power series expression
            double LOWER_TAIL_IS_ONE = MTRandom.GAUSSIAN_MAX; //I.e., alnorm(8.5,0) = .999999999999+

            double aln;
            bool up = (z < 0.0);
            if (up)
                z = -z;

            Func<double> y = () => Math.Exp(-0.5 * z * z) / Math.Sqrt(2.0 * Math.PI);

            //Evaluates the tail area of the standard normal curve from z to infinity if up, or from -infinity to z if not up
            if (z < LOWER_TAIL_IS_ONE || (up && z < UPPER_TAIL_IS_ZERO))
            {
                //double y = ;
                if (z > FORMULA_BREAK)
                    aln = y() /
                            (z - 3.8052E-8 + 1.00000615302 /
                            (z + 3.98064794E-4 + 1.98615381364 /
                            (z - 0.151679116635 + 5.29330324926 /
                            (z + 4.8385912808 - 15.1508972451 /
                            (z + 0.742380924027 + 30.789933034 /
                            (z + 3.99019417011))))));
                else
                    aln = 1.0 - BlackScholes(z);
            }
            else
            {
                if (up)
                {
                    //7/8/92
                    //Uses asymptotic expansion for exp(-z*z/2)/alnorm(z)
                    //Agrees with continued fraction to 11 s.f. when z >= 16 and coefficients through 706 are used
                    double w = 1.0 / (z * z); //1/z^2
                    aln = y() / (z * (1 + w * (1 + w * (-2 + w * (10 + w * (-74 + w * (706 + w * (-8162 + w * 110410))))))));
                    //https://oeis.org/A000698
                }
                else
                {
                    aln = 0.0;
                }
            }

            return up ? aln : 1.0 - aln;
        }
        static double BlackScholes(double z)
        {
            int sign = (z >= 0 ? 1 : -1);
            z = Math.Abs(z) / Math.Sqrt(2.0);
            double coefficient = 1.0;
            double prev = double.NaN;
            double erf = z;
            for (int n = 1; prev != erf; n++)
            {
                coefficient *= (-2.0 * n + 1.0) / (n * (2.0 * n + 1.0));
                prev = erf;
                erf += coefficient * Math.Pow(z, 2.0 * n + 1.0);
            }
            return 0.5 + sign * erf / Math.Sqrt(Math.PI);
        }

        #endregion NormSDist

        #region XComShipSim
        static void XComShipSim()
        {
            const int Beginner = 0;
            const int Experienced = 1;
            const int Veteran = 2;
            const int Genius = 3;
            const int Superhuman = 4;
            Func<XComShip> Cruiser = () => new XComShip(new Tuple<String, int, int>("Cruiser", 150, 4), new XComShip.Weapon(25, 35, 48));
            Func<XComShip> Escort = () => new XComShip(new Tuple<String, int, int>("Escort", 150, 4), new XComShip.Weapon(30, 13, 56));
            Func<XComShip> Hunter = () => new XComShip(new Tuple<String, int, int>("Hunter", 250, 3), new XComShip.Weapon(50, 18, 48));
            Func<XComShip> HeavyCruiser = () => new XComShip(new Tuple<String, int, int>("HeavyCruiser", 225, 3), new XComShip.Weapon(60, 20, 32));
            Func<XComShip> FleetSupplyCruiser = () => new XComShip(new Tuple<String, int, int>("FleetSupplyCruiser", 1000, 2), new XComShip.Weapon(70, 38, 24));
            Func<XComShip> Battleship = () => new XComShip(new Tuple<String, int, int>("Battleship", 700, 2), new XComShip.Weapon(120, 50, 24));
            Func<XComShip> Dreadnought = () => new XComShip(new Tuple<String, int, int>("Dreadnought", 1700, 1), new XComShip.Weapon(140, 60, 24));
            Func<Tuple<String, int>> Barracuda = () => new Tuple<String, int>("Barracuda", 120);
            Func<Tuple<String, int>> Manta = () => new Tuple<String, int>("Manta", 400);
            Func<Tuple<String, int>> Hammerhead = () => new Tuple<String, int>("Hammerhead", 960);
            Func<Tuple<String, int>> Leviathan = () => new Tuple<String, int>("Leviathan", 1250);
            Func<XComShip.Weapon> GasCannon = () => new XComShip.Weapon("GasCannon", 200, 15, 8, .25, 3);
            Func<XComShip.Weapon> AJAX = () => new XComShip.Weapon("AJAX", 6, 60, 32, .7, 24);
            Func<XComShip.Weapon> DUPHead = () => new XComShip.Weapon("DUPHead", 3, 110, 50, .8, 36);
            Func<XComShip.Weapon> GaussCannon = () => new XComShip.Weapon("GaussCannon", 50, 90, 20, .35, 18);
            Func<XComShip.Weapon> SonicOscillator = () => new XComShip.Weapon("SonicOscillator", 100, 150, 55, .5, 18);
            Func<XComShip.Weapon> PWTLauncher = () => new XComShip.Weapon("PWTLauncher", 2, 200, 60, 1, 24);

            XComShip.Weapon.setDifficulty(Superhuman);
            for (int numBs = 1; numBs <= 3; ++numBs)
            {
                //Console.WriteLine(numBs);
                //Console.WriteLine();
                //Console.WriteLine();
                XComShip[] xcom = new XComShip[numBs];
                for (int b = 0; b < numBs; ++b)
                    xcom[b] = new XComShip(Barracuda(), PWTLauncher(), PWTLauncher());
                XComShipSim(FleetSupplyCruiser(), xcom);
                XComShipSim(Battleship(), xcom);
                XComShipSim(Dreadnought(), xcom);
            }

            for (int a = 0; a < 5; ++a)
            {
                XComShip.Weapon.setDifficulty(a);

                XComShipSim(FleetSupplyCruiser(), new XComShip(Barracuda(), SonicOscillator(), SonicOscillator()));
                XComShipSim(Battleship(), new XComShip(Barracuda(), SonicOscillator(), SonicOscillator()));
                XComShipSim(Dreadnought(), new XComShip(Barracuda(), SonicOscillator(), SonicOscillator()),
                        new XComShip(Barracuda(), SonicOscillator(), SonicOscillator()));

                XComShipSim(Hunter(), new XComShip(Barracuda(), GasCannon(), GasCannon()));
                XComShipSim(HeavyCruiser(), new XComShip(Barracuda(), GasCannon(), GasCannon()));

                XComShipSim(FleetSupplyCruiser(), new XComShip(Barracuda(), GasCannon(), GasCannon()));
                XComShipSim(FleetSupplyCruiser(), new XComShip(Barracuda(), GaussCannon(), GaussCannon()));

                XComShipSim(Battleship(), new XComShip(Barracuda(), GasCannon(), GasCannon()), new XComShip(Barracuda(), GasCannon(), GasCannon()));
                XComShipSim(Battleship(), new XComShip(Barracuda(), DUPHead(), DUPHead()), new XComShip(Barracuda(), AJAX(), AJAX()));
                XComShipSim(Battleship(), new XComShip(Barracuda(), DUPHead(), DUPHead()), new XComShip(Barracuda(), DUPHead(), DUPHead()));
                XComShipSim(FleetSupplyCruiser(), new XComShip(Barracuda(), GasCannon(), GasCannon()), new XComShip(Barracuda(), GasCannon(), GasCannon()));
                XComShipSim(Dreadnought(), new XComShip(Barracuda(), GasCannon(), GasCannon()), new XComShip(Barracuda(), GasCannon(), GasCannon()),
                        new XComShip(Barracuda(), GasCannon(), GasCannon()));

                XComShipSim(FleetSupplyCruiser(), new XComShip(Manta(), GasCannon(), GasCannon()));
                XComShipSim(FleetSupplyCruiser(), new XComShip(Manta(), GaussCannon(), GaussCannon()));
                XComShipSim(Battleship(), new XComShip(Manta(), GasCannon(), GasCannon()));
                XComShipSim(Battleship(), new XComShip(Manta(), GaussCannon(), GaussCannon()));
                XComShipSim(Dreadnought(), new XComShip(Manta(), GasCannon(), GasCannon()));
                XComShipSim(Dreadnought(), new XComShip(Manta(), GaussCannon(), GaussCannon()));
                XComShipSim(Dreadnought(), new XComShip(Manta(), SonicOscillator(), SonicOscillator()));
            }
        }
        static void XComShipSim(XComShip ap, params XComShip[] xp)
        {
            const int timePer = 2100;
            int tries = 0, time = Environment.TickCount;
            int[] killed = new int[xp.Length + 1];
            Action action = () =>
            {
                while (time + timePer > Environment.TickCount)
                {
                    XComShip alien = ap.Clone(true);
                    XComShip[] xcom = new XComShip[xp.Length];
                    for (int a = 0; a < xp.Length; ++a)
                        xcom[a] = xp[a].Clone(false);
                    lock (typeof(Program))
                        ++tries;
                    foreach (var s in xcom.Concat(new[] { alien }))
                        s.Reset();

                    var alive = xcom as IEnumerable<XComShip>;
                    const int distMult = 2;
                    int dist = alive.Concat(new[] { alien }).SelectMany(s => s.weapons).Max(w => w.range) * distMult;
                    do
                    {
                        var weapons = alive.Concat(new[] { alien }).SelectMany(s => s.weapons).Where(w => w.curShots > 0);
                        Func<XComShip.Weapon, bool> InRange = w => w.range * distMult >= dist;
                        Func<XComShip.Weapon, bool> CanFire = w => w.curReload <= 0 && InRange(w);

                        int interval = weapons.Where(w => !InRange(w)).Select(w => dist - w.range * distMult).Concat(weapons.Where(InRange).Select(w => w.curReload)).Min();
                        dist -= interval;
                        foreach (var w in weapons)
                            w.curReload -= interval;

                        //simulate projectile travel speed?
                        var aW = alien.weapons[0];
                        if (CanFire(aW))
                            aW.Fire(rand.SelectValue(alive), true, alive.Count() > 1);
                        foreach (var w in alive.SelectMany(s => s.weapons).Where(CanFire))
                            w.Fire(alien, false, false);

                        alive = alive.Where(s => s.curHits > 0).ToList();
                    } while (alien.curHits > 0 && alive.Any());

                    for (int b = 0; b < xcom.Length; ++b)
                        if (xcom[b].curHits <= 0)
                            lock (typeof(Program))
                                ++killed[b];
                    if (alien.curHits <= 0)
                        lock (typeof(Program))
                            ++killed[xcom.Length];
                }
            };

            Parallel.Invoke(Enumerable.Repeat<Action>(action, Math.Max(1, rand.Round(Environment.ProcessorCount / 1.95))).ToArray());
            Console.WriteLine(tries.ToString("000000"));
            for (int c = 0; c < killed.Length; ++c)
            {
                XComShip ship = c < xp.Length ? xp[c] : ap;
                Console.WriteLine("{0:000.0%} - {1} {2} {3}", killed[c] / (double)tries, ship.name, ship.weapons[0].name, ship.weapons.Count > 1 ? ship.weapons[1].name : "");
            }
            Console.WriteLine();
        }
        class XComShip
        {
            public XComShip(Tuple<string, int, int> nameHitsSize, Weapon w1)
                : this(new Tuple<string, int>(nameHitsSize.Item1, nameHitsSize.Item2), w1)
            {
                this.size = nameHitsSize.Item3;
            }
            public XComShip(Tuple<string, int> nameHits, Weapon w1, Weapon w2 = null)
            {
                this.name = nameHits.Item1;
                this.hits = nameHits.Item2;
                this.weapons = new List<Weapon>(new[] { w1 });
                if (w2 != null)
                    this.weapons.Add(w2);
            }
            public void Reset()
            {
                curHits = hits;
                weapons.ForEach(w => w.Reset());
            }
            public XComShip Clone(bool alien)
            {
                if (alien)
                    return new XComShip(new Tuple<string, int, int>(name, hits, size), weapons[0].Clone(alien));
                else
                    return new XComShip(new Tuple<string, int>(name, hits), weapons[0].Clone(alien),
                            weapons.Count > 1 ? weapons[1].Clone(alien) : null);
            }
            public readonly string name;
            private readonly int hits, size;
            public int curHits;
            public readonly List<Weapon> weapons;
            public override string ToString()
            {
                return string.Format("{2}: {0}/{1}", curHits, hits, name);
            }
            public class Weapon
            {
                private static int difficulty = int.MinValue;
                public static void setDifficulty(int difficulty)
                {
                    Weapon.difficulty = difficulty;
                    string dName = null;
                    switch (difficulty)
                    {
                        case 0:
                            dName = "Beginner";
                            break;
                        case 1:
                            dName = "Experienced";
                            break;
                        case 2:
                            dName = "Veteran";
                            break;
                        case 3:
                            dName = "Genius";
                            break;
                        case 4:
                            dName = "Superhuman";
                            break;
                    }
                    Console.WriteLine();
                    Console.WriteLine(dName);
                    Console.WriteLine();
                }
                public Weapon(int dmg, int range, int reload)
                {
                    if (difficulty < 0 || difficulty > 4)
                        throw new Exception();
                    this.name = "";
                    this.shots = int.MaxValue;
                    this.dmg = dmg;
                    this.range = range;
                    this.acc = 60.0 / 101.0;
                    this.reload = rand.Round(1.5 * (reload - 2.0 * difficulty));
                }
                public Weapon(string name, int shots, int dmg, int range, double acc, int reload)
                {
                    this.name = name;
                    this.shots = shots;
                    this.dmg = dmg;
                    this.range = range;
                    this.acc = acc;
                    this.reload = reload;
                }
                public void Reset()
                {
                    curShots = shots;
                    curReload = 0;
                }
                public void Fire(XComShip target, bool alien, bool mob)
                {
                    --curShots;
                    curReload = reload;
                    double acc = this.acc;
                    if (alien)
                        curReload += rand.RangeInt(0, reload);
                    else if (target.size >= 1 && target.size <= 5)
                        ; //acc *= ( 1.0 + ( 3.0 / target.size ) ) / 2.0; //this seems hard to believe
                    else
                        throw new Exception();
                    if (mob)
                        curReload = rand.Round(curReload * .75); //this is just a best a guess of the actual behavior
                    if (rand.Bool(acc)) //(acc > 0 && ( acc > 1 || rand.Bool(acc) )) //only needed with above accuracy modifier
                        target.curHits -= rand.RangeInt(dmg, alien ? 0 : rand.Round(dmg / 2.0));
                }
                public Weapon Clone(bool alien)
                {
                    if (alien)
                        return new Weapon(dmg, range, reload);
                    else
                        return new Weapon(name, shots, dmg, range, acc, reload);
                }
                public readonly string name;
                private readonly int shots, dmg, reload;
                public readonly int range;
                public int curShots, curReload;
                private readonly double acc;
                public override string ToString()
                {
                    return string.Format("{7}: {0} {5} {6} {1}/{2} {3}/{4}", dmg, curReload, reload, curShots, shots, acc, range, name);
                }
            }
        }
        #endregion //XComShipSim

        #region TickTest
        private static void TickTest()
        {
            const double AvgSeedSize = 520;
            const int max = MTRandom.MAX_SEED_SIZE - 1;
            uint[] seed = MTRandom.GenerateSeed((ushort)(rand.WeightedInt(max, (AvgSeedSize - 1.0) / max) + 1));
            ParameterizedThreadStart func = (object param) =>
            {
                MTRandom r;
                lock (typeof(Program))
                    r = (MTRandom)param;
                for (int a = 0; a < 3; ++a)
                {
                    Console.WriteLine(r.OEFloat());
                    Thread.Sleep(3);
                }
            };
            Thread[] threads = new Thread[2];
            threads[0] = new Thread(func);
            threads[1] = new Thread(func);
            MTRandom r1 = new MTRandom(seed);
            MTRandom r2 = new MTRandom(seed);
            lock (typeof(Program))
            {
                threads[0].Start(r1);
                threads[1].Start(r2);
            }
            while (true)
            {
                Console.ReadKey(true);
                Console.WriteLine();
                Console.WriteLine();
                threads[0] = new Thread(func);
                threads[1] = new Thread(func);
                r1 = new MTRandom(seed);
                r2 = new MTRandom(seed);
                lock (typeof(Program))
                {
                    threads[0].Start(r2);
                    threads[1].Start(r1);
                    r1.StartTick();
                    r2.StartTick();
                }
            }
        }
        #endregion //TickTest

        #region SeedTest
        //static void SeedTest()
        //{
        //    Console.BufferHeight *= 2;
        //    Console.BufferWidth *= 2;
        //    const double AvgSeedSize = 520;
        //    const int max = MTRandom.MAX_SEED_SIZE - 1;
        //    uint[] seed = MTRandom.GenerateSeed((ushort)( rand.WeightedInt(max, ( AvgSeedSize - 1.0 ) / max) + 1 ));
        //    Write(new MTRandom(true, seed));
        //    seed[rand.Next(seed.Length)] ^= ( (uint)1 << rand.Next(32) );
        //    Write(new MTRandom(true, seed));
        //}
        //static void Write(MTRandom rand)
        //{
        //    Console.WriteLine("seed:");
        //    foreach (uint seed in rand.Seed)
        //        Write(seed);
        //    Console.WriteLine();
        //    Console.WriteLine();
        //    Console.WriteLine("state:");
        //    Write(rand.lcgn);
        //    Write(rand.lfsr);
        //    Write(rand.mwc1);
        //    Write(rand.mwc2);
        //    Console.WriteLine();
        //    foreach (uint v in rand.m)
        //        Write(v);
        //    Console.WriteLine();
        //    Console.WriteLine();
        //}

        //static int SeedTest(bool write, bool ver)
        //{
        //    Console.WriteLine();
        //    if (write)
        //    {
        //        Console.BufferHeight *= 2;
        //        Console.BufferWidth *= 2;
        //    }

        //    const double AvgSeedSize = 390;
        //    const int max = MTRandom.MAX_SEED_SIZE - 1;

        //    ushort sl = (ushort)( rand.WeightedInt(max, ( AvgSeedSize - 1.0 ) / max) + 1 );
        //    Console.WriteLine();
        //    Console.WriteLine(sl);

        //    uint[] seed;
        //    if (ver)
        //        seed = MTRandom.GenerateSeed(sl);
        //    else
        //        seed = new uint[sl];

        //    MTRandom r1 = new MTRandom(true, seed);
        //    MTRandom r2 = new MTRandom(true, seed);

        //    int d1 = 0;
        //    d1 += DiffBits(r1.lcgn, r2.lcgn);
        //    d1 += DiffBits(r1.lfsr, r2.lfsr);
        //    d1 += DiffBits(r1.mwc1, r2.mwc1);
        //    d1 += DiffBits(r1.mwc2, r2.mwc2);
        //    for (int a = 0 ; a < MTRandom.LENGTH ; ++a)
        //        d1 += DiffBits(r1.m[a], r2.m[a]);

        //    MTRandom r3 = null;
        //    while (r3 == null)
        //    {
        //        try
        //        {
        //            if (ver)
        //                seed[rand.Next(seed.Length)] ^= ( (uint)1 << rand.Next(32) );
        //            else
        //                seed = new uint[sl + rand.GaussianInt()];
        //            r3 = new MTRandom(true, seed);
        //        }
        //        catch { }
        //    }

        //    Console.WriteLine(seed.Length);
        //    if (write)
        //        Console.WriteLine();

        //    int d2 = 0;
        //    d2 += DiffBits(r1.lcgn, r3.lcgn);
        //    d2 += DiffBits(r1.lfsr, r3.lfsr);
        //    d2 += DiffBits(r1.mwc1, r3.mwc1);
        //    d2 += DiffBits(r1.mwc2, r3.mwc2);
        //    for (int a = 0 ; a < MTRandom.LENGTH ; ++a)
        //        d2 += DiffBits(r1.m[a], r3.m[a]);

        //    if (write)
        //    {
        //        Write(r1);
        //        Write(r2);
        //        Write(r3);
        //    }

        //    Console.WriteLine(d1);
        //    Console.WriteLine(d2);
        //    Console.WriteLine(16 * ( MTRandom.LENGTH + 4 ));

        //    return d2;
        //}
        //static void Write(uint seed)
        //{
        //    Console.Write(Convert.ToString(seed, 2).PadLeft(32, '0'));
        //}

        //private static int DiffBits(uint v1, uint v2)
        //{
        //    uint diff = v1 ^ v2;
        //    return Convert.ToString(diff, 2).Replace("0", "").Length;
        //}
        #endregion //SeedTest

        #region CWPortalStart
        static void CWPortalStart()
        {
            Console.BufferHeight = 6500;

            Action[] actions = new Action[]
            {
                () => CWPortalStart(077, 059.0177976546929, "Zombies"), //      1
                () => CWPortalStart(081, 077.1171385555153, "Golem"), //        2
                () => CWPortalStart(129, 077.1171385555153, "Bats"), //         3
                () => CWPortalStart(137, 066.4231558922337, "Gryphon"), //      4
                () => CWPortalStart(137, 053.5436012924079, "Pyroraptor"), //   5
                () => CWPortalStart(150, 100.4116741003120, "Shield"), //       6
                () => CWPortalStart(156, 067.6899674689582, "Fanglers"), //     7
                () => CWPortalStart(190, 063.9845569584666, "Giant"), //        8
                () => CWPortalStart(202, 067.6899674689582, "Salamander"), //   9
                () => CWPortalStart(219, 073.2961632985778, "Dryads"), //      10
                () => CWPortalStart(222, 100.4116741003120, "Roc"), //         11
                () => CWPortalStart(226, 063.9845569584666, "Scorpion"), //    12
                () => CWPortalStart(232, 073.2961632985778, "Unicorn"), //     13
                () => CWPortalStart(242, 073.2961632985778, "Treant"), //      14
                () => CWPortalStart(283, 041.9128572788207, "Wyrm"), //        15
                () => CWPortalStart(301, 067.6899674689582, "Leviathan"), //   16
                () => CWPortalStart(317, 077.1171385555153, "Troll"), //       17
                () => CWPortalStart(320, 043.4823892578576, "Wraith"), //      18
                () => CWPortalStart(321, 040.2159889856090, "Kraken"), //      19
                () => CWPortalStart(356, 059.0177976546929, "Behemoth"), //    20
                () => CWPortalStart(368, 037.2169472931034, "Wind Sprite"), // 21
                () => CWPortalStart(374, 100.4116741003120, "Pegasi"), //      22
                () => CWPortalStart(393, 066.4231558922337, "Wyverns"), //     23
                () => CWPortalStart(402, 050.1118884368771, "Hydra"), //       24
                () => CWPortalStart(403, 065.8706680810215, "Daemon"), //      25
                () => CWPortalStart(408, 065.8706680810215, "Spirit"), //      26
                () => CWPortalStart(435, 053.5436012924079, "Elemental"), //   27
                () => CWPortalStart(560, 054.5983483459748, "Dragon"), //      28
            };

            foreach (Action action in rand.Iterate(actions))
                action();
        }

        static void CWPortalStart(int baseUnitCost, double portalTurnInc, string name)
        {
            const int digits = 6;
            int max = int.Parse("".PadLeft(digits, '9'));
            string format = "".PadLeft(digits, '0');
            double sum = 0;
            const double StartAmt = .26;
            int upper = (int)Math.Ceiling(baseUnitCost - portalTurnInc + portalTurnInc * StartAmt);
            int[] val = new int[upper + 1];
            for (int x = 0; x < max; ++x)
            {
                int l = rand.Round(rand.Weighted(baseUnitCost - portalTurnInc, StartAmt) + portalTurnInc * StartAmt);
                sum += l;
                val[l]++;
            }
            //Console.BufferHeight = baseUnitCost + 3;
            int runtot = 0;

            Console.WriteLine("{0} - {1} - {2}", name, sum / max, StartAmt * baseUnitCost);
            Console.WriteLine();
            for (int x = (int)(portalTurnInc * StartAmt); x <= upper; ++x)
                Console.WriteLine("{0} - {1} - {3}", x.ToString("000"), val[x].ToString(format),
                        ((max - runtot) / (1.0 + upper - x)).ToString(format), (runtot += val[x]).ToString(format));
            Console.WriteLine();
            Console.WriteLine(baseUnitCost);
            Console.WriteLine();
            Console.WriteLine();
        }
        #endregion //CWPortalStart

        #region IterateTest
        static void IterateTest()
        {
            const int count = 4;
            IEnumerable<int> e = rand.Iterate(count);
            ThreadStart func = () =>
            {
                int[] r = new int[count];
                for (int a = 0; a < 222222; ++a)
                    lock (e)
                        foreach (int i in e)
                        {
                            if (i == 0)
                            {
                                if (rand.Bool())
                                    r[i]++;
                                break;
                            }
                            r[i]++;
                        }
                lock (typeof(Program))
                {
                    foreach (int c in r)
                        Console.WriteLine(c);
                    Console.WriteLine();
                }
            };
            const int tests = 13;
            Thread[] threads = new Thread[tests];
            for (int a = 0; a < tests; ++a)
                threads[a] = new Thread(func);
            for (int a = 0; a < tests; ++a)
                threads[a].Start();
        }
        #endregion //IterateTest

        #region BinarySearchSqrt
        static void BinarySearchSqrt()
        {
            do
            {
                bool f = rand.Bool();
                if (f)
                {
                    float val = rand.OEFloat();
                    //float act = (float)Math.Sqrt(val);
                    Console.WriteLine(val);
                    float s = BinarySearchSqrt(val);
                    Console.WriteLine(s);
                    Console.WriteLine((float)Math.Sqrt(val));
                    //if (val != act) 
                    if (s * s != val)
                        Console.WriteLine("off");
                    if ((float)Math.Sqrt(val) * (float)Math.Sqrt(val) != val)
                        Console.WriteLine("Math");
                }
                else
                {
                    double val = rand.OE();
                    //double act = Math.Sqrt(val);
                    Console.WriteLine(val);
                    double s = BinarySearchSqrt(val);
                    Console.WriteLine(s);
                    Console.WriteLine(Math.Sqrt(val));
                    //if (val != act)
                    if (s * s != val)
                        Console.WriteLine("off");
                    if (Math.Sqrt(val) * Math.Sqrt(val) != val)
                        Console.WriteLine("Math");
                }
                Console.WriteLine();
                Thread.Sleep(3900);
            } while (rand.Bool(999 / 1000.0));
        }
        static float BinarySearchSqrt(float n)
        {
            return (float)BinarySearchSqrt(n, true);
        }
        static double BinarySearchSqrt(double n)
        {
            return BinarySearchSqrt(n, false);
        }
        static double BinarySearchSqrt(double n, bool isFloat)
        {
            if (n < 0)
                throw new ArgumentOutOfRangeException();
            int count = 0;
            double min, max;
            if (n < 1)
            {
                min = n;
                max = 1;
            }
            else
            {
                min = 1;
                max = n;
            }
            double val;
            while (true)
            {
                ++count;
                val = (min + max) / 2.0;
                if (isFloat ? ((float)val == (float)min || (float)val == (float)max) : (val == min || val == max))
                {
                    if (Math.Abs((min * min) - n) < Math.Abs((max * max) - n))
                        val = min;
                    else
                        val = max;
                    break;
                }
                double sqr = val * val;
                if (isFloat ? ((float)sqr == (float)n) : (sqr == n))
                {
                    Console.WriteLine("break");
                    break;
                }
                else if (sqr < n)
                    min = val;
                else
                    max = val;
            }
            Console.WriteLine(count);
            return val;
        }
        #endregion //BinarySearchSqrt

        #region DaeReserveMorale
        static void DaeReserveMorale()
        {
            //test w/ randomness (max 1.047)?  how close can we get accuracy?  binary search impossible?  mathematically integrate over gaussian curve?
            //use 0.9909 and hardcode 0.4933376783627060 to increase speed/accuracy?  
            DaeReserveMorale0();
        }
        static void DaeReserveMorale0()
        {
            //const double closest = 1.046319547648739;
            //Console.WriteLine(Convert.ToString(BitConverter.DoubleToInt64Bits(CalcDaeReserveMorale()), 2));
            //Console.WriteLine(Convert.ToString(BitConverter.DoubleToInt64Bits(closest), 2));
            //Console.WriteLine(CalcDaeReserveMorale());
            //Console.WriteLine(closest);

            Action<Action> ThreadRun = Action =>
            {
                Thread thread = new Thread(new ThreadStart(Action));
                thread.IsBackground = true;
                thread.Start();
            };
            ThreadRun(() => DaeRandTest(1));
            ThreadRun(() => DaeRandTest(1 - .0091));
            ThreadRun(() => DaeRandTest(.5));
            ThreadRun(() => DaeRandTest(Math.Pow(.00117, 1 / Math.Pow(1 / .39, 1.3)) * (1.0 + .169 * 1.3)));
            ThreadRun(() => DaeRandTest(.00117));
            ThreadRun(() => DaeRandTest(float.Epsilon));
            ThreadRun(() => DaeRandTest(Math.Pow(double.Epsilon, 1 / Math.Pow(1 / .39, 1.3)) * 2));
            ThreadRun(() => DaeRandTest(double.Epsilon));
            ThreadRun(() => DaeRandTest(() => 1 - rand.NextDouble()));

            //Console.WriteLine(Convert.ToString(BitConverter.DoubleToInt64Bits(1.0463195476487386), 2));
            //Console.WriteLine(Convert.ToString(BitConverter.DoubleToInt64Bits(1.0463195476487388), 2));
            //Console.WriteLine(Convert.ToString(BitConverter.DoubleToInt64Bits(1.0463195476487390), 2));
            //Console.WriteLine(Convert.ToString(BitConverter.DoubleToInt64Bits(1.0463195476487392), 2));
            //Console.WriteLine(Convert.ToString(BitConverter.DoubleToInt64Bits(1.0463195476487395), 2));
            //Console.WriteLine(Convert.ToString(BitConverter.DoubleToInt64Bits(1.0463195476487397), 2));
            //Console.WriteLine();
            //Dictionary<double, int> otherCnt = new Dictionary<double, int>();
            //for (int b = 0 ; b < 130000 ; ++b)
            //{
            //    double test = CalcDaeReserveMorale();
            //    int cnt;
            //    if (!otherCnt.TryGetValue(test, out cnt))
            //        cnt = 0;
            //    otherCnt[test] = cnt + 1;
            //}
            //foreach (KeyValuePair<double, int> p in otherCnt.OrderBy(p => p.Key))
            //{
            //    Console.WriteLine(Convert.ToString(BitConverter.DoubleToInt64Bits(p.Key), 2));
            //    Console.WriteLine(p.Value);
            //}
            //Console.WriteLine();
            //foreach (KeyValuePair<double, int> p in daeCnt.OrderBy(p => p.Key))
            //{
            //    Console.WriteLine(Convert.ToString(BitConverter.DoubleToInt64Bits(p.Key), 2));
            //    Console.WriteLine(p.Value);
            //}
        }

        private static void DaeRandTest(double morale)
        {
            DaeRandTest(() => morale);
        }
        private static void DaeRandTest(Func<double> GetMorale)
        {
            double[] a = new double[2];
            const double closest = 1.046319547648739;
            const int d = 1300000;
            double tot = 0;
            for (int b = 0; b < d; ++b)
            {
                double morale = GetMorale();
                tot += morale;
                double[] c = DaeReserveMoraleRand(closest, morale);
                a[0] += c[0];
                a[1] += c[1];
            }
            Console.WriteLine(tot / d);
            Console.WriteLine(a[0] / d);
            Console.WriteLine(a[1] / d);
            Console.WriteLine();
        }

        static double CalcDaeReserveMorale()
        {
            double min = 1;
            double max = 1.13744213481967;
            double val;
            while (true)
            {
                val = (min + max) / 2.0;
                if ((val == min || val == max))
                    return val;

                if (DaeReserveMorale(val))
                    min = val;
                else
                    max = val;
            }
        }
        static Dictionary<double, int> daeCnt = new Dictionary<double, int>();
        static bool DaeReserveMorale(double test)
        {
            const double loss = 1.3;
            double m1 = 1 - rand.NextDouble();
            double m2 = m1;

            m1 = LoseMorale(m1, loss);

            m2 = LoseMorale(m2, loss / 2.0) / test;
            m2 = LoseMorale(m2, loss / 2.0) / test;

            if (m1 == m2)
            {
                int cnt;
                if (!daeCnt.TryGetValue(test, out cnt))
                    cnt = 0;
                daeCnt[test] = cnt + 1;
                return rand.Bool();
            }
            return (m1 < m2);
        }
        static double LoseMorale(double Morale, double mult)
        {
            return Math.Pow(Morale / (1.0 + .169 * mult), Math.Pow(1 / .39, mult));
        }
        static double[] DaeReserveMoraleRand(double test, double morale)
        {
            const double loss = 1.3;
            double m1 = morale;
            double m2 = morale;

            m1 = LoseMoraleRand(m1, loss);

            m2 = LoseMoraleRand(m2, loss / 2.0) / test;
            m2 = LoseMoraleRand(m2, loss / 2.0) / test;

            return new[] { m1, m2 };
        }
        static double LoseMoraleRand(double Morale, double mult)
        {
            return RandMorale(Morale, LoseMorale(Morale, mult));
        }
        static double RandMorale(double Morale, double value)
        {
            if (value < double.Epsilon)
                value = double.Epsilon;
            return rand.GaussianCapped(value, Math.Abs(Morale - value) / value * .21, Math.Max(double.Epsilon, 2 * value - 1));
        }
        #endregion //DaeReserveMorale

        #region CWMapGen
        static void CWMapGen()
        {
            int width = 18, height = 18;
            Console.WindowHeight = height * 2 + 3;
            Tile[,] map = new Tile[width, height];
            CreateMap(map, width, height, (point) =>
            {
                Tile t = map[point.X, point.Y];
                char c = ' ';
                if (t != null)
                    c = t.Terrain;
                int x = point.X * 2;
                if (point.Y % 2 == 0)
                    ++x;
                Console.SetCursorPosition(x, point.Y * 2);
                Console.Write(c);
                Console.SetCursorPosition(x, point.Y * 2);
                Console.ReadKey(true);
            });
        }

        class Tile
        {
            public int x, y;
            public char Terrain;
            public Tile(int x, int y)
            {
                this.x = x;
                this.y = y;
                this.Terrain = rand.SelectValue(new[] { '_', '.', '!', 'M' });
            }
            public Tile(int x, int y, char Terrain)
            {
                this.x = x;
                this.y = y;
                this.Terrain = Terrain;
            }
        }
        static Tile[,] CreateMap(Tile[,] map, int width, int height, Action<Point> Callback)//, int numPlayers)
        {
            foreach (Point coord in rand.Iterate(width, height))
            {
                map[coord.X, coord.Y] = CreateTile(map, coord.X, coord.Y, width, height);
                Callback(coord);
            }
            return map;
        }
        static Tile CreateTile(Tile[,] map, int x, int y, int width, int height)
        {
            Tile tile = null;
            //try three times to find a neighbor that has already been initialized
            for (int i = 0; i < 3; ++i)
            {
                tile = GetTileIn(map, x, y, rand.Next(6), width, height);
                if (tile != null)
                    break;
            }
            Tile result;
            if (tile == null)
                result = new Tile(x, y);
            else
                result = new Tile(x, y, tile.Terrain);

            return result;
        }
        static Tile GetTileIn(Tile[,] map, int x, int y, int direction, int width, int height)
        {
            //this methoid is called to set up the neighbors array
            bool odd = y % 2 > 0;
            switch (direction)
            {
                case 0:
                    if (odd)
                        --x;
                    --y;
                    break;
                case 1:
                    if (!odd)
                        ++x;
                    --y;
                    break;
                case 2:
                    --x;
                    break;
                case 3:
                    ++x;
                    break;
                case 4:
                    if (odd)
                        --x;
                    ++y;
                    break;
                case 5:
                    if (!odd)
                        ++x;
                    ++y;
                    break;
                default:
                    throw new Exception();
            }
            if (x < 0 || x >= width || y < 0 || y >= height)
                return null;
            else
                return map[x, y];
        }
        #endregion //CWMapGen

        #region GenerateTerrain
        static void GenerateTerrain()
        {
            do
            {
                int width = Console.LargestWindowWidth, height = Console.LargestWindowHeight;
                if (width > Console.WindowWidth)
                    Console.WindowWidth = Console.BufferWidth = width;
                else
                    Console.BufferWidth = Console.WindowWidth = width;
                if (height > Console.WindowHeight)
                    Console.WindowHeight = (Console.BufferHeight = height + 1) - 1;
                else
                    Console.BufferHeight = (Console.WindowHeight = height) + 1;

                Terrain[,] res = new Terrain[width, height];
                for (int y = 0; y < height; ++y)
                    for (int x = 0; x < width; ++x)
                        res[x, y] = new Terrain();

                DoStat(width, height, res, Terrain.GetHeight, Terrain.SetHeight);
                DoStat(width, height, res, Terrain.GetRain, Terrain.SetRain);
                DoStat(width, height, res, Terrain.GetTemp, Terrain.SetTemp);

                float equator = rand.FloatHalf(), eqDif = rand.GaussianCapped(1f - Math.Abs(.5f - equator), .169f, .39f);

                for (int y = 0; y < height; ++y)
                    for (int x = 0; x < width; ++x)
                    {
                        float h = 0, t = 0, r = 0, count = 0;
                        for (int a = -2; a < 3; ++a)
                            if (x + a >= 0 && x + a < width)
                                for (int b = -2; b < 3; ++b)
                                    if (y + b >= 0 && y + b < height)
                                    {
                                        float mult = 1 / (a * a + b * b + .3f);
                                        h += Terrain.GetHeight(res[x + a, y + b]) * mult;
                                        t += Terrain.GetTemp(res[x + a, y + b]) * mult;
                                        r += Terrain.GetRain(res[x + a, y + b]) * mult;
                                        count += mult;
                                    }
                        h /= count;
                        t /= count;
                        r /= count;

                        double pow = .5 + 3 * Math.Abs((y + .5) / height - equator) * eqDif;
                        t = (float)Math.Pow(t, pow * pow);
                        DrawTerrain(h, t, r);
                    }

                Console.SetCursorPosition(0, 0);
            } while (Console.ReadKey(true).Key != ConsoleKey.Q);
        }

        static void DrawTerrain(float height, float temp, float rain)
        {
            if (height > .39 && height < .5)
            {
                rain = (float)Math.Pow(rain, 1 + (height - .5) * 7.8);
            }
            else if (height > .75 && height < .87)
            {
                rain = (float)Math.Pow(rain, 1 + (.75 - height) * 3.9);
                temp -= .13f;
                if (temp > 0)
                    temp = (float)Math.Pow(temp, 1 + (height - .75) * 6.5);
                temp += .13f;
            }

            temp *= 500;
            rain *= temp;

            if (height > .87)
                if (height % .02f < (height - .87f) / 5f && height < .98)
                    Console.BackgroundColor = ConsoleColor.DarkGray; // mountain
                else if (temp < 6 || height > .98)
                    Console.BackgroundColor = ConsoleColor.White; // glacier 
                else
                    Console.BackgroundColor = ConsoleColor.Gray; // alpine tundra
            else if (temp < 6)
                Console.BackgroundColor = ConsoleColor.White; // glacier
            else if (height < .169)
                Console.BackgroundColor = ConsoleColor.DarkBlue; // deep sea
            else if (height < .39)
                Console.BackgroundColor = ConsoleColor.Blue; // sea
            else if (height > .97)
                Console.BackgroundColor = ConsoleColor.White; // alpine glacier
            else if ((temp > 100) && ((rain < (temp - 100) * (temp - 100) / 1000f)))
                if (rain < temp - 313)
                    Console.BackgroundColor = ConsoleColor.Red; // sub. desert
                else
                    Console.BackgroundColor = ConsoleColor.DarkRed; // temp. grass / desert
            else if ((temp > 300) && (rain < 25 + 490 / (1.0 + Math.Pow(Math.E, (390 - temp) / 26.0))))
                if (rain < 260 + (temp - 333) * (temp - 333) / 260f)
                    Console.BackgroundColor = ConsoleColor.Yellow; // trop. seas. forest / savannah
                else
                    Console.BackgroundColor = ConsoleColor.Cyan; // trop. rain forest
            else if (rain < (temp - 65) / 2.1f)
                Console.BackgroundColor = ConsoleColor.DarkYellow; // woodland / shrubland
            else if ((temp > 200) || ((temp > 130) && (rain < 21 + 196 / (1.0 + Math.Pow(Math.E, (169 - temp) / 13.0)))))
                if ((temp < 169) || rain < 125 + 10 * Math.Sqrt(temp - 169))
                    Console.BackgroundColor = ConsoleColor.Green; // temp. dec. forest
                else
                    Console.BackgroundColor = ConsoleColor.DarkCyan; // temp. rain forest
            else if ((temp > 100) || (rain < 120 / (1.0 + Math.Pow(Math.E, (78 - temp) / 13.0))))
                Console.BackgroundColor = ConsoleColor.DarkGreen; // taiga
            else
                Console.BackgroundColor = ConsoleColor.Gray; // tundra

            Console.Write(' ');
        }

        static float DoStat(int width, int height, Terrain[,] res, Terrain.GetStat Get, Terrain.SetStat Set)
        {
            float cur = 1, frq = 1.95f, amp = 1.5f;
            Dictionary<Point, float> points = new Dictionary<Point, float>();
            //int lim = rand.Round(Math.Sqrt(width * height / 1300.0));
            for (int dim = rand.Round(res.Length / frq); dim > 2; dim = rand.GaussianInt(dim / frq, .03f))
            {
                cur *= rand.Gaussian(amp, .03f);

                points.Clear();
                for (int a = 0; a < dim; ++a)
                    points[new Point(rand.Next(width), rand.Next(height))] = rand.DoubleHalf(cur);
                if (points.Count < rand.OEInt())
                    continue;

                for (int y = 0; y < height; ++y)
                    for (int x = 0; x < width; ++x)
                    {
                        int modX = x + rand.GaussianInt(), modY = y + rand.GaussianInt();

                        int found = int.MaxValue, total = 0;
                        float sum = 0;
                        foreach (KeyValuePair<Point, float> pair in points)
                        {
                            int distX = (pair.Key.X - modX);
                            int distY = (pair.Key.Y - modY);
                            int dist = distX * distX + distY * distY;
                            if (dist < found)
                            {
                                sum = pair.Value;
                                total = 1;
                                found = dist;
                            }
                            else if (dist == found)
                            {
                                sum += pair.Value;
                                ++total;
                            }
                        }
                        Set(res[x, y], Get(res[x, y]) + sum / total);
                    }
            }

            SortedDictionary<float, Point> sort = new SortedDictionary<float, Point>();
            foreach (Point p in rand.Iterate(width, height))
            {
                int x = p.X, y = p.Y;
                while (sort.ContainsKey(Get(res[x, y])))
                    Set(res[x, y], Get(res[x, y]) + rand.GaussianFloat());
                sort.Add(Get(res[x, y]), p);
            }
            sort.First();
            sort.Last();
            int v1 = 0;
            float div = res.Length - 1;
            foreach (Point value in sort.Values)
                Set(res[value.X, value.Y], v1++ / div);
            return cur;
        }

        class Terrain
        {
            public float height, temp, rain;

            public Terrain()
            {
                height = rand.DoubleHalf(1f);
                temp = rand.DoubleHalf(1f);
                rain = rand.DoubleHalf(1f);
            }

            public delegate float GetStat(Terrain tile);
            public delegate void SetStat(Terrain tile, float stat);
            public static float GetHeight(Terrain tile)
            {
                return tile.height;
            }
            public static void SetHeight(Terrain tile, float stat)
            {
                tile.height = stat;
            }
            public static float GetTemp(Terrain tile)
            {
                return tile.temp;
            }
            public static void SetTemp(Terrain tile, float stat)
            {
                tile.temp = stat;
            }
            public static float GetRain(Terrain tile)
            {
                return tile.rain;
            }
            public static void SetRain(Terrain tile, float stat)
            {
                tile.rain = stat;
            }
        }
        #endregion //GenerateTerrain
    }
}
