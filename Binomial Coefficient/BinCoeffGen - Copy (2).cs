using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Numerics;

namespace BinomialCoefficient
{
   public class BinCoeffGen<T>
   {
      // This class offers a more flexible way of dealing with problems relating to the binomial coefficient.
      // In the previous versions of this library, if a different N choose K case needed to be worked on, then
      // a new instance of the appropriate class would be required. But, this is not necessarily required provided
      // that the new desired N and K are both less than or equal to the current N and K, respectively.
      //
      // This class also automatically chooses the appropriate binomial coefficient class (uint, ulong, or BigInteger)
      // depending on the number of combinations for the N choose K case. It chooses the smallest data type that
      // will fit the total number of combinations from the N choose K case. This occurs both on creation of the class, and
      // when using one of the general methods that take N and K. If the current data type can't handle a larger N choose K
      // case, then the next larger type is chosen. It does not do this in reverse. So, for example, if a case of
      // 10 choose 5 is first tried, which uses the uint data type, then 60 choose 30 is then tried, the class will create
      // a new BinCoeffL type to use the ulong data type to handle the larger data. If the next case is 8 choose 4, it does
      // not go back to uint, but instead continues to use the ulong type. So, some thought should be given on how to best
      // use this class, meaning that priority should be given to handling smaller N choose K cases over larger ones.
      //
      // If the need arises to handle random N choose K cases, then perhaps an instance of each of the types could be created
      // BinCoeff (uint), BinCoeffL (ulong), and BinCoeffBigInt (BigInteger) could be created - assuming memory is plentiful.
      //
      // This class was designed and written by Robert G. Bryan in November, 2020.
      //
      // This class provides the following functionality:
      // 1. Added a BigInteger implementation, which is limited only by available memory.
      // 2. This class figures out how big the data type needs to be (uint, ulong, or BigInteger), based upon the total number of unique
      // combinations obtained from the N choose K case.
      // 3. Implemented a more general way to handle multiple N choose K cases, where the user decides on the maximum N and K.
      // This means that mulitple N Choose K cases may be handled from the same Pascal's Triangle table and it only needs to be generated once.
      // 4. A new version of calculating the total number of unique combinations is provided that is much faster since
      // it uses Pascal's Triangle to look up the answer instead of calculating it, but it does have the restriction that it can't be used if
      // the table has yet to be created or either N or K is greater than that used to create the table.
      //
      // Generic type T represents the type of table data (if any) the user wants to use, but only applies to the Bincoeff class (uint data).
      //
      // This code is in the public domain.
      // Even though it has been tested, the user assumes full responsibility for any bugs or anomalies.
      //
      BinCoeffBase BCB; // Holds ref to actual binomial coeff class - Bincoeff (uint), BinCoeffL (ulong), or BinCoeffBigInt (BigInteger).
      //
      public BinCoeffGen(int numItems, int groupSize, bool initTable = false)
      {
         // This constructor builds the index tables used to retrieve the index to the binomial coefficient table.
         // n is the number of items and k is the number of items in a group, and reflects the case n choose k.
         //
         // Get the total number of unique combinations.
         BigInteger totalCombos = BinCoeffBase.GetCombosCountBigInt(numItems, groupSize);
         // If n & k result in overflow of ulong, then use BigInteger. Otherwise either use int or long for best performance.
         if (totalCombos > ulong.MaxValue)
            BCB = new BinCoeffBigInt(numItems, groupSize, totalCombos);
         else if (totalCombos <= int.MaxValue)
            BCB = new BinCoeff<T>(numItems, groupSize, (uint)totalCombos, initTable);
         else
            BCB = new BinCoeffL(numItems, groupSize, (ulong)totalCombos);
      }
   }
}
