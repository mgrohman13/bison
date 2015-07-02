using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sorting
{
    class Sorter
    {
        //TODO:
        //smoothsort 
        //strand
        //pancake

        //stuff for communicating with the graph
        #region Sorter

        //delegate to refresh the graph on each move
        private RefreshDelegate Swapped, Refresh;
        public Sorter(RefreshDelegate Swapped, RefreshDelegate Refresh)
        {
            this.Swapped = Swapped;
            this.Refresh = Refresh;
        }

        //we use a uint wrapper class for the elements being sorted 
        //which helps ensure that the sorts are implemented as comparison sorts
        private Obj[] list;
        public Obj[] List
        {
            get
            {
                return list;
            }
            set
            {
                list = value;
                //maintain the length
                length = list.Length;
            }
        }
        //length is publicly read only
        private int length;
        public int Length
        {
            get
            {
                return length;
            }
        }

        private void Swap(int i1, int i2)
        {
            if (list[i1].Value != list[i2].Value)
            {
                //store both objects in case we abort
                Obj o1 = list[i1];
                Obj o2 = list[i2];
                try
                {
                    //swap them
                    list[i1] = o2;
                    list[i2] = o1;
                }
                catch (System.Threading.ThreadAbortException)
                {
                    //make sure they are properly swapped before aborting
                    list[i1] = o2;
                    list[i2] = o1;
                    //abort
                    throw;
                }
                //add a swap and refresh the graph
                Swapped();
            }
        }

        private void Done()
        {
            Refresh();
        }

        #endregion //Sorter

        //all sorts are done entirely in-place and the only valid operation on the list is a swap
        //this makes them more interesting to watch, leaves no hidden information,
        //and ensures that we can stop the sort at any time and not corrupt the elements in the list
        //they are all also implemented as purely comparison sorts
        #region Sorts

        //bitonic sorting network modified to work for arbitrary list length
        //standard bitonic sort requires the list length to be a power of 2
        public void BitonicSort()
        {
            BitonicSort(0, length, true);

            Done();
        }
        private void BitonicSort(int left, int right, bool dir)
        {
            if (right > 1)
            {
                int mid = right / 2;
                BitonicSort(left, mid, !dir);
                BitonicSort(left + mid, right - mid, dir);
                BitonicMerge(left, right, dir);
            }
        }
        private void BitonicMerge(int left, int right, bool dir)
        {
            if (right > 1)
            {
                //find the greatest power of 2 that is less than right
                int a = 1;
                while (a < right)
                    a = a << 1;
                a >>= 1;

                for (int b = left ; b < left + right - a ; b++)
                    if (dir == ( list[b] > list[b + a] ))
                        Swap(b, b + a);

                BitonicMerge(left, a, dir);
                BitonicMerge(left + a, right - a, dir);
            }
        }

        //this is an optimized bozo sort
        //a true bozo sort always swaps the two chosen elements
        //and always checks for a sorted list each swap
        public void BozoSort()
        {
            while (true)
            {
                //check if the list is already sorted
                bool sorted = true;
                for (int i = 1 ; i < length ; ++i)
                    if (list[i - 1] > list[i])
                    {
                        sorted = false;
                        break;
                    }
                //if it is sorted we are done
                if (sorted)
                    break;
                //swap two random elements
                while (true)
                {
                    //choose two random elements
                    int a = Program.Random.Next(length);
                    int b = Program.Random.Next(length);
                    //only swap them if they are in the wrong order
                    int compVal = Obj.Compare(list[a], list[b]);
                    if (a < b ? compVal > 0 : a > b && compVal < 0)
                    {
                        Swap(a, b);
                        //only check for a sorted list if we made a swap
                        break;
                    }
                }
            }

            Done();
        }

        //optimized bubble sort that breaks out early when the list is sorted
        public void BubbleSort()
        {
            //we can reduce the number of elemets to look at during each consecutive loop by one
            for (int a = length ; --a > -1 ; )
            {
                bool swapped = false;
                for (int b = 0 ; b < a ; ++b)
                    if (list[b] > list[b + 1])
                    {
                        Swap(b, b + 1);
                        swapped = true;
                    }
                //we can break out early if there were no swaps
                if (!swapped)
                    break;
            }

            Done();
        }

        //essentially a bubble sort that alternates each direction
        public void CocktailSort()
        {
            //we bring the left and right bounds closer each iteration
            int left = -1;
            int right = length - 2;
            //continue until we dont make any swaps
            bool swapped;
            do
            {
                //forwards bubble sort
                swapped = false;
                for (int i = ++left ; i <= right ; ++i)
                    if (list[i] > list[i + 1])
                    {
                        Swap(i, i + 1);
                        swapped = true;
                    }
                //break out early if no swaps
                if (!swapped)
                    break;
                //backwards bubble sort
                swapped = false;
                for (int i = --right ; i >= left ; --i)
                    if (list[i] > list[i + 1])
                    {
                        Swap(i, i + 1);
                        swapped = true;
                    }
            }
            while (swapped);

            Done();
        }

        //this optimized gnome sort stores the topmost position and returns to it immediately to take out some unnecessary comparisons
        //esentially an insertion sort with swaps instead of a temporary variable
        public void GnomeSort()
        {
            //topmost position
            int top = 1;
            for (int i = 1 ; i < length ; )
                if (list[i - 1] > list[i])
                {
                    //swap out of place elemnts
                    Swap(i, --i);
                    //check if we hit the bottom and if so return to the top
                    if (i < 1)
                        i = ++top;
                }
                else
                    //return to top
                    i = ++top;

            Done();
        }

        //standard heap sort
        public void HeapSort()
        {
            int bottom = length - 1;
            //build the heap
            for (int i = length / 2 ; --i > -1 ; )
                Heapify(i, bottom);

            //for an odd number of elements, the last element wont get heapified
            //so make sure it is not already higher than the top of the heap before we swap it
            if (length % 2 > 0 && list[0] < list[bottom])
                --bottom;

            while (bottom > 0)
            {
                //pull out each highest element
                Swap(0, bottom);
                //resatisfy the heap
                Heapify(0, --bottom);
            }

            Done();
        }
        private void Heapify(int root, int bottom)
        {
            int maxChild;
            while (( maxChild = root * 2 ) < bottom)
            {
                //find the max child of the current root
                if (list[maxChild] < list[maxChild + 1])
                    ++maxChild;

                if (list[root] < list[maxChild])
                {
                    Swap(root, maxChild);
                    //continue with the new index
                    root = maxChild;
                }
                else //it is in its heap position
                    break;
            }
        }

        //this is an in-place merge sort implementation that uses an unsorted section to merge two sorted ones
        public void KronrodSort()
        {
            KronrodSort(0, length - 1);

            Done();
        }
        private void KronrodSort(int left, int right)
        {
            int count = ( right - left + 1 );
            if (count > 2)
            {
                //choose a mid with two thirds in the left section and one third in the right
                int mid = left + ( count * 2 ) / 3;
                //sort the right section
                KronrodSort(mid, right);

                //sort using the two sections with an effective bound for 'small' blocks
                KronrodSort(left, mid, right, Program.Random.Round(count / Math.Log(count, 2.0)));
            }
            else if (count > 1 && list[left] > list[right])
                Swap(left, right);
        }
        //takes in two sections, an unsorted left and sorted right, and sorts
        private void KronrodSort(int left, int mid, int right, int smallBlock)
        {
            int unsorted = mid - left;
            //since merging a 'small' list with a 'large' one is highly inefficient
            //we use a different algorithm if a 'small' block is encountered
            if (unsorted <= smallBlock)
            {
                //sort the initial unsorted elements
                KronrodSort(left, mid - 1);

                //merge the upper portions
                MergeUpper(left, mid, ref right);

                //sort the remaining unsorted lower elements
                KronrodSort(left, right);
            }
            else
            {
                int use = left + unsorted / 2;
                //sort the first half of the left section
                KronrodSort(left, use - 1);

                int leftEnd = use - 1;
                //check for a single extra element and move use past it
                if (mid - use > use - left)
                    ++use;
                //merge the first half of the left with the right, using the second half of the left
                MergeAt(use, left, leftEnd, mid, right);

                //recurse with the larger sorted section and smaller unsorted
                KronrodSort(left, use, right, smallBlock);
            }
        }

        //the standard merge sort is not done in-place and is more efficient
        //this uses the basic algorithm but changes the merge method to work in place
        public void MergeSort()
        {
            MergeSort(0, length - 1);

            Done();
        }
        //this is the basic merge sort algorithm
        private void MergeSort(int left, int right)
        {
            //only sort if we have more than two elements
            int count = right - left;
            if (count > 1)
            {
                //split down the middle
                int mid = ( right + left ) / 2;
                //sort the left
                MergeSort(left, mid);
                //sort the right
                MergeSort(mid + 1, right);
                //merge the sorted sections
                Merge(left, mid + 1, right);
            }
            else if (count > 0 && list[left] > list[right])
                Swap(left, right);
        }
        //this is a redone merge algorithm that works in-place
        private void Merge(int left, int mid, int right)
        {
            mid = MergeUpper(left, mid, ref right);

            //at this point, the elements from left to mid are already sorted
            //so we can just sort mid through right and merge them
            MergeSort(mid, right);
            //only merge if both sorted sections contain at least one element
            if (!( left > mid || mid > right ))
                Merge(left, mid, right);
        }

        //standard odd-even sort
        public void OddEvenSort()
        {
            bool swapped;
            do
            {
                swapped = false;
                //compare all element pairs and swap if necessary
                for (int i = 2 ; i < length ; i += 2)
                    if (list[i] < list[i - 1])
                    {
                        Swap(i, i - 1);
                        swapped = true;
                    }
                //compare between pairs
                for (int i = 1 ; i < length ; i += 2)
                    if (list[i] < list[i - 1])
                    {
                        Swap(i, i - 1);
                        swapped = true;
                    }
            }
            while (swapped);

            Done();
        }

        //standard quick sort with randomly chosen pivot
        public void QuickSort()
        {
            QuickSort(0, length - 1);

            Done();
        }
        private void QuickSort(int left, int right)
        {
            if (left < right)
            {
                //split into two sections
                int index = Split(left, right);
                //sort each section
                QuickSort(left, index - 1);
                QuickSort(index + 1, right);
            }
        }
        private int Split(int left, int right)
        {
            //randomly choose a pivot and move it to the right
            //a randomly chosen pivot prevents the sort from degenerating to O(n^2) for nearly-sorted lists
            Swap(left + Program.Random.Next(right - left + 1), right);
            //split the other elements
            for (int i = left ; i < right ; ++i)
            {
                int compVal = Obj.Compare(list[i], list[right]);
                //if the value is equal to the pivot move it randomly to either side 
                //this prevents the sort from degenerating to O(n^2) when there are a large number of identical values
                if (compVal < 0 || ( compVal == 0 && Program.Random.Bool() ))
                    Swap(i, left++);
            }
            //place the pivot in its final position
            Swap(right, left);
            return left;
        }

        //this shell sort actually uses the optimized gnome sort as its inner loop instead of insertion
        //this is necessary since I only allow swaps
        public void ShellSort()
        {
            //start with the length
            int step = length;
            while (step > 1)
            {
                //progress the increment by a factor of 2.2
                //randomly round it just to make things a little more interesting
                step = Program.Random.Round(step / 2.2f);
                if (step < 1)
                    step = 1;

                //swap each element into place using the current increment
                for (int a = step ; a < length ; ++a)
                    for (int b = a ; list[b] < list[b - step] ; )
                    {
                        Swap(b, b -= step);
                        if (b < step)
                            break;
                    }
            }

            Done();
        }

        //the stooge sort is horribly inefficient but surprisingly ingenious
        public void StoogeSort()
        {
            StoogeSort(0, length - 1);

            Done();
        }
        private void StoogeSort(int left, int right)
        {
            if (right > left)
            {
                //swap the first and last elements if they are not in order
                if (list[left] > list[right])
                    Swap(left, right);
                //check if there are at least three elements
                int elements = right - left + 1;
                if (elements > 2)
                {
                    //split into thirds
                    elements /= 3;
                    //stooge the first two thirds
                    StoogeSort(left, right - elements);
                    //stooge the last two thirds
                    StoogeSort(left + elements, right);
                    //stooge the first two thirds again
                    StoogeSort(left, right - elements);
                }
            }
        }

        //this in-place variant of the strand sort is almost just as efficient as the standard
        public void StrandSort()
        {
            //first build a sorted strand at the end of the list
            int sorted = length - 1;
            while (sorted > 0 && !( list[sorted] < list[--sorted] ))
            {
            }
            ++sorted;
            //for (int i = sorted; --i > -1; )
            //    if (i < sorted && !(list[i] > list[sorted]))
            //        Swap(i, --sorted);

            //continue while there are at least four unordered elements left
            int subStart = 0;
            bool first = true;
            while (sorted > 3)
            {
                //build a sorted strand at the start of the list
                int sublist = subStart;
                int max = ( sorted + subStart ) / 2 - 1;
                if (subStart < max)
                {
                    for (int i = subStart ; ++i < sorted && sublist < max ; )
                        if (!( list[i] < list[sublist] ))
                            Swap(i, ++sublist);

                    //merge the strand with the sorted elements at the end
                    MergeAt(sorted - ++sublist + subStart, subStart, sublist - 1, sorted, length - 1);
                    sorted -= sublist - subStart;
                    if (first)
                        subStart = sublist;
                }
                else
                {
                    first = false;
                    subStart = 0;
                }
            }

            //insert the remaining elements individually
            int right = length - 1;
            while (sorted > 0)
                for (int i = --sorted ; i < right && list[i] > list[i + 1] ; )
                    Swap(i, ++i);

            Done();
        }

        //supporting methods shared by two or more sorts
        #region Shared

        //currenly unused
        ////this inserts the last few elements that cant be merged
        //private void Insert(int left, int mid, int right)
        //{
        //    //since this can be one of the least efficient parts of some sorts, we use a binary search to reduce comparisons
        //    while (mid > left)
        //    {
        //        //search bounds
        //        int low = mid--;
        //        int high = right;
        //        bool testLow = true;
        //        while (low < high)
        //        {
        //            //index to search
        //            int search = (low + high) / 2;
        //            //move the relevant bound inward
        //            int compVal = Obj.Compare(list[search], list[mid]);
        //            if (compVal > 0)
        //                high = search - 1;
        //            else if (compVal < 0)
        //                low = search + 1;
        //            else //they are equal
        //            {
        //                //we use low as the insert position
        //                low = search - 1;
        //                //no need to search farther
        //                testLow = false;
        //                break;
        //            }
        //        }
        //        //determine if the new element fits before or after the found one
        //        if (testLow && list[mid] < list[low])
        //            --low;
        //        //swap it into place
        //        for (int i = mid; i < low; )
        //            Swap(i, ++i);
        //    }
        //    ////alternatively, you could just compare the elements before each swap, simplifying this entire method to:
        //    //while (mid > left)
        //    //    for (int i = --mid; i < right && list[i] > list[i + 1]; )
        //    //        Swap(i, ++i);
        //}

        //merges two lists, leaving an unsorted left and sorted right
        //where all elements in the left section are smaller than the elements in the right
        private int MergeUpper(int left, int mid, ref int right)
        {
            int leftEnd = mid;
            int use;
            //bring in the starting indices of both lists creating space we can use to merge
            while (( use = leftEnd - left ) > mid - leftEnd)
            {
                if (left >= leftEnd)
                    ++mid;
                else if (mid > right)
                    ++left;
                else if (list[left] < list[mid])
                    ++left;
                else
                    ++mid;
            }
            //merge what we can of the lists, using the lower section of the right list as the work area
            MergeAt(use = mid - use, left, leftEnd - 1, mid, right);
            //set right to the last element in the left section 
            right = use - 1;
            //return the first unsorted element in the left section
            return left;
        }

        //starts at use and places each element from left and right in order
        //assumes the result will overlap right, so stops once left has no remaining elements
        private void MergeAt(int start, int left, int leftEnd, int right, int rightEnd)
        {
            //place each element in order
            while (( left <= leftEnd ) && ( right <= rightEnd ))
            {
                if (list[left] > list[right])
                    Swap(start++, right++);
                else
                    Swap(start++, left++);
            }
            //place the remaining elements from left
            while (left <= leftEnd)
                Swap(start++, left++);
            //we can assume anything remaining in right will already be in its final merged place
        }

        #endregion //Shared

        #endregion //Sorts

        //radix sorts are non-comparative so this doesnt quite fit in, but is fun nonetheless
        #region Radix

        //in-place lsd radix sort
        //this is the only algorithm that manupulates the raw uint data rather than swapping
        //I wanted to avoid this since the hidden information involved diminishes algorithm visibility, but radix already breaks the rule of only comparison sorts anyways
        public void RadixLSDSort()
        {
            ulong bit = 1ul;
            do
            {
                //place objects in either bucket depending on the value of the current bit
                List<Obj> zeros = new List<Obj>(), ones = new List<Obj>();
                for (int a = 0 ; a < length ; ++a)
                    if (( list[a].Value & bit ) == 0)
                        zeros.Add(list[a]);
                    else
                        ones.Add(list[a]);

                //replace the list with the concatenation of the buckets
                int b = 0;
                foreach (Obj c in zeros.Concat(ones))
                {
                    list[b++] = c;
                    Swapped();
                }

                //continue sorting with the next-highest bit
                bit <<= 1;
            } while (bit <= 1ul << 31);

            Done();
        }

        //in-place msd radix sort
        public void RadixMSDSort()
        {
            //start with highest uint bit
            RadixMSDSort(0, length, 1u << 31);

            Done();
        }
        private void RadixMSDSort(int start, int end, uint bit)
        {
            int a = start, b = end;
            while (a < b)
                if (( list[a].Value & bit ) == 0)
                {
                    //if the current bit is 0, leave the element at the bottom
                    a++;
                }
                else
                {
                    //if the current bit is 1, find the topmost 0-bit element
                    while (a < --b && ( list[b].Value & bit ) != 0)
                        ;
                    //swap it with the original 1-bit element
                    if (a < b)
                        Swap(a, b);
                }

            if (bit > 1)
            {
                //recursively sort both sides with the next-lowest bit
                bit >>= 1;
                RadixMSDSort(start, a, bit);
                RadixMSDSort(a, end, bit);
            }
        }

        #endregion //Radix

        //sorts programmed but left out for various reasons
        #region Removed

        //removed for being too similar to shell
        public void CombSort()
        {
            int step = length;
            bool swapped;
            do
            {
                if (step > 1)
                {
                    if (step < 5)
                        --step;
                    else
                        step = Program.Random.Round(step / 1.3);
                }
                swapped = false;
                for (int i = step ; i < length ; ++i)
                {
                    if (list[i - step] > list[i])
                    {
                        Swap(i, i - step);
                        swapped = true;
                    }
                }
            }
            while (step > 1 || swapped);

            Done();
        }

        //removed because it requires the list length to be a power of 2
        public void OddEvenMergeSort()
        {
            OddEvenMergeSort(0, length);

            Done();
        }
        //sorts a piece of length n of the array starting at position lo
        private void OddEvenMergeSort(int lo, int n)
        {
            if (n > 1)
            {
                int m = n / 2;
                OddEvenMergeSort(lo, m);
                OddEvenMergeSort(lo + m, m);
                OddEvenMergeSort(lo, n, 1);
            }
        }
        // lo is the starting position and n is the length of the piece to be merged, r is the distance of the elements to be compared
        private void OddEvenMergeSort(int lo, int n, int r)
        {
            int m = r * 2;
            if (m < n)
            {
                // even subsequence
                OddEvenMergeSort(lo, n, m);
                // odd subsequence
                OddEvenMergeSort(lo + r, n, m);
                for (int i = lo + r ; i + r < lo + n ; i += m)
                    if (list[i] > list[i + r])
                        Swap(i, i + r);
            }
            else
                if (list[lo] > list[lo + r])
                    Swap(lo, lo + r);
        }

        //removed for being lame an boring
        public void SelectionSort()
        {
            for (int a = 0 ; a < length - 1 ; ++a)
            {
                int min = a;
                for (int b = a + 1 ; b < length ; ++b)
                {
                    if (list[b] < list[min])
                    {
                        min = b;
                    }
                }
                Swap(a, min);
            }

            Done();
        }

        #endregion //Removed

        #region Smoothsort

        /* File: Smoothsort.hh
        * Author: Keith Schwarz (htiek@cs.stanford.edu)
        * An implementation of Dijkstra's Smoothsort algorithm, a modification of heapsort that runs in O(n lg n) in the worst case, but O(n) if the data
        * are already sorted. For more information about how this algorithm works and some of the details necessary for its proper operation, please see
        * http://www.keithschwarz.com/smoothsort/
        * This implementation is designed to work on a 32-bit machine and may have portability issues on 64-bit computers. In particular, I've only
        * precomputed the Leonardo numbers up 2^32, and so if you try to sort a sequence of length greater than that you'll run into trouble. Similarly,
        * I've used the tricky O(1) optimization to use a constant amount of space given the fact that the machine is 32 bits. */

        /* Function: Smoothsort(RandomIterator begin, RandomIterator end);
        * -----------------------------------------------------------------------
        * Sorts the input range into ascending order using the smoothsort algorithm. */
        //template <typename RandomIterator> 
        //void Smoothsort(RandomIterator begin, RandomIterator end); 

        /* Function: Smoothsort(RandomIterator begin, RandomIterator end, Comparator comp);
        * -----------------------------------------------------------------------
        * Sorts the input range into ascending order according to the strict total ordering comp using the smoothsort algorithm. */
        //template <typename RandomIterator, typename Comparator> 
        //void Smoothsort(RandomIterator begin, RandomIterator end, Comparator comp); 

        /* Implementation Below This Point */

        /* A constant containing the number of Leonardo numbers that can fit into 32 bits. For a 64-bit machine, you'll need to update this value and the */
        //const int kNumLeonardoNumbers = 46;

        /* A list of all the Leonardo numbers below 2^32, precomputed for efficiency.
        * Source: http://oeis.org/classic/b001595.txt */
        private static int[] kLeonardoNumbers = new[] 
        {
            1, 1, 3, 5, 9, 15, 25, 41, 67, 109, 177, 287, 465, 753, 1219, 1973, 3193, 5167, 8361, 13529, 21891,
            35421, 57313, 92735, 150049, 242785, 392835, 635621, 1028457, 1664079, 2692537, 4356617, 7049155,
            11405773, 18454929, 48315633, 78176337, 126491971, 204668309, 331160281, 535828591, 866988873, 1402817465
            //, 2269806339u, 3672623805u
        };

        /* A structure containing a bitvector encoding of the trees in a Leonardo heap. The representation is as a bitvector shifted down so that its
        * first digit is a one, along with the amount that it was shifted. */
        private class HeapShape
        {
            /* A bitvector capable of holding all the Leonardo numbers. */
            public ulong trees = 0x0ul;
            /* The shift amount, which is also the size of the smallest tree. */
            public int smallestTreeSize = 0;
        };

        /* Function: RandomIterator SecondChild(RandomIterator root)
        * ---------------------------------------------------------------------
        * Given an iterator to the root of Leonardo heap, returns an iterator to the root of that tree's second child. It's assumed that the heap is well-formed and that size > 1. */
        private int SecondChild(int root)
        {
            /* The second child root is always one step before the root. */
            return root - 1;
        }

        /* Function: RandomIterator FirstChild(RandomIterator root, int size)
        * ---------------------------------------------------------------------
        * Given an iterator to the root of Leonardo heap, returns an iterator to the root of that tree's first child. It's assumed that the heap is well-formed and that size > 1. */
        private int FirstChild(int root, int size)
        {
            /* Go to the second child, then step backwards L(size - 2) steps to skip over it. */
            return SecondChild(root) - kLeonardoNumbers[size - 2];
        }

        /* Function: RandomIterator LargerChild(RandomIterator root, int size, Comparator comp);
        * --------------------------------------------------------------------
        * Given an iterator to the root of a max-heap Leonardo tree, returns an iterator to its larger child. It's assumed that the heap is well-formatted and that the heap has order > 1. */
        private int LargerChild(int root, int size)
        {
            /* Get pointers to the first and second child. */
            int first = FirstChild(root, size);
            int second = SecondChild(root);

            /* Determine which is greater. */
            return list[first] < list[second] ? second : first;
        }

        /* Function: RebalanceSingleHeap(RandomIterator root, int size, Comparator comp);
        * --------------------------------------------------------------------
        * Given an iterator to the root of a single Leonardo tree that needs rebalancing, rebalances that tree using the standard "bubble-down" approach. */
        private void RebalanceSingleHeap(int root, int size)
        {
            /* Loop until the current node has no children, which happens when the order of the tree is 0 or 1. */
            while (size > 1)
            {
                /* Get pointers to the first and second child. */
                int first = FirstChild(root, size);
                int second = SecondChild(root);

                /* Determine which child is larger and remember the order of its tree. */
                int largerChild;
                int childSize;
                if (list[first] < list[second])
                {
                    largerChild = second;
                    // Second child is larger... 
                    childSize = size - 2;
                    // ... and has order k - 2. 
                }
                else
                {
                    largerChild = first;
                    // First child is larger... 
                    childSize = size - 1;
                    // ... and has order k - 1. 
                }

                /* If the root is bigger than this child, we're done. */
                if (!( list[root] < list[largerChild] ))
                    return;

                /* Otherwise, swap down and update our order. */
                Swap(root, largerChild);
                root = largerChild;
                size = childSize;
            }
        }

        /* Function: LeonardoHeapRectify(RandomIterator begin, RandomIterator end, HeapShape shape, Comparator comp);
        * ---------------------------------------------------------------------
        * Given an implicit Leonardo heap spanning [begin, end) that has just had an element inserted into it at the very end, along with the size
        * list for that heap, rectifies the heap structure by shuffling the new root down to the proper position and rebalancing the target heap. */
        private void LeonardoHeapRectify(int begin, int end, HeapShape tempShape)
        {
            HeapShape shape = new HeapShape();
            shape.smallestTreeSize = tempShape.smallestTreeSize;
            shape.trees = tempShape.trees;

            /* Back up the end iterator one step to get to the root of the rightmost heap. */
            int itr = end - 1;
            /* Keep track of the size of the last heap size that we visited. We need this so that once we've positioned the new node atop the correct heap we remember how large it is. */
            int lastHeapSize;

            /* Starting at the last heap and working backward, check whether we need to swap the root of the current heap with the previous root. */
            while (true)
            {
                /* Cache the size of the heap we're currently on top of. */
                lastHeapSize = shape.smallestTreeSize;

                /* If this is the very first heap in the tree, we're done. */
                if (itr - begin == kLeonardoNumbers[lastHeapSize] - 1)
                    break;

                /* We want to swap the previous root with this one if it's strictly greater than both the root of this tree and both its children.
                * In order to avoid weird edge cases when the current heap has size zero or size one, we'll compute what value will be compared against. */
                int toCompare = itr;

                /* If we aren't an order-0 or order-1 tree, we have two children, and need to check which of the three values is largest. */
                if (shape.smallestTreeSize > 1)
                {
                    /* Get the largest child and see if we need to change what we're comparing against. */
                    int largeChild = LargerChild(itr, shape.smallestTreeSize);

                    /* Update what element is being compared against. */
                    if (list[toCompare] < list[largeChild])
                        toCompare = largeChild;
                }

                /* Get a pointer to the root of the second heap by backing up the size of this heap. */
                int priorHeap = itr - kLeonardoNumbers[lastHeapSize];

                /* If we ran out of trees or the new tree root is less than the element we're comparing, we now have the new node at the top of the correct heap. */
                if (!( list[toCompare] < list[priorHeap] ))
                    break;

                /* Otherwise, do the swap and adjust our location. */
                Swap(itr, priorHeap);
                itr = priorHeap;

                /* Scan down until we find the heap before this one. We do this by continously shifting down the tree bitvector and bumping up the size
                * of the smallest tree until we hit a new tree. */
                do
                {
                    shape.trees >>= 1;
                    ++shape.smallestTreeSize;
                } while (( shape.trees & 1 ) == 0);
            }

            /* Finally, rebalance the current heap. */
            RebalanceSingleHeap(itr, lastHeapSize);
        }

        /* Function: LeonardoHeapAdd(RandomIterator begin, RandomIterator end, RandomIterator heapEnd, HeapShape& shape, Comparator comp);
        * ----------------------------------------------------------------------
        * Given an implicit Leonardo heap spanning [begin, end) in a range spanned by [begin, heapEnd], along with the shape and a comparator, increases the
        * size of that heap by one by inserting the element at end. */
        private void LeonardoHeapAdd(int begin, int end, int heapEnd, HeapShape shape)
        {
            /* There are three cases to consider, which are analogous to the cases in the proof that it is possible to partition the input into heaps of decreasing size:
            * 
            * Case 0: If there are no elements in the heap, add a tree of order 1.
            * Case 1: If the last two heaps have sizes that differ by one, we add the new element by merging the last two heaps.
            * Case 2: Otherwise, if the last heap has Leonardo number 1, add a singleton heap of Leonardo number 0.
            * Case 3: Otherwise, add a singleton heap of Leonardo number 1. */

            /* Case 0 represented by the first bit being a zero; it should always be one during normal operation.
            */
            if (( shape.trees & 1 ) == 0)
            {
                shape.trees |= 1;
                shape.smallestTreeSize = 1;
            }

            /* Case 1 would be represented by the last two bits of the bitvector both being set. */
            else if (( shape.trees & 2 ) == 2 && ( shape.trees & 1 ) == 1)
            {
                /* First, remove those two trees by shifting them off the bitvector. */
                shape.trees >>= 2;
                /* Set the last bit of the bitvector; we just added a tree of this size. */
                shape.trees |= 1;
                /* Finally, increase the size of the smallest tree by two, since the new Leonardo tree has order one greater than both of them. */
                shape.smallestTreeSize += 2;
            }

            /* Case two is represented by the size of the smallest tree being 1. */
            else if (shape.smallestTreeSize == 1)
            {
                /* Shift the bits up one spot so that we have room for the zero bit. */
                shape.trees <<= 1;
                shape.smallestTreeSize = 0;
                /* Set the bit. */
                shape.trees |= 1;
            }

            /* Case three is everything else. */
            else
            {
                /* We currently have a forest encoded with a format that looks like (W, n) for bitstring W and exponent n. We want to convert this to
                * (W00...01, 1) by shifting up n - 1 spaces, then setting the last bit. */
                shape.trees <<= shape.smallestTreeSize - 1;
                shape.trees |= 1;
                /* Set the smallest tree size to one, since that is the new smallest tree size. */
                shape.smallestTreeSize = 1;
            }

            /* At this point, we've set up a new tree. We need to see if this tree is at the final size it's going to take. If so, we'll do a full rectify
            * on it. Otherwise, all we need to do is maintain the heap property. */
            bool isLast = false;
            switch (shape.smallestTreeSize)
            {
            /* If this last heap has order 0, then it's in its final position only if it's the very last element of the array. */
            case 0:
                if (end + 1 == heapEnd)
                    isLast = true;
                break;

            /* If this last heap has order 1, then it's in its final position if it's the last element, or it's the penultimate element and it's not about to be merged. For simplicity */
            case 1:
                if (end + 1 == heapEnd || ( end + 2 == heapEnd && ( shape.trees & 2 ) == 0 ))
                    isLast = true;
                break;

            /* Otherwise, this heap is in its final position if there isn't enough room for the next Leonardo number and one extra element. */
            default:
                if (heapEnd - end - 1 < kLeonardoNumbers[shape.smallestTreeSize - 1] + 1)
                    isLast = true;
                break;
            }

            /* If this isn't a final heap, then just rebalance the current heap. */
            if (!isLast)
                RebalanceSingleHeap(end, shape.smallestTreeSize);
            /* Otherwise do a full rectify to put this node in its place. */
            else
                LeonardoHeapRectify(begin, end + 1, shape);
        }

        /* Function: LeonardoHeapRemove(RandomIterator begin, RandomIterator end, HeapShape& shape, Comparator comp);
        * ----------------------------------------------------------------------
        * Given an implicit Leonardo heap spanning [begin, end), along with the size list and a comparator, dequeues the element at end - 1 and rebalances 
        * the heap. Since the largest element of the heap is already at end, this essentially keeps the max element in its place and does a rebalance if necessary. */
        private void LeonardoHeapRemove(int begin, int end, HeapShape shape)
        {
            /* There are two cases to consider:
            * 
            * Case 1: The last heap is of order zero or one. In this case, removing it doesn't expose any new trees and we can just drop it from the list of trees.
            * Case 2: The last heap is of order two or greater. In this case, we exposed two new heaps, which may require rebalancing. */

            /* Case 1. */
            if (shape.smallestTreeSize <= 1)
            {
                /* Keep scanning up the list looking for the next tree. */
                do
                {
                    shape.trees >>= 1;
                    ++shape.smallestTreeSize;
                } while (shape.trees != 0 && ( shape.trees & 1 ) == 0);
                return;
            }

            /* Break open the last heap to expose two subheaps of order k - 2 and k - 1. This works by mapping the encoding (W1, n) to the encoding (W011, n - 2). */
            int heapOrder = shape.smallestTreeSize;
            shape.trees -= 1;
            shape.trees <<= 2;
            shape.trees |= 3;
            shape.smallestTreeSize -= 2;

            /* We now do the insertion-sort/rebalance operation on the larger exposed heap to put it in its proper place, then on the smaller of the two. But first, we need
            * to find where they are. This can be done by just looking up the first and second children of the former root, which was at end - 1. */
            int leftHeap = FirstChild(end - 1, heapOrder);
            int rightHeap = SecondChild(end - 1);

            /* Rebalance the left heap. For this step we'll pretend that there is one fewer heap than there actually is, since we're ignoring the rightmost heap. */
            HeapShape allButLast = new HeapShape();
            allButLast.smallestTreeSize = shape.smallestTreeSize;
            allButLast.trees = shape.trees;
            ++allButLast.smallestTreeSize;
            allButLast.trees >>= 1;

            /* We add one to the position of the left heap because the function assumes an exclusive range, while leftHeap is actually an iterator directly to where the root is. */
            LeonardoHeapRectify(begin, leftHeap + 1, allButLast);
            LeonardoHeapRectify(begin, rightHeap + 1, shape);
        }

        /* Actual smoothsort implementation. */
        public void Smoothsort()
        {
            /* Construct a shape object describing the empty heap. */
            HeapShape shape = new HeapShape();

            /* Convert the input into an implicit Leonardo heap. */
            for (int itr = 0 ; itr != length ; ++itr)
                LeonardoHeapAdd(0, itr, length, shape);

            /* Continuously dequeue from the implicit Leonardo heap until we've consumed all the elements. */
            for (int itr = length ; itr != 0 ; --itr)
                LeonardoHeapRemove(0, itr, shape);

            Done();
        }

        #endregion //Smoothsort

    }
}
