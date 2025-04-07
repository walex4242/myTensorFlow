import tensorflow as tf
from transformers import BertTokenizer, TFBertModel
import numpy as np
import json

saved_model_path = r"C:\dev\myTensorFlow\python\savemodel" # Using raw string
vocabulary_path = r"C:\dev\myTensorFlow\python\savemodel\assets\vocab.txt"
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
        return output['bert_encoder'].numpy().tolist()
    # Check for 'last_hidden_state' (likely token embeddings)
    elif 'last_hidden_state' in output:
        return output['last_hidden_state'].numpy().tolist()
    # Check for any of the 'bert_encoder_XX' tensors (could be specific layers)
    elif any(key.startswith('bert_encoder_') for key in output):
        # Return the first 'bert_encoder_XX' tensor found
        for key in output:
            if key.startswith('bert_encoder_'):
                print(f"Returning embeddings from: {key}")
                return output[key].numpy().tolist()
    elif 'Identity_1' in output: # Based on your original attempt
        return output['Identity_1'].numpy().tolist()
    else:
        raise ValueError(f"Could not find a recognized embedding output tensor in the model's output. Available keys: {output.keys()}")

text_to_embed = "This is a sample sentence."
embeddings = get_bert_embedding(text_to_embed)

# Save the embeddings to a JSON file
with open("embeddings.json", "w") as f:
    json.dump({"text": text_to_embed, "embeddings": embeddings}, f)

print("Embeddings saved to embeddings.json")