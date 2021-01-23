using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Unicode;
using System.Threading;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using SZDL.Plain;
using SZDL.Ring;
using PublicKey = SZDL.Plain.PublicKey;
using SecretKey = SZDL.Plain.SecretKey;
using Signature = SZDL.Plain.Signature;
using Static = SZDL.Ring.Static;
using Utils = SZDL.Plain.Utils;

namespace SZDL
{
    class Program
    {
        static void Main(string[] args)
        {
            var mode = "benchmark";
                       //"sign";
                       //"compare";

            if (mode == "benchmark")
            {
                var summary = BenchmarkRunner.Run<TestCases>();
            }
            else if (mode == "sign")
            {
                int falseCounter = 0, trueCounter = 0;
                for (int i = 0; i < 150; i++)
                {
                    var (publicKey1, secretKey1) = PlainScheme.GenerateKeys();
                    var sign1 = PlainScheme.Sign(secretKey1, "Hello");

                    //Console.WriteLine(PlainScheme.Verify(publicKey1, sign1, "Hello"));
                    var result = PlainScheme.Verify(publicKey1, sign1, "Hello");
                    
                    if (result)
                    {
                        trueCounter++;
                    }
                    else
                    {
                        falseCounter++;
                    }
                }

                Console.WriteLine("True/False: {0}/{1}", trueCounter, falseCounter);
            }
            else if (mode == "compare")
            {
                var num1 = BigInteger.Parse("141562178236123719023182636512735127318231273816231730913");
                var num2 = BigInteger.Parse("9573092321784619475983727739052210384891203183287482102384");
                var modulus = BigInteger.Parse("69457109491271640540324213238132238195765914719571218");
                Plain.Static.P = modulus;
                var matr = Utils.GenerateMatrix();

                for (int i = 0; i < 100000; i++)
                {
                }
            }
        }
    }

    public class TestCases
    {
        //[Benchmark]
        public void UsePrimeNumberWithRandom()
        {
            Utils.GeneratePrimeNumber();
        }

        //[Benchmark]
        public void UsePrimeNumberWithRNG()
        {
            Utils.GeneratePrimeNumberRNG();
        }

        [Benchmark]
        public void UsePlainGenerateKeys()
        {
            PlainScheme.GenerateKeys();
        }

        public IEnumerable<object> SignArgs()
        {
            yield return PlainScheme.GenerateKeys().Item2;
        }

        [Benchmark]
        [ArgumentsSource(nameof(SignArgs))]
        public void UsePlainSign(SecretKey secretKey)
        {
            PlainScheme.Sign(secretKey, "Hello");
        }

        public IEnumerable<object[]> VerifyArgs()
        {
            yield return new object[] { PlainScheme.GenerateKeys().Item1, PlainScheme.Sign(PlainScheme.GenerateKeys().Item2, "Hello") };
        }

        [Benchmark]
        [ArgumentsSource(nameof(VerifyArgs))]
        public void UsePlainVerify(PublicKey publicKey, Signature signature)
        {
            PlainScheme.Verify(publicKey, signature, "Hello");
        }

        public void UseRing()
        {
            var (publicKey, secretKey) = RingScheme.GenerateKeys();
            var sign = RingScheme.Sign(secretKey, "Hello");

            RingScheme.Verify(publicKey, sign, "Hello");
            //Console.WriteLine(RingScheme.Verify(publicKey, sign, "Hello"));
        }
    }
}
