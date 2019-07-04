using Minax;
using Minax.Collections;
using Minax.Domain.Translation;
using Minax.Web.Translation;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace MinaxWebTranslator
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

		public const string HtmlNewLine = "<br>";


		#region "Free API Translator"

		public static async Task<bool> XlateApiFree( RemoteType remoteType, string text, WebView wv,
									CancellationToken cancelToken, IProgress<ProgressInfo> progress )
		{
			var result = false;

			try {
				RemoteAgents.DescendedModels = DescendedModels;
				RemoteAgents.MarkMappedTextWithHtmlBoldTag = MarkMappedTextWithHtmlBoldTag;

				wv.Source = new HtmlWebViewSource { Html = "" };

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
						if( !MainThread.IsMainThread ) {
							MainThread.BeginInvokeOnMainThread( () => { _AddText( wv, text2 ); } );
						}
						else {
							_AddText( wv, text2 );
						}

					}
					return lastPercent < 100 ? false : true;
				}, cancelToken )
				.ConfigureAwait( true );

				// update CurrentUsedModels
				CurrentUsedModels.Clear();
				CurrentUsedModels.AddRange( RemoteAgents.CurrentUsedModels );
			}
			catch( Exception ex ) {
				_Report( progress, -1, string.Format( Languages.WebXlator.Str1TranslatingError, ex.Message ), ex );
				return false;
			}

			// finally, scroll to top
			if( AutoScrollToTop && text.Length > 0 ) {
				wv.Eval( "window.scrollTo(0, 0)" );
			}

			return result;
		}


		public static async Task<bool> XlateApiFree( RemoteType remoteType, string text, Entry et,
									CancellationToken cancelToken, IProgress<ProgressInfo> progress )
		{
			var result = false;

			try {
				RemoteAgents.DescendedModels = DescendedModels;
				RemoteAgents.MarkMappedTextWithHtmlBoldTag = false;

				et.Text = null;

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
						if( !MainThread.IsMainThread ) {
							MainThread.BeginInvokeOnMainThread( () => { _AddText( et, text2 ); } );
						}
						else {
							_AddText( et, text2 );
						}

					}
					return lastPercent < 100 ? false : true;
				}, cancelToken )
				.ConfigureAwait( true );

				// update CurrentUsedModels
				CurrentUsedModels.Clear();
				CurrentUsedModels.AddRange( RemoteAgents.CurrentUsedModels );
			}
			catch( Exception ex ) {
				_Report( progress, -1, string.Format( Languages.WebXlator.Str1TranslatingError, ex.Message ), ex );
				return false;
			}

			return result;
		}

		#endregion

		#region "Charged API Translator"
		public static async Task<bool> XlateApiCharged( RemoteType remoteType,
									string text, WebView wv,
									CancellationToken cancelToken, IProgress<ProgressInfo> progress,
									string subOrApiKey, string secretKey = null,
									string preferHost = null, string preferRegion = null )
		{
			var result = false;

			try {
				RemoteAgents.DescendedModels = DescendedModels;
				RemoteAgents.MarkMappedTextWithHtmlBoldTag = MarkMappedTextWithHtmlBoldTag;

				wv.Source = new HtmlWebViewSource { Html = "" };

				IEnumerable<YieldResult> xlatorResults = null;

				switch( remoteType ) {
					//case RemoteType.CrossLanguageCharged:
					//	xlatorResults = RemoteAgents.TranslateByCrossLanguageCharged( text, SourceLanguage,
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
						if( !MainThread.IsMainThread ) {
							MainThread.BeginInvokeOnMainThread( () => { _AddText( wv, text2 ); } );
						}
						else {
							_AddText( wv, text2 );
						}
					}
					return lastPercent < 100 ? false : true;
				}, cancelToken )
				.ConfigureAwait( true );

				// update CurrentUsedModels
				CurrentUsedModels.Clear();
				CurrentUsedModels.AddRange( RemoteAgents.CurrentUsedModels );
			}
			catch( Exception ex ) {
				_Report( progress, -1, string.Format( Languages.WebXlator.Str1TranslatingError, ex.Message ), ex );
				return false;
			}

			// finally, scroll to top
			if( AutoScrollToTop && text.Length > 0 ) {
				wv.Eval( "window.scrollTo(0, 0)" );
			}

			return result;
		}

		#endregion

		#region "Web Page Translators"

		public static async Task<bool> XlateExcitePage( string text, WebView wv,
								CancellationToken cancelToken, IProgress<ProgressInfo> progress )
		{

			try {
				RemoteAgents.DescendedModels = DescendedModels;
				RemoteAgents.MarkMappedTextWithHtmlBoldTag = MarkMappedTextWithHtmlBoldTag;

				wv.Source = new HtmlWebViewSource { Html = "" };

				var result = await Task.Run( () => {
					int lastPercent = 0;
					foreach( var section in RemoteAgents.TranslateByExcite( text, SourceLanguage,
											TargetLanguage, cancelToken, progress ) ) {
						lastPercent = section.PercentOrErrorCode;
						text = section.TranslatedSection;
						if( !MainThread.IsMainThread ) {
							MainThread.BeginInvokeOnMainThread( () => { _AddText( wv, text ); } );
						}
						else {
							_AddText( wv, text );
						}
					}

					return lastPercent < 100 ? false : true;
				}, cancelToken )
				.ConfigureAwait( true );

				// update CurrentUsedModels
				CurrentUsedModels.Clear();
				CurrentUsedModels.AddRange( RemoteAgents.CurrentUsedModels );
			}
			catch( Exception ex ) {
				_Report( progress, -1, string.Format( Languages.WebXlator.Str1TranslatingError, ex.Message ), ex );
				return false;
			}

			// finally, scroll to top
			if( AutoScrollToTop && text.Length > 0 ) {
				wv.Eval( "window.scrollTo(0, 0)" );
			}

			return true;
		}

		public static async Task<bool> XlateWeblioPage( string text, WebView wv,
									CancellationToken cancelToken, IProgress<ProgressInfo> progress )
		{
			try {
				RemoteAgents.DescendedModels = DescendedModels;
				RemoteAgents.MarkMappedTextWithHtmlBoldTag = MarkMappedTextWithHtmlBoldTag;

				wv.Source = new HtmlWebViewSource { Html = "" };

				var result = await Task.Run( () => {
					int lastPercent = 0;
					foreach( var section in RemoteAgents.TranslateByWeblio( text, SourceLanguage,
											TargetLanguage, cancelToken, progress ) ) {
						lastPercent = section.PercentOrErrorCode;
						text = section.TranslatedSection;
						if( !MainThread.IsMainThread ) {
							MainThread.BeginInvokeOnMainThread( () => { _AddText( wv, text ); } );
						}
						else {
							_AddText( wv, text );
						}
					}

					return lastPercent < 100 ? false : true;
				}, cancelToken )
				.ConfigureAwait( true );

				// update CurrentUsedModels
				CurrentUsedModels.Clear();
				CurrentUsedModels.AddRange( RemoteAgents.CurrentUsedModels );
			}
			catch( Exception ex ) {
				_Report( progress, -1, string.Format( Languages.WebXlator.Str1TranslatingError, ex.Message ), ex );
				return false;
			}

			// finally, scroll to top
			if( AutoScrollToTop ) {
				wv.Eval( "window.scrollTo(0, 0)" );
			}

			return true;
		}

		#endregion

		private static string[] sNewLineSep = new[] { "\r\n", "\n\r", "\n" };
		private static void _AddText( WebView wv, string text )
		{
			StringBuilder sb = new StringBuilder( text );
			foreach( var rs in sNewLineSep ) {
				sb.Replace( rs, HtmlNewLine );
			}

			var html = wv.Source as HtmlWebViewSource;
			html.Html += sb.ToString() + HtmlNewLine;
		}

		private static void _AddText( Entry et, string text )
		{
			et.Text += text + Environment.NewLine;
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
