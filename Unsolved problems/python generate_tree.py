import os

def generate_folder_tree(start_path, output_file):
    with open(output_file, 'w') as file:
        for root, dirs, files in os.walk(start_path):
            level = root.replace(start_path, '').count(os.sep)
            indent = ' ' * 4 * (level)
            file.write('{}{}/\n'.format(indent, os.path.basename(root)))
            subindent = ' ' * 4 * (level + 1)
            for file_name in files:
                file.write('{}{}\n'.format(subindent, file_name))

if __name__ == "__main__":
    start_path = os.getcwd()  # Current working directory
    output_file = 'folder_tree.txt'
    generate_folder_tree(start_path, output_file)
    print(f"Folder tree saved to {output_file}")
