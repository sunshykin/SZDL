using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Unicode;
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
            var useBenchmark = false;

            if (useBenchmark)
            {
                var summary = BenchmarkRunner.Run<TestCases>();
            }
            else
            {

                /*Plain.Static.P = 11;
                var g = new Matrix(2, 7, 6, 10);
                var d = new Matrix(5, 10, 4, 10);
                var u = new Matrix(8, 10, 8, 9);
                var dInv = d.Inverse();


                var left = (d * g * dInv).Pow(3);
                var right = d * (g.Pow(3)) * dInv;

                var (publicKey0, secretKey0) = PlainScheme.GenerateKeysManually(g, d, u, 4, 9, 10);
                var sign0 = PlainScheme.Sign(secretKey0, "Hello");

                Console.WriteLine(PlainScheme.Verify(publicKey0, sign0, "Hello"));


                var test = 1;
                */
                int falseCounter = 0, trueCounter = 0;
                for (int i = 0; i < 100; i++)
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


                /*var (publicKey, secretKey) = RingScheme.GenerateKeys();
                var sign = RingScheme.Sign(secretKey, "Hello");

                Console.WriteLine(RingScheme.Verify(publicKey, sign, "Hello"));
                */
                //Console.ReadKey();
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

        //[Benchmark]
        public void UsePlain()
        {
            var (publicKey, secretKey) = PlainScheme.GenerateKeys();
            var sign = PlainScheme.Sign(secretKey, "Hello");

            PlainScheme.Verify(publicKey, sign, "Hello");
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
