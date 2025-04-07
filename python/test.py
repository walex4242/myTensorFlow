import tensorflow as tf
from transformers import BertTokenizer, TFBertModel
import numpy as np
import json

saved_model_path = r"C:\Users\OLAWALE\Desktop\myTensorFlow\python\savemodel" # Using raw string
vocabulary_path = r"C:\Users\OLAWALE\Desktop\myTensorFlow\python\savemodel\assets\vocab.txt"
tokenizer = BertTokenizer.from_pretrained(vocabulary_path)
model = tf.saved_model.load(saved_model_path)
infer = model.signatures["serving_default"]

def get_bert_embedding(text):
    encoded_input = tokenizer(text, return_tensors='tf', truncation=True, padding=True)
    output = infer(**encoded_input)
    # Adjust based on the output tensor you need (e.g., last_hidden_state, pooler_output)
    return output['Identity_1'].numpy().tolist() # Example: token embeddings

text_to_embed = "This is a sample sentence."
embeddings = get_bert_embedding(text_to_embed)

# Save the embeddings to a JSON file
with open("embeddings.json", "w") as f:
    json.dump({"text": text_to_embed, "embeddings": embeddings}, f)

print("Embeddings saved to embeddings.json")