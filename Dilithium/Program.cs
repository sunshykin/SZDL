using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace Dilithium
{
    class Program
    {
        static void Main(string[] args)
        {

            var useBenchmark = true;

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
        public static int DilithiumMode = 1;
        
        [GlobalSetup]
        public void GlobalSetup()
        {
            Scheme.SetUpMode(DilithiumMode);
        }
        
        [Benchmark]
        public void DilithiumGenerateKeys()
        {
            Scheme.GenerateKeys();
        }

        public IEnumerable<object> SignArgs()
        {
            Scheme.SetUpMode(DilithiumMode);
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
            Scheme.SetUpMode(DilithiumMode);
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
