import re

def extract_classes_and_members(csharp_code):
    # Regular expression patterns for classes and member usage
    class_pattern = r'\bnew\s+([A-Za-z0-9_]+)\b|\b([A-Za-z0-9_]+)\s+[A-Za-z0-9_]+\s*='
    member_pattern = r'\b([A-Za-z0-9_]+)\.([A-Za-z0-9_]+)\b'

    # Dictionary to store classes and their members
    class_members = {}

    # Find all class instances and static calls
    for match in re.finditer(class_pattern, csharp_code):
        class_name = match.group(1) or match.group(2)
        if class_name not in class_members:
            class_members[class_name] = set()

    # Find all member (function/variable) calls
    for match in re.finditer(member_pattern, csharp_code):
        class_name, member_name = match.groups()
        if class_name in class_members:
            class_members[class_name].add(member_name)

    # Convert sets to lists for serialization
    for class_name in class_members:
        class_members[class_name] = list(class_members[class_name])

    return class_members

#csharp_code from file LidarManager.cs
import os
csharp_code = open(os.path.join(os.path.dirname(__file__), 'LidarManager.cs')).read()
print(extract_classes_and_members(csharp_code))
