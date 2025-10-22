# LexiconLookup

A high-performance C# library for finding words from a dictionary that can be formed using available letters (like Scrabble tiles).

## Quick Start

```csharp
// Create lexicon from a text file (one word per line)
ILexicon lexicon = new Lexicon();
using (FileStream stream = File.OpenRead("dictionary.txt"))
{
    await lexicon.InitializeAsync(stream);
}

// Find words using available letters
LetterSet letters = LetterSet.FromString("AETR"); // 1 of each letter
IReadOnlyList<string> words = lexicon.FindWords(letters);
// Returns: ["ART", "EAT", "RATE", "TEA", "TAR", "RAT"]

// Support for blank tiles (wildcards)
LetterSet lettersWithBlanks = LetterSet.FromString("AT?"); // A, T, and one blank
IReadOnlyList<string> wordsWithBlanks = lexicon.FindWords(lettersWithBlanks);
// Returns: ["CAT", "BAT", "RAT", "HAT", etc.]

// Check if a specific word exists in the dictionary
bool isValid = lexicon.ContainsWord("HELLO"); // true if "HELLO" is in dictionary
bool isInvalid = lexicon.ContainsWord("XYZZZ"); // false if not found
```

## Performance

Tested on Intel i9-9900K with a dictionary of **216,353 words**:

### Initialization
- **Time**: 410ms
- **Memory**: 130MB

### Lookup Performance
| Scenario | Average Time | Lookups/Second |
|----------|-------------|----------------|
| No blank tiles | 0.011ms | **93,269** |
| 1 blank tile | 0.136ms | **7,347** |
| 2 blank tiles | 0.893ms | **1,120** |

The Trie-based data structure provides excellent performance, with over 70,000 lookups per second for exact letter matches and still over 1,000 lookups per second even with multiple blank tiles.
