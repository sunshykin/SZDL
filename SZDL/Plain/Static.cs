using System.Numerics;

namespace SZDL.Plain
{
    public static class Static
    {
        public const int N = 384;
        public static BigInteger P;
        public static BigInteger Q;


        public static BigInteger NumberK;
        public static BigInteger NumberE;

        public static PrimeGenerator PrimeGenerator { get; set; } = new PrimeGenerator(new PrimeGeneratorSettings
        {
            KeyLength = N,
            ParallelTaskCount = 5,
            NumbersInFactorization = new BigInteger[] { 2, 3, 7, 13, 31 },
            AccumulatingValue = 109,
            ShowIterations = false
        });
    }
}