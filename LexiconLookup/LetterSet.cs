namespace LexiconLookup
{
    /// <summary>
    /// Represents a set of letters with their frequencies, supporting blank tiles as wildcards.
    /// </summary>
    public readonly struct LetterSet
    {
        private readonly Dictionary<char, int> _letterCounts;
        private readonly int _blankCount;

        /// <summary>
        /// Creates a LetterSet from a dictionary of letter counts.
        /// </summary>
        /// <param name="letterCounts">Dictionary mapping each letter to its count. Blank tiles represented by '?' or '*'.</param>
        public LetterSet(Dictionary<char, int> letterCounts)
        {
            _letterCounts = new Dictionary<char, int>();
            _blankCount = 0;

            foreach (KeyValuePair<char, int> kvp in letterCounts)
            {
                char letter = char.ToUpperInvariant(kvp.Key);
                
                if (letter == '?' || letter == '*')
                {
                    _blankCount += kvp.Value;
                }
                else if (kvp.Value > 0)
                {
                    if (_letterCounts.ContainsKey(letter))
                    {
                        _letterCounts[letter] += kvp.Value;
                    }
                    else
                    {
                        _letterCounts[letter] = kvp.Value;
                    }
                }
            }
        }

        /// <summary>
        /// Creates a LetterSet from a string where each character appears as many times as available.
        /// </summary>
        /// <param name="letters">String containing letters (e.g., "AAETR" for 2 A's, 1 E, 1 T, 1 R). Use '?' or '*' for blanks.</param>
        /// <returns>A new LetterSet instance.</returns>
        public static LetterSet FromString(string letters)
        {
            Dictionary<char, int> counts = new Dictionary<char, int>();
            
            foreach (char character in letters)
            {
                char letter = char.ToUpperInvariant(character);
                
                if (counts.ContainsKey(letter))
                {
                    counts[letter]++;
                }
                else
                {
                    counts[letter] = 1;
                }
            }
            
            return new LetterSet(counts);
        }

        /// <summary>
        /// Gets the count of a specific letter in this set.
        /// </summary>
        /// <param name="letter">The letter to query (case-insensitive).</param>
        /// <returns>The count of the letter, or 0 if not present.</returns>
        public int GetCount(char letter)
        {
            char upperLetter = char.ToUpperInvariant(letter);
            return _letterCounts.TryGetValue(upperLetter, out int count) ? count : 0;
        }

        /// <summary>
        /// Gets the count of blank tiles (wildcards) in this set.
        /// </summary>
        public int BlankCount => _blankCount;

        /// <summary>
        /// Gets all letters in this set.
        /// </summary>
        public IEnumerable<char> Letters => _letterCounts.Keys;

        /// <summary>
        /// Creates a copy of the letter counts for internal use.
        /// </summary>
        internal Dictionary<char, int> GetLetterCountsCopy()
        {
            return new Dictionary<char, int>(_letterCounts);
        }

        /// <summary>
        /// Creates a copy of the letter counts for external use.
        /// </summary>
        public Dictionary<char, int> GetLetterCounts()
        {
            return new Dictionary<char, int>(_letterCounts);
        }
    }
}

