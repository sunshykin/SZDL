using System;
using System.Numerics;

namespace Dilithium
{
    public static class Extensions
    {
        public static Polynom[] Transform(this Polynom[] source, Func<int, int> action)
        {
            var result = new Polynom[source.Length];
            var polynomPow = source[0].Coefficients.Length;

            for (int iterVect = 0; iterVect < source.Length; iterVect++)
            {
                result[iterVect] = new Polynom(polynomPow);

                for (int iterCoef = 0; iterCoef < polynomPow; iterCoef++)
                {
                    result[iterVect].Coefficients[iterCoef] = action.Invoke(source[iterVect].Coefficients[iterCoef]);
                }
            }

            return result;
        }
        
        public static Polynom[] Transform(this Polynom[] source, Func<Polynom, Polynom> action)
        {
            var result = new Polynom[source.Length];

            for (int iterVect = 0; iterVect < source.Length; iterVect++)
            {
                result[iterVect] = action.Invoke(source[iterVect]);
            }

            return result;
        }

        public static byte[] ToByteArray(this int source)
        {
            //ToDo: optimize here
            var bigInt = new BigInteger(source);

            return bigInt.ToByteArray();
        }
        
        public static byte[] ToByteArray(this Polynom[] source)
        {
            var result = new byte[source.Length * 512 * 32 / Scheme.ByteBitCount];

            var iterator = 0;
            
            foreach (var poly in source)
            {
                foreach (var coef in poly.Coefficients)
                {
                    coef.ToByteArray().CopyTo(result, iterator);
                    iterator += 32 / Scheme.ByteBitCount;
                }
            }

            return result;
        }
    }
}