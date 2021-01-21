using System.Numerics;

namespace SZDL.Plain
{
    public class PrimeGeneratorSettings
    {
        /// <summary>
        /// Bits length of prime number generated
        /// </summary>
        public int KeyLength { get; set; }

        public int KeyLengthNumber => (int)KeyLength;

        /// <summary>
        /// Ignore stored prime numbers
        /// </summary>
        public bool IgnoreStoredData { get; set; } = false;

        /// <summary>
        /// Use parallel for calculating prime multipliers
        /// </summary>
        public bool UseParallel { get; set; } = true;

        /// <summary>
        /// Count of Tasks used for calculating prime multipliers
        /// </summary>
        public int ParallelTaskCount { get; set; } = 3;

        public BigInteger[] NumbersInFactorization { get; set; } = { };

        public BigInteger AccumulatingValue { get; set; } = BigInteger.Zero;

        public bool ShowIterations { get; set; } = true;

        public int MaxIteration { get; set; } = 1000;
    }
}