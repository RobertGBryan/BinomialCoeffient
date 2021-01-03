using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using BinomialCoefficient;
using System.IO;
using System.Windows.Forms;

namespace TestBinCoeff
{
   // Originally written by Robert G. Bryan 5/2011.
   // Updated on 4/2015 to fix a bug with the N choose 1 cases.
   // Code is in the public domain.
   // The user assumes responsibility for any bugs or anomalies.
   //
   class TestBinCoeffMain
   {
      // Class placeholder for the entry point to the app.
      static void Main(string[] args)
      {
         TestBinCoeff TBC = new TestBinCoeff();
         TBC.RunTests();
      }
   }

   class TestBinCoeff
   {
      // This class tests out the BinCoeff class.
      // 
      private BinCoeff<uint> BC;
      private BinCoeffL BCL;
      private const int RanksInDeck = 13;
      private const int RanksInDeckM1 = 12;
      private const int RanksInDeckM2 = 11;
      private const int RanksInDeckM3 = 10;
      private int[] NoPair1 = new int[RanksInDeck];
      private int[] NoPair2 = new int[RanksInDeckM1];
      private int[] NoPair3 = new int[RanksInDeckM2];
      private int[] NoPair4 = new int[RanksInDeckM3];
      //

      public void RunTests()
      {
         // This event function gets called when the user presses the associated button.
         // This function tests out the functionality in the BinCoeff class.
         //
         TestSubCases();
         Test35Choose16();
         TestRanksAndCombos();
         TestIntSubCases();
         TestLongSubCases();
         Test13Choose5();
         Test50Choose25();
         Test67Choose33();
         Test100Choose95SubCases();
         Console.WriteLine();
         Console.WriteLine("Press any key to exit.");
         Console.ReadKey();
      }

      private bool TestRanksAndCombos()
      {
         // This method verifies that the rank and combination in kIndexes is correct for the following cases:
         // 1 choose 1.
         // 100 choose 100.
         // 3 choose 1.
         // 100 choose 1.
         // 3 choose 2.
         // 100 choose 2.
         // 10 choose 9.
         // 100 choose 99.
         // 10 choose 5.
         // 13 choose 5.
         // 100 choose 5.
         // 100 choose 95.
         // 52 choose 7.
         // 34 choose 17.
         //
         int[] numElemArray = new int[]  { 1, 100, 3, 100, 3, 100, 10, 100, 10, 13, 100, 100, 52, 34 };
         int[] numGroupArray = new int[] { 1, 100, 1,   1, 2,   2,  9,  99,  5,  5,   5,  95,  7, 17 };
         int n, k, loopTest, numTests = numElemArray.Length;
         bool testPassed = true;
         //
         // Test each of the n choose k cases defined in numElmArray and numGroupArray.
         for (loopTest = 0; loopTest < numTests; loopTest++)
         {
            n = numElemArray[loopTest];
            k = numGroupArray[loopTest];
            testPassed = TestRankCombo(n, k);
            if (!testPassed)
               return testPassed;
            testPassed = TestRankComboulong(n, k);
            if (!testPassed)
               return testPassed;
         }
         return testPassed;
      }

      private bool TestRankCombo(int numItems, int groupSize)
      {
         // This method tests getting the rank and combination for the specified 32-bit int case.
         // If the test failed, then false is returned. Otherwise true is returned.
         string s;
         int n = numItems, k = groupSize;
         bool overflow;
         // Create the bin coeff object required to get all the combos for this n choose k combination.
         BinCoeff<int> bc = new BinCoeff<int>(n, k);
         // The Kindexes array specifies the indexes for a combination.
         int[] kIndexes = new int[k];
         uint numCombos = bc.TotalCombos, rankLoop;
         bc.GetCombFromRank(numCombos - 1, kIndexes);
         uint numCombos2 = (uint)BinCoeffBase.GetRankAlt(kIndexes, n, k, out overflow) + 1;
         if (overflow)
         {
            s = $"TestRankCombo: {n} choose {k}: # of combos overflowed with alt method.";
            Console.WriteLine(s);
            return false;
         }
         if (numCombos != numCombos2)
         {
            s = $"TestRankCombo: {n} choose {k}: # of combos not the same with alternate method - {numCombos} .vs. {numCombos2}";
            Console.WriteLine(s);
            return false;
         }
         uint rank2, rankHigh;
         uint comboCount = numCombos;
         // Loop thru all the combinations for this n choose k case if # of combos <= 1000.
         // Otherwise, just do the lowest ranked 500 and the highest ranked 500.
         if (numCombos > 1000)
            comboCount = 500;
         // Loop thru all the combinations for this n choose k case.
         for (rankLoop = 1; rankLoop < comboCount; rankLoop++)
         {
            // Get the k-indexes for this combination.  
            bc.GetCombFromRank(rankLoop, kIndexes);
            // Verify that the Kindexes returned can be used to retrieve the rank of the combination.
            rank2 = bc.GetRank(true, kIndexes, out overflow);
            if (rank2 != rankLoop)
            {
               s = $"TestRankCombo: {n} choose {k}: GetRank or GetCombFromRank failed - value of {rank2} != rank at {rankLoop}";
               Console.WriteLine(s);
               return false;
            }
            // If not doing all the combinations, then process the high ranks.
            if (numCombos > comboCount)
            {
               rankHigh = numCombos - rankLoop - 1;
               // Get the k-indexes for this combination.  
               bc.GetCombFromRank(rankHigh, kIndexes);
               // Verify that the Kindexes returned can be used to retrieve the rank of the combination.
               rank2 = bc.GetRank(true, kIndexes, out overflow);
               if (rank2 != rankHigh)
               {
                  s = $"TestRankCombo: {n} choose {k}: GetRank or GetCombFromRank failed - value of {rank2} != rankHigh at {rankLoop}";
                  Console.WriteLine(s);
                  return false;
               }
            }
         }
         s = $"TestRankCombo: {n} choose {k} completed successfully.";
         Console.WriteLine(s);
         return true;
      }

      private bool TestRankComboulong(int numItems, int groupSize)
      {
         // This method tests getting the rank and combination for the specified case.
         // If the test failed, then false is returned. Otherwise true is returned.
         string s;
         bool overflow;
         int n = numItems, k = groupSize;
         // Create the bin coeff object required to get all the combos for this n choose k combination.
         BinCoeffL bcl = new BinCoeffL(n, k);
         // The Kindexes array specifies the indexes for a combination.
         int[] kIndexes = new int[k];
         ulong numCombos = bcl.TotalCombos, rankLoop;
         bcl.GetCombFromRank(numCombos - 1, kIndexes);
         uint numCombos2 = (uint)BinCoeffBase.GetRankAlt(kIndexes, n, k, out overflow) + 1;
         if (numCombos != numCombos2)
         {
            s = $"TestRankComboLong: {n} choose {k}: # of combos not the same with alternate method - {numCombos} .vs. {numCombos2}";
            Console.WriteLine(s);
            return false;
         }
         ulong rank, offset = 1;
         // Loop thru all the combinations for this n choose k case if # of combos <= 1000000.
         // Otherwise, choose a random rank between 100 & 200.
         if (numCombos > 1000000)
         {
            Random rand = new Random(-12345);
            offset = (ulong)rand.Next(100, 200);
         }
         for (rankLoop = 0; rankLoop < numCombos; rankLoop += offset)
         {
            // Get the k-indexes for this combination.  
            bcl.GetCombFromRank(rankLoop, kIndexes);
            // Verify that the Kindexes returned can be used to retrieve the rank of the combination.
            rank = bcl.GetRank(true, kIndexes, out overflow, groupSize);
            if (rank != rankLoop)
            {
               s = $"TestRankComboLong: {n} choose {k}: GetRank or GetCombFromRank failed - value of {rank} != rank at {rankLoop}";
               Console.WriteLine(s);
               return false;
            }
         }
         s = $"TestRankComboLong: {n} choose {k} completed successfully.";
         Console.WriteLine(s);
         return true;
      }

      enum TestResult { Passed = 0, IntOverflow, LongOverflow, Failed }; // Contains the possible results of running a test.

      private void TestSubCases()
      {
         // This method tests the bottom and top 10 ranks for all of the subcases for the following cases:
         // 
         int[] numElemArray = new int[]  { 4, 13, 100, 100, 52, 35, 34, 35, 50, 67,  68 };
         int[] numGroupArray = new int[] { 3,  5,   5,  95,  7, 16, 17, 17, 25, 33,  33 };
         int[] overflowVer = new int[]   { 0,  0,   0,   0,  1,  0,  0,  1,  1,  1,   2 };
         string s;
         int n, k, loopTest, numTests = numElemArray.Length;
         TestResult testPassed;
         //
         // Test each of the n choose k cases defined in numElmArray and numGroupArray.
         for (loopTest = 0; loopTest < numTests; loopTest++)
         {
            n = numElemArray[loopTest];
            k = numGroupArray[loopTest];
            testPassed = TestIntSubCases(n, k);
            if (testPassed == TestResult.Failed)
               return;
            if ((testPassed == TestResult.IntOverflow) && (overflowVer[loopTest] == 0))
            {
               s = $"TestSubCases: Error - {n} choose {k}: unexpected uint overflow.";
               Console.WriteLine(s);
               return;
            }
            testPassed = TestLongSubCase(n, k);
            if (!testPassed)
               return;
         }
         return;
      }

      private TestResult TestIntSubCases(int n, int k)
      {
         // This method tests out the new int functionality of specifying a lower n choose k case without having to
         // create a new version of Pascal's Triangle to handle it. n & k specifies the main n choose k case.
         //
         string s;
         int nLoop, kLoop;
         BinCoeff<int> bc = new BinCoeff<int>(n, k);
         uint numCombos, numCombos2, rank, rankHigh, rank2, rankLoop;
         int[] kIndexes = new int[k];
         // If the # of combos overflowed, then don't try this case with uint.
         if (bc.TotalCombos == 0)
            return TestResult.IntOverflow;
         // Try getting the rank and combination for all subcases of n choose k.
         for (kLoop = k; kLoop > 0; kLoop--)
         {
            for (nLoop = n; nLoop >= kLoop; nLoop--)
            {
               numCombos = bc.GetNumCombos(nLoop, kLoop);
               // If GetNumCombos overflowed, then return that status.
               if (numCombos == 0)
                  return TestResult.IntOverflow;
               numCombos2 = (uint)BinCoeffBase.GetCombosCount(nLoop, kLoop);
               if (numCombos != numCombos2)
               {
                  s = $"TestIntSubCases: Error - {n} choose {k}: # of combos does not match.";
                  Console.WriteLine(s);
                  return TestResult.Failed;
               }
               uint comboCount = numCombos;
               // Loop thru all the combinations for this n choose k case if # of combos <= 1000.
               // Otherwise, just do the lowest ranked 500 and the highest ranked 500.
               if (numCombos > 1000)
                  comboCount = 500;
               // Loop thru the combinations for this n choose k case.
               for (rankLoop = 0; rankLoop < comboCount; rankLoop++)
               {
                  bc.GetCombFromRank(rankLoop, kIndexes, nLoop, kLoop);
                  rank = bc.GetRank(true, kIndexes, out bool overflow, kLoop);
                  if (rank != rankLoop)
                  {
                     s = $"TestIntSubCases: Error - {n} choose {k}: rank or combo is wrong.";
                     Console.WriteLine(s);
                     return TestResult.Failed;
                  }
                  // If not doing all the combinations, then process the high ranks.
                  if (numCombos > comboCount)
                  {
                     rankHigh = numCombos - rankLoop - 1;
                     // Get the k-indexes for this combination.  
                     bc.GetCombFromRank(rankHigh, kIndexes, nLoop, kLoop);
                     // Verify that the Kindexes returned can be used to retrieve the rank of the combination.
                     rank2 = bc.GetRank(true, kIndexes, out overflow, kLoop);
                     if (rank2 != rankHigh)
                     {
                        s = $"TestIntSubCases: Error - {n} choose {k}: GetRank or GetCombFromRank failed - {rank2} != rankHigh at {rankLoop}";
                        Console.WriteLine(s);
                        return TestResult.Failed;
                     }
                  }
               }
            }
         }
         return TestResult.Passed;
      }

      private TestResult TestLongSubCases(int n, int k)
      {
         // This method tests out the new int functionality of specifying a lower n choose k case without having to
         // create a new version of Pascal's Triangle to handle it. n & k specifies the main n choose k case.
         //
         string s;
         int nLoop, kLoop;
         BinCoeffL bcl = new BinCoeffL(n, k);
         ulong numCombos, numCombos2, rank, rankHigh, rank2, rankLoop;
         int[] kIndexes = new int[k];
         // If the # of combos overflowed, then don't try this case with uint.
         if (bcl.TotalCombos == 0)
            return TestResult.IntOverflow;
         // Try getting the rank and combination for all subcases of n choose k.
         for (kLoop = k; kLoop > 0; kLoop--)
         {
            for (nLoop = n; nLoop >= kLoop; nLoop--)
            {
               numCombos = bcl.GetNumCombos(nLoop, kLoop);
               numCombos2 = (uint)BinCoeffBase.GetCombosCount(nLoop, kLoop);
               if (numCombos != numCombos2)
               {
                  s = $"TestIntSubCases: Error - {n} choose {k}: # of combos does not match.";
                  Console.WriteLine(s);
                  return TestResult.Failed;
               }
               ulong comboCount = numCombos;
               // Loop thru all the combinations for this n choose k case if # of combos <= 1000.
               // Otherwise, just do the lowest ranked 500 and the highest ranked 500.
               if (numCombos > 1000)
                  comboCount = 500;
               // Loop thru the combinations for this n choose k case.
               for (rankLoop = 0; rankLoop < comboCount; rankLoop++)
               {
                  bcl.GetCombFromRank(rankLoop, kIndexes, nLoop, kLoop);
                  rank = bcl.GetRank(true, kIndexes, out bool overflow, kLoop);
                  if (rank != rankLoop)
                  {
                     s = $"TestIntSubCases: Error - {n} choose {k}: rank or combo is wrong.";
                     Console.WriteLine(s);
                     return TestResult.Failed;
                  }
                  // If not doing all the combinations, then process the high ranks.
                  if (numCombos > comboCount)
                  {
                     rankHigh = numCombos - rankLoop - 1;
                     // Get the k-indexes for this combination.  
                     bcl.GetCombFromRank(rankHigh, kIndexes, nLoop, kLoop);
                     // Verify that the Kindexes returned can be used to retrieve the rank of the combination.
                     rank2 = bcl.GetRank(true, kIndexes, out overflow, kLoop);
                     if (rank2 != rankHigh)
                     {
                        s = $"TestIntSubCases: Error - {n} choose {k}: GetRank or GetCombFromRank failed - {rank2} != rankHigh at {rankLoop}";
                        Console.WriteLine(s);
                        return TestResult.Failed;
                     }
                  }
               }
            }
         }
         return TestResult.Passed;
      }

      private void Test100Choose95SubCases()
      {
         // This method tests out the new int functionality of specifying a lower n choose k case without having to
         // create a new version of Pascal's Triangle to handle it. This version checks the 100 choose 95 case.
         //
         string s;
         bool overflow;
         int n = 100, k = 95, kLoop;
         BinCoeffL bcl = new BinCoeffL(n, k);
         ulong numCombos, numCombos2, rank, highRank;
         uint rankLoop;
         int[] kIndexes = new int[k];

         // Test all subcases of 100 choose 95.
         for (kLoop = k; kLoop > 0; kLoop--)
         {
            numCombos = bcl.GetNumCombos(n, kLoop);
            numCombos2 = BinCoeffBase.GetCombosCount(n, kLoop);
            if (numCombos != numCombos2)
            {
               s = $"Test100Choose95SubCases: Error - {n} choose {k}: # of combos does not match.";
               Console.WriteLine(s);
               return;
            }
            // Test the lowest 10 ranks and the highest 10 ranks for each subcase test case.
            for (rankLoop = 1; rankLoop <= 10; rankLoop++)
            {
               bcl.GetCombFromRank(rankLoop, kIndexes, n, kLoop);
               rank = bcl.GetRank(true, kIndexes, out overflow, kLoop);
               if (rank != rankLoop)
               {
                  s = $"Test100Choose95SubCases: Error - {n} choose {kLoop}: rank or combo is wrong.";
                  Console.WriteLine(s);
                  return;
               }
               highRank = numCombos - rankLoop;
               bcl.GetCombFromRank(highRank, kIndexes, n, kLoop);
               rank = bcl.GetRank(true, kIndexes, out overflow, kLoop);
               if (rank != highRank)
               {
                  s = $"Test100Choose95SubCases: Error - {n} choose {kLoop}: high rank or combo is wrong.";
                  Console.WriteLine(s);
                  return;
               }
            }
         }
      }

      private void Test13Choose5()
      {
         // This function tests out the binomial coefficien class for the 13 choose 5 case.
         int n = 13;
         int k = 5;
         ulong numCombos = BinCoeffBase.GetCombosCount(n, k);
         if (numCombos != 1287)
         {
            DisplayMsg("BinCoeffBase.GetNumCombos did not calculate the correct value for 13 choose 5 case.");
            return;
         }
         // Create the BinCoeff object that will be used for all the tests for the 13 choose 5 case.
         // The table is created when the 3rd argument of the constuctor is set to true.
         BC = new BinCoeff<uint>(n, k, 0, true);
         try
         {
            TestTableData();
            VerifyPascalsTriangle();
            VerifyTranslastion();
            TestOutputIndexes();
         }
         catch (Exception e)
         {
            DisplayMsg($"The Test 13 choose 5 tests did not complete successfully - {e.Message}.");
            return;
         }
         DisplayMsg("Test13Choose5: completed successfully.");
      }

      private void TestTableData()
      {
         // This function tests out the BinCoeff table data to make sure it works properly.
         //
         uint loop, rank, rank1, val, val1;
         int[] kIndexes = new int[5];
         string s;
         // Init the table with the value of each index.
         for (loop = 0; loop < BC.TotalCombos; loop++)
         {
            BC.AddItem(loop);
         }
         // Verify that the table was initialized properly and that the correct values can be retrieved.
         for (loop = 0; loop < BC.TotalCombos; loop++)
         {
            // Verify that the table value can be retrieved with an index.
            rank = BC.GetItem((int)loop);
            BC.GetCombFromRank(rank, kIndexes);
            // Verify that the same table value can be retrieved with the K indexes.
            rank1 = BC.GetItem(false, kIndexes);
            if ((rank != loop) || (rank1 != loop))
            {
               s = "TestTableData: BC.GetItem did not return the correct value.";
               DisplayMsg(s);
               throw new ApplicationException(s);
            }
         }
         // Reset the table by adding 100 to each value. The value of each index is no longer the rank.
         for (loop = 0; loop < BC.TotalCombos; loop++)
            BC.SetItem((int)loop, loop + 100);
         // Verify that the table was initialized properly and that the correct values can be retrieved.
         for (loop = 0; loop < BC.TotalCombos; loop++)
         {
            // Verify that the table value can be retrieved with an index.
            val = BC.GetItem((int)loop);
            BC.GetCombFromRank(loop, kIndexes);
            // Verify that the same table value can be retrieved with the K indexes.
            val1 = BC.GetItem(true, kIndexes);
            if ((val != loop + 100) || (val1 != loop + 100))
            {
               s = "TestTableData: BC.GetItem (2nd test) did not return the correct value.";
               DisplayMsg(s);
               throw new ApplicationException(s);
            }
         }
         // ReInit the object and the table with the value of each index. This time items are
         // added via the K indexes.
         for (loop = 0; loop < BC.TotalCombos; loop++)
         {
            BC.GetCombFromRank(loop, kIndexes);
            BC.SetItem(true, kIndexes, loop);
         }
         // Verify that the table was initialized properly and that the correct values can be retrieved.
         for (loop = 0; loop < BC.TotalCombos; loop++)
         {
            // Verify that the table value can be retrieved with an index.
            rank = BC.GetItem((int)loop);
            BC.GetCombFromRank(rank, kIndexes);
            // Verify that the same table value can be retrieved with the K indexes.
            rank1 = BC.GetItem(true, kIndexes);
            if ((rank != loop) || (rank1 != loop))
            {
               s = "TestTableData: BC.GetItem (3rd test) did not return the correct value.";
               DisplayMsg(s);
               throw new ApplicationException(s);
            }
         }
         DisplayMsg("TestTableData: completed successfully.");
      }

      private void VerifyPascalsTriangle()
      {
         // This function verifies that the internal indexes created in the BinCoeff class which are used to
         // translate between the KIndexes and the corresponding index in the sorted binomial coeff table
         // have been created correctly.
         //
         int loop1, loop2;
         uint[] indexArray;
         List<uint[]> pasTri;
         string s;
         // First get the translation indexes via counting the position of each card.
         GetIndexesForVer();
         // Now compare the translation indexes from the BC class with the verification indexes.
         pasTri = BC.GetPascalsTriangle();
         for (loop1 = 0; loop1 < 4; loop1++)
         {
            indexArray = pasTri[loop1];
            for (loop2 = 0; loop2 < 13 - loop1; loop2++)
            {
               if ((indexArray[loop2] != NoPair1[loop2]) && (indexArray[loop2] != NoPair2[loop2]) &&
                   (indexArray[loop2] != NoPair3[loop2]) && (indexArray[loop2] != NoPair4[loop2]) &&
                   (indexArray[loop2] != loop1))
               {
                  s = "VerifyPascalsTriangle: Pasacal's Triangle is wrong.";
                  DisplayMsg(s);
                  throw new ApplicationException(s);
               }
            }
         }
         s = "VerifyPascalsTriangle: completed successfully.";
         DisplayMsg(s);
      }

      private void VerifyTranslastion()
      {
         // This test function verifies that the BinCoeff class has been created correctly and that each
         // combination translates properly to the correct rank in the Pascal's Triangle.
         // Testing for translating between the rank and the combination in KIndexes is also done here.
         //
         int[] kIndexes = new int[5];
         int[] outkIndexes = new int[5];
         int loop;
         int rank1, rank2, rank3, rank4, rank5;
         uint comboRank, value = 0;
         string s;
         bool overflow;
         // 
         for (rank1 = 4; rank1 < RanksInDeck; rank1++)
         {
            kIndexes[0] = rank1;
            for (rank2 = 3; rank2 < rank1; rank2++)
            {
               kIndexes[1] = rank2;
               for (rank3 = 2; rank3 < rank2; rank3++)
               {
                  kIndexes[2] = rank3;
                  for (rank4 = 1; rank4 < rank3; rank4++)
                  {
                     kIndexes[3] = rank4;
                     for (rank5 = 0; rank5 < rank4; rank5++)
                     {
                        kIndexes[4] = rank5;
                        comboRank = BC.GetRank(false, kIndexes, out overflow);
                        if (comboRank != value)
                        {
                           s = "VerifyTranslastion: GetRank did not return the correct index.";
                           DisplayMsg(s);
                           throw new ApplicationException(s);
                        }
                        // Verify that the KIndexes can be correctly retrieved from the index.
                        BC.GetCombFromRank(value, outkIndexes);
                        for (loop = 0; loop < 5; loop++)
                        {
                           if (kIndexes[loop] != outkIndexes[loop])
                           {
                              s = "VerifyTranslastion: GetCombFromRank did not return the correct values.";
                              DisplayMsg(s);
                              throw new ApplicationException(s);
                           }
                        }
                        value++;
                     }
                  }
               }
            }
         }
      }

      private void GetIndexesForVer()
      {
         // This function gets the indexes for the 13 choose 5 case by looping through and counting the
         // position of each card.  This is an alternative way of calculating the indexes and is used to
         // verify that the BinCoeff class does this correctly.
         //
         int rank1, rank2, rank3, rank4, rank5, value = 0;
         // First, get the indexes for the most significant card.
         for (rank1 = 4; rank1 < RanksInDeck; rank1++)
         {
            NoPair1[rank1] = value;
            for (rank2 = 3; rank2 < rank1; rank2++)
            {
               for (rank3 = 2; rank3 < rank2; rank3++)
               {
                  for (rank4 = 1; rank4 < rank3; rank4++)
                  {
                     for (rank5 = 0; rank5 < rank4; rank5++)
                        value++;
                  }
               }
            }
         }
         // Next, get the index values for the next most significant card.
         value = 0;
         for (rank2 = 3; rank2 < RanksInDeckM1; rank2++)
         {
            NoPair2[rank2] = value;
            for (rank3 = 2; rank3 < rank2; rank3++)
            {
               for (rank4 = 1; rank4 < rank3; rank4++)
               {
                  for (rank5 = 0; rank5 < rank4; rank5++)
                     value++;
               }
            }
         }
         // Next, get the relative value for the next most significant card.
         value = 0;
         for (rank3 = 2; rank3 < RanksInDeckM2; rank3++)
         {
            NoPair3[rank3] = value;
            for (rank4 = 1; rank4 < rank3; rank4++)
            {
               for (rank5 = 0; rank5 < rank4; rank5++)
                  value++;
            }
         }
         value = 0;
         // Get the relative value for the next most significant card.
         for (rank4 = 1; rank4 < RanksInDeckM3; rank4++)
         {
            NoPair4[rank4] = value;
            for (rank5 = 0; rank5 < rank4; rank5++)
               value++;
         }
      }

      private void TestOutputIndexes()
      {
         // This test function writes out each unique combination of the binomial coefficient values out to a file.
         // 2 tests are done here.  The first test outputs the underlying reprensentation of the data,
         // which in this case is simply each of the 1287 5 card no-pair poker hands, including straights.
         // The 2nd test writes out the numeric value of the K indexes for each combination.
         //
         string[] dispChars = new string[] { "2", "3", "4", "5", "6", "7", "8", "9", "T", "J", "Q", "K", "A" };
         string[] dispChars2 = null;
         string dataPath = Application.StartupPath;
         int n = dataPath.LastIndexOf("bin\\");
         if (n >= 0)
            dataPath = dataPath.Substring(0, n);
         // The first test writes out the values given in the string DispChars instead of numeric values.
         string fileName = dataPath + "TestResults13Choose5 Disp Hands.txt";
         BinCoeffBase.OutputCombos(fileName, dispChars, "", " ", 60, BC, 0, 1286);
         // The 2nd test writes out the numeric values for each combination.
         fileName = dataPath + "TestResults13Choose5 Disp Values.txt";
         BinCoeffBase.OutputCombos(fileName, dispChars2, " ", ", ", 60, BC, 1286, 0);
      }

      private void Test50Choose25()
      {
         // This function tests out the long binomial coefficien class for the case 50 choose 25.
         //
         // Test out the unsigned long version of GetBinCoeff.  The int version overflows.
         int n = 50;
         int k = 25;
         // Create the BinCoeff object that will be used for all the tests for the 50 choose 25 case.
         BCL = new BinCoeffL(n, k);
         ulong NumCombos50Choose25 = BCL.GetNumCombos(n, k);
         if (NumCombos50Choose25 != 126410606437752)
         {
            string s = "Test50Choose25: GetNumCombos did not calculate the correct value for 50 choose 25.";
            DisplayMsg(s);
            return;
         }
         try
         {
            VerifyTranslastion50Choose25();
         }
         catch
         {
            DisplayMsg("Test50Choose25: did not complete successfully.");
            return;
         }
         DisplayMsg("Test50Choose25: completed successfully.");
      }

      private void VerifyTranslastion50Choose25()
      {
         // This test function verifies that the BinCoeff class has been created correctly and that each
         // combination translates properly to the correct index in the sorted binomial coefficient table.
         // Testing for translating between the index and the KIndexes is also done here.
         //
         int k = 25;
         int[] kIndexes = new int[k];
         int[,] verKIndexes = new int[5, 4] { {24, 23, 22, 21}, {25, 23, 22, 21}, {25, 24, 22, 21},
            {25, 24, 23, 21}, {25, 24, 23, 22} };
         uint verLoop;
         ulong loop, rank, rank2;
         // There are 126,410,606,437,752 unique combinations in this test case, far too many to go thru them all.
         // So, just check out the first 5 and the last 5.
         for (loop = 0; loop < 5; loop++)
         {
            BCL.GetCombFromRank(loop, kIndexes);
            // Verify that the KIndexes can be translated back to the proper index.
            rank = BCL.GetRank(true, kIndexes, out bool overflow);
            if (rank != loop)
            {
               DisplayMsg("BinCoeffL.GetKindex did not calculate the correct index.");
               return;
            }
            // Verify that the KIndexes were returned correctly.
            for (verLoop = 0; verLoop < k; verLoop++)
            {
               if (verLoop < 4)
               {
                  if (kIndexes[verLoop] != verKIndexes[loop, verLoop])
                  {
                     DisplayMsg("BinCoeffL.GetKindexes did not calculate the correct KIndexes.");
                     return;
                  }
               }
               else
               {
                  if (kIndexes[verLoop] != k - 1 - verLoop)
                  {
                     DisplayMsg("BinCoeffL.GetKindexes did not calculate the correct last index in KIndexes.");
                     return;
                  }
               }
            }
            rank = 126410606437747 + loop;
            BCL.GetCombFromRank(rank, kIndexes);
            // Verify that the KIndexes can be translated back to the proper index.
            rank2 = BCL.GetRank(true, kIndexes, out overflow);
            if (overflow)
            {
               DisplayMsg("BinCoeffL.GetRank overflowed.");
               return;
            }
            if (rank != rank2)
            {
               DisplayMsg("BinCoeffL.GetRank did not calculate the correct index.");
               return;
            }
            for (verLoop = 0; verLoop < k; verLoop++)
            {
               if (verLoop != k - 1)
               {
                  if (kIndexes[verLoop] != 49 - verLoop)
                  {
                     DisplayMsg("BinCoeffL.GetKindexes did not calculate the correct KIndexes.");
                     return;
                  }
               }
               else
               {
                  if ((uint)kIndexes[verLoop] != 45 - verLoop + loop)
                  {
                     DisplayMsg("BinCoeffL.GetKindexes did not calculate the correct last index in KIndexes.");
                     return;
                  }
               }
            }
         }
      }

      private void Test35Choose16()
      {
         // This function tests out the long binomial coefficien class for the case 35 choose 16.
         // This produces one of the largest number of combinations that will fit within a uint:
         // 4,059,928,950
         //
         int n = 35;
         int k = 16;
         string s;
         bool overflow;
         // Create the BinCoeff object that will be used for all the tests for the 50 choose 25 case.
         var bc = new BinCoeff<uint>(n, k);
         if (bc.TotalCombos != 4059928950)
         {
            s = $"Test35Choose16: Error calculating # of combos - {BC.TotalCombos}.";
            DisplayMsg(s);
            throw new ApplicationException(s);
         }
         int kLoop;
         uint rank = 0, numCombos, numCombos2, highRank, rank2, rankLoop;
         int[] kIndexes = new int[k];
         // Try getting the first and last 5 combinations and ranks for this case.
         try
         {
            // Test all subcases of 35 choose 16.
            for (kLoop = k; kLoop > 0; kLoop--)
            {
               numCombos = bc.GetNumCombos(n, kLoop);
               numCombos2 = (uint)BinCoeffBase.GetCombosCount(n, kLoop);
               if (numCombos != numCombos2)
               {
                  s = $"Test100Choose95SubCases: Error - {n} choose {k}: # of combos does not match.";
                  Console.WriteLine(s);
                  return;
               }
               // Test the lowest 10 ranks and the highest 10 ranks for each subcase test case.
               for (rankLoop = 1; rankLoop < 10; rankLoop++)
               {
                  bc.GetCombFromRank(rankLoop, kIndexes, n, kLoop);
                  rank = bc.GetRank(true, kIndexes, out overflow, kLoop);
                  if (rank != kLoop)
                  {
                     s = $"Test67Choose33: Either rank or combos was not calculated correctly. rank = {rank}, kloop = {kloop}";
                     DisplayMsg(s);
                     throw new ApplicationException(s);
                  }
                  highRank = numCombos - rankLoop;
                  rank2 = bc.GetRank(true, kIndexes, out overflow);
                  bc.GetCombFromRank(highRank, kIndexes, n, kLoop);
                  rank2 = bc.GetRank(true, kIndexes, out overflow);
                  if (highRank != rank2)
                  {
                     s = $"Test67Choose33: Either rank or combos was not calculated correctly. highRank = {highRank}, rank2 = {rank2}";
                     DisplayMsg(s);
                     throw new ApplicationException(s);
                  }
                  highRank--;
               }
            }
         }
         catch (Exception e)
         {
            DisplayMsg($"Test67Choose33: Exception = {e.Message}.");
            return;
         }
         DisplayMsg("Test67Choose33: completed successfully.");
      }

      private void Test67Choose33()
      {
         // This function tests out the long binomial coefficien class for the case 67 choose 33.
         // This produces one of the largest number of combinations that will fit within a ulong:
         // 14,226,520,737,620,288,370
         //
         // Test out the unsigned long version of GetBinCoeff.  The int version overflows.
         int n = 67;
         int k = 33;
         string s;
         bool overflow;
         // Create the BinCoeff object that will be used for all the tests for the 50 choose 25 case.
         BCL = new BinCoeffL(n, k);
         // ulong numCombos50Choose25 = BCL.TotalCombos;
         if (BCL.TotalCombos != 14226520737620288370)
         {
            s = $"Test67Choose33: Error calculating # of combos - {BCL.TotalCombos}.";
            DisplayMsg(s);
            throw new ApplicationException(s);
         }
         uint loop;
         ulong rank = 0, highRank = BCL.TotalCombos - 1, rank2;
         int[] kIndexes = new int[k];
         // Try getting the first and last 5 combinations and ranks for this case.
         try
         {
            for (loop = 0; loop < 5; loop++)
            {
               BCL.GetCombFromRank(rank, kIndexes);
               rank2 = BCL.GetRank(true, kIndexes, out overflow);
               if (rank != rank2)
               {
                  s = $"Test67Choose33: Either rank or combos was not calculated correctly. rank = {rank}, rank2 = {rank2}";
                  DisplayMsg(s);
                  throw new ApplicationException(s);
               }
               BCL.GetCombFromRank(highRank, kIndexes);
               rank2 = BCL.GetRank(true, kIndexes, out overflow);
               if (highRank != rank2)
               {
                  s = $"Test67Choose33: Either rank or combos was not calculated correctly. highRank = {highRank}, rank2 = {rank2}";
                  DisplayMsg(s);
                  throw new ApplicationException(s);
               }
               rank++;
               highRank--;
            }
         }
         catch (Exception e)
         {
            DisplayMsg($"Test67Choose33: Exception = {e.Message}.");
            return;
         }
         DisplayMsg("Test67Choose33: completed successfully.");
      }

      public void DisplayMsg(String msg)
      {
         // This function displays a message to the status window.
         Console.WriteLine(msg);
         // rtfStatus.AppendText(Msg + "\r\n");
      }
   }
}
