using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinCoeffBenchmark
{
   class Program
   {
      static void Main(string[] args)
      {
         BinCoeffBC binBC = new BinCoeffBC();
         binBC.Benchmark();
         BinCoeffBCL binBCL = new BinCoeffBCL();
         binBCL.Benchmark();
         BinCoeffBCBigInt binBCBBI = new BinCoeffBCBigInt();
         binBCBBI.Benchmark();
         Console.WriteLine();
         Console.WriteLine("Press any key to exit.");
         Console.ReadKey();
      }
   }
}
