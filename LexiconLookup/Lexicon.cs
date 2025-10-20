namespace LexiconLookup
{
    /// <summary>
    /// High-performance lexicon implementation using a Trie data structure for optimal word lookup.
    /// </summary>
    public class Lexicon : ILexicon
    {
        private TrieNode _root;
        private bool _initialized;

        public Lexicon()
        {
            _root = new TrieNode();
            _initialized = false;
        }

        /// <inheritdoc/>
        public async Task InitializeAsync(Stream dictionaryStream)
        {
            _root = new TrieNode();
            
            using (StreamReader reader = new StreamReader(dictionaryStream, leaveOpen: true))
            {
                string? line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    string word = line.Trim().ToUpperInvariant();
                    
                    if (string.IsNullOrWhiteSpace(word))
                        continue;
                    
                    InsertWord(word);
                }
            }
            
            _initialized = true;
        }

        /// <inheritdoc/>
        public Task<IReadOnlyList<string>> FindWordsAsync(LetterSet letters)
        {
            if (!_initialized)
                throw new InvalidOperationException("Lexicon must be initialized before finding words.");
            
            List<string> results = new List<string>();
            Dictionary<char, int> availableLetters = letters.GetLetterCountsCopy();
            int availableBlanks = letters.BlankCount;
            
            char[] currentWord = new char[50]; // Maximum reasonable word length
            
            FindWordsRecursive(_root, availableLetters, availableBlanks, currentWord, 0, results);
            
            return Task.FromResult<IReadOnlyList<string>>(results);
        }

        private void InsertWord(string word)
        {
            TrieNode current = _root;
            
            foreach (char character in word)
            {
                if (!current.Children.ContainsKey(character))
                {
                    current.Children[character] = new TrieNode();
                }
                
                current = current.Children[character];
            }
            
            current.IsWordEnd = true;
        }

        private void FindWordsRecursive(
            TrieNode node,
            Dictionary<char, int> availableLetters,
            int availableBlanks,
            char[] currentWord,
            int depth,
            List<string> results)
        {
            // If this node marks the end of a word, add it to results
            if (node.IsWordEnd)
            {
                string word = new string(currentWord, 0, depth);
                results.Add(word);
            }

            // Try each possible child node
            foreach (KeyValuePair<char, TrieNode> child in node.Children)
            {
                char letter = child.Key;
                TrieNode childNode = child.Value;
                
                // Try using an actual letter from available letters
                if (availableLetters.TryGetValue(letter, out int count) && count > 0)
                {
                    // Use the letter
                    availableLetters[letter]--;
                    currentWord[depth] = letter;
                    
                    FindWordsRecursive(childNode, availableLetters, availableBlanks, currentWord, depth + 1, results);
                    
                    // Backtrack
                    availableLetters[letter]++;
                }
                // Try using a blank tile as this letter
                else if (availableBlanks > 0)
                {
                    currentWord[depth] = letter;
                    
                    FindWordsRecursive(childNode, availableLetters, availableBlanks - 1, currentWord, depth + 1, results);
                }
            }
        }
    }
}

