namespace LexiconLookup.Tests
{
    public class LetterSetTests
    {
        [Fact]
        public void FromString_CreatesLetterSetWithCorrectCounts()
        {
            // Arrange & Act
            LetterSet letterSet = LetterSet.FromString("AAETR");

            // Assert
            Assert.Equal(2, letterSet.GetCount('A'));
            Assert.Equal(1, letterSet.GetCount('E'));
            Assert.Equal(1, letterSet.GetCount('T'));
            Assert.Equal(1, letterSet.GetCount('R'));
            Assert.Equal(0, letterSet.GetCount('Z'));
        }

        [Fact]
        public void FromString_IsCaseInsensitive()
        {
            // Arrange & Act
            LetterSet letterSet = LetterSet.FromString("AaEtR");

            // Assert
            Assert.Equal(2, letterSet.GetCount('A'));
            Assert.Equal(2, letterSet.GetCount('a'));
            Assert.Equal(1, letterSet.GetCount('E'));
            Assert.Equal(1, letterSet.GetCount('e'));
        }

        [Fact]
        public void FromString_HandlesBlankTilesWithQuestionMark()
        {
            // Arrange & Act
            LetterSet letterSet = LetterSet.FromString("AETR??");

            // Assert
            Assert.Equal(1, letterSet.GetCount('A'));
            Assert.Equal(2, letterSet.BlankCount);
        }

        [Fact]
        public void FromString_HandlesBlankTilesWithAsterisk()
        {
            // Arrange & Act
            LetterSet letterSet = LetterSet.FromString("AETR**");

            // Assert
            Assert.Equal(1, letterSet.GetCount('A'));
            Assert.Equal(2, letterSet.BlankCount);
        }

        [Fact]
        public void Constructor_WithDictionary_CreatesLetterSetCorrectly()
        {
            // Arrange
            Dictionary<char, int> counts = new Dictionary<char, int>
            {
                { 'A', 2 },
                { 'E', 1 },
                { 'T', 1 },
            };

            // Act
            LetterSet letterSet = new LetterSet(counts);

            // Assert
            Assert.Equal(2, letterSet.GetCount('A'));
            Assert.Equal(1, letterSet.GetCount('E'));
            Assert.Equal(1, letterSet.GetCount('T'));
        }

        [Fact]
        public void Constructor_WithDictionary_HandlesBlankTiles()
        {
            // Arrange
            Dictionary<char, int> counts = new Dictionary<char, int>
            {
                { 'A', 1 },
                { '?', 2 },
            };

            // Act
            LetterSet letterSet = new LetterSet(counts);

            // Assert
            Assert.Equal(1, letterSet.GetCount('A'));
            Assert.Equal(2, letterSet.BlankCount);
        }

        [Fact]
        public void Constructor_IgnoresNegativeCounts()
        {
            // Arrange
            Dictionary<char, int> counts = new Dictionary<char, int>
            {
                { 'A', 2 },
                { 'E', -1 },
            };

            // Act
            LetterSet letterSet = new LetterSet(counts);

            // Assert
            Assert.Equal(2, letterSet.GetCount('A'));
            Assert.Equal(0, letterSet.GetCount('E'));
        }

        [Fact]
        public void GetCount_ReturnsZeroForMissingLetter()
        {
            // Arrange
            LetterSet letterSet = LetterSet.FromString("AET");

            // Act & Assert
            Assert.Equal(0, letterSet.GetCount('Z'));
        }

        [Fact]
        public void Letters_ReturnsAllLettersInSet()
        {
            // Arrange
            LetterSet letterSet = LetterSet.FromString("AAETR");

            // Act
            HashSet<char> letters = letterSet.Letters.ToHashSet();

            // Assert
            Assert.Equal(4, letters.Count);
            Assert.Contains('A', letters);
            Assert.Contains('E', letters);
            Assert.Contains('T', letters);
            Assert.Contains('R', letters);
        }
    }
}

