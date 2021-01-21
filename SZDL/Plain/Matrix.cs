using System;
using System.Linq;
using System.Numerics;
using System.Text;
using SZDL.Plain;

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
    /*public struct Matrix
    {
        private readonly BigInteger element1;
        private readonly BigInteger element2;
        private readonly BigInteger element3;
        private readonly BigInteger element4;

        public Matrix(BigInteger elem1, BigInteger elem2, BigInteger elem3, BigInteger elem4)
        {
            element1 = elem1;
            element2 = elem2;
            element3 = elem3;
            element4 = elem4;
        }*/

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
                (a[1].MultiplyMod(b[1]) + a[2].MultiplyMod(b[3])) % Static.P,
                (a[1].MultiplyMod(b[2]) + a[2].MultiplyMod(b[4])) % Static.P,
                (a[3].MultiplyMod(b[1]) + a[4].MultiplyMod(b[3])) % Static.P,
                (a[3].MultiplyMod(b[2]) + a[4].MultiplyMod(b[4])) % Static.P
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

        public string ToByteString()
        {
            var builder = new StringBuilder();
            builder.AppendJoin(null,
                String.Join(null, this[1].ToByteArray().Select(bt => $"{bt:X2}")),
                String.Join(null, this[2].ToByteArray().Select(bt => $"{bt:X2}")),
                String.Join(null, this[3].ToByteArray().Select(bt => $"{bt:X2}")),
                String.Join(null, this[4].ToByteArray().Select(bt => $"{bt:X2}"))
            );

            return builder.ToString();
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
            return (this[1].MultiplyMod(this[4]) - this[2].MultiplyMod(this[3])).Mod();
        }

        public Matrix Pow(BigInteger exponent)
        {
            if (exponent < 0)
            {
                exponent = exponent.Mod();
                //return this.Inverse().Pow(-exponent);
            }
            
            if (exponent.IsZero)
                return new Matrix(1, 0, 0, 1);

            if (exponent % 2 == 1)
                return this * this.Pow(exponent - 1);

            var square = this.Pow(exponent / 2);

            return square * square;
        }

        /*public Matrix Pow(BigInteger exponent)
        {
            var internatExp = exponent;
            if (internatExp < 0)
            {
                internatExp = exponent.Mod();
                //return this.Inverse().Pow(-exponent);
            }

            return this.InternalPow(ref internatExp);
        }
        
        public Matrix InternalPow(ref BigInteger exponent)
        {
            if (exponent.IsZero)
                return new Matrix(1, 0, 0, 1);

            if (exponent % 2 == 1)
            {
                exponent -= 1;
                return this * this.InternalPow(ref exponent);
            }
            else
            {
                exponent /= 2;
                var square = this.InternalPow(ref exponent);

                return square * square;
            }
        }*/

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

        public bool IsUnit()
        {
            //return this[1] == 1 && this[2] == 0 && this[3] == 0 && this[4] == 1;
            return this[1] == this[4] && this[2] == 0 && this[3] == 0 && this[1] != 0;
        }
    }
}