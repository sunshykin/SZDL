using System;
using System.Collections;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Threading.Tasks;
using SZDL.Plain;

namespace SZDL.Plain
{
    public class Utils
    {
        public static Matrix GenerateRightUnit(Matrix source)
        {
            return new Matrix(
                (new BigInteger(1) - source[2]).Mod(),
                source[2],
                source[1],
                (new BigInteger(1) - source[1]).Mod()
            );
        }

        private static Random random = new Random((int)DateTime.Now.Ticks);
        private static RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

        public static BigInteger GeneratePrimeNumber()
        {
            var bytes = new byte[Static.N / 8];
            random.NextBytes(bytes);

            var result = new BigInteger(bytes);

            while (result <= 0 || !MillerRabinTest(result))
            {
                random.NextBytes(bytes);
                result = new BigInteger(bytes);
            }

            return result;
        }
        public static BigInteger GeneratePrimeNumberRNG()
        {
            byte[] bytes = new byte[Static.N / 8];
            rng.GetBytes(bytes);

            var result = new BigInteger(bytes);

            while (result <= 0 || !MillerRabinTest(result))
            {
                rng.GetBytes(bytes);
                result = new BigInteger(bytes);
            }

            return result;
        }
        
        public static BigInteger GenerateNumber()
        {
            var bytes = new byte[Static.N / 2 / 8];
            random.NextBytes(bytes);

            return new BigInteger(bytes).Mod();
        }

        public static Matrix GenerateMatrix()
        {
            return new Matrix(
                GenerateNumber(),
                GenerateNumber(),
                GenerateNumber(),
                GenerateNumber()
            );
        }

        public static Matrix GenerateNonInvertibleMatrix()
        {
            BigInteger a1 = GenerateNumber(),
                a2 = GenerateNumber(),
                a3 = GenerateNumber();

            /*while (a1.IsZero)
            {
                a1 = GenerateNumber();
            }
            while (a2.IsZero)
            {
                a2 = GenerateNumber();
            }
            while (a3.IsZero)
            {
                a3 = GenerateNumber();
            }*/

            var a1Inverse = a1.ModInverse(Static.P);
            var a4 = a2.MultiplyMod(a3).MultiplyMod(a1Inverse);

            return new Matrix(
                a1,
                a2,
                a3,
                a4
            );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="number">Testing number N</param>
        /// <param name="roundCount">Count of rounds of testing K</param>
        /// <returns></returns>
        public static bool MillerRabinTest(BigInteger number, int roundCount = 10)
        {
            // Prime if faced 2 || 3
            if (number == 2 || number == 3)
                return true;

            // Not Prime if faced even || <2
            if (number < 2 || number.IsEven)
                return false;

            // представим n − 1 в виде (2^s)·t, где t нечётно, это можно сделать последовательным делением n - 1 на 2
            BigInteger t = number - 1;
            var nMinusOne = number - 1;

            int s = 0;

            while (t.IsEven)
            {
                t /= 2;
                s += 1;
            }
            
            byte[] _a;
            BigInteger a;
            var numberByteLength = number.GetByteCount();

            // повторить k раз
            for (int i = 0; i < roundCount; i++)
            {
                _a = new byte[numberByteLength];

                // выберем случайное целое число a в отрезке [2, n − 2]
                //do
                //{
                //    rng.GetBytes(_a);
                //    a = new BigInteger(_a);
                //}
                //while (a < 2 || a >= number - 2);

                random.NextBytes(_a);
                a = BigInteger.Abs(new BigInteger(_a)) % (number - 4) + 2;

                // x ← a^t mod n, вычислим с помощью возведения в степень по модулю
                BigInteger x = BigInteger.ModPow(a, t, number);

                // если x == 1 или x == n − 1, то перейти на следующую итерацию цикла
                if (x.IsOne || x == nMinusOne)
                    continue;

                // повторить s − 1 раз
                for (int r = 1; r < s; r++)
                {
                    // x ← x^2 mod n
                    x = (x * x) % number;
                    //x = BigInteger.ModPow(x, 2, number);

                    // если x == 1, то вернуть "составное"
                    if (x.IsOne)
                        return false;

                    // если x == n − 1, то перейти на следующую итерацию внешнего цикла
                    if (x == nMinusOne)
                        break;
                }

                if (x != nMinusOne)
                    return false;
            }

            // вернуть "вероятно простое"
            return true;
        }
        
        public static byte[] ConcatBytes(byte[] source, byte[] nonce)
        {
            var result = new byte[source.Length + nonce.Length];

            source.CopyTo(result, 0);
            nonce.CopyTo(result, source.Length);

            return result;
        }

        public static BigInteger GetMinNumberExactBitLength(int bitLength)
        {
            bool signByteNeeded = bitLength % 8 == 0;

            // Calculating byte length
            var byteLength = bitLength / 8 + 1;

            // Creating array of bytes
            var bytes = new byte[byteLength];

            // Calculating shift
            int bitShift = (bitLength % 8 + 7) % 8;

            // Calculating high byte
            byte highByte = 0b_1;
            highByte <<= bitShift;

            if (signByteNeeded)
                bytes[byteLength - 2] = highByte;
            else
                bytes[byteLength - 1] = highByte;

            return new BigInteger(bytes);
        }
    }
}