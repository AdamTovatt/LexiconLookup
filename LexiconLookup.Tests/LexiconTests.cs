namespace LexiconLookup.Tests
{
    public class LexiconTests
    {
        private static async Task<ILexicon> CreateLexiconWithWordsAsync(params string[] words)
        {
            string dictionaryContent = string.Join(Environment.NewLine, words);
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(dictionaryContent);
            
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                ILexicon lexicon = new Lexicon();
                await lexicon.InitializeAsync(stream);
                return lexicon;
            }
        }

        [Fact]
        public async Task FindWords_WithSimpleLetters_FindsAllMatchingWords()
        {
            // Arrange
            ILexicon lexicon = await CreateLexiconWithWordsAsync("TEA", "EAT", "ART", "RATE", "TAR", "RAT");
            LetterSet letters = LetterSet.FromString("AETR");

            // Act
            IReadOnlyList<string> results = lexicon.FindWords(letters);

            // Assert
            Assert.Contains("TEA", results);
            Assert.Contains("EAT", results);
            Assert.Contains("ART", results);
            Assert.Contains("RATE", results);
            Assert.Contains("TAR", results);
            Assert.Contains("RAT", results);
            Assert.Equal(6, results.Count);
        }

        [Fact]
        public async Task FindWords_WithNoMatches_ReturnsEmptyList()
        {
            // Arrange
            ILexicon lexicon = await CreateLexiconWithWordsAsync("HELLO", "WORLD", "QUIZ");
            LetterSet letters = LetterSet.FromString("AETR");

            // Act
            IReadOnlyList<string> results = lexicon.FindWords(letters);

            // Assert
            Assert.Empty(results);
        }

        [Fact]
        public async Task FindWords_WithSubsetOfLetters_FindsMatchingWords()
        {
            // Arrange
            ILexicon lexicon = await CreateLexiconWithWordsAsync("TEA", "EAT", "ART", "RATE", "TAR", "AE");
            LetterSet letters = LetterSet.FromString("EA");

            // Act
            IReadOnlyList<string> results = lexicon.FindWords(letters);

            // Assert
            Assert.Contains("AE", results);
            Assert.DoesNotContain("TEA", results); // Needs T
            Assert.DoesNotContain("EAT", results); // Needs T
            Assert.DoesNotContain("ART", results); // Needs R and T
            Assert.DoesNotContain("RATE", results); // Needs R and T
            Assert.DoesNotContain("TAR", results); // Needs R and T
        }

        [Fact]
        public async Task FindWords_WithDuplicateLetters_RespectsLetterCounts()
        {
            // Arrange
            ILexicon lexicon = await CreateLexiconWithWordsAsync("APPLE", "APE", "PALE", "LEAP");
            LetterSet letters = LetterSet.FromString("APEL");

            // Act
            IReadOnlyList<string> results = lexicon.FindWords(letters);

            // Assert
            Assert.Contains("APE", results);
            Assert.Contains("PALE", results);
            Assert.Contains("LEAP", results);
            Assert.DoesNotContain("APPLE", results); // Would need 2 P's
        }

        [Fact]
        public async Task FindWords_WithBlankTile_FindsWordsUsingBlankAsWildcard()
        {
            // Arrange
            ILexicon lexicon = await CreateLexiconWithWordsAsync("CAT", "BAT", "RAT", "HAT");
            LetterSet letters = LetterSet.FromString("AT?");

            // Act
            IReadOnlyList<string> results = lexicon.FindWords(letters);

            // Assert
            Assert.Contains("CAT", results);
            Assert.Contains("BAT", results);
            Assert.Contains("RAT", results);
            Assert.Contains("HAT", results);
            Assert.Equal(4, results.Count);
        }

        [Fact]
        public async Task FindWords_WithMultipleBlanks_UsesAllBlanks()
        {
            // Arrange
            ILexicon lexicon = await CreateLexiconWithWordsAsync("CAT", "CATS", "CAST");
            LetterSet letters = LetterSet.FromString("AT??");

            // Act
            IReadOnlyList<string> results = lexicon.FindWords(letters);

            // Assert
            Assert.Contains("CAT", results);
            Assert.Contains("CATS", results);
            Assert.Contains("CAST", results);
        }

        [Fact]
        public async Task InitializeAsync_WithLowercaseWords_ConvertsToUppercase()
        {
            // Arrange
            ILexicon lexicon = await CreateLexiconWithWordsAsync("tea", "eat", "art");
            LetterSet letters = LetterSet.FromString("AETR");

            // Act
            IReadOnlyList<string> results = lexicon.FindWords(letters);

            // Assert
            Assert.Contains("TEA", results);
            Assert.Contains("EAT", results);
            Assert.Contains("ART", results);
        }

        [Fact]
        public async Task InitializeAsync_WithMixedCaseWords_ConvertsToUppercase()
        {
            // Arrange
            ILexicon lexicon = await CreateLexiconWithWordsAsync("Tea", "EaT", "ArT");
            LetterSet letters = LetterSet.FromString("AETR");

            // Act
            IReadOnlyList<string> results = lexicon.FindWords(letters);

            // Assert
            Assert.Contains("TEA", results);
            Assert.Contains("EAT", results);
            Assert.Contains("ART", results);
        }

        [Fact]
        public async Task InitializeAsync_WithEmptyStream_CreatesEmptyLexicon()
        {
            // Arrange
            ILexicon lexicon = await CreateLexiconWithWordsAsync();
            LetterSet letters = LetterSet.FromString("AETR");

            // Act
            IReadOnlyList<string> results = lexicon.FindWords(letters);

            // Assert
            Assert.Empty(results);
        }

        [Fact]
        public async Task FindWordsAsync_WithEmptyLetterSet_ReturnsEmptyList()
        {
            // Arrange
            ILexicon lexicon = await CreateLexiconWithWordsAsync("TEA", "EAT", "ART");
            LetterSet letters = LetterSet.FromString("");

            // Act
            IReadOnlyList<string> results = lexicon.FindWords(letters);

            // Assert
            Assert.Empty(results);
        }

        [Fact]
        public async Task FindWordsAsync_WithSingleLetter_FindsSingleLetterWords()
        {
            // Arrange
            ILexicon lexicon = await CreateLexiconWithWordsAsync("A", "I", "O", "AT", "IT");
            LetterSet letters = LetterSet.FromString("A");

            // Act
            IReadOnlyList<string> results = lexicon.FindWords(letters);

            // Assert
            Assert.Contains("A", results);
            Assert.DoesNotContain("I", results);
            Assert.DoesNotContain("O", results);
            Assert.DoesNotContain("AT", results);
            Assert.Equal(1, results.Count);
        }

        [Fact]
        public async Task InitializeAsync_WithWhitespaceLines_IgnoresEmptyLines()
        {
            // Arrange
            string dictionaryContent = "TEA\n\nEAT\n   \nART";
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(dictionaryContent);
            
            ILexicon lexicon = new Lexicon();
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                await lexicon.InitializeAsync(stream);
            }
            
            LetterSet letters = LetterSet.FromString("AETR");

            // Act
            IReadOnlyList<string> results = lexicon.FindWords(letters);

            // Assert
            Assert.Equal(3, results.Count);
            Assert.Contains("TEA", results);
            Assert.Contains("EAT", results);
            Assert.Contains("ART", results);
        }

        [Fact]
        public async Task ContainsWord_WithValidWord_ReturnsTrue()
        {
            // Arrange
            ILexicon lexicon = await CreateLexiconWithWordsAsync("HELLO", "WORLD", "TEST");

            // Act
            bool result = lexicon.ContainsWord("HELLO");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ContainsWord_WithInvalidWord_ReturnsFalse()
        {
            // Arrange
            ILexicon lexicon = await CreateLexiconWithWordsAsync("HELLO", "WORLD", "TEST");

            // Act
            bool result = lexicon.ContainsWord("GOODBYE");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ContainsWord_WithLowercaseWord_ReturnsTrueCaseInsensitive()
        {
            // Arrange
            ILexicon lexicon = await CreateLexiconWithWordsAsync("HELLO", "WORLD");

            // Act
            bool result = lexicon.ContainsWord("hello");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ContainsWord_WithMixedCaseWord_ReturnsTrueCaseInsensitive()
        {
            // Arrange
            ILexicon lexicon = await CreateLexiconWithWordsAsync("HELLO", "WORLD");

            // Act
            bool result = lexicon.ContainsWord("HeLLo");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ContainsWord_WithEmptyString_ReturnsFalse()
        {
            // Arrange
            ILexicon lexicon = await CreateLexiconWithWordsAsync("HELLO", "WORLD");

            // Act
            bool result = lexicon.ContainsWord("");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ContainsWord_WithNullString_ReturnsFalse()
        {
            // Arrange
            ILexicon lexicon = await CreateLexiconWithWordsAsync("HELLO", "WORLD");

            // Act
            bool result = lexicon.ContainsWord(null!);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ContainsWord_BeforeInitialization_ThrowsInvalidOperationException()
        {
            // Arrange
            ILexicon lexicon = new Lexicon();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => lexicon.ContainsWord("HELLO"));
        }

        [Fact]
        public async Task ContainsWord_WithPartialMatch_ReturnsFalse()
        {
            // Arrange
            ILexicon lexicon = await CreateLexiconWithWordsAsync("HELLO", "WORLD", "TESTING");

            // Act
            bool resultPrefix = lexicon.ContainsWord("HEL");
            bool resultSuffix = lexicon.ContainsWord("LLO");

            // Assert
            Assert.False(resultPrefix);
            Assert.False(resultSuffix);
        }

        [Fact]
        public async Task ContainsWord_WithSingleLetterWord_ReturnsTrue()
        {
            // Arrange
            ILexicon lexicon = await CreateLexiconWithWordsAsync("A", "I", "O");

            // Act
            bool result = lexicon.ContainsWord("A");

            // Assert
            Assert.True(result);
        }
    }
}

