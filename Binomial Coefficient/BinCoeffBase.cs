using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BinomialCoefficient
{
   public enum OptimalType { UnsignedInt = 0, UnsignedLong, BigInt }; // Contains the 3 types of Bincoeff that may be created - uint, ulong, & BigInteger.

   public abstract class BinCoeffBase
   {
      // This is the base class for all 3 implementations of BinCoeff (int, long, & BigInteger). It contains common functionality.
      // These 3 classes provide a way to quickly and efficiently work with problems dealing with the binomial coefficient.
      // A problem constrained by the binomial coefficient works with 2 or more items grouped together. This is
      // often referred to as n choose k, where n is the total number of items in a set and k is the group size.
      // For example, 5 choose 3 means a total of 5 items are in the set, and they will be taken 3 at a time.
      // The total number of unique combinations with 5 choose 3 may be calculated by using the binomial coefficient formula:
      // Total number of combinations = n! / ( k! (n - k)! ).
      // The ! symbol is called a factorial and means: n * (n - 1) * (n - 2) ... 1.
      // In the case of 5 choose 3, this yields 10.
      //
      // These classes provide the fastest and most efficient processing for the following functionality:
      // 1. Return the rank (or lexical order) of a combination.
      // 2. Return a combination from the rank, which is returned as an integer array of length k.
      // 3. Calculate the total number of unique combinations in a given n choose k case.
      // 4. Output all the combinations from a given n choose k case.
      //
      // The reason why these classes are much more efficient than other implementations is because they use Pascal's Triangle.
      // Pascal's Triangle contains all of the binomial coefficients (for any n choose k case) and can be built very quickly
      // since it is based upon simple addition. Thus, no multiplying or dividing is required, just addition or subtraction
      // when calculating the rank of a combination or returning the combination for a given rank.
      //
      // See the ReadMe.txt file for more detailed info.
      // This class was designed and written by Robert G. Bryan in Jan, 2021. The original version (BinCoeff - 32 bit ints) was written in April, 2011.
      // All the code is in the public domain.
      // None of this code is covered by a warranty. Use at your own risk. Test your n choose k cases carefully.
      //
      public int NumItems { get; private set; } // Total number of items. Equal to N.
      public int GroupSize { get; private set; } // # of items in a group. Equal to K.
      protected int GroupSizeM1;   // Total number of index tables.
      protected int GroupSizeM2;   // Total number of index tables minus 1.
      protected int GroupSizeM3;   // Total number of index tables minus 2.
      protected int GroupSizeM4;   // Total number of index tables minus 3.

      protected abstract void GetCombFromRank<U>(U startRank, int fromStartRank, int[] kIndexes);

      protected void Init(int numItems, int groupSize)
      {
         // This method validates the inputs and calculates the number of combinations from the n choose k case.
         // overflow is set to true if it exceeds ulong.MaxValue.
         //
         // Validate the inputs.
         //
         string s;
         if (groupSize < 1)
         {
            s = "BinCoeffBase:Init - input arg error - group size < 1.";
            ApplicationException AE = new ApplicationException(s);
            throw AE;
         }
         if (numItems < groupSize)
         {
            s = "BinCoeffBase:Init - input arg error - number of items < group size.";
            ApplicationException AE = new ApplicationException(s);
            throw AE;
         }
         GroupSizeM1 = groupSize - 1;
         GroupSizeM2 = GroupSizeM1 - 1;
         GroupSizeM3 = GroupSizeM2 - 1;
         GroupSizeM4 = GroupSizeM3 - 1;
         NumItems = numItems;
         GroupSize = groupSize;
      }

      public static void OutputCombos<U>(string filePath, string[] dispChars, string sep, string groupSep, int maxCharsInLine,
         BinCoeffBase bcb, U startRank, U endRank)
      {
         // This function writes out the K indexes in sorted order to either a file or a list. It is used by all 3 implementations.
         // U specifies uint, ulong, or BigInteger.
         // filePath - path & name of file.
         // dispChars - if not null, then the string in DispChars is displayed instead of the
         //    numeric value of the corresponding combination.
         // sep - String used to separate each individual combination in a group.
         // groupSep - String used to separate each combination group.
         // maxCharsInLine - maximum number of chars in an output line.
         // bcb - the type to be worked on - either Bincoeff (uint), BincoeffL (ulong), or BinCoeffBig (BigInteger).
         // startRank - the first combo that should be output. Range is zero to NumCombos - 1.
         // endRank - the last combo that should be output. If startRank > endRank, then output is in descending order.
         //
         // No more than 2^31 combinations are output.
         //
         const int bufferSize = 65536;
         int loop1, n, outPos = 0;
         int offset = 0, inc, outCount = 0, elemCount = 0;
         int maxCharsInN = bcb.NumItems / 10 + 1;
         var kIndexes = new int[bcb.GroupSize];
         var outbuf = new byte[maxCharsInLine + 2];
         string s, s1;
         StringBuilder sb = new StringBuilder();
         FileStream outFile = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, FileOptions.None);
         // Set to output in ascending or descending order depending if startRank < endRank.
         dynamic dynStart = startRank, dynEnd = endRank;
         inc = (dynStart > dynEnd) ? -1 : 1;
         outCount = (dynEnd - dynStart) * inc;
         // Output the K Indexes
         do
         {
            bcb.GetCombFromRank(startRank, offset, kIndexes);
            offset += inc;
            for (loop1 = 0; loop1 < bcb.GroupSize; loop1++)
            {
               n = kIndexes[loop1];
               if (dispChars != null)
                  s = dispChars[n];
               else
               {
                  s1 = "{0, " + maxCharsInN.ToString() + "}";
                  s = String.Format(s1, kIndexes[loop1]);
               }
               sb.Append(s);
               if (loop1 < bcb.GroupSizeM1)
                  sb.Append(sep);
            }
            if (outPos + sb.Length >= maxCharsInLine)
            {
               outbuf[outPos++ - groupSep.Length] = (byte)'\r';
               outbuf[outPos - groupSep.Length] = (byte)'\n';
               outFile.Write(outbuf, 0, outPos - groupSep.Length);
               outPos = 0;
            }
            sb.Append(groupSep);
            // Move the string value to the output buffer.
            for (loop1 = 0; loop1 < sb.Length; loop1++)
            {
               outbuf[outPos++] = (byte)sb[loop1];
            }
            sb.Remove(0, sb.Length);
            elemCount++;
         } while (elemCount <= outCount);
         if (outPos > 0)
            outFile.Write(outbuf, 0, outPos - groupSep.Length);
         outFile.Close();
         outFile.Dispose();
      }

      public static OptimalType GetLeastTypeForCase(int numItems, int groupSize)
      {
         // This method returns the least type that can handle all ranks for the case of numItems choose groupSize.
         //
         OptimalType returnValue;
         BigInteger numCombos  = Combination.GetNumCombosBigInt(numItems, groupSize);
         // If overflowed ulong, then only big int will work with this case.
         if (numCombos > ulong.MaxValue)
            returnValue = OptimalType.BigInt;
         else if (numCombos <= uint.MaxValue)
            returnValue = OptimalType.UnsignedInt;
         else
            returnValue = OptimalType.UnsignedLong;
         return returnValue;
      }

      public static OptimalType GetLeastTypeForAllSubcases(int numItems, int groupSize)
      {
         // This method returns the least type that will be able to handle all subcases without overflowing.
         //
         // n choose (n / 2) is the worst subcase that generates the most combinations.
         // So, if numItems is twice or more groupSize, then there is no subcase that will increase the number of combos.
         //
         BigInteger numCombos;
         OptimalType returnValue;
         if (numItems >= groupSize + groupSize)
            numCombos  = Combination.GetNumCombos(numItems, groupSize);
         else
            numCombos  = Combination.GetNumCombosBigInt(numItems, numItems / 2);
         // If overflowed ulong, then only big int will work with all subtypes.
         if (numCombos > ulong.MaxValue)
            returnValue = OptimalType.BigInt;
         else if (numCombos <= uint.MaxValue)
            returnValue = OptimalType.UnsignedInt;
         else
            returnValue = OptimalType.UnsignedLong;
         return returnValue;
      }
   }

   static public class ArraySorter<T> where T : IComparable
   {
      // This class provides the capability to sort in reverse order.  Taken from:
      // http://www.csharp411.com/sort-c-array-in-descendingreverse-order/
      static public void SortDescending(T[] array)
      {
         Array.Sort<T>(array, s_Comparer);
      }

      static private ReverseComparer s_Comparer = new ReverseComparer();

      private class ReverseComparer : IComparer<T>
      {
         public int Compare(T object1, T object2)
         {
            return -((IComparable)object1).CompareTo(object2);
         }
      }
   }
}
