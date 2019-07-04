# Overview
This folder place some sample projects and glossary files.

## *.conf
Sample Translation Project files.

<br />
<br />

## GlossaryFileList.txt

This file stores many relative paths (to this folder) of Glossary sub-folders and files.
- It shall be an **UTF-8 pure text file**!
- Each line presents a relative folder name or file name
- A file name shall has extension, like .tsv, otherwise this name would be considered as a folder name!
- The App could fetch all Glossary files under this folder by GlossaryFileList.txt via Project Settings page.

<br />

### Content sample
```
./Glossary/Excite
./Glossary/Excite/English2ChineseTraditional
./Glossary/Excite/Japanese2ChineseTraditional
./Glossary/Excite/Japanese2ChineseTraditional/MiscTerms1.tsv
./Glossary/Excite/Japanese2ChineseTraditional/Monster1.csv
./Glossary/Excite/Japanese2ChineseTraditional/Translator1.tsv
./Glossary/Excite/Japanese2English
 ```

<br />
<br />

## GlossaryEmpty.zip
This file compressed some empty glossary folders (not contain any glossary file) which can be extracted to Project folder by yourself.
