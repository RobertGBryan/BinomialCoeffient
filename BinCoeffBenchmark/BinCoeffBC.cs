using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinomialCoefficient;

namespace BinCoeffBenchmark
{
   public class BinCoeffBC
   {
      // This class benchmarks the BinCoeff, BinCoeffL, and BinCoeffBigInt classes.
      // It also compares the performace against the alternative math versions.
      // The following methods are analyzed - GetNumCombos, GetRank, and GetCombFromRank.
      //
      private long Frequency; // Number of ticks per second.
      //
      public void Benchmark()
      {
         // This method benchmarks the 3 methods - GetNumCombos, GetRank, and GetCombFromRank for all 3 implementations.
         // It also benchmarks the alternate math versions for each of these methods.
         //
         // Define the test n choose k test cases.
         int[] numElemArray = new int[] { 40, 100, 25, 52, 30, 34, 20, };
         int[] numGroupArray = new int[] {10,   6, 12,  7, 26, 17, 10, };
         //
         Frequency = Stopwatch.Frequency; // Frequency is # of ticks per second on your system.
         Console.WriteLine("Benchmark Results For 32-bit uint Pascal's Triangle Library Methods .VS. Math Methods:");
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
         int[] kIndexes = new int[26];
         ulong rankSum = 1;
         int kLoop;
         bool overflow;
         BinCoeff<int> bc;
         long ticks, ticksAlt, ticksSum = 0, ticksSumAlt = 0;
         uint numCombos, rank, startRank, endRank, rankCount = 1000, rankLoop;
         double d;
         Stopwatch sw;
         // Benchmark all the specified n choose k test cases.
         for (caseLoop = 0; caseLoop < numElemArray.Length; caseLoop++)
         {
            GC.Collect();
            sw = Stopwatch.StartNew();
            numItems = numElemArray[caseLoop];
            groupSize = numGroupArray[caseLoop];
            bc = new BinCoeff<int>(numItems, groupSize);
            // Benchmark the subcases > 1,000 combinations.
            for (kLoop = groupSize; kLoop > 3; kLoop--)
            {
               numCombos = bc.GetNumCombos(numItems, kLoop);
               rankSum += numCombos;
               startRank = (numCombos / 2) - (rankCount / 2);
               endRank = startRank + rankCount;
               for (rankLoop = startRank; rankLoop < endRank; rankLoop++)
               {
                  bc.GetCombFromRank(rankLoop, kIndexes, numItems, kLoop);
                  rank = bc.GetRank(true, kIndexes, out overflow, kLoop);
                  rankSum += rank;
               }
            }
            sw.Stop();
            ticks = sw.ElapsedTicks;
            sw = Stopwatch.StartNew();
            // Benchmark the combination class using the same code as above.
            for (kLoop = groupSize; kLoop > 3; kLoop--)
            {
               numCombos = (uint)Combination.GetNumCombos(numItems, kLoop);
               rankSum += numCombos;
               startRank = (numCombos / 2) - (rankCount / 2);
               endRank = startRank + rankCount;
               for (rankLoop = startRank; rankLoop < endRank; rankLoop++)
               {
                  Combination.GetCombFromRank(rankLoop, kIndexes, numItems, kLoop);
                  rank = (uint)Combination.GetRank(kIndexes, out overflow, numItems, kLoop);
                  rankSum += rank;
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
         Console.WriteLine();
         // Looking at the sum of the return values makes sure that the compiler does not optimize the code away.
         if (rankSum == 0)
            Console.WriteLine("Error - rankSum = 0?");
      }
   }
}
