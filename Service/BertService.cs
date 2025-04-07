using System;
using System.Collections.Generic;
using System.Linq;
using Bert.Net.Tokenizer; // Adjust this namespace if needed
using Tensorflow;

public class BertService
{
    private readonly SavedModelBundle _model;
    private readonly BertTokenizer _tokenizer;
    private readonly int _maxSequenceLength = 128; // Adjust as needed

    // Input tensor names from the Python script output
    private const string InputIdsName = "input_word_ids";
    private const string AttentionMaskName = "input_mask";
    private const string TokenTypeIdsName = "input_type_ids";

    // Output tensor names from the Python script output (adjust based on your needs)
    private const string SentenceEmbeddingName = "Identity:0";       // Assuming this is the sentence embedding
    private const string TokenEmbeddingsName = "Identity_1:0";     // Assuming this is the token embeddings

    public BertService(string modelPath, string vocabularyPath)
    {
        try
        {
            _model = tf.saved_model.load(modelPath);
            _tokenizer = new BertTokenizer(vocabularyPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading BERT model or vocabulary: {ex.Message}");
            throw;
        }
    }

    public float[] GetSentenceEmbedding(string text)
    {
        if (_model == null || _tokenizer == null)
        {
            Console.WriteLine("BERT model or tokenizer not initialized.");
            return null;
        }

        var encoding = _tokenizer.Encode(text.Take(_maxSequenceLength).ToList(), _maxSequenceLength);

        var inputIdsTensor = tf.constant(encoding.Select(x => (long)x.InputIds).ToArray(), TF_INT64, new long[] { 1, _maxSequenceLength });
        var attentionMaskTensor = tf.constant(encoding.Select(x => (long)x.AttentionMask).ToArray(), TF_INT64, new long[] { 1, _maxSequenceLength });
        var tokenTypeIdsTensor = tf.constant(encoding.Select(x => (long)x.TokenTypeIds).ToArray(), TF_INT64, new long[] { 1, _maxSequenceLength });

        try
        {
            var runner = _model.signatures["serving_default"];
            var feed = new Dictionary<string, Tensor>
            {
                { InputIdsName, inputIdsTensor },
                { AttentionMaskName, attentionMaskTensor },
                { TokenTypeIdsName, tokenTypeIdsTensor }
            };
            var results = runner.Call(feed);

            var sentenceEmbeddingTensor = results[SentenceEmbeddingName];
            var sentenceEmbedding = sentenceEmbeddingTensor.numpy();

            // Assuming the shape is [1, embedding_size], we take the first element
            return JaggedArrayToSingleDimensional(sentenceEmbedding);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during BERT inference for sentence embedding: {ex.Message}");
            return null;
        }
    }

    public float[][] GetTokenEmbeddings(string text)
    {
        if (_model == null || _tokenizer == null)
        {
            Console.WriteLine("BERT model or tokenizer not initialized.");
            return null;
        }

        var encoding = _tokenizer.Encode(text.Take(_maxSequenceLength).ToList(), _maxSequenceLength);

        var inputIdsTensor = tf.constant(encoding.Select(x => (long)x.InputIds).ToArray(), TF_INT64, new long[] { 1, _maxSequenceLength });
        var attentionMaskTensor = tf.constant(encoding.Select(x => (long)x.AttentionMask).ToArray(), TF_INT64, new long[] { 1, _maxSequenceLength });
        var tokenTypeIdsTensor = tf.constant(encoding.Select(x => (long)x.TokenTypeIds).ToArray(), TF_INT64, new long[] { 1, _maxSequenceLength });

        try
        {
            var runner = _model.signatures["serving_default"];
            var feed = new Dictionary<string, Tensor>
            {
                { InputIdsName, inputIdsTensor },
                { AttentionMaskName, attentionMaskTensor },
                { TokenTypeIdsName, tokenTypeIdsTensor }
            };
            var results = runner.Call(feed);

            var tokenEmbeddingsTensor = results[TokenEmbeddingsName];
            return JaggedArrayToMultiDimensional(tokenEmbeddingsTensor.numpy());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during BERT inference for token embeddings: {ex.Message}");
            return null;
        }
    }

    // Helper function to convert jagged array (likely [1, seq_len, embedding_dim]) to multi-dimensional array [seq_len, embedding_dim]
    private static float[][] JaggedArrayToMultiDimensional(Array jaggedArray)
    {
        if (jaggedArray.Rank != 3 || jaggedArray.GetLength(0) != 1)
        {
            Console.WriteLine("Unexpected shape for token embeddings.");
            return null;
        }

        int seqLen = jaggedArray.GetLength(1);
        int embeddingDim = jaggedArray.GetLength(2);
        float[][] result = new float[seqLen][];

        for (int i = 0; i < seqLen; i++)
        {
            result[i] = new float[embeddingDim];
            for (int j = 0; j < embeddingDim; j++)
            {
                result[i][j] = (float)jaggedArray.GetValue(0, i, j);
            }
        }
        return result;
    }

    // Helper function to convert jagged array (likely [1, embedding_size]) to single-dimensional array [embedding_size]
    private static float[] JaggedArrayToSingleDimensional(Array jaggedArray)
    {
        if (jaggedArray.Rank != 2 || jaggedArray.GetLength(0) != 1)
        {
            Console.WriteLine("Unexpected shape for sentence embedding.");
            return null;
        }

        int embeddingSize = jaggedArray.GetLength(1);
        float[] result = new float[embeddingSize];
        for (int i = 0; i < embeddingSize; i++)
        {
            result[i] = (float)jaggedArray.GetValue(0, i);
        }
        return result;
    }
}