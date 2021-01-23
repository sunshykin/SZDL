using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SZDL.Plain
{
	public class PrimeGenerator
	{
		private const string StoreName = "appstore.json";

		private Dictionary<int, List<BigInteger>> dataStore = new Dictionary<int, List<BigInteger>>();

		private List<BigInteger> primesList;

		private readonly PrimeGeneratorSettings settings;

		public PrimeGenerator(PrimeGeneratorSettings settings)
		{
			this.settings = settings;

			Initialize();
		}

		private void Initialize()
		{
			// Checking if data store exists
			var fileName = Path.Combine(Environment.CurrentDirectory, StoreName);

			if (File.Exists(fileName))
			{
				// If it does - restore it
				dataStore = JsonConvert.DeserializeObject<Dictionary<int, List<BigInteger>>>(File.ReadAllText(fileName));
			}

			if (dataStore.ContainsKey(settings.KeyLengthNumber))
			{
				primesList = dataStore[settings.KeyLengthNumber];
			}
			else
			{
				primesList = new List<BigInteger>();
				dataStore.TryAdd(settings.KeyLengthNumber, primesList);
			}

			// If we have no primes for current bits-key find them
			if (primesList.Count == 0 || settings.IgnoreStoredData)
			{
				FillPrimes();

				// Save the store
				File.WriteAllText(fileName, JsonConvert.SerializeObject(dataStore));
			}

		}

		private void FillPrimes()
		{
			var sw = Stopwatch.StartNew();
			// Getting half of the key length required
			var halfOfTheKey = settings.KeyLengthNumber / 2;


			// Getting the first and last checking numbers
			BigInteger firstNumber = Utils.GetMinNumberExactBitLength(halfOfTheKey),
				lastNumber = Utils.GetMinNumberExactBitLength(halfOfTheKey + 1);

			if (settings.UseParallel)
			{
				var partSize = (lastNumber - firstNumber) / settings.ParallelTaskCount;

				var taskArray = new Task[settings.ParallelTaskCount];
				for (int i = 0; i < settings.ParallelTaskCount; i++)
				{
					// Closure
					var counter = i;

					taskArray[i] = Task.Run(() => DoFindPrimeNumbers(firstNumber + counter * partSize, firstNumber + (counter + 1) * partSize));
				}

				Task.WaitAll(taskArray);
			}
			else
			{
				DoFindPrimeNumbers(firstNumber, lastNumber);
			}

			sw.Stop();
			Console.WriteLine("Primes filled in. Time elapsed {0}", sw.Elapsed);
		}

		private void DoFindPrimeNumbers(BigInteger from, BigInteger to)
		{
			for (var i = from; i < to; i++)
			{
				if (Utils.MillerRabinTest(i))
					primesList.Add(i);

				if (primesList.Count >= 15000)
					break;
			}
		}

		public (BigInteger P, BigInteger Q) GeneratePair()
		{
			var randomGen = new Random(Environment.TickCount);
			var iterationCounter = 0;

			while (iterationCounter < settings.MaxIteration)
			{
				// Select prime randomly
				var prime = primesList.ElementAt(randomGen.Next(0, primesList.Count - 1));

				var checkingNumber = prime;

				foreach (var num in settings.NumbersInFactorization)
				{
					checkingNumber *= num;
				}

				if (settings.NumbersInFactorization.All(num => num != 2))
				{
					checkingNumber *= 2;
				}

				var accumulatingValue = settings.AccumulatingValue == BigInteger.Zero
					? new BigInteger(2)
					: settings.AccumulatingValue;

				do
				{
					checkingNumber *= accumulatingValue;
				} while (checkingNumber.GetBitLength() < settings.KeyLengthNumber);

				if (Utils.MillerRabinTest(checkingNumber + 1))
				{
					if (settings.ShowIterations)
						Console.WriteLine("Iteration #{0} | Prime is {1}", iterationCounter, checkingNumber + 1);
					return (checkingNumber + 1, prime);
				}

				if (settings.ShowIterations)
					Console.WriteLine("Iteration #{0} | Prime not found", iterationCounter);
				iterationCounter++;
			}

			return (0, 0);
		}
	}
}
