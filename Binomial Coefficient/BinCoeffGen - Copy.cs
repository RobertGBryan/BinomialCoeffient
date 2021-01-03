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
   class BinCoeffGen<T, U>
   {
      // This class provides a way to quickly and efficiently work with problems dealing with the binomial coefficient.
      // A problem constrained by the binomial coefficient works with 2 or more items grouped together. This is
      // often referred to as N choose K, where N is the total number of items in a set and K is the group size.
      // For example, 5 choose 3 means a total of 5 items are in the set, and they will be taken 3 at a time.
      // The total number of unique combinations with 5 choose 3 may be calculated by using the binomial coefficient formula:
      // Total number of combinations = N! / ( K! (N - K)! ).
      // The ! symbol is called a factorial and means: N * (N - 1) * (N - 2) ... 1.
      // In the case of 5 choose 3, this yields 10.
      //
      // This class provides the fastest and most efficient processing for the following functionality:
      // 1. Return the rank (or lexical order) of a combination.
      // 2. Return a combination from the rank.
      // 3. Calculate the total number of unique combinations in a given N Choose K case.
      // 4. Output all the combinations from a given N Choose K case.
      //
      // The reason why this class is much more efficient than other implementations is because it uses Pascal's Triangle.
      // Pascal's Triangle contains all of the binomial coefficients (for any N choose K case) and can be built very quickly
      // since it is based upon simple addition. Thus, no multiplying or dividing is required, just addition or subtraction
      // when calculating the rank of a combination or returning the combination for a given rank.
      //
      // This class was designed and written by Robert G. Bryan in November, 2020. The original version was written in April, 2011.
      // This class provides the following:
      // 1. Added a BigInteger implementation.
      // 2. Redesigned to use generic common code, regardless if using int32, int64, or BigInteger. Type is figured out based upon
      // the number of combos generated for the N choose K case.
      // 3. Implemented a more general way to handle multiple N choose K cases, where the user decides on the maximum N and K.
      // This means that mulitple N Choose K cases may be handled from the same table and that table only needs to be generated once.
      // 4. A new version of calculating the total number of unique combinations is provided that is much faster since
      // it uses Pascal's Triangle to look up the answer instead of calculating it, but it does have some restrictions.
      // 5. Cleaned up variable names to use Microsoft recommended variable naming guidelines.
      //
      // Generic type T represents the type of table data (if any) the user wants to use.
      // Generic type U represents either int, long, or BigInteger and is determined by the create instance method.
      //
      // This code is in the public domain.
      // Even though it has been tested, the user assumes full responsibility for any bugs or anomalies.
      //
      private int NumItems;        // Total number of items. Equal to N.
      private int GroupSize;       // # of items in a group. Equal to K.
      private int IndexTabNum;     // Total number of index tables.  Equal to K - 1.
      private int IndexTabNumM1;   // Total number of index tables minus 1.  Equal to K - 2.
      private int IndexTabNumM2;   // Total number of index tables minus 2.  Equal to K - 3.
      private int TotalCombos;     // Total number of unique combinations.
      private List<U[]> Indexes;   // Holds the indexes used to access the bin coeff table. This object is a list,
      // with each element containing an array of either int, long, or BigInteger and represented by the generic type U.
      private List<T> TableData;   // Holds a list of the objects the user wants to create. This table is
      // optionally created if the user wants this class to manage the table data.
      //
      public static BinCoeffGen<T> CreateInstance(int numItems, int groupSize, bool initTable = false)
      {
         // This method calls the GetBinCoeff method to obtain the total number of combinations for this N choose K case.
         // 
         ulong n1, k1, totalCombosL;
         // Validate the inputs.
         if (groupSize < 1)
         {
            ApplicationException AE = new ApplicationException("BinCoeffGen:BinCoeffGen - input arg error - group size < 1.");
            throw AE;
         }
         if (numItems < groupSize)
         {
            ApplicationException AE = new ApplicationException("BinCoeffGen:BinCoeffGen - input arg error - number of items < group size.");
            throw AE;
         }
         // Get the total number of unique combinations.
         n1 = (ulong)numItems;
         k1 = (ulong)groupSize;
         bool overflow;
         totalCombosL = GetBinCoeff(n1, k1, out overflow);
         BinCoeffGen<T, U> result;
         // If n & k result in overflow of long, then use BigInteger.
         if (overflow)
         {
            result = new BinCoeffGen<T, BigInteger>(numItems, groupSize, initTable);
         }
         if (totalCombosL > int.MaxValue)
         {
            ApplicationException AE = new ApplicationException("BinCoeffGen:BinCoeffGen - Total # of combos > 2GB.");
            throw AE;
         }
      }

      public BinCoeffGen(int numItems, int groupSize, bool initTable = false)
      {
         // This constructor builds the index tables used to retrieve the index to the binomial coefficient table.
         // n is the number of items and k is the number of items in a group, and reflects the case n choose k.
         //
         IndexTabNum = groupSize - 1;
         IndexTabNumM1 = IndexTabNum - 1;
         IndexTabNumM2 = IndexTabNumM1 - 1;
         NumItems = numItems;
         GroupSize = groupSize;
         IndexTabNum = GroupSize - 1;
         TotalCombos = (int)TotalCombosL;
         GetIndexes();
         if (initTable)
            InitializeTable();
      }

      public static int GetBinCoeff(int n, int k)
      {
         // This function gets the total number of unique combinations based upon N and K.
         // N is the total number of items.
         // K is the size of the group.
         // Total number of unique combinations = N! / ( K! (N - K)! ).
         // For example, to get the total number of unique combinations for a 52 card deck in groups of 7,
         // it should return 133,784,560.
         //
         if (k == 1)
            return n;
         int StartNum, Loop, Divisor, TotalCombinations;
         StartNum = n - k + 1; // N! / (N-K)!
         TotalCombinations = StartNum++;
         for (Loop = StartNum; Loop <= n; Loop++)
         {
            TotalCombinations *= Loop;
         }
         Divisor = 2;
         for (Loop = 3; Loop <= k; Loop++)
         {
            Divisor *= Loop;
         }
         TotalCombinations /= Divisor;
         return TotalCombinations;
      }

      public static ulong GetBinCoeff(ulong n, ulong k)
      {
         // This function gets the total number of unique combinations based upon N and K.
         // N is the total number of items.
         // K is the size of the group.
         // Total number of unique combinations = N! / ( K! (N - K)! ).
         // This function is less efficient, but is more likely to not overflow when N and K are large.
         // Taken from:  http://blog.plover.com/math/choose.html
         //
         ulong r = 1;
         if (k > n) return 0;
         ulong loop;
         for (loop = 1; loop <= k; loop++)
         {
            r *= n--;
            r /= loop;
         }
         return r;
      }

      public static ulong GetBinCoeff(ulong n, ulong k, out bool overflow)
      {
         // This function gets the total number of unique combinations based upon N and K.
         // N is the total number of items.
         // K is the size of the group.
         // Total number of unique combinations = N! / ( K! (N - K)! ).
         // This function is less efficient, but is more likely to not overflow when N and K are large.
         // This method checks for overflow and sets the overflow flag to true when it occurs. The return value should be ignored in this case.
         // Taken from:  http://blog.plover.com/math/choose.html
         //
         overflow = false;
         ulong r = 1;
         if (k > n) return 0;
         ulong loop;
         try
         {
            for (loop = 1; loop <= k; loop++)
            {
               r *= n--;
               r /= loop;
            }
         }
         catch (System.OverflowException)
         {
            overflow = true;
         }
         return r;
      }

      public static long GetBinCoeff(long n, long k)
      {
         // This function gets the total number of unique combinations based upon N and K.
         // N is the total number of items.
         // K is the size of the group.
         // Total number of unique combinations = N! / ( K! (N - K)! ).
         // This function is less efficient, but is more likely to not overflow when N and K are large.
         // Taken from:  http://blog.plover.com/math/choose.html
         //
         long r = 1;
         long d;
         if (k > n) return 0;
         for (d = 1; d <= k; d++)
         {
            r *= n--;
            r /= d;
         }
         return r;
      }

      private void GetIndexes()
      {
         // This function creates each index that is used to obtain the index to the binomial coefficient
         // table based upon the underlying K indexes.
         //
         // If this is an n choose 1 case, then simply return since the indexes are not used for these cases.
         if (GroupSize == 1)
            return;
         int loopIndex, loop, value, incValue, startIndex, endIndex;
         int[] indexArray, indexArrayPrev, indexArrayLeast;
         //
         Indexes = new List<int[]>(IndexTabNum);
         // Create the arrays used for each index.
         for (loop = 0; loop < IndexTabNum; loop++)
         {
            indexArray = new int[NumItems - loop];
            Indexes.Add(indexArray);
         }
         // Get the indexes values for the least significant index.
         indexArrayLeast = Indexes[IndexTabNumM1];
         value = 1;
         incValue = 2;
         for (loop = 2; loop < indexArrayLeast.Length; loop++)
         {
            indexArrayLeast[loop] = value;
            value += incValue++;
         }
         // Get the index values for the remaining indexes.
         startIndex = 3;
         endIndex = NumItems - IndexTabNumM2;
         for (loopIndex = IndexTabNumM2; loopIndex >= 0; loopIndex--)
         {
            indexArrayPrev = Indexes[(loopIndex + 1)];
            indexArray = Indexes[loopIndex];
            indexArray[startIndex] = 1;
            for (loop = startIndex + 1; loop < endIndex; loop++)
            {
               indexArray[loop] = indexArray[loop - 1] + indexArrayPrev[loop - 1];
            }
            startIndex++;
            endIndex++;
         }
      }

      public int GetIndex(bool sorted, int[] kIndexes)
      {
         // This function returns the proper index to an entry in the sorted binomial coefficient table from
         // the underlying values in KIndexes. For example, for the 13 chooose 5 example which
         // corresponds to 5 card poker hand ranks, then AKQJT (which is the greatest hand in the table) would
         // be passed as value 12, 11, 10, 9, and 8, and the return value would be 1286, which is the highest
         // element.  Note that if the Sorted flag is false, then the values in KIndexes will be put into sorted
         // order and returned that way.  The sorted flag must be set to false if KIndexes is not in descending order.
         //
         // Handle the N choose 1 case.
         if (GroupSize == 1)
         {
            return kIndexes[0];
         }
         int LoopIndex, n, Index = 0;
         int[] IndexArray;
         if (!sorted)
         {
            ArraySorter<int>.SortDescending(kIndexes);
         }
         for (LoopIndex = 0; LoopIndex < GroupSize - 1; LoopIndex++)
         {
            IndexArray = Indexes[LoopIndex];
            n = kIndexes[LoopIndex];
            Index += IndexArray[n];
         }
         Index += kIndexes[GroupSize - 1];
         return Index;
      }

      public void GetKIndexes(int index, int[] kIndexes)
      {
         // This function returns the proper K indexes from an index to the sorted binomial coefficient table.
         // This is the reverse of the GetIndex function.  The correct K indexes are returned in descending order
         // in KIndexes.
         //
         // Handle the N choose 1 case.
         if (GroupSize == 1)
         {
            kIndexes[0] = index;
            return;
         }
         int LoopIndex, Loop, End, RemValue = index;
         int[] IndexArray;
         for (LoopIndex = 0; LoopIndex < GroupSize - 1; LoopIndex++)
         {
            IndexArray = Indexes[LoopIndex];
            End = IndexArray.Length - 1;
            for (Loop = End; Loop >= 0; Loop--)
            {
               if (RemValue >= IndexArray[Loop])
               {
                  kIndexes[LoopIndex] = Loop;
                  RemValue -= IndexArray[Loop];
                  break;
               }
            }
         }
         kIndexes[GroupSize - 1] = RemValue;
      }

      public void GetKIndexes(int index, List<long> kIndexes)
      {
         // This function returns the proper K indexes from an index to the sorted binomial coefficient table.
         // This is the reverse of the GetIndex function.  The correct K indexes are returned in descending order
         // in KIndexes.
         // Handle the N choose 1 case.
         if (GroupSize == 1)
         {
            kIndexes[0] = index;
            return;
         }
         int LoopIndex, Loop, End, RemValue = index;
         int[] IndexArray;
         for (LoopIndex = 0; LoopIndex < GroupSize - 1; LoopIndex++)
         {
            IndexArray = Indexes[LoopIndex];
            End = IndexArray.Length - 1;
            for (Loop = End; Loop >= 0; Loop--)
            {
               if (RemValue >= IndexArray[Loop])
               {
                  kIndexes[LoopIndex] = Loop;
                  RemValue -= IndexArray[Loop];
                  break;
               }
            }
         }
         kIndexes[GroupSize - 1] = RemValue;
      }

      public void InitializeTable()
      {
         // This function creates an array of the type specified by the user.
         TableData = new List<T>(TotalCombos);
      }

      public List<T> GetTable()
      {
         // This access function is provided so that the user can work on it when needed.
         return TableData;
      }

      public void AddItem(T obj)
      {
         // Adds the specified object to the end of the list.
         TableData.Add(obj);
      }

      public void AddItem(int index, T obj)
      {
         // Adds the specified object to the table at the specified Index.
         // This function is less efficient than the one above.  It is provided for flexibility to init
         // the table in a non-linear order.
         int Loop, n;
         if (index >= TableData.Count)
         {
            n = index - TableData.Count + 1;
            for (Loop = 0; Loop < n; Loop++)
            {
               TableData.Add(obj);
            }
         }
         else
            TableData[index] = obj;
      }

      public void AddItem(bool sorted, int[] kIndexes, T obj)
      {
         // Adds the specified object to the table based upon the K indexes.
         int Index = GetIndex(sorted, kIndexes);
         AddItem(Index, obj);
      }

      public T GetItem(int index)
      {
         // Gets the specified object stored in TableData.
         return TableData[index];
      }

      public T GetItem(bool sorted, int[] kIndexes)
      {
         // Gets the specified object in TableData based upon the K indexes.
         int Index = GetIndex(sorted, kIndexes);
         return TableData[Index];
      }

      public void OutputKIndexes(String filePath, String[] dispChars, String sep, String groupSep, int maxCharsInLine, bool sscOrder)
      {
         // This function writes out the K indexes in sorted order.
         // FilePath - path & name of file.
         // DispChars - if not null, then the string in DispChars is displayed instead of the
         // numeric value of the corresponding K index.
         // Sep - String used to separate each individual K index in a group.
         // GroupSep - String used to separate each KIndex group.
         // MaxCharsInLine - maximum number of chars in an output line.
         // AscOrder - true means the K indexes are written out in ascending order.
         const int BufferSize = 65536;
         int Loop, Loop1, n;
         int StartPos, EndPos, Inc, OutPos = 0;
         int MaxCharsInN = NumItems / 10 + 1;
         int[] KIndex = new int[GroupSize];
         byte[] Outbuf = new byte[maxCharsInLine + 2];
         String S, S1;
         StringBuilder SB = new StringBuilder();
         FileStream OutFile = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, BufferSize, FileOptions.None);
         // Set to output in ascending or descending order depending on how AscOrder is set.
         if (sscOrder)
         {
            StartPos = 0;
            EndPos = TotalCombos;
            Inc = 1;
         }
         else
         {
            StartPos = TotalCombos - 1;
            EndPos = -1;
            Inc = -1;
         }
         // Output the K Indexes
         for (Loop = StartPos; Loop != EndPos; Loop += Inc)
         {
            if (Loop == 998)
               n = 0; // debug
            GetKIndexes(Loop, KIndex);
            for (Loop1 = 0; Loop1 < GroupSize; Loop1++)
            {
               n = KIndex[Loop1];
               if (dispChars != null)
                  S = dispChars[n];
               else
               {
                  S1 = "{0, " + MaxCharsInN.ToString() + "}";
                  S = String.Format(S1, n);
               }
               // S = String.Format("{0,MaxCharsInN}", n);
               SB.Append(S);
               if (Loop1 < IndexTabNum)
                  SB.Append(sep);
            }
            if (OutPos + SB.Length >= maxCharsInLine)
            {
               Outbuf[OutPos++ - groupSep.Length] = (byte)'\r';
               Outbuf[OutPos - groupSep.Length] = (byte)'\n';
               OutFile.Write(Outbuf, 0, OutPos - groupSep.Length);
               OutPos = 0;
            }
            SB.Append(groupSep);
            // Move the string value to the output buffer.
            for (Loop1 = 0; Loop1 < SB.Length; Loop1++)
            {
               Outbuf[OutPos++] = (byte)SB[Loop1];
            }
            SB.Remove(0, SB.Length);
         }
         if (OutPos > 0)
            OutFile.Write(Outbuf, 0, OutPos - groupSep.Length);
         OutFile.Close();
         OutFile.Dispose();
      }

      public void OutputKIndexes(string dispChars, string sep, bool ascOrder, List<string> outList)
      {
         // This function writes out the K indexes in sorted order.
         // dispChars - if not null, then the string in DispChars is displayed instead of the numeric value of the corresponding K index.
         // sep - String used to separate each individual K index in a group.
         // ascOrder - true means the K indexes are written out in ascending order.
         // outList - appends the results to this list.
         int Loop, Loop1, n;
         int StartPos, EndPos, Inc;
         int MaxCharsInN = NumItems / 10 + 1;
         int[] KIndex = new int[GroupSize];
         string S, S1;
         StringBuilder SB = new StringBuilder();
         // Set to output in ascending or descending order depending on how AscOrder is set.
         if (ascOrder)
         {
            StartPos = 0;
            EndPos = TotalCombos;
            Inc = 1;
         }
         else
         {
            StartPos = TotalCombos - 1;
            EndPos = -1;
            Inc = -1;
         }
         // Output the K Indexes
         for (Loop = StartPos; Loop != EndPos; Loop += Inc)
         {
            // if (Loop == 998)
            //    n = 0; // debug
            GetKIndexes(Loop, KIndex);
            for (Loop1 = GroupSize - 1; Loop1 >= 0; Loop1--)
            {
               n = KIndex[Loop1];
               if (dispChars != null)
                  S = dispChars.Substring(n, 1);
               else
               {
                  S1 = "{0, " + MaxCharsInN.ToString() + "}";
                  S = String.Format(S1, n);
               }
               // S = String.Format("{0,MaxCharsInN}", n);
               SB.Append(S);
               if (Loop1 < IndexTabNum)
                  SB.Append(sep);
            }
            outList.Add(SB.ToString());
            SB.Remove(0, SB.Length);
         }
      }

      public void GetInternalIndexes(out List<int[]> indexes)
      {
         // This access function is provided for testing purposes.
         indexes = Indexes;
      }
   }
}
