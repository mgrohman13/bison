using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MattUtil
{
    public static class TBSUtil
    {

        public static Dictionary<Player, int> RandMoveOrder<Player>(MTRandom random, Player[] players, double shuffle)
        {
            Dictionary<Player, int> retVal = new Dictionary<Player, int>();

            int playerLength = players.Length;
            int numShuffles = random.OEInt(playerLength * shuffle);
            HashSet<int> affected = new HashSet<int>();
            for (int a = 0 ; a < numShuffles ; ++a)
            {
                int index = random.Next(playerLength);
                if (index == 0 || affected.Contains(index - 1) || affected.Contains(index))
                {
                    int amt;
                    retVal.TryGetValue(players[index], out amt);
                    retVal[players[index]] = amt + 1;
                }
                else
                {
                    affected.Add(index - 1);
                    affected.Add(index);
                    Player temp = players[index - 1];
                    players[index - 1] = players[index];
                    players[index] = temp;
                }
            }

            return retVal;
        }

        //performs a binary search to find the minimum value (when trueHigh is true, otherwise the maximum value)
        //for which Predicate returns true
        public static int FindValue(Predicate<int> Predicate, int min, int max, bool trueHigh)
        {
            if (trueHigh)
                --min;
            else
                ++max;
            int mid;
            while (( mid = max - min ) > 1)
            {
                mid = min + mid / 2;
                if (Predicate(mid))
                    if (trueHigh)
                        max = mid;
                    else
                        min = mid;
                else if (trueHigh)
                    min = mid;
                else
                    max = mid;
            }
            return ( trueHigh ? max : min );
        }

    }
}
