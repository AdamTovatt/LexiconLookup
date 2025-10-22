namespace LexiconLookup
{
    /// <summary>
    /// Interface for a lexicon that can find words from a dictionary based on available letters.
    /// </summary>
    public interface ILexicon
    {
        /// <summary>
        /// Initializes the lexicon by loading words from a stream.
        /// </summary>
        /// <param name="dictionaryStream">Stream containing words separated by line breaks (one word per line).</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task InitializeAsync(Stream dictionaryStream);

        /// <summary>
        /// Finds all words from the dictionary that can be formed using the available letters.
        /// </summary>
        /// <param name="letters">The set of available letters with their counts.</param>
        /// <returns>A read-only list of all valid words that can be formed.</returns>
        IReadOnlyList<string> FindWords(LetterSet letters);

        /// <summary>
        /// Checks whether a specific word exists in the dictionary.
        /// </summary>
        /// <param name="word">The word to check. Case-insensitive.</param>
        /// <returns>True if the word exists in the dictionary; otherwise, false.</returns>
        bool ContainsWord(string word);
    }
}

