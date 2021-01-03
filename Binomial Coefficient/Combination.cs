using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BinomialCoefficient
{
   public class Combination
   {
      // This class provides alternate methods based on combinatorial math to:
      // 1. Calculate the number of combinations for the specified n choose k case.
      // 2. Calculate the rank from a combination for the specified n choose k case.
      // 3. Calculate the combination from a rank for the specified n choose k case.
      // These methods are used to valildate the code that uses Pascal's Triangle and
      // also as a comparison for benchmarking purposes.
      //
      public static ulong GetNumCombos(int n, int k)
      {
         // This function gets the total number of unique combinations for the specified n choose k case.
         // n is the total number of items.
         // k is the size of the group.
         // Total number of unique combinations = n! / ( k! (n - k)! ).
         // This function is less efficient, but is more likely to not overflow when n and k are large.
         // This method checks for ulong overflow and returns a value of zero.
         // Zero is also returned for other error conditions like k > n, k == 0, or n == 0.
         // Taken from:  http://blog.plover.com/math/choose.html
         //
         if ((k > n) || (k == 0) || (n == 0))
            return 0;
         if ((k == 1) || (n - k == 1))
            return (ulong)n;
         ulong r = 1;
         int loop;
         // C(n, k) = C(n, n-k), so use the smaller value if k > n - k.
         if (k > n - k)
            k = n - k;
         try
         {
            checked
            {
               for (loop = 1; loop <= k; loop++)
               {
                  r *= (ulong)n--;
                  r /= (ulong)loop;
               }
            }
         }
         catch (OverflowException)
         {
            return 0;
         }
         return r;
      }

      public static BigInteger GetNumCombosBigInt(int numItems, int groupSize)
      {
         // This function gets the total number of unique combinations based upon n and k and returns a BigInteger.
         // n is the total number of items.
         // k is the size of the group.
         // Total number of unique combinations = n! / ( k! (n - k)! ).
         // Zero is returned for error conditions like k > n, k == 0, or n == 0.
         // Unlike the ulong version of this method, it never overflows. But, it could throw
         // an out of memory exception if the numbers are big enough.
         // Taken from:  http://blog.plover.com/math/choose.html
         //
         if ((groupSize > numItems) || (groupSize == 0) || (numItems == 0))
            return 0;
         if ((groupSize == 1) || (numItems - groupSize == 1))
            return (ulong)numItems;
         BigInteger r = 1;
         int loop;
         // C(n, k) = C(n, n-k), so use the smaller value if k > n - k.
         if (groupSize > numItems - groupSize)
            groupSize = numItems - groupSize;
         for (loop = 1; loop <= groupSize; loop++)
         {
            r *= (ulong)numItems--;
            r /= (ulong)loop;
         }
         return r;
      }

      public static ulong GetRank(int[] kIndexes, out bool overflow, int numItems, int groupSize)
      {
         // This method is used for testing purposes to obtain the rank for a given combination.
         // It does not use Pascal's triangle, but instead uses the math function:
         // rank = c1 choose k + c2 choose (k - 1) + ...  + cn choose 1, where c1, c2, ... ck are one of the kIndexes and k is groupSize.
         //
         overflow = false;
         if (groupSize == 1)
            return (uint)kIndexes[0];
         if (groupSize == numItems)
            return 0;
         int loopIndex, nc, gc;
         ulong rank = 0, val;
         for (loopIndex = 0; loopIndex < groupSize; loopIndex++)
         {
            nc = kIndexes[loopIndex];
            gc = groupSize - loopIndex;
            if ((nc == 0) || (gc > nc))
               return rank;
            val = GetNumCombos(nc, gc);
            // If GetNumCombos overflowed, then set overflow to true.
            if (val == 0)
            {
               overflow = true;
               return 0;
            }
            // Check for overflow. rank + val should never exceed the max value of a long.
            if (val > (ulong.MaxValue - rank))
            {
               overflow = true;
               return 0;
            }
            rank += val;
         }
         return rank;
      }

      public static BigInteger GetRankBigInt(int[] kIndexes, int numItems, int groupSize)
      {
         // This method is used for testing purposes to obtain the rank for a given combination.
         // It does not use Pascal's triangle, but instead uses the math function:
         // rank = c1 choose k + c2 choose (k - 1) + ...  + ck choose 1, where c1, c2, ck are one of the kIndexes and k is groupSize.
         //
         int loopIndex, nc, gc;
         if (groupSize == 1)
            return (uint)kIndexes[0];
         if (groupSize == numItems)
            return 0;
         BigInteger rank = 0;
         for (loopIndex = 0; loopIndex < groupSize; loopIndex++)
         {
            nc = kIndexes[loopIndex];
            gc = groupSize - loopIndex;
            if (gc > nc)
               return rank;
            rank += GetNumCombosBigInt(nc, gc);
         }
         return rank;
      }

      public static void GetCombFromRank(uint rank, int[] kIndexes, int numItems, int groupSize)
      {
         // This method calculates the combination from the rank and returns it in kIndexes.
         //
         // This method returns the combination in kIndexes from the specified rank.
         // This is the reverse of the GetRank method. The correct combination is returned in descending order
         // in kIndexes.
         // numItems  - number of items in the set, and must be specified.
         // groupSize - the number of items to group by, and must be specified.
         //
         if ((numItems == 0) || (groupSize == 0))
         {
            string s = "Combination.GetCombFromRank: numItems or groupSize is zero. Must not be 0.";
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
         int groupSizeM1 = groupSize - 1, k = groupSize, loop;
         int lowestValue = groupSizeM1, highestValue = numItems;
         ulong remValue = rank;
         // Find the combination for this rank.
         for (loopIndex = 0; loopIndex < groupSizeM1; loopIndex++)
         {
            kIndexes[loopIndex] = FindNextValue(ref remValue, numItems, k--, lowestValue--, highestValue--);
            // If remValue <= 1, then calculate the rest of the kIndexes values.
            // See https://en.wikipedia.org/wiki/Combinatorial_number_system for an example.
            if (remValue <= 1)
            {
               if (remValue == 1)
                  kIndexes[++loopIndex] = groupSize - loopIndex;
               for (loop = ++loopIndex; loop < groupSize; loop++)
                  kIndexes[loop] = groupSizeM1 - loop;
               return;
            }
         }
         kIndexes[groupSizeM1] = (int)remValue;
      }

      public static int FindNextValue(ref ulong remValue, int n, int k, int lowestValue, int highestValue)
      {
         // This method is called by GetCombFromRank to obtain the next digit in a combination.
         // For example, if this method is called with remValue = 72 (initially the rank), n = 10, and k = 5, then it would return 8 as
         // the first number of the 5-digit combination. The algorithm looks for the highest n choose 5
         // number of combinations that does not exceed 72. So, 9 choose 5 is 126, which is too high. 8 choose 5 is
         // 56. So, 8 is the first digit of the 5-digit combination. Combination numbers start from zero.
         // After this, 56 is subtracted from 72 = 16, which is passed back to the calling method for input when this method
         // is called to obtain the next digit of the combination. When called again, this time with remValue = 16, n = 10, and k = 4,
         // the algorithm again finds the greatest value that does not exceed 16. 8 choose 4 is 70, which is too high.
         // 6 choose 4 is 15, which is the highest n choose 4 value that does not exceed 16. So, remValue is returned as 16 - 15 = 1.
         // 6 is the return value. Once remValue is returned as zero or one, the calling method can easily figure out the rest of the
         // combination digits. For example, the only n choose 3 case that solves for 1 is 3 choose 3, which generates just one combination.
         // So, the calling method then knows that the smallest digits that could occupy the last two remaining slots are 1 and 0. Thus, the entire
         // combination is [8, 6, 3, 1, 0]. See https://en.wikipedia.org/wiki/Combinatorial_number_system for more info.
         //
         // Rather than use a linear search, this algorithm uses a binary search for the largest number of combinations generated by an
         // n choose k case that does not exceed remValue. The expected runtime is thus O(log(n)). The binary search algorithm used is
         // the same as used by Java. See https://stackoverflow.com/a/13306784/643828 for more info.
         //
         int mid = 0, midPrev = 0, low = lowestValue, high = highestValue;
         ulong midVal = 0, midValPrev = 0;

         while (low <= high)
         {
            if ((midVal < remValue) && (midVal > midValPrev))
            {
               midPrev = mid;
               midValPrev = midVal;
            }
            mid = low + (high - low) / 2;
            if (mid < k)
               midVal = 0;
            else
            {
               midVal = GetNumCombos(mid, k);
               // If overflowed, then try BigInteger.
               if (midVal == 0)
               {
                  BigInteger bigVal = GetNumCombosBigInt(mid, k);
                  if (bigVal > ulong.MaxValue)
                  {
                     string s = $"Combination.FindNextValue: {n} choose {k} overflow of ulong.";
                     ApplicationException ae = new ApplicationException(s);
                     throw ae;
                  }
                  midVal = (ulong)bigVal;
               }
            }
            if (midVal < remValue)
               low = mid + 1;
            else if (midVal > remValue)
               high = mid - 1;
            else
            {
               // Exact match found.
               remValue = 0;
               return mid;
            }
         }
         // Not an exact match.
         if (midVal > remValue)
         {
            midVal = midValPrev;
            mid = midPrev;
         }
         remValue -= midVal;
         return mid;
      }

      public static void GetCombFromRankLong(ulong rank, int[] kIndexes, int numItems, int groupSize)
      {
         // This method calculates the combination from the rank and returns it in kIndexes.
         //
         // This method returns the combination in kIndexes from the specified rank.
         // This is the reverse of the GetRank method. The correct combination is returned in descending order
         // in kIndexes.
         // numItems  - number of items in the set, and must be specified.
         // groupSize - the number of items to group by, and must be specified.
         //
         if ((numItems == 0) || (groupSize == 0))
         {
            string s = "Combination.GetCombFromRank: numItems or groupSize is zero. Must not be 0.";
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
         int groupSizeM1 = groupSize - 1, k = groupSize, loop;
         int lowestValue = groupSizeM1, highestValue = numItems;
         ulong remValue = rank;
         // Find the combination for this rank.
         for (loopIndex = 0; loopIndex < groupSizeM1; loopIndex++)
         {
            kIndexes[loopIndex] = FindNextValue(ref remValue, numItems, k--, lowestValue--, highestValue--);
            // If remValue <= 1, then calculate the rest of the kIndexes values.
            // See https://en.wikipedia.org/wiki/Combinatorial_number_system for an example.
            if (remValue <= 1)
            {
               if (remValue == 1)
                  kIndexes[++loopIndex] = groupSize - loopIndex;
               for (loop = ++loopIndex; loop < groupSize; loop++)
                  kIndexes[loop] = groupSizeM1 - loop;
               return;
            }
         }
         kIndexes[groupSizeM1] = (int)remValue;
      }

      public static void GetCombFromRankBigInt(BigInteger rank, int[] kIndexes, int numItems, int groupSize)
      {
         // This method calculates the combination from the rank and returns it in kIndexes.
         //
         // This method returns the combination in kIndexes from the specified rank.
         // This is the reverse of the GetRank method. The correct combination is returned in descending order
         // in kIndexes.
         // numItems  - number of items in the set, and must be specified.
         // groupSize - the number of items to group by, and must be specified.
         //
         if ((numItems == 0) || (groupSize == 0))
         {
            string s = "BinCoeff:GetCombFromRankAlt: numItems or groupSize is zero. Must not be 0.";
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
         int groupSizeM1 = groupSize - 1, k = groupSize, loop;
         int lowestValue = groupSizeM1, highestValue = numItems;
         BigInteger remValue = rank;
         // Find the combination for this rank.
         for (loopIndex = 0; loopIndex < groupSizeM1; loopIndex++)
         {
            kIndexes[loopIndex] = FindNextValueBigInt(ref remValue, numItems, k--, lowestValue--, highestValue--);
            // If remValue <= 1, then calculate the rest of the kIndexes values.
            // See https://en.wikipedia.org/wiki/Combinatorial_number_system for an example.
            if (remValue <= 1)
            {
               if (remValue == 1)
                  kIndexes[++loopIndex] = groupSize - loopIndex;
               for (loop = ++loopIndex; loop < groupSize; loop++)
                  kIndexes[loop] = groupSizeM1 - loop;
               return;
            }
         }
         kIndexes[groupSizeM1] = (int)remValue;
      }

      public static int FindNextValueBigInt(ref BigInteger remValue, int n, int k, int lowestValue, int highestValue)
      {
         // This method is called by GetCombFromRankBigIntAlt to obtain the next digit in a combination.
         // See FindNextValue method description for more info.
         //
         int mid = 0, midPrev = 0, low = lowestValue, high = highestValue;
         BigInteger midVal = 0, midValPrev = 0;

         while (low <= high)
         {
            if ((midVal < remValue) && (midVal > midValPrev))
            {
               midPrev = mid;
               midValPrev = midVal;
            }
            mid = low + (high - low) / 2;
            midVal = GetNumCombosBigInt(mid, k);
            if (midVal < remValue)
               low = mid + 1;
            else if (midVal > remValue)
               high = mid - 1;
            else
            {
               // Exact match found.
               remValue = 0;
               return mid;
            }
         }
         // Not an exact match.
         if (midVal > remValue)
         {
            midVal = midValPrev;
            mid = midPrev;
         }
         remValue -= midVal;
         return mid;
      }
   }
}
