using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Minax.Web.Formats
{
	/// <summary>
	/// Microsoft Translator API V3 Response Body in JSON format
	/// </summary>
	/// <remarks>https://docs.microsoft.com/en-us/azure/cognitive-services/translator/reference/v3-0-translate?tabs=curl#response-body</remarks>
	public class MicrosoftTranslatorFormatV3
	{
		/// <summary>
		/// An object describing the detected language, only present in the result object when language auto-detection is requested.
		/// </summary>
		[JsonProperty( PropertyName = "detectedLanguage" )]
		public LanguageData DetectedLanguage { get; set; }

		/// <summary>
		/// An array of translation results.
		/// </summary>
		[JsonProperty( PropertyName = "translations" )]
		public List<TranslationData> Translations { get; set; }

		/// <summary>
		/// Only presented when the input is expressed in a script that's not the usual script for the language.
		/// </summary>
		[JsonProperty( PropertyName = "sourceText" )]
		public TextData SourceText { get; set; }

		public class TranslationData
		{
			/// <summary>
			/// A string giving the translated text.
			/// </summary>
			[JsonProperty( PropertyName = "text" )]
			public string Text { get; set; }

			/// <summary>
			/// A string representing the language code of the target language.
			/// </summary>
			[JsonProperty( PropertyName = "to" )]
			public string To { get; set; }

			/// <summary>
			///  An object with a single string property named proj, which maps input text to translated text.
			///  The alignment information is only provided when the request parameter includeAlignment is true.
			/// </summary>
			[JsonProperty( PropertyName = "alignment" )]
			public AlignmentData Alignment { get; set; }

			/// <summary>
			/// An object returning sentence boundaries in the input and output texts.
			/// Sentence boundaries are only included when the request parameter includeSentenceLength is true.
			/// </summary>
			[JsonProperty( PropertyName = "sentLen" )]
			public SentenceLength SentenceLength { get; set; }

			/// <summary>
			/// An object giving the translated text in the script specified by the toScript parameter.
			/// The transliteration object is not included if transliteration does not take place.
			/// </summary>
			[JsonProperty( PropertyName = "transliteration" )]
			public TransliterationData Transliteration { get; set; }
		}

		public class LanguageData
		{
			/// <summary>
			/// A string representing the code of the detected language.
			/// </summary>
			[JsonProperty( PropertyName = "language" )]
			public string Language { get; set; }

			/// <summary>
			///  float value indicating the confidence in the result. The score is between zero and one and a low score indicates a low confidence.
			/// </summary>
			[JsonProperty( PropertyName = "score" )]
			public double Score { get; set; }
		}

		public class SentenceLength
		{
			/// <summary>
			/// An integer array representing the lengths of the sentences in the input text.
			/// </summary>
			[JsonProperty( PropertyName = "srcSentLen" )]
			public List<int> SourceSentenceLength { get; set; }

			/// <summary>
			///  An integer array representing the lengths of the sentences in the translated text.
			/// </summary>
			[JsonProperty( PropertyName = "transSentLen" )]
			public List<int> TranslatedSentenceLength { get; set; }
		}

		public class AlignmentData
		{
			/// <summary>
			/// maps input text to translated text. Format :
			/// [[SourceTextStartIndex]:[SourceTextEndIndex]–[TgtTextStartIndex]:[TgtTextEndIndex]]
			/// The colon separates start and end index, the dash separates the languages, and space separates the words.
			/// </summary>
			/// <remarks>https://docs.microsoft.com/en-us/azure/cognitive-services/translator/reference/v3-0-translate?tabs=curl#obtain-alignment-information</remarks>
			[JsonProperty( PropertyName = "proj" )]
			public string Projection { get; set; }
		}

		public class TransliterationData
		{
			/// <summary>
			/// A string specifying the target script.
			/// </summary>
			[JsonProperty( PropertyName = "script" )]
			public string Script { get; set; }

			/// <summary>
			/// A string giving the translated text in the target script.
			/// </summary>
			[JsonProperty( PropertyName = "text" )]
			public string Text { get; set; }
		}

		/// <summary>
		/// Translate API V3 text body for POST Request to <site>/translate and other useages. Only has a single text field.
		/// </summary>
		/// <remarks>See https://docs.microsoft.com/en-us/azure/cognitive-services/translator/reference/v3-0-translate?tabs=curl#request-body</remarks>
		public class TextData
		{
			/// <summary>
			/// gives the input text in the default script of the source language.
			/// </summary>
			[JsonProperty( PropertyName = "text" )]
			public string Text { get; set; }
		}

		//public class TextBody
		//{
		//	/// <summary>
		//	/// gives the input text in the default script of the source language.
		//	/// </summary>
		//	[JsonProperty( PropertyName = "Text" )]
		//	public string Text { get; set; }
		//}



		public class TranslatorError
		{
			[JsonProperty( PropertyName = "error" )]
			public ErrorData Error { get; set; }

			public class ErrorData
			{
				[JsonProperty( PropertyName = "code" )]
				public int Code { get; set; }

				[JsonProperty( PropertyName = "message" )]
				public string Message { get; set; }
			}
		}

		public enum ErrorCode : int
		{

			/*
			https://docs.microsoft.com/zh-tw/azure/cognitive-services/translator/reference/v3-0-reference#errors

			*/
		}
	}


}
