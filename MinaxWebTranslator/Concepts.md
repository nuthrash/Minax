# Minax Web Translator (MWT) Concepts

## Simple Concepts
- Step 1: Input source text.
- Step 2: Replace each `MappingEntry`'s `OriginalText` to a non-replaceable string for each translator/translation platform.
- Step 3: Invoke remote Translator/Translation site to perform **Statistical Machine Translation** (**SMT**), **Neural Machine Translation** (**NMT**), or other translation technologies.
- Step 4: Receive translated text which contains non-replaceable strings.
- Step 5: Replace each non-replaceable string to corresponding `MappingEntry`'s `MappingText` .

That's all.

## MappingEntry
This class contains five fields:
1. `OriginalText`: The original text want to replace or keep. **This field cannot be empty or full of whitespace text** .
2. `MappingText`: The mapping text want to be replaced or keeped.
3. `Category`: This `MappingEntry`'s category to classify. Such as character name, family name, material, etc..
4. `Description`: The text describe this `MappingEntry`'s meanings or comments.
5. `Comment`: The extra text to comment this `MappingEntry` about something.

The screenshot:
![MWT-WinDesktop-MappingProject](../Assets/Images/ScreenShots/MWT-WinDesktop-MappingProject.jpg "MWT MappingProject of Desktop version")

## Glossary File
A glossary file is a file which stores some mapping text/terms. It might be a simple two columns text file, table-based text file, Excel file, Translation Memory eXchange (TMX) XML file, or other translation related file format.

<br />

### Supported File Formats
- Currently, MWT only support tab-separated values (TSV) .tsv and camma-separated values (CSV) .csv (MWT would detect by itself!)
- **Each file shall be UTF-8 encoded with BOM** .
- Columns
  - First row may be Column Name, or row data
  - Like `MappingEntry`, when a glossary file has first row with Column Name, their sequence is prefer `OriginalText`, `MappingText`, `Category`, `Description`, and `Comment`.
  - Otherwise, first-two column names shall be `OriginalText` and `MappingText`. Other fields are optional.
  - That means, **each glossary file shall has at least two columns** for `OriginalText` and `MappingText` (like [Google Translation toolkit](https://translate.google.com/toolkit) ).

## Translator or Translation
These two terms are almost exchangable in all cases. However, we use **Translator** to mean *Web Page based translator site* like Google Translate. Meanwhile, use **Translation** for *API-based translation service* like Google Translation API v2.

---

# Translation API Services

## Microsoft Translator Text API v3.0

- Documents: https://docs.microsoft.com/en-us/azure/cognitive-services/translator/reference/v3-0-reference
- How to sign-up: https://docs.microsoft.com/en-us/azure/cognitive-services/translator/translator-text-how-to-signup

## Google Cloud Translation API v2

- Documents: https://cloud.google.com/translate/docs
- How to create API key: https://cloud.google.com/docs/authentication/api-keys
- Cloud Translation API v3 (beta): https://cloud.google.com/translate/docs/intro-to-v3
  - Has free quota!!

## Baidu Translation API (百度翻译开放平台)

- Documents (简体中文): http://api.fanyi.baidu.com/api/trans/product/apidoc 
- Where to request APP ID and Secret Key (简体中文): http://api.fanyi.baidu.com/api/trans/product/desktop  

## Youdao Translation API (有道智云翻译 API)

- Docuemnts (简体中文): https://ai.youdao.com/docs/doc-trans-api.s#p01
- Where to request App Key (App ID) and App Secret (简体中文): https://ai.youdao.com/doc.s#guide

