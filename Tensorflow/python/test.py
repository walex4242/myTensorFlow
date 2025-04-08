import tensorflow as tf
from transformers import BertTokenizer, TFBertModel
import numpy as np
import json
import nltk
import sys
import os

# Download punkt tokenizer if you haven't already
try:
    nltk.data.find('tokenizers/punkt')
except LookupError:  # Corrected exception handling
    nltk.download('punkt')

saved_model_path = r"C:\Users\OLAWALE\Desktop\myTensorFlow\Tensorflow\python\savemodel"  # Using raw string
vocabulary_path = r"C:\Users\OLAWALE\Desktop\myTensorFlow\Tensorflow\python\savemodel\assets\vocab.txt"
tokenizer = BertTokenizer.from_pretrained(vocabulary_path)
model = tf.saved_model.load(saved_model_path)
infer = model.signatures["serving_default"]

print("Output signature of the infer function:")
print(infer.structured_outputs)

def get_bert_embedding(text):
    encoded_input = tokenizer(text, return_tensors='tf', truncation=True, padding=True)
    output = infer(input_word_ids=encoded_input['input_ids'],
                    input_mask=encoded_input['attention_mask'],
                    input_type_ids=encoded_input['token_type_ids'])
    # Check for 'bert_encoder' (likely pooled output)
    if 'bert_encoder' in output:
        return output['bert_encoder'].numpy().tolist()[0]  # Access the first (and only) element
    # Check for 'last_hidden_state' (likely token embeddings - you might want to pool these)
    elif 'last_hidden_state' in output:
        # For sentence embedding from last_hidden_state, take the mean over the sequence
        return tf.reduce_mean(output['last_hidden_state'], axis=1).numpy().tolist()[0]
    # Check for other potential output layers
    elif 'pooler_output' in output:
        return output['pooler_output'].numpy().tolist()[0]
    elif any(key.startswith('bert_encoder_') for key in output):
        for key in output:
            if key.startswith('bert_encoder_'):
                print(f"Returning embeddings from: {key}")
                return output[key].numpy().tolist()[0]
    elif 'Identity_1' in output:  # Based on your original attempt
        return output['Identity_1'].numpy().tolist()[0]
    else:
        raise ValueError(f"Could not find a recognized embedding output tensor. Available keys: {output.keys()}")

def process_document_and_get_embeddings(document_content):
    sentences = nltk.sent_tokenize(document_content)
    sentence_embeddings = []
    for sentence in sentences:
        embedding = get_bert_embedding(sentence)
        sentence_embeddings.append({"text": sentence, "embedding": embedding})
    return {"text": document_content, "sentences": sentence_embeddings}

if __name__ == "__main__":
    if len(sys.argv) != 3:
        print("Usage: python your_script.py <input_text_file> <output_json_file>")
        sys.exit(1)

    input_text_file = sys.argv[1]
    output_json_file = sys.argv[2]

    try:
        with open(input_text_file, 'r', encoding='utf-8') as infile:
            document_content = infile.read()
    except FileNotFoundError:
        print(f"Error: Input file not found at {input_text_file}")
        sys.exit(1)

    embeddings_data = process_document_and_get_embeddings(document_content)

    with open(output_json_file, "w", encoding='utf-8') as outfile:
        json.dump(embeddings_data, outfile, ensure_ascii=False, indent=4)

    print(f"Embeddings for the document's sentences saved to {output_json_file}")