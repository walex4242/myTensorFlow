using System.Text.RegularExpressions;

public partial class TextProcessingService
{
    private readonly HashSet<string> _stopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "the", "a", "an", "is", "are", "was", "were", "be", "being", "been",
        "to", "from", "in", "out", "on", "off", "at", "by", "for", "with",
        "and", "or", "but", "if", "then", "else", "that", "this", "these",
        "those", "it", "its", "he", "him", "his", "she", "her", "hers",
        "they", "them", "their", "theirs", "of", "as", "while", "after",
        "before", "during", "about", "against", "between", "into", "through",
        "above", "below", "under", "over", "again", "further", "then", "once",
        "here", "there", "when", "where", "why", "how", "all", "any", "both",
        "each", "few", "many", "some", "such", "no", "nor", "not", "only",
        "own", "same", "so", "than", "too", "very", "can", "will", "just",
        "should", "do", "does", "did", "had", "has", "have", "may", "might",
        "must", "need", "shall", "would", "could"
        // Add more stop words as needed
    };

    [GeneratedRegex(@"[^a-z0-9\s]")]
    private static partial Regex MyRegex();

    public string PreprocessText(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        text = text.ToLowerInvariant();
        text = MyRegex().Replace(text, " ");
        string[] tokens = text.Split([' ', '\t', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries);
        tokens = tokens.Where(token => !_stopWords.Contains(token)).ToArray();

        return string.Join(" ", tokens).Trim();
    }
}