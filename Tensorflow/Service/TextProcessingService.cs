using System.Text.RegularExpressions;

public partial class TextProcessingService
{
    private readonly Dictionary<string, HashSet<string>> _stopWordsByLanguage = new(StringComparer.OrdinalIgnoreCase)
    {
        { "en", new HashSet<string> { "the", "a", "an", "is", "are", "was", "were", "be", "being", "been", "to", "from", "in", "out", "on", "off", "at", "by", "for", "with", "and", "or", "but", "if", "then", "else", "that", "this", "these", "those", "it", "its", "he", "him", "his", "she", "her", "hers", "they", "them", "their", "theirs", "of", "as", "while", "after", "before", "during", "about", "against", "between", "into", "through", "above", "below", "under", "over", "again", "further", "then", "once", "here", "there", "when", "where", "why", "how", "all", "any", "both", "each", "few", "many", "some", "such", "no", "nor", "not", "only", "own", "same", "so", "than", "too", "very", "can", "will", "just", "should", "do", "does", "did", "had", "has", "have", "may", "might", "must", "need", "shall", "would", "could" } },
        { "pt", new HashSet<string> { "o", "a", "os", "as", "um", "uma", "uns", "umas", "é", "são", "era", "eram", "ser", "sendo", "sido", "ao", "aos", "à", "às", "de", "do", "da", "dos", "das", "em", "para", "com", "por", "se", "sem", "sobre", "sob", "ante", "até", "após", "durante", "entre", "já", "ainda", "muito", "pouco", "todo", "algum", "nenhum", "outro", "próprio", "tal", "qual", "quem", "que", "porque", "quando", "onde", "como", "mais", "menos", "bem", "mal", "sim", "não", "também", "assim", "contudo", "todavia", "entretanto", "logo", "portanto" } },
        { "es", new HashSet<string> { "el", "la", "los", "las", "un", "una", "unos", "unas", "es", "son", "era", "eran", "ser", "siendo", "sido", "al", "a", "del", "de", "da", "de", "los", "de", "las", "en", "para", "con", "por", "se", "sin", "sobre", "bajo", "ante", "hasta", "tras", "durante", "entre", "ya", "aún", "mucho", "poco", "todo", "algún", "ningún", "otro", "propio", "tal", "cual", "quien", "que", "porque", "cuando", "donde", "como", "más", "menos", "bien", "mal", "sí", "no", "también", "así", "sin embargo", "no obstante", "mientras tanto", "luego", "por lo tanto" } }
        // Add more languages and their stop words here
    };

    [GeneratedRegex(@"[^a-z0-9\s]")]
    private static partial Regex MyRegex();

    public string PreprocessText(string text, string languageCode = "en")
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        text = text.ToLowerInvariant();
        text = MyRegex().Replace(text, " ");
        string[] tokens = text.Split([' ', '\t', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries);

        if (_stopWordsByLanguage.TryGetValue(languageCode, out var stopWords))
        {
            tokens = tokens.Where(token => !stopWords.Contains(token)).ToArray();
        }

        return string.Join(" ", tokens).Trim();
    }

    // Optional: Implement a basic language detection function (very basic example)
    private string DetectLanguage(string text)
    {
        // This is a very basic and unreliable example.
        // For real-world applications, use a dedicated language detection library.
        if (Regex.IsMatch(text, @"\b(olá|bom dia|boa tarde|boa noite)\b", RegexOptions.IgnoreCase))
        {
            return "pt";
        }
        if (Regex.IsMatch(text, @"\b(hola|buen día|buenas tardes|buenas noches)\b", RegexOptions.IgnoreCase))
        {
            return "es";
        }
        return "en"; // Default to English
    }
}