using System.Numerics;

namespace SZDL.Plain
{
    public class SecretKey
    {
        public Matrix MatrixG { get; set; }
        public Matrix MatrixD { get; set; }
        public Matrix MatrixU { get; set; }
        
        public BigInteger NumberW { get; set; }
        public BigInteger NumberX { get; set; }
        public BigInteger NumberT { get; set; }
    }
}