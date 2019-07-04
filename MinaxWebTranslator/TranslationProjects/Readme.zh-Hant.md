# 概觀
本目錄存放一些範例專案與 Glossary 檔案

## *.conf
範例翻譯專案檔

<br />
<br />

## GlossaryFileList.txt

此檔案儲存了關於許多 Glossary 子目錄與檔案的相對路徑（對這個目錄來說）
- 它應為一個 **UTF-8 純文字檔案**！
- 每一行代表一個相對目錄名稱或是檔案名稱
- 檔名必須包含副檔名，例如 .tsv，否則此名會被視為目錄名稱！
- 主程式可以從專案設定頁面中利用 GlossaryFileList.txt 抓取本目錄中存放的所有 Glossary 檔案

<br />

### 內容範例
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
此檔案壓縮了一些空的 Glossary 目錄（不包含任何 Glossary 檔案），可自行解壓縮到專案目錄中。
