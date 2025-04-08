using System.Text.Json;

public partial class BertService
{
    public EmbeddingData LoadedEmbeddings { get; private set; }

    public void LoadEmbeddings(string filePath)
    {
        LoadedEmbeddings = ReadEmbeddingsFromFile(filePath);
        if (LoadedEmbeddings == null)
        {
            Console.WriteLine("Failed to load embeddings.");
        }
    }

    public List<SentenceEmbedding> GetSentenceEmbeddingsFromLoadedData()
    {
        return LoadedEmbeddings?.Sentences;
    }

    public EmbeddingData ReadEmbeddingsFromFile(string filePath)
    {
        try
        {
            string jsonString = File.ReadAllText(filePath);
            EmbeddingData data = JsonSerializer.Deserialize<EmbeddingData>(jsonString);
            return data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading embeddings from file: {ex.Message}");
            return null;
        }
    }
}

public class SummarizationService
{
    private readonly BertService _bertService;

    public SummarizationService(BertService bertService)
    {
        _bertService = bertService;
    }

    public string GenerateSummary(string documentText)
    {
        var sentenceEmbeddings = _bertService.GetSentenceEmbeddingsFromLoadedData();
        if (sentenceEmbeddings == null || !sentenceEmbeddings.Any())
        {
            return "Could not generate summary due to missing embeddings.";
        }

        // 1. Calculate the document embedding (average of sentence embeddings)
        var documentEmbedding = sentenceEmbeddings
            .Where(s => s.Embedding != null && s.Embedding.Any())
            .Select(s => s.Embedding) // Access the inner List<float>
            .Aggregate((acc, vec) =>
            {
                if (acc == null) return vec;
                return acc.Zip(vec, (a, b) => a + b).ToList();
            })
            .Select(x => (float)x / (sentenceEmbeddings.Count > 0 ? sentenceEmbeddings.Count : 1)) // Cast to float
            .ToList();

        // Handle the case where there are no valid embeddings
        if (documentEmbedding == null || !documentEmbedding.Any())
        {
            return "Could not generate summary due to issues with embedding calculation.";
        }

        // 2. Calculate cosine similarity between each sentence embedding and the document embedding
        var sentenceScores = sentenceEmbeddings
            .Where(s => s.Embedding != null && s.Embedding.Any())
            .Select(sentence =>
            {
                double similarity = CosineSimilarity(sentence.Embedding, documentEmbedding);
                return new { Sentence = sentence.Text, Score = similarity, OriginalIndex = sentenceEmbeddings.IndexOf(sentence) };
            })
            .OrderByDescending(s => s.Score)
            .ToList();

        // 3. Select the top N sentences (e.g., based on a desired summary length)
        int summaryLength = Math.Min(5, sentenceScores.Count);
        var topSentences = sentenceScores
            .Take(summaryLength)
            .OrderBy(s => s.OriginalIndex)
            .Select(s => s.Sentence);

        // 4. Join the selected sentences to form the summary
        return string.Join(" ", topSentences);
    }

    private double CosineSimilarity(List<float> vecA, List<float> vecB)
    {
        if (vecA == null || vecB == null || vecA.Count != vecB.Count)
        {
            return 0.0;
        }

        double dotProduct = 0;
        double normA = 0;
        double normB = 0;

        for (int i = 0; i < vecA.Count; i++)
        {
            dotProduct += vecA[i] * vecB[i];
            normA += Math.Pow(vecA[i], 2);
            normB += Math.Pow(vecB[i], 2);
        }

        normA = Math.Sqrt(normA);
        normB = Math.Sqrt(normB);

        if (normA == 0 || normB == 0)
        {
            return 0.0;
        }

        return dotProduct / (normA * normB);
    }
}