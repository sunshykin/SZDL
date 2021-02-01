using System;
using System.Numerics;

namespace SZDL.Plain
{
    public class Matrix
    {
        private readonly BigInteger[,] internalMatrix;

        public Matrix()
        {
            internalMatrix = new BigInteger[2, 2];
        }

        public Matrix(BigInteger elem1, BigInteger elem2, BigInteger elem3, BigInteger elem4) : this()
        {
            internalMatrix[0, 0] = elem1;
            internalMatrix[0, 1] = elem2;
            internalMatrix[1, 0] = elem3;
            internalMatrix[1, 1] = elem4;
        }

        public BigInteger this[int i]
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

        public static Matrix operator*(Matrix a, Matrix b)
        {
            return new Matrix(
                (a[1] * b[1] + a[2]* b[3]) % Static.P,
                (a[1] * b[2] + a[2]* b[4]) % Static.P,
                (a[3] * b[1] + a[4]* b[3]) % Static.P,
                (a[3] * b[2] + a[4]* b[4]) % Static.P
            );
        }

        public static bool operator==(Matrix a, Matrix b)
        {
            return a[1] == b[1] &&
                   a[2] == b[2] &&
                   a[3] == b[3] &&
                   a[4] == b[4];
        }

        public static bool operator!=(Matrix a, Matrix b)
        {
            return !(a == b);
        }

        public override string ToString()
        {
            return $"({this[1]}, {this[2]}), ({this[3]}, {this[4]})";
        }

        public byte[] ToBytes()
        {
            var bytes1 = this[1].ToByteArray();
            var bytes2 = this[2].ToByteArray();
            var bytes3 = this[3].ToByteArray();
            var bytes4 = this[4].ToByteArray();
            var length = bytes1.Length + bytes2.Length + bytes3.Length + bytes4.Length;


            var result = new byte[length];

            bytes1.CopyTo(result, 0);
            bytes2.CopyTo(result, bytes1.Length);
            bytes3.CopyTo(result, bytes1.Length + bytes2.Length);
            bytes4.CopyTo(result, bytes1.Length + bytes2.Length + bytes3.Length);

            return result;
        }

        public bool IsInvertible()
        {
            return this[1].MultiplyMod(this[4]) != this[2].MultiplyMod(this[3]);
        }

        public bool IsNonMultipliableBothSide(Matrix b)
        {
            return this[2].MultiplyMod(b[3]) != this[3].MultiplyMod(b[2]);
        }

        public BigInteger GetDeterminant()
        {
            return (this[1] * this[4] - this[2] * this[3]) % Static.P;
        }

        public Matrix Pow(BigInteger exponent)
        {
            if (exponent < 0)
            {
                exponent = exponent.Mod();
            }

            if (exponent.IsOne)
                return this;

            if (!exponent.IsEven)
                return this * this.Pow(exponent - 1);

            var square = this.Pow(exponent / 2);

            return square * square;
        }

        public Matrix Inverse()
        {
            var det = GetDeterminant();
            var detInverse = det.ModInverse(Static.P);

            return new Matrix(
                this[4].MultiplyMod(detInverse),
                this[2].Negate().MultiplyMod(detInverse),
                this[3].Negate().MultiplyMod(detInverse),
                this[1].MultiplyMod(detInverse)
            );
        }
    }
}