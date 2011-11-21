using System;
using System.Collections.Generic;
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
        //and ensures that we can stop the sort at any time and not corrupt the values in the list
        //they are all also implemented as purely comparison sorts
        #region Sorts

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
                KronrodSort(left, mid, right, count / Math.Log(count, 2));
            }
            else if (count > 1 && list[left] > list[right])
                Swap(left, right);
        }
        //takes in two sections, an unsorted left and sorted right, and sorts
        private void KronrodSort(int left, int mid, int right, double smallBlock)
        {
            int unsorted = mid - left;
            //since merging a 'small' list with a 'large' one is highly inefficient
            //we use a different algorithm if a 'small' block is encountered
            if (unsorted < smallBlock)
            {
                //sort the initial  unsorted elements
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
        private void StoogeSort(int start, int end)
        {
            if (end > start)
            {
                //swap the first and last elements if they are not in order
                if (list[start] > list[end])
                    Swap(start, end);
                //check if there are at least three elements
                int elements = end - start + 1;
                if (elements > 2)
                {
                    //split into thirds
                    elements /= 3;
                    //stooge the first two thirds
                    StoogeSort(start, end - elements);
                    //stooge the last two thirds
                    StoogeSort(start + elements, end);
                    //stooge the first two thirds again
                    StoogeSort(start, end - elements);
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

        //sorts programmed but left out for various reasons
        #region Removed

        //removed for being too similar to shell
        //public void CombSort()
        //{
        //    int step = length;
        //    bool swapped;
        //    do
        //    {
        //        if (step > 1)
        //        {
        //            if (step < 5)
        //                --step;
        //            else
        //                step = Program.Random.Round(step / 1.3);
        //        }
        //        swapped = false;
        //        for (int i = step; i < length; ++i)
        //        {
        //            if (list[i - step] > list[i])
        //            {
        //                Swap(i, i - step);
        //                swapped = true;
        //            }
        //        }
        //    }
        //    while (step > 1 || swapped);

        //    Done();
        //}

        //removed for being lame an boring
        //public void SelectionSort()
        //{
        //    for (int a = 0; a < length - 1; ++a)
        //    {
        //        int min = a;
        //        for (int b = a + 1; b < length; ++b)
        //        {
        //            if (list[b] < list[min])
        //            {
        //                min = b;
        //            }
        //        }
        //        Swap(a, min);
        //    }

        //    Done();
        //}

        #endregion //Removed
    }
}
