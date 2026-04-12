import xml.etree.ElementTree as ET
import os

xml_path = r'd:\aDesktop\Dotnet\ITxUnitTestProject\Document\extracted_docx\word\document.xml'
output_path = r'd:\aDesktop\Dotnet\ITxUnitTestProject\Document\extracted_full_text.txt'

namespace = {'w': 'http://schemas.openxmlformats.org/wordprocessingml/2006/main'}

tree = ET.parse(xml_path)
root = tree.getroot()

with open(output_path, 'w', encoding='utf-8') as f:
    for paragraph in root.findall('.//w:p', namespace):
        texts = [node.text for node in paragraph.findall('.//w:t', namespace) if node.text]
        if texts:
            f.write(" ".join(texts) + '\n')
            
print(f"Extraction complete to {output_path}")
