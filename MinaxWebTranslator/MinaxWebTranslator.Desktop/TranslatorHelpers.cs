using HtmlAgilityPack;
using Minax.Collections;
using Minax.Domain.Translation;
using Minax.Web.Translation;
using Microsoft.International.Converters.TraditionalChineseToSimplifiedConverter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Minax;

namespace MinaxWebTranslator.Desktop
{
	/// <summary>
	/// App internal Translator helper methods
	/// </summary>
	internal static class TranslatorHelpers
	{

		public static SupportedSourceLanguage SourceLanguage { get; set; } = SupportedSourceLanguage.Japanese;
		public static SupportedTargetLanguage TargetLanguage { get; set; } = SupportedTargetLanguage.ChineseTraditional;

		/// <summary>
		/// Auto scoll up to top after translating finished
		/// </summary>
		public static bool AutoScrollToTop { get; set; } = true;

		/// <summary>
		/// Mark mapped text with HTML bold tag to highlight
		/// </summary>
		public static bool MarkMappedTextWithHtmlBoldTag { get; set; } = true;

		/// <summary>
		/// MappingModel list in descended order for replacing from long to short
		/// </summary>
		public static ReadOnlyObservableList<MappingMonitor.MappingModel> DescendedModels { get; set; }
		/// <summary>
		/// Current used MappingModel after translating
		/// </summary>
		public static ObservableList<MappingMonitor.MappingModel> CurrentUsedModels { get; } = new ObservableList<MappingMonitor.MappingModel>();


		#region "Free API Translator"

		public static async Task<bool> XlateApiFree( RemoteType remoteType, string sourceText, RichTextBox rtbDst,
									CancellationToken cancelToken, IProgress<ProgressInfo> progress )
		{
			string text = sourceText;
			var result = false;

			try {
				RemoteAgents.DescendedModels = DescendedModels;
				RemoteAgents.MarkMappedTextWithHtmlBoldTag = MarkMappedTextWithHtmlBoldTag;
				rtbDst.Document.Blocks.Clear();

				var dispatcher = Application.Current.Dispatcher;
				IEnumerable<YieldResult> xlatorResults = null;

				switch( remoteType ) {
					case RemoteType.CrossLanguageFree:
						xlatorResults = RemoteAgents.TranslateByCrossLanguageFree( text, SourceLanguage,
												TargetLanguage, cancelToken, progress );
						break;
					case RemoteType.BaiduFree:
						xlatorResults = RemoteAgents.TranslateByBaiduFree( text, SourceLanguage,
												TargetLanguage, cancelToken, progress );
						break;
					case RemoteType.YoudaoFree:
						xlatorResults = RemoteAgents.TranslateByYoudaoFree( text, SourceLanguage,
												TargetLanguage, cancelToken, progress );
						break;
					case RemoteType.GoogleFree:
						xlatorResults = RemoteAgents.TranslateByGoogleFree( text, SourceLanguage,
												TargetLanguage, cancelToken, progress );
						break;

					case RemoteType.MicrosoftFree:
					default:
						// NOT SUPPORTED
						return false;
				}


				result = await Task.Run( () => {
					int lastPercent = 0;
					foreach( var section in xlatorResults ) {
						lastPercent = section.PercentOrErrorCode;
						var text2 = section.TranslatedSection;
						if( !dispatcher.CheckAccess() ) {
							dispatcher.BeginInvoke( (Action)delegate () {
								_AddText( rtbDst, text2 );
							} );
						}
						else {
							_AddText( rtbDst, text2 );
						}

					}
					return lastPercent < 100 ? false : true;
				}, cancelToken )
				.ConfigureAwait( true );

				// update CurrentUsedModels
				CurrentUsedModels.Clear();
				foreach( var model in RemoteAgents.CurrentUsedModels ) {
					CurrentUsedModels.Add( model );
				}
			}
			catch( Exception ex ) {
				_Report( progress, -1, string.Format( Languages.WebXlator.Str1TranslatingError, ex.Message ), ex );
				return false;
			}

			// finally, scroll to top
			if( AutoScrollToTop && rtbDst.LineCount() > 0 ) {
				rtbDst.ScrollToHome();
			}

			return result;
		}

		#endregion


		#region "Charged API Translator"

		public static async Task<bool> XlateApiCharged( RemoteType remoteType,
									string sourceText, RichTextBox rtbDst,
									CancellationToken cancelToken, IProgress<ProgressInfo> progress,
									string subOrApiKey, string secretKey = null,
									string preferHost = null, string preferRegion = null )
		{
			string text = sourceText;
			var result = false;

			try {
				RemoteAgents.DescendedModels = DescendedModels;
				RemoteAgents.MarkMappedTextWithHtmlBoldTag = MarkMappedTextWithHtmlBoldTag;
				rtbDst.Document.Blocks.Clear();

				var dispatcher = Application.Current.Dispatcher;
				IEnumerable<YieldResult> xlatorResults = null;

				switch( remoteType ) {
					//case RemoteType.CrossLanguageCharged:
					//	xlatorResults = WebAgents.TranslateByCrossLanguageCharged( text, SourceLanguage,
					//							TargetLanguage, cancelToken, progress );
					//	break;
					case RemoteType.BaiduCharged:
						xlatorResults = RemoteAgents.TranslateByBaiduCharged( text, SourceLanguage,
												TargetLanguage, cancelToken, progress, subOrApiKey, secretKey );
						break;
					case RemoteType.YoudaoCharged:
						xlatorResults = RemoteAgents.TranslateByYoudaoCharged( text, SourceLanguage,
												TargetLanguage, cancelToken, progress, subOrApiKey, secretKey );
						break;

					case RemoteType.GoogleCharged:
						xlatorResults = RemoteAgents.TranslateByGoogleCharged( text, SourceLanguage,
												TargetLanguage, cancelToken, progress, subOrApiKey );
						break;

					case RemoteType.MicrosoftCharged:
						xlatorResults = RemoteAgents.TranslateByMicrosoftCharged( text, SourceLanguage,
												TargetLanguage, cancelToken, progress, subOrApiKey,
												preferRegion, preferHost );
						break;
					default:
						// NOT SUPPORTED
						return false;
				}


				result = await Task.Run( () => {
					int lastPercent = 0;
					foreach( var section in xlatorResults ) {
						lastPercent = section.PercentOrErrorCode;
						var text2 = section.TranslatedSection;
						if( !dispatcher.CheckAccess() ) {
							dispatcher.BeginInvoke( (Action)delegate () {
								_AddText( rtbDst, text2 );
							} );
						}
						else {
							_AddText( rtbDst, text2 );
						}

					}
					return lastPercent < 100 ? false : true;
				}, cancelToken )
				.ConfigureAwait( true );

				// update CurrentUsedModels
				CurrentUsedModels.Clear();
				foreach( var model in RemoteAgents.CurrentUsedModels ) {
					CurrentUsedModels.Add( model );
				}
			}
			catch( Exception ex ) {
				_Report( progress, -1, string.Format( Languages.WebXlator.Str1TranslatingError, ex.Message ), ex );
				return false;
			}

			// finally, scroll to top
			if( AutoScrollToTop && rtbDst.LineCount() > 0 ) {
				rtbDst.ScrollToHome();
			}

			return result;
		}

		#endregion


		#region "Web Page Translators"

		public static async Task<bool> XlateExcitePage( WebBrowser browser, string sourceText, RichTextBox rtbDst,
								CancellationToken cancelToken, IProgress<ProgressInfo> progress )
		{
			string text = sourceText;

			try {
				RemoteAgents.DescendedModels = DescendedModels;
				RemoteAgents.MarkMappedTextWithHtmlBoldTag = MarkMappedTextWithHtmlBoldTag;
				rtbDst.Document.Blocks.Clear();

				var dispatcher = Application.Current.Dispatcher;

				var result = await Task.Run( () => {
					int lastPercent = 0;
					foreach( var section in RemoteAgents.TranslateByExcite( text, SourceLanguage,
											TargetLanguage, cancelToken, progress ) ) {
						lastPercent = section.PercentOrErrorCode;
						text = section.TranslatedSection;
						if( !dispatcher.CheckAccess() ) {
							dispatcher.BeginInvoke( (Action)delegate () {
								_AddText( rtbDst, text );
							} );
						}
						else {
							_AddText( rtbDst, text );
						}
					}

					return lastPercent < 100 ? false : true;
				}, cancelToken )
				.ConfigureAwait( true );

				// update CurrentUsedModels
				CurrentUsedModels.Clear();
				foreach( var model in RemoteAgents.CurrentUsedModels ) {
					CurrentUsedModels.Add( model );
				}
			}
			catch( Exception ex ) {
				_Report( progress, -1, string.Format( Languages.WebXlator.Str1TranslatingError, ex.Message ), ex );
				return false;
			}

			// finally, scroll to top
			if( AutoScrollToTop && rtbDst.LineCount() > 0 ) {
				rtbDst.ScrollToHome();
			}

			return true;
		}


		public static async Task<bool> XlateWeblioPage( WebBrowser browser, string sourceText, RichTextBox rtbDst,
									CancellationToken cancelToken, IProgress<ProgressInfo> progress )
		{
			string text = sourceText;

			try {
				RemoteAgents.DescendedModels = DescendedModels;
				RemoteAgents.MarkMappedTextWithHtmlBoldTag = MarkMappedTextWithHtmlBoldTag;
				rtbDst.Document.Blocks.Clear();

				var dispatcher = Application.Current.Dispatcher;

				var result = await Task.Run( () => {
					int lastPercent = 0;
					foreach( var section in RemoteAgents.TranslateByWeblio( text, SourceLanguage,
											TargetLanguage, cancelToken, progress ) ) {
						lastPercent = section.PercentOrErrorCode;
						text = section.TranslatedSection;
						if( !dispatcher.CheckAccess() ) {
							dispatcher.BeginInvoke( (Action)delegate () {
								_AddText( rtbDst, text );
							} );
						}
						else {
							_AddText( rtbDst, text );
						}
					}

					return lastPercent < 100 ? false : true;
				}, cancelToken )
				.ConfigureAwait( true );

				// update CurrentUsedModels
				CurrentUsedModels.Clear();
				foreach( var model in RemoteAgents.CurrentUsedModels ) {
					CurrentUsedModels.Add( model );
				}
			}
			catch( Exception ex ) {
				_Report( progress, -1, string.Format( Languages.WebXlator.Str1TranslatingError, ex.Message ), ex );
				return false;
			}

			// finally, scroll to top
			if( AutoScrollToTop && rtbDst.LineCount() > 0 ) {
				rtbDst.ScrollToHome();
			}

			return true;
		}

		public static async Task<bool> XlateCrossLangPage( WebBrowser browser, string sourceText, RichTextBox rtbDst,
								CancellationToken cancelToken, IProgress<ProgressInfo> progress )
		{
			string text = sourceText;

			// prepare default location, lang. code mapping refer to "http://cross.transer.com/js/translation-utilities.js?ver=2.0.1"
			string defLoc = sLocCrossLang, srcLang = "1", tgtLang = "14"; // from Japanese to ChineseTraditional
			switch( SourceLanguage ) {
				case SupportedSourceLanguage.English:
					srcLang = "2"; //"en";
					switch( TargetLanguage ) {
						case SupportedTargetLanguage.ChineseTraditional:
							tgtLang = "14"; // "zh-hant-n";
							break;
						default:
							return false;
					}
					break;
				case SupportedSourceLanguage.ChineseSimplified:
					srcLang = "13"; // "zh-hans-n";
					switch( TargetLanguage ) {
						case SupportedTargetLanguage.ChineseTraditional:
							tgtLang = "14"; //"zh-hant-n";
							break;
						case SupportedTargetLanguage.English:
							tgtLang = "2"; //"en";
							break;
						default:
							return false;
					}
					break;
				case SupportedSourceLanguage.ChineseTraditional:
					srcLang = "14"; // "zh-hant-n";
					switch( TargetLanguage ) {
						case SupportedTargetLanguage.English:
							tgtLang = "2"; //"en";
							break;
						default:
							return false;
					}
					break;
			}

			bool rst = await _WaitBrowserLoaded( browser, defLoc, RemoteType.CrossLanguage, 3 );
			if( rst == false ) {
				_Report( progress, -1, Languages.WebXlator.Str0NavigateFailed, string.Format( Languages.WebXlator.Str0FailedLocation, defLoc) );
				return false;
			}

			var sb = new StringBuilder( text );

			// replace some text to avoid collision
			if( SourceLanguage == SupportedSourceLanguage.Japanese ) {
				foreach( var (From, To) in Profiles.JapaneseEscapeHtmlText ) {
					sb.Replace( From, To );
				}
			}

			CurrentUsedModels.Clear();

			// Tulpe3 => <Original text, tmp. text, Mapping text>
			var tmpList = new List<(string OrigText, string TmpText, string MappingText)>();
			int num = rnd.Next( 10, 30 );
			foreach( var mm in DescendedModels ) {
				if( text.Contains( mm.OriginalText ) == false )
					continue;

				var k1 = string.Format( "{0}", 5.61359 + num++ );
				tmpList.Add( (mm.OriginalText, k1, mm.MappingText) );
				sb.Replace( mm.OriginalText, k1 );

				CurrentUsedModels.Add( mm );
			}

			if( cancelToken.IsCancellationRequested )
				return false;

			text = sb.ToString();
			sb.Clear();

			// slice to some sections
			List<string> sections = null;
			if( false == _SliceSections( text, Profiles.MaxWords.CrossLanguage, out sections ) ||
				sections == null || sections.Count <= 0 )
				return false;

			if( cancelToken.IsCancellationRequested )
				return false;

			_Report( progress, 1, Languages.WebXlator.Str0PreparingTranslation, null );

			rtbDst.Document.Blocks.Clear();

			// wait for clear button
			rst = await _WaitForSelector( browser, "a#source-erace", defLoc );
			if( rst == false ) {
				_Report( progress, -1, Languages.WebXlator.Str0NavigateFailed, string.Format( Languages.WebXlator.Str0FailedLocation, defLoc ) );
				browser.Navigate( defLoc );
				return false;
			}

			dynamic docObj = browser.Document;
			// assign from langauge and to language
			docObj.getElementById( "lng-from" ).value = srcLang;
			docObj.getElementById( "lng-to" ).value = tgtLang;
			await Task.Delay( 300 );

			int xlatedSectionCnt = 0;
			foreach( var section in sections ) {
				if( xlatedSectionCnt > 0 )
					await Task.Delay( rnd.Next( 1500, 3500 ) );

				if( cancelToken.IsCancellationRequested )
					return false;

				try {
					// clear old text!!
					browser.InvokeScript( "eval", new[] {
						$"document.querySelector('a#source-erace').click();",
					} );
					await Task.Delay( 300 );

					// fill original text
					var textArea = docObj.getElementById( "source" );
					textArea.innerText = section;

					sb.Clear();

					// perform translate
					browser.InvokeScript( "eval", new[] {
						$"document.querySelector('#btn-translate').click();",
					} );
					await Task.Delay( 300 );

					if( cancelToken.IsCancellationRequested )
						return false;

					rst = await _WaitForSelector( browser, "span.sentence", defLoc, 200 );
					if( rst == false ) {
						// extract Web Translator's error string DIV
						var tstrLen = (int)browser.InvokeScript( "eval", new[] {
								@"document.querySelectorAll('div#org-error').length;"
							} );

						var errStr = Languages.WebXlator.Str0FailedTranslatedRetry;
						if( tstrLen > 0 ) {
							errStr += Languages.WebXlator.Str0ErrorMessage + (string)browser.InvokeScript( "eval", new[] {
												@"document.querySelector('div#org-error').textContent;"
											} );
						}

						_Report( progress, -1, errStr, section );
						return false;
					}

					// originalText has been translted
					var tstr = (string)browser.InvokeScript( "eval", new string[] {
									@"document.querySelector('#rep-layer').innerText;",
								} );

					if( string.IsNullOrWhiteSpace( tstr ) ) {
						continue;
					}

					// extract translated strings from span with class='sentence'
					sb.AppendLine( tstr );
					if( cancelToken.IsCancellationRequested )
						return false;

					// filter out useless html code
					sb.Replace( "&#010;", Environment.NewLine );
					foreach( var tuple2 in Profiles.HtmlCodeRecoveryText )
						sb.Replace( tuple2.From, tuple2.To );

					// replace some CHS char. from Baidu translated string
					if( TargetLanguage == SupportedTargetLanguage.ChineseTraditional ) {
						foreach( var (From, To) in Profiles.CrossLanguageXlationAfter2Cht )
							sb.Replace( From, To );
					}

					if( MarkMappedTextWithHtmlBoldTag ) {
						foreach( var tuple3 in tmpList )
							sb.Replace( tuple3.TmpText, $"<b>{tuple3.MappingText}</b>" );
					}
					else {
						foreach( var tuple3 in tmpList )
							sb.Replace( tuple3.TmpText, tuple3.MappingText );
					}
					_AddText( rtbDst, sb.ToString() );

					_Report( progress, (int)(++xlatedSectionCnt * 100 / sections.Count),
						string.Format( Languages.WebXlator.Str2TranslatedFractions, xlatedSectionCnt, sections.Count ), section );
				}
				catch( OperationCanceledException ) {
					throw; // re-throw to outside 
				}
				catch( Exception ex ) {
					_Report( progress, -1, string.Format( Languages.WebXlator.Str1TranslatingError, ex.Message ), ex );
					return false;
				}
			}

			// finally, scroll to top
			if( AutoScrollToTop && rtbDst.LineCount() > 0 ) {
				rtbDst.ScrollToHome();
			}

			return true;
		}


		public static async Task<bool> XlateBaiduPage( WebBrowser browser, string sourceText, RichTextBox rtbDst,
									CancellationToken cancelToken, IProgress<ProgressInfo> progress )
		{
			string text = sourceText;

			// prepare default location
			string defLoc = sLocBaiduCht;
			switch( TargetLanguage ) {
				case SupportedTargetLanguage.English:
					defLoc = sLocBaiduEn;
					break;
			}

			// step 0: load default location, Host is "fanyi.baidu.com"
			bool rst = await _WaitBrowserLoaded( browser, defLoc, RemoteType.Baidu, 3 );
			if( rst == false ) {
				_Report( progress, -1, Languages.WebXlator.Str0NavigateFailed, string.Format( Languages.WebXlator.Str0FailedLocation, defLoc ) );
				return false;
			}

			cancelToken.ThrowIfCancellationRequested();

			var sb = new StringBuilder( text );

			// replace some text to avoid collision
			if( SourceLanguage == SupportedSourceLanguage.Japanese ) {
				foreach( var (From, To) in Profiles.JapaneseEscapeHtmlText ) {
					sb.Replace( From, To );
				}
			}

			CurrentUsedModels.Clear();

			// Tulpe3 => <Original text, tmp. text, Mapping text>
			var tmpList = new List<(string OrigText, string TmpText, string MappingText)>();
			int num = rnd.Next( 10, 30 );
			foreach( var mm in DescendedModels ) {
				if( text.Contains( mm.OriginalText ) == false )
					continue;

				var k1 = string.Format( "θabcde◎{0}λ", num++ );
				var k2 = $"{{{k1}}}";
				tmpList.Add( (mm.OriginalText, k2, mm.MappingText) );
				sb.Replace( mm.OriginalText, k2 );

				var tmpstr = sb.ToString();
				if( tmpstr.Contains( $"…{k2}" ) || tmpstr.Contains( $"【{k2}" ) ||
					tmpstr.Contains( $"{k2}】" ) || tmpstr.Contains( $"に{k2}" ) ||
					tmpstr.Contains( $"は{k2}" ) ) {
					tmpList.Add( (mm.OriginalText, k1, mm.MappingText) );
				}

				CurrentUsedModels.Add( mm );
			}
			cancelToken.ThrowIfCancellationRequested();

			text = sb.ToString();
			sb.Clear();

			// slice to some sections
			List<string> sections = null;
			if( false == _SliceSections( text, Profiles.MaxWords.Baidu, out sections ) ||
				sections == null || sections.Count <= 0 )
				return false;

			cancelToken.ThrowIfCancellationRequested();
			_Report( progress, 1, Languages.WebXlator.Str0PreparingTranslation, null );

			rtbDst.Document.Blocks.Clear();

			rst = await _WaitForSelector( browser, "a.textarea-clear-btn", defLoc );
			if( rst == false ) {
				_Report( progress, -1, Languages.WebXlator.Str0NavigateFailed, string.Format( Languages.WebXlator.Str0FailedLocation, defLoc ) );
				browser.Navigate( defLoc );
				return false;
			}

			cancelToken.ThrowIfCancellationRequested();

			// the a.textarea-clear-btn's style would null when first and un-finished loading...
			rst = await _WaitForStyle( browser, "a.textarea-clear-btn", defLoc );
			if( rst != true ) {
				_Report( progress, -1, Languages.WebXlator.Str0NavigateFailed, string.Format( Languages.WebXlator.Str0FailedLocation, defLoc ) );
				return false;
			}

			int xlatedSectionCnt = 0;
			foreach( var section in sections ) {
				if( xlatedSectionCnt > 0 )
					await Task.Delay( rnd.Next( 1500, 3500 ) );

				cancelToken.ThrowIfCancellationRequested();

				try {
					// clear old text!!
					browser.InvokeScript( "eval", new string[] {
						$"document.querySelector('a.textarea-clear-btn').click();",
					} );
					await Task.Delay( 300 );

					// fill original text
					dynamic docObj = browser.Document;

					// wait clear button hidden
					var clearBtn = docObj.querySelector( "a.textarea-clear-btn" );
					int retry = 20;
					while( "none" != clearBtn.style.display ) {
						if( --retry <= 0 )
							break;

						cancelToken.ThrowIfCancellationRequested();
						await Task.Delay( 200 );
					}

					var textArea = docObj.getElementById( "baidu_translate_input" );
					textArea.innerText = section;

					sb.Clear();
					await Task.Delay( 2000 );

					cancelToken.ThrowIfCancellationRequested();
					rst = await _WaitForSelector( browser, "p.ordinary-output.target-output.clearfix", defLoc, 200 );
					if( rst == false ) {
						// extract Web Translator's error string DIV
						var tstrLen = (int)browser.InvokeScript( "eval", new[] {
								@"document.querySelectorAll('div.long-text-prompt-wrap').length;"
							} );

						var errStr = Languages.WebXlator.Str0FailedTranslatedRetry;
						if( tstrLen > 0 ) {
							errStr += Languages.WebXlator.Str0ErrorMessage + (string)browser.InvokeScript( "eval", new[] {
												@"document.querySelector('div.long-text-prompt-wrap').textContent;"
											} );
						}

						cancelToken.ThrowIfCancellationRequested();
						_Report( progress, -1, errStr, section );
						return false;
					}

					// originalText has been translted
					var tstr = (string)browser.InvokeScript( "eval", new string[] {
									@"document.querySelector('div.output-mod.ordinary-wrap > div.output-bd').innerHTML;",
								} );

					if( string.IsNullOrWhiteSpace( tstr ) ) {
						continue;
					}

					// extract translated strings with NOT class='ordinary-output source-output'
					mHtmlDoc.LoadHtml( tstr );
					foreach( HtmlNode node in mHtmlDoc.DocumentNode.SelectNodes( "//p[not(contains(@class, 'ordinary-output source-output'))]" ) ) {
						if( node.HasClass( "clearfix" ) ) // one translated line in whole text
							sb.AppendLine( node.InnerText );
						else if( node.SelectSingleNode( "span" ) != null ) // ignore useless lines
							continue;
						else
							sb.Append( node.InnerHtml.Replace( "<br>", Environment.NewLine ) );
					}

					cancelToken.ThrowIfCancellationRequested();

					// filter out useless html code
					sb.Replace( "&#010;", Environment.NewLine );
					foreach( var tuple2 in Profiles.HtmlCodeRecoveryText )
						sb.Replace( tuple2.From, tuple2.To );

					// replace some CHS char. from Baidu translated string
					if( TargetLanguage == SupportedTargetLanguage.ChineseTraditional ) {
						foreach( var (From, To) in Profiles.BaiduXlationAfter2Cht )
							sb.Replace( From, To );
					}

					if( MarkMappedTextWithHtmlBoldTag ) {
						foreach( var tuple3 in tmpList )
							sb.Replace( tuple3.TmpText, $"<b>{tuple3.MappingText}</b>" );
					}
					else {
						foreach( var tuple3 in tmpList )
							sb.Replace( tuple3.TmpText, tuple3.MappingText );
					}
					_AddText( rtbDst, sb.ToString() );

					_Report( progress, (int)(++xlatedSectionCnt * 100 / sections.Count),
						string.Format( Languages.WebXlator.Str2TranslatedFractions, xlatedSectionCnt, sections.Count ), section );
				}
				catch( OperationCanceledException ) {
					throw; // re-throw to outside 
				}
				catch( Exception ex ) {
					_Report( progress, -1, string.Format( Languages.WebXlator.Str1TranslatingError, ex.Message ), ex );
					return false;
				}
			}

			// finally, scroll to top
			if( AutoScrollToTop && rtbDst.LineCount() > 0 ) {
				rtbDst.ScrollToHome();
			}

			return true;
		}

		public static async Task<bool> XlateYoudaoPage( WebBrowser browser, string sourceText, RichTextBox rtbDst,
									CancellationToken cancelToken, IProgress<ProgressInfo> progress )
		{
			string text = sourceText;

			// https://ai.youdao.com/product-fanyi.s 
			// prepare default location
			string defLoc = sLocYoudaoCht;
			switch( TargetLanguage ) {
				case SupportedTargetLanguage.English: // Youdao NOT support JA2EN
					//defLoc = sLocYoudaoEn;
					//break;
					progress?.Report( new ProgressInfo {
						PercentOrErrorCode = -1,
						Message = string.Format( Languages.WebXlator.Str2YoudaoNotSupportSrc2Tgt, SourceLanguage.ToL10nString(), TargetLanguage.ToL10nString() )
					} );
					return false;
			}

			// only support Chinese ==> English by Youdao!!
			if( TargetLanguage == SupportedTargetLanguage.English && SourceLanguage != SupportedSourceLanguage.ChineseSimplified )
				return false;

			// step 0: load default location, Host is "fanyi.youdao.com"
			bool rst = await _WaitBrowserLoaded( browser, defLoc, RemoteType.Youdao, 3 );
			if( rst == false ) {
				_Report( progress, -1, Languages.WebXlator.Str0NavigateFailed, string.Format( Languages.WebXlator.Str0FailedLocation, defLoc ) );
				return false;
			}

			cancelToken.ThrowIfCancellationRequested();

			var sb = new StringBuilder( text );

			// replace some text to avoid collision
			if( SourceLanguage == SupportedSourceLanguage.Japanese ) {
				foreach( var (From, To) in Profiles.JapaneseEscapeHtmlText ) {
					sb.Replace( From, To );
				}
			}

			CurrentUsedModels.Clear();

			// Tulpe3 => <Original text, tmp. text, Mapping text>
			var tmpList = new List<(string OrigText, string TmpText, string MappingText)>();
			int num = rnd.Next( 10, 30 );
			foreach( var mm in DescendedModels ) {
				if( text.Contains( mm.OriginalText ) == false )
					continue;

				var k1 = $"(abc0{num++})";
				tmpList.Add( (mm.OriginalText, k1, mm.MappingText) );
				sb.Replace( mm.OriginalText, k1 );

				CurrentUsedModels.Add( mm );
			}
			cancelToken.ThrowIfCancellationRequested();

			text = sb.ToString();
			sb.Clear();

			// slice to some sections
			List<string> sections = null;
			if( false == _SliceSections( text, Profiles.MaxWords.Youdao, out sections ) ||
				sections == null || sections.Count <= 0 )
				return false;

			cancelToken.ThrowIfCancellationRequested();
			_Report( progress, 1, Languages.WebXlator.Str0PreparingTranslation, null );

			rtbDst.Document.Blocks.Clear();

			// wait for textarea inputOriginal loaded
			rst = await _WaitForSelector( browser, "#inputOriginal", defLoc );
			if( rst == false ) {
				_Report( progress, -1, Languages.WebXlator.Str0NavigateFailed, string.Format( Languages.WebXlator.Str0FailedLocation, defLoc ) );
				return false;
			}

			int xlatedSectionCnt = 0;
			foreach( var section in sections ) {
				if( xlatedSectionCnt > 0 )
					await Task.Delay( rnd.Next( 1500, 3500 ) );

				cancelToken.ThrowIfCancellationRequested();

				try {
					// clear old text!!
					rst = await _WaitForSelector( browser, "#inputDelete", defLoc );
					if( rst == false ) {
						_Report( progress, -1, Languages.WebXlator.Str0FailedClearOldTextRetry, section );
						return false;
					}


					// the inputDelete's style would null when first and un-finished loading...
					rst = await _WaitForStyle( browser, "a#inputDelete", defLoc );
					if( rst != true ) {
						_Report( progress, -1, Languages.WebXlator.Str0NavigateFailed, string.Format( Languages.WebXlator.Str0FailedLocation, defLoc ) );
						return false;
					}

					dynamic docObj = browser.Document;
					var clearBtn = docObj.getElementById( "inputDelete" );
					if( clearBtn.style.display == "inline" ) {
						clearBtn.click();
						await Task.Delay( 200 );
					}

					cancelToken.ThrowIfCancellationRequested();

					// fill original text
					var textArea = docObj.getElementById( "inputOriginal" );
					textArea.innerText = section;

					sb.Clear();
					await Task.Delay( 1000 );

					rst = await _WaitForSelector( browser, "div#transTarget > p", defLoc, 100 );
					if( rst == false ) {
						// extract Web Translator's error string DIV
						var errBlock = docObj.getElementById( "inputTargetError" );
						var errStr = Languages.WebXlator.Str0FailedTranslatedRetry;
						if( "none" != errBlock.style.display ) {
							errStr += Languages.WebXlator.Str0ErrorMessage + errBlock.textContent;
						}

						_Report( progress, -1, errStr, section );
						return false;
					}

					var tstr = (string)browser.InvokeScript( "eval", new string[] {
									@"document.querySelector('div#transTarget').innerHTML;",
								} );

					if( string.IsNullOrWhiteSpace( tstr ) ) {
						continue;
					}

					cancelToken.ThrowIfCancellationRequested();

					// extract translated strings with "<p><span data-section="0">xxxxx</span></p>" in transTarget
					mHtmlDoc.LoadHtml( tstr );

					var lines = section.Split( sNewLineSeparator, StringSplitOptions.None );
					int origIndex = 0;
					foreach( HtmlNode node in mHtmlDoc.DocumentNode.SelectNodes( "//p" ) ) {
						if( string.IsNullOrWhiteSpace( node.InnerText ) ) // empty line
							continue;

						for( int i = origIndex; i < lines.Length; ++i ) {
							if( string.IsNullOrEmpty( lines[i] ) ) {
								sb.Append( Environment.NewLine );
								continue;
							}

							foreach( var ch in lines[i] ) {
								if( char.IsWhiteSpace( ch ) || ch == '　' )
									sb.Append( ch );
								else
									break;
							}

							if( TargetLanguage == SupportedTargetLanguage.ChineseTraditional )
								sb.AppendLine( ChineseConverter.Convert( node.InnerText,
												ChineseConversionDirection.SimplifiedToTraditional ) );
							else
								sb.AppendLine( node.InnerText );
							origIndex = i + 1;
							break;
						}
					}

					cancelToken.ThrowIfCancellationRequested();

					// filter out useless html code
					sb.Replace( "&#010;", Environment.NewLine );
					foreach( var tuple2 in Profiles.HtmlCodeRecoveryText )
						sb.Replace( tuple2.From, tuple2.To );

					// replace some CHS char. from Youdao translated string
					if( TargetLanguage == SupportedTargetLanguage.ChineseTraditional ) {
						foreach( var (From, To) in Profiles.YoudaoXlationAfter2Cht )
							sb.Replace( From, To );

						// replace some CHS char. after MS CHT-CHS converter string
						foreach( var (From, To) in Profiles.XlationAfterMsConvert2Cht )
							sb.Replace( From, To );
					}

					// Regex replace mis-translated temp. string
					string regex1 = @"[(]?abc0(?<SeqNum>[0-9]+)[)]?"; // $"(abc0{num++})";
					tstr = System.Text.RegularExpressions.Regex.Replace( sb.ToString(), regex1, "(abc0${SeqNum})" );
					if( string.IsNullOrWhiteSpace( tstr ) == false ) {
						sb.Clear();
						sb.Append( tstr );
					}

					if( MarkMappedTextWithHtmlBoldTag ) {
						foreach( var tuple3 in tmpList )
							sb.Replace( tuple3.TmpText, $"<b>{tuple3.MappingText}</b>" );
					}
					else {
						foreach( var tuple3 in tmpList )
							sb.Replace( tuple3.TmpText, tuple3.MappingText );
					}
					_AddText( rtbDst, sb.ToString() );

					_Report( progress, (int)(++xlatedSectionCnt * 100 / sections.Count),
						string.Format( Languages.WebXlator.Str2TranslatedFractions, xlatedSectionCnt, sections.Count ), section );
				}
				catch( OperationCanceledException ) {
					throw; // re-throw to outside 
				}
				catch( Exception ex ) {
					_Report( progress, -1, string.Format( Languages.WebXlator.Str1TranslatingError, ex.Message ), ex );
					return false;
				}
			}

			// finally, scroll to top
			if( AutoScrollToTop && rtbDst.LineCount() > 0 ) {
				rtbDst.ScrollToHome();
			}

			return true;
		}

		public static async Task<bool> XlateGooglePage( WebBrowser browser, string sourceText, RichTextBox rtbDst,
									CancellationToken cancelToken, IProgress<ProgressInfo> progress )
		{
			string text = sourceText;

			// prepare default location, default Host is "translate.google.com"
			string defLocPrefix = sLocGoogleChtPrefix;
			switch( TargetLanguage ) {
				case SupportedTargetLanguage.English:
					defLocPrefix = sLocGoogleEnPrefix;
					break;
			}

			var sb = new StringBuilder( text );
			// replace some text to avoid collision
			if( SourceLanguage == SupportedSourceLanguage.Japanese ) {
				foreach( var (From, To) in Profiles.JapaneseEscapeHtmlText ) {
					sb.Replace( From, To );
				}
			}

			CurrentUsedModels.Clear();

			// Tulpe3 => <Original text, tmp. text, Mapping text>
			var tmpList = new List<(string OrigText, string TmpText, string MappingText)>();
			int num = rnd.Next( 10, 30 );
			foreach( var mm in DescendedModels ) {
				if( text.Contains( mm.OriginalText ) == false )
					continue;

				var k1 = $"[{num++}]";
				tmpList.Add( (mm.OriginalText, k1, mm.MappingText) );
				sb.Replace( mm.OriginalText, k1 );

				CurrentUsedModels.Add( mm );
			}
			cancelToken.ThrowIfCancellationRequested();

			text = sb.ToString();
			sb.Clear();

			// slice to some encoded uri text sections
			List<string> sections = null;
			// force slice sections to very small pieces for escaped URL string 
			//if( false == _SliceUrl2EncodedSections( defLocPrefix, text, 1900, out sections ) ||
			if( false == _SliceSections( text, (1900 - defLocPrefix.Length) / 9, out sections ) ||
				sections == null || sections.Count <= 0 )
				return false;

			cancelToken.ThrowIfCancellationRequested();
			_Report( progress, 1, Languages.WebXlator.Str0PreparingTranslation, null );

			rtbDst.Document.Blocks.Clear();

			int xlatedSectionCnt = 0;
			foreach( var section in sections ) {
				sb.Clear();
				if( xlatedSectionCnt > 0 ) {
					sb.AppendLine();

					await Task.Delay( rnd.Next( 1500, 2500 ) );
				}

				cancelToken.ThrowIfCancellationRequested();

				try {
					browser.Navigate( defLocPrefix + Uri.EscapeDataString( section ) );
					await Task.Delay( rnd.Next( 1800, 2200 ) );

					// check output option bar is appear
					var jsQueryOutOptBarCnt = new string[] { @"document.querySelectorAll('div.tlid-result.result-dict-wrapper').length;" };
					int outOptBarCnt = (int)browser.InvokeScript( "eval", jsQueryOutOptBarCnt ), checkRetry = 50;
					while( outOptBarCnt <= 0 ) {
						// Failed to translated strings, retried too manay times
						if( checkRetry-- <= 0 ) {
							// extract Web Translator's error string by overflow
							// gt-ovfl-ctr > gt-ovfl-msg, gt-ovfl-ctr > gt-ovfl-xlt
							var errStr = Languages.WebXlator.Str0FailedTranslatedRetry;

							_Report( progress, -1, errStr, section );
							return false;
						}
						await Task.Delay( rnd.Next( 150, 250 ) );
						outOptBarCnt = (int)browser.InvokeScript( "eval", jsQueryOutOptBarCnt );
					}

					var tstr = (string)browser.InvokeScript( "eval", new string[] {
									@"document.querySelector('span.tlid-translation.translation').innerHTML;",
								} );

					if( string.IsNullOrWhiteSpace( tstr ) ) {
						continue;
					}

					cancelToken.ThrowIfCancellationRequested();

					if( tstr.StartsWith( "<span" ) ) {
						mHtmlDoc.LoadHtml( tstr );
						foreach( HtmlNode node in mHtmlDoc.DocumentNode.SelectNodes( "//*" ) ) {
							if( node.Name == "span" )
								sb.Append( node.InnerText );
							else if( node.Name == "br" )
								sb.AppendLine();
						}
					}
					else {
						sb.Append( tstr.Replace( "<br>", Environment.NewLine ) );
					}

					cancelToken.ThrowIfCancellationRequested();

					// prepare translated strings by mapping to original lines

					// filter out useless html code
					sb.Replace( "&#010;", Environment.NewLine );
					foreach( var tuple2 in Profiles.HtmlCodeRecoveryText )
						sb.Replace( tuple2.Item1, tuple2.Item2 );

					// replace some CHS char. from Google translated string
					if( TargetLanguage == SupportedTargetLanguage.ChineseTraditional ) {
						foreach( var (From, To) in Profiles.GoogleXlationAfter2Cht )
							sb.Replace( From, To );
					}

					// replace mis-translated text
					sb.Replace( " ]", "]" );
					sb.Replace( "[ ", "[" );

					if( MarkMappedTextWithHtmlBoldTag ) {
						foreach( var tuple3 in tmpList )
							sb.Replace( tuple3.TmpText, $"<b>{tuple3.MappingText}</b>" );
					}
					else {
						foreach( var tuple3 in tmpList )
							sb.Replace( tuple3.TmpText, tuple3.MappingText );
					}
					_AddText( rtbDst, sb.ToString() );

					_Report( progress, (int)(++xlatedSectionCnt * 100 / sections.Count),
						string.Format( Languages.WebXlator.Str2TranslatedFractions, xlatedSectionCnt, sections.Count ), section );
				}
				catch( OperationCanceledException ) {
					throw; // re-throw to outside 
				}
				catch( Exception ex ) {
					_Report( progress, -1, string.Format( Languages.WebXlator.Str1TranslatingError, ex.Message ), ex );
					return false;
				}
			}

			// finally, scroll to top
			if( AutoScrollToTop && rtbDst.LineCount() > 0 ) {
				rtbDst.ScrollToHome();
			}

			return true;
		}


		public static async Task<bool> XlateMicrosoftPage( WebBrowser browser, string sourceText, RichTextBox rtbDst,
									CancellationToken cancelToken, IProgress<ProgressInfo> progress )
		{
			string text = sourceText;

			// prepare default location
			string defLoc = sLocBingCht;
			switch( TargetLanguage ) {
				case SupportedTargetLanguage.English:
					defLoc = sLocBingEn;
					break;
			}

			// step 0: load default location, Host is "www.bing.com/translator"
			if( browser.Source == null || browser.Source.OriginalString == "about:blank" ||
				browser.Source.Host != Profiles.DefaultHosts[RemoteType.Microsoft] ||
				browser.Source.AbsolutePath.StartsWith( "/translator" ) == false ) {
				browser.Navigate( defLoc );
				await Task.Delay( 3000 );
			}

			cancelToken.ThrowIfCancellationRequested();

			var sb = new StringBuilder( text );

			// replace some text to avoid collision
			if( SourceLanguage == SupportedSourceLanguage.Japanese ) {
				foreach( var (From, To) in Profiles.JapaneseEscapeHtmlText ) {
					sb.Replace( From, To );
				}
			}

			CurrentUsedModels.Clear();

			// Tulpe3 => <Original text, tmp. text, Mapping text>
			var tmpList = new List<(string OrigText, string TmpText, string MappingText)>();
			int num = rnd.Next( 10, 30 );
			foreach( var mm in DescendedModels ) {
				if( text.Contains( mm.OriginalText ) == false )
					continue;

				var k1 = $"www.zks{num++}.org"; // seems good, but some exceptions...so need regex replacement
				tmpList.Add( (mm.OriginalText, k1, mm.MappingText) );
				sb.Replace( mm.OriginalText, k1 );

				CurrentUsedModels.Add( mm );
			}
			cancelToken.ThrowIfCancellationRequested();

			text = sb.ToString();
			sb.Clear();

			// slice to some sections
			List<string> sections = null;
			if( false == _SliceSections( text, Profiles.MaxWords.Bing, out sections ) ||
				sections == null || sections.Count <= 0 )
				return false;

			cancelToken.ThrowIfCancellationRequested();
			_Report( progress, 1, Languages.WebXlator.Str0PreparingTranslation, null );

			rtbDst.Document.Blocks.Clear();

			int xlatedSectionCnt = 0;
			foreach( var section in sections ) {
				// Hmm, Bing Translator seems track user's total translation words in somewhere to prevent overloading...
				// Therefore, after translated too many words, Bing Translator would show error messages!!
				if( xlatedSectionCnt > 0 )
					await Task.Delay( rnd.Next( 2500, 4500 ) );

				cancelToken.ThrowIfCancellationRequested();

				try {
					// clear old text!!
					browser.InvokeScript( "eval", new[] {
						@"document.getElementById('t_edc').click();",
					} );

					await Task.Delay( 500 );

					dynamic docObj = browser.Document;
					var textArea = docObj.getElementById( "t_sv" );
					// assign original text to t_sv, tta_input
					textArea.value = section;
					// trigger 'keyup' event of t_sv to refresh t_tv, tta_output
					var evt2 = docObj.createEvent( "HTMLEvents" );
					evt2.initEvent( "keyup", true, true );
					textArea.dispatchEvent( evt2 );
					cancelToken.ThrowIfCancellationRequested();
					await Task.Delay( 2000 );

					// check output option bar is appear
					var jsQueryOutOptBarCnt = new string[] { @"document.querySelectorAll('div#t_outoption.t_outputoptions.b_hide').length;" };
					int outOptBarCnt = (int)browser.InvokeScript( "eval", jsQueryOutOptBarCnt ), checkRetry = 50;
					while( outOptBarCnt > 0 ) {
						// Failed to translated strings, retried too manay times
						if( checkRetry-- <= 0 ) {
							// extract Web Translator's error string DIV
							outOptBarCnt = (int)browser.InvokeScript( "eval", new[] {
								@"document.querySelectorAll('div#t_err.b_hide').length;"
							} );

							var errStr = Languages.WebXlator.Str0FailedTranslatedRetry;
							if( outOptBarCnt <= 0 )
								errStr += Languages.WebXlator.Str0ErrorMessage + docObj.getElementById( "t_err" ).textContent + " ";

							_Report( progress, -1, errStr, section );
							return false;
						}
						await Task.Delay( rnd.Next( 150, 250 ) );
						outOptBarCnt = (int)browser.InvokeScript( "eval", jsQueryOutOptBarCnt );
					}

					var tstr = (string)browser.InvokeScript( "eval", new string[] {
									@"document.querySelector('textarea#t_tv').textContent;",
								} );

					if( string.IsNullOrWhiteSpace( tstr ) ) {
						continue;
					}

					cancelToken.ThrowIfCancellationRequested();
					sb.Clear();

					// prepare translated strings by mapping to original lines
					var origLines = section.Split( sNewLineSeparator, StringSplitOptions.None );
					int origIndex = 0;
					foreach( var data in tstr.Split( sNewLineSeparator, StringSplitOptions.None ) ) {
						if( string.IsNullOrWhiteSpace( data ) )
							continue;

						// found a solid text line, so mapping it to original line
						for( int i = origIndex; i < origLines.Length; ++i ) {
							if( string.IsNullOrEmpty( origLines[i] ) ) {
								sb.Append( Environment.NewLine );
								continue;
							}

							// check original string start with whitespace or Fullwidth form space
							foreach( var ch in origLines[i] ) {
								if( char.IsWhiteSpace( ch ) || '　' == ch )
									sb.Append( ch );
								else
									break;
							}

							sb.Append( data + Environment.NewLine );

							origIndex = i + 1;
							break;
						}
					}

					// filter out useless html code
					sb.Replace( "&#010;", Environment.NewLine );
					foreach( var tuple2 in Profiles.HtmlCodeRecoveryText )
						sb.Replace( tuple2.Item1, tuple2.Item2 );

					// replace some CHS char. from Bing translated string
					if( TargetLanguage == SupportedTargetLanguage.ChineseTraditional ) {
						foreach( var (From, To) in Profiles.MicrosoftXlationAfter2Cht )
							sb.Replace( From, To );
					}

					// Regex replace mis-translated temp. string
					string regex1 = @"[wW]{3}[ .]+zks(?<SeqNum>[0-9]+)[ .]+.org";
					tstr = System.Text.RegularExpressions.Regex.Replace(
								sb.ToString(), regex1, "www.zks${SeqNum}.org",
								System.Text.RegularExpressions.RegexOptions.IgnoreCase );
					if( string.IsNullOrWhiteSpace( tstr ) == false ) {
						sb.Clear();
						sb.Append( tstr );
					}

					if( MarkMappedTextWithHtmlBoldTag ) {
						foreach( var tuple3 in tmpList )
							sb.Replace( tuple3.TmpText, $"<b>{tuple3.MappingText}</b>" );
					}
					else {
						foreach( var tuple3 in tmpList )
							sb.Replace( tuple3.TmpText, tuple3.MappingText );
					}
					_AddText( rtbDst, sb.ToString() );

					_Report( progress, (int)(++xlatedSectionCnt * 100 / sections.Count),
						string.Format( Languages.WebXlator.Str2TranslatedFractions, xlatedSectionCnt, sections.Count ), section );
				}
				catch( OperationCanceledException ) {
					throw; // re-throw to outside 
				}
				catch( Exception ex ) {
					_Report( progress, -1, string.Format( Languages.WebXlator.Str1TranslatingError, ex.Message ), ex );
					return false;
				}
			}

			// finally, scroll to top
			if( AutoScrollToTop && rtbDst.LineCount() > 0 ) {
				rtbDst.ScrollToHome();
			}

			return true;
		}


		private static SemaphoreSlim sSemaBrowser = null;
		private static volatile bool sBrowserLoaded = false;
		private static void WebBrowser_LoadCompleted( object sender, System.Windows.Navigation.NavigationEventArgs e )
		{
			sBrowserLoaded = true;
			sSemaBrowser.Release();
		}

		private static async Task<bool> _WaitBrowserLoaded( WebBrowser browser, string location,
											RemoteType remoteType, int timeOutInSeconds )
		{
			if( browser == null || string.IsNullOrWhiteSpace( location ) )
				return false;

			if( remoteType == RemoteType.Google ) {
				browser.LoadCompleted -= WebBrowser_LoadCompleted;
				browser.LoadCompleted += WebBrowser_LoadCompleted;

				await sSemaBrowser.WaitAsync( TimeSpan.FromSeconds( timeOutInSeconds ) );
				browser.LoadCompleted -= WebBrowser_LoadCompleted;
				await Task.Delay( 1500 );

				if( sBrowserLoaded == true && browser.Source.OriginalString == location )
					return true;
				return sBrowserLoaded;
			}

			if( browser.Source == null || browser.Source.OriginalString == "about:blank" ||
				browser.Source.Host != Profiles.DefaultHosts[remoteType] ) {

				sBrowserLoaded = false;
				if( sSemaBrowser != null )
					sSemaBrowser.Dispose();
				sSemaBrowser = new SemaphoreSlim( 0, 1 );

				browser.LoadCompleted -= WebBrowser_LoadCompleted;
				browser.LoadCompleted += WebBrowser_LoadCompleted;
				browser.Navigate( location );

				await sSemaBrowser.WaitAsync( TimeSpan.FromSeconds( timeOutInSeconds ) );
				browser.LoadCompleted -= WebBrowser_LoadCompleted;

				await Task.Delay( 1500 );
				return sBrowserLoaded;
			}

			return true;
		}

		/// <summary>
		/// Wait for CSS-style selector to appear
		/// </summary>
		/// <param name="browser">Target web browser</param>
		/// <param name="cssSelector">Checking CSS selector</param>
		/// <param name="defaultLocation">Default location when out-of-retry</param>
		/// <param name="retry">Retry count</param>
		/// <returns>true for success</returns>
		private static async Task<bool> _WaitForSelector( WebBrowser browser, string cssSelector, string defaultLocation, int retry = 50 )
		{
			var jsQuerySelectorCnt = new string[] {
				string.Format( @"document.querySelectorAll('{0}').length;", cssSelector ),
			};
			int selectorCnt = (int)browser.InvokeScript( "eval", jsQuerySelectorCnt );
			while( selectorCnt <= 0 ) {
				if( retry-- <= 0 ) {
					if( defaultLocation != null )
						browser.Navigate( defaultLocation );
					return false;
				}

				await Task.Delay( rnd.Next( 200, 300 ) );
				selectorCnt = (int)browser.InvokeScript( "eval", jsQuerySelectorCnt );
			}

			return true;
		}

		/// <summary>
		/// Wait for style element appear
		/// </summary>
		/// <param name="browser">Target web browser</param>
		/// <param name="cssSelector">Checking CSS selector</param>
		/// <param name="defaultLocation">Default location when out-of-retry</param>
		/// <param name="retry">Retry count</param>
		/// <returns>true for success</returns>
		private static async Task<bool> _WaitForStyle( WebBrowser browser, string cssSelector, string defaultLocation, int retry = 50 )
		{
			var jsQuerySelectorStyle = new string[] { $"document.querySelector('{cssSelector}').getAttribute('style');" };
			var selectorStyle = browser.InvokeScript( "eval", jsQuerySelectorStyle ) as string;
			while( string.IsNullOrEmpty( selectorStyle ) ) {
				if( retry-- <= 0 ) {
					browser.Navigate( defaultLocation );
					return false;
				}

				await Task.Delay( rnd.Next( 250, 450 ) );
				selectorStyle = browser.InvokeScript( "eval", jsQuerySelectorStyle ) as string;
			}

			return true;
		}

		#endregion // "Web Page Translators"

		// WebView/WebBrowser variables
		//private static readonly string sLocExciteCht = "https://www.excite.co.jp/world/fantizi/";
		//private static readonly string sLocExciteEn = "https://www.excite.co.jp/world/english/";
		//private static readonly string sLocWeblioCht = "https://translate.weblio.jp/chinese/";
		//private static readonly string sLocWeblioEn = "https://translate.weblio.jp/";
		private static readonly string sLocCrossLang = "http://cross.transer.com/";
		private static readonly string sLocBaiduCht = "https://fanyi.baidu.com/#jp/cht/しかも";
		private static readonly string sLocBaiduEn = "https://fanyi.baidu.com/#jp/en/しかも";
		private static readonly string sLocYoudaoCht = "http://fanyi.youdao.com/";
		private static readonly string sLocGoogleChtPrefix = "https://translate.google.com/?op=translate&sl=ja&tl=zh-TW&text=";
		private static readonly string sLocGoogleEnPrefix = "https://translate.google.com/?op=translate&sl=ja&tl=en&text=";
		private static readonly string sLocBingCht = "https://www.bing.com/translator?to=zh-CHT&text=Change";
		private static readonly string sLocBingEn = "https://www.bing.com/translator?to=en&text=しかも";


		private static readonly string[] sNewLineSeparator = new string[] { "\r\n", "\r", "\n" };
		private static readonly Random rnd = new Random();
		private static readonly HtmlAgilityPack.HtmlDocument mHtmlDoc = new HtmlAgilityPack.HtmlDocument();
		private static readonly HttpClient client = new HttpClient();

		private static void _AddText( RichTextBox rtbDst, string text )
		{
			if( MarkMappedTextWithHtmlBoldTag ) {
				StringBuilder sb = new StringBuilder( text );
				foreach( var rstr in sNewLineSeparator ) {
					sb.Replace( rstr, "<br>" );
				}

				var paragraph = new Paragraph();
				rtbDst.Document.Blocks.Add( paragraph );
				var range = new TextRange( paragraph.ContentStart, paragraph.ContentEnd );
				var xamlStr = HTMLConverter.HtmlToXamlConverter.ConvertHtmlToXaml( sb.ToString(), false );
				using( var ms = new MemoryStream( Encoding.UTF8.GetBytes( xamlStr ) ) )
					range.Load( ms, DataFormats.Xaml );
			}
			else {
				var paragraph = new Paragraph();
				paragraph.Inlines.Add( new Run( text ) );
				rtbDst.Document.Blocks.Add( paragraph );
			}
		}

		private static bool _SliceSections( string text, int maxWords, out List<string> sections )
		{
			return RemoteAgents.SliceSections( text, maxWords, out sections );
		}

		private static void _Report( IProgress<ProgressInfo> progress,
						int percentOrCode, string message, object infoObject )
		{
			progress?.Report( new ProgressInfo {
				PercentOrErrorCode = percentOrCode,
				Message = message,
				InfoObject = infoObject
			} );

		}
	}
}
