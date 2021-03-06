Introduction

The code library presented here involves working with combinational problems of the nature n choose k, where n is the number of items in a set and k is the number to group them by. For example, if n is 5 and k is 3, then the total number of combinations can be calculated as (n! / (k! * (n - k)!. When k is two, the equation simplifies to n * (n - 1) / 2. The "!" symbol is called a factorial and means: n * (n - 1) * (n - 2) ... 1.

A combination is composed of unique values from the set. No duplicate numbers are allowed in a combination. In the case of 5 choose 3, the total number of combinations is 10. Each combination has a rank, or lexical order. For example:

Rank  Combination
----  -----------
   0        2 1 0
   1        3 1 0
   2        3 2 0
   3        3 2 1
   4        4 1 0
   5        4 2 0
   6        4 2 1
   7        4 3 0
   8        4 3 1
   9        4 3 2

It is useful at times to obtain the rank from the combination and vice-a-versa. In the past, one way was to iterate over each combination using multiple loops and increment the rank for each iteration, which was very inefficient. At some point, a better formula was discovered:

rank = [c1 choose k] + [c2 choose (k - 1)] + ...  + [cn choose 1], where c1, c2, ... ck is one of the combination values and k is the group size. Using the test case of 5 choose 3, if the combination [4 2 0] is selected, then the rank of that combination can be calculated as:

[4 choose 3] + [2 choose 2] + (0 choose 1) = 4 + 1 + 0 = 5. By the way, in this context [4 choose 3] means number of combinations for [4 choose 3].
See the following links for more info:
https://en.wikipedia.org/wiki/Combinatorial_number_system
https://www.developertyrone.com/blog/generating-the-mth-lexicographical-element-of-a-mathematical-combination/

So, both of these techniques work for obtaining the rank from a combination. But, there is a faster way to obtain the rank for a combination. This technique involves creating Pascal's Triangle for the n choose k case of interest. Here is an example of Pascal's Triangle:

          1
        1   1
      1   2   1
    1   3   3   1
  1   4   6   4   1
1   5   10  10  5   1

Each of the n choose k cases calculation of number of combinations is present in Pascal's Triangle. See the following links for more info:
https://math.libretexts.org/Bookshelves/Mathematical_Logic_and_Proof/Book%3A_Book_of_Proof_(Hammack)/03%3A_Counting/3.06%3A_Pascal%E2%80%99s_Triangle_and_the_Binomial_Theorem
https://www.mathsisfun.com/algebra/binomial-theorem.html#coefficients
http://ghcimdm4u.weebly.com/uploads/1/3/5/8/13589538/5.4.pdf
https://arxiv.org/pdf/1601.05794.pdf
https://math.stackexchange.com/q/453843/42760

Using the example of 5 choose 3, you can see that each of those coefficients is present in Pascal's Triangle and when added together, produce the same result as the rank calculation formula above.

This means that once Pascal's Triangle is created in memory, the runtime to calculate the rank is reduced to O(k), where the underlying operation is addition. A similar time saving may be achieved when converting a rank to a combination. Calculating the number of combinations for any n choose k case can now be performed in O(1) time.

Another advantage of using Pascal's Triangle is that it offers the ability to work with subcases of the n choose k case used to create the object. A subcase is any n' choose k' case where both n' and k' are less than or equal to the respective n choose k case used when the Triangle was first created.

For example, if 100 choose 95 is used to create an instance of the class, then 100 choose 94 may be handled with that same Pascal's Triangle as well since the data used for the subcases are calculated when the object is created. But, cases like 99 choose 96 or 101 choose 85 could not be handled without creating a new instance. So, if you knew ahead of time that both 101 choose 85 and 99 choose 96 needed to be handled, then creating an instance with 101 choose 99 (taking the max of n and max of k), would allow both of these subcases to be processed without having to create a new object.

With 100 choose 95, the subcase of 100 choose 82 would begin to fail for many of the higher ranks since the total number of combinations can't be held by a 64-bit ulong value. If some larger n choose k subcases that overflow need to be handled, then the only other alternative is to use the BigInteger implementation, which is guaranteed to never overflow. But, at some point it could throw an out of memory exception if the numbers get too big.

The test code's largest case is 200 choose 100, which generates a 59 decimal digit number as the number of combinations. This value is calculated correctly on my test machine, which has 16 GB of memory. To create Pascal's Triangle for 200 choose 100, it creates roughly 20,000 values (n * k). If each value required 25 bytes (2 ^ 196 would require roughly 25 bytes to represent that value), then the entire table would require 20,000 * 25 bytes = 500,000 bytes of memory. So, even when working with rather large numbers, the memory requirements are not too severe.

If the uint or ulong version overflows, then the code handles this by limiting the data in Pascal's Triangle to only the values that have not overflowed. Once overflow is detected, then the affected array is reduced in size to the values that have not overflowed. Checks are also provided so that an overflow condition is returned if the code should try to access array values outside the defined bounds when calculating the rank or number of combinations. Test cases are provided that prove this works correctly. When using the BinCoeff (uint) or BinCoeffL (ulong) classes, you should expect to test for overflow - if your n choose k cases have not been previously analyzed for overflow.


UNDERSTANDING HOW TO USE SUBCASES

The main performance feature of the 3 implementations is that the binomial coefficients are pre-calculated for a given n choose k case. However, if you look at one of the old Benchmark Result files included as part of the download, you will see that the time to create Pascal's Triangle sometimes costs more than calling one or more of the alternative Combination math class methods, which does not use Pascal's Triangle.

For example, the 34 choose 17 uint case takes an average of 17 ticks to create Pascal's Triangle. It takes 1 tick to obtain the total number of combinations .vs. 4 ticks for the Combination class alternative. Getting the rank and combination takes 2 and 3 ticks, respectively, where as the Combination class alternatives take 4 and 9 ticks, respectively. So, if you are not planning on using different ranks or different n' choose k' subcases, then you may be better off using the Combination class for simple one-off number of combinations, getting the rank from a combination, or getting the combination from the rank. The best way to find this out is to add your own test cases to the BenchmarkBinCoeff.cs file and see for yourself which approach is faster.

But, this project was not really intended for one-offs. When I first released this project back in 2011, it could only be used with a specific n choose k case. Those needing to quickly obtain the number of combinations, and convert between the rank and an array that represents a combination benefited the most.

New to the Jan 2021 release, the GetNumCombos, GetRank, and GetCombFromRank methods all have optional parameters that provide a way to specify a different n' choose k' subcase without having to recreate Pascal's Triangle. The limitation to this is that n' and k' must both be less than or equal to the respective n choose k case used to create the instance. In the code, numItems is often used to specify n' and groupSize is used to specify k'. Subcases provide a way to efficiently obtain the number of combinations, the rank, or the combination of a given rank that enhances performance.

Continuing with the 34 choose 17 uint test case, if GetNumCombos is called with optional parameters numItems = 30 and groupSize = 15, then it will return the correct number of combinations for that case and only use 1 tick to do so in an O(1) operation. If after that, the combination for the value returned by GetNumCombos - 1 (the highest ranked element for the n' choose k' subcase), then GetCombFromRank should be called with numItems = 30 and groupSize = 15 to obtain the 15-digit combination for that rank. The array (known in the code as kIndexes) is passed in as a parameter and it may have more elements than specified by groupSize. So, if the array contains 17 elements, then the first 15 elements would contain the combination in this case.

If at some point the combination needs to be converted back to its rank, then GetRank should be called with that same array and the optional parameter groupSize set to 15. If the array is larger than 15 elements, then only the first 15 are looked at and any remaining elements are ignored. This makes it easy to work with subcases.

Decreasing the value for n' from any n choose k case always decreases the total number of combinations for that subcase. However, the same is not necessarily true for k. There are times when decreasing the value of k' will increase the number of combinations. If the original k is higher than n / 2, then decreasing k will increase the number of combinations up to and including k' = n / 2.

For example, take the initial case of 8 choose 7, which yields 8 combinations. If a subcase of 8 choose 6 is tried, then the number of combinations increases to 28. The combinations keep increasing as k approaches half of n: 8 choose 5 = 56. 8 choose 4 = 70. 8 choose 3 = 56. 8 choose 2 = 28. 8 choose 1 = 8. As this example demonstrates, the number of combinations is symmetrical based upon the n choose k' case, with n choose (n / 2) as the case that generates the highest number of combinations. This can be inferred from the formula used to calculate the total number of combinations: n! / (k! * (n - k)!.

So, what this means is that there may be times when a main n choose k case will not overflow (when k is significantly higher than n / 2), but a subcase of n choose k' may overflow because the number of combinations won't fit within a uint or ulong. 

To help determine the appropriate implementation to use, two static base class methods are provided:

GetLeastTypeForCase - returns an enum value that specifies whether uint, ulong, or BigInteger should be used. The type returned is guaranteed to not overflow for the specified n choose k case and is the minimal implementation that can be used.

GetLeastTypeForAllSubcases - returns an enum value that specifies whether uint, ulong, or BigInteger should be used to handle all n' choose k' subcases. The type returned is the smallest type guaranteed to not overflow for all subcases.

So, some time should be invested into evaluating the n choose k cases and the type that needs to be used for the n choose k cases. Keep in mind that if GetNumCombos did not overflow, then neither will GetRank when using the same n choose k case, even for the highest rank, which is defined as number of combinations - 1. Ranks start from zero.

GetNumCombos returns zero for the number of combinations if overflow was detected. GetRank uses a bool out parameter to specify that overflow has occurred. Always check for overflow if you have not previously analyzed each of your n choose k cases and n' choose k' subcases for overflow.


LIBRARY FUNCTIONALITY

The library provides 3 different classes that implement uint (32-bit), ulong (64-bit), and BigInteger (unlimited) types in the BinCoeff, BincoeffL, and BinCoeffBigInt classes, respectively. A common base class provides the following functionality:

Properties:

NumItems - total number of items in the set used to create the instance. Also referred to as n as in n choose k.
GroupSize - number of items to group by. Also referred to as k in n choose k.

Static methods:

OutputCombos - writes out the specified combinations to a file.

GetLeastTypeForCase - returns an enum value that specifies whether uint, ulong, or BigInteger should be used. The type returned is guaranteed to not overflow for the specified n choose k case and is the minimal implementation that can be used.

GetLeastTypeForAllSubcases - returns an enum value that specifies whether uint, ulong, or BigInteger should be used to handle all n' choose k' subcases. The type returned is the smallest type guaranteed to not overflow for all subcases.

Each of the Bincoeff implementations for uint, ulong, and BigInteger provides the following common functionality:

Properties:

TotalCombos - Total number of unique combinations.

SubCaseOverflow - true means subcases may overflow; false means subcases probably will not overflow, but not guaranteed. Gets set to true in CreatePascalsTriangle if any calculated value is larger than uint.MaxValue for uint, or ulong.MaxValue for ulong. Also, it this value is false, it does not imply that a subcase will not overflow when calculating a rank from the combination.

Constructor:

public BinCoeff<T>(int numItems, int groupSize, uint totalCombos = 0, bool initTable = false)

numItems - parameter specifies the number of elements within the set to be processed. If less than one, then an exception is thrown.
groupSize - the number of elements contained in the group. If less than one, then an exception is thrown.
totalCombos - this optional parameter specifies the total number of combinations for this numItems choose groupSize case. If not specified or set to zero, then the constructor calls GetNumCombos to obtain it and sets the public property TotalCombos to this value.
initTable - creates a corresponding table from the specified generic parameter type if true is specified.

Typical examples of use that creates the BinCoeff object that use uints and ulongs for the case 10 choose 5:

BinCoeff<int> bc = new BinCoeff<int>(10, 5);

BinCoeffL bcl = new BinCoeffL10, 5);

The class generic parameter specifies the type of data that an optional data table will be composed of. The uint implementation is the only implementation that implements this feature, since a data table will most likely take up too much memory for larger number of combinations. See the TestBinCoeff code for additional examples.

GetNumCombos - calculates the number of combinations for the main n choose k case or any subcase n' choose k' subcase when the optional parameters are supplied. A subcase is any n' choose k' case where n' and k' are less than or equal to the respective n and k used to create the object. Subcases may overflow for the uint and ulong implementations. Zero is returned if there is overflow. The BigInteger implementation will never overflow, but may throw an out of memory exception if the numbers get too large. Subcase handling is consistently implemented with the methods described below.

GetRank - calculates the rank for the main n choose k case or any n' choose k' subcase. Since zero is a legitimate rank, a bool overflow variable is set when overflow occurs for the uint and ulong versions.

GetCombFromRank - calculates the combination in descending order for the specified rank. The value is returned in an int array that must have been previously created and passed in as the 2nd parameter. The values in the array start from zero and represent the set of n items, which is known as numItems in the code.

Methods that manage the generic table (only implemented in the BinCoeff (uint) class):

GetTable - returns a reference to the generic table. Table is implemented as a List<T>.
AddItem - adds an item to the table.
SetItem - sets table item to the specified value with the specified index.
SetItem - sets table item to the specified value with the specified combination.
GetItem - returns the specified item with the specified index.


HOW TO BEST USE THE LIBRARY

the BinCoeff (uint) class offers the best performance, so this class should be used when the total number of combinations is 2^32 or less. Even though the code is the fastest, it is limited to n choose k cases that don't exceed a 32-bit uint. For example, 34 choose 17 produces 2,333,606,220 combinations and 35 choose 16 produces 4,059,928,950 combinations, both of which will fit inside of a 32-bit uint. But, 35 choose 17 produces 4,537,567,650 combinations, which does not fit inside of a 32-bit uint. This means that the BinCoeffL class, which uses the ulong type, would need to be used for this case.

The BinCoeffL class may be used for up to 2^64 = 18,446,744,073,709,551,616 combinations. So, this class may be used for 67 choose 33, but not for 68 choose 33, which exceeds 2^64. In this case, the BinCoeffBigInt class would have to be used.

A very good binomial coefficient calculator on the web that uses big ints may be found here: https://www.ohrt.com/odds/binomial.php

You are not limited to using just one of these classes at a time. In other words, you could create an instance of all 3 implementations and then use each of them according to the n choose k situation. The BinCoeff and BinCoeffL classes don't use very much memory.


COMBINATION CLASS

The Combination class is used to test out the 3 implementations (uint, ulong, and BigInteger) that use Pascal's Triangle. Like each implementation, the Combination class provides methods based upon combinatorial math theory for the following functions:

1. Get the total number of combinations for an n choose k case.
2. Calculate the rank from a combination.
3. Calculate the combination for the specified rank.

The GetNumCombos method returns the number of combinations. It incorporates the time saving step:
if (k > n - k)
   k = n - k;

This is because C(n, k) = C(n, n - k). Here is a simple algebraic proof of this:

We start with the formula to calculate the number of combinations for any n choose k case:

C(n, k) = n! / (k! (n - k)!), where n >= k > 0.

If k really does equal n - k, then we should be able to substitute the value (n - k) for k on one side of the equation. Thus:

1. n! / k! (n - k)! = n! / (n - k) ! (n - (n - k))!

2. Divide by n! and invert both sides of the equation to end up with:
k! (n - k)! = (n - k)! (n - (n - k))!

3. Divide by (n - k)! to end up with:
k! = (n - (n - k))!

4. The last step of simplifying by subtracting out n leaves:
k! = k!

Thus, the assertion is proven.


LIBRARY CODE

In each implementation, there is a private method called CreatePascalsTriangle that creates a list of arrays of type uint, ulong, and BigInteger for BinCoeff, BincoeffL, and BinCoeffBigInt, respectively. This is the key method behind the faster performance. I did consider and actually tried to write a generic method that would handle all 3 of these implementations, but unfortunately C# does not allow two generic variables to be added together or even compared (0). The same is true in Java (1). But, adding 2 generic variables together is allowed in C++ (2).

There are at least 2 workarounds to this. One is to convert the variables to a dynamic type and then add them together. The problem with this is that Dynamic is about 18 times slower than static variables (3). The primary reason I wrote this library is for performance. So, this approach seemed unacceptable. The other workaround is to convert the variables to an expression tree and then add them together. This approach is faster than using Dynamic, but it is still slow, so I discarded this idea as well.

I did think about writing a generic class in C++ that would handle each of these types, but I have already spent far too much time with this project. I will leave it as an exercise to a gifted C++ developer looking for an interesting project. I would advise having 2 separate C++ implementations. A templated version that includes overflow checking for both uint and ulong, and another version that contains no overflow checks for the big integer implementation. Keep in mind that C++ does not offer a big integer class (as of Jan, 2021). However there are some C++ libraries that may be used. I would recommend Boost, in general, because of reliability and minimal license restrictions.

(0) https://stackoverflow.com/a/8122675/643828
(1) https://stackoverflow.com/questions/29010699/can-i-add-two-generic-values-in-java
(2) https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/generics/differences-between-cpp-templates-and-csharp-generics
(3) https://dev.to/mokenyon/benchmarking-and-exploring-c-s-dynamic-keyword-1l5i

The CreatePascalsTriangle method creates a list of arrays of the type uint, ulong, or BigInteger for the respective BinCoeff, BinCoeffL, or BinCoeffBigInt class. Each array in the list represents a diagonal of numbers in the triangle rather than a horizontal line of numbers across (4). This makes it easier to work with the code. The first array created is the least significant one where each value is one additional away than the previous value. After that, each diagonal array entry is created from the two values before it for all the remaining arrays up to group size minus one. If this is not easily comprehended, then try debugging the code and place a breakpoint on the following line in CratePascalsTriangle:

indexArray[loop] = indexArray[loop - 1] + indexArrayPrev[loop - 1];

As you step through the code, refer back to an image of Pascal's Triangle (4) and observe that each value is created correctly. The last diagonal in Pascal's Triangle is not created since it can be easily inferred.

(4) https://tablizingthebinomialcoeff.wordpress.com/

There are 3 projects that make up the library:

1. BinomialCoefficient - library code that provides BinCoeff, BinCoeffL, and BinCoeffBigInt classes that uses respective types of uint, ulong, and BigInteger types that provide methods to get the number of combinations for the specified n choose k case or subcase, the rank of a combination, the combination from a given rank, and a method to output a specified range of combinations to a text file. In addition, the uint version provides the capability to manage a generic table. A Combination class is provided that provides similar functionality using combinatorial math instead of Pascal's Triangle so that the code may be tested. A base class called BinCoeffBase provides common functionality and is used as the base class for all 3 implementations.

2. TestBinCoeff - provides robust tests that prove the code works as advertised. Most of the code has been consolidated into two methods:

A. TestRanksAndCombos - looks at 14 n choose k cases and verifies that all the ranks (if < 1,000) work correctly. If more than 1,000 combinations, then limits the number of ranks to the bottom and top 500, for a total of 1,000. All 3 implementations are tested with these 14 n choose k cases.

B. TestSubCases - looks at mostly a different set of 14 n choose k cases and verifies that the subcases for each main n choose k case are correctly handled. 

3. BenchmarkBinCoeff - benchmarks each implementation against the math version defined in the Combination class. Summary of benchmark results:

BinCoeff (uint)   - Pascal's Triangle is roughly 7 times faster than the corresponding Combination methods.
BinCoeffL (ulong) - Pascal's Triangle is roughly 16 times faster than the corresponding Combination methods.
BinCoeffBigInt    - Pascal's Triangle is roughly 10 times faster than the corresponding Combination methods.

The BinCoeff and BinCoeffL n choose k test cases were chosen so that they would not overflow. So, even though a ulong n choose k case results in a number of combinations that will fit within a 64-bit value, it does not mean that the Combination class will not overflow. The reason for this is because of the way the combinations are calculated:

for (loop = 1; loop <= k; loop++)
{
  r *= (ulong)n--;
  r /= (ulong)loop;
}

So, this code overflows for the following cases: 200 choose 12, 74 choose 24, 67 choose 33, and many more. None of these test cases overflows with the BinCoeffL class because the code adds the precalculated binomial coefficients together instead of multiplying (which causes the overflow) and then dividing. If overflow occurs in a Combination method, then it severely impacts the benchmark results in that it could take 100 to 300 times as long because the only work around is to call the big integer version of the method. That is another reason why the Combination class should be avoided and one of the 3 implementations that uses Pascal's Triangle used instead.


RUNNING THE TEST CODE:

The TestBinCoeff project tests the 3 implemenations with many n choose k cases. A new test method called TestSubCases tests all of the subcases for each n choose k case against the respective Combination class math methods.

Two command line parameters may be specified:

1. RunAll / NoRunAll (default)

When the default value of NoRunAll is specified, the last test case of 200 choose 100 is not run. This case takes many multiples of time to run than all of the other tests for all of the other methods combined. When RunAll is specified, all of the n choose k cases in TestSubCases are tested, including 200 choose 100.

NoRunAll will complete in about 30 seconds in release mode and roughly double that in debug mode. RunAll takes just over 3.5 minutes to complete on my test machine in release mode.

2. DebugOutput / NoDebugOutput (default)

When DebugOutput is specified, TestSubCases provides debug output as to which subcases are tested. For example:

100 choose 95 uint:
95 94 93
100 choose 95 ulong:
95 94 93 92 91 90 89 88 87 86 85 84 83 82
100 choose 95 big int:
95 94 93 92 91 90 89 88 87 86 85 84 83 82 81 80 79 78 77 76 75 74 73 72 71 70 69 68 67 66 65 64 63 62 61 60 59 58 57 56 55 54 53 52 51 50 49 48 47 46 45 44 43 42 41 40 39 38 37 36 35 34 33 32 31 30 29 28 27 26 25 24 23 22 21 20 19 18 17 16 15 14 13 12 11 10 9 8 7 6 5 4 3 2 1

So, for the uint subcases of 100 choose 95, 100 choose 92 is the first subcase that overflows. For ulong, the first case to overflow is 100 choose 81. No subcases overflow for the big int tests.

When NoDebugOutput is specified, then plus signs are displayed to let the user know that the test program is not hanging, but rather doing something constructive.

The code may be run by building it in release mode and then navigating to the download folder/TestBinCoeff/bin/release from Windows Explorer. Next, either double click on TestBinCoeff.exe or from the Windows Explorer address bar enter TestBinCoeff.exe, followed by one or both of the desired command line parameters.


CHANGE LOG:

Author Robert G. Bryan

Apr, 2011:
Original release of Bincoeff (int) class

Apr, 2015:
Fixed the code to work with n choose 1 cases and also includes a new version called BinCoeffL that works with long values.

Jan, 2021:
1. Added a BigInteger class implementation called BinCoeffBigInt. This class does not overflow, but could run out of memory if the resulting numbers get big enough.

2. Implemented a more general way to handle multiple n choose k cases, where the user decides on the maximum n and k. This means that multiple n choose k cases may be handled from the same Pascal's Table and that table only needs to be generated once, which was not the case in the original version. I call this functionality "subcases" of the main n choose k case. The main limitation of subcases are that n and k can't go up, but may go down. Also, there are cases where the original n choose k case won't overflow, but the subcases will. For example - the main case of 100 choose 95 does not overflow when using 32-bit uints (Bincoeff class). But, the subcase of 100 choose 50 will overflow both the 32-bit uint and 64-bit ulong versions. It does not overflow the BigInteger implementation (BinCoeffBigInt).

3. A new version of calculating the total number of unique combinations is provided that is much faster [O(1) time] since it uses Pascal's Triangle to look up the binomial coefficient sums instead of calculating them, but it does have the restriction that it can't be used if the table has yet to be created or either n or k is greater than that used to create Pascal's Triangle.

4. Cleaned up method names and variable names to be more descriptive and to use Microsoft recommended variable naming guidelines.

5. Changed OutputCombos to a generic method so it could be used for all 3 implementations. Added an abstract GetCombFromRank method
so that each implementation can override it to get the number of combos from a base class reference.

6. Changed Bincoeff & BincoeffL to use uint and ulong from int & long, respectively.

7. Added more robust testing, which includes more n choose k cases, edge cases like 100 choose 100, 100 choose 99, 100 choose 1, etc.
The test code has been consolidated and also implemented extensive subcase testing - see TestBinCoeff.TestSubCases.

8. Added tests for GetLeastTypeForCase & GetLeastTypeForAllSubCases.


LICENSE:

All the code is in the public domain, which means the code may be used in commercial projects without attribution or payment.


WARRANTY:

None of this code is covered by a warranty. Use at your own risk. The user assumes responsibility for any bugs or anomalies.

Test your n choose k cases carefully.
