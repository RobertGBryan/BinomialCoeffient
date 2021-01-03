using BinomialCoefficient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BinCoeffBenchmark
{
   class BinCoeffBCBigInt
   {
      public void Benchmark()
      {
         // This method benchmarks the 3 methods - GetNumCombos, GetRank, and GetCombFromRank for all 3 implementations.
         // It also benchmarks the alternate math versions for each of these methods.
         //
         // n choose k test cases:
         int[] numElemArray = new int[] { 100, 100, 200, 125, 150, 185, 200, };
         int[] numGroupArray = new int[] { 50,  82,  50,  67,  75,  40, 100, };
         Console.WriteLine("Benchmark Results For Big Integer Pascal's Triangle Library Methods .VS. Math Methods:");
         Console.WriteLine();
         BenchmarkBinCoeffMultiple(numElemArray, numGroupArray);
      }

      private void BenchmarkBinCoeffMultiple(int[] numElemArray, int[] numGroupArray)
      {
         // This method benchmarks the n choose k tests for many subcases & ranks, and compares the performance against the Combination class methods.
         // The first benchmark compares the performance of single methods. This one attempts a more "real-life" analysis and calls GetRank and
         // GetComboFromRank 1,000 times for all the subcases for each test case.
         //
         int numItems, groupSize, caseLoop;
         int[] kIndexes = new int[100];
         BigInteger rankSum = 1;
         int kLoop;
         BinCoeffBigInt bcbi;
         long ticks, ticksAlt, ticksSum = 0, ticksSumAlt = 0;
         BigInteger numCombos, rank, rank2;
         int rankCount = 1000, rankLoop;
         double d;
         Stopwatch sw;
         // Benchmark all the specified n choose k test cases.
         for (caseLoop = 0; caseLoop < numElemArray.Length; caseLoop++)
         {
            numItems = numElemArray[caseLoop];
            groupSize = numGroupArray[caseLoop];
            GC.Collect();
            sw = Stopwatch.StartNew();
            bcbi = new BinCoeffBigInt(numItems, groupSize);
            // Benchmark the subcases > 1,000 combinations.
            for (kLoop = groupSize; kLoop > 3; kLoop--)
            {
               numCombos = bcbi.GetNumCombos(numItems, kLoop);
               rank = (numCombos / 2) - (rankCount / 2);
               for (rankLoop = 0; rankLoop < rankCount; rankLoop++)
               {
                  bcbi.GetCombFromRank(rank, kIndexes, numItems, kLoop);
                  rank2 = bcbi.GetRank(true, kIndexes, kLoop);
                  if (rankLoop > rankCount)
                     rankSum += rank2;
                  rank += 1;
               }
            }
            sw.Stop();
            ticks = sw.ElapsedTicks;
            // Benchmark the combination class using the same code as above.
            sw = Stopwatch.StartNew();
            for (kLoop = groupSize; kLoop > 3; kLoop--)
            {
               numCombos = Combination.GetNumCombos(numItems, kLoop);
               rank = (numCombos / 2) - (rankCount / 2);
               for (rankLoop = 0; rankLoop < rankCount; rankLoop++)
               {
                  Combination.GetCombFromRankBigInt(rank, kIndexes, numItems, kLoop);
                  rank2 = Combination.GetRankBigInt(kIndexes, numItems, kLoop);
                  // rankLoop will never be > rankCount, but the compiler does not know this -> trying to avoid unnecessary bigint ops,
                  // but making sure that the compiler does not optimize away code that it thinks is not being used.
                  if (rankLoop > rankCount)
                     rankSum += rank2;
                  rank += 1;
               }
            }
            sw.Stop();
            ticksAlt = sw.ElapsedTicks;
            // Display the results.
            d = (double)ticksAlt / (double)ticks;
            d = Math.Round(d, 2);
            Console.WriteLine($"Multiple methods benchmark for {numItems} choose {groupSize} & subcases .vs. Combination class ratio = {d} to 1.");
            ticksSum += ticks;
            ticksSumAlt += ticksAlt;
         }
         d = (double)ticksSumAlt / (double)ticksSum;
         d = Math.Round(d, 2);
         Console.WriteLine($"Average for all n choose k cases & subcases .vs. Combination class ratio = {d} to 1.");
         // Looking at the sum of the return values makes sure that the compiler does not optimize the code away.
         if (rankSum == 0)
            Console.WriteLine("Error - rankSum = 0?");
      }
   }
}
