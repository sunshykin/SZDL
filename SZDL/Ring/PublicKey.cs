using System.Numerics;

namespace SZDL.Ring
{
    public class PublicKey
    {
        public RingBigInteger Modulus { get; set; }
        public RingBigInteger PrimeNumber { get; set; }
        public RingMatrix MatrixY { get; set; }
        public RingMatrix MatrixZ { get; set; }
        public RingMatrix MatrixL { get; set; }
    }
}