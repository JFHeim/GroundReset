import re

import requests


def get_version_from_github(file_url):
    try:
        response = requests.get(file_url)
        response.raise_for_status()
        content = response.text

        version_line = re.search(r'ModVersion = "(\d+\.\d+\.\d+)"', content)
        version_line = version_line.group(1).replace("Current version: ", "")
        if version_line:
            return version_line
        else:
            return "VersionNotFound"
    except Exception as e:
        return f"Error: {e}"


file_url = 'https://raw.githubusercontent.com/FroggerHH/GroundReset/main/GroundReset/Plugin.cs'
version = get_version_from_github(file_url)
print(version)
