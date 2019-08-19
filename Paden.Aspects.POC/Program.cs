using Paden.Aspects.DAL;
using System;
using System.Linq;

namespace Paden.Aspects.POC
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(new StudentRepository().ReadAll().First().Name);
        }
    }
}
