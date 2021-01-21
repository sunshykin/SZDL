using System;
using System.Numerics;

namespace SZDL.Ring
{
    public class Utils
    {
        public static RingMatrix GenerateRightUnit(RingMatrix source)
        {
            return new RingMatrix(
                1 - source[2],
                source[2],
                source[1],
                1 - source[1]
            );
        }

        private static Random random = new Random((int)DateTime.Now.Ticks);

        public static RingBigInteger GenerateNumber()
        {
            var bytes = new byte[256 / 8];
            random.NextBytes(bytes);

            return new RingBigInteger(bytes);
        }

        public static RingMatrix GenerateMatrix()
        {
            return new RingMatrix(
                GenerateNumber(),
                GenerateNumber(),
                GenerateNumber(),
                GenerateNumber()
            );
        }

        public static RingMatrix GenerateNonInvertibleMatrix()
        {
            RingBigInteger a1 = GenerateNumber(),
                a2 = GenerateNumber(),
                a3 = GenerateNumber();

            var a1Inverse = !a1;
            var a4 = a2 *a3 * a1Inverse;

            return new RingMatrix(
                a1,
                a2,
                a3,
                a4
            );
        }
    }
}