using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BinomialCoefficient
{
   public class BinCoeffBigInt : BinCoeffBase
   {
      // This class provides a big integer implementation for working with the binomial coefficient. The BigInteger class does not
      // overflow and works until memory is exhausted.
      // See BinCoeffBase for more detailed functionality info.
      //
      public BigInteger TotalCombos { get; set; } // Holds the total number of unique combos for the N choose K case.
      private List<BigInteger[]> PasTri; // Pascals' Triangle. Used to translate between a combination and the rank, and vice-a-versa.
      //
      public BinCoeffBigInt(int numItems, int groupSize, BigInteger totalCombos = new BigInteger())
      {
         // This constructor builds the index tables used to retrieve the index to the binomial coefficient table.
         // n is the number of items and k is the number of items in a group, and reflects the case n choose k.
         //
         Init(numItems, groupSize);
         CreatePascalsTriangle();
         if (totalCombos == 0)
            TotalCombos = GetNumCombos(numItems, groupSize);
         else
            TotalCombos = totalCombos;
      }

      private void CreatePascalsTriangle()
      {
         // This function creates Pascal's Triangle. If you are not familiar with Pascal's Triangle, it would be helpful to
         // see an example of it. See https://tablizingthebinomialcoeff.wordpress.com/ for an example + math explanation of what is happening here.
         //
         // If this is an N choose 1 case or N choose N case, then simply return since Pascal's Triangle is not used for these cases.
         if ((GroupSize == 1) || (GroupSize == NumItems))
            return;
         int loopIndex, loop, startIndex, endIndex;
         BigInteger value, incValue;
         BigInteger[] indexArray, indexArrayPrev, indexArrayLeast;
         //
         PasTri = new List<BigInteger[]>(GroupSizeM1);
         // Create the arrays used for each index.
         for (loop = 0; loop < GroupSizeM1; loop++)
         {
            indexArray = new BigInteger[NumItems];
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
         // Get the index values for the remaining indexes.
         startIndex = 3;
         endIndex = NumItems;
         for (loopIndex = GroupSizeM3; loopIndex >= 0; loopIndex--)
         {
            indexArrayPrev = PasTri[(loopIndex + 1)];
            indexArray = PasTri[loopIndex];
            indexArray[startIndex] = 1;
            for (loop = startIndex + 1; loop < endIndex; loop++)
            {
               indexArray[loop] = indexArray[loop - 1] + indexArrayPrev[loop - 1];
            }
            startIndex++;
            // endIndex++;
         }
      }

      public BigInteger GetNumCombos(int numItems = -1, int groupSize = -1)
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
         BigInteger numCombos = 0;
         //
         numItems = (numItems == -1) ? NumItems : numItems;
         groupSize = (groupSize == -1) ? GroupSize : groupSize;
         if ((numItems > NumItems) || (groupSize > GroupSize))
         {
            s = "BinCoeffBigInt:GetNumCombos: numItems > NumItems || groupSize > GroupSize. Neither is allowed. Create a new instance instead.";
            ApplicationException ae = new ApplicationException(s);
            throw ae;
         }
         if ((numItems == 0) || (groupSize == 0))
         {
            s = "BinCoeffBigInt:GetNumCombos: numItems or groupSize equals zero. Neither is allowed.";
            ApplicationException ae = new ApplicationException(s);
            throw ae;
         }
         if ((groupSize == 1) || (numItems == groupSize + 1))
            return numItems;
         if (groupSize == numItems)
            return 1;
         // There are times when Pascal's triangle may not have been legitimately created. For example 5 choose 5.
         // If this method is called, for example, with 5 choose 3 after being created with a 5 choose 5 case, then this is handled here.
         if (PasTri == null)
         {
            numCombos = Combination.GetNumCombos(numItems, groupSize);
            return numCombos;
         }
         uint n = (uint)numItems - 1;
         int startIndex = GroupSize - groupSize;
         BigInteger[] indexArray = PasTri[startIndex];
         int endIndex = indexArray.Length - 1;
         if (groupSize == 2)
         {
            if (numItems != NumItems)
               endIndex -= (NumItems - numItems);
            numCombos = indexArray[endIndex] + (uint)numItems - 1;
         }
         else
         {
            if (numItems == NumItems)
            {
               BigInteger[] indexArrayPrev = PasTri[startIndex + 1];
               int endIndexPrev = indexArrayPrev.Length - 1;
               numCombos = indexArray[endIndex] + indexArrayPrev[endIndexPrev];
            }
            else
            {
               endIndex = endIndex - (NumItems - numItems) + 1;
               return indexArray[endIndex];
            }
         }
         return numCombos;
      }

      public BigInteger GetRank(bool sorted, int[] kIndexes, int groupSize = -1)
      {
         // This method returns the rank of the combination in kIndexes. For example, with the 13 chooose 5 case which
         // corresponds to 5 card poker hand ranks, then AKQJT (which is the greatest hand in the table) would
         // be passed as value 12, 11, 10, 9, and 8, and the return value would be 1286, which is the highest
         // element. Note that if the Sorted flag is false, then the values in KIndexes will be put into sorted
         // descending order and returned that way. The sorted flag must be set to false if KIndexes is not in descending order.
         //
         // If the optional argument groupSize is specified, then it must be <= to the GroupSize used to create the instance.
         //
         groupSize = (groupSize == -1) ? GroupSize : groupSize;
         // Handle the n choose 1 case.
         if (groupSize == 1)
            return kIndexes[0];
         // Handle the n choose n case.
         if (groupSize == NumItems)
            return 0;
         int loopIndex, n;
         // The times that Pascal's triangle may not have been legitimately created are handled above.
         // So, if it has not been created, then throw an exception.
         if (PasTri == null)
         {
            string s = "BinCoeffBigInt:GetNumCombos: Error - Pascal's Triangle has not been created.";
            ApplicationException ae = new ApplicationException(s);
            throw ae;
         }
         BigInteger rank = 0;
         BigInteger[] indexArray;
         if (!sorted)
            ArraySorter<int>.SortDescending(kIndexes);
         int startIndex = GroupSize - groupSize;
         int kIndex = 0;
         for (loopIndex = startIndex; loopIndex < GroupSizeM1; loopIndex++)
         {
            indexArray = PasTri[loopIndex];
            n = kIndexes[kIndex++];
            rank += indexArray[n];
         }
         rank += kIndexes[groupSize - 1];
         return rank;
      }

      public void GetCombFromRank(BigInteger rank, int[] kIndexes, int numItems = -1, int groupSize = -1)
      {
         // This function returns the combination in kIndexes from the specified rank.
         // This is the reverse of the GetRank method. The correct combination is returned in descending order
         // in kIndexes.
         // Pascal's Triangle must have been created. Otherwise, an exception is thrown.
         // groupSize - if specified, then must be <= GroupSize.
         // numItems  - only used in n choose n cases.
         //
         string s;
         numItems = (numItems < 0) ? NumItems : numItems;
         groupSize = (groupSize < 0) ? GroupSize : groupSize;
         if (groupSize > GroupSize)
         {
            s = "BinCoeff:GetCombFromRank: groupSize must be <= GroupSize used when the instance was created.";
            ApplicationException ae = new ApplicationException(s);
            throw ae;
         }
         // Handle the n choose 1 case.
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
            s = "BinCoeff:GetNumCombos: Pascal's Triangle has not been created. This could occur if the instance is created with 5 choose 5 and then 5 choose 3 is tried.";
            ApplicationException ae = new ApplicationException(s);
            throw ae;
         }
         int index = 0, loop, startIndex = GroupSize - groupSize;
         BigInteger remValue = rank;
         BigInteger[] indexArray;
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
         BigInteger r = (BigInteger)Convert.ChangeType(rank, typeof(BigInteger)); // If this does not work, then try dynamic.
         if (offset < 0)
            r -= offset;
         else
            r += offset;
         GetCombFromRank(r, kIndexes);
      }
   }
}
