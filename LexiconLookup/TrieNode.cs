namespace LexiconLookup
{
    /// <summary>
    /// Represents a node in a Trie (prefix tree) data structure.
    /// </summary>
    internal class TrieNode
    {
        /// <summary>
        /// Children nodes indexed by character.
        /// </summary>
        public Dictionary<char, TrieNode> Children { get; }

        /// <summary>
        /// Indicates whether this node represents the end of a valid word.
        /// </summary>
        public bool IsWordEnd { get; set; }

        public TrieNode()
        {
            Children = new Dictionary<char, TrieNode>();
            IsWordEnd = false;
        }
    }
}

