using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Dilithium;

namespace Dilithium
{
    public class Scheme
    {
        #region Dilithium Constants

        public const int N = 256;
        public const int Q = 8_380_417;
        public const int D = 14;
        public const int Gamma1 = (Q - 1) / 16;
        public const int Gamma2 = Gamma1 / 2;


        #endregion

        #region Dilithium Mode Params

        private static int k;
        private static int l;
        private static int eta;
        private static int beta;
        private static int omega;

        #endregion

        private static Random random = new Random((int)DateTime.Now.Ticks);
        private static SHA512 sha = new SHA512Managed();

        public static void SetUpMode(int mode)
        {
            (k, l, eta, beta, omega) = mode switch
            {
                1 => (3, 2, 7, 375, 64),
                2 => (4, 3, 6, 325, 80),
                3 => (5, 4, 5, 275, 96),
                4 => (6, 5, 3, 175, 120),
                _ => throw new NotImplementedException()
            };
        }

        private static byte[] GenerateBits(int bitsCount)
        {
            var bytes = new byte[bitsCount / 8];
            random.NextBytes(bytes);

            return bytes;
        }

        /// <summary>
        /// Generates number in [-boundary; boundary] (in Z_Q)
        /// </summary>
        /// <returns></returns>
        private static int GenerateNumberInBoundaries(int boundary)
        {
            //ToDo: Create bits to Int method
            var number = (int)new BigInteger(GenerateBits(32));

            return Mod(number % (2 * boundary + 1));
        }

        private static int CenteredMod(int number, int modulo)
        {
            if (modulo % 2 != 0)
                throw new NotImplementedException();

            int result;

            if (number >= modulo)
            {
                result = number % modulo;
            }
            else if (number < 0)
            {
                result = (number + (number / -modulo + 1) * modulo) % modulo;
            }
            else
            {
                result = number;
            }

            if (result > modulo / 2)
            {
                return result - modulo;
            }
            else
            {
                return result;
            }
        }

        private static int Mod(int number)
        {
            if (number >= Q)
            {
                return number % Q;
            }
            else if (number < 0)
            {
                return (number + (number / -Q + 1) * Q) % Q;
            }
            else
            {
                return number;
            }
        }

        public static long Mod(long number, int modulo = Q)
        {
            if (number >= Q)
            {
                return number % Q;
            }
            else if (number < 0)
            {
                return (number + (number / -Q + 1) * Q) % Q;
            }
            else
            {
                return number;
            }
        }

        private static Polynom[] GenerateCenteredVector(int centeringModulo, int length)
        {
            var result = new Polynom[length];

            for (int iterVect = 0; iterVect < length; iterVect++)
            {
                result[iterVect] = new Polynom(512);

                for (int iterPoly = 0; iterPoly < 512; iterPoly++)
                {
                    result[iterVect].Coefficients[iterPoly] = GenerateNumberInBoundaries(centeringModulo);
                }
            }

            return result;
        }

        private static int IntBitCount = 32;

        public static int ByteBitCount = 8;

        public static bool[] ToBitArray(byte[] bytes)
        {
            var bits = new bool[ByteBitCount * bytes.Length];
            var bitPos = bits.Length - 1;

            for (int i = bytes.Length - 1; i >= 0; i--)
            {
                var curByte = bytes[i];

                for (int _ = 0; _ < ByteBitCount; _++)
                {
                    bits[bitPos] = (curByte & 1) == 1;
                    curByte >>= 1;
                    bitPos--;
                }
            }

            return bits;
        }

        public static string ToString(bool[] bits)
        {
            var builder = new StringBuilder();

            for (int i = 0; i < bits.Length; i++)
            {
                builder.Append(bits[i] ? "1" : "0");

                if (i > 0 && i % 8 == 7)
                {
                    builder.Append(" ");
                }
            }

            return builder.ToString();
        }

        public static int GetIntFromBits(bool[] bits, int startIndex)
        {
            var length = bits.Length;
            int result = 0;

            for (int i = startIndex; i < startIndex + IntBitCount; i++)
            {
                result |= bits[i % 512] ? 1 : 0;
                result <<= 1;
            }

            return result;
        }

        public static Polynom[] Multiply(Polynom[,] matrix, Polynom[] vector)
        {
            var matrixRows = matrix.GetLength(0);
            var matrixCols = matrix.GetLength(1);

            var result = new Polynom[matrixRows];

            for (int i = 0; i < matrixRows; i++)
            {
                result[i] = new Polynom(512);

                for (int j = 0; j < matrixCols; j++)
                {
                    result[i] += matrix[i, j] * vector[j];
                }
            }

            return result;
        }

        public static Polynom[] Add(Polynom[] a, Polynom[] b)
        {
            var count = a.Length;
            var result = new Polynom[count];

            for (int i = 0; i < count; i++)
            {
                result[i] = a[i] + b[i];
            }

            return result;
        }

        public static Polynom[] Subtract(Polynom[] a, Polynom[] b)
        {
            var count = a.Length;
            var result = new Polynom[count];

            for (int i = 0; i < count; i++)
            {
                result[i] = a[i] - b[i];
            }

            return result;
        }

        public static byte[] ConcatBytes(byte[] source, byte[] nonce)
        {
            var result = new byte[source.Length + nonce.Length];

            source.CopyTo(result, 0);
            nonce.CopyTo(result, source.Length);

            return result;
        }

        public static byte[] GetHash(byte[] source, int length = 512)
        {
            var hash = sha.ComputeHash(source);

            return hash.Take(length / 8).ToArray();
        }

        public static Polynom[,] ExpandA(byte[] seed)
        {
            var result = new Polynom[k, l];

            for (int i = 0; i < k; i++)
            {
                for (int j = 0; j < l; j++)
                {
                    result[i, j] = new Polynom(512);

                    var nonce = new[] { (byte)((i << 4) + j) };

                    var hash = GetHash(ConcatBytes(seed, nonce));
                    var bitHash = ToBitArray(hash);

                    //Console.WriteLine("Initial bits: {0}", ToString(bitHash));

                    var poly = result[i, j];

                    for (int coef = 0; coef < 512; coef++)
                    {
                        poly.Coefficients[coef] = Mod(GetIntFromBits(bitHash, coef));
                    }
                }
            }

            return result;
        }

        public static (Polynom[], Polynom[]) Power2Round(Polynom[] vector)
        {
            var modulo = Convert.ToInt32(Math.Pow(2, D));

            var r0 = vector.Transform(c => CenteredMod(c, modulo));

            var r1 = Subtract(vector, r0).Transform(c => c / modulo);

            return (r1, r0);
        }

        public static (Polynom[], Polynom[]) Decompose(Polynom[] vector, int modulo)
        {
            var r0 = vector.Transform(c => CenteredMod(c, modulo));
            var r1 = new Polynom[vector.Length];

            for (int iterVect = 0; iterVect < vector.Length; iterVect++)
            {
                r1[iterVect] = new Polynom(512);

                for (int iterCoef = 0; iterCoef < 512; iterCoef++)
                {
                    if (Mod(vector[iterVect].Coefficients[iterCoef] - r0[iterVect].Coefficients[iterCoef]) == Q - 1)
                    {
                        r1[iterVect].Coefficients[iterCoef] = 0;
                        r0[iterVect].Coefficients[iterCoef] -= 1;
                    }
                    else
                    {
                        r1[iterVect].Coefficients[iterCoef] = (vector[iterVect].Coefficients[iterCoef] - r0[iterVect].Coefficients[iterCoef]) / modulo;
                    }
                }
            }

            return (r1, r0);
        }

        public static Polynom[] HighBits(Polynom[] vector, int modulo) => Decompose(vector, modulo).Item1;

        public static Polynom[] LowBits(Polynom[] vector, int modulo) => Decompose(vector, modulo).Item2;

        public static Polynom BallInHash(byte[] bytes)
        {
            var hash = GetHash(bytes);

            var poly = new Polynom(512);

            var signs = 0;
            for (int i = 0; i < 8; ++i)
                signs |= hash[i] << 8 * i;
            
            var pos = 8;
            var b = 0;

            for (int i = 392; i < 512; i++)
            {
                do
                {
                    if (pos >= 64)
                    {
                        hash = GetHash(hash);
                        pos = 0;
                    }

                    b = hash[pos++];
                } while (b > i + 256);

                poly.Coefficients[i] = poly.Coefficients[b];
                poly.Coefficients[b] = 1;
                poly.Coefficients[b] ^= -(signs & 1) & (1 ^ (Q - 1));
                signs >>= 1;
            }

            return poly;
        }

        public static bool CheckNorm(Polynom polynom, int bound)
        {
            int t;
            
            for (int i = 0; i < 512; i++)
            {
                t = (Q - 1) / 2 - polynom.Coefficients[i];
                t ^= t >> 31;
                t = (Q - 1) / 2 - t;
                
                //t = CenteredMod(polynom.Coefficients[i], )

                if (t >= bound)
                    return false;
            }

            return true;
        }

        public static bool CheckNorm(Polynom[] vector, int bound)
        {
            foreach (var polynom in vector)
            {
                if (!CheckNorm(polynom, bound))
                    return false;
            }

            return true;
        }


        public static (PublicKey, SecretKey) GenerateKeys()
        {
            var rho = GenerateBits(256);
            rho = new byte[] {
                1, 44, 91, 253,
                148, 28, 21, 210,
                190, 181, 205, 240,
                89, 59, 39, 229,
                85, 40, 88, 53,
                251, 230, 107, 139,
                177, 163, 160, 113,
                59, 62, 41, 12,
            };

            //var K = new BigInteger(GenerateBits(256));

            Polynom[] s1 = GenerateCenteredVector(eta, l),
                s2 = GenerateCenteredVector(eta, k);

            var A = ExpandA(rho);

            var t = Add(Multiply(A, s1), s2);

            //var (t1, t0) = Power2Round(t);

            //var tr = GetHash(ConcatBytes(rho, t1.ToByteArray()), 384);

            return (new PublicKey
            {
                MatrixA = A,
                VectorT = t
            }, new SecretKey
            {
                MatrixA = A,
                VectorT = t,
                VectorS1 = s1,
                VectorS2 = s2
            });
        }

        public static Signature Sign(SecretKey secretKey, string message)
        {
            while (true)
            {
                var y = GenerateCenteredVector(Gamma1 - 1, l);

                var w1 = HighBits(Multiply(secretKey.MatrixA, y), 2 * Gamma2);

                var c = BallInHash(ConcatBytes(Encoding.Unicode.GetBytes(message), w1.ToByteArray()));
                //var count = c.Coefficients.Count(c => c != 0);

                var z = Add(y, secretKey.VectorS1.Transform(p => p * c));

                if (!CheckNorm(z, Gamma1 - beta))
                {
                    //continue;
                }

                var Ay = Multiply(secretKey.MatrixA, y);
                var cs2 = secretKey.VectorS2.Transform(p => p * c);
                if (!CheckNorm(LowBits(Subtract(Ay, cs2), 2*Gamma2), Gamma2 - beta))
                {
                    //continue;
                }

                return new Signature { VectorZ = z, PolynomC = c };
            }
        }

        public static bool Verify(PublicKey publicKey, Signature signature, string message)
        {
            var Az = Multiply(publicKey.MatrixA, signature.VectorZ);
            var ct = publicKey.VectorT.Transform(p => p * signature.PolynomC);


            var w1Prime = HighBits(Subtract(Az, ct), 2 * Gamma2);

            if (!CheckNorm(signature.VectorZ, Gamma1 - beta))
            {
                //return false
            }

            var cPrime = BallInHash(ConcatBytes(Encoding.Unicode.GetBytes(message), w1Prime.ToByteArray()));
            if (cPrime != signature.PolynomC)
            {
                return false;
            }

            return true;
        }
    }
}


public struct Polynom
{
    public int[] Coefficients;

    public Polynom(int length)
    {
        Coefficients = new int[length];
    }

    public static Polynom operator *(Polynom a, Polynom b)
    {
        var result = new Polynom(512);

        for (int iterA = 0; iterA < 512; iterA++)
        {
            for (int iterB = 0; iterB < 512; iterB++)
            {
                if (iterA + iterB >= 512)
                    continue;

                result.Coefficients[iterA + iterB] = (int)Scheme.Mod((long)a.Coefficients[iterA] * (long)b.Coefficients[iterB]);
            }
        }

        return result;
    }

    public static Polynom operator +(Polynom a, Polynom b)
    {
        var result = new Polynom(512);

        for (int i = 0; i < 512; i++)
        {
            result.Coefficients[i] = (int)Scheme.Mod((long)a.Coefficients[i] + (long)b.Coefficients[i]);
        }

        return result;
    }

    public static Polynom operator -(Polynom a, Polynom b)
    {
        var result = new Polynom(512);

        for (int i = 0; i < 512; i++)
        {
            result.Coefficients[i] = (int)Scheme.Mod((long)a.Coefficients[i] - (long)b.Coefficients[i]);
        }

        return result;
    }

    public static bool operator ==(Polynom a, Polynom b)
    {
        for (int i = 0; i < 512; i++)
        {
            if (a.Coefficients[i] != b.Coefficients[i])
                return false;
        }

        return true;
    }

    public static bool operator !=(Polynom a, Polynom b)
    {
        return !(a == b);
    }
}

public class Matrix
{
    public Polynom[,] Values;
}