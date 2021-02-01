using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace SZDL.Plain
{
    public class PlainScheme
    {
        public static (PublicKey, SecretKey) GenerateKeys()
        {
            (Static.P, Static.Q) = Static.PrimeGenerator.GeneratePair();

            var g = Utils.GenerateNonInvertibleMatrix();
            var gPow = g.Pow(Static.Q + 1);
            
            while (gPow != g)
            {
                var m = (Static.P - 1) / Static.Q;
                
                g = g.Pow(m);
                gPow = g.Pow(Static.Q + 1);
            }


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
            var numberX = Utils.GenerateNumber().Mod(Static.Q - 2) + 2; // Crete 1 < x < q

            var e = Utils.GenerateRightUnit(g);

            var dPowW = d.Pow(numberW);
            var dPowWInversed = dPowW.Inverse();
            var uPowT = u.Pow(numberT);
            var uPowTInversed = uPowT.Inverse();



            var l = dPowWInversed * e * uPowT;
            var y = dPowWInversed * g.Pow(numberX) * dPowW;
            var z = uPowTInversed * g * uPowT;

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
            var numberK = Utils.GenerateNumber().Mod(Static.Q - 2) + 2; // Crete 1 < k < q

            var matrixR =
                secretKey.MatrixD.Pow(secretKey.NumberW).Inverse() *
                secretKey.MatrixG.Pow(numberK) *
                secretKey.MatrixU.Pow(secretKey.NumberT);

            byte[] hash;

            using (SHA512 shaM = new SHA512Managed())
            {
                var bytes = Utils.ConcatBytes(Encoding.Unicode.GetBytes(message), matrixR.ToBytes());
                hash = shaM.ComputeHash(bytes);
            }

            var numberE = new BigInteger(hash);
            var numberEModed = numberE.Mod();
            Static.NumberE = numberEModed;
            var numberS = (numberK + numberEModed * secretKey.NumberX).Mod(Static.Q);

            return new Signature { NumberE = numberEModed, NumberS = numberS };
        }

        public static bool Verify(PublicKey publicKey, Signature signature, string message)
        {
            var matrixR =
                publicKey.MatrixY.Pow(
                    (publicKey.PrimeNumber
                    - signature.NumberE).Mod(Static.Q)) *
                publicKey.MatrixL *
                publicKey.MatrixZ.Pow(signature.NumberS);

            var matrixRModed = new Matrix(matrixR[1].Mod(), matrixR[2].Mod(), matrixR[3].Mod(), matrixR[4].Mod());

            byte[] hash;

            using (SHA512 shaM = new SHA512Managed())
            {
                var bytes = Utils.ConcatBytes(Encoding.Unicode.GetBytes(message), matrixRModed.ToBytes());
                hash = shaM.ComputeHash(bytes);
            }

            var numberE = new BigInteger(hash);
            var numberEModed = numberE.Mod();

            return numberEModed.Equals(signature.NumberE);
        }
    }
}