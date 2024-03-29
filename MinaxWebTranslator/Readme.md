﻿<img src="../Assets/Images/Translator.png" width="64" title="Translator App Icon">

# Overview
**Minax Web Translator** app is **not** a translator. It just an **agent and text mapping-replacer** for remote web translator/translation result.
It works with some online web sites and APIs, such as **Excite, CROSS-transer, Weblio, Baidu, Youdao, Google, Microsoft/Bing** etc..

- Wiki: [https://github.com/nuthrash/Minax/wiki](https://github.com/nuthrash/Minax/wiki)
- Sample Projects: [https://github.com/nuthrash/Minax/tree/master/MinaxWebTranslator/TranslationProjects](https://github.com/nuthrash/Minax/tree/master/MinaxWebTranslator/TranslationProjects)

[Readme - 繁體中文版](./Readme.zh-Hant.md)

<br />

---

# Concepts and Translation API information
See [Conepts.md](./Concepts.md)  ([Concepts - 繁體中文版](./Concepts.zh-Hant.md))
<br />
<br />

---

# Usage and Requirements
## Windows Desktop
- Project Folder: [MinaxWebTranslator.Desktop](https://github.com/nuthrash/Minax/tree/master/MinaxWebTranslator/MinaxWebTranslator.Desktop)
- System Requirement
  1. Operating System: Windows 7 Service Pack 1 (SP1) and above
  2. .Net Framework [4.7](https://dotnet.microsoft.com/download/dotnet-framework/net47) and above, .Net [6.0](https://dotnet.microsoft.com/download/dotnet/6.0).
  3. Privileged Access: local storage, network

### Main Window

- Startup
<img src="../Assets/Images/ScreenShots/MWT-WinDesktop-Main1.jpg" title="MWT Desktop Main - Startup">

<br />

- Draggable and Dockable Panels
<img src="../Assets/Images/ScreenShots/MWT-WinDesktop-Main2-Dockable.jpg" title="MWT Desktop Main - Drag and Dockable">
  - You can drag and dock each panel to anywhere <br />
  - And restore to original docking layout by Menu -> Panel Layout -> Restore Default  
<br />
<br />

- Hideable Panels
<img src="../Assets/Images/ScreenShots/MWT-WinDesktop-Main3-Hideable.jpg" title="MWT Desktop Main - Hideable">
  - You can hide some panels to focus on Target panel

### Menu
- Project New, Open, Save, Close, and Recent Project(s)
<img src="../Assets/Images/ScreenShots/MWT-WinDesktop-Menu1-RecentProjectWithTooltip.jpg" title="MWT Desktop Menu1 - Recent Project With Tooltip">
  - Create New Project: Menu -> Project -> New <br />
  - Open Existed Project: Menu -> Project -> Open <br />
  - Save Opened Project: Menu -> Project -> Save <br />
  - Close Opened Project: Menu -> Project -> Close <br />
  - Open One of Recent Project(s): Menu -> Project -> Recent Project(s) -> Click desired project <br />
    - When mouse stop on one recent project item, it would popup tooltip table to show some information about it. <br />
<br />

- Translator
  - Select Translator and fill required field(s): Menu -> Translator
  <img src="../Assets/Images/ScreenShots/MWT-WinDesktop-Translator1.jpg" title="MWT Desktop Translator1"> <br />
  - When select a **charged** Translation API, you might fill required field(s) or keep blank until click Translate button to ask it or them.
  <img src="../Assets/Images/ScreenShots/MWT-WinDesktop-Translator2.jpg" title="MWT Desktop Translator2">


### Translating
This feature is the main feature of this App to translate large size text.
- Step 1: Open or create a Translation Project
- Step 2: Select a desired Translator (Menu -> Translator)
- Step 3: Create/Modify some mapping item in Mapping panel
- Step 4: Fill some text to Source box in Source panel
- Step 5: Click Translate button in Target panel
<img src="../Assets/Images/ScreenShots/MWT-WinDesktop-Target3-note.jpg" title="MWT Desktop Translator1">
  - The translated text would show on the Target box in Target panel. <br />
  - The mapped text would be marked with bold.  

  <br />

### Quick Translation
This feature is a simple feature to quick translate small piece of text,
therefore the Input box is restricted to max 500 words.

![MWT-WinDesktop-QuickTranslation2](../Assets/Images/ScreenShots/MWT-WinDesktop-QuickTranslation2.jpg "Quick Translation Panel")

The basic usage is same with main translating feature, besides you can select multiple Translators to translate short text simultaneously.
And the "Integrated Output" box would integrate all selected Translators' output text to a single section.

---

## Xamarin.Forms
- Base Project Folder: [MinaxWebTranslator](https://github.com/nuthrash/Minax/tree/master/MinaxWebTranslator/MinaxWebTranslator)
- System Requirement
  1. Operating System: depends on each sub-package's platform
  2. Privileged Access: local storage, network


### Android
- Project Folder: [MinaxWebTranslator.Android](https://github.com/nuthrash/Minax/tree/master/MinaxWebTranslator/MinaxWebTranslator.Android)
- System Requirement
  1. Operating System: Android 5.0 (API Level 21 - Lollipop) and above
  2. Target API Version: Android 11.0 (API Level 30)
  3. Privilege Access: local storage, network, cache, diagnostic, account, external store, write settings, write secure settings
- Not Support
  1. Auto restore original content after App sleep
  2. File Changed Monitor

<br />

#### Main Page
- Startup

<img src="../Assets/Images/ScreenShots/MWT-Droid-Main.png" width="256 " title="MWT Droid Main - Startup">

#### Menu
- No project opened

<img src="../Assets/Images/ScreenShots/MWT-Droid-Menu1.png" width="256 " title="MWT Droid Menu - No project opened">

- Project opened (some menu items would show after project opened)

<img src="../Assets/Images/ScreenShots/MWT-Droid-Menu2.png" width="256 " title="MWT Droid Menu - Project opened">

<br />

---

## Translator Selection
There are many Translators or Translation Services provided by this App.
However, The Android version has less selection due to the limitation of operating system.  

Besides, not every Translating engine can translate any source language to target language.
Therefore, I only put 2 source languages and 2 target languages in the App options, that is because I am more familiar with them.

The rules of Translator selection are very simple:
1. Select the engine which is familiar with source language
   - The Google Translate come from America, it is more familiar with English.
   - The Excite Translator come from Japan, it is more familiar with Japanese.
   - The Baidu Translator come from China, it is more familiar with Chinese.
2. Select the engine which is familiar with target language
   - By 1., we understand and select a engine which is familiar with source language
   - That selected engine might not be familiar with target language
     - For exmaple, Chinese is Logogram, and English is Phonogram, the syntax and word sequence are very different with each other.
     - When you select Google Translate to translate English to Japanese, you may get a acceptable result. But in the reverse direction is not so.
     - When you select Baidu Translator to translate between Chinese and Japanese, the result is acceptable on bi-direction, that's because these two languages are Logogram.
3. Press (Translate button) and Pray
   - No matter what, these Translating engines are Machine Translation, you should not expect them would translate very well!!

If you do not understand what I talk about, you could try every engine many times to find what is the best for you. 

<br />

### Service Availability
This App almost works with remote web translation services, whether via Web Page or Web APIs.
No App can ensure remote services always online or usable, because the network and service availabilities are heavily depend on user's environment.  

Therefore, when you encounter some problems in translating, first thing is to check your environment, then retry again.
Sometimes, it just caused by network latency.
Maybe after you retry the operation, it would work again.  

In addition to the network problem, there are other situations which cause the operation failed.  
One of them is the service providers changed their HTML codes. Another situation maybe they blocked out-of-quota requests.  
The latter might be resolved by retrying later in hours.  
The former shall call the programmer to see what happened.

### Means of Mapping
The main function of this App is replacing from a segment of text to another in translating, this is the Mapping.

Main usages of Mapping:
1. Keep original text
   - Maybe a segment text will be replaced to a strange segment text in translating, but it shall not be replaced in the Project.
   - You can put same text in "Original Text" and "Mapping Text" fields to ensure it would not be mis-translated in translating.
2. Clear some text
   - Sometimes the layout of translated text is so bad, that you shall fix it line by line.
   - It would be used when inserting or deleting whitespace text.
3. Manually translate
   - This is the common case of Mapping. The vocabulary databases of translation engines are neither at the cutting-edges, nor understand any self-defined term. Therefore, you should add some new Mappings for them manually.
   - Fill "Original Text" with original text, and fill "Mapping Text" with preferred text.
   - When translating, this App would automatically replace the text of <u>Target input box</u> where contains "Original Text" string to non-replaced string. Then invoke remote translation, and replace translated text with "Mapping Text" after translation. 

