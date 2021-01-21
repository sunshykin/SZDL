using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace SZDL.Ring
{
    public class RingScheme
    {
        public static (PublicKey, SecretKey) GenerateKeys()
        {
            //Static.P = Utils.GenerateNumber();
            Static.P = new BigInteger(11);
            //Static.P = new BigInteger(3187);
            //Static.P = new BigInteger(274876858367);
            //Static.P = new BigInteger(1125899839733759);
            //Static.P = BigInteger.Parse("1298074214633706835075030044377087");
            Static.Q = new RingBigInteger(5);
            //Static.Q = new RingBigInteger(274876858367);

            var g = Utils.GenerateNonInvertibleMatrix();

            var d = Utils.GenerateMatrix();
            while (!d.IsInvertible() || !d.IsNonMultipliableBothSide(g))
            {
                d = Utils.GenerateMatrix();
            }

            var u = Utils.GenerateMatrix();
            while (!u.IsInvertible() || !u.IsNonMultipliableBothSide(g) || !u.IsNonMultipliableBothSide(d))
            {
                u = Utils.GenerateMatrix();
            }

            var numberW = Utils.GenerateNumber();
            var numberT = Utils.GenerateNumber();
            var numberX = Utils.GenerateNumber();

            var e = Utils.GenerateRightUnit(g);

            var dPowW = d ^ numberW;
            var dPowWInversed = !(d ^ numberW);
            var uPowT = u ^ numberT;



            var l = dPowWInversed * e * uPowT;
            var y = dPowWInversed * (g ^ numberX) * dPowW;
            var z = !uPowT * g * uPowT;

            return (new PublicKey
            {
                Modulus = Static.P,
                PrimeNumber = Static.Q,
                MatrixL = l,
                MatrixY = y,
                MatrixZ = z
            }, new SecretKey
            {
                MatrixD = d,
                MatrixG = g,
                MatrixU = u,
                NumberW = numberW,
                NumberX = numberX,
                NumberT = numberT
            });
        }

        public static Signature Sign(SecretKey secretKey, string message)
        {
            var numberK = Utils.GenerateNumber();
            var matrixR =
                !(secretKey.MatrixD ^ secretKey.NumberW) *
                (secretKey.MatrixG ^ numberK) *
                (secretKey.MatrixU ^ secretKey.NumberT);

            byte[] hash;

            using (SHA512 shaM = new SHA512Managed())
            {
                var bytes = Encoding.Unicode.GetBytes(message + matrixR.ToByteString());
                hash = shaM.ComputeHash(bytes);
            }

            var numberE = new RingBigInteger(hash);
            var numberS = numberK + numberE * secretKey.NumberX;

            return new Signature { NumberE = numberE, NumberS = numberS };
        }

        public static bool Verify(PublicKey publicKey, Signature signature, string message)
        {
            var matrixR =
                (publicKey.MatrixY ^ (publicKey.PrimeNumber - signature.NumberE)) *
                publicKey.MatrixL *
                (publicKey.MatrixZ ^ signature.NumberS);

            byte[] hash;

            using (SHA512 shaM = new SHA512Managed())
            {
                var bytes = Encoding.Unicode.GetBytes(message + matrixR.ToByteString());
                hash = shaM.ComputeHash(bytes);
            }

            var numberE = new RingBigInteger(hash);

            return numberE == signature.NumberE;
        }
    }
}