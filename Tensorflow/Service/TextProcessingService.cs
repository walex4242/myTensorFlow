using Lucene.Net.Analysis;
using Lucene.Net.Analysis.En;
using Lucene.Net.Analysis.Util;
using Lucene.Net.Util;
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

    private readonly LuceneVersion _luceneVersion = LuceneVersion.LUCENE_48;

    public string PreprocessText(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        text = text.ToLowerInvariant();
        text = MyRegex().Replace(text, " ");
        string[] tokens = text.Split([' ', '\t', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries);
        tokens = [.. tokens.Where(token => !_stopWords.Contains(token))];

        // Use Lucene.Net Analyzer for stemming
        using (Analyzer analyzer = new EnglishAnalyzer(_luceneVersion, CharArraySet.EMPTY_SET)) // EnglishAnalyzer includes StopFilter and PorterStemFilter
        {
            List<string> stemmedTokens = new List<string>();
            foreach (var token in tokens)
            {
                using (var reader = new StringReader(token))
                using (var tokenStream = analyzer.GetTokenStream("content", reader))
                {
                    var termAttribute = tokenStream.GetAttribute<Lucene.Net.Analysis.TokenAttributes.ICharTermAttribute>();
                    tokenStream.Reset();
                    if (tokenStream.IncrementToken())
                    {
                        stemmedTokens.Add(termAttribute.ToString());
                    }
                    tokenStream.End();
                }
            }
            return string.Join(" ", stemmedTokens).Trim();
        }
    }

    [GeneratedRegex(@"[^a-z0-9\s]")]
    private static partial Regex MyRegex();
}