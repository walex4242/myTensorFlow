using System.Text.Json.Serialization;

public class SentenceEmbedding
{
    [JsonPropertyName("text")]
    public string Text { get; set; }

    [JsonPropertyName("embedding")]
    public List<float> Embedding { get; set; }
}

public class EmbeddingData
{
    [JsonPropertyName("text")]
    public string Text { get; set; }

    [JsonPropertyName("sentences")]
    public List<SentenceEmbedding> Sentences { get; set; }
}