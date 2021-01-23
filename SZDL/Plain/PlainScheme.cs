using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using SZDL.Plain;

namespace SZDL.Plain
{
    public class PlainScheme
    {
        public static (PublicKey, SecretKey) GenerateKeysManually(Matrix g, Matrix d, Matrix u, BigInteger numberW, BigInteger numberT, BigInteger numberX)
        {
            Static.P = new BigInteger(11);
            Static.Q = new BigInteger(5);

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
        public static (PublicKey, SecretKey) GenerateKeys()
        {
            //Static.P = Utils.GeneratePrimeNumber();

            //Static.P = new BigInteger(11);
            //Static.Q = new BigInteger(5);

            //Static.P = new BigInteger(29);
            //Static.Q = new BigInteger(7);

            //Static.P = new BigInteger(2267);
            //Static.Q = new BigInteger(103);

            //Static.P = new BigInteger(3187);
            //Static.Q = new BigInteger(59);

            //Static.P = new BigInteger(48731);
            //Static.Q = new BigInteger(443);

            //Static.P = new BigInteger(426389);
            //Static.Q = new BigInteger(67);

            //Static.P = new BigInteger(2147483647);
            //Static.Q = new BigInteger(331);

            //Static.P = BigInteger.Parse("170141183460469231731687303715884105727");
            //Static.Q = BigInteger.Parse("77158673929");
            //Static.Q = 73;


            //Static.P = new BigInteger(274876858367);
            //Static.P = new BigInteger(1125899839733759);
            //Static.P = BigInteger.Parse("1298074214633706835075030044377087");
            //Static.Q = new BigInteger(274876858367);

            (Static.P, Static.Q) = Static.PrimeGenerator.GeneratePair();

            var g = Utils.GenerateNonInvertibleMatrix();
            var gPow = g.Pow(Static.Q + 1);
            var temp = 0;
            
            while (gPow != g)
            {
                var m = (Static.P - 1) / Static.Q;
                
                g = g.Pow(m);
                gPow = g.Pow(Static.Q + 1);
                temp++;
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
            //Static.NumberK = numberK;

            /*Matrix part1 = secretKey.MatrixD.Pow(secretKey.NumberW).Inverse(),
                part2 = secretKey.MatrixG.Pow(numberK),
                part3 = secretKey.MatrixU.Pow(secretKey.NumberT);*/

            var matrixR =
                secretKey.MatrixD.Pow(secretKey.NumberW).Inverse() *
                secretKey.MatrixG.Pow(numberK) *
                secretKey.MatrixU.Pow(secretKey.NumberT);

            //var matrixRModed = new Matrix(matrixR[1].Mod(), matrixR[2].Mod(), matrixR[3].Mod(), matrixR[4].Mod());

            byte[] hash;

            using (SHA512 shaM = new SHA512Managed())
            {
                //var bytes = Encoding.Unicode.GetBytes(message + matrixR.ToByteString());
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
            /*Matrix part1 = publicKey.MatrixY.Pow(
                    publicKey.PrimeNumber
                    //publicKey.Modulus
                    - signature.NumberE),
                part2 = publicKey.MatrixL,
                part3 = publicKey.MatrixZ.Pow(signature.NumberS);*/
            
            var matrixR =
                publicKey.MatrixY.Pow(
                    (publicKey.PrimeNumber
                    //publicKey.Modulus
                    - signature.NumberE).Mod(Static.Q)) *
                publicKey.MatrixL *
                publicKey.MatrixZ.Pow(signature.NumberS);

            var matrixRModed = new Matrix(matrixR[1].Mod(), matrixR[2].Mod(), matrixR[3].Mod(), matrixR[4].Mod());

            byte[] hash;

            using (SHA512 shaM = new SHA512Managed())
            {
                //var bytes = Encoding.Unicode.GetBytes(message + matrixRModed.ToByteString());
                var bytes = Utils.ConcatBytes(Encoding.Unicode.GetBytes(message), matrixRModed.ToBytes());
                hash = shaM.ComputeHash(bytes);
            }

            var numberE = new BigInteger(hash);
            var numberEModed = numberE.Mod();

            return numberEModed.Equals(signature.NumberE);
        }
    }
}