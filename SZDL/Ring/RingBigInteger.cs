using System.Numerics;

namespace SZDL.Ring
{
    public struct RingBigInteger
    {
        public bool Equals(RingBigInteger other)
        {
            return Value.Equals(other.Value);
        }

        public override bool Equals(object obj)
        {
            return obj is RingBigInteger other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public BigInteger Value { get; private set; }

        public RingBigInteger(BigInteger bigInt)
        {
            if (Static.P.IsZero)
            {
                Value = bigInt;
            }
            else
            {
                if (bigInt < 0)
                {
                    Value = bigInt + Static.P;
                }
                else if (bigInt >= Static.P)
                {
                    Value = bigInt % Static.P;
                }
                else
                {
                    Value = bigInt;
                }
            }
        }
        
        public RingBigInteger(int number) : this((BigInteger)number)
        {
        }
        
        public RingBigInteger(byte[] bytes) : this(new BigInteger(bytes))
        {
        }

        public static implicit operator RingBigInteger(BigInteger number) => new RingBigInteger(number);
        public static implicit operator RingBigInteger(int number) => new RingBigInteger(number);

        
        public static RingBigInteger operator *(RingBigInteger a, RingBigInteger b)
        {
            return a.Value * b.Value;
        }
        
        public static RingBigInteger operator +(RingBigInteger a, RingBigInteger b)
        {
            return a.Value + b.Value;
        }
        
        public static RingBigInteger operator -(RingBigInteger a)
        {
            return -a.Value;
        }
        
        public static RingBigInteger operator -(RingBigInteger a, RingBigInteger b)
        {
            return a + (-b);
        }
        
        public static bool operator ==(RingBigInteger a, RingBigInteger b)
        {
            return a.Value == b.Value;
        }
        
        public static bool operator !=(RingBigInteger a, RingBigInteger b)
        {
            return a.Value != b.Value;
        }

        public static bool operator >(RingBigInteger a, RingBigInteger b)
        {
            return a.Value > b.Value;
        }

        public static bool operator <(RingBigInteger a, RingBigInteger b)
        {
            return a.Value < b.Value;
        }

        public static RingBigInteger operator !(RingBigInteger a)
        {
            if (Static.P.IsOne)
                return 0;

            BigInteger modulus = Static.P;

            (RingBigInteger x, RingBigInteger y) = (1, 0);

            while (a > 1)
            {
                BigInteger q = a.Value / modulus;
                (a, modulus) = (modulus, a.Value % modulus);
                (x, y) = (y, x - q * y);
            }

            return x;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}