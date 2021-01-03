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
      long TicksNumCombosSum = 0;
      long TicksNumCombosSumAlt = 0;
      long TicksRankSum = 0;
      long TicksRankSumAlt = 0;
      long TicksComboSum = 0;
      long TicksComboSumAlt = 0;
      //
      public void Benchmark()
      {
         // This method benchmarks the 3 methods - GetNumCombos, GetRank, and GetCombFromRank for all 3 implementations.
         // It also benchmarks the alternate math versions for each of these methods.
         //
         // n choose k test cases:
         int[] numElemArray = new int[] { 100, 100, 200, 125, 150, 185, 200, };
         int[] numGroupArray = new int[] { 50,  82,  50,  67,  75,  40, 100, };
         BenchmarkBinCoeff(numElemArray, numGroupArray);
         BenchmarkBinCoeffMultiple(numElemArray, numGroupArray);
      }

      private void BenchmarkBinCoeff(int[] numElemArray, int[] numGroupArray)
      {
         // This method benchmarks GetNumCombos, GetRank, and GetCombFromRank for the BinCoeff (32-bit unsigned int) class.
         //
         Console.WriteLine("Benchmark Results For Big Integer Pascal's Triangle Library Methods .VS. Math Methods:");
         Console.WriteLine();
         int loop, n, k;
         long ticks, ticksAlt;
         BigInteger numCombos, numCombosAlt, rank, rank2;
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
         Console.WriteLine($"Overall Pascal's GetRank performance ratio = {d} to 1.");
         d = (double)TicksComboSumAlt / (double)TicksComboSum;
         d = Math.Round(d, 2);
         Console.WriteLine($"Overall Pascal's GetCombo performance ratio = {d} to 1.");
         Console.WriteLine();
      }

      private void BenchmarkBCCombos(int n, int k)
      {
         // This method benchmarks the BinCoeff constructor and GetNumCombos.
         //
         int loop1, loop1Count = 25;
         long ticks, ticksAlt, ticksSum = 0, ticksSum2 = 0, ticksAltSum = 0, ticksConstCount = 0;
         BigInteger combosCount = 0, combosCountAlt = 0;
         BigInteger numCombos;
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

      private BigInteger BenchmarkBCNumCombos(int n, int k, out long ticks)
      {
         // This method benchmarks getting the number of combos for this n choose k case.
         //
         BigInteger numCombos;
         GC.Collect();
         var bc = new BinCoeffBigInt(n, k);
         Stopwatch sw = Stopwatch.StartNew();
         numCombos = bc.GetNumCombos();
         sw.Stop();
         ticks = sw.ElapsedTicks;
         return numCombos;
      }

      private BigInteger BenchmarkBCNumCombos2(int n, int k, out long ticks)
      {
         // This method benchmarks getting the number of combos for this n choose k case, including the constructor.
         //
         BigInteger numCombos;
         GC.Collect();
         Stopwatch sw = Stopwatch.StartNew();
         var bc = new BinCoeffBigInt(n, k);
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
         var bc = new BinCoeffBigInt(n, k);
         sw.Stop();
         ticks = sw.ElapsedTicks;
      }

      private BigInteger BenchmarkBCNumCombosAlt(int n, int k, out long ticks)
      {
         // This method benchmarks the alternate math version of obtaining the number of combos for this n choose k case.
         //
         BigInteger numCombos;
         GC.Collect();
         Stopwatch sw = Stopwatch.StartNew();
         numCombos = Combination.GetNumCombos(n, k);
         sw.Stop();
         ticks = sw.ElapsedTicks;
         return numCombos;
      }

      private void BenchmarkBCRankCombos(int n, int k)
      {
         // This method benchmarks getting the number of combos for this n choose k case.
         //
         int loop1, loop1Count = 25;
         long ticks, ticksAlt, ticksRankSum = 0, ticksRankSumAlt = 0, ticksComboSum = 0, ticksComboSumAlt = 0;
         BigInteger rankCount = 0, rankCountAlt = 0;
         BigInteger rank;
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

      private BigInteger BenchmarkBCRank(int n, int k, out long ticks)
      {
         // This method benchmarks obtaining the rank for this n choose k case.
         //
         BigInteger rank;
         GC.Collect();
         var bc = new BinCoeffBigInt(n, k);
         int[] kIndexes = new int[k];
         bc.GetCombFromRank(bc.TotalCombos / 2, kIndexes);
         Stopwatch sw = Stopwatch.StartNew();
         rank = bc.GetRank(true, kIndexes);
         sw.Stop();
         ticks = sw.ElapsedTicks;
         return rank;
      }
      private BigInteger BenchmarkBCRankAlt(int n, int k, out long ticks)
      {
         // This method benchmarks obtaining the rank for this n choose k case
         // using the Combination.GetRank method.
         //
         BigInteger rank;
         GC.Collect();
         // Get the combination for rank of 100 in kIndexes.
         var bc = new BinCoeffBigInt(n, k);
         int[] kIndexes = new int[k];
         bc.GetCombFromRank(bc.TotalCombos / 2, kIndexes);
         Stopwatch sw = Stopwatch.StartNew();
         rank = Combination.GetRank(kIndexes, out bool overflow, n, k);
         sw.Stop();
         ticks = sw.ElapsedTicks;
         return rank;
      }

      private int[] BenchmarkBCGetComboFromRank(int n, int k, out long ticks)
      {
         // This method benchmarks obtaining the rank for this n choose k case.
         //
         GC.Collect();
         var bc = new BinCoeffBigInt(n, k);
         int[] kIndexes = new int[k];
         BigInteger rank = bc.TotalCombos / 2;
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
         var bc = new BinCoeffBigInt(n, k);
         BigInteger rank = bc.TotalCombos / 2;
         Stopwatch sw = Stopwatch.StartNew();
         Combination.GetCombFromRankBigInt(rank, kIndexes, n, k);
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
         int[] kIndexes = new int[100];
         BigInteger rankSum = 1;
         int kLoop;
         bool overflow;
         BinCoeffBigInt bcbi;
         long ticks, ticksAlt, ticksSum = 0, ticksSumAlt = 0;
         BigInteger numCombos, rank, rank2;
         int rankCount = 1000, rankLoop;
         double d;
         Stopwatch sw;
         // Benchmark all the subcases.
         for (caseLoop = 0; caseLoop < numElemArray.Length; caseLoop++)
         {
            GC.Collect();
            sw = Stopwatch.StartNew();
            numItems = numElemArray[caseLoop];
            groupSize = numGroupArray[caseLoop];
            bcbi = new BinCoeffBigInt(numItems, groupSize);
            for (kLoop = groupSize; kLoop > 0; kLoop--)
            {
               numCombos = bcbi.GetNumCombos(numItems, kLoop);
               rank = (numCombos / 2) - 500;
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
            // Looking at the sum of the return values makes sure that the compiler does not optimize the code away.
            if (rankSum == 0)
               Console.WriteLine("Error - rankSum = 0?");
            //
            // Benchmark the uint BinCoeff class subcases for 30 choose 15.
            sw = Stopwatch.StartNew();
            for (kLoop = groupSize; kLoop > 4; kLoop--)
            {
               numCombos = Combination.GetNumCombos(numItems, kLoop);
               rank = (numCombos / 2) - 500;
               for (rankLoop = 0; rankLoop < rankCount; rankLoop++)
               {
                  Combination.GetCombFromRankBigInt(rank, kIndexes, numItems, kLoop);
                  rank2 = Combination.GetRank(kIndexes, out overflow, numItems, kLoop);
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
         Console.WriteLine();
         // Looking at the sum of the return values makes sure that the compiler does not optimize the code away.
         if (rankSum == 0)
            Console.WriteLine("Error - rankSum = 0?");
      }
   }
}
