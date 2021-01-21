using System.Numerics;

namespace SZDL.Plain
{
    public class PublicKey
    {
        public BigInteger Modulus { get; set; }
        public BigInteger PrimeNumber { get; set; }
        public Matrix MatrixY { get; set; }
        public Matrix MatrixZ { get; set; }
        public Matrix MatrixL { get; set; }
    }
}