using System;
using System.Collections.Generic;
using System.Diagnostics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.VisualBasic.CompilerServices;

namespace Dilithium
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
                var bytes = new byte[] { 1, 191 };

                Scheme.SetUpMode(3);
                var (pk, sk) = Scheme.GenerateKeys();
                var sign = Scheme.Sign(sk, "Hello");

                var verify = Scheme.Verify(pk, sign, "Hello");

                Console.WriteLine(verify);
            }
        }
    }

    public class TestCases
    {
        [GlobalSetup]
        public void GlobalSetup()
        {
            Scheme.SetUpMode(4);
        }
        
        [Benchmark]
        public void DilithiumGenerateKeys()
        {
            Scheme.GenerateKeys();
        }

        public IEnumerable<object> SignArgs()
        {
            Scheme.SetUpMode(4);
            yield return Scheme.GenerateKeys().Item2;
        }

        [Benchmark]
        [ArgumentsSource(nameof(SignArgs))]
        public void DilithiumSign(SecretKey secretKey)
        {
            Scheme.Sign(secretKey, "Hello");
        }

        public IEnumerable<object[]> VerifyArgs()
        {
            Scheme.SetUpMode(4);
            var keys = Scheme.GenerateKeys();

            yield return new object[] { keys.Item1, Scheme.Sign(keys.Item2, "Hello") };
        }

        [Benchmark]
        [ArgumentsSource(nameof(VerifyArgs))]
        public void DilithiumVerify(PublicKey publicKey, Signature signature)
        {
            Scheme.Verify(publicKey, signature, "Hello");
        }
    }
}
