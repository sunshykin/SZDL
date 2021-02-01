using System.Numerics;

namespace SZDL.Plain
{
    public static class Extensions
    {
        public static BigInteger MultiplyMod(this BigInteger a, BigInteger b)
        {
            return (a * b) % Static.P;
        }
        
        public static BigInteger Mod(this BigInteger source)
        {
            if (source >= Static.P)
            {
                return source % Static.P;
            }
            else if (source < 0)
            {
                return (source + (source / -Static.P + 1) * Static.P) % Static.P;
            }
            else
            {
                return source;
            }
        }
        
        public static BigInteger Mod(this BigInteger source, BigInteger modulus)
        {
            if (source >= modulus)
            {
                return source % modulus;
            }
            else if (source < 0)
            {
                return (source + (source / -modulus + 1) * modulus) % modulus;
            }
            else
            {
                return source;
            }
        }


        public static BigInteger ModInverse(this BigInteger a, BigInteger modulus)
        {
            if (modulus.IsOne)
                return 0;

            BigInteger originModulus = modulus;

            (BigInteger x, BigInteger y) = (1, 0);

            while (a > 1)
            {
                BigInteger q = a / modulus;
                (a, modulus) = (modulus, a % modulus);
                (x, y) = (y, x - q * y);
            }

            return x < 0
                ? x + originModulus
                : x;
        }

        public static BigInteger Negate(this BigInteger source)
        {
            return BigInteger.Negate(source) + Static.P;
        }
    }
}