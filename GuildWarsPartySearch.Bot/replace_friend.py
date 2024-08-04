import sys
import os

def replace_friend(file_path):
    with open(file_path, 'r') as file:
        content = file.read()

    content = content.replace('*friend', '*friendVar')

    with open(file_path, 'w') as file:
        file.write(content)

if __name__ == "__main__":
    for root, dirs, files in os.walk(sys.argv[1]):
        for file in files:
            if file.endswith('.h') or file.endswith('.hpp'):
                replace_friend(os.path.join(root, file))
