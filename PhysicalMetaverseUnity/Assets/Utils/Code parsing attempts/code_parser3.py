import re

def find_capital_letter_words(file_path):
    with open(file_path, 'r') as file:
        text = file.read()

    # Remove block comments
    text = re.sub(r'/\*.*?\*/', '', text, flags=re.DOTALL)

    # Remove single-line comments
    text = re.sub(r'//.*', '', text)

    # Regex pattern to match CamelCase words or words starting with a capital letter
    # This will match 'PillarManager' and similar words
    pattern = r'\b[A-Z][a-zA-Z]*\b'

    # Find all matches in the text
    matches = re.findall(pattern, text)

    #remove fully capital letter words
    matches = [word for word in matches if not word.isupper()]

    #clear matches from repeated words
    matches = list(dict.fromkeys(matches))
    return matches

file_path = 'SunManager.txt'  # Replace with your file path
matched_words = find_capital_letter_words(file_path)
print(matched_words)
