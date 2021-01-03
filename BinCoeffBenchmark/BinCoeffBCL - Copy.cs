using BinomialCoefficient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinCoeffBenchmark
{
   public class BinCoeffBCL
   {
      // This file implements the benchmark tests for BinCoeffL, which is the ulong version.
      //
      long TicksNumCombosSum = 0;
      long TicksNumCombosSumAlt = 0;
      long TicksRankSum = 0;
      long TicksRankSumAlt = 0;
      long TicksComboSum = 0;
      long TicksComboSumAlt = 0;

      public void Benchmark()
      {
         // This method benchmarks the 3 methods - GetNumCombos, GetRank, and GetCombFromRank for all 3 implementations.
         // It also benchmarks the alternate math versions for each of these methods.
         //
         // Define the test n choose k test cases.
         int[] numElemArray = new int[] { 100, 200, 52, 71, 60, 75, 56, };
         int[] numGroupArray = new int[] { 12,  10,  7, 21, 40, 20, 28, };
         //
         BenchmarkBinCoeffL(numElemArray, numGroupArray);
         BenchmarkBinCoeffMultiple(numElemArray, numGroupArray);
      }

      private void BenchmarkBinCoeffL(int[] numElemArray, int[] numGroupArray)
      {
         // This method benchmarks GetNumCombos, GetRank, and GetCombFromRank for the BinCoeff (32-bit unsigned int) class.
         // Each of these methods is tested solo.
         //
         Console.WriteLine("Benchmark Results For 64-bit ulong Pascal's Triangle Library Methods .VS. Math Methods:");
         Console.WriteLine();
         int loop, n, k;
         long ticks, ticksAlt;
         ulong numCombos, numCombosAlt, rank, rank2;
         int[] kIndexes = new int[5], kIndexesAlt = new int[5];
         // Warmup the benchmark code, but throw away the results.
         BenchmarkConst(13, 5, out ticks);
         numCombos = BenchmarkBCNumCombos(13, 5, out ticks);
         numCombosAlt = BenchmarkBCNumCombosAlt(13, 5, out ticksAlt);
         if (numCombos != numCombosAlt)
            Console.WriteLine($"Error - numCombos = {numCombos}, numCombosAlt = {numCombosAlt}");
         numCombos = BenchmarkBCNumCombos2(13, 5, out ticks);
         if (numCombos != numCombosAlt)
            Console.WriteLine($"Error - numCombos (2) = {numCombos}, numCombosAlt = {numCombosAlt}");
         rank = BenchmarkBCRank(13, 5, out ticks);
         rank2 = BenchmarkBCRankAlt(13, 5, out ticks);
         if (rank != rank2)
            Console.WriteLine($"Error - ranks are different.");
         kIndexes = BenchmarkBCGetComboFromRank(13, 5, out ticks);
         kIndexesAlt = BenchmarkBCGetComboFromRankAlt(13, 5, out ticksAlt);
         if (!kIndexes.SequenceEqual(kIndexesAlt))
         {
            string s = $"TestRankCombo: 13 choose 5: GetCombFromRankAlt kIndexes does not match.";
            Console.WriteLine(s);
         }
         for (loop = 0; loop < numElemArray.Length; loop++)
         {
            n = numElemArray[loop];
            k = numGroupArray[loop];
            BenchmarkBCCombos(n, k);
            BenchmarkBCRankCombos(n, k);
         }
         double d = (double)TicksNumCombosSumAlt / (double)TicksNumCombosSum;
         d = Math.Round(d, 2);
         Console.WriteLine($"Overall Pascal's Triangle # of combos performance ratio = {d} to 1.");
         d = (double)TicksRankSumAlt / (double)TicksRankSum;
         d = Math.Round(d, 2);
         Console.WriteLine($"Overall Pascal's Triangle GetRank performance ratio = {d} to 1.");
         d = (double)TicksComboSumAlt / (double)TicksComboSum;
         d = Math.Round(d, 2);
         Console.WriteLine($"Overall Pascal's Triangle GetCombo performance ratio = {d} to 1.");
         Console.WriteLine();
      }

      private void BenchmarkBCCombos(int n, int k)
      {
         // This method benchmarks the BinCoeff constructor and GetNumCombos.
         //
         int loop1, loop1Count = 500;
         long ticks, ticksAlt, ticksSum = 0, ticksSum2 = 0, ticksAltSum = 0, ticksConstCount = 0;
         ulong combosCount = 0, combosCountAlt = 0;
         ulong numCombos;
         string s;
         for (loop1 = 0; loop1 < loop1Count; loop1++)
         {
            BenchmarkConst(n, k, out ticks);
            ticksConstCount += ticks;
            numCombos = BenchmarkBCNumCombos(n, k, out ticks);
            combosCount += numCombos;
            ticksSum += ticks;
            numCombos = BenchmarkBCNumCombos2(n, k, out ticks);
            combosCount += numCombos;
            ticksSum2 += ticks;
            numCombos = BenchmarkBCNumCombosAlt(n, k, out ticksAlt);
            combosCountAlt += numCombos;
            ticksAltSum += ticksAlt;
         }
         // Look at result so that the compiler won't optimize away the benchmark code.
         if (combosCount + combosCountAlt == 0)
            Console.WriteLine("combos = 0?");
         TicksNumCombosSum += ticksSum;
         TicksNumCombosSumAlt += ticksAltSum;
         ticksSum = (ticksSum + loop1Count - 1) / loop1Count;
         ticksSum2 = (ticksSum2 + loop1Count - 1) / loop1Count;
         ticksAltSum = (ticksAltSum + loop1Count - 1) / loop1Count;
         ticksConstCount = (ticksConstCount + loop1Count - 1) / loop1Count;
         s = $"n = {n}, k = {k}, avg const ticks = {ticksConstCount}, avg combo ticks = {ticksSum}, avg combo2 ticks = {ticksSum2}, avg alt combo ticks = {ticksAltSum}";
         Console.WriteLine(s);
      }

      private ulong BenchmarkBCNumCombos(int n, int k, out long ticks)
      {
         // This method benchmarks getting the number of combos for this n choose k case.
         //
         ulong numCombos;
         GC.Collect();
         var bc = new BinCoeffL(n, k);
         Stopwatch sw = Stopwatch.StartNew();
         numCombos = bc.GetNumCombos();
         sw.Stop();
         ticks = sw.ElapsedTicks;
         return numCombos;
      }

      private ulong BenchmarkBCNumCombos2(int n, int k, out long ticks)
      {
         // This method benchmarks getting the number of combos for this n choose k case, including the constructor.
         //
         ulong numCombos;
         GC.Collect();
         Stopwatch sw = Stopwatch.StartNew();
         var bc = new BinCoeffL(n, k);
         numCombos = bc.GetNumCombos();
         sw.Stop();
         ticks = sw.ElapsedTicks;
         return numCombos;
      }

      private void BenchmarkConst(int n, int k, out long ticks)
      {
         // This method benchmarks the constructor for this n choose k case.
         //
         GC.Collect();
         Stopwatch sw = Stopwatch.StartNew();
         var bc = new BinCoeffL(n, k);
         sw.Stop();
         ticks = sw.ElapsedTicks;
      }

      private ulong BenchmarkBCNumCombosAlt(int n, int k, out long ticks)
      {
         // This method benchmarks the alternate math version of obtaining the number of combos for this n choose k case.
         //
         ulong numCombos;
         GC.Collect();
         Stopwatch sw = Stopwatch.StartNew();
         numCombos = Combination.GetNumCombos(n, k);
         // Check for overflow.
         if (numCombos == 0)
            numCombos = (ulong)Combination.GetNumCombosBigInt(n, k);
         sw.Stop();
         ticks = sw.ElapsedTicks;
         return numCombos;
      }

      private void BenchmarkBCRankCombos(int n, int k)
      {
         // This method benchmarks getting the number of combos for this n choose k case.
         //
         int loop1, loop1Count = 500;
         long ticks, ticksAlt, ticksRankSum = 0, ticksRankSumAlt = 0, ticksComboSum = 0, ticksComboSumAlt = 0;
         ulong rankCount = 0, rankCountAlt = 0;
         ulong rank;
         string s;
         int[] kIndexes = new int[k];
         for (loop1 = 0; loop1 < loop1Count; loop1++)
         {
            rank = BenchmarkBCRank(n, k, out ticks);
            rankCount += rank;
            ticksRankSum += ticks;
            rank = BenchmarkBCRankAlt(n, k, out ticks);
            rankCountAlt += rank;
            ticksRankSumAlt += ticks;
            kIndexes = BenchmarkBCGetComboFromRank(n, k, out ticks);
            rankCount += (uint)kIndexes[0];
            ticksComboSum += ticks;
            kIndexes = BenchmarkBCGetComboFromRankAlt(n, k, out ticksAlt);
            rankCountAlt += (uint)kIndexes[0];
            ticksComboSumAlt += ticksAlt;
         }
         TicksRankSum += ticksRankSum;
         TicksRankSumAlt += ticksRankSumAlt;
         TicksComboSum += ticksComboSum;
         TicksComboSumAlt += ticksComboSumAlt;
         ticksRankSum = (ticksRankSum + loop1Count - 1) / loop1Count;
         ticksRankSumAlt = (ticksRankSumAlt + loop1Count - 1) / loop1Count;
         ticksComboSum = (ticksComboSum + loop1Count - 1) / loop1Count;
         ticksComboSumAlt = (ticksComboSumAlt + loop1Count - 1) / loop1Count;
         s = $"n = {n}, k = {k}, avg rank ticks = {ticksRankSum}, avg alt rank ticks = {ticksRankSumAlt}, avg combo ticks = {ticksComboSum}, " +
            $"avg alt combo ticks = {ticksComboSumAlt}";
         Console.WriteLine(s);
         Console.WriteLine();
      }

      private ulong BenchmarkBCRank(int n, int k, out long ticks)
      {
         // This method benchmarks obtaining the rank for this n choose k case.
         //
         ulong rank;
         GC.Collect();
         var bc = new BinCoeffL(n, k);
         int[] kIndexes = new int[k];
         bc.GetCombFromRank(bc.TotalCombos / 2, kIndexes);
         Stopwatch sw = Stopwatch.StartNew();
         rank = bc.GetRank(true, kIndexes, out bool overflow);
         sw.Stop();
         ticks = sw.ElapsedTicks;
         return rank;
      }
      private ulong BenchmarkBCRankAlt(int n, int k, out long ticks)
      {
         // This method benchmarks obtaining the rank for this n choose k case
         // using the Combination.GetRank method.
         //
         ulong rank;
         GC.Collect();
         // Get the combination for a mid in kIndexes.
         var bc = new BinCoeffL(n, k);
         int[] kIndexes = new int[k];
         bc.GetCombFromRank(bc.TotalCombos / 2, kIndexes);
         Stopwatch sw = Stopwatch.StartNew();
         rank = Combination.GetRank(kIndexes, out bool overflow, n, k);
         // Some cases overflow. So, use big integer.
         if (rank == 0)
            rank = (ulong)Combination.GetRankBigInt(kIndexes, n, k);
         sw.Stop();
         ticks = sw.ElapsedTicks;
         return rank;
      }

      private int[] BenchmarkBCGetComboFromRank(int n, int k, out long ticks)
      {
         // This method benchmarks obtaining the rank for this n choose k case.
         //
         GC.Collect();
         var bc = new BinCoeffL(n, k);
         int[] kIndexes = new int[k];
         ulong rank = bc.TotalCombos / 2;
         Stopwatch sw = Stopwatch.StartNew();
         bc.GetCombFromRank(rank, kIndexes);
         sw.Stop();
         ticks = sw.ElapsedTicks;
         return kIndexes;
      }

      private int[] BenchmarkBCGetComboFromRankAlt(int n, int k, out long ticks)
      {
         // This method benchmarks obtaining the combination from the rank using Combination.GetComboFromRank.
         //
         GC.Collect();
         int[] kIndexes = new int[k];
         var bc = new BinCoeffL(n, k);
         ulong rank = bc.TotalCombos / 2;
         Stopwatch sw = Stopwatch.StartNew();
         Combination.GetCombFromRankLong(rank, kIndexes, n, k);
         sw.Stop();
         ticks = sw.ElapsedTicks;
         return kIndexes;
      }

      private void BenchmarkBinCoeffMultiple(int[] numElemArray, int[] numGroupArray)
      {
         // This method benchmarks the n choose k tests for many subcases & ranks, and compares the performance against the Combination class methods.
         // The first benchmark compares the performance of single methods. This one attempts a more "real-life" analysis and calls GetRank and
         // GetComboFromRank 1,000 times for all the subcases for each test case.
         //
         int numItems, groupSize, caseLoop;
         int[] kIndexes = new int[40];
         ulong rankSum = 0;
         int kLoop;
         bool overflow;
         BinCoeffL bcl;
         long ticks, ticksAlt, ticksSum = 0, ticksSumAlt = 0;
         ulong numCombos, rank, startRank, endRank, rankCount = 1000, rankLoop;
         double d;
         Stopwatch sw;
         // Benchmark all the subcases.
         for (caseLoop = 0; caseLoop < numElemArray.Length; caseLoop++)
         {
            GC.Collect();
            sw = Stopwatch.StartNew();
            numItems = numElemArray[caseLoop];
            groupSize = numGroupArray[caseLoop];
            bcl = new BinCoeffL(numItems, groupSize);
            for (kLoop = groupSize; kLoop > 0; kLoop--)
            {
               numCombos = bcl.GetNumCombos(numItems, kLoop);
               rankSum += numCombos;
               startRank = (numCombos / 2) - 500;
               endRank = startRank + rankCount;
               for (rankLoop = startRank; rankLoop < endRank; rankLoop++)
               {
                  bcl.GetCombFromRank(rankLoop, kIndexes, numItems, kLoop);
                  rank = bcl.GetRank(true, kIndexes, out overflow, kLoop);
                  rankSum += rank;
               }
            }
            sw.Stop();
            // Looking at the sum of the return values makes sure that the compiler does not optimize the code away.
            if (rankSum == 0)
               Console.WriteLine("Error - rankSum = 0?");
            ticks = sw.ElapsedTicks;
            //
            // Benchmark the uint BinCoeff class subcases for 30 choose 15.
            rankSum = 0;
            sw = Stopwatch.StartNew();
            for (kLoop = groupSize; kLoop > 4; kLoop--)
            {
               numCombos = Combination.GetNumCombos(numItems, kLoop);
               rankSum += numCombos;
               startRank = (numCombos / 2) - 500;
               endRank = startRank + rankCount;
               for (rankLoop = startRank; rankLoop < endRank; rankLoop++)
               {
                  Combination.GetCombFromRankLong(rankLoop, kIndexes, numItems, kLoop);
                  rank = Combination.GetRank(kIndexes, out overflow, numItems, kLoop);
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
