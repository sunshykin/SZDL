using System.Numerics;

namespace SZDL.Ring
{
    public class SecretKey
    {
        public RingMatrix MatrixG { get; set; }
        public RingMatrix MatrixD { get; set; }
        public RingMatrix MatrixU { get; set; }
        
        public RingBigInteger NumberW { get; set; }
        public RingBigInteger NumberX { get; set; }
        public RingBigInteger NumberT { get; set; }
    }
}