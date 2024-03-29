﻿# Overview

Minax is a series of sub-projects and Apps for myself.  

[Readme - 繁體中文版](./Readme.zh-Hant.md)

<br />

## Minax Web Translator
- The Windows and Android Apps for remote Web Translator.  

See [MinaxWebTranslator](https://github.com/nuthrash/Minax/tree/master/MinaxWebTranslator/)


![MWT-WinDesktop-Target3-note.jpg](./Assets/Images/ScreenShots/MWT-WinDesktop-Target3-note.jpg "Minax Web Translator Desktop version") 

- [Windows Desktop version](https://github.com/nuthrash/Minax/tree/master/MinaxWebTranslator#windows-desktop)
- [Android version](https://github.com/nuthrash/Minax/tree/master/MinaxWebTranslator#android)

<br />

## Minax.Shared
- The shared library among Minax sub-projects
- Contains common facilities, functionality, or profiles.  
<br />

## Minax Inventory Manager
- A simple inventory manager app like Memento Database (Android).
- To Be Designed...  
<br />

## Build Environment
- Microsoft Visual Studio 2022 17.4.x
- .Net Framework 4.7 SDK
- .Net 6.0
- Android SDK Platform 31 (Android 12.0)


<br />

## Change Logs

### Minax Web Translator

#### v0.0.8
1. Add MiraiTranslate, iCIBA, LingoCloud, Papago translation support.
2. Update the target SDK version to 31(Android 12.0) of MinaxWebTranslator.Android according by Google Play's suggestion.
3. Update nuget packages.
4. Change some web translators AJAX version to web page version.
5. Change CROSS-Transer quick translation to Weblio of MinaxWebTranslator.Android.
6. Suspend Excite web translator support until it resume.
7. Fix unable open hyperlink of Credits page of of MinaxWebTranslator.Desktop net6-windows version.

#### v0.0.7
1. Update .Net 6.0 related facilities.
2. Update nuget packages and add net6.0 to TargetFramework declaration to support .Net 6.0.
3. Drop .Net Core 3.1 and .Net 5.0 support.
4. Drop WebPage version of Baidu, Google and Microsoft/Bing web translators support of MinaxWebTranslator.Desktop because of Internet Explorer 11 being retired(that would cause AJAX processing flow misfunction).
5. Suspend CrossLanguage web translator support until it upgraded.

#### v0.0.6
1. Update .Net 5.0 related facilities.
2. Update MahApps.Metro nuget version to v2.3.4 and update all its related facilities.
3. Update Dirkster.AvalonDock nuget version to v4.50.0 and update all its related facilities.
4. Add minimum font size settings of Source/Destination panel of MinaxWebTranslator.Desktop.
5. Change replacing pattern, and replacing words of Baidu API in TranslaterHelpers.cs of MinaxWebTranslator.Desktop.
6. Hide web-browser-based translators in TranslaterSelectorPanel.xaml.cs of MinaxWebTranslator.Desktop.

#### v0.0.5
1. Update .Net Core 3.1 related facilities.
2. Update MahApps.Metro nuget version to v2.2.0 and update all its related facilities.
3. Update Dirkster.AvalonDock nuget version to v4.3.0 and update all its related facilities.
4. Fix missing restoring and saving some fields of Project File in MainWindow.xaml.cs of MinaxWebTranslator.Desktop.

#### v0.0.4
1. Update .Net Core 3 final version related facilities.
2. Update MahApps.Metro nuget version to v2.0.0-alpha0490 and update all its related facilities.
3. Add text search boxes of Mapping panel of MinaxWebTranslator.Desktop.
4. Change some zh-Hant locale's words of MinaxWebTranslator.Desktop.
5. Remove section of "system.diagnostics" in the app.config of MinaxWebTranslator.Desktop to fit .Net Core 3.
6. Change DataGridCell's single-click editing code and flow of MinaxWebTranslator.Desktop.
7. Fix add items range error of _ModifyBindingWhenFileChanged() in MappingDockingPanel.xaml.cs of MinaxWebTranslator.Desktop.
8. Fix un-usable Bing Translator selector of MinaxWebTranslator.Desktop to reflect changed official Bing Translator web page code.
9. Fix missing App setting about "Monitor Auto Merge When File Changed" option and its text to "Auto merge Glossary Mapping entries without asking, when file added, deleted, changed" of MinaxWebTranslator.Desktop.
10. Fix error of getting source text of Source Panel in SourceDockingPanel.cs of MinaxWebTranslator.Desktop.

#### v0.0.3
1. Add "The meaning of Mapping" section of MinaxWebTranslator's readme.
2. Change text and missing copy action for MiCopyAndQuickXlateSelection MenuItem of MinaxWebTranslator.Desktop's Source panel
3. Change RtbSrcTgt_ScrollChanged() algorithm of MainWindow.cs of MinaxWebTranslator.Desktop.

#### v0.0.2
1. Add zh-Hant (繁體中文) locale and documents.
2. Fix MinaxWebTranslator Target translate button does not enabled after Source box key some text.
3. Fix other bugs and some mis-typed words.

#### v0.0.1
Initial release

<br />

### Minax.Shared

#### v0.0.8
1. Add MiraiTranslate, iCIBA, LingoCloud, Papago translation support.
2. Update nuget packages.
3. Change some arguments according the new version of Youdao web translator.

#### v0.0.7
1. Update new flow of Excite web translator of MinaxWebTranslator.
2. Change some arguments according the new version of Excite web translator.
3. Change some arguments according the new version of Youdao web translator.

#### v0.0.6
1. Change replacing pattern, and replacing words of Baidu API in RemoteAgents.cs.
2. Fix error of replacing Japanese HTML characters of JapaneseEscapeHtmlText in Profiles.cs.

#### v0.0.5
1. Change default values of TranslationProject class in TranslationProject.cs.

#### v0.0.4
1. Change replacing pattern, retry logic, replacing words and BaiduTranslatorFormat2 format of Baidu API.

#### v0.0.3
1. Add some replacing text for Baidu translator of Profiles.cs.
2. Change Excite translator location of RemoteAgents.cs.

#### v0.0.2
1. Add zh-Hant (繁體中文) locale and documents.
2. Fix Minax.Shared Profiles DefaultEngineFolders' Microsoft folder name
3. Fix other bugs and some mis-typed words.

#### v0.0.1
Initial release

