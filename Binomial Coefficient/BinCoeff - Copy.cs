using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Data;
using System.Diagnostics;
using System.IO;

namespace BinomialCoefficient
{
   public class BinCoeff<T> : BinCoeffBase
   {
      // This class provides a 32-bit integer implementation for working with the binomial coefficient.
      // See BinCoeffBase for more detailed info.
      //
      // This class was designed and originally written by Robert G. Bryan in April, 2011.
      // Updated on 4/2015 to fix cases involving N choose 1 and also includes a new version called BinCoeffL that works with long values.
      // Updated in 12/2020 to add the following:
      // 1. Added an optional parameter to GetRank & GetCombFromRank that returns the rank / combo of any n' choose k' case such that
      //    n' and k' are <= n & k, respectively. The benefit of this is that Pascal's triangle does not have to be recreated.
      // 2. Improved testing with more edge cases and consolidated the code.
      // 3. Cleaned up method names and variable names to be more descriptive and to use Microsoft recommended variable naming guidelines.
      // This code is in the public domain.
      // Even though it has been tested, the user assumes full responsibility for any bugs or anomalies.
      // Recommend that you do your own tests for your specific n choose k cases.
      //
      public uint TotalCombos { get; set; } // Total number of unique combinations.
      public bool SubCaseOverflow { get; set; } // true means subcases may overflow; false means subcases probably will not overflow.
      // Gets set to true in CreatePascalsTriangle if any calculated value is larger than ulong.MaxValue.
      // Also, it this value is false, it does not imply that a subcase will not overflow when calculating the rank or
      // returning a combination. Even if this value is true, the main n choose k case used to create this instance
      // will not overflow when calculating the number of combinations, the rank, or obtaining the combination from the rank.
      private List<uint[]> PasTri; // Pascals' Triangle. Used to translate between a combination and the rank, and vice-a-versa.
      private List<T> TableData;   // Holds a list of the objects the user wants to create. This table is optionally
                                   // created if the user wants this class to manage the table data.
      //
      public BinCoeff(int numItems, int groupSize, uint totalCombos = 0, bool initTable = false)
      {
         // This constructor builds the index tables used to retrieve the index to the binomial coefficient table.
         // n is the number of items and k is the number of items in a group, and reflects the case n choose k.
         //
         Init(numItems, groupSize);
         CreatePascalsTriangle();
         TotalCombos = (totalCombos == 0) ? GetNumCombos() : totalCombos;
         if (initTable)
            InitializeTable();
      }

      private void CreatePascalsTriangle()
      {
         // This function creates Pascal's Triangle. If you are not familiar with Pascal's Triangle, it would be helpful to
         // see an example of it. See https://tablizingthebinomialcoeff.wordpress.com/ for an example + math explanation of what is happening here.
         // The Read Me.txt file also has an explanation.
         // The class variable SubCaseOverflow is set to true if overflow is detected when creating Pascal's Triangle.
         // This means that doing a subcase like 100 choose 93 from a main case of 100 choose 95 may fail. But, the main case of
         // 100 choose 95 won't overflow, just some subcases.
         //
         SubCaseOverflow = false;
         // If this is an N choose 1 case or N choose N case, then simply return since Pascal's Triangle is not used for these cases.
         if ((GroupSize == 1) || (GroupSize == NumItems))
            return;
         int loopIndex, loop, startIndex, endIndex;
         uint value, incValue;
         uint[] indexArray, indexArrayPrev, indexArrayLeast;
         //
         PasTri = new List<uint[]>(GroupSizeM1);
         // Create the arrays used for each index.
         for (loop = 0; loop < GroupSizeM1; loop++)
         {
            indexArray = new uint[NumItems];
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
               // Check for overflow before adding. See https://codereview.stackexchange.com/a/37178 for more info.
               if ((loop - 1 >= indexArrayPrev.Length) || (indexArrayPrev[loop - 1] > (ulong.MaxValue - indexArray[loop - 1])))
               {
                  // The add operation will cause an overflow here. So, resize the array to the # of good values in it and set the return flag to
                  // indicate that some subcases may overflow.
                  // What is a subcase? When Pascal's Triangle is created, it is created with a specific n choose k case in mind. For example,
                  // 10 choose 5. Some subcases of 10 choose 5 are 10 choose 4, 8 choose 5, and 7 choose 2. All these subcases may be used to
                  // efficiently obtain the rank or combination from the rank without having to recreate Pascal's Triangle.
                  // Howver, if the main case is 100 choose 95, then many subcases will fail.
                  // For example, 100 choose 93 produces 16,007,560,800 combinations - which will not fit inisde of a 32-bit unsigned int.
                  // If fact, all subcases of 100 choose 93 down to 100 chose 7 will fail. But, that does not mean that the main case of
                  // 100 choose 95 or a subcase of 100 choose 94, which produces 1,192,052,400 combinations, can't be successfully used.
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

      public uint GetNumCombos(int numItems = -1, int groupSize = -1)
      {
         // This method gets the total number of combos for numItems choose groupSize from Pascal's triangle.
         // If Pascal's triangle has not been created, then an alternative method is called to get the # of combinations.
         // If either numItems or groupSize < 0, then NumItems or GroupSize, respectively is used instead.
         // Both numItems and groupSize are optional parameters that provide a way to efficiently calculate the number of
         // combinations from Pascal's Triangle without having to re-create Pascal's Triangle for a different case. But,
         // both numItems and groupSize must be <= to the original NumItems and GroupSize, respectively, that were used to create the instance.
         // If either numItems or groupSize < 0, then NumItems or GroupSize, respectively, is used instead.
         // Zero is returned if overflow occurred.
         //
         string s;
         ulong numCombos = 0;
         //
         numItems = (numItems < 0) ? NumItems : numItems;
         groupSize = (groupSize < 0) ? GroupSize : groupSize;
         if ((groupSize == 1) || (numItems == groupSize + 1))
            return (uint)numItems;
         if ((numItems > NumItems) || (groupSize > GroupSize))
         {
            s = "BinCoeffL:GetNumCombos: numItems > NumItems || groupSize > GroupSize. Neither is allowed.";
            ApplicationException ae = new ApplicationException(s);
            throw ae;
         }
         if ((numItems == 0) || (groupSize == 0))
         {
            s = "BinCoeffL:GetNumCombos: numItems or groupSize equals zero. Neither is allowed.";
            ApplicationException ae = new ApplicationException(s);
            throw ae;
         }
         if (groupSize == numItems)
            return 1;
         // if Pascal's Triangle has not been created, then use an alternate method to obtain the number of combos.
         if (PasTri == null)
         {
            numCombos = GetCombosCount(numItems, groupSize);
            if (numCombos > uint.MaxValue)
               return 0;
            return (uint)numCombos;
         }
         uint[] indexArray;
         uint n = (uint)numItems - 1;
         int loopIndex;
         int startIndex;
         // C(n, k) = C(n, n - k), so use the smaller value if k > n - k.
         if (numItems - groupSize < groupSize)
            startIndex = GroupSize - (numItems - groupSize);
         else
            startIndex = GroupSize - groupSize;
         // Get the number of combinations from Pascal's Triangle. If the count exceeds the max value of a uint, then return 0.
         for (loopIndex = startIndex; loopIndex < GroupSizeM1; loopIndex++)
         {
            indexArray = PasTri[loopIndex];
            if ((indexArray.Length > n) && (numCombos <= uint.MaxValue - indexArray[n]))
               numCombos += indexArray[n--];
            else
               return 0;
         }
         if (numCombos <= uint.MaxValue - (n + 1))
            numCombos += n + 1;
         else
            return 0;
         return (uint)numCombos;
      }

      public uint GetFastNumCombos(int numItems = -1, int groupSize = -1)
      {
         // This method gets the total number of combos for numItems choose groupSize from Pascal's triangle.
         // This method is faster than GetNumCombos because it either obtains the number of combinations in a single
         // look up from Pascal's Triangle - in the case of a subcase where numItems < NumItems, or in two lookups
         // for all other cases.
         // If Pascal's triangle has not been created, then an alternative method is called to get the # of combinations.
         // If either numItems or groupSize < 0, then NumItems or GroupSize, respectively is used instead.
         // Both numItems and groupSize are optional parameters that provide a way to efficiently calculate the number of
         // combinations from Pascal's Triangle without having to re-create Pascal's Triangle for a different case. But,
         // both numItems and groupSize must be <= to the original NumItems and GroupSize, respectively, that were used to create the instance.
         // If either numItems or groupSize < 0, then NumItems or GroupSize, respectively, is used instead.
         // Zero is returned if overflow occurred.
         //
         string s;
         ulong numCombos = 0;
         //
         numItems = (numItems < 0) ? NumItems : numItems;
         groupSize = (groupSize < 0) ? GroupSize : groupSize;
         if ((groupSize == 1) || (numItems == groupSize + 1))
            return (uint)numItems;
         if ((numItems > NumItems) || (groupSize > GroupSize))
         {
            s = "BinCoeffL:GetNumCombos: numItems > NumItems || groupSize > GroupSize. Neither is allowed.";
            ApplicationException ae = new ApplicationException(s);
            throw ae;
         }
         if ((numItems == 0) || (groupSize == 0))
         {
            s = "BinCoeffL:GetNumCombos: numItems or groupSize equals zero. Neither is allowed.";
            ApplicationException ae = new ApplicationException(s);
            throw ae;
         }
         if (groupSize == numItems)
            return 1;
         // if Pascal's Triangle has not been created, then use an alternate method to obtain the number of combos.
         if (PasTri == null)
         {
            numCombos = GetCombosCount(numItems, groupSize);
            if (numCombos > uint.MaxValue)
               return 0;
            return (uint)numCombos;
         }
         uint n = (uint)numItems - 1;
         int startIndex = GroupSize - groupSize;
         uint[] indexArray = PasTri[startIndex];
         int endIndex = indexArray.Length - 1;
         if (groupSize == 2)
         {
            if (numItems != NumItems)
               endIndex = endIndex - (NumItems - numItems);
            if ((indexArray.Length > n) && (uint.MaxValue - indexArray[endIndex] > numItems - 1))
               numCombos = indexArray[endIndex] + (uint)numItems - 1;
         }
         else
         {
            if (numItems == NumItems)
            {
               uint[] indexArrayPrev = PasTri[startIndex + 1];
               int endIndexPrev = indexArrayPrev.Length - 1;
               if ((indexArray.Length > n) && (indexArrayPrev.Length > n - 1) && (uint.MaxValue - indexArray[endIndex] > indexArrayPrev[endIndexPrev]))
                  numCombos = indexArray[endIndex] + indexArrayPrev[endIndexPrev];
            }
            else
            {
               if (numItems != NumItems)
                  endIndex = endIndex - (NumItems - numItems) + 1;
               return indexArray[endIndex];
            }
         }
         return (uint)numCombos;
      }

      public uint GetRank(bool sorted, int[] kIndexes, out bool overflow, int groupSize = -1)
      {
         // This method returns the rank of the combination in kIndexes. For example, with the 13 chooose 5 case which
         // corresponds to 5 card poker hand ranks, then AKQJT (which is the greatest hand in the table) would
         // be passed as value 12, 11, 10, 9, and 8, and the return value would be 1286, which is the highest
         // element. Note that if the Sorted flag is false, then the values in KIndexes will be put into sorted
         // order and returned that way. The sorted flag must be set to false if KIndexes is not in descending order.
         //
         // If the optional argument groupSize is specified, then it must be <= to the GroupSize used to create the instance.
         //
         string s;
         overflow = false;
         groupSize = (groupSize == -1) ? GroupSize : groupSize;
         if (groupSize == 0)
         {
            s = "BinCoeffL:GetNumCombos: groupSize equals zero. This is not allowed.";
            ApplicationException ae = new ApplicationException(s);
            throw ae;
         }
         // Handle the n choose 1 case.
         if (groupSize == 1)
            return (uint)kIndexes[0];
         // Handle the n choose n case.
         if (groupSize == NumItems)
            return 0;
         int loopIndex, n;
         // The times that Pascal's triangle may not have been legitimately created are handled above.
         // So, if it has not been created, then throw an exception.
         if (PasTri == null)
         {
            s = "BinCoeffL:GetNumCombos: Error - Pascal's Triangle has not been created. ";
            ApplicationException ae = new ApplicationException(s);
            throw ae;
         }
         uint rank = 0;
         uint[] indexArray;
         if (!sorted)
            ArraySorter<int>.SortDescending(kIndexes);
         int startIndex = GroupSize - groupSize;
         int kIndex = 0;
         // for (LoopIndex = 0; LoopIndex < GroupSize - 1; LoopIndex++)
         for (loopIndex = startIndex; loopIndex < GroupSizeM1; loopIndex++)
         {
            indexArray = PasTri[loopIndex];
            n = kIndexes[kIndex++];
            // Check for overflow first.
            if ((indexArray.Length > n) && (rank <= uint.MaxValue - indexArray[n]))
               rank += indexArray[n];
            else
            {
               overflow = true;
               return 0;
            }
         }
         if (rank <= uint.MaxValue - (ulong)kIndexes[groupSize - 1])
            rank += (uint)kIndexes[groupSize - 1];
         else
         {
            overflow = true;
            return 0;
         }
         return rank;
      }

      public void GetCombFromRank(uint rank, int[] kIndexes, int numItems = -1, int groupSize = -1)
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
         if ((groupSize == 0) || (numItems == 0))
         {
            s = "BinCoeff:GetCombFromRank: groupSize or numItems is zero. Must not be == 0.";
            ApplicationException ae = new ApplicationException(s);
            throw ae;
         }
         if (groupSize > GroupSize)
         {
            s = "BinCoeff:GetCombFromRank: groupSize must be <= GroupSize used when the instance was created.";
            ApplicationException ae = new ApplicationException(s);
            throw ae;
         }
         if (kIndexes.Length > GroupSize)
         {
            s = "BinCoeff:GetCombFromRank: Increasing the length of kIndexes from the original size is not allowed.";
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
            s = "BinCoeff:GetCombFromRank: Pascal's Triangle has not been created. This could occur if the instance is created with 5 choose 5 and then 5 choose 3 is tried.";
            ApplicationException ae = new ApplicationException(s);
            throw ae;
         }
         int index = 0, loop, startIndex = GroupSize - groupSize;
         uint remValue = rank;
         uint[] indexArray;
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
         int r = (int)Convert.ChangeType(rank, typeof(int)); // If this does not work, then try dynamic.
         r += offset;
         GetCombFromRank((uint)r, kIndexes);
      }

      public void InitializeTable()
      {
         // This function creates an array of the type specified by the user.
         //
         TableData = new List<T>((int)TotalCombos);
      }

      public List<T> GetTable()
      {
         // This access function is provided so that the user can work on it when needed.
         return TableData;
      }

      public void AddItem(T obj)
      {
         // Adds the specified object to the end of the list.
         // Assumes that this list is in sorted order by rank.
         // The List container limits the number of elements to 2^31, not 2^32.
         //
         TableData.Add(obj);
      }

      public void SetItem(int index, T obj)
      {
         // Sets the specified object to the table at the specified Index.
         //
         if (index <= TableData.Count)
            TableData[index] = obj;
      }

      public void SetItem(bool sorted, int[] kIndexes, T obj)
      {
         // Adds the specified object to the table based upon the K indexes.
         //
         int rank = (int)GetRank(sorted, kIndexes, out bool overflow);
         if (overflow)
         {
            string s = "BinCoeff:SetItem - GetRank overflowed.";
            ApplicationException ae = new ApplicationException(s);
            throw ae;
         }
         SetItem(rank, obj);
      }

      public T GetItem(int index)
      {
         // Gets the specified object stored in TableData.
         //
         return TableData[index];
      }

      public T GetItem(bool sorted, int[] kIndexes)
      {
         // Gets the specified object in TableData based upon the K indexes.
         // Note that only 2^31 number of items in the list is supported here.
         //
         int rank = (int)GetRank(sorted, kIndexes, out bool overflow);
         if (overflow)
         {
            string s = "BinCoeff:GetItem - GetRank overflowed.";
            ApplicationException ae = new ApplicationException(s);
            throw ae;
         }
         return TableData[rank];
      }

      public List<uint[]> GetPascalsTriangle()
      {
         // This access function is provided for testing purposes.
         //
         return PasTri;
      }
   }
}
