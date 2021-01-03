using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinomialCoefficient
{
   public class BinCoeffL : BinCoeffBase
   {
      // This class provides a way to access a table generated with the binomial coefficient by the underlying indexes.
      // It also determines the number of unique combinations with the binomial coefficient. The binomial coefficient
      // is used to calculate the total number of unique combinations for a given set of numbers (N), when grouped
      // by K items at a time.  Total number of unique combinations = N! / ( K! (N - K)! ).
      // It provides the same functionality as BinCoeff, except that it handles longs. It also does not
      // provide the capability to manage a generic table, since it would take too much memory.
      //
      // This class uses 64 bit unsigned longs and so is limited to 2^64 or 18,446,744,073,709,551,616 before it overflows.
      // One of the largest cases that can be used with this class is 66 choose 33, which yields
      // 7,219,428,434,016,265,740 unique combinations.
      //
      // This class was designed and written by Robert G. Bryan in October, 2012, and updated in Dec, 2020.
      // It is in the public domain.
      // Even though it has been tested, the user assumes full responsibility for any bugs or anomalies.
      //
      public ulong TotalCombos { get; set; } // Total number of unique combinations.
      public bool SubCaseOverflow { get; set; } // true means subcases may overflow; false means subcases probably will not overflow.
      // Gets set to true in CreatePascalsTriangle if any calculated value is larger than ulong.MaxValue.
      // Also, it this value is false, it does not imply that a subcase will not overflow when calculating the rank or
      // returning a combination. Even if this value is true, the main n choose k case used to create this instance
      // will not overflow when calculating the number of combinations, the rank, or obtaining the combination from the rank.
      //
      private List<ulong[]> PasTri; // Pascals' Triangle. Used to translate between a combination and the rank, and vice-a-versa.
      public BinCoeffL(int numItems, int groupSize, ulong totalCombos = 0)
      {
         // This constructor builds the index tables used to retrieve the index to the binomial coefficient table.
         // N is the number of items and K is the number of items in a group.
         //
         Init(numItems, groupSize);
         CreatePascalsTriangle();
         if (totalCombos == 0)
            TotalCombos = GetNumCombos();
         else
            TotalCombos = totalCombos;
      }

      private void CreatePascalsTriangle()
      {
         // This function creates Pascal's Triangle. If you are not familiar with Pascal's Triangle, it would be helpful to
         // see an example of it. See https://tablizingthebinomialcoeff.wordpress.com/ for an example + math explanation of what is happening here.
         // The Read Me.txt file also has an explanation.
         // The class variable SubCaseOverflow is set to true if overflow is detected when creating Pascal's Triangle.
         // This means that doing a subcase like 100 choose 80 from a main case of 100 choose 95 may fail. But, the main case of
         // 100 choose 95 won't overflow, just some subcases.
         //
         SubCaseOverflow = false;
         // If this is an N choose 1 case or N choose N case, then simply return since Pascal's Triangle is not used for these cases.
         if ((GroupSize == 1) || (GroupSize == NumItems))
            return;
         int loopIndex, loop, startIndex, endIndex;
         ulong value, incValue;
         ulong[] indexArray, indexArrayPrev, indexArrayLeast;
         //
         PasTri = new List<ulong[]>(GroupSizeM1);
         // Create the arrays used for each index.
         for (loop = 0; loop < GroupSizeM1; loop++)
         {
            indexArray = new ulong[NumItems];
            PasTri.Add(indexArray);
         }
         // Get the indexes values for the least significant index.
         indexArrayLeast = PasTri[GroupSizeM2];
         value = 1;
         incValue = 2;
         for (loop = 2; loop < indexArrayLeast.Length; loop++)
         {
            indexArrayLeast[loop] = value;
            value += incValue++;
         }
         // Get the binomial coefficients for the remaining indexes by adding the 2 previous values together.
         // Take a look at Pascal's Triangle to have a better understanding of what this code is doing: https://tablizingthebinomialcoeff.wordpress.com/
         startIndex = 3;
         endIndex = NumItems;
         for (loopIndex = GroupSizeM3; loopIndex >= 0; loopIndex--)
         {
            indexArrayPrev = PasTri[(loopIndex + 1)];
            indexArray = PasTri[loopIndex];
            indexArray[startIndex] = 1;
            for (loop = startIndex + 1; loop < endIndex; loop++)
            {
               // Check for overflow before adding. See https://codereview.stackexchange.com/a/37178 for more info.
               if ((loop - 1 >= indexArrayPrev.Length) || (indexArrayPrev[loop - 1] > (ulong.MaxValue - indexArray[loop - 1])))
               {
                  // The add operation will cause an overflow here. So, resize the array to the # of good values in it and set the flag to
                  // indicate that some subcases may overflow.
                  // What is a subcase? When Pascal's Triangle is created, it is created with a specific n choose k case in mind. For example,
                  // 10 choose 5. Some subcases of 10 choose 5 are 10 choose 4, 8 choose 5, and 7 choose 2. All these subcases may be used to
                  // efficiently obtain the rank or combination from the rank without having to recreate Pascal's Triangle.
                  // Howver, if the main case is 100 choose 95 (75,287,520 combinations), then many subcases will fail.
                  // For example, 100 choose 82 produces 30,664,510,802,988,208,300 combinations - which will not fit inisde of a 64-bit unsigned int.
                  // If fact, all subcases of 100 choose 82 down to 100 chose 18 will fail. But, that does not mean that the main case of
                  // 100 choose 95 or a subcase of 100 choose 83, which produces 6,650,134,872,937,201,800 combinations, can't be successfully used.
                  //
                  SubCaseOverflow = true;
                  Array.Resize(ref indexArray, loop);
                  PasTri[loopIndex] = indexArray;
                  break;
               }
               indexArray[loop] = indexArray[loop - 1] + indexArrayPrev[loop - 1];
            }
            startIndex++;
         }
         return;
      }

      public ulong GetNumCombos(int numItems = -1, int groupSize = -1)
      {
         // This method gets the total number of combos for numItems choose groupSize from Pascal's triangle.
         // If Pascal's triangle has not been created, then an alternative method is called to get the # of combinations.
         // If either numItems or groupSize < 0, then NumItems or GroupSize, respectively is used instead.
         // Both numItems and groupSize are optional parameters that provide a way to efficiently calculate the number of
         // combinations from Pascal's Triangle without having to re-create Pascal's Triangle from a different n choose k case. But,
         // both numItems and groupSize must be <= to the original NumItems and GroupSize, respectively, that were used to create the instance.
         // Zero is returned if overflow occurred.
         //
         string s;
         ulong numCombos = 0;
         numItems = (numItems < 0) ? NumItems : numItems;
         groupSize = (groupSize < 0) ? GroupSize : groupSize;
         if ((groupSize == 0) || (numItems == 0))
            return 0;
         if (groupSize == 1)
            return (uint)numItems;
         if (groupSize == numItems)
            return 1;
         if ((numItems > NumItems) || (groupSize > GroupSize))
         {
            s = "BinCoeffL:GetNumCombos: numItems > NumItems || groupSize > GroupSize. Neither is allowed.";
            ApplicationException ae = new ApplicationException(s);
            throw ae;
         }
         // if Pascal's Triangle has not been created, then use an alternate method to obtain the number of combos.
         if (PasTri == null)
         {
            numCombos = GetCombosCount(numItems, groupSize);
            return numCombos;
         }
         ulong[] indexArray;
         uint n = (uint)numItems - 1;
         int loopIndex;
         int startIndex;
         // C(n, k) = C(n, n - k), so use the smaller value if k > n - k.
         if (numItems - groupSize < groupSize)
            startIndex = GroupSize - (numItems - groupSize);
         else
            startIndex = GroupSize - groupSize;
         for (loopIndex = startIndex; loopIndex < GroupSizeM1; loopIndex++)
         {
            indexArray = PasTri[loopIndex];
            if ((indexArray.Length > n) && (numCombos <= ulong.MaxValue - indexArray[n]))
               numCombos += indexArray[n--];
            else
               return 0;
         }
         if (numCombos <= ulong.MaxValue - (n + 1))
            numCombos += n + 1;
         else
            return 0;
         return numCombos;
      }

      public ulong GetRank(bool sorted, int[] kIndexes, out bool overflow, int groupSize = -1)
      {
         // This function returns the proper index to an entry in the sorted binomial coefficient table from
         // the underlying values in KIndexes. For example, for the 13 chooose 5 example which
         // corresponds to 5 card poker hand ranks, then AKQJT (which is the greatest hand in the table) would
         // be passed as value 12, 11, 10, 9, and 8, and the return value would be 1286, which is the highest
         // element. Note that if the Sorted flag is false, then the values in KIndexes will be put into descending
         // order and returned that way. The sorted flag must be set to false if KIndexes needs to be sorted.
         // overflow is set to true if the operation overflows.
         //
         overflow = false;
         groupSize = (groupSize == -1) ? GroupSize : groupSize;
         // Handle the n choose n case.
         if (groupSize == NumItems)
            return 0;
         // Handle the n choose 1 case.
         if (groupSize == 1)
            return (ulong)kIndexes[0];
         // The times that Pascal's triangle may not have been legitimately created are handled above.
         // So, if it has not been created, then throw an exception.
         if (PasTri == null)
         {
            string s = "BinCoeffL:GetNumCombos: Pascal's Triangle has not been created." +
               "This could occur if the instance is created with 5 choose 5 and then 5 choose 3 is tried.";
            ApplicationException ae = new ApplicationException(s);
            throw ae;
         }
         ulong rank = 0;
         int loopIndex;
         long n;
         ulong[] indexArray;
         if (!sorted)
            ArraySorter<int>.SortDescending(kIndexes);
         int startIndex = GroupSize - groupSize;
         int kIndex = 0;
         for (loopIndex = startIndex; loopIndex < GroupSizeM1; loopIndex++)
         {
            indexArray = PasTri[loopIndex];
            n = kIndexes[kIndex++];
            // Check for overflow first.
            if ((indexArray.Length > n) && (rank <= ulong.MaxValue - indexArray[n]))
               rank += indexArray[n];
            else
            {
               overflow = true;
               return 0;
            }
         }
         if (rank <= ulong.MaxValue - (ulong)kIndexes[groupSize - 1])
            rank += (ulong)kIndexes[groupSize - 1];
         else
         {
            overflow = true;
            return 0;
         }
         return rank;
      }

      public ulong GetRank(int numItems, int groupSize, bool sorted, int[] kIndexes)
      {
         // This function returns the proper index to an entry in the sorted binomial coefficient table from
         // the underlying values in KIndexes. For example, for the 13 chooose 5 example which
         // corresponds to 5 card poker hand ranks, then AKQJT (which is the greatest hand in the table) would
         // be passed as value 12, 11, 10, 9, and 8, and the return value would be 1286, which is the highest
         // element.  Note that if the Sorted flag is false, then the values in KIndexes will be put longo sorted
         // order and returned that way.  The sorted flag must be set to false if KIndexes is not in descending order.
         //
         // Notes: 13 choose 4 = 715. Start at 2nd index array. Examples:
         // 13 choose 4 = 495 + 165 + 45 + 9 = 714 -> add one since these numbers start from zero -> 715.
         // 13 choose 3 = 220 +  55 + 10 = 285 (+ 1) = 286.
         // Both of the above cases are correct.
         // 12 choose 5 = 792. Start at 1st index array, 2nd entry. Examples:
         // 12 choose 5 = 462 + 210 + 84 + 28 + 7 = 791 (+ 1) = 792.
         // 11 choose 5 = 256 + 126 + 56 + 21 + 6 = 461 (+ 1) = 462.
         // Both of the above cases are correct.
         // 12 choose 4 = 495. Start at 2nd index and start off at 2nd entry in array.
         // 12 choose 4 = 494 (+ 1) = 495.
         //  9 choose 3 =  83 (+ 1) =  84. Start at 3rd index, 
         //  9 choose 3 =  56 + 21 + 6 = 83 ( + 1) = 84.
         //  8 choose 2 = 27 ( + 1)= 28. Start at 
         //
         // Proof that C(n, k) = C(n, n-k):
         // N! / K! (N - K)! = N! / (N - K) ! (N - (N - K))!
         // K! (N - K)! = (N - K)! (N - (N - K))!
         // K! = (N - (N - K))!
         // K! = K!
         // Benchmark of dynamic -> 18 times slower than static types:
         // https://dev.to/mokenyon/benchmarking-and-exploring-c-s-dynamic-keyword-1l5i#:~:text=You%20can%20see%20that%20the,18x%20slower%20and%20allocated%20memory.
         // http://ghcimdm4u.weebly.com/uploads/1/3/5/8/13589538/5.4.pdf - Proof of each binomial coefficient can be obtained from N choose K.
         // https://www.mathsisfun.com/algebra/binomial-theorem.html#coefficients - shows how Pascal's triangle is composed of binomial coefficients.
         // https://math.libretexts.org/Bookshelves/Mathematical_Logic_and_Proof/Book%3A_Book_of_Proof_(Hammack)/03%3A_Counting/3.06%3A_Pascal%E2%80%99s_Triangle_and_the_Binomial_Theorem
         // The above link has a good explanation on Pascal's Triangle and the Binomial Theorem.
         // https://math.stackexchange.com/questions/453843/why-does-pascals-triangle-give-the-binomial-coefficients - Why does Pascal's Triangle give the Binomial Coefficients?
         // https://en.wikipedia.org/wiki/Combinatorial_number_system - Combinatorial number system (Ordering Combinations).
         // https://www.developertyrone.com/blog/generating-the-mth-lexicographical-element-of-a-mathematical-combination/ - Generating the mth Lexicographical Element of a Mathematical Combination
         // https://stackoverflow.com/questions/29010699/can-i-add-two-generic-values-in-java - indicates that Java has same issue as C# when adding generic variables.
         // https://arxiv.org/pdf/1601.05794.pdf proof of n choose k = rank = c1 choose k + c2 choose (k - 1) + ...  + cn choose 1,
         // where c1, c2, ... ck are one of the kIndexes and k is groupSize. PROOF OF BIJECTION FOR COMBINATORIAL NUMBER SYSTEM.
         // 
         //  2 ^ 32 =              4,294,967,296
         // 34 C 17 =              2,333,606,220
         // 35 C 16 =              4,059,928,950
         // 35 C 17 =              4,537,567,650
         // 50 C 25 =        126,410,606,437,752
         //  2 ^ 64 = 18,446,744,073,709,551,616
         // 67 C 33 = 14,226,520,737,620,288,370
         // Calling BinCoeffBase.GetNumCombos(67, 33, out bool overflow) overflows.

         // 68 C 33 = 27,640,097,433,090,845,976
         //
         // 100  C   50 = 100,891,344,545,564,193,334,812,497,256
         // 200  C  100 =  90,548,514,656,103,281,165,404,177,077,484,163,874,504,589,675,413,336,841,320
         // 1000 C  500 = 27028824094543656951561469362597527549615200844654828700739287510662542870552219389861248392450237016536260608502154610480220975
         //               00506799175498942196995184754236654842637517333561624640797378873443645741611194976045710449857562878805146009942194267523669158
         //               56603136862602484428109296905863799821216320
         if ((numItems > NumItems) || (groupSize > GroupSize))
         {
            ApplicationException ae = new ApplicationException("BinCoeffL:GetRank: Input parameter(s) greater than expected.");
            throw ae;
         }
         int loopIndex;
         long n;
         ulong rank = 0;
         ulong[] indexArray;

         if (!sorted)
            ArraySorter<int>.SortDescending(kIndexes);
         int groupOffset = GroupSize - groupSize;
         int startIndex = NumItems - numItems;
         for (loopIndex = startIndex; loopIndex < GroupSize - 1; loopIndex++)
         {
            indexArray = PasTri[loopIndex];
            n = kIndexes[loopIndex + groupOffset];
            rank += indexArray[n];
         }
         rank += (ulong)kIndexes[GroupSize - 1];
         return rank;
      }

      public void GetCombFromRank(ulong rank, int[] kIndexes, int numItems = -1, int groupSize = -1)
      {
         // This function returns the combination in kIndexes from the specified rank.
         // This is the reverse of the GetRank method. The correct combination is returned in descending order
         // in kIndexes.
         // Pascal's Triangle must have been created. Otherwise, an exception is thrown.
         // groupSize - if specified, then must be <= GroupSize.
         // numItems  - only used in n choose n cases, which allows cases of 4 choose 4 from a case of 10 choose 5.
         //
         string s;
         groupSize = (groupSize < 0) ? GroupSize : groupSize;
         numItems = (numItems < 0) ? NumItems : numItems;
         if ((groupSize == 0) || (numItems == 0))
         {
            s = "BinCoeffL:GetCombFromRank: groupSize or numItems is zero. Must not be 0.";
            ApplicationException ae = new ApplicationException(s);
            throw ae;
         }
         if (groupSize > GroupSize)
         {
            s = "BinCoeffL:GetCombFromRank: groupSize must be <= GroupSize used when the instance was created.";
            ApplicationException ae = new ApplicationException(s);
            throw ae;
         }
         if (kIndexes.Length > GroupSize)
         {
            s = "BinCoeffL:GetCombFromRank: Increasing the length of kIndexes from the original size is not allowed.";
            ApplicationException ae = new ApplicationException(s);
            throw ae;
         }
         // Handle the N choose 1 case.
         if (groupSize == 1)
         {
            kIndexes[0] = (int)rank;
            return;
         }
         int loopIndex;
         // Handle the n choose n case.
         if (numItems == groupSize)
         {
            int val = numItems - 1;
            for (loopIndex = 0; loopIndex < groupSize; loopIndex++)
            {
               kIndexes[loopIndex] = val--;
            }
            return;
         }
         // If Pascal's Triangle has not been created, then throw an exception.
         if (PasTri == null)
         {
            ApplicationException ae = new ApplicationException("BinCoeffL:GetCombFromRank: Pascal's Triangle has not been created.");
            throw ae;
         }
         int index = 0, loop, startIndex = GroupSize - groupSize;
         ulong remValue = rank;
         ulong[] indexArray;
         for (loopIndex = startIndex; loopIndex < GroupSizeM1; loopIndex++)
         {
            indexArray = PasTri[loopIndex];
            // If remValue is zero, then look for the last zero value in the array.
            if (remValue == 0)
            {
               for (loop = 1; loop < indexArray.Length; loop++)
               {
                  if (indexArray[loop] > 0)
                  {
                     index = loop - 1;
                     break;
                  }
               }
            }
            else
            {
               index = Array.BinarySearch(indexArray, remValue);
               // If the binary search failed to find an exact match, it returns a negative number that is the complement
               // of the index that should be taken. It is the first index whose value is larger than the search term.
               if (index < 0)
                  index = ~index - 1;
            }
            kIndexes[loopIndex - startIndex] = index;
            remValue -= indexArray[index];
         }
         kIndexes[groupSize - 1] = (int)remValue;
      }

      protected override void GetCombFromRank<U>(U rank, int offset, int[] kIndexes)
      {
         // This method is provided to support the generic method OutputCombos.
         // The idea is to provide 3 methods to handle the translation from generic to concrete type.
         // The reason why this is needed is because C# does not currently provide (as of Ver 8) the capabaility
         // to do numberic operations on generic variables, unlike C++. So, for example, an error is generated from:
         // rank = rank + 1;
         // The compiler does not know that rank is an int. Microsoft should consider adding an AllowIntOps generic constraint
         // so that generic classes and methods could better support integer operations on generic variables.
         //
         long r = (long)Convert.ChangeType(rank, typeof(long)); // If this does not work, then try dynamic.
         r += offset;
         GetCombFromRank((uint)r, kIndexes);
      }
   }
}
