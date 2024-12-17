import os
import sys
from pathlib import Path

def generate_enum_names(directory_path):
    """
    Scans a directory for .tres files and generates enum names.
    Returns both the formatted enum string and a list of names.
    """
    # Get all .tres files
    p = Path(directory_path)
    resource_files = list(p.glob('*.tres'))
    
    if not resource_files:
        print(f"No .tres files found in {directory_path}")
        return "", []
    
    # Extract names and format them
    enum_names = []
    for file_path in resource_files:
        # Remove .tres and convert to PascalCase
        name = file_path.stem  # get filename without extension
        # Handle hyphenated or underscore names
        words = name.replace('-', '_').split('_')
        pascal_case = ''.join(word.capitalize() for word in words)
        enum_names.append(pascal_case)
    
    # Sort names alphabetically
    enum_names.sort()
    
    # Generate enum string
    enum_string = "public enum EnemyType\n{\n"
    enum_string += ",\n".join(f"    {name}" for name in enum_names)
    enum_string += "\n}"
    
    return enum_string, enum_names

def main():
    if len(sys.argv) < 2:
        print("Please provide the path to your resources directory")
        print("Usage: python generate_enums.py <path_to_resource_directory>")
        return
    
    directory_path = sys.argv[1]
    
    if not os.path.exists(directory_path):
        print(f"Directory not found: {directory_path}")
        return
    
    enum_string, names = generate_enum_names(directory_path)
    
    if names:
        print("\nFound the following enemy resources:")
        for name in names:
            print(f"  - {name}")
        
        print("\nGenerated enum definition:")
        print(enum_string)
        
        # Write to file
        output_file = "EnemyType.cs"
        with open(output_file, 'w') as f:
            f.write(enum_string)
        print(f"\nEnum definition written to {output_file}")

if __name__ == "__main__":
    main()