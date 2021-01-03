using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinomialCoefficient
{
   public class BinCoeffL : BinCoeffBase
   {
      // This class provides a 64-bit integer implementation for working with the binomial coefficient.
      // SeeRead Me.txt file for more detailed info.
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
         //
         // Both numItems and groupSize are optional parameters that provide a way to efficiently calculate the number of
         // combinations from Pascal's Triangle without having to re-create Pascal's Triangle for a different case. But,
         // both numItems and groupSize must be <= to the original NumItems and GroupSize, respectively, that were used to create the instance.
         // If either numItems or groupSize < 0, then NumItems or GroupSize, respectively, is used instead.
         //
         // Zero is returned if overflow occurred.
         // The expected runtime of this algorithm is O(1) when Pascal's Triangle is used.
         //
         string s;
         ulong numCombos = 0;
         numItems = (numItems < 0) ? NumItems : numItems;
         groupSize = (groupSize < 0) ? GroupSize : groupSize;
         if ((numItems == 0) || (groupSize == 0))
         {
            s = "BinCoeffL:GetNumCombos: numItems or groupSize equals zero. Neither is allowed.";
            ApplicationException ae = new ApplicationException(s);
            throw ae;
         }
         if ((numItems > NumItems) || (groupSize > GroupSize))
         {
            s = "BinCoeffL:GetNumCombos: numItems > NumItems || groupSize > GroupSize. Neither is allowed.";
            ApplicationException ae = new ApplicationException(s);
            throw ae;
         }
         if (groupSize == numItems)
            return 1;
         if ((groupSize == 1) || (numItems == groupSize + 1))
            return (uint)numItems;
         // if Pascal's Triangle has not been created, then use an alternate method to obtain the number of combos.
         if (PasTri == null)
         {
            numCombos = Combination.GetNumCombos(numItems, groupSize);
            return numCombos;
         }
         uint n = (uint)numItems - 1;
         int startIndex = GroupSize - groupSize;
         ulong[] indexArray = PasTri[startIndex];
         int endIndex = indexArray.Length - 1;
         if (groupSize == 2)
         {
            endIndex -= (NumItems - numItems);
            if ((indexArray.Length > endIndex) && (ulong.MaxValue - indexArray[endIndex] > (uint)numItems - 1))
               numCombos = indexArray[endIndex] + (uint)numItems - 1;
         }
         else
         {
            if (numItems == NumItems)
            {
               ulong[] indexArrayPrev = PasTri[startIndex + 1];
               int endIndexPrev = indexArrayPrev.Length - 1;
               if ((indexArray.Length > n) && (indexArrayPrev.Length > n) && (ulong.MaxValue - indexArray[endIndex] > indexArrayPrev[endIndexPrev]))
                  numCombos = indexArray[endIndex] + indexArrayPrev[endIndexPrev];
            }
            else
            {
               endIndex = endIndex - (NumItems - numItems) + 1;
               if (indexArray.Length > endIndex)
                  return indexArray[endIndex];
            }
         }
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
         string s;
         if (groupSize == 0)
         {
            s = "BinCoeffL:GetRank: groupSize equals zero. This is not allowed.";
            ApplicationException ae = new ApplicationException(s);
            throw ae;
         }
         if (groupSize > GroupSize)
         {
            s = "BinCoeffL:GetRank: numItems > NumItems. Not allowed. Create a new instance instead.";
            ApplicationException ae = new ApplicationException(s);
            throw ae;
         }
         // The times that Pascal's triangle may not have been legitimately created are handled above.
         // So, if it has not been created, then throw an exception.
         if (PasTri == null)
         {
            s = "BinCoeffL:GetRank: Pascal's Triangle has not been created." +
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
         numItems = (numItems < 0) ? NumItems : numItems;
         groupSize = (groupSize < 0) ? GroupSize : groupSize;
         if ((groupSize == 0) || (numItems == 0))
         {
            s = "BinCoeffL:GetCombFromRank: groupSize or numItems is zero. Not allowed.";
            ApplicationException ae = new ApplicationException(s);
            throw ae;
         }
         if ((groupSize == 0) || (numItems == 0))
         {
            s = "BinCoeffL:GetCombFromRank: groupSize or numItems is zero. Must not be 0.";
            ApplicationException ae = new ApplicationException(s);
            throw ae;
         }
         if ((groupSize > GroupSize) || (numItems > NumItems))
         {
            s = "BinCoeffL:GetCombFromRank: groupSize > GroupSize or numItems > NumItems. Neither is allowed. Create a new instance instead.";
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
            // If the rest of the combination is sequential (i.e. 876543210), then handle that here.
            remValue -= indexArray[index];
            if (remValue <= 1)
            {
               if (remValue == 1)
                  kIndexes[++loopIndex - startIndex] = groupSize - loopIndex + startIndex;
               for (loop = ++loopIndex - startIndex; loop < groupSize; loop++)
                  kIndexes[loop] = groupSize - 1 - loop;
               return;
            }
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
