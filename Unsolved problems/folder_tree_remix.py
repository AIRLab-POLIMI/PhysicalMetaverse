import os
import json

def generate_vscode_structure(file_list):
    vscode_structure = {}

    for file_path in file_list:
        current = vscode_structure
        parts = file_path.split("/")

        for part in parts:
            if part not in current:
                current[part] = {}
            current = current[part]

    return vscode_structure

def generate_vscode_settings(vscode_structure, indent=0):
    settings = ""

    for key, value in vscode_structure.items():
        if value:
            settings += " " * indent + f'"{key}": ' + "{\n"
            settings += generate_vscode_settings(value, indent + 4)
            settings += " " * indent + "},\n"
        else:
            settings += ' ' * indent + f'"{key}": {{}},\n'

    return settings

def main():
    file_path = "folder_tree.txt"

    try:
        # Read file content
        with open(file_path, "r") as file:
            file_content = file.read()
    except FileNotFoundError:
        print(f"File not found: {file_path}")
        return

    # Split the content into lines and remove leading/trailing whitespaces
    lines = [line.strip() for line in file_content.split("\n") if line.strip()]

    # Generate a hierarchical structure for vscode settings
    vscode_structure = generate_vscode_structure(lines)

    # Generate vscode settings.json content
    vscode_settings = generate_vscode_settings(vscode_structure)

    # Specify the output file path
    output_file_path = "folder_json.json"

    # Write the formatted content to the output file
    with open(output_file_path, "w") as output_file:
        output_file.write("{\n")
        output_file.write(vscode_settings.rstrip(",\n"))
        output_file.write("\n}")

    print(f"Formatted content has been written to {output_file_path}")

if __name__ == "__main__":
    main()
