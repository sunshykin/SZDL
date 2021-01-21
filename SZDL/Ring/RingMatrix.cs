using System;
using System.Linq;
using System.Text;

namespace SZDL.Ring
{
    public class RingMatrix
    {
        private readonly RingBigInteger[,] internalMatrix;

        public RingMatrix()
        {
            internalMatrix = new RingBigInteger[2, 2];
        }

        public RingMatrix(RingBigInteger elem1, RingBigInteger elem2, RingBigInteger elem3, RingBigInteger elem4) : this()
        {
            internalMatrix[0, 0] = elem1;
            internalMatrix[0, 1] = elem2;
            internalMatrix[1, 0] = elem3;
            internalMatrix[1, 1] = elem4;
        }

        public RingBigInteger this[int i]
        {
            get
            {
                if (i < 1 || i > 4)
                    throw new ArgumentException("Matrix has no element with that index");

                return i switch
                {
                    1 => internalMatrix[0, 0],
                    2 => internalMatrix[0, 1],
                    3 => internalMatrix[1, 0],
                    4 => internalMatrix[1, 1],
                };
            }
        }

        public static RingMatrix operator *(RingMatrix a, RingMatrix b)
        {
            return new RingMatrix(
                a[1] * b[1] + a[2] * b[3],
                a[1] * b[2] + a[2] * b[4],
                a[3] * b[1] + a[4] * b[3],
                a[3] * b[2] + a[4] * b[4]
            );
        }

        public static RingMatrix operator ^(RingMatrix source, RingBigInteger exponent)
        {
            if (exponent.Value.IsZero)
                return new RingMatrix(1, 0, 0, 1);

            if (exponent.Value % 2 == 1)
                return source * (source ^ (exponent - 1));

            var square = source ^ (exponent.Value / 2);

            return square * square;
        }


        public static RingMatrix operator !(RingMatrix source)
        {
            var det = source.GetDeterminant();
            var detInverse = !det;

            return new RingMatrix(
                source[4] * detInverse,
                -source[2] * detInverse,
                -source[3] * detInverse,
                source[1]* detInverse
            );
        }

        public override string ToString()
        {
            return $"({this[1]}, {this[2]}), ({this[3]}, {this[4]})";
        }

        public string ToByteString()
        {
            var builder = new StringBuilder();
            builder.AppendJoin(null,
                String.Join(null, this[1].Value.ToByteArray().Select(bt => $"{bt:X2}")),
                String.Join(null, this[2].Value.ToByteArray().Select(bt => $"{bt:X2}")),
                String.Join(null, this[3].Value.ToByteArray().Select(bt => $"{bt:X2}")),
                String.Join(null, this[4].Value.ToByteArray().Select(bt => $"{bt:X2}"))
            );

            return builder.ToString();
        }
        
        public bool IsInvertible()
        {
            return this[1] * this[4] != this[2] * this[3];
        }

        public bool IsNonMultipliableBothSide(RingMatrix b)
        {
            return this[2] * b[3] != this[3] * b[2];
        }

        public RingBigInteger GetDeterminant()
        {
            return this[1] * this[4] - this[2] * this[3];
        }
    }
}