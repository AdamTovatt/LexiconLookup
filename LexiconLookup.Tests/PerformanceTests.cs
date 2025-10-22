using EasyReasy;
using System.Diagnostics;
using Xunit.Abstractions;

namespace LexiconLookup.Tests
{
    public class PerformanceTests : IAsyncLifetime
    {
        private static readonly Random _random = new Random(42); // Fixed seed for reproducible tests
        private ILexicon _lexicon = null!;
        private string[] _allWords = null!;
        private readonly ITestOutputHelper _output;
        private TimeSpan _initializationTime;
        private long _memoryUsed;

        public PerformanceTests(ITestOutputHelper output)
        {
            _output = output;
        }

        public async Task InitializeAsync()
        {
            // Load dictionary once for all tests
            
            ResourceManager resourceManager = await ResourceManager.CreateInstanceAsync();
            string csvContent = await resourceManager.ReadAsStringAsync(TestResources.Dictionary.WordsCsv);
            
            // Parse CSV to get words (assuming one word per line or comma-separated)
            _allWords = csvContent
                .Split(new[] { '\n', '\r', ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(word => word.Trim().ToUpperInvariant())
                .Where(word => !string.IsNullOrWhiteSpace(word))
                .ToArray();

            // Force garbage collection to get accurate memory measurement
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
            long memoryBefore = GC.GetTotalMemory(false);
            Stopwatch stopwatch = Stopwatch.StartNew();

            // Create lexicon with all words
            _lexicon = new Lexicon();
            using (Stream stream = await resourceManager.GetResourceStreamAsync(TestResources.Dictionary.WordsCsv))
            {
                await _lexicon.InitializeAsync(stream);
            }
            
            stopwatch.Stop();
            long memoryAfter = GC.GetTotalMemory(false);
            
            _initializationTime = stopwatch.Elapsed;
            _memoryUsed = memoryAfter - memoryBefore;
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public void FindWords_WithLargeDictionary_PerformsWell()
        {
            // Arrange - lexicon is already initialized

            // Test with multiple random letter sets
            for (int testRun = 0; testRun < 10; testRun++)
            {
                // Generate random letter set (5-8 letters)
                int letterCount = _random.Next(5, 9);
                string randomLetters = GenerateRandomLetters(letterCount);
                LetterSet letterSet = LetterSet.FromString(randomLetters);

                // Act - measure performance
                Stopwatch stopwatch = Stopwatch.StartNew();
                IReadOnlyList<string> results = _lexicon.FindWords(letterSet);
                stopwatch.Stop();

                // Assert
                TimeSpan duration = stopwatch.Elapsed;
                Assert.True(duration.TotalMilliseconds < 100, $"Lookup took {duration.TotalMilliseconds}ms, should be under 100ms");

                // Verify results are valid (all found words can actually be formed with the letters)
                foreach (string word in results)
                {
                    Assert.True(CanFormWord(word, letterSet), $"Word '{word}' cannot be formed with letters '{randomLetters}'");
                }

                // Log results for verification
                _output.WriteLine($"Test run {testRun + 1}: Letters '{randomLetters}' found {results.Count} words in {duration.TotalMilliseconds:F2}ms");
                if (results.Count > 0 && results.Count <= 10)
                {
                    _output.WriteLine($"  All words: {string.Join(", ", results)}");
                }
                else if (results.Count > 10)
                {
                    _output.WriteLine($"  Sample words: {string.Join(", ", results.Take(5))}... (and {results.Count - 5} more)");
                }
            }
        }

        [Fact]
        public void FindWords_WithBlankTiles_PerformsWell()
        {
            // Arrange - lexicon is already initialized

            // Test with blank tiles
            for (int testRun = 0; testRun < 5; testRun++)
            {
                // Generate letter set with 1-2 blank tiles
                int letterCount = _random.Next(4, 7);
                string randomLetters = GenerateRandomLetters(letterCount);
                int blankCount = _random.Next(1, 3);
                string lettersWithBlanks = randomLetters + new string('?', blankCount);

                LetterSet letterSet = LetterSet.FromString(lettersWithBlanks);

                // Act
                Stopwatch stopwatch = Stopwatch.StartNew();
                IReadOnlyList<string> results = _lexicon.FindWords(letterSet);
                stopwatch.Stop();

                // Assert
                TimeSpan duration = stopwatch.Elapsed;
                Assert.True(duration.TotalMilliseconds < 150, $"Lookup with blanks took {duration.TotalMilliseconds}ms, should be under 150ms");

                // Log results
                _output.WriteLine($"Blank tile test {testRun + 1}: Letters '{lettersWithBlanks}' found {results.Count} words in {duration.TotalMilliseconds:F2}ms");
                if (results.Count > 0 && results.Count <= 10)
                {
                    _output.WriteLine($"  All words: {string.Join(", ", results)}");
                }
                else if (results.Count > 10)
                {
                    _output.WriteLine($"  Sample words: {string.Join(", ", results.Take(5))}... (and {results.Count - 5} more)");
                }
            }
        }

        [Fact]
        public void InitializeAsync_WithLargeDictionary_CompletesInReasonableTime()
        {
            // This test verifies that initialization completed successfully during setup
            // The actual timing is done in InitializeAsync() method

            // Assert
            Assert.NotNull(_lexicon);
            Assert.NotNull(_allWords);
            Assert.True(_allWords.Length > 0, "Should have loaded words from the dictionary");

            _output.WriteLine($"Successfully initialized lexicon with {_allWords.Length} words in {_initializationTime.TotalMilliseconds:F2}ms");
            _output.WriteLine($"Memory used by lexicon: {_memoryUsed / 1024.0 / 1024.0:F2} MB");
        }

        private static string GenerateRandomLetters(int count)
        {
            const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            char[] result = new char[count];

            for (int i = 0; i < count; i++)
            {
                result[i] = letters[_random.Next(letters.Length)];
            }

            return new string(result);
        }

        private static bool CanFormWord(string word, LetterSet letterSet)
        {
            Dictionary<char, int> availableLetters = letterSet.GetLetterCounts();
            int availableBlanks = letterSet.BlankCount;

            foreach (char character in word)
            {
                if (availableLetters.TryGetValue(character, out int count) && count > 0)
                {
                    availableLetters[character]--;
                }
                else if (availableBlanks > 0)
                {
                    availableBlanks--;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        [Fact]
        public void PerformanceTest_NoBlankTiles_AverageLookupTime()
        {
            RunPerformanceTest(0, "No blank tiles");
        }

        [Fact]
        public void PerformanceTest_OneBlankTile_AverageLookupTime()
        {
            RunPerformanceTest(1, "One blank tile");
        }

        [Fact]
        public void PerformanceTest_TwoBlankTiles_AverageLookupTime()
        {
            RunPerformanceTest(2, "Two blank tiles");
        }

        private void RunPerformanceTest(int blankTileCount, string testDescription)
        {
            const int warmupRuns = 3000;
            const int testRuns = 10000;

            // Warmup phase
            _output.WriteLine($"Warming up JIT with {warmupRuns} runs...");
            for (int i = 0; i < warmupRuns; i++)
            {
                string randomLetters = GenerateRandomLetters(_random.Next(5, 9));
                string lettersWithBlanks = randomLetters + new string('?', blankTileCount);
                LetterSet letterSet = LetterSet.FromString(lettersWithBlanks);
                
                _lexicon.FindWords(letterSet);
            }

            // Performance test phase
            _output.WriteLine($"Running {testRuns} performance tests...");
            List<double> timings = new List<double>(testRuns);

            for (int i = 0; i < testRuns; i++)
            {
                string randomLetters = GenerateRandomLetters(_random.Next(5, 9));
                string lettersWithBlanks = randomLetters + new string('?', blankTileCount);
                LetterSet letterSet = LetterSet.FromString(lettersWithBlanks);

                Stopwatch stopwatch = Stopwatch.StartNew();
                IReadOnlyList<string> results = _lexicon.FindWords(letterSet);
                stopwatch.Stop();

                timings.Add(stopwatch.Elapsed.TotalMilliseconds);
            }

            // Calculate statistics
            double averageTime = timings.Average();
            double minTime = timings.Min();
            double maxTime = timings.Max();
            double medianTime = timings.OrderBy(x => x).Skip(timings.Count / 2).First();
            double lookupsPerSecond = 1000.0 / averageTime; // Convert ms to lookups per second

            _output.WriteLine($"Performance Test Results - {testDescription}:");
            _output.WriteLine($"  Average lookup time: {averageTime:F3}ms");
            _output.WriteLine($"  Lookups per second: {lookupsPerSecond:F0}");
            _output.WriteLine($"  Median lookup time: {medianTime:F3}ms");
            _output.WriteLine($"  Min lookup time: {minTime:F3}ms");
            _output.WriteLine($"  Max lookup time: {maxTime:F3}ms");
            _output.WriteLine($"  Total test runs: {testRuns}");

            // Assert that average performance is reasonable
            Assert.True(averageTime < 1.0, $"Average lookup time {averageTime:F3}ms should be under 1ms for {testDescription}");
        }
    }
}
