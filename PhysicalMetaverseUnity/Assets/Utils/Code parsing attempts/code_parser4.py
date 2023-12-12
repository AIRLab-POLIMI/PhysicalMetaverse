import re

def retrieve_words_in_angle_brackets_and_instance(file_path):
    with open(file_path, 'r') as file:
        filename = file_path.split('.')[0]
        in_block_comment = False
        in_string_literal = False
        angle_bracket_results = []
        instance_results = []

        for line in file:
            # Reset string literal flag at the start of each line
            in_string_literal = False

            # Skip lines with line comments (//), allowing spaces before
            line = line.strip()
            if line.startswith('//'):
                continue

            # Process each character in the line
            for i, char in enumerate(line):
                # Handle block comments
                if char == '/' and i + 1 < len(line) and line[i + 1] == '*':
                    in_block_comment = True
                elif char == '*' and i + 1 < len(line) and line[i + 1] == '/':
                    in_block_comment = False
                    continue

                # Skip characters in block comments
                if in_block_comment:
                    continue

                # Handle string literals
                if char == '"':
                    in_string_literal = not in_string_literal
                    continue

                # Skip characters in string literals
                if in_string_literal:
                    continue

            # Find words enclosed in < > if not in block comment or string literal
            if not in_block_comment and not in_string_literal:
                angle_bracket_matches = re.findall(r'<(\S+?)>', line)
                angle_bracket_results.extend(angle_bracket_matches)

                # Find words followed by .Instance
                instance_matches = re.findall(r'(\w+)\.Instance', line)
                instance_results.extend(instance_matches)

        # Remove duplicates
        angle_bracket_results = list(set(angle_bracket_results))
        instance_results = list(set(instance_results))

        return angle_bracket_results, instance_results, filename

# Example usage
file_path = 'LidarManager.txt'
angle_bracket_words, instance_words, filename = retrieve_words_in_angle_brackets_and_instance(file_path)
print("Words in angle brackets:", angle_bracket_words)
print("Words followed by .Instance:", instance_words)
#merge lists
angle_bracket_words.extend(instance_words)
#remove duplicates
angle_bracket_words = list(dict.fromkeys(angle_bracket_words))
print(angle_bracket_words)
#write file SunManager_classes.txt, first line filename, then angle_bracket_words
with open(filename + '_classes.txt', 'w') as file:
    file.write(filename + '\n')
    for word in angle_bracket_words:
        file.write(word + '\n')

